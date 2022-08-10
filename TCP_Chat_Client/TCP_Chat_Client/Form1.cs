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
        TcpClient clientSocket = new TcpClient(); //소켓 클라 생성
        NetworkStream stream = default(NetworkStream);
        string message = string.Empty;

        public Form1()
        {
            InitializeComponent();
        }

        private void Connection_button_Click(object sender, EventArgs e)//연결버튼 누르면
        {
            try
            {
                clientSocket.Connect("111.111.0.31", 50000);//연결 시도한다.
                stream = clientSocket.GetStream();
            }
            catch//뭔 문제 생기면 실행.
            {
                MessageBox.Show("서버 접속에 실패했습니다.");
                Application.Exit();
            }

            //catch안뜨고 잘 왔으면 실행
            message = "채팅 서버에 연결 되었습니다.";
            AddText(message);

            byte[] buffer = Encoding.Unicode.GetBytes(textBox2.Text + "$");//닉네임 설정칸 내용 송신하고 해당 텍스트박스 비활성화
            stream.Write(buffer, 0, buffer.Length);//버퍼에 담은 내용 스트림에 쓴다.
            stream.Flush();//스트림 초기화
            textBox2.ReadOnly = true;//textBox수정 못하게한다.

            Thread tcp_handler = new Thread(GetMessage);//메시지 수신할 스레드 선언 및 시작
            tcp_handler.IsBackground = true;//폼 종료되면 같이 종료되도록한다.
            tcp_handler.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0)//아무것도 입력 안하고 전송하면 아무것도 전송 안하도록함.
            {
                textBox1.Focus();
                byte[] buffer = Encoding.Unicode.GetBytes(textBox1.Text + "$");//내용 버퍼에 집어넣고
                stream.Write(buffer, 0, buffer.Length);//스트림에 쓰고
                stream.Flush();//스트림 초기화하고
                textBox1.Text = "";//보낼채팅 내용 쓰는 텍스트박스 clear시킨다.
            }
        }

        private void textBox1_KeyUP(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)//엔터키 입력받으면 텍스트박스 내용 송신한다.
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
            Application.ExitThread();//메인 스레드 종료하고
            Environment.Exit(0);//프로세스 종료시킨다.
        }

        private void GetMessage()//메시지 수신받는 메서드
        {
            while (true)//계속반복한다.
            {
                stream = clientSocket.GetStream();//스트림받아와서
                int buffersize = clientSocket.ReceiveBufferSize;//받은 버퍼사이즈를 int로 구하고
                byte[] buffer = new byte[buffersize];//구한 사이즈만큼 버퍼 바이트배열 크기를 설정한다.

                int bytes = stream.Read(buffer, 0, buffer.Length);//몇바이튼지 읽어오고
                string message = Encoding.Unicode.GetString(buffer, 0, bytes);//string으로 바꾼다.
                AddText(message);//그렇게 만든 메시지 출력.
            }
        }

        private void AddText(string text)//채팅내용 텍스트박스 수정하는 메서드
        {
            if (richTextBox1.InvokeRequired)//텍스트박스에 접근하는데 인보크가 필요하면 즉석 델리게이트 선언으로 실행한다.
                richTextBox1.BeginInvoke(new MethodInvoker(delegate { richTextBox1.AppendText(text + "\r\n"); }));
            else//인보크 필요없으면 걍 텍스트수정 갖다박는다.
                richTextBox1.AppendText(text + "\r\n");
        }





        



        
    }
}
