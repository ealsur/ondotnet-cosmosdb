using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
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
            
            CosmosClient client = new CosmosClientBuilder(configuration.GetConnectionString("Cosmos"))
                    .WithApplicationName("OnDotNetRocks")
                    .Build();

            await Program.InitializeContainersAsync(client);    

            // Parse command line arguments
            ParserResult<CommandLineOptions> result = Parser.Default.ParseArguments<CommandLineOptions>(args);
            try
            {
                await result.MapResult(async options => await Program.ParseOptionsAndRunAsync(options, client), _ => Task.CompletedTask);
                Console.WriteLine("Job's done.");
            }
            catch(ArgumentException exception)
            {
                Console.WriteLine(exception.Message);
                Console.WriteLine("Options: --processor \"<name>\": Starts a processor instance with a particular name.");
                Console.WriteLine("         --estimator: Starts an estimator to watch for changes.");
                Console.WriteLine("         --writer <number>: Starts a writer for a fixed number of documents.");
            }
        }

        private static Task ParseOptionsAndRunAsync(CommandLineOptions options, CosmosClient client)
        {
            Program.ValidateArguments(options);
            if (!string.IsNullOrEmpty(options.Processor))
            {
                return Program.StartProcessorAsync(client, options.Processor);
            }

            if (options.DocumentWriter > 0)
            {
                return Program.StartWriterAsync(client, options.DocumentWriter);
            }

            return Program.StartEstimatorAsync(client);
        }

        private static async Task StartProcessorAsync(CosmosClient client, string instanceName)
        {
            Container container = client.GetContainer("OnDotNet", "changefeed-items");
            Container leasesContainer = client.GetContainer("OnDotNet", "changefeed-leases");

            ChangeFeedProcessor processor = container.GetChangeFeedProcessorBuilder<Model>("OnDotNet", Program.ProcessChangesAsync)
                .WithInstanceName(instanceName)
                .WithLeaseContainer(leasesContainer)
                .Build();

            await processor.StartAsync();
            Console.WriteLine($"Instance {instanceName} started.");
            Console.WriteLine("Press any key to stop.");
            Console.ReadKey();
            await processor.StopAsync();
            Console.WriteLine($"Instance {instanceName} stopped.");
        }

        private static Task ProcessChangesAsync(IReadOnlyCollection<Model> changes, CancellationToken cancellationToken)
        {
            ConsoleColor previousColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            foreach(Model item in changes)
            {
                Console.WriteLine(item.ToString());
            }

            Console.ForegroundColor = previousColor;
            return Task.CompletedTask;
        }

        private static async Task StartEstimatorAsync(CosmosClient client)
        {
            Container container = client.GetContainer("OnDotNet", "changefeed-items");
            Container leasesContainer = client.GetContainer("OnDotNet", "changefeed-leases");

            ChangeFeedProcessor processor = container.GetChangeFeedEstimatorBuilder("OnDotNet", Program.ShowEstimation, TimeSpan.FromSeconds(1))
                .WithLeaseContainer(leasesContainer)
                .Build();

            await processor.StartAsync();
            Console.WriteLine($"Estimator started.");
            Console.WriteLine("Press any key to stop.");
            Console.ReadKey();
            await processor.StopAsync();
            Console.WriteLine($"Estimator stopped.");
        }

        private static Task ShowEstimation(long pendingChanges, CancellationToken cancellationToken)
        {
            ConsoleColor previousColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{pendingChanges} changes to be read.");
            Console.ForegroundColor = previousColor;
            return Task.CompletedTask;
        }

        private static Task StartWriterAsync(CosmosClient client, int documentsToWrite)
        {
            List<Task> tasks = new List<Task>();
            Container container = client.GetContainer("OnDotNet", "changefeed-items");
            Console.WriteLine($"Generating {documentsToWrite} items to insert...");
            foreach(Model item in new Bogus.Faker<Model>()
                                        .StrictMode(true)
                                        .RuleFor(o => o.id, f => Guid.NewGuid().ToString())
                                        .RuleFor(o => o.partitionKey, f => Guid.NewGuid().ToString())
                                        .RuleFor(o => o.userName, f => f.Internet.UserName())
                                        .RuleFor(o => o.email, f => f.Internet.Email())
                                        .RuleFor(o => o.age, f => f.Random.Number(20, 80))
                                        .RuleFor(o => o.favLanguage, f => (Language)f.Random.Number(0, 9))
                                        .Generate(documentsToWrite))
            {
                tasks.Add(container.CreateItemAsync(item));
            }

            return Task.WhenAll(tasks);
        }

        private static async Task InitializeContainersAsync(CosmosClient cosmosClient)
        {
            Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync("OnDotNet");
            await database.CreateContainerIfNotExistsAsync(id: "changefeed-items", partitionKeyPath: "/partitionKey", throughput: 10000);
            await database.CreateContainerIfNotExistsAsync(id: "changefeed-leases", partitionKeyPath: "/id");
        }

        private static void ValidateArguments(CommandLineOptions options)
        {
            bool validArguments = true;
            validArguments |= (options.Estimator && !string.IsNullOrEmpty(options.Processor));
            validArguments |= (options.Estimator && options.DocumentWriter > 0);
            validArguments |= (options.DocumentWriter > 0 && !string.IsNullOrEmpty(options.Processor));
            if (!validArguments)
            {
                throw new ArgumentException("Only one argument mode is allowed.");
            }

            if (!options.Estimator && string.IsNullOrEmpty(options.Processor) && options.DocumentWriter == 0)
            {
                throw new ArgumentException("At least one argument mode should be set.");
            }
        }
    }
}
