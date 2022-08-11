using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace TCP_Async_Chatting_Server
{
    public partial class Form1 : Form
    {
        TcpListener server = null; // 서버
        TcpClient clientSocket = null; // 소켓
        static int counter = 0; // 사용자 수
        string date; // 날짜 
        string start_time = DateTime.Now.ToString("[yyyy.MM.dd HH:mm:ss]");//로그파일에 기록할 서버시작시간.
        int chat_capacity;
        // 각 클라이언트 마다 리스트에 추가
        public Dictionary<TcpClient, string> clientList = new Dictionary<TcpClient, string>();


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //로드 될 때 스레드도 같이 생성 및 시작.
            Thread socketThread = new Thread(initSocket);
            socketThread.IsBackground = true;//폼 종료될 때 같이 종료된다.
            socketThread.Start();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            chat_capacity = richTextBox1.Text.Length;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            string LogFolderPath = string.Format(".\\ChatLogs");//채팅로그파일경로설정
            string end_time = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");//종료시점 날짜를 받아온다.
            if (!Directory.Exists(LogFolderPath))//로그파일폴더가 존재하지 않으면 폴더를 만든다.
            {
                Directory.CreateDirectory(LogFolderPath);
            }

            try
            {
                //로그파일 생성 및 작성
                string filePath = string.Format(".\\ChatLogs\\" + start_time + "__" + end_time + ".txt");
                FileStream LogFileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                StreamWriter LogWriter = new StreamWriter(LogFileStream, Encoding.UTF8);


                LogWriter.WriteLine(richTextBox1.Text);
                LogWriter.Flush();
                LogWriter.Close();
                LogFileStream.Close();
            }
            catch (Exception exa){
                return;
            }
        }

        private void initSocket()
        {
            server = new TcpListener(IPAddress.Any, 50000); //서버 접속을 허용할 ip주소와 접속을 받을 포트번호
            clientSocket = default(TcpClient);//Tcpclient 클래스의 디폴트 설정으로 소켓 클라이언트 생성.
            server.Start();//Listener 시작해 연결대기
            AddText("==서버 시작==");

            while (true)
            {
                try
                {
                    clientSocket = server.AcceptTcpClient(); //client 소켓 접속 허용.
                    counter++;
                    AddText("==사용자 연결==");

                    NetworkStream stream = clientSocket.GetStream();//받아온 client소켓에서 스트림을 받아온다.
                    byte[] buffer = new byte[1024];
                    int bytes = stream.Read(buffer, 0, buffer.Length);
                    string NickName = Encoding.Unicode.GetString(buffer, 0, bytes);
                    NickName = NickName.Substring(0, NickName.IndexOf("$"));//client 사용자 이름을 받아온다. 이 형식대로 사용자가 통신을 할 것이다.

                    clientList.Add(clientSocket, NickName);//클라 리스트에 닉네임과 클라소켓 맵핑시켜 추가한다.

                    SendMessageToAll(NickName + " 님이 입장했습니다.", "", false);

                    handleClient h_client = new handleClient();//클라이언트 추가

                    h_client.OnReceived += new handleClient.MessageDisplayHandler(OnReceived);//MessageDisplayHandler에 OnReceived 메서드를 패러미터로 넘긴다.
                    h_client.OnDisconnected += new handleClient.DisconnectedHandler(h_client_OnDisconnected);//DisconnectedHandler에 h_client_OnDisconnected메서드를 패러미터로 넘긴다.
                    h_client.startClient(clientSocket, clientList);
                }
                catch { break; }//오류든 뭐든 중간에 뭐가 이상하면 반복문 박-살
            }
            clientSocket.Close();//클라소켓 닫고
            server.Stop();//서버중지
        }

        void h_client_OnDisconnected(TcpClient clientSocket)
        {
            if (clientList.ContainsKey(clientSocket))//클라리스트에 해당 클라가 존재하면 삭제한다.
                clientList.Remove(clientSocket);
        }

        private void OnReceived(string message, string NickName)
        {
            if (message.Equals("LeaveChat"))//클라로부터 받아온 메시지가 떠난다는거면
            {
                string displayMessage = "user " + NickName + " leave chat";//떠난다고 채팅방에 표기하기
                AddText(displayMessage);
                SendMessageToAll("LeaveChat", NickName, true);
            }
            else//아니면
            {
                string displayMessage = "From client " + NickName + " : " + message;//보낸 메시지 출력
                AddText(displayMessage);
                SendMessageToAll(message, NickName, true);
            }
        }


        private void AddText(string contents)
        {
            if (chat_capacity < 1000000)//총 텍스트 갯수가 100만개 이하여야함.
            {
                if (richTextBox1.InvokeRequired)//폼 컨트롤은 다른 쓰레드에서 관리할 수도 있으므로 invokeRequired로 체크해 
                {//필요하면 델리게이트를 사용해서 텍스트박스내용을 수정하고
                    richTextBox1.BeginInvoke(new MethodInvoker(delegate { richTextBox1.AppendText(contents + "\r\n"); })); 
                }
                else//안필요하면 그냥 수정한다.
                    richTextBox1.AppendText(contents + "\r\n");
            }
            else
            {
                SendMessageToAll("서버가 터졌습니다. 접속을 종료해주세요.", "Server Manager", true);
                MessageBox.Show("서버용량 초과. 재시작 필요.", "경고");
            }
        }

        public void SendMessageToAll(string message, string NickName, bool flag)
        {
            foreach (var pair in clientList)//클라리스트에 있는 모든 클라에게 송신한다.
            {
                date = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");

                TcpClient client = pair.Key as TcpClient;//해당 딕셔너리 원소의 키값을 tcp클라이언트로 받는다.

                NetworkStream stream = client.GetStream();//클라에서 네트워크 스트림 받아온다음.
                byte[] buffer = null;

                if (flag)//접속시도에서 보낸 스트림인지 아닌지 여부
                {
                    if (message.Equals("LeaveChat"))//클라가 채팅방 떠날 때
                        buffer = Encoding.Unicode.GetBytes(NickName + "님이 나갔습니다.");
                    else//그냥 채팅일 때
                        buffer = Encoding.Unicode.GetBytes("[" + date + "]" + NickName + " : " + message);
                }
                else
                    buffer = Encoding.Unicode.GetBytes(message);
                stream.Write(buffer, 0, buffer.Length);//버퍼에 쓰기
                stream.Flush();
            }
        }




    }
}
