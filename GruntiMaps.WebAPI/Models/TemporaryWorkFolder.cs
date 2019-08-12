using System;
using System.IO;

namespace GruntiMaps.WebAPI.Models
{
    public class TemporaryWorkFolder : IDisposable
    {
        public string Path { get; }

        public TemporaryWorkFolder()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(Path);
        }

        public string CreateSubFolder(string folder)
        {
            var subFolderPath = System.IO.Path.Combine(Path, folder);
            Directory.CreateDirectory(subFolderPath);
            return subFolderPath;
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Path, true);
            }
            catch
            {
                // don't care if deletion failed
            }
            
        }
    }
}
