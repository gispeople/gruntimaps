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
using System.Net;
using GruntiMaps.Api.Common.Resources;
using GruntiMaps.Api.DataContracts.V2;
using GruntiMaps.Domain.Common.Exceptions;
using GruntiMaps.WebAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers.Layers.Get
{
    public class GetLayerGridController : WorkspaceLayerControllerBase
    {
        private readonly IMapData _mapData;

        public GetLayerGridController(IMapData mapData)
        {
            _mapData = mapData;
        }

        [HttpGet(Resources.GridSubResource + "/{x}/{y}/{z}", Name = RouteNames.GetLayerGrid)]
        public ActionResult Invoke(int x, int y, int z)
        {
            y = (1 << z) - y - 1; // convert xyz to tms

            return _mapData.HasLayer(WorkspaceId, LayerId)
                ? new ContentResult
                {
                    StatusCode = (int) HttpStatusCode.OK,
                    Content = _mapData.GetLayer(WorkspaceId, LayerId).Grid(x, y, z),
                    ContentType = "application/json"
                }
                : throw new EntityNotFoundException();
        }
    }
}
