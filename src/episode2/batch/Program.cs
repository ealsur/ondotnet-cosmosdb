using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;

namespace episode2
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            
            CosmosClient cosmosClient = new CosmosClientBuilder(configuration.GetConnectionString("Cosmos"))
                    .WithApplicationName("OnDotNetRocks")
                    .Build();

            Container container = await Program.InitializeContainerAsync(cosmosClient);

            Show theShow;
            Episode episode1;
            Episode episode2;
            Episode episode3;
            
            const string showName = "OnDotNet";

            Console.WriteLine("Creating show and episodes...");
            Console.ReadLine();
            TransactionalBatchResponse createShowResponse = await container.CreateTransactionalBatch(new PartitionKey(showName))
                .CreateItem<Show>(Show.Create(showName))
                .CreateItem<Episode>(Episode.Create(showName, "Cosmos DB SDK Features 1"))
                .CreateItem<Episode>(Episode.Create(showName, "Cosmos DB SDK Features 2"))
                .CreateItem<Episode>(Episode.Create(showName, "Cosmos DB SDK Features 3"))
                .ExecuteAsync();

            using (createShowResponse)
            {
                if (createShowResponse.StatusCode != HttpStatusCode.OK)
                {
                    // Log and handle failure
                    LogFailure(createShowResponse);
                    return;
                }

                TransactionalBatchOperationResult<Show> showResult = createShowResponse.GetOperationResultAtIndex<Show>(0);
                theShow = showResult.Resource;
                TransactionalBatchOperationResult<Episode> episode1Result = createShowResponse.GetOperationResultAtIndex<Episode>(1);
                episode1 = episode1Result.Resource;
                TransactionalBatchOperationResult<Episode> episode2Result = createShowResponse.GetOperationResultAtIndex<Episode>(2);
                episode2 = episode2Result.Resource;
                TransactionalBatchOperationResult<Episode> episode3Result = createShowResponse.GetOperationResultAtIndex<Episode>(3);
                episode3 = episode3Result.Resource;
            }

            Console.WriteLine($"Current Show LastUpdate {theShow.LastUpdated}");
            await Task.Delay(2000);
            theShow.LastUpdated = DateTime.UtcNow;
            Console.WriteLine($"Trying to update Show with date {theShow.LastUpdated}");
            Console.ReadLine();
            TransactionalBatchResponse createDuplicatedShow = await container.CreateTransactionalBatch(new PartitionKey(showName))
                .ReplaceItem<Show>(theShow.Id, theShow)
                .CreateItem<Episode>(Episode.Create(showName, "Cosmos DB SDK Features 2"))
                .ExecuteAsync();

            using (createDuplicatedShow)
            {
                TransactionalBatchOperationResult<Show> showResult = createDuplicatedShow.GetOperationResultAtIndex<Show>(0);
                Console.WriteLine($"Batch failed > Show Replace resulted in: {showResult.StatusCode}");
                TransactionalBatchOperationResult<Episode> episodeResult = createDuplicatedShow.GetOperationResultAtIndex<Episode>(1);
                Console.WriteLine($"Batch failed > Episode Create resulted in: {episodeResult.StatusCode}");
            }

            Show storedShow = await container.ReadItemAsync<Show>(theShow.Id, new PartitionKey(showName));
            Console.WriteLine($"Stored Show date after batch fail: {storedShow.LastUpdated}");
            Console.WriteLine("Updating show and episodes...");
            Console.ReadLine();
            theShow.LastUpdated = DateTime.UtcNow;
            episode1.AirDate = DateTime.UtcNow.AddDays(7);
            episode2.AirDate = DateTime.UtcNow.AddDays(14);
            TransactionalBatchResponse updateAirDate = await container.CreateTransactionalBatch(new PartitionKey(showName))
                .ReplaceItem<Show>(theShow.Id, theShow)
                .ReplaceItem<Episode>(episode1.Id, episode1)
                .ReplaceItem<Episode>(episode2.Id, episode2)
                .DeleteItem(episode3.Id)
                .ExecuteAsync();

            using (updateAirDate)
            {
                if (updateAirDate.StatusCode != HttpStatusCode.OK)
                {
                    // Log and handle failure
                    LogFailure(updateAirDate);
                    return;
                }
            }

            storedShow = await container.ReadItemAsync<Show>(theShow.Id, new PartitionKey(showName));
            Console.WriteLine($"Stored Show date {storedShow.LastUpdated}");
            Episode storedEpisode1 = await container.ReadItemAsync<Episode>(episode1.Id, new PartitionKey(showName));
            Console.WriteLine($"Stored Episode 1 date {storedEpisode1.AirDate}");
            Episode storedEpisode2 = await container.ReadItemAsync<Episode>(episode2.Id, new PartitionKey(showName));
            Console.WriteLine($"Stored Episode 2 date {storedEpisode2.AirDate}");
            try
            {
                await container.ReadItemAsync<Episode>(episode3.Id, new PartitionKey(showName));
            }
            catch(CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine("Episode 3 was deleted.");
            }
        }

        private static void LogFailure(TransactionalBatchResponse batchResponse)
        {
            // Note: Please log batchResponse.Diagnostics along with Timestamp once available in the SDK.
            Console.WriteLine("Timestamp={0} Status={1}, ErrorMessage={2}, ActivityId={3}",
                DateTime.UtcNow,
                batchResponse.StatusCode,
                batchResponse.ErrorMessage,
                batchResponse.ActivityId);
        }


        private static async Task<Container> InitializeContainerAsync(CosmosClient cosmosClient)
        {
            Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync("OnDotNet");

            return await database.CreateContainerIfNotExistsAsync("episode2batch", "/Show");
        }
    }
}
