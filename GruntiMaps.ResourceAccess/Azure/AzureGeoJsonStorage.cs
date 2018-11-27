using GruntiMaps.ResourceAccess.Storage;
using Microsoft.Extensions.Logging;

namespace GruntiMaps.ResourceAccess.Azure
{
    public class AzureGeoJsonStorage : AzureStorage, IGeoJsonStorage
    {
        public AzureGeoJsonStorage(string storageAccount, string storageKey, string containerName, ILogger logger)
            : base(storageAccount, storageKey, containerName, logger)
        {
        }
    }
}
