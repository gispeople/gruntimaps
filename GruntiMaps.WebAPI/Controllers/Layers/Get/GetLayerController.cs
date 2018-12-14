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
using GruntiMaps.ResourceAccess.Table;
using GruntiMaps.WebAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers.Layers.Get
{
    public class GetLayerController : WorkspaceLayerControllerBase
    {
        private readonly IMapData _mapData;
        private readonly IStatusTable _statusTable;
        private readonly IResourceLinksGenerator _resourceLinksGenerator;

        public GetLayerController(IMapData mapData,
            IStatusTable statusTable,
            IResourceLinksGenerator resourceLinksGenerator)
        {
            _mapData = mapData;
            _statusTable = statusTable;
            _resourceLinksGenerator = resourceLinksGenerator;
        }

        [AllowAnonymous]
        [HttpGet(Name = RouteNames.GetLayer)]
        public LayerDto Invoke()
        {
            var layer = _mapData.HasLayer(WorkspaceId, LayerId) 
                ? _mapData.GetLayer(WorkspaceId, LayerId) 
                : throw new EntityNotFoundException();

            return new LayerDto
            {
                Id = layer.Id,
                Name = layer.Name,
                Description = layer.Source.Description,
                Links = _resourceLinksGenerator.GenerateResourceLinks(WorkspaceId, LayerId)
            };
        }

    }
}
