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

using System.Threading.Tasks;
using GruntiMaps.Api.DataContracts.V2;
using GruntiMaps.Api.DataContracts.V2.Layers;
using GruntiMaps.Api.DataContracts.V2.Styles;
using GruntiMaps.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers.Layers
{
    [Authorize]
    public class UpdateLayerStyleController : WorkspaceLayerControllerBase
    {
        private readonly ILayerStyleService _styleService;

        public UpdateLayerStyleController(ILayerStyleService styleService)
        {
            _styleService = styleService;
        }

        [HttpPut(Resources.StyleSubResource)]
        public async Task<ActionResult> Invoke([FromBody] UpdateLayerStyleDto dto)
        {
            await _styleService.Update(WorkspaceId, LayerId, dto.Styles ?? new StyleDto[] { });

            return NoContent();
        }
    }
}
