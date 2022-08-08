using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Async_TCP_Server_Console_Tutorial
{
    class Program
    {
        static void Main(string[] args)
        {
            AsyncEchoServer().Wait();
        }

        async static Task AsyncEchoServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 7000);
            listener.Start();
            Console.WriteLine("서버 오픈\n클라이언트 연결 대기중....");

            while (true)
            {

                TcpClient tc = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                Console.WriteLine("클라이언트 연결 성공");

                // 새 쓰레드에서 처리
                Task.Factory.StartNew(AsyncTcpProcess, tc);    
            }

        
        }


        async static void AsyncTcpProcess(object o)
        {
            TcpClient tc = (TcpClient)o;

            int MAX_Size = 1024;
            NetworkStream stream = tc.GetStream();
            while (true)
            {
                
                var buff = new byte[MAX_Size];

                var readTask = stream.ReadAsync(buff, 0, buff.Length);
                var timeoutTask = Task.Delay(10 * 1000);  // 10 secs
                var doneTask = await Task.WhenAny(timeoutTask, readTask).ConfigureAwait(false);

                if (doneTask == timeoutTask) // 타임아웃이면
                {
                    var bytes = Encoding.ASCII.GetBytes("Read Timeout Error");
                    await stream.WriteAsync(bytes, 0, bytes.Length);
                    break;
                }
                else
                {
                    var nbytes = await stream.ReadAsync(buff, 0, buff.Length).ConfigureAwait(false);
                    if (nbytes > 0)
                    {
                        string msg = Encoding.ASCII.GetString(buff, 0, nbytes);
                        Console.WriteLine(string.Format(msg + " at " + DateTime.Now));

                        await stream.WriteAsync(buff, 0, nbytes).ConfigureAwait(false);
                    }
                }
            }

            stream.Close();
            tc.Close();
        }
    }
}                                
