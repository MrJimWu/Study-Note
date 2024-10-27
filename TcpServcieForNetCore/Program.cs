using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// 添加 TcpServerService 到依赖注入容器
builder.Services.AddHostedService<TcpServerService>();
builder.Services.AddHostedService<UdpServerService>();
// 将 ModelBusTcpService 添加到依赖注入容器中
builder.Services.AddHostedService<ModelBusTcpService>();
builder.Services.AddSignalR();


var app = builder.Build();

// 配置中间件和路由等
app.MapGet("/", () => "Hello World!");

app.Run();
