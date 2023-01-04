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
using GruntiMaps.Api.Common.Resources;
using GruntiMaps.Api.DataContracts.V2;
using GruntiMaps.Domain.Common.Exceptions;
using GruntiMaps.ResourceAccess.Storage;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers.Layers.Get
{
    public class GetLayerGeoJsonController : WorkspaceLayerControllerBase
    {
        private readonly IGeoJsonStorage _geoJsonStorage;
        public GetLayerGeoJsonController(IGeoJsonStorage geoJsonStorage)
        {
            _geoJsonStorage = geoJsonStorage;
        }

        [HttpGet(Resources.GeoJsonSubResource, Name = RouteNames.GetLayerGeoJson)]
        public async Task<ActionResult> Invoke(string workspaceId, string layerId)
        {
            var file = $"{WorkspaceId}/{LayerId}.geojson";
            if (await _geoJsonStorage.Exist(file))
            {
                return Redirect(await _geoJsonStorage.GetDownloadUrl(file));
            }
            throw new EntityNotFoundException();
        }
    }
}
