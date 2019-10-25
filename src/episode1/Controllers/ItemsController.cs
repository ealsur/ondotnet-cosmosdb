using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Net.Http.Headers;

namespace episode1
{
    public class ItemController : Controller
    {
        private readonly Container container;
        public ItemController(CosmosClient client)
        {
            this.container = client.GetContainer("OnDotNet", "episode1");
        }

        [Route("/item/read/{id}")]
        [HttpGet]
        public async Task<IActionResult> ReadItemAsync(string id)
        {
            ResponseMessage response = await container.ReadItemStreamAsync(id, new PartitionKey(id));
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int) response.StatusCode, response.ErrorMessage);
            }

            return new FileStreamResult(response.Content, new MediaTypeHeaderValue("application/json"));
        }

        [Route("/item/save/{id}")]
        [HttpPost]
        public async Task<IActionResult> SaveItemAsync(string id)
        {
            ResponseMessage response = await container.CreateItemStreamAsync(HttpContext.Request.Body, new PartitionKey(id));
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int) response.StatusCode, response.ErrorMessage);
            }

            return StatusCode(201);
        }

        [Route("/item/readtype/{id}")]
        [HttpGet]
        public async Task<IActionResult> ReadTypedItemAsync(string id)
        {
            try
            {
                Model response = await container.ReadItemAsync<Model>(id, new PartitionKey(id));
                return new OkObjectResult(response.DescriptiveTitle);
            }
            catch (CosmosException exception)
            {
                return StatusCode((int) exception.StatusCode, exception.Message);
            }
        }
    }
}