# Episode 1 - Dependency Injection and Streams

[<- Back to root](../../../README.md)

## Dependency Injection

Following [Azure Cosmos DB performance tips for .NET](https://docs.microsoft.com/azure/cosmos-db/performance-tips) it is always good to maintain a **single instance** of the `CosmosClient` for the entire application lifetime.

We can leverage [.NET Core Dependency Injection](https://docs.microsoft.com/aspnet/core/fundamentals/dependency-injection) and [register the client instance](./Startup.cs#L34):

```csharp
public void ConfigureServices(IServiceCollection services)
{
    CosmosClient client = new CosmosClientBuilder("connection string")
            .WithApplicationName("OnDotNetRocks")
            .Build();

    services.AddSingleton(client);
}
```

And consume it across our application, like in an [MVC Controllers](./Controllers/ItemsController.cs#L14):

```csharp
public class ItemController : Controller
{
    public ItemController(CosmosClient client)
    {
    }
}
```

## Streams

The Azure Cosmos DB .NET SDK has Stream support for most of it's operations. The goal of the Stream APIs is to avoid unnecessary serialization when the nature of the information is already binary or when the component being built does not need to worry about parsing and understanding the content, but just relying the operations.

In the [sample code](./Controllers/ItemsController.cs#L26), we create a component that is acting as a middleware, and just does a read operation on a Cosmos container and relies the response to the caller, without doing any kind of serialization:

```csharp
public async Task<IActionResult> ReadItemAsync(string id)
{
    ResponseMessage response = await this.container.ReadItemStreamAsync(id, new PartitionKey(id));
    foreach (string headerName in response.Headers)
    {
        Response.Headers.Add(headerName, response.Headers[headerName]);
    }

    if (!response.IsSuccessStatusCode)
    {
        return StatusCode((int) response.StatusCode, response.ErrorMessage);
    }

    return new FileStreamResult(response.Content, new MediaTypeHeaderValue("application/json"));
}
```

The same can be done for [save operations](./Controllers/ItemsController.cs#L45), by taking the Stream content from the caller and passing it along:

```csharp
public async Task<IActionResult> SaveItemAsync(string id)
{
    ResponseMessage response = await this.container.CreateItemStreamAsync(HttpContext.Request.Body, new PartitionKey(id));
    foreach (string headerName in response.Headers)
    {
        Response.Headers.Add(headerName, response.Headers[headerName]);
    }

    if (!response.IsSuccessStatusCode)
    {
        return StatusCode((int) response.StatusCode, response.ErrorMessage);
    }

    return StatusCode((int) response.StatusCode);
}
```
