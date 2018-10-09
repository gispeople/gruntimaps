using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GruntiMaps.Interfaces;

namespace GruntiMaps.Models
{
    public class LocalStorage: IStorageContainer
    {
        private string LocalStoreLocation;
        private string Container;
        private string ContainerPath;
        public LocalStorage(Options options, string containerName)
        {
            LocalStoreLocation = options.StoragePath;
            Container = containerName;
            ContainerPath = Path.Combine(LocalStoreLocation, Container);
            Directory.CreateDirectory(ContainerPath);
        }

        // the methods are not really asynchronous but this makes it consistent with other providers
        public async Task<string> Store(string fileName, string inputPath)
        {
            var outputPath = Path.Combine(ContainerPath, fileName);
            if (File.Exists(inputPath)) {
                File.Copy(inputPath, outputPath);
            }
            return outputPath;
        }

        public async Task<bool> GetIfNewer(string fileName, string outputPath)
        {
            var inputPath = Path.Combine(ContainerPath, fileName);
            // if the source doesn't exist, not much to do
            if (File.Exists(inputPath)) {
                // if the dest doesn't exist, no need to check length
                if (File.Exists(outputPath)) {
                    var inputAttrs = new FileInfo(inputPath);
                    var outputAttrs = new FileInfo(outputPath);
                    if (inputAttrs.Length == outputAttrs.Length) {
                        return false;
                    }
                }
                File.Copy(inputPath, outputPath);
                return true;
            }
            return false;
        }

        public async Task<List<string>> List()
        {
            var result = new List<string>();
            foreach (var fileName in Directory.EnumerateFiles(ContainerPath)) {
                result.Add(fileName);
            }
            return result;
        }
    }
}