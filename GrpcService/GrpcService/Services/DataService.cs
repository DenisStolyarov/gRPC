#define DEFAULT

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Data;
using static Grpc.Services.DataService;

namespace GrpcService.Services;

public class DataService : DataServiceBase
{
    public override Task<Data> GetData(Empty request, ServerCallContext context) =>
        Task.FromResult(DataGenerator.GenerateData());

    public override Task<NullData> GetNullData(Empty request, ServerCallContext context) =>
        Task.FromResult(DataGenerator.GenerateNullData());

    public override Task<CollectionData> GetCollectionData(Empty request, ServerCallContext context) =>
        Task.FromResult(DataGenerator.GenerateCollectionData());

    public override Task<AnyData> GetAnyData(Empty request, ServerCallContext context) =>
        Task.FromResult(DataGenerator.GenerateAnyData());

    public override Task<OneOfData> GetOneOfData(Empty request, ServerCallContext context) =>
        Task.FromResult(DataGenerator.GenerateOneOfData());

    public override Task<ValueData> GetValueData(Empty request, ServerCallContext context) =>
        Task.FromResult(DataGenerator.GenerateValueData());

    private static class DataGenerator
    {
        public static Data GenerateData()
        {
            return new Data()
            {
                IntValue = 0,
                LongValue = 1L,
                FloatValue = 0.0f,
                DoubleValue = 1.0f,
                BoolValue = true,
                StringValue = "Hello World!",
                BytesValue = ByteString.CopyFrom(1, 2, 3),
                Time = Timestamp.FromDateTime(DateTime.UtcNow),
                Duration = Duration.FromTimeSpan(TimeSpan.Zero),
            };
        }

        public static NullData GenerateNullData()
        {
            return new NullData()
            {
                IntValue = null,
                LongValue = null,
                FloatValue = null,
                DoubleValue = null,
                BoolValue = null,
                StringValue = null,
                BytesValue = null,
            };
        }

        public static CollectionData GenerateCollectionData()
        {
            CollectionData data = new();
            int[] values = new int[] { 1, 2, 3 };

            data.ListValue.AddRange(values);

            data.KeyToValue.Add(1, 10);
            data.KeyToValue.Add(2, 20);
            data.KeyToValue.Add(3, 30);

            return data;
        }

        public static AnyData GenerateAnyData()
        {
            AnyData any = new();
            Data data = GenerateData();

            any.Data = Any.Pack(data);

            return any;
        }

        public static OneOfData GenerateOneOfData()
        {
            OneOfData data = new()
            {
                Success = new OneOfData.Types.Success
                {
                    Status = Grpc.Data.Status.Success,
                },
            };

#if !DEFAULT

        // A oneof field ensures that only one of its fields can be set at a time.
        // If set Error field, it will automatically clear Success field, and vice versa.
        data.Error = new OneOfData.Types.Error
        {
            Status = Grpc.Data.Status.Error,
        };

#endif

            return data;
        }

        public static ValueData GenerateValueData()
        {
#if DEFAULT

            ValueData value = new()
            {
                Data = Value.ForStruct(
                new Struct()
                {
                    Fields =
                    {
                    ["bool"] = Value.ForBool(true),
                    ["string"] = Value.ForString("Hello world!"),
                    ["list"] = Value.ForList(
                        Value.ForNumber(1),
                        Value.ForNumber(2)),
                    },
                }),
            };

#else

        ValueData value = new()
        {
            Data = Value.Parser.ParseJson(
            @"{
						""bool"": true,
						""string"": ""Hello world!"",
						""list"": [1, 2],
					}"),
        };

#endif

            return value;
        }
    }
}
