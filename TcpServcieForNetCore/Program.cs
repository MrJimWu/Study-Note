using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// ��� TcpServerService ������ע������
builder.Services.AddHostedService<TcpServerService>();
builder.Services.AddHostedService<UdpServerService>();
// �� ModelBusTcpService ��ӵ�����ע��������
builder.Services.AddHostedService<ModelBusTcpService>();
builder.Services.AddSignalR();


var app = builder.Build();

// �����м����·�ɵ�
app.MapGet("/", () => "Hello World!");

app.Run();
