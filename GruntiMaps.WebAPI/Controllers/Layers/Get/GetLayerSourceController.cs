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
using GruntiMaps.Api.Common.Services;
using GruntiMaps.Api.DataContracts.V2.Layers;
using GruntiMaps.Domain.Common.Exceptions;
using GruntiMaps.WebAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers.Layers.Get
{
    public class GetLayerSourceController : ApiControllerBase
    {
        private readonly IMapData _mapData;
        private readonly IUrlGenerator _urlGenerator;

        public GetLayerSourceController(IMapData mapData,
            IUrlGenerator urlGenerator)
        {
            _mapData = mapData;
            _urlGenerator = urlGenerator;
        }

        [HttpGet("layers/{id}/source", Name = RouteNames.GetLayerSource)]
        public SourceDto Invoke(string id)
        {
            if (!_mapData.HasLayer(id))
                throw new EntityNotFoundException();
            var src = _mapData.GetLayer(id).Source;
            src.Tiles = new[] { $"{_urlGenerator.BuildUrl(RouteNames.GetLayer, new { id })}/tile/" + "{x}/{y}/{z}" };
            return src;
        }
    }
}
