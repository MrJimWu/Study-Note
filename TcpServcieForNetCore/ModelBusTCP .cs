using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class ModelBusTcpService : IHostedService, IDisposable
{
    private readonly ILogger<ModelBusTcpService> _logger;
    private TcpListener _tcpListener;
    private CancellationTokenSource _cts;

    public ModelBusTcpService(ILogger<ModelBusTcpService> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ModelBus TCP 服务启动中...");

        // 初始化 TCP 监听器并绑定到端口 5000
        _tcpListener = new TcpListener(IPAddress.Any, 5000);
        _tcpListener.Start();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // 启动客户端连接的处理任务
        _ = AcceptClientsAsync(_cts.Token);
        return Task.CompletedTask;
    }

    private async Task AcceptClientsAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var client = await _tcpListener.AcceptTcpClientAsync();
                _logger.LogInformation("客户端已连接");

                // 每个客户端连接开启一个独立的任务来处理
                _ = Task.Run(() => HandleClientAsync(client, token));
            }
            catch (Exception ex) when (ex is ObjectDisposedException || ex is SocketException)
            {
                _logger.LogInformation("TCP 监听已停止.");
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken token)
    {
        using (client)
        {
            var buffer = new byte[1024];
            var stream = client.GetStream();

            while (!token.IsCancellationRequested)
            {
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                if (bytesRead == 0) break;  // 客户端断开连接

                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                _logger.LogInformation("收到消息: {Message}", message);

                // 回应客户端
                var responseMessage = Encoding.UTF8.GetBytes("服务器已收到: " + message);
                await stream.WriteAsync(responseMessage, 0, responseMessage.Length, token);
            }

            _logger.LogInformation("客户端已断开连接");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ModelBus TCP 服务停止中...");
        _cts?.Cancel();
        _tcpListener?.Stop();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _tcpListener?.Stop();
        _cts?.Dispose();
    }
}
