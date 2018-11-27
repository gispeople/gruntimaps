using GruntiMaps.ResourceAccess.Queue;

namespace GruntiMaps.ResourceAccess.Azure
{
    public class AzureGdConversionQueue : AzureQueue, IGdConversionQueue
    {
        public AzureGdConversionQueue(string key, string account, string queueName) : base(key, account, queueName)
        {
        }
    }
}
