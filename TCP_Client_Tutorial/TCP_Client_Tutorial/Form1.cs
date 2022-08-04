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



namespace TCP_Client_Tutorial
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

        private void writeRichTextbox(string str) {//인보크 써서 스레드간 충돌 방지
            richTextBox1.Invoke((MethodInvoker)delegate { richTextBox1.AppendText(str + "\r\n"); });//데이터 쓰기
            richTextBox1.Invoke((MethodInvoker)delegate { richTextBox1.ScrollToCaret(); });//스크롤 내리기
        }

        private void clearRichTextBox() {//리치 텍스트박스 클리어해주는 메소드
            richTextBox1.Invoke((MethodInvoker)delegate { richTextBox1.Clear(); });
        }

        private void connect()
        {
            TcpClient client = new TcpClient();
            IPEndPoint ipEndPt = new IPEndPoint(IPAddress.Parse(textBox1.Text), int.Parse(textBox2.Text));
            try
            {
                client.Connect(ipEndPt);
                writeRichTextbox("서버 연결완료");

                strReader = new StreamReader(client.GetStream());
                strWriter = new StreamWriter(client.GetStream());
                strWriter.AutoFlush = true;

                while (client.Connected)
                {
                    string receivedData = strReader.ReadLine();
                    writeRichTextbox(receivedData);
                }
            }
            catch {
                MessageBox.Show("연결에 문제가 생겼습니다. 연결을 종료합니다.", "경고");
                client.Close();
                clearRichTextBox();
            }
        }

    }
}
