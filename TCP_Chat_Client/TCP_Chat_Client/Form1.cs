using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;

namespace TCP_Chat_Client
{
    public partial class Form1 : Form
    {
        TcpClient clientSocket = new TcpClient(); //소켓
        NetworkStream stream = default(NetworkStream);
        string message = string.Empty;

        public Form1()
        {
            InitializeComponent();
        }

        private void Connection_button_Click(object sender, EventArgs e)
        {
            try
            {
                clientSocket.Connect("111.111.0.31", 50000);
                stream = clientSocket.GetStream();
            }
            catch
            {
                MessageBox.Show("서버 접속에 실패했습니다.");
                Application.Exit();
            }

            message = "채팅 서버에 연결 되었습니다.";
            AddText(message);

            byte[] buffer = Encoding.Unicode.GetBytes(textBox2.Text + "$");
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
            textBox2.ReadOnly = true;

            Thread tcp_handler = new Thread(GetMessage);
            tcp_handler.IsBackground = true;
            tcp_handler.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0)
            {
                textBox1.Focus();
                byte[] buffer = Encoding.Unicode.GetBytes(textBox1.Text + "$");
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
                textBox1.Text = "";
            }
        }

        private void textBox1_KeyUP(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                button1_Click(this, e);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (clientSocket.Connected)//연결된 상태에서 종료하게되면 서버에 떠난다는 메시지를 남긴다.
            {
                byte[] buffer = Encoding.Unicode.GetBytes("LeaveChat" + "$");
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
            }
            Application.ExitThread();
            Environment.Exit(0);
        }

        private void GetMessage()
        {
            while (true)
            {
                stream = clientSocket.GetStream();
                int buffersize = clientSocket.ReceiveBufferSize;
                byte[] buffer = new byte[buffersize];

                int bytes = stream.Read(buffer, 0, buffer.Length);
                string message = Encoding.Unicode.GetString(buffer, 0, bytes);
                AddText(message);
            }
        }

        private void AddText(string text)
        {
            if (richTextBox1.InvokeRequired)
                richTextBox1.BeginInvoke(new MethodInvoker(delegate { richTextBox1.AppendText(text + "\r\n"); }));
            else
                richTextBox1.AppendText(text + "\r\n");
        }





        



        
    }
}
