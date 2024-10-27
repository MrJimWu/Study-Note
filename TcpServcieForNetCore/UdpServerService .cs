using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class UdpServerService : IHostedService, IDisposable
{
    private readonly ILogger<UdpServerService> _logger;
    private UdpClient _udpClient;
    private CancellationTokenSource _cts;

    public UdpServerService(ILogger<UdpServerService> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("UDP 服务器启动中...");

        // 初始化 UDP 客户端，并绑定到端口 8080
        _udpClient = new UdpClient(8080);
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // 开始接收消息
        _ = ReceiveMessagesAsync(_cts.Token);
        return Task.CompletedTask;
    }

    private async Task ReceiveMessagesAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var result = await _udpClient.ReceiveAsync();
                string receivedMessage = Encoding.UTF8.GetString(result.Buffer);
                _logger.LogInformation("收到消息: {Message} 来自 {Sender}", receivedMessage, result.RemoteEndPoint);

                // 发送响应消息
                string responseMessage = "服务器已收到: " + receivedMessage;
                byte[] responseBytes = Encoding.UTF8.GetBytes(responseMessage);
                await _udpClient.SendAsync(responseBytes, responseBytes.Length, result.RemoteEndPoint);
                _logger.LogInformation("已响应客户端");
            }
            catch (Exception ex) when (ex is ObjectDisposedException || ex is SocketException)
            {
                _logger.LogWarning("UDP 服务器已停止接收消息。");
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("UDP 服务器停止中...");
        _cts?.Cancel();
        _udpClient?.Close();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _udpClient?.Dispose();
        _cts?.Dispose();
    }
}
