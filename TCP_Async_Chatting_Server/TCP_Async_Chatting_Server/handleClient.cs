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

        TcpClient clientSocket;
        public Dictionary<TcpClient, string> clientList = null;

        public void startClient(TcpClient ClientSocket, Dictionary<TcpClient, string> clientList)
        {
            this.clientSocket = ClientSocket;
            this.clientList = clientList;

            Thread thread_handler = new Thread(Chat_execute);//새 스레드 생성해 실행.
            thread_handler.IsBackground = true;//폼 종료되면 같이 종료되도록 설정.
            thread_handler.Start();
        }

        public delegate void MessageDisplayHandler(string message, string NickName);//메시지 출력 핸들러 델리게이트 설정 및 선언
        public event MessageDisplayHandler OnReceived;

        public delegate void DisconnectedHandler(TcpClient clientSocket);//
        public event DisconnectedHandler OnDisconnected;

        private void Chat_execute()
        {
            NetworkStream stream = null;
            try
            {
                byte[] buffer = new byte[1024];
                string msg = string.Empty;
                int bytes = 0;
                int MessageCount = 0;

                while (true)
                {
                    MessageCount++;
                    stream = clientSocket.GetStream();
                    bytes = stream.Read(buffer, 0, buffer.Length);
                    msg = Encoding.Unicode.GetString(buffer, 0, bytes);
                    msg = msg.Substring(0, msg.IndexOf("$"));

                    if (OnReceived != null)
                        OnReceived(msg, clientList[clientSocket].ToString());
                }
            }
            catch 
            {
                if (clientSocket != null)
                {
                    if (OnDisconnected != null)
                        OnDisconnected(clientSocket);

                    clientSocket.Close();
                    stream.Close();
                }

            }
        }
    }
}
