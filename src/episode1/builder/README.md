# Episode 1 - Builders

[<- Back to root](../../README.md)

The Azure Cosmos DB .NET SDK lets you use the Fluent or Builder pattern to [create the `CosmosClient`](./Program.cs#L18) instance:

```csharp
CosmosClient client = new CosmosClientBuilder("your-connection-string")
        .WithApplicationName("OnDotNetRocks")
        .WithApplicationRegion(Regions.WestUS2)
        .WithThrottlingRetryOptions(
            TimeSpan.FromSeconds(10),
            5)
        .Build();
```

That declaratively let you customize settings that are available within the `CosmosClientOptions`.

And you can also use the same pattern to [create Cosmos containers](./Program.cs#L42):

```csharp
Container container = await database.DefineContainer("episode1builder", "/pk")
    .WithDefaultTimeToLive(TimeSpan.FromSeconds(60))
    .WithIndexingPolicy()
        .WithIndexingMode(IndexingMode.Consistent)
        .WithExcludedPaths()
            .Path("/excludeThis/*")
            .Path("/andExcludeThis/*")
            .Attach()
        .WithIncludedPaths()
            .Path("/*")
            .Attach()
        .WithCompositeIndex()
            .Path("/composite", CompositePathSortOrder.Ascending)
            .Path("/composite2", CompositePathSortOrder.Ascending)
            .Attach()
        .Attach()
    .WithUniqueKey()
        .Path("/uniqueKey")
        .Attach()
    .CreateIfNotExistsAsync();
```

All extensions in the Builders are available using the base classes and options.