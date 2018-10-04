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
using System.IO.Compression;
using System.Threading.Tasks;
using GruntiMaps.Interfaces;
using GruntiMaps.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using static System.IO.Directory;

namespace GruntiMaps.Models
{
    /// <summary>
    ///     MapData provides methods and properties related to the map store's location and contents.
    /// </summary>
    public class MapData : IMapData
    {
        private readonly ILogger<MapData> _logger;
        // we need to be able to see the watcher so we can disable it while downloading layers
        private FileSystemWatcher watcher = new FileSystemWatcher();
        public CloudBlobContainer PackContainer;
        public CloudBlobContainer TileContainer;
        public CloudBlobContainer MbtContainer { get; }
        public CloudBlobContainer GeojsonContainer { get; }
        public Options CurrentOptions { get; }

        public CloudStorageAccount CloudAccount { get; }
        public CloudBlobClient CloudClient { get; }
        public Dictionary<string, ILayer> LayerDict { get; set; } = new Dictionary<string, ILayer>();

        public MapData(Options options, ILogger<MapData> logger)
        {
            CurrentOptions = options;
            _logger = logger;
            _logger.LogDebug($"Creating MapData root={CurrentOptions.RootDir}");
            CloudAccount =
                new CloudStorageAccount(
                    new StorageCredentials(CurrentOptions.StorageAccount, CurrentOptions.StorageKey), true);
            CloudClient = CloudAccount.CreateCloudBlobClient();
            PackContainer = CloudClient.GetContainerReference(CurrentOptions.StorageContainer);
            PackContainer.CreateIfNotExistsAsync();
            PackContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            TileContainer = CloudClient.GetContainerReference(CurrentOptions.MbTilesContainer);
            TileContainer.CreateIfNotExistsAsync();
            TileContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            MbtContainer = CloudClient.GetContainerReference(CurrentOptions.MbTilesContainer);
            MbtContainer.CreateIfNotExistsAsync();
            MbtContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            GeojsonContainer = CloudClient.GetContainerReference(CurrentOptions.GeoJsonContainer);
            GeojsonContainer.CreateIfNotExistsAsync();
            GeojsonContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            CheckDirectories();
            PopulateFonts();
            // RefreshLayers(); // we can do this as part of our background task.
            OpenTiles();
        }

        public async Task CreateGdalConversionRequest(ConversionMessageData messageData)
        {
            await CreateConversionRequest(messageData, CurrentOptions.GdConvQueue);
        }

        public async Task CreateMbConversionRequest(ConversionMessageData messageData)
        {
            await CreateConversionRequest(messageData, CurrentOptions.MbConvQueue);
        }

        public async Task CreateConversionRequest(ConversionMessageData messageData, string queueName)
        {
            var queueClient = CloudAccount.CreateCloudQueueClient();
            // _logger.LogDebug($"Monitoring {_mapdata.CurrentOptions.GdConvQueue}");
            var queueRef = queueClient.GetQueueReference(queueName);
            await queueRef.CreateIfNotExistsAsync();

            _logger.LogDebug("create the new message");
            _logger.LogDebug($"new message = {messageData}");
            var jsonMsg = JsonConvert.SerializeObject(messageData);
            _logger.LogDebug($"msg = {jsonMsg}");
            CloudQueueMessage message = new CloudQueueMessage(jsonMsg);
            _logger.LogDebug("Adding msg to queue");
            await queueRef.AddMessageAsync(message);
        }

        // retrieve global and per-instance tile packs 
        public async Task RefreshLayers()
        {
            // we don't want to do this more than once at the same time so we will need to track that.
            //_logger.LogDebug("Refreshing layers.");

            // stop watching for filesystem changes while we download 
            watcher.EnableRaisingEvents = false;

            // check for map packs first.
            _logger.LogDebug("Starting Refreshing layers");
            // var cloudClient = CloudAccount.CreateCloudBlobClient();
            // var packContainer = CloudClient.GetContainerReference(CurrentOptions.StorageContainer);
            // await packContainer.CreateIfNotExistsAsync();
            // await packContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            BlobContinuationToken continuationToken = null;
            do
            {
                var response = await PackContainer.ListBlobsSegmentedAsync(continuationToken);
                continuationToken = response.ContinuationToken;
                foreach (var item in response.Results)
                    if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        var blob = (CloudBlockBlob)item;
                        var thispackfile = Path.Combine(CurrentOptions.PackPath, blob.Name);

                        if (NeedToExtract(thispackfile, blob.Properties.Length))
                        {
                            _logger.LogDebug($"Downloading {thispackfile}.");
                            using (var fileStream = File.OpenWrite(thispackfile))
                            {
                                await blob.DownloadToStreamAsync(fileStream);
                            }
                        }

                        // extract pack to appropriate places
                        using (var zip = ZipFile.OpenRead(thispackfile))
                        {
                            foreach (var entry in zip.Entries)
                            {
                                var fn = Path.GetFileName(entry.FullName);
                                var ext = Path.GetExtension(fn);
                                if (ext.Equals(".json"))
                                {
                                    var style = Path.Combine(CurrentOptions.StylePath, fn);
                                    if (NeedToExtract(style, entry.Length))
                                    {
                                        _logger.LogDebug($"Extracting {style}.");
                                        entry.ExtractToFile(style, true);
                                    }
                                }

                                if (ext.Equals(".mbtiles"))
                                {
                                    var tile = Path.Combine(CurrentOptions.TilePath, fn);
                                    if (NeedToExtract(tile, entry.Length))
                                    {
                                        _logger.LogDebug($"Extracting {tile}.");
                                        entry.ExtractToFile(tile, true);
                                        OpenService(tile);
                                    }
                                }
                            }
                        }
                    }
            } while (continuationToken != null);

            // see if there's any new standalone map layers
            _logger.LogDebug("Checking for standalone layers");
            // var tileContainer = CloudClient.GetContainerReference(CurrentOptions.MbTilesContainer);
            // await tileContainer.CreateIfNotExistsAsync();
            // await tileContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            do
            {
                var response = await TileContainer.ListBlobsSegmentedAsync(continuationToken);
                continuationToken = response.ContinuationToken;
                foreach (var item in response.Results)
                    if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        var blob = (CloudBlockBlob)item;
                        var thismbtile = Path.Combine(CurrentOptions.TilePath, blob.Name);

                        if (NeedToExtract(thismbtile, blob.Properties.Length))
                        {
                            _logger.LogDebug($"Downloading {thismbtile}.");
                            try
                            {
                                using (var fileStream = File.OpenWrite(thismbtile))
                                {
                                    _logger.LogDebug($"about to download");
                                    await blob.DownloadToStreamAsync(fileStream);
                                    _logger.LogDebug("finished download");
                                }
                                _logger.LogDebug($"about to open {thismbtile}");
                                OpenService(thismbtile);
                                _logger.LogDebug($"opened {thismbtile}");
                            }
                            catch (Exception e)
                            {
                                _logger.LogError($"An exception occurred while downloading {e}");
                            }
                        }
                    }
            } while (continuationToken != null);
            // reenable watching for filesystem changes 
            watcher.EnableRaisingEvents = true;

            _logger.LogDebug("Ending Refreshing layers");

        }

        /// Close a MapBox tile service so that it can be changed.
        /// <param name="name">The name of the service to close</param>
        public void CloseService(string name)
        {
            // don't try to close a non-existent service
            if (!LayerDict.ContainsKey(name)) return;
            LayerDict[name].Conn.Close();
            LayerDict.Remove(name);
        }

        /// Open a MapBox tile service.
        /// <param name="mbtilefile">The file containing the service to open</param>
        public void OpenService(string mbtilefile)
        {
            // don't try to open the file if it's locked, or if it doesn't pass basic mbtile file requirements
            if (!isFileLocked(mbtilefile) && IsValidMbTile(mbtilefile))
            {
                var name = Path.GetFileNameWithoutExtension(mbtilefile);
                if (name == null || LayerDict.ContainsKey(name)) return;

                try
                {
                    var layer = new Layer(CurrentOptions, name);
                    LayerDict.Add(layer.Name, layer);

                }
                catch (Exception e)
                {
                    throw new Exception("Could not create layer", e);
                }
            }
        }

        // returns true if the file doesn't exist, or if it does exist 
        private static bool NeedToExtract(string filepath, long expectedLength)
        {
            var result = true;
            // don't retrieve pack if we already have it (TODO: check should probably be more than just size)
            if (File.Exists(filepath))
            {
                var fi = new FileInfo(filepath);
                if (fi.Length == expectedLength) result = false;
            }

            return result;
        }

        // retrieve font set and populate font directory.
        private async void PopulateFonts()
        {
            var globalClient = CloudAccount.CreateCloudBlobClient();
            var fontContainer = globalClient.GetContainerReference("fonts");
            await fontContainer.CreateIfNotExistsAsync();
            await fontContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            BlobContinuationToken continuationToken = null;
            do
            {
                var response = await fontContainer.ListBlobsSegmentedAsync(continuationToken);
                continuationToken = response.ContinuationToken;
                foreach (var item in response.Results)
                    if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        var blob = (CloudBlockBlob)item;
                        var fontPackFile = Path.Combine(CurrentOptions.RootDir, blob.Name);

                        if (NeedToExtract(fontPackFile, blob.Properties.Length))
                        {
                            _logger.LogDebug($"Downloading {fontPackFile}.");
                            using (var fileStream = File.OpenWrite(fontPackFile))
                            {
                                await blob.DownloadToStreamAsync(fileStream);
                            }
                        }

                        // extract pack to appropriate places
                        using (var zip = ZipFile.OpenRead(fontPackFile))
                        {
                            foreach (var entry in zip.Entries)
                                if (entry.Length != 0)
                                {
                                    var fontFile = Path.Combine(CurrentOptions.FontPath, entry.FullName);
                                    if (NeedToExtract(fontFile, entry.Length)) entry.ExtractToFile(fontFile, true);
                                }
                                else
                                {
                                    var dir = Path.Combine(CurrentOptions.FontPath, entry.FullName);
                                    CreateDirectory(dir);
                                }
                        }
                    }
            } while (continuationToken != null);
        }

        private void CheckDirectories()
        {
            if (!Exists(CurrentOptions.RootDir)) CreateDirectory(CurrentOptions.RootDir);
            if (!Exists(CurrentOptions.TilePath)) CreateDirectory(CurrentOptions.TilePath);
            if (!Exists(CurrentOptions.PackPath)) CreateDirectory(CurrentOptions.PackPath);
            if (!Exists(CurrentOptions.StylePath)) CreateDirectory(CurrentOptions.StylePath);
            if (!Exists(CurrentOptions.FontPath)) CreateDirectory(CurrentOptions.FontPath);
        }

        /// <summary>
        ///     Open all MapBox tile databases found in the Tile directory and store the open connections in the
        ///     SqLiteConnections dictionary. Also create a watcher to monitor for additions to that directory.
        /// </summary>
        private void OpenTiles()
        {
            LayerDict = new Dictionary<string, ILayer>();
            var mbtiles = GetFiles(CurrentOptions.TilePath, "*.mbtiles");

            foreach (var file in mbtiles)
            {
                OpenService(file);
            }

            //var watcher = new FileSystemWatcher();
            watcher.Path = CurrentOptions.TilePath;
            watcher.Filter = "*.mbtiles";
            watcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.CreationTime |
                               NotifyFilters.DirectoryName | NotifyFilters.LastWrite;
            watcher.Created += Watcher_Created;
            watcher.EnableRaisingEvents = true;

        }

        // function to check if a file is openable. We use this below.
        private bool isFileLocked(string filepath)
        {
            FileStream stream = null;
            try
            {
                stream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                stream?.Close();
            }
            return false;
        }

        // For the moment we validate a MBTile file by the presence of the metadata and tiles tables.
        // We could possibly check for the presence of entries in both but it is probably valid to have (at least) an empty tiles table?
        private bool IsValidMbTile(string filepath)
        {
            try
            {
                var builder = new SqliteConnectionStringBuilder
                {
                    Mode = SqliteOpenMode.ReadOnly,
                    Cache = SqliteCacheMode.Shared,
                    DataSource = filepath
                };
                var connStr = builder.ConnectionString;
                var count = 0;
                using (var dbConnection = new SqliteConnection(connStr))
                {
                    dbConnection.Open();
                    using (var cmd = dbConnection.CreateCommand())
                    {
                        cmd.CommandText = "select count(*) from sqlite_master where type='table' and name in ('metadata','tiles')";
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            count = Convert.ToInt32(result);
                        }
                    }
                    dbConnection.Close();
                    if (count == 2) return true;
                }
            }
            catch
            {
                _logger.LogWarning($"{filepath} failed mbtile sanity check.");
            }
            return false;
        }
        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            _logger.LogDebug($"found file at {e.FullPath}");
            OpenService(e.FullPath);
        }
    }
}