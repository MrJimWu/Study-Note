
using Microsoft.Extensions.Configuration;

namespace DeleteForDoc
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly ILogger<TimedHostedService> _logger;
        private readonly IConfiguration _configuration;
        private string _directoryPath;
        private int _daysOld;
        private int _intervalSeconds;

        public TimedHostedService(ILogger<TimedHostedService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            // 从配置文件中读取参数
             this._directoryPath = _configuration["FileCleanupSettings:DirectoryPath"];
             this._daysOld = int.Parse(_configuration["FileCleanupSettings:DaysOld"]);
             this._intervalSeconds = int.Parse(_configuration["FileCleanupSettings:IntervalSeconds"]);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("定时任务服务开始...");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(_intervalSeconds)); // 每10秒执行一次任务
            return Task.CompletedTask;
        }


        private void DoWork(object state)
        {
            try
            {
                // 检查文件夹是否存在
                if (Directory.Exists(_directoryPath))
                {
                    // 获取文件夹中的所有文件
                    var files = Directory.GetFiles(_directoryPath);
                    foreach (var file in files)
                    {
                        // 获取文件的最后修改时间
                        DateTime lastWriteTime = File.GetLastWriteTime(file);

                        // 如果文件的修改时间早于当前时间减去指定天数（例如7天），则删除文件
                        if (lastWriteTime < DateTime.Now.AddDays(-_daysOld))
                        {
                            File.Delete(file);
                            _logger.LogInformation($"已删除文件: {file}, 最后修改时间: {lastWriteTime}");
                        }
                    }
                    if (files.Length == 0)
                    {
                        _logger.LogInformation("没有找到需要删除的文件。");
                    }
                }
                else
                {
                    _logger.LogWarning($"目录 {_directoryPath} 不存在。");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"删除文件时发生错误: {ex.Message}");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("定时任务服务停止...");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
