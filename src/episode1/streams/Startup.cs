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
            CosmosClient client = new CosmosClientBuilder(this.Configuration.GetConnectionString("Cosmos"))
                    .WithApplicationName("OnDotNetRocks")
                    .WithApplicationRegion(Regions.WestUS2)
                    .WithCustomSerializer(new TextJsonSerializer())
                    .WithConsistencyLevel(ConsistencyLevel.Session)
                    .WithThrottlingRetryOptions(
                        TimeSpan.FromSeconds(10),
                        5)
                    // .WithSerializerOptions(new CosmosSerializationOptions(){
                    //     IgnoreNullValues = true,
                    //     Indented = false,
                    //     PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                    // })
                    .Build();

            services.AddSingleton(client);            
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
