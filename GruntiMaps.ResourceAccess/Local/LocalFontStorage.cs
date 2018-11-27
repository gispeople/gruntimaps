using GruntiMaps.ResourceAccess.Storage;

namespace GruntiMaps.ResourceAccess.Local
{
    public class LocalFontStorage : LocalStorage, IFontStorage
    {
        public LocalFontStorage(string storagePath, string containerName) 
            : base(storagePath, containerName)
        {
        }
    }
}
