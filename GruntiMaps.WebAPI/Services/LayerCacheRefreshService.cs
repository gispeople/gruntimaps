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
using System.Threading.Tasks;
using GruntiMaps.Api.Common.Configuration;
using GruntiMaps.WebAPI.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GruntiMaps.WebAPI.Services
{
    /// <inheritdoc />
    /// <summary>
    ///     Service to poll for changes to available map layers and update the available list.
    /// </summary>
    public class LayerCacheRefreshService : BackgroundService
    {
        private readonly IMapData _mapData;
        private readonly ServiceOptions _serviceOptions;
        private readonly ILayerStyleService _layerStyleService;
        private readonly ILogger<LayerCacheRefreshService> _logger;

        public LayerCacheRefreshService(IOptions<ServiceOptions> serviceOptions, 
            IMapData mapData,
            ILayerStyleService layerStyleService,
            ILogger<LayerCacheRefreshService> logger)
        {
            _mapData = mapData;
            _serviceOptions = serviceOptions.Value;
            _layerStyleService = layerStyleService;
            _logger = logger;
        }

        protected override async Task Process()
        {
            //                _logger.LogDebug("LayerUpdate task doing background work.");

            //var start = DateTime.UtcNow;
            try
            {
                await _mapData.RefreshLayers();
            }
            catch (Exception ex)
            {
                _logger.LogError("Refreshing Layer exits unexpectedly", ex);
            }
            
            //var end = DateTime.UtcNow;
            //var duration = end - start;
            //                _logger.LogDebug($"Layer refresh took {duration.TotalMilliseconds} ms.");

            try
            {
                await _layerStyleService.RefreshAll();
            }
            catch (Exception ex)
            {
                _logger.LogError("Refreshing Layer style exits unexpectedly", ex);
            }

            await Task.Delay(_serviceOptions.LayerRefresh);
        }
    }
}