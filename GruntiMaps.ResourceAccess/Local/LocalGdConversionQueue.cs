using GruntiMaps.ResourceAccess.Queue;

namespace GruntiMaps.ResourceAccess.Local
{
    public class LocalGdConversionQueue : LocalQueue, IGdConversionQueue
    {
        public LocalGdConversionQueue(string storagePath, int queueTimeLimit, int queueEntryTries, string queueName) 
            : base(storagePath, queueTimeLimit, queueEntryTries, queueName)
        {
        }
    }
}
