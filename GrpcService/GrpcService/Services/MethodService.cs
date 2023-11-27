// #define CANCEL

using System.Diagnostics.CodeAnalysis;
using Grpc.Core;
using Grpc.Data;
using static Grpc.Services.MethodService;

namespace GrpcService.Services;

public class MethodService : MethodServiceBase
{
    public override Task<UnaryResponse> UnaryCall(
        UnaryRequest request,
        ServerCallContext context)
    {
        UnaryResponse response = new()
        {
            Data = "Pong",
        };

        return Task.FromResult(response);
    }

    public override async Task StreamingFromServer(
        UnaryRequest request,
        [NotNull] IServerStreamWriter<ServerStreamingResponse> responseStream,
        ServerCallContext context)
    {
#if CANCEL

			while (!context.CancellationToken.IsCancellationRequested)
			{
				ServerStreamingResponse serverStreamingResponse = new()
				{
					Data = "Pong Infinity",
					Count = Random.Shared.Next(0, 10),
				};

				await responseStream.WriteAsync(serverStreamingResponse);
				await Task.Delay(TimeSpan.FromSeconds(1), context.CancellationToken);
			}

#else

        for (int i = 1; i <= 5; i++)
        {
            ServerStreamingResponse serverStreamingResponse = new()
            {
                Data = "Pong",
                Count = i,
            };

            await responseStream.WriteAsync(serverStreamingResponse);
            await Task.Delay(TimeSpan.FromSeconds(i));
        }

#endif
    }

    public override async Task<UnaryResponse> StreamingFromClient(
        IAsyncStreamReader<ClientStreamingRequest> requestStream,
        ServerCallContext context)
    {
        int count = 0;

        await foreach (ClientStreamingRequest? request in requestStream.ReadAllAsync())
        {
            count += request.Count;

#if CANCEL

				if (count > 5)
				{
					return new()
					{
						Data = $"Count: {count}"
					};
				}

#endif
        }

        return new()
        {
            Data = $"Count: {count}",
        };
    }

    public override async Task StreamingBothWays(
        [NotNull] IAsyncStreamReader<ClientStreamingRequest> requestStream,
        [NotNull] IServerStreamWriter<ServerStreamingResponse> responseStream,
        ServerCallContext context)
    {
        int sum = 0;
        int iteration = 0;

        Task sumTask = Task.Run(async () =>
        {
            await foreach (ClientStreamingRequest request in requestStream.ReadAllAsync())
            {
                sum += request.Count;
            }
        });

        while (!sumTask.IsCompleted)
        {
            iteration++;

            ServerStreamingResponse response = new()
            {
                Data = $"Iteration: {iteration}",
                Count = sum,
            };

            await responseStream.WriteAsync(response);
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}
