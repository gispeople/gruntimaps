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
using GruntiMaps.Api.Common.Resources;
using GruntiMaps.Domain.Common.Exceptions;
using GruntiMaps.WebAPI.Interfaces;
using GruntiMaps.WebAPI.Util;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers.Layers.Get
{
    public class GetLayerTileController : ApiControllerBase
    {
        private readonly IMapData _mapData;

        public GetLayerTileController(IMapData mapData)
        {
            _mapData = mapData;
        }

        [HttpGet("layers/{id}/tile/{x}/{y}/{z}", Name = RouteNames.GetLayerTile)]
        public ActionResult Invoke(string id, int x, int y, int z)
        {
            y = (1 << z) - y - 1; // convert xyz to tms
            var bytes = _mapData.GetLayer(id).Tile(x, y, z);
            switch (_mapData.GetLayer(id).Source.Format)
            {
                case "png": return File(bytes, "image/png");
                case "jpg": return File(bytes, "image/jpg");
                case "pbf": return File(Decompressor.Decompress(bytes), "application/vnd.mapbox-vector-tile");
            }
            throw new EntityNotFoundException();
        }
    }
}
