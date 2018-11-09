using System;
using System.Threading.Tasks;
using GruntiMaps.WebAPI.DataContracts;
using GruntiMaps.WebAPI.Interfaces;
using GruntiMaps.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers.Layers
{
    public class CreateLayerController : ApiControllerBase
    {
        private readonly IMapData _mapData;

        public CreateLayerController(IMapData mapData)
        {
            _mapData = mapData;
        }

        [HttpPost(Resources.Layers)]
        public async Task<LayerCreationDto> Invoke([FromBody] CreateLayerDto dto)
        {
            ConversionMessageData messageData = new ConversionMessageData
            {
                LayerId = Guid.NewGuid().ToString(),
                LayerName = dto.Name,
                DataLocation = dto.DataLocation,
                Description = dto.Description
            };
            await _mapData.CreateGdalConversionRequest(messageData);
            await _mapData.JobStatusTable.UpdateStatus(messageData.LayerId, LayerStatus.Processing);
            return new LayerCreationDto()
            {
                LayerId = messageData.LayerId,
                RequestId = ""
            };
        }
    }
}
