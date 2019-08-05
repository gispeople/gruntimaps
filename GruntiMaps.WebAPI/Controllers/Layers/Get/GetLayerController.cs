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
using GruntiMaps.Api.Common.Resources;
using GruntiMaps.Api.Common.Services;
using GruntiMaps.Api.DataContracts.V2.Layers;
using GruntiMaps.Domain.Common.Exceptions;
using GruntiMaps.ResourceAccess.Storage;
using GruntiMaps.ResourceAccess.Table;
using GruntiMaps.WebAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GruntiMaps.WebAPI.Controllers.Layers.Get
{
    public class GetLayerController : WorkspaceLayerControllerBase
    {
        private readonly IMapData _mapData;
        private readonly IResourceLinksGenerator _resourceLinksGenerator;
        private readonly ITileStorage _tileStorage;
        private readonly IStyleStorage _styleStorage;

        public GetLayerController(IMapData mapData,
            IResourceLinksGenerator resourceLinksGenerator, 
            ITileStorage tileStorage, 
            IStyleStorage styleStorage)
        {
            _mapData = mapData;
            _resourceLinksGenerator = resourceLinksGenerator;
            _tileStorage = tileStorage;
            _styleStorage = styleStorage;
        }

        [AllowAnonymous]
        [HttpGet(Name = RouteNames.GetLayer)]
        public async Task<ActionResult> Invoke()
        {
            var layer = _mapData.HasLayer(WorkspaceId, LayerId) 
                ? _mapData.GetLayer(WorkspaceId, LayerId) 
                : throw new EntityNotFoundException();

            return new JsonResult(new LayerDto
            {
                Id = layer.Id,
                Name = layer.Name,
                Description = layer.Source.Description,
                MbTileMd5 = await _tileStorage.GetMd5($"{WorkspaceId}/{LayerId}.mbtiles"),
                CustomStyleMd5 = await _styleStorage.GetMd5($"{WorkspaceId}/{LayerId}.json"),
                Links = _resourceLinksGenerator.GenerateResourceLinks(WorkspaceId, LayerId)
            }, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

    }
}
