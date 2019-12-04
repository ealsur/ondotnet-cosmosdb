# Underdog features of the Azure Cosmos DB NET SDK

## Summary

This repo acts as backing material for the [OnDotNET](https://channel9.msdn.com/Shows/On-NET) show talking about cool and new features using the [.NET Cosmos DB SDK](https://docs.microsoft.com/azure/cosmos-db/sql-api-sdk-dotnet-standard).

## Episode 1

During episode 1, we'll cover:

* Take advantage of the [Fluent builders](./src/episode1/builder/) to create your `CosmosClient` instances and define your Cosmos containers.
* Learn how to [customize the serialization process](./src/episode1/customserializer/) by the options in the built-in serializer or introduce your own custom serializer implementation.
* Use the [Stream APIs](./src/episode1/streams/) in your middleware applications to optimize throughput and follow best practices by leveraging [NET Core Dependency Injection](./src/episode1/streams/startup.cs).

Watch the recording! 👇

[![Watch the On Dotnet episode](http://img.youtube.com/vi/uFWWkzYL7tA/0.jpg)](http://www.youtube.com/watch?v=uFWWkzYL7tA "On dotnet episode")

## Episode 2

During episode 2, we'll cover:

* Learn to consume the [Change Feed](./src/episode2/changefeed/) and create reactive applications that listen to changes in Cosmos containers.
* Use the [Transactional Batch](./src/episode2/batch/) to create groups of operations that need to either succeed or fail as a single transaction.
* Leverage the [Bulk support](./src/episode2/bulk/) to optimize throughput for mass ingestion operations.

Watch the recording! 👇

[![Watch the On Dotnet episode](http://img.youtube.com/vi/zEscjbdGLZ4/0.jpg)](http://www.youtube.com/watch?v=zEscjbdGLZ4 "On dotnet episode")
