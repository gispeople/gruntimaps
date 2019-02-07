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
using GruntiMaps.Api.DataContracts.V2.Layers;
using GruntiMaps.Common.Enums;
using GruntiMaps.Domain.Common.Validation;
using Microsoft.AspNetCore.Mvc;
using GruntiMaps.ResourceAccess.Queue;
using GruntiMaps.ResourceAccess.Table;
using GruntiMaps.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;

namespace GruntiMaps.WebAPI.Controllers.Layers
{
    [Authorize]
    public class UpdateLayerController : WorkspaceLayerControllerBase
    {
        private readonly IGdConversionQueue _gdConversionQueue;
        private readonly IStatusTable _statusTable;
        private readonly ILayerStyleService _layerStyleService;
        private readonly IValidator<UpdateLayerDto> _validator;

        public UpdateLayerController(
            IGdConversionQueue gdConversionQueue,
            IStatusTable statusTable,
            ILayerStyleService layerStyleService,
            IValidator<UpdateLayerDto> validator)
        {
            _gdConversionQueue = gdConversionQueue;
            _statusTable = statusTable;
            _layerStyleService = layerStyleService;
            _validator = validator;
        }

        [HttpPatch]
        public async Task<ActionResult> Invoke([FromBody] UpdateLayerDto dto)
        {
            await _validator.Validate(dto);

            var status = await _statusTable.GetStatus(WorkspaceId, LayerId);
            if (!status.HasValue)
            {
                return NotFound();
            }

            if (dto.DataLocation != null)
            {
                // in other cases we will have to create a new conversion job
                var job = new ConversionJobData
                {
                    LayerId = LayerId,
                    WorkspaceId = WorkspaceId,
                    LayerName = dto.Name,
                    DataLocation = dto.DataLocation,
                    Description = dto.Description
                };
                await _gdConversionQueue.Queue(job);
                await _statusTable.UpdateStatus(WorkspaceId, LayerId, LayerStatus.Processing);
            }

            if (dto.Styles != null)
            {
                await _layerStyleService.Update(WorkspaceId, LayerId, dto.Styles);
            }

            return Accepted(new LayerStatusDto
            {
                Id = LayerId,
                Status = LayerStatus.Processing
            });
        }
    }
}