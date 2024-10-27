using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 创建一个新的TCP监听器，监听本地IP和端口8080
            TcpListener listener = new TcpListener(IPAddress.Any, 8080);
            listener.Start();
            Console.WriteLine("服务器已启动，等待客户端连接...");

            while (true)
            {
                // 等待客户端连接
                TcpClient client = await listener.AcceptTcpClientAsync();
                Console.WriteLine("客户端已连接");

                // 开启新任务处理客户端
                _ = Task.Run(async () =>
                {
                    using NetworkStream stream = client.GetStream();
                    byte[] buffer = new byte[1024];
                    int bytesRead;

                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine("收到消息: " + message);

                        // 发送响应给客户端
                        byte[] response = Encoding.UTF8.GetBytes("服务器已收到: " + message);
                        await stream.WriteAsync(response, 0, response.Length);
                    }

                    client.Close();
                    Console.WriteLine("客户端已断开连接");
                });
            }
        }
    }
}
