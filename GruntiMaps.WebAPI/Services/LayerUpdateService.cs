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

using System.Threading.Tasks;
using GruntiMaps.Api.Common.Configuration;
using GruntiMaps.WebAPI.Interfaces;
using Microsoft.Extensions.Options;

namespace GruntiMaps.WebAPI.Services
{
    /// <inheritdoc />
    /// <summary>
    ///     Service to poll for changes to available map layers and update the available list.
    /// </summary>
    public class LayerUpdateService : BackgroundService
    {
        private readonly IMapData _mapdata;
        private readonly ServiceOptions _serviceOptions;

        public LayerUpdateService(IOptions<ServiceOptions> serviceOptions, IMapData mapdata)
        {
            _mapdata = mapdata;
            _serviceOptions = serviceOptions.Value;
        }

        protected override async Task Process()
        {
            //                _logger.LogDebug("LayerUpdate task doing background work.");

            //var start = DateTime.UtcNow;
            await _mapdata.RefreshLayers();
            //var end = DateTime.UtcNow;
            //var duration = end - start;
            //                _logger.LogDebug($"Layer refresh took {duration.TotalMilliseconds} ms.");

            await Task.Delay(_serviceOptions.LayerRefresh);
        }
    }
}