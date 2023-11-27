using System.Diagnostics.CodeAnalysis;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace GrpcService.Interceptors;

public class ServerLoggerInterceptor : Interceptor
{
    private static readonly Action<ILogger, string, string, Exception> StartingReceivingCallValue =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(1, "StartingReceivingCall"),
            "Starting receiving call. Type/Method: {Type} / {Method}");

    private static readonly Action<ILogger, string, Exception> ErrorThrownByMethodValue =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(2, "ErrorThrownByMethod"),
            "Error thrown by {Method}.");

    private readonly ILogger logger;

    public ServerLoggerInterceptor([NotNull] ILogger<ServerLoggerInterceptor> logger)
    {
        this.logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        [NotNull] ServerCallContext context,
        [NotNull] UnaryServerMethod<TRequest, TResponse> continuation)
    {
        this.StartingReceivingCall(context.Method);

        try
        {
            return await continuation(request, context);
        }
        catch (Exception ex)
        {
            this.ErrorThrownByMethod(context.Method, ex);
            throw;
        }
    }

    private void StartingReceivingCall(string method)
    {
        StartingReceivingCallValue(this.logger, MethodType.Unary.ToString(), method, null);
    }

    private void ErrorThrownByMethod(string method, Exception ex)
    {
        ErrorThrownByMethodValue(this.logger, method, ex);
    }
}
