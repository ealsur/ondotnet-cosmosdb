using Microsoft.Azure.Cosmos;

namespace episode1
{
    public class ContainerProxy : IContainerProxy
    {
        private readonly Container mainContainer;
        private readonly Container otherContainer;
        public ContainerProxy(CosmosClient client)
        {
            this.mainContainer = client.GetContainer("OnDotNet", "episode1");
            this.otherContainer = client.GetContainer("OnDotNet", "episode2");
        }

        public Container GetMainContainer() => this.mainContainer;

        public Container GetAnotherContainer() => this.otherContainer;
    }
}