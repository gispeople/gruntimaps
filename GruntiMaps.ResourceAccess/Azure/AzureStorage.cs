/*

Copyright 2016, 2017, 2018 GIS People Pty Ltd

This file is part of GruntiMaps.

GruntiMaps is free software: you can redistribute it and/or modify it under 
the terms of the GNU Affero General Public License as published by the Free
Software Foundation, either version 3 of the License, or (at your option) any
later version.

GruntiMaps is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR
A PARTICULAR PURPOSE. See the GNU Affero General Public License for more 
details.

You should have received a copy of the GNU Affero General Public License along
with GruntiMaps.  If not, see <https://www.gnu.org/licenses/>.

*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GruntiMaps.ResourceAccess.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace GruntiMaps.ResourceAccess.Azure
{
    public class AzureStorage : IStorage
    {
        private readonly CloudBlobContainer _azureContainer;
        private readonly ILogger _logger;
        public AzureStorage(string connectionString, string containerName, ILogger logger)
        {
            _logger = logger;
            _azureContainer = CloudStorageAccount
                .Parse(connectionString)
                .CreateCloudBlobClient()
                .GetContainerReference(containerName);
            _azureContainer.CreateIfNotExistsAsync();
            _azureContainer.SetPermissionsAsync(new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            });
        }
        // returns the location of the created file 
        public async Task<string> Store(string fileName, string inputPath)
        {
            var blob = _azureContainer.GetBlockBlobReference(fileName);
            try
            {
                using (var fileStream = File.OpenRead(inputPath))
                {
                    await blob.UploadFromStreamAsync(fileStream);
                }

                return blob.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not read input file at {inputPath}. {ex}");
                return null;
            }
        }

        public async Task<string> GetMd5(string fileName)
        {
            var blob = _azureContainer.GetBlockBlobReference(fileName);
            await blob.FetchAttributesAsync();
            return blob.Properties.ContentMD5;
        }

        public async Task UpdateLocalFile(string fileName, string localPath)
        {
            CloudBlockBlob blob = _azureContainer.GetBlockBlobReference(fileName);
            try
            {
                using (var fileStream = File.OpenWrite(localPath))
                {
                    await blob.DownloadToStreamAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not write to output file at {localPath}. {ex}");
            }
        }

        public Task<bool> DeleteIfExist(string fileName)
        {
            CloudBlockBlob blob = _azureContainer.GetBlockBlobReference(fileName);
            return blob.DeleteIfExistsAsync();
        }

        public async Task<List<string>> List()
        {
            BlobContinuationToken continuationToken = null;
            var result = new List<string>();
            do
            {
                var response = await _azureContainer.ListBlobsSegmentedAsync(continuationToken);
                continuationToken = response.ContinuationToken;

                result.AddRange(from item in response.Results where item.GetType() == typeof(CloudBlockBlob) select ((CloudBlockBlob)item).Name);
            } while (continuationToken != null);

            return result;
        }
    }
}