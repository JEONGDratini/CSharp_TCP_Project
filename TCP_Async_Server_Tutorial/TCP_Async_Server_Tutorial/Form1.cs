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


        //tcp 비동기 서버 만들어보기
        public Form1()
        {
            InitializeComponent();
        }
        
        
       

        private void button1_Click(object sender, EventArgs e)
        {
            Aysnc_Server().Wait();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string sendData = textBox3.Text;


            writeRichTextbox(sendData);
        }

        private void writeRichTextbox(string str)
        {//인보크 써서 스레드간 충돌 방지
            richTextBox1.Invoke((MethodInvoker)delegate { richTextBox1.AppendText(str + "\r\n"); });//데이터 쓰기
        }


        private async Task Aysnc_Server() {
            TcpListener listener = new TcpListener(IPAddress.Any, int.Parse("50000"));
            listener.Start();
            while (true) 
            {
                TcpClient tc = await listener.AcceptTcpClientAsync().ConfigureAwait(false);//비동기로 클라접속 대기
                
                Task.Factory.StartNew(AsyncTcpProcess, tc);
                

            }
        }


        private async void AsyncTcpProcess(object o)
        {
            TcpClient tc = (TcpClient)o;

            int MAX_Size = 1024;
            NetworkStream stream = tc.GetStream();

            //비동기 수신
            var buff = new byte[MAX_Size];
            var nbytes = await stream.ReadAsync(buff, 0, buff.Length).ConfigureAwait(false);
            if (nbytes > 0) 
            {
                string msg = Encoding.ASCII.GetString(buff, 0, nbytes);
                richTextBox1.Invoke((MethodInvoker)delegate { richTextBox1.AppendText(DateTime.Now +"  "+ msg + "\r\n"); });//데이터 쓰기
                richTextBox1.Invoke((MethodInvoker)delegate { richTextBox1.ScrollToCaret(); });//스크롤 내리기

                await stream.WriteAsync(buff, 0, nbytes).ConfigureAwait(false);
            }

            stream.Close();
            tc.Close();

        }


       
        //업데이트 되면 다른 클라이언트들에게도 메시지를 뿌린다.
        private void Chat_Content_Updated(object sender, EventArgs e)
        {

        }





    }
}
