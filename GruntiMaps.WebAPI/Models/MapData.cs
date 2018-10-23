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
using GruntiMaps.WebAPI.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GruntiMaps.WebAPI.Models
{
    /// <summary>
    ///     MapData provides methods and properties related to the map store's location and contents.
    /// </summary>
    public class MapData : IMapData
    {
        private readonly ILogger<MapData> _logger;
        // we need to be able to see the watcher so we can disable it while downloading layers
        private FileSystemWatcher watcher = new FileSystemWatcher();
        public IStorageContainer PackContainer;
        public IStorageContainer TileContainer;
        public IStorageContainer GeojsonContainer { get; }
        public IStorageContainer FontContainer { get; }
        public IQueue MbConversionQueue { get; }
        public IQueue GdConversionQueue { get; }
        public IStatusTable JobStatusTable { get; }
        public Options CurrentOptions { get; }

        public Dictionary<string, ILayer> LayerDict { get; set; } = new Dictionary<string, ILayer>();

        public MapData(Options options, ILogger<MapData> logger)
        {
            CurrentOptions = options;
            _logger = logger;
            _logger.LogDebug($"Creating MapData root={CurrentOptions.RootDir}");
            switch (options.StorageProvider)
            {
                case StorageProviders.Azure: 
                    PackContainer = new AzureStorage(CurrentOptions, CurrentOptions.StorageContainer);
                    TileContainer = new AzureStorage(CurrentOptions, CurrentOptions.MbTilesContainer);
                    GeojsonContainer = new AzureStorage(CurrentOptions, CurrentOptions.GeoJsonContainer);
                    FontContainer = new AzureStorage(CurrentOptions, CurrentOptions.FontContainer);
                    MbConversionQueue = new AzureQueue(CurrentOptions, CurrentOptions.MbConvQueue);
                    GdConversionQueue = new AzureQueue(CurrentOptions, CurrentOptions.GdConvQueue);
                    JobStatusTable = new AzureStatusTable(CurrentOptions, CurrentOptions.JobStatusTable);
                    break;
                case StorageProviders.Local:
                    PackContainer = new LocalStorage(CurrentOptions, CurrentOptions.StorageContainer);
                    TileContainer = new LocalStorage(CurrentOptions, CurrentOptions.MbTilesContainer);
                    GeojsonContainer = new LocalStorage(CurrentOptions, CurrentOptions.GeoJsonContainer);
                    FontContainer = new LocalStorage(CurrentOptions, CurrentOptions.FontContainer);
                    MbConversionQueue = new LocalQueue(CurrentOptions, CurrentOptions.MbConvQueue);
                    GdConversionQueue = new LocalQueue(CurrentOptions, CurrentOptions.GdConvQueue);
                    JobStatusTable = new LocalStatusTable(CurrentOptions, CurrentOptions.JobStatusTable);
                    break;
                default:
                    _logger.LogCritical("No valid storage provider set.");
                    break;
            }

            CheckDirectories();
            PopulateFonts();
            OpenTiles();
        }

        public async Task<string> CreateGdalConversionRequest(ConversionMessageData messageData)
        {
            return await GdConversionQueue.AddMessage(JsonConvert.SerializeObject(messageData));
        }

        public async Task<string> CreateMbConversionRequest(ConversionMessageData messageData)
        {
            return await MbConversionQueue.AddMessage(JsonConvert.SerializeObject(messageData));
        }

        // retrieve global and per-instance tile packs 
        public async Task RefreshLayers()
        {
            // we don't want to do this more than once at the same time so we will need to track that.

            // stop watching for filesystem changes while we download 
            watcher.EnableRaisingEvents = false;

            // check for map packs first.
            _logger.LogDebug("Starting Refreshing layers");

            var packs = await PackContainer.List();
            foreach (var pack in packs)
            {
                var thispackfile = Path.Combine(CurrentOptions.PackPath, pack);
                if (await PackContainer.GetIfNewer(pack, thispackfile))
                {
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
            }

            // see if there's any new standalone map layers
            _logger.LogDebug("Checking for standalone layers");

            var mbtiles = await TileContainer.List();
            foreach (var mbtile in mbtiles)
            {
                var thismbtile = Path.Combine(CurrentOptions.TilePath, mbtile);
                if (await PackContainer.GetIfNewer(mbtile, thismbtile))
                {
                    OpenService(thismbtile);
                }
            }

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
            if (!IsFileLocked(mbtilefile) && IsValidMbTile(mbtilefile))
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
            var fontPacks = await FontContainer.List();
            foreach (var fontPack in fontPacks)
            {
                var thisFontPack = Path.Combine(CurrentOptions.FontPath, fontPack);
                if (await FontContainer.GetIfNewer(fontPack, thisFontPack))
                {
                    using (var zip = ZipFile.OpenRead(thisFontPack))
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
                                Directory.CreateDirectory(dir);
                            }
                    }

                }
            }

        }

        private void CheckDirectories()
        {
            if (!Directory.Exists(CurrentOptions.RootDir)) Directory.CreateDirectory(CurrentOptions.RootDir);
            if (!Directory.Exists(CurrentOptions.TilePath)) Directory.CreateDirectory(CurrentOptions.TilePath);
            if (!Directory.Exists(CurrentOptions.PackPath)) Directory.CreateDirectory(CurrentOptions.PackPath);
            if (!Directory.Exists(CurrentOptions.StylePath)) Directory.CreateDirectory(CurrentOptions.StylePath);
            if (!Directory.Exists(CurrentOptions.FontPath)) Directory.CreateDirectory(CurrentOptions.FontPath);
        }

        /// <summary>
        ///     Open all MapBox tile databases found in the Tile directory and store the open connections in the
        ///     SqLiteConnections dictionary. Also create a watcher to monitor for additions to that directory.
        /// </summary>
        private void OpenTiles()
        {
            LayerDict = new Dictionary<string, ILayer>();
            var mbtiles = Directory.GetFiles(CurrentOptions.TilePath, "*.mbtiles");

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
        private bool IsFileLocked(string filepath)
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