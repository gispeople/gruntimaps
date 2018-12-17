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
using System.Linq;
using System.Threading.Tasks;
using GruntiMaps.Api.Common.Configuration;
using GruntiMaps.ResourceAccess.Storage;
using GruntiMaps.ResourceAccess.WorkspaceCache;
using GruntiMaps.WebAPI.Interfaces;
using GruntiMaps.WebAPI.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GruntiMaps.WebAPI.Models
{
    /// <summary>
    ///     MapData provides methods and properties related to the map store's location and contents.
    /// </summary>
    public class MapData : IMapData
    {
        private readonly ILogger<MapData> _logger;

        private readonly ITileStorage _tileStorage;
        private readonly IFontStorage _fontStorage;
        private readonly IWorkspaceTileCache _tileCache;
        private readonly IWorkspaceStyleCache _styleCache;
        private readonly Dictionary<string, ILayer> _layerDict;

        private readonly PathOptions _pathOptions;

        public MapData(IOptions<PathOptions> pathOptions, 
            ILogger<MapData> logger,
            ITileStorage tileStorage,
            IFontStorage fontStorage,
            IWorkspaceTileCache tileCache,
            IWorkspaceStyleCache styleCache)
        {
            _layerDict = new Dictionary<string, ILayer>();
            _pathOptions = pathOptions.Value;
            _logger = logger;
            _logger.LogDebug($"Creating MapData root={_pathOptions.Root}");

            _tileStorage = tileStorage;
            _fontStorage = fontStorage;

            _tileCache = tileCache;
            _styleCache = styleCache;

            CheckDirectories();
            PopulateFonts();
            OpenTiles();
        }

        public ILayer GetLayer(string workspaceId, string id)
            => _layerDict[id].WorkspaceId == workspaceId ? _layerDict[id] : null;

        public ILayer[] GetAllActiveLayers(string workspaceId) =>
            _layerDict.Values.Where(layer => layer.WorkspaceId == workspaceId).ToArray();

        public bool HasLayer(string workspaceId, string id)
            => _layerDict.ContainsKey(id) && _layerDict[id].WorkspaceId == workspaceId;

        public void UploadLocalLayer(string workspaceId, string id)
        {
            if (HasLayer(workspaceId, id))
            {
                _tileStorage.Store($"{workspaceId}/{id}.mbtiles", _tileCache.GetFilePath(workspaceId, id));
            }
        }

        // retrieve global and per-instance tile packs 
        public async Task RefreshLayers()
        {
            // check for map packs first.
            _logger.LogDebug("Starting Refreshing layers");

//            var packs = await _packStorage.List();
//            foreach (var pack in packs)
//            {
//                var localFile = Path.Combine(_pathOptions.Packs, pack);
//                var localHash = HashCalculator.GetLocalFileMd5(localFile);
//                if (GetWorkspaceAndLayerId(pack, out string workspaceId, out string layerId))
//
//                if (localHash == await _packStorage.GetMd5(pack))
//                {
//                    continue;
//                }
//                await _packStorage.UpdateLocalFile(pack, localFile);
//                // extract pack to appropriate places
//                using (var zip = ZipFile.OpenRead(localFile))
//                {
//                    foreach (var entry in zip.Entries)
//                    {
//                        var fn = Path.GetFileName(entry.FullName);
//                        var ext = Path.GetExtension(fn);
//                        if (ext.Equals(".json"))
//                        {
//                            var style = Path.Combine(_pathOptions.Styles, fn);
//                            if (NeedToExtract(style, entry.Length))
//                            {
//                                _logger.LogDebug($"Extracting {style}.");
//                                entry.ExtractToFile(style, true);
//                            }
//                        }
//
//                        if (!ext.Equals(".mbtiles")) continue;
//                        var tile = Path.Combine(_pathOptions.Tiles, fn);
//                        if (!NeedToExtract(tile, entry.Length)) continue;
//                        _logger.LogDebug($"Extracting {tile}.");
//                        entry.ExtractToFile(tile, true);
//                        OpenService(tile);
//                    }
//                }
//            }

            // see if there's any new standalone map layers
            _logger.LogDebug("Checking for standalone layers");

            var mbtiles = await _tileStorage.List();
            foreach (var mbtile in mbtiles)
            {
                if (!GetWorkspaceAndLayerId(mbtile, out string workspaceId, out string layerId))
                {
                    _logger.LogDebug($"Failed to retrieve correct ids for mbtile {mbtile}");
                    continue;
                }
                if (_tileCache.GetFileMd5(workspaceId, layerId) != await _tileStorage.GetMd5(mbtile))
                {
                    _logger.LogDebug($"Syncing layer {layerId} for workspace {workspaceId}");
                    CloseService(layerId);
                    await _tileStorage.UpdateLocalFile(mbtile, _tileCache.GetFilePath(workspaceId, layerId));
                    OpenService(workspaceId, layerId);
                }
            }

            // remove deleted layer from local cache
            foreach (var workspace in _tileCache.ListWorkspaces())
            {
                foreach (var id in _tileCache.ListFileIds(workspace))
                {
                    if (!mbtiles.Any(mbtile => mbtile.Contains($"{workspace}/{id}")))
                    {
                        CloseService(id);
                        _tileCache.DeleteIfExist(workspace, id);
                    }
                }
            }

            _logger.LogDebug("Ending Refreshing layers");

        }

        /// <summary>
        /// Completely delete layer from local cache and hosted storage
        /// </summary>
        /// <param name="workspaceId"></param>
        /// <param name="layerId"></param>
        /// <returns></returns>
        public async Task DeleteLayer(string workspaceId, string layerId)
        {
            Task task = _tileStorage.DeleteIfExist($"{workspaceId}/{layerId}.mbtiles");
            CloseService(layerId);
            _tileCache.DeleteIfExist(workspaceId, layerId);
            await task;
        }

        /// Close a MapBox tile service so that it can be changed.
        /// <param name="layerId">The name of the service to close</param>
        public void CloseService(string layerId)
        {
            // don't try to close a non-existent service
            if (!_layerDict.ContainsKey(layerId)) return;
            _layerDict[layerId].Close();
            _layerDict.Remove(layerId);
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// Open a mabox service
        /// </summary>
        /// <param name="workspaceId">workspace id</param>
        /// <param name="layerId">layer id</param>
        public void OpenService(string workspaceId, string layerId)
        {
            if (layerId == null || workspaceId == null)
            {
                return;
            }

            if (_layerDict.ContainsKey(layerId))
            {
                _logger.LogDebug($"Layer creation canceled for workspace {workspaceId} and layer {layerId} since same layer already activated");
                return;
            }

            if (!_tileCache.FileExists(workspaceId, layerId))
            {
                _logger.LogDebug($"Layer creation canceled for workspace {workspaceId} and layer {layerId} since file not exist");
                return;
            }

            if (!_tileCache.FileIsValidMbTile(workspaceId, layerId))
            {
                _logger.LogDebug($"Layer creation canceled for workspace {workspaceId} and layer {layerId} since file is not valid mbtile");
                return;
            }

            try
            {
                _layerDict.Add(layerId, new Layer(workspaceId, layerId, _tileCache, _styleCache));
            }
            catch (Exception e)
            {
                _logger.LogError($"Layer creation failed for workspace {workspaceId} and layer {layerId}", e);
            }
        }

        private void CheckDirectories()
        {
            Directory.CreateDirectory(_pathOptions.Fonts);
        }

        /// <summary>
        /// Open all MapBox tile databases found in the Tile directory
        /// </summary>
        private void OpenTiles()
        {
            foreach (var workspace in _tileCache.ListWorkspaces())
            {
                foreach (var id in _tileCache.ListFileIds(workspace))
                {
                    OpenService(workspace, id);
                }
            }
        }

        private bool GetWorkspaceAndLayerId(string file, out string workspaceId, out string layerId)
        {
            var ids = file.Split('/');
            if (ids.Length != 2)
            {
                workspaceId = null;
                layerId = null;
                return false;
            }
            workspaceId = ids[0];
            layerId = Path.GetFileNameWithoutExtension(ids[1]);
            return true;
        }

        // returns true if the file doesn't exist, or if it does exist 
        private static bool NeedToExtract(string filepath, long expectedLength)
        {
            var result = true;
            // don't retrieve pack if we already have it (TODO: check should probably be more than just size)
            if (!File.Exists(filepath)) return true;
            var fi = new FileInfo(filepath);
            if (fi.Length == expectedLength) result = false;

            return result;
        }

        // retrieve font set and populate font directory.
        private async void PopulateFonts()
        {
            var fontPacks = await _fontStorage.List();
            foreach (var fontPack in fontPacks)
            {
                var localFile = Path.Combine(_pathOptions.Fonts, fontPack);
                var localHash = HashCalculator.GetLocalFileMd5(localFile);
                var remoteHash = await _tileStorage.GetMd5(fontPack);
                if (localHash == remoteHash)
                {
                    continue;
                }

                await _fontStorage.UpdateLocalFile(fontPack, localFile);
                using (var zip = ZipFile.OpenRead(localFile))
                {
                    foreach (var entry in zip.Entries)
                        if (entry.Length != 0)
                        {
                            var fontFile = Path.Combine(_pathOptions.Fonts, entry.FullName);
                            if (NeedToExtract(fontFile, entry.Length)) entry.ExtractToFile(fontFile, true);
                        }
                        else
                        {
                            var dir = Path.Combine(_pathOptions.Fonts, entry.FullName);
                            Directory.CreateDirectory(dir);
                        }
                }
            }
        }
    }
}