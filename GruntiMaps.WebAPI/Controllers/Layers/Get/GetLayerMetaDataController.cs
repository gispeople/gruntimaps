﻿/*

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
using GruntiMaps.WebAPI.Utils;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers.Layers.Get
{
    public class GetLayerMetaDataController : WorkspaceLayerControllerBase
    {
        private readonly IMapData _mapData;

        public GetLayerMetaDataController(IMapData mapData)
        {
            _mapData = mapData;
        }

        [HttpGet(Resources.MetaDataSubResource, Name = RouteNames.GetLayerMetaData)]
        public ActionResult Invoke()
        {
            return _mapData.HasLayer(WorkspaceId, LayerId)
                ? Content(JsonUtils.JsonPrettify(_mapData.GetLayer(WorkspaceId, LayerId).DataJson.ToString()), "application/json")
                : throw new EntityNotFoundException();
        }
    }
}
