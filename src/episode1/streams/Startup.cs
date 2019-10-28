using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace episode1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Fluent builder
            CosmosClient client = new CosmosClientBuilder(this.Configuration.GetConnectionString("Cosmos"))
                    .WithApplicationName("OnDotNetRocks")
                    .WithConnectionModeGateway()
                    .WithApplicationRegion(Regions.WestUS2)
                    .WithConsistencyLevel(ConsistencyLevel.Session)
                    .WithThrottlingRetryOptions(
                        TimeSpan.FromSeconds(10),
                        5)
                    .Build();

            // Similar non-fluent initialization
            // CosmosClient client = new CosmosClient(this.Configuration.GetConnectionString("Cosmos"), 
            //     new CosmosClientOptions(){
            //        ApplicationName = "OnDotNetRocks",
            //        ApplicationRegion = Regions.WestUS2,
            //        MaxRetryAttemptsOnRateLimitedRequests = 5,
            //        MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(10),
            //        ConnectionMode = ConnectionMode.Gateway,
            //        ConsistencyLevel = ConsistencyLevel.Session
            //     });

            services.AddSingleton(client);

            IContainerProxy containerProxy = new ContainerProxy(client);
            services.AddSingleton<IContainerProxy>(containerProxy);
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
