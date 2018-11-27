using GruntiMaps.ResourceAccess.Storage;
using Microsoft.Extensions.Logging;

namespace GruntiMaps.ResourceAccess.Azure
{
    public class AzurePackStorage : AzureStorage, IPackStorage
    {
        public AzurePackStorage(string storageAccount, string storageKey, string containerName, ILogger logger)
            : base(storageAccount, storageKey, containerName, logger)
        {
        }
    }
}
