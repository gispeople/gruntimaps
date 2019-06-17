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
using GruntiMaps.Api.DataContracts.V2;
using GruntiMaps.Domain.Common.Exceptions;
using GruntiMaps.WebAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GruntiMaps.WebAPI.Controllers.Layers.Get
{
    public class GetLayerStyleController : WorkspaceLayerControllerBase
    {
        private readonly IMapData _mapData;

        public GetLayerStyleController(IMapData mapData)
        {
            _mapData = mapData;
        }

        [HttpGet(Resources.StyleSubResource, Name = RouteNames.GetLayerStyle)]
        public ActionResult Invoke()
        {
            var styles =  _mapData.HasLayer(WorkspaceId, LayerId)
                ? _mapData.GetLayer(WorkspaceId, LayerId).Styles
                : throw new EntityNotFoundException();

            return new JsonResult(styles, new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }
    }
}
