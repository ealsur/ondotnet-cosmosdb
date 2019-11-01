using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;

namespace episode1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Fluent builder
            CosmosClient client = new CosmosClientBuilder(configuration.GetConnectionString("Cosmos"))
                    .WithApplicationName("OnDotNetRocks")
                    .WithConnectionModeGateway()
                    .WithApplicationRegion(Regions.WestUS2)
                    .WithConsistencyLevel(ConsistencyLevel.Session)
                    .WithThrottlingRetryOptions(
                        TimeSpan.FromSeconds(10),
                        5)
                    .Build();

            // Similar non-fluent initialization
            // CosmosClient client = new CosmosClient(configuration.GetConnectionString("Cosmos"), 
            //     new CosmosClientOptions(){
            //        ApplicationName = "OnDotNetRocks",
            //        ApplicationRegion = Regions.WestUS2,
            //        MaxRetryAttemptsOnRateLimitedRequests = 5,
            //        MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(10),
            //        ConnectionMode = ConnectionMode.Gateway,
            //        ConsistencyLevel = ConsistencyLevel.Session
            //     });

            Database database = await client.CreateDatabaseIfNotExistsAsync("OnDotNet");

            // Define container
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
        }
    }
}
