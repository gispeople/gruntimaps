using GruntiMaps.ResourceAccess.Storage;
using Microsoft.Extensions.Logging;

namespace GruntiMaps.ResourceAccess.Azure
{
    public class AzureStyleStorage : AzureStorage, IStyleStorage
    {
        public AzureStyleStorage(string connectionString, string containerName, ILogger logger)
            : base(connectionString, containerName, logger)
        {
        }
    }
}
