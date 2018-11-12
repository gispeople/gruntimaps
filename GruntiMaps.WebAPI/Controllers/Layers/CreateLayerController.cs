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
                Link = GetLayerLink(messageData.LayerId)
            };
        }

        public LinkDto GetLayerLink(string id)
        {
            return new LinkDto(LinkRelations.Self, $"{GetBaseHost()}/api/layers/{id}");
        }

        private string GetBaseHost()
        {
            // if X-Forwarded-Proto or X-Forwarded-Host headers are set, use them to build the self-referencing URLs
            var proto = string.IsNullOrWhiteSpace(Request.Headers["X-Forwarded-Proto"])
                ? Request.Scheme
                : (string)Request.Headers["X-Forwarded-Proto"];
            var host = string.IsNullOrWhiteSpace(Request.Headers["X-Forwarded-Host"])
                ? Request.Host.ToUriComponent()
                : (string)Request.Headers["X-Forwarded-Host"];
            return $"{proto}://{host}";
        }
    }
}
