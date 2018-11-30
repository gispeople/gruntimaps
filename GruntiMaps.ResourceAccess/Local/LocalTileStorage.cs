using GruntiMaps.ResourceAccess.Storage;

namespace GruntiMaps.ResourceAccess.Local
{
    public class LocalTileStorage : LocalStorage, ITileStorage
    {
        public LocalTileStorage(string storagePath, string containerName)
            : base(storagePath, containerName)
        {
        }
    }
}
