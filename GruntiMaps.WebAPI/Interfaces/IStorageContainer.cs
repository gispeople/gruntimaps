using System.Collections.Generic;
using System.Threading.Tasks;

namespace GruntiMaps.WebAPI.Interfaces
{
    public interface IStorageContainer
    {
        // returns the location of the created file (provider-dependent)
        Task<string> Store(string fileName, string inputPath);
        Task<bool> GetIfNewer(string fileName, string outputPath);
        Task<List<string>> List();
    }
}