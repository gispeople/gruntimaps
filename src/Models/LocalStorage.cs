using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GruntiMaps.Interfaces;

namespace GruntiMaps.Models
{
    public class LocalStorage: IStorageContainer
    {
        public LocalStorage(Options options, string containerName)
        {

        }
        
        public Task<string> Store(string fileName, string inputPath)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> GetIfNewer(string fileName, string outputPath)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<string>> List()
        {
            throw new System.NotImplementedException();
        }
    }
}