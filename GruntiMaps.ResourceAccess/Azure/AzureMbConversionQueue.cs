using GruntiMaps.ResourceAccess.Queue;

namespace GruntiMaps.ResourceAccess.Azure
{
    public class AzureMbConversionQueue : AzureQueue, IMbConversionQueue
    {
        public AzureMbConversionQueue(string key, string account, string queueName) 
            : base(key, account, queueName)
        {
        }
    }
}
