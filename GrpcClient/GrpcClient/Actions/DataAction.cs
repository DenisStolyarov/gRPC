using Google.Protobuf.WellKnownTypes;
using Grpc.Data;
using Grpc.Net.Client;

using static Grpc.Services.DataService;

namespace GrpcClient.Actions
{
	public static class DataAction
	{
		internal static async Task Main()
		{
			using var channel = GrpcChannel.ForAddress("https://localhost:7019");

			DataServiceClient dataServiceClient = new(channel);

			Data data = await dataServiceClient.GetDataAsync(new Empty());
			NullData nullData = await dataServiceClient.GetNullDataAsync(new Empty());
			CollectionData collectionData = await dataServiceClient.GetCollectionDataAsync(new Empty());
			AnyData AnyData = await dataServiceClient.GetAnyDataAsync(new Empty());
			OneOfData OneOfData = await dataServiceClient.GetOneOfDataAsync(new Empty());
			ValueData ValueData = await dataServiceClient.GetValueDataAsync(new Empty());

			Console.WriteLine(data);
			Console.WriteLine(nullData);
			Console.WriteLine(collectionData);
			Console.WriteLine(AnyData);
			Console.WriteLine(OneOfData);
			Console.WriteLine(ValueData);
		}
	}
}
