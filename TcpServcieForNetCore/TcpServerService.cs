using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class TcpServerService : IHostedService, IDisposable
{
    private readonly ILogger<TcpServerService> _logger;
    private TcpListener _listener;
    private CancellationTokenSource _cts;

    public TcpServerService(ILogger<TcpServerService> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("TCP 服务器启动中...");
        _listener = new TcpListener(IPAddress.Any, 8080);
        _listener.Start();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _ = AcceptClientsAsync(_cts.Token);  // 开始异步接受客户端连接
        return Task.CompletedTask;
    }

    private async Task AcceptClientsAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var client = await _listener.AcceptTcpClientAsync();
                _logger.LogInformation("客户端已连接");

                _ = Task.Run(() => HandleClientAsync(client, token));
            }
            catch (Exception ex) when (ex is ObjectDisposedException || ex is SocketException)
            {
                _logger.LogInformation("服务器已停止接受连接。");
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
                _logger.LogInformation("收到客户端消息: {Message}", message);

                var responseMessage = Encoding.UTF8.GetBytes("服务器已收到: " + message);
                await stream.WriteAsync(responseMessage, 0, responseMessage.Length, token);
            }

            _logger.LogInformation("客户端已断开连接");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("TCP 服务器停止中...");
        _cts?.Cancel();
        _listener?.Stop();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _listener?.Stop();
        _cts?.Dispose();
    }
}
