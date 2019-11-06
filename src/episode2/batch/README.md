# Episode 2 - Transactional Batch

[<- Back to root](../../../README.md)

Transaction batch aims at creating a single transactional unit that will either commit or rollback as a whole.

In the [example](./Program.cs#L35), we create the OnDotNet show along with 3 episodes:

```csharp
TransactionalBatchResponse createShowResponse = await container.CreateTransactionalBatch(new PartitionKey(showName))
    .CreateItem<Show>(Show.Create(showName))
    .CreateItem<Episode>(Episode.Create(showName, "Cosmos DB SDK Features 1"))
    .CreateItem<Episode>(Episode.Create(showName, "Cosmos DB SDK Features 2"))
    .CreateItem<Episode>(Episode.Create(showName, "Cosmos DB SDK Features 3"))
    .ExecuteAsync();
```

The result contains one response for each operation as a `TransactionalBatchOperationResult` and can be obtained through the `GetOperationResultAtIndex` method using the operation index:

```csharp
TransactionalBatchOperationResult<Show> showResult = createShowResponse.GetOperationResultAtIndex<Show>(0);
theShow = showResult.Resource;
TransactionalBatchOperationResult<Episode> episode1Result = createShowResponse.GetOperationResultAtIndex<Episode>(1);
episode1 = episode1Result.Resource;
```

If any single of these items fails, the entire operation fails.

In a [following example](./Program.cs#L66), trying to create a duplicate as part of a transaction that does another valid operation will fail:

```csharp
TransactionalBatchResponse createDuplicatedShow = await container.CreateTransactionalBatch(new PartitionKey(showName))
    .ReplaceItem<Show>(theShow.Id, theShow)
    .CreateItem<Episode>(Episode.Create(showName, "Cosmos DB SDK Features 2"))
    .ExecuteAsync();
```

And the `TransactionalBatchOperationResult` will show that by returning a status of `FailedDependency` for those items that failed as part of the batch failing:

```console
Batch failed > Show Replace resulted in: FailedDependency
Batch failed > Episode Create resulted in: Conflict
```