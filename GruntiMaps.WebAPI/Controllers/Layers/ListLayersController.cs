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
using System.Linq;
using GruntiMaps.Api.Common.Services;
using GruntiMaps.Api.DataContracts.V2.Layers;
using GruntiMaps.Common.Enums;
using GruntiMaps.WebAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers.Layers
{
    [Authorize]
    public class ListLayersController : ApiControllerBase
    {
        private readonly IMapData _mapData;
        private readonly IResourceLinksGenerator _resourceLinksGenerator;

        public ListLayersController(IMapData mapData,
            IResourceLinksGenerator resourceLinksGenerator)
        {
            _mapData = mapData;
            _resourceLinksGenerator = resourceLinksGenerator;
        }

        [HttpGet("layers")]
        public LayerDto[] Invoke()
        {
            return _mapData.AllActiveLayers.Select(layer => new LayerDto()
            {
                Id = layer.Id,
                Name = layer.Name,
                Description = layer.Source.Description,
                Status = LayerStatus.Finished,
                Links = _resourceLinksGenerator.GenerateResourceLinks(layer.Id)
            }).ToArray();
        }
    }
}
