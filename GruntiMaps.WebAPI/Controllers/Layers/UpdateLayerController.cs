using System.Threading.Tasks;
using GruntiMaps.Api.DataContracts.V2;
using GruntiMaps.Api.DataContracts.V2.Layers;
using GruntiMaps.Common.Enums;
using GruntiMaps.WebAPI.Interfaces;
using GruntiMaps.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using GruntiMaps.Api.Common.Services;
using GruntiMaps.ResourceAccess.Queue;
using GruntiMaps.ResourceAccess.Table;
using Newtonsoft.Json;

namespace GruntiMaps.WebAPI.Controllers.Layers
{
    public class UpdateLayerController : ApiControllerBase
    {
        private readonly IMapData _mapData;
        private readonly IResourceLinksGenerator _resourceLinksGenerator;
        private readonly IGdConversionQueue _gdConversionQueue;
        private readonly IStatusTable _statusTable;

        public UpdateLayerController(IMapData mapData,
            IResourceLinksGenerator resourceLinksGenerator,
            IGdConversionQueue gdConversionQueue,
            IStatusTable statusTable)
        {
            _mapData = mapData;
            _resourceLinksGenerator = resourceLinksGenerator;
            _gdConversionQueue = gdConversionQueue;
            _statusTable = statusTable;
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
            await _gdConversionQueue.AddMessage(JsonConvert.SerializeObject(messageData));
            await _statusTable.UpdateStatus(messageData.LayerId, LayerStatus.Processing);
            return new LayerDto
            {
                Id = id,
                Status = LayerStatus.Processing,
                Links = _resourceLinksGenerator.GenerateResourceLinks(id),
            };
        }
    }
}