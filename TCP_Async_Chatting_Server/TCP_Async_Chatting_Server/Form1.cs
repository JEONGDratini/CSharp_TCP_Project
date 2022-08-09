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

                }
                catch { break; }
            }


        }

        private void AddText(string contents)
        {
            if (richTextBox1.InvokeRequired)
            {
                richTextBox1.BeginInvoke(new MethodInvoker(delegate { richTextBox1.AppendText(contents + "\r\n"); }));
            }
            else
                richTextBox1.AppendText(contents + "\r\n");
        }
    }
}
