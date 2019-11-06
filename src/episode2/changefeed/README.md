# Episode 2 - Change Feed Processor

[<- Back to root](../../../README.md)

This sample covers using the [Change Feed Processor](https://docs.microsoft.com/azure/cosmos-db/change-feed-processor) to create a reactive application that will print changes as they happen in the monitored container through a [Delegate](./Program.cs#L76).

## Starting a Processing

Running `dotnet run --processor "somename"` will [start a processor](./Program.cs#L58) instance. Multiple processor instances can be started with **different names** and [dynamic scaling](https://docs.microsoft.com/azure/cosmos-db/change-feed-processor#dynamic-scaling) will apply.

## Starting an Estimator

Running `dotnet run --estimator` will [start an estimator](./Program.cs#L89), which takes care of [monitoring the pending changes](https://docs.microsoft.com/azure/cosmos-db/how-to-use-change-feed-estimator) to be read by the processor instances.

## Starting a writer

Once at least one processor is started, we can use `dotnet run --writer X` where `X` is the amount of documents to write. The process will [write random data](./Program.cs#L115) that will get picked up by the existing processor(s).