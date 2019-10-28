using Microsoft.Azure.Cosmos;

namespace episode1
{
    public interface IContainerProxy
    {
        public Container GetMainContainer();

        public Container GetAnotherContainer();
    }
}