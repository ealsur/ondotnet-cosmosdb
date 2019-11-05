# Episode 1 - Custom serialization

[<- Back to root](../../../README.md)

The Azure Cosmos DB .NET SDK uses [JsonNET](https://www.nuget.org/packages/Newtonsoft.Json) as the default serialization engine for item operations.

It provides customization of the serialization process through the `CosmosSerializationOptions` that can be defined on the `CosmosClient` [creation](./Program.cs#L44):

```csharp
CosmosClient client = new CosmosClientBuilder("connection string")
    .WithApplicationName("OnDotNetRocks")
    .WithSerializerOptions(new CosmosSerializationOptions(){
        IgnoreNullValues = true,
        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
    })
    .Build();
```

And going a step further, you can [completely replace the serialization engine](./Program.cs#L25) with your own by implementing the `CosmosSerializer` interface:

```csharp
CosmosClient client = new CosmosClientBuilder("connection string")
    .WithApplicationName("OnDotNetRocks")
    .WithCustomSerializer(new MySerializer())
    .Build();

public class MySerializer : CosmosSerializer
{
    public override T FromStream<T>(Stream stream)
    {
        ...
    }

    public override Stream ToStream<T>(T input)
    {
        ....
    }
}
```
