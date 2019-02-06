using GruntiMaps.ResourceAccess.Storage;

namespace GruntiMaps.ResourceAccess.Local
{
    public class LocalStyleStorage : LocalStorage, IStyleStorage
    {
        public LocalStyleStorage(string storagePath, string containerName)
            : base(storagePath, containerName)
        {
        }
    }
}
