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

        private List<TcpClient> clients;
        private List<StreamWriter> streamwriters;
        private List<StreamReader> streamreaders;

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
            foreach(StreamWriter strWriter in streamwriters)
                strWriter.WriteLine(sendData);
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

            Thread get_client = new Thread(Get_Client);
            get_client.IsBackground = true;
            get_client.Start();

            Thread Chat = new Thread();
            


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
                        streamwriters.Add(new StreamWriter(client.GetStream()));
                        streamreaders.Add(new StreamReader(client.GetStream()));
                        user_count++;
                    }
                    Thread.Sleep(50);
                }

            }
            catch {
                return;
            }
        }

        private async Task<bool> check_client_list()
        {
            while (true)
            {
                if (clients.Count > 0)
                    return true;
                Thread.Sleep(50);
            }
        }


    }
}
