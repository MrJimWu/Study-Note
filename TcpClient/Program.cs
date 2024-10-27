using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpClientApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // 连接到服务器IP地址和端口
                using TcpClient client = new TcpClient("127.0.0.1", 8080);
                Console.WriteLine("已连接到服务器");

                using NetworkStream stream = client.GetStream();

                // 发送消息给服务器
                string message = "Hello, Server!";
                byte[] data = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(data, 0, data.Length);
                Console.WriteLine("发送消息: " + message);

                // 接收服务器响应
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("收到服务器响应: " + response);

                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("异常: " + e.Message);
            }
        }
    }
}
