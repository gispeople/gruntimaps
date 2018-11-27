using System.Collections.Generic;
using System.Threading.Tasks;

namespace GruntiMaps.ResourceAccess.Storage
{
    public interface IStorage
    {
        // returns the location of the created file (provider-dependent)
        Task<string> Store(string fileName, string inputPath);
        // returns true if it retrieved a newer version of the file, false if no newer version existed (or an error occurred during the check)
        Task<bool> GetIfNewer(string fileName, string outputPath);
        Task<List<string>> List();
    }
}
