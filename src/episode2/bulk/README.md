# Episode 2 - Bulk support

[<- Back to root](../../../README.md)

Bulk support is an opt-in feature that needs to be enabled as part of the `CosmosClient` [creation](./Program.cs#L33):

```csharp
CosmosClient cosmosClient = new CosmosClientBuilder("connection string")
    .WithBulkExecution(true)
    .Build();

CosmosClient cosmosClient = new CosmosClient("connection string"
    new CosmosClientOptions() {
        AllowBulkExecution = true
    });
```

The idea behind Bulk is to optimize scenarios where mass data ingestion or data migration is needed, **throughput optimization scenarios** where you need to squeeze the provisioned RU/s.

Once enabled, it will affect all **item point operations** (CreateItem, ReadItem, ReplaceItem, UpsertItem, and DeleteItem) that are executed concurrently.

## What is Bulk actually doing?

When Bulk support is enabled, the SDK will group concurrent executing operations by partition affinity and treat them as a single payload. Instead of issuing one service call per operation, the SDK will group up to 100 operations per service call (thus reducing Network latency) dynamically, send them over the wire, and wiring back the result to the user code waiting for the `Task` response.

## How to take advantage of Bulk

As shown in the example, the only requirement is to [execute the point operations concurrently](./Program.cs#L58), so simply creating a list of `Task` that represents the operations, will take advantage of Bulk:

```csharp
List<Task> tasks = new List<Task>(ItemsToInsert);
foreach (KeyValuePair<PartitionKey, Stream> item in itemsToInsert)
{
    tasks.Add(container.CreateItemStreamAsync(item.Value, item.Key)
        .ContinueWith((Task<ResponseMessage> task) =>
        {
            using (ResponseMessage response = task.Result)
            {
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}) status code for operation {response.RequestMessage.RequestUri.ToString()}.");
                }
            }
        }));
}

await Task.WhenAll(tasks);
```

It is recommended to use the [Stream APIs](../../episode1/streams/) to obtain the best performance.

> Bulk support should be used in scenarios that require throughput optimization, it is not designed for latency-sensitive scenarios where a small number of point operations are issued.

## Running the example

The example will create two **100,000 RU/s** containers and store 300,000 items and measure their time. Due to the cost impact, this can be done using the [Cosmos DB Emulator](https://docs.microsoft.com/azure/cosmos-db/local-emulator).

Running `dotnet run -c Release --nobulk` will do the operations with **Bulk disabled**.

Running `dotnet run -c Release` will do the operations with **Bulk enabled**.

The goal is to compare performance of both scenarios, running the same source code (storing 300,000 items), with the Bulk support enabled and disabled.