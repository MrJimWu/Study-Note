using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
public class SerialPortService : IHostedService, IDisposable
{
    private readonly ILogger<SerialPortService> _logger;
    private SerialPort _serialPort;
    private CancellationTokenSource _cts;

    public SerialPortService(ILogger<SerialPortService> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("串口服务启动中...");

        // 配置串口
        _serialPort = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
        _serialPort.DataReceived += OnDataReceived;  // 设置数据接收事件
        _serialPort.Open(); // 打开串口连接
        _logger.LogInformation("串口已打开，等待数据...");

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        return Task.CompletedTask;
    }

    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            // 读取数据
            string data = _serialPort.ReadExisting();
            _logger.LogInformation("收到串口数据: {Data}", data);

            // 发送响应（可选）
            string responseMessage = "服务器已收到: " + data;
            _serialPort.WriteLine(responseMessage);
            _logger.LogInformation("已响应客户端: {Response}", responseMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError("读取串口数据时发生错误: {Message}", ex.Message);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("串口服务停止中...");
        _cts?.Cancel();
        _serialPort?.Close();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _serialPort?.Dispose();
        _cts?.Dispose();
    }
}
