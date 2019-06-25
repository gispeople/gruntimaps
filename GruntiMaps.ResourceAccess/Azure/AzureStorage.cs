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
using System.Threading.Tasks;
using Flurl;
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
            _logger = logger ?? throw new NullReferenceException(nameof(logger));
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

        public Task<bool> Exist(string fileName)
        {
            return _azureContainer.GetBlockBlobReference(fileName).ExistsAsync();
        }

        public async Task<string> GetMd5(string fileName)
        {
            var blob = _azureContainer.GetBlockBlobReference(fileName);
            if (await blob.ExistsAsync())
            {
                await blob.FetchAttributesAsync();
                return blob.Properties.ContentMD5;
            }

            return null;
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

        public Task<Url> GetDownloadUrl(string fileName)
        {
            return Task.FromResult<Url>(_azureContainer.GetBlockBlobReference(fileName).Uri.AbsoluteUri);
        }

        public Task<List<string>> List()
        {
            var directory = _azureContainer.GetDirectoryReference("");
            return RecursiveListFile(directory);
        }

        private async Task<List<string>> RecursiveListFile(IListBlobItem listBlobItem)
        {
            switch (listBlobItem)
            {
                case CloudBlockBlob blob:
                    return new List<string> {blob.Name};
                case CloudBlobDirectory directory:
                    BlobContinuationToken continuationToken = null;
                    var files = new List<string>();
                    do
                    {
                        var response = await directory.ListBlobsSegmentedAsync(continuationToken);
                        continuationToken = response.ContinuationToken;

                        foreach (var result in response.Results)
                        {
                            files.AddRange(await RecursiveListFile(result));
                        }
                    } while (continuationToken != null);
                    return files;
                default:
                    return new List<string>();
            }
        }
    }
}