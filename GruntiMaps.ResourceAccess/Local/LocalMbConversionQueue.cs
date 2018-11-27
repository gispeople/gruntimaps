using GruntiMaps.ResourceAccess.Queue;

namespace GruntiMaps.ResourceAccess.Local
{
    public class LocalMbConversionQueue : LocalQueue, IMbConversionQueue
    {
        public LocalMbConversionQueue(string storagePath, int queueTimeLimit, int queueEntryTries, string queueName)
            : base(storagePath, queueTimeLimit, queueEntryTries, queueName)
        {
        }
    }
}
