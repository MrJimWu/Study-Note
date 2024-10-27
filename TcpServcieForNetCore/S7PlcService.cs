using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using S7.Net;

public class S7PlcService : IHostedService, IDisposable
{
    private readonly ILogger<S7PlcService> _logger;
    private readonly IConfiguration _configuration;
    private Plc _plc;
    private CancellationTokenSource _cts;

    public S7PlcService(ILogger<S7PlcService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("S7 PLC 服务启动中...");

        var configSection = _configuration.GetSection("S7PlcConfig");
        var cpuType = Enum.Parse<CpuType>(configSection["CpuType"]);
        var ipAddress = configSection["IpAddress"];
        var rack = short.Parse(configSection["Rack"]);
        var slot = short.Parse(configSection["Slot"]);

        _plc = new Plc(cpuType, ipAddress, rack, slot);
        _plc.Open();
        _logger.LogInformation("已连接到 PLC");

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = ReadDataLoopAsync(_cts.Token);
        return Task.CompletedTask;
    }

    private async Task ReadDataLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                // 读取 PLC 数据示例（读取 DB1 数据块中的 Real 值）
                var dbValue = (float)_plc.Read("DB1.DBD0");
                _logger.LogInformation("读取到 PLC 数据: {DbValue}", dbValue);

                // 等待一段时间再进行下一次读取
                await Task.Delay(TimeSpan.FromSeconds(1), token);
            }
            catch (Exception ex)
            {
                _logger.LogError("读取 PLC 数据时发生错误: {Message}", ex.Message);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("S7 PLC 服务停止中...");
        _cts?.Cancel();
        _plc?.Close();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        //_plc?.Dispose();
        _cts?.Dispose();
    }
}
