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

namespace TCP_Async_Server_Tutorial
{
    public partial class Form1 : Form
    {
        //비동기 채팅서버 만들어보기
        public Form1()
        {
            InitializeComponent();
        }
        //TcpClient 와 그에 해당하는 streamwriter, streamreader를 한번에 묶어서 저장해야할듯. 즉, 구조체? c#에선 뭐라고 부르지 어쨋든 그거 써야할듯.
        private List<TcpClient> clients;

        private TcpListener listener;
        private int user_count = 0;

        private void button1_Click(object sender, EventArgs e)
        {
            Thread thr1 = new Thread(Server_Open);
            thr1.IsBackground = true;//Form이 종료되면 thr1도 종료된다.
            thr1.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string sendData = textBox3.Text;
            foreach (TcpClient client in clients)
            {
                StreamWriter strWriter = new StreamWriter(client.GetStream());
                strWriter.WriteLine(sendData);
            }

            writeRichTextbox(sendData);
        }

        private void writeRichTextbox(string str)
        {//인보크 써서 스레드간 충돌 방지
            richTextBox1.Invoke((MethodInvoker)delegate { richTextBox1.AppendText(str + "\r\n"); });//데이터 쓰기
            richTextBox1.Invoke((MethodInvoker)delegate { richTextBox1.ScrollToCaret(); });//스크롤 내리기
        }


        private async void Server_Open() {
            listener = new TcpListener(IPAddress.Parse(textBox1.Text), int.Parse(textBox2.Text));

            listener.Start();
            writeRichTextbox("클라이언트 대기중....");

            //언제 클라이언트가 새로 접속할지 모르므로 따로 스레드를 만들어서 클라연결을 받는다.
            Thread get_client = new Thread(Get_Client);
            get_client.IsBackground = true;
            get_client.Start();

            //클라가 몇명이 접속했든 채팅기능은 작동해야하므로 이놈도 스레드를 하나 더 만든다.
            Thread Chat = new Thread(Chatting);
            Chat.IsBackground = true;
            Chat.Start();


        }

        private void Get_Client() {
            try
            {
                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    if (client != null)
                    {
                        clients.Add(client);
                        user_count++;
                    }
                    Thread.Sleep(50);
                }

            }
            catch {
                return;
            }
        }

        private void Chatting()
        {
            try
            {
                foreach (TcpClient client in clients)
                {
                    StreamReader strReader = new StreamReader(client.GetStream());
                    string receivedData = strReader.ReadLine();
                    writeRichTextbox(receivedData);
                }
            }
            catch 
            {
                return;
            }
        }


    }
}
