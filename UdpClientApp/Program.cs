using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UdpClientApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using UdpClient udpClient = new UdpClient();
            udpClient.Connect("127.0.0.1", 8080);

            string message = "Hello, UDP Server!";
            byte[] data = Encoding.UTF8.GetBytes(message);

            // 发送消息给服务器
            await udpClient.SendAsync(data, data.Length);
            Console.WriteLine("已发送消息: " + message);

            // 接收服务器响应
            var responseResult = await udpClient.ReceiveAsync();
            string responseMessage = Encoding.UTF8.GetString(responseResult.Buffer);
            Console.WriteLine("收到服务器响应: " + responseMessage);
        }
    }
}
