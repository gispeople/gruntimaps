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
using Microsoft.AspNetCore.Mvc;
using GruntiMaps.Api.Common.Services;
using GruntiMaps.ResourceAccess.Queue;
using GruntiMaps.ResourceAccess.Table;
using GruntiMaps.WebAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace GruntiMaps.WebAPI.Controllers.Layers
{
    [Authorize]
    public class UpdateLayerController : WorkspaceLayerControllerBase
    {
        private readonly IResourceLinksGenerator _resourceLinksGenerator;
        private readonly IGdConversionQueue _gdConversionQueue;
        private readonly IStatusTable _statusTable;
        private readonly IMapData _mapData;

        public UpdateLayerController(
            IResourceLinksGenerator resourceLinksGenerator,
            IGdConversionQueue gdConversionQueue,
            IStatusTable statusTable,
            IMapData mapData)
        {
            _resourceLinksGenerator = resourceLinksGenerator;
            _gdConversionQueue = gdConversionQueue;
            _statusTable = statusTable;
            _mapData = mapData;
        }

        [HttpPatch]
        public async Task<ActionResult> Invoke([FromBody] UpdateLayerDto dto)
        {
            var status = await _statusTable.GetStatus(WorkspaceId, LayerId);
            if (!status.HasValue)
            {
                return NotFound();
            }

            if (_mapData.HasLayer(WorkspaceId, LayerId) && string.IsNullOrEmpty(dto.DataLocation) && status == LayerStatus.Finished)
            {
                // Try change it locally and upload if it's already active and no ongoing conversion.
                var layer = _mapData.GetLayer(WorkspaceId, LayerId);
                layer.UpdateNameDescription(dto.Name ?? layer.Source.Name, dto.Description ?? layer.Source.Description);
                _mapData.UploadLocalLayer(WorkspaceId, LayerId);
                return Accepted(new LayerStatusDto
                {
                    Id = LayerId,
                    Status = LayerStatus.Finished
                });
            }

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
            return Accepted(new LayerStatusDto
            {
                Id = LayerId,
                Status = LayerStatus.Processing
            });
        }
    }
}