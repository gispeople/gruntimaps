using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GruntiMaps.WebAPI.Interfaces;

namespace GruntiMaps.WebAPI.Models
{
    public class LocalStorage: IStorageContainer
    {
        private readonly string _containerPath;
        public LocalStorage(Options options, string containerName)
        {
            var localStoreLocation = options.StoragePath;
            _containerPath = Path.Combine(localStoreLocation, containerName);
            Directory.CreateDirectory(_containerPath);
        }

        // the methods are not really asynchronous but this makes it consistent with other providers
        public async Task<string> Store(string fileName, string inputPath)
        {

            var outputPath = Path.Combine(_containerPath, fileName);
            if (File.Exists(inputPath)) {
                await Task.Run(() =>
                {
                    File.Copy(inputPath, outputPath);
                });
            }
            return outputPath;
        }

        public async Task<bool> GetIfNewer(string fileName, string outputPath)
        {
            var inputPath = Path.Combine(_containerPath, fileName);
            // if the source doesn't exist, not much to do
            if (!File.Exists(inputPath)) return false;
            // if the dest doesn't exist, no need to check length
            if (File.Exists(outputPath)) {
                var inputAttrs = new FileInfo(inputPath);
                var outputAttrs = new FileInfo(outputPath);
                if (inputAttrs.Length == outputAttrs.Length) {
                    return false;
                }
            }
            await Task.Run(() =>
            {
                File.Copy(inputPath, outputPath);
            });
            return true;
        }

        public async Task<List<string>> List()
        {
            var result = new List<string>();
            await Task.Run(() =>
            {
                DirectoryInfo di = new DirectoryInfo(_containerPath);
                if (!di.Exists) return;
                result.AddRange(di.GetFiles().Select(file => file.Name));
            });
            return result;
        }
    }
}