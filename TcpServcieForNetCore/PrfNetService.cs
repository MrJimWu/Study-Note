using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class PrfNetService : IHostedService, IDisposable
{
    private readonly ILogger<PrfNetService> _logger;
    private UdpClient _udpClient;
    private CancellationTokenSource _cts;

    public PrfNetService(ILogger<PrfNetService> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("PRFNet 服务启动中...");

        // 初始化 UdpClient，并绑定到端口
        _udpClient = new UdpClient(9000); // 假设使用端口 9000
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // 启动接收数据的任务
        _ = ReceiveDataAsync(_cts.Token);
        return Task.CompletedTask;
    }

    private async Task ReceiveDataAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                // 异步接收 PRFNet 数据包
                UdpReceiveResult result = await _udpClient.ReceiveAsync();
                byte[] data = result.Buffer;
                IPEndPoint remoteEndpoint = result.RemoteEndPoint;

                // 处理收到的数据
                ProcessReceivedData(data, remoteEndpoint);
            }
            catch (Exception ex) when (ex is ObjectDisposedException || ex is SocketException)
            {
                _logger.LogWarning("PRFNet 服务已停止接收消息: {Message}", ex.Message);
            }
        }
    }

    private void ProcessReceivedData(byte[] data, IPEndPoint remoteEndpoint)
    {
        string receivedMessage = Encoding.UTF8.GetString(data);
        _logger.LogInformation("收到 PRFNet 数据: {Message} 来自 {RemoteEndpoint}", receivedMessage, remoteEndpoint);

        // 发送响应
        string responseMessage = "PRFNet 服务已收到: " + receivedMessage;
        byte[] responseBytes = Encoding.UTF8.GetBytes(responseMessage);
        _udpClient.Send(responseBytes, responseBytes.Length, remoteEndpoint);
        _logger.LogInformation("已响应客户端: {Response}", responseMessage);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("PRFNet 服务停止中...");
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
