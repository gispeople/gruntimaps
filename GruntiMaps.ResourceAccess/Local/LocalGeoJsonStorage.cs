using GruntiMaps.ResourceAccess.Storage;

namespace GruntiMaps.ResourceAccess.Local
{
    public class LocalGeoJsonStorage : LocalStorage, IGeoJsonStorage
    {
        public LocalGeoJsonStorage(string storagePath, string containerName)
            : base(storagePath, containerName)
        {
        }
    }
}
