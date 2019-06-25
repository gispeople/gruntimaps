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
using GruntiMaps.ResourceAccess.Table;
using GruntiMaps.ResourceAccess.TopicSubscription;
using GruntiMaps.WebAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers.Layers
{
    [Authorize]
    public class DeleteLayerController : WorkspaceLayerControllerBase
    {
        private readonly IMapData _mapData;
        private readonly IStatusTable _statusTable;
        private readonly IMapLayerUpdateTopicClient _topicClient;

        public DeleteLayerController(IMapData mapData,
            IStatusTable statusTable,
            IMapLayerUpdateTopicClient topicClient)
        {
            _mapData = mapData;
            _statusTable = statusTable;
            _topicClient = topicClient;
        }

        [HttpDelete]
        public async Task<IActionResult> Invoke()
        {
            await _statusTable.RemoveStatus(WorkspaceId, LayerId);
            await _mapData.DeleteLayer(WorkspaceId, LayerId);
            await _topicClient.SendMessage(new MapLayerUpdateData()
            {
                MapLayerId = LayerId,
                WorkspaceId = WorkspaceId,
                Type = MapLayerUpdateType.Delete
            });
            return NoContent();
        }
    }
}
