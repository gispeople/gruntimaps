using System.Threading.Tasks;
using GruntiMaps.Api.DataContracts.V2;
using GruntiMaps.Api.DataContracts.V2.Layers;
using GruntiMaps.Common.Enums;
using GruntiMaps.WebAPI.Interfaces;
using GruntiMaps.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Gruntify.Api.Common.Services;

namespace GruntiMaps.WebAPI.Controllers.Layers
{
    public class UpdateLayerController : ApiControllerBase
    {
        private readonly IMapData _mapData;
        private readonly IResourceLinksGenerator _resourceLinksGenerator;

        public UpdateLayerController(IMapData mapData,
            IResourceLinksGenerator resourceLinksGenerator)
        {
            _mapData = mapData;
            _resourceLinksGenerator = resourceLinksGenerator;
        }

        [HttpPatch(Resources.Layers + "/{id}")]
        public async Task<LayerDto> Invoke(string id, [FromBody] UpdateLayerDto dto)
        {
            ConversionMessageData messageData = new ConversionMessageData
            {
                LayerId = id,
                LayerName = dto.Name,
                DataLocation = dto.DataLocation,
                Description = dto.Description
            };
            await _mapData.CreateGdalConversionRequest(messageData);
            await _mapData.JobStatusTable.UpdateStatus(messageData.LayerId, LayerStatus.Processing);
            return new LayerDto()
            {
                Id = id,
                Status = LayerStatus.Processing,
                Links = _resourceLinksGenerator.GenerateResourceLinks(id),
            };
        }
    }
}