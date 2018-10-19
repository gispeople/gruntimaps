using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GruntiMaps.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace GruntiMaps.Models
{
    public class AzureStorage: IStorageContainer
    {        
        public CloudStorageAccount CloudAccount { get; }
        public CloudBlobClient CloudClient { get; }
        private CloudBlobContainer AzureContainer { get; }
        public AzureStorage(Options options, string containerName)
        {
            CloudAccount =
                new CloudStorageAccount(
                    new StorageCredentials(options.StorageAccount, options.StorageKey), true);
            CloudClient = CloudAccount.CreateCloudBlobClient();
            AzureContainer = CloudClient.GetContainerReference(containerName);
            AzureContainer.CreateIfNotExistsAsync();
            AzureContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
        }
        // returns the location of the created file 
        public async Task<string> Store(string fileName, string inputPath)
        {
            CloudBlockBlob blob = (CloudBlockBlob)AzureContainer.GetBlobReference(fileName);

            using (var fileStream = File.OpenRead(inputPath))
            {
                await blob.UploadFromStreamAsync(fileStream);
            }

            return blob.Uri.ToString();
        }

        public async Task<bool> GetIfNewer(string fileName, string outputPath)
        {
            var result = false;
            CloudBlockBlob blob = (CloudBlockBlob)AzureContainer.GetBlobReference(fileName);
            if (!MatchesLength(fileName, blob.Properties.Length))
            {
                using (var fileStream = File.OpenWrite(outputPath))
                {
                    await blob.DownloadToStreamAsync(fileStream);
                    result = true;
                }
            }

            return result;
        }

        private static bool MatchesLength(string filepath, long expectedLength)
        {
            var result = false;
            // don't retrieve pack if we already have it (TODO: check should probably be more than just size)
            if (File.Exists(filepath))
            {
                var fi = new FileInfo(filepath);
                if (fi.Length == expectedLength) result = true;
            }

            return result;
        }

        public async Task<List<string>> List()
        {
            BlobContinuationToken continuationToken = null;
            var result = new List<string>();
            do
            {
                var response = await AzureContainer.ListBlobsSegmentedAsync(continuationToken);
                continuationToken = response.ContinuationToken;

                foreach (var item in response.Results)
                    if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        result.Add(((CloudBlockBlob)item).Name);
                    }
            } while (continuationToken != null);

            return result;
        }
    }
}