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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GruntiMaps.ResourceAccess.TopicSubscription;
using GruntiMaps.WebAPI.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GruntiMaps.WebAPI.Services
{
    /// <inheritdoc />
    /// <summary>
    ///     Service to poll for changes to available map layers and update the available list.
    /// </summary>
    public class LayerCacheRefreshService : IHostedService
    {
        private readonly IMapData _mapData;
        private readonly IMapLayerUpdateSubscriptionClient _subscriptionClient;
        private readonly ILogger<LayerCacheRefreshService> _logger;

        public LayerCacheRefreshService(IMapData mapData,
            IMapLayerUpdateSubscriptionClient subscriptionClient,
            ILogger<LayerCacheRefreshService> logger)
        {
            _mapData = mapData;
            _subscriptionClient = subscriptionClient;
            _logger = logger;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Preforming full layer refresh upon initialization");
                var timer = new Stopwatch();
                timer.Start();
                await _mapData.RefreshLayers();
                timer.Stop();
                _logger.LogDebug($"Full layer refresh finished in {timer.ElapsedMilliseconds} ms");
            }
            catch (Exception ex)
            {
                _logger.LogError("Refreshing Layer exits unexpectedly", ex);
            }

            _subscriptionClient.RegisterOnMessageHandlerAndReceiveMessages(UpdateMapLayer);

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(5000, cancellationToken);
            }
        }

        private async Task UpdateMapLayer(MapLayerUpdateData data)
        {
            switch (data.Type)
            {
                case MapLayerUpdateType.Create:
                case MapLayerUpdateType.Update:
                    await _mapData.UpdateLayer(data.WorkspaceId, data.MapLayerId);
                    break;
                case MapLayerUpdateType.Delete:
                    await _mapData.DeleteLayer(data.WorkspaceId, data.MapLayerId);
                    break;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}