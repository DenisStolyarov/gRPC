//#define CANCEL

using Grpc.Core;
using Grpc.Data;
using Grpc.Net.Client;

using static Grpc.Services.MethodService;

namespace GrpcClient.Actions
{
	public static class MethodAction
	{
		internal static async Task Main()
		{
			using var channel = GrpcChannel.ForAddress("https://localhost:7019");

			MethodServiceClient methodServiceClient = new(channel);

			await Handle(UnaryCall, nameof(UnaryCall), methodServiceClient);
			await Handle(ServerStreamingCall, nameof(ServerStreamingCall), methodServiceClient);
			await Handle(ClientStreamingCall, nameof(ClientStreamingCall), methodServiceClient);
			await Handle(StreamingBothWays, nameof(StreamingBothWays), methodServiceClient);
			await Handle(DeadlineUnaryCall, nameof(DeadlineUnaryCall), methodServiceClient);
		}

		private static async Task Handle(
			Func<MethodServiceClient, Task> method,
			string methodName,
			MethodServiceClient methodServiceClient)
		{
			Console.WriteLine($"Start: {methodName}");

			await method(methodServiceClient);

			Console.WriteLine($"Finish: {methodName}");
		}

		private static async Task UnaryCall(MethodServiceClient methodServiceClient)
		{
			UnaryRequest request = new()
			{
				Data = "Ping",
			};

			UnaryResponse response = await methodServiceClient.UnaryCallAsync(request);

			Console.WriteLine(response);
		}

		private static async Task ServerStreamingCall(MethodServiceClient methodServiceClient)
		{
			UnaryRequest request = new()
			{
				Data = "Ping",
			};

			using AsyncServerStreamingCall<ServerStreamingResponse> streamingResponse = methodServiceClient.StreamingFromServer(request);

			await foreach (ServerStreamingResponse response in streamingResponse.ResponseStream.ReadAllAsync())
			{
				Console.WriteLine(response);

#if CANCEL

				if (response.Count == 3)
				{
					streamingResponse.Dispose();
					break;
				}

#endif
			}
		}

		private static async Task ClientStreamingCall(MethodServiceClient methodServiceClient)
		{
			using AsyncClientStreamingCall<ClientStreamingRequest, UnaryResponse> clientStreamingCall = methodServiceClient.StreamingFromClient();

			for (int i = 1; i <= 5; i++)
			{
				ClientStreamingRequest clientStreamingRequest = new()
				{
					Data = "Ping",
					Count = i,
				};

				await clientStreamingCall.RequestStream.WriteAsync(clientStreamingRequest);
			}

			await clientStreamingCall.RequestStream.CompleteAsync();

			UnaryResponse response = await clientStreamingCall;

			Console.WriteLine(response);
		}

		private static async Task StreamingBothWays(MethodServiceClient methodServiceClient)
		{
			using AsyncDuplexStreamingCall<ClientStreamingRequest, ServerStreamingResponse> duplexStreamingCall = methodServiceClient.StreamingBothWays();

			Task readTask = Task.Run(async () =>
			{
				await foreach (var response in duplexStreamingCall.ResponseStream.ReadAllAsync())
				{
					Console.WriteLine(response);
				}
			});

			for (int i = 1; i <= 10; i++)
			{
				ClientStreamingRequest clientStreamingRequest = new()
				{
					Data = "Ping",
					Count = i,
				};

				await duplexStreamingCall.RequestStream.WriteAsync(clientStreamingRequest);
				await Task.Delay(500);
			}

			await duplexStreamingCall.RequestStream.CompleteAsync();
			await readTask;
		}

		private static async Task DeadlineUnaryCall(MethodServiceClient methodServiceClient)
		{
			UnaryRequest request = new()
			{
				Data = "Ping",
			};

			try
			{
				UnaryResponse response = await methodServiceClient.UnaryCallAsync(request, deadline: DateTime.UtcNow.AddSeconds(1));

				Console.WriteLine(response);
			}
			catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
			{
				Console.WriteLine("RPC timeout.");
			}
		}
	}
}
