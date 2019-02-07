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
using System.IO;
using System.Threading.Tasks;
using GruntiMaps.Api.DataContracts.V2.Styles;
using GruntiMaps.ResourceAccess.Storage;
using GruntiMaps.ResourceAccess.WorkspaceCache;
using GruntiMaps.WebAPI.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GruntiMaps.WebAPI.Services
{
    public interface ILayerStyleService
    {
        Task Update(string workspaceId, string layerId, StyleDto[] styles);
        Task RefreshAll();
    }

    public class LayerStyleService : ILayerStyleService
    {
        private readonly IMapData _mapData;
        private readonly ILogger<LayerStyleService> _logger;
        private readonly IStyleStorage _styleStorage;
        private readonly IWorkspaceStyleCache _styleCache;

        public LayerStyleService(IMapData mapData,
            ILogger<LayerStyleService> logger,
            IStyleStorage styleStorage,
            IWorkspaceStyleCache styleCache)
        {
            _styleStorage = styleStorage;
            _styleCache = styleCache;
            _mapData = mapData;
            _logger = logger;
        }

        public async Task Update(string workspaceId, string layerId, StyleDto[] styles)
        {
            if (styles != null && styles.Length > 0)
            {
                foreach (var style in styles)
                {
                    var tempId = (new Guid()).ToString();
                    style.Id = tempId;
                    style.Name = $"{tempId}-{style.Type}";
                    style.Source = null;
                    style.SourceLayer = null;
                }

                var json = JsonConvert.SerializeObject(styles);
                var path = _styleCache.GetFilePath(workspaceId, layerId, "json");
                File.WriteAllText(path, json);
                await _styleStorage.Store($"{workspaceId}/{layerId}.json", path);
            }
            else
            {
                // remove style if it's null or empty
                await _styleStorage.DeleteIfExist($"{workspaceId}/{layerId}.json");
                _styleCache.DeleteIfExist(workspaceId, layerId, "json");
            }

            _mapData.GetLayer(workspaceId, layerId)?.ReFetchStyle();
        }

        public async Task RefreshAll()
        {
            var layerStyles = await _styleStorage.List();

            // update new or changed layer
            foreach (var layerStyle in layerStyles)
            {
                if (!GetWorkspaceAndLayerId(layerStyle, out string workspaceId, out string layerId))
                {
                    _logger.LogDebug($"Failed to retrieve correct ids for layer style {layerStyle}");
                    continue;
                }
                if (_styleCache.GetFileMd5(workspaceId, layerId) != await _styleStorage.GetMd5(layerStyle))
                {
                    _logger.LogDebug($"Syncing layer style {layerId} for workspace {workspaceId}");
                    await _styleStorage.UpdateLocalFile(layerStyle, _styleCache.GetFilePath(workspaceId, layerId));
                    _mapData.GetLayer(workspaceId, layerId)?.ReFetchStyle();
                }
            }

            // delete file that no longer exists remotely
            foreach (var workspaceId in _styleCache.ListWorkspaces())
            {
                foreach (var id in _styleCache.ListFileIds(workspaceId))
                {
                    if (!layerStyles.Contains($"{workspaceId}/{id}.json"))
                    {
                        _logger.LogDebug($"Deleting layer style {id} for workspace {workspaceId}");
                        _styleCache.DeleteIfExist(workspaceId, id);
                        _logger.LogDebug($"Deleted layer style {id} for workspace {workspaceId}");
                    }
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
    }
}
