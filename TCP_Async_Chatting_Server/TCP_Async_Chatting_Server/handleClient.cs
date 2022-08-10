using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace TCP_Async_Chatting_Server
{
    class handleClient
    {

        //클라이언트 객체와 그것들을 닉네임과 맵핑시킨 딕셔너리 선언
        TcpClient clientSocket;
        public Dictionary<TcpClient, string> clientList = null;

        public void startClient(TcpClient ClientSocket, Dictionary<TcpClient, string> clientList)
        {
            this.clientSocket = ClientSocket;//매개변수들을 각 속성에 집어넣는다.
            this.clientList = clientList;

            Thread thread_handler = new Thread(Chat_execute);//새 스레드 언선
            thread_handler.IsBackground = true;//폼 종료되면 같이 종료되도록 설정.
            thread_handler.Start();//스레드 실행
        }

        //메시지 출력 핸들러 델리게이트 설정 및 객체 선언
        public delegate void MessageDisplayHandler(string message, string NickName);
        public event MessageDisplayHandler OnReceived;

        //클라연결 종료 핸들러 델리게이트 설정 및 객체 선언
        public delegate void DisconnectedHandler(TcpClient clientSocket);
        public event DisconnectedHandler OnDisconnected;

        //채팅 실행.
        private void Chat_execute()
        {
            NetworkStream stream = null;
            try
            {
                byte[] buffer = new byte[1024];//버퍼설정 (크기 : 1024바이트)
                string msg = string.Empty;
                int bytes = 0;
                int MessageCount = 0;

                while (true)//계속반복한다.
                {
                    MessageCount++;
                    stream = clientSocket.GetStream();//클라소켓에서 스트림을 받아온다.
                    bytes = stream.Read(buffer, 0, buffer.Length);//스트림에서 버퍼를 버퍼크기만큼 바이트 변수로 읽어온다.
                    msg = Encoding.Unicode.GetString(buffer, 0, bytes);
                    msg = msg.Substring(0, msg.IndexOf("$"));//처음부터 $까지만 msg에 저장한다.

                    if (OnReceived != null)//처음에 만들었던 OnReceived 이벤트객체가 null이라면
                        OnReceived(msg, clientList[clientSocket].ToString());//OnReceived메서드를 호출해준다.
                }
            }
            catch 
            {
                if (clientSocket != null)//클라소켓이 null이 아닌데 뭔 문제가 생길 때 실행
                {
                    if (OnDisconnected != null)//처음에 만들었던 OnDisconnected 이벤트객체가 null이면
                        OnDisconnected(clientSocket);//OnDisconnected메서드를 호출한다.

                    clientSocket.Close();//소켓닫고
                    stream.Close();//네트워크 스트림도 닫는다.
                }

            }
        }
    }
}
