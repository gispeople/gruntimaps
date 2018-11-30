using GruntiMaps.ResourceAccess.Storage;

namespace GruntiMaps.ResourceAccess.Local
{
    public class LocalPackStorage : LocalStorage, IPackStorage
    {
        public LocalPackStorage(string storagePath, string containerName)
            : base(storagePath, containerName)
        {
        }
    }
}
