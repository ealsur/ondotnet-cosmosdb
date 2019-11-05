namespace episode2
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;
    using CommandLine;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Cosmos.Fluent;
    using Microsoft.Extensions.Configuration;

    public class Program
    {
        private const int ItemsToInsert = 300000;

        static async Task Main(string[] args)
        {
            ParserResult<CommandLineOptions> result = Parser.Default.ParseArguments<CommandLineOptions>(args);
            await result.MapResult(async options => await Program.RunAsync(options), _ => Task.CompletedTask);
        }

        private static async Task RunAsync(CommandLineOptions options)
        {
            bool isBulkEnabled = !options.NoBulk;
            Console.WriteLine($"Running with Bulk Support enabled: {isBulkEnabled}");
            string containerName = isBulkEnabled ? "episode2bulk" : "episode2nobulk";
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            
            CosmosClient cosmosClient = new CosmosClientBuilder(configuration.GetConnectionString("Cosmos"))
                    .WithApplicationName("OnDotNetRocks")
                    .WithBulkExecution(isBulkEnabled)
                    .Build();

            // new CosmosClientOptions() { AllowBulkExecution = isBulkEnabled}

            Database database = await Program.InitializeContainerAsync(cosmosClient, containerName);

            try
            {
                // Prepare items for insertion
                Console.WriteLine($"Preparing {ItemsToInsert} items to insert...");
                Dictionary<PartitionKey, Stream> itemsToInsert = new Dictionary<PartitionKey, Stream>(ItemsToInsert);
                foreach (Item item in Program.GetItemsToInsert())
                {
                    MemoryStream stream = new MemoryStream();
                    await JsonSerializer.SerializeAsync(stream, item);
                    itemsToInsert.Add(new PartitionKey(item.pk), stream);
                }

                // Create the list of Tasks
                Console.WriteLine($"Starting...");
                Stopwatch stopwatch = Stopwatch.StartNew();
                Container container = database.GetContainer(containerName);
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

                // Wait until all are done
                await Task.WhenAll(tasks);
                stopwatch.Stop();

                Console.WriteLine($"Finished in writing {ItemsToInsert} items in {stopwatch.Elapsed}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static async Task<Database> InitializeContainerAsync(CosmosClient cosmosClient, string containerName)
        {
            Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync("OnDotNet");

            try
            {
                await database.DefineContainer(containerName, "/pk")
                    .WithIndexingPolicy()
                        .WithIndexingMode(IndexingMode.Consistent)
                        .WithIncludedPaths()
                            .Attach()
                        .WithExcludedPaths()
                            .Path("/*")
                            .Attach()
                    .Attach()
                .CreateAsync(100000);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                // Expected
            }

            return database;
        }

        private static IReadOnlyCollection<Item> GetItemsToInsert()
        {
            return new Bogus.Faker<Item>()
            .StrictMode(true)
            //Generate item
            .RuleFor(o => o.id, f => Guid.NewGuid().ToString()) //id
            .RuleFor(o => o.username, f => f.Internet.UserName())
            .RuleFor(o => o.pk, (f, o) => o.id) //partitionkey
            .Generate(ItemsToInsert);
        }
    }
}
