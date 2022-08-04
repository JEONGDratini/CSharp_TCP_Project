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

namespace TCP_Server_Tutorial
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)//연결
        {
            Thread thr1 = new Thread(connect);
            thr1.IsBackground = true;//Form이 종료되면 thr1도 종료된다.
            thr1.Start();
        }

        StreamReader strReader;
        StreamWriter strWriter;

        private void button2_Click(object sender, EventArgs e)//전송
        {
            string sendData = textBox3.Text;
            strWriter.WriteLine(sendData);
            writeRichTextbox(sendData);
        }

        private void writeRichTextbox(string str)
        {//인보크 써서 스레드간 충돌 방지
            richTextBox1.Invoke((MethodInvoker)delegate { richTextBox1.AppendText(str + "\r\n"); });//데이터 쓰기
            richTextBox1.Invoke((MethodInvoker)delegate { richTextBox1.ScrollToCaret(); });//스크롤 내리기
        }

        private void connect()
        {
            TcpListener Listener = new TcpListener(IPAddress.Parse(textBox1.Text), int.Parse(textBox2.Text));

            Listener.Start(); //tcp리스닝 시작
            writeRichTextbox("클라이언트 대기중....");

            TcpClient client1 = Listener.AcceptTcpClient();

            writeRichTextbox("클라이언트 연결확인");

            strReader = new StreamReader(client1.GetStream());
            strWriter = new StreamWriter(client1.GetStream());
            strWriter.AutoFlush = true;//쓰기버퍼 쓰고나면 자동으로 플러시실행

            while (client1.Connected)
            {
                string receivedData = strReader.ReadLine();
                writeRichTextbox(receivedData);
            }

        }
    }
}
