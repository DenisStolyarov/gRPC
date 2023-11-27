using GrpcService.Interceptors;
using GrpcService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ServerLoggerInterceptor>();
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ServerLoggerInterceptor>();
});

var app = builder.Build();

app.MapGrpcService<DataService>();
app.MapGrpcService<MethodService>();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
