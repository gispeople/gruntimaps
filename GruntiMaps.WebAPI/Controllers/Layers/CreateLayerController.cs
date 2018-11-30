using System;
using System.Threading.Tasks;
using GruntiMaps.Api.Common.Services;
using GruntiMaps.Api.DataContracts.V2;
using GruntiMaps.Api.DataContracts.V2.Layers;
using GruntiMaps.Common.Enums;
using GruntiMaps.ResourceAccess.Queue;
using GruntiMaps.ResourceAccess.Table;
using GruntiMaps.WebAPI.Interfaces;
using GruntiMaps.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GruntiMaps.WebAPI.Controllers.Layers
{
    public class CreateLayerController : ApiControllerBase
    {
        private readonly IMapData _mapData;
        private readonly IResourceLinksGenerator _resourceLinksGenerator;
        private readonly IGdConversionQueue _gdConversionQueue;
        private readonly IStatusTable _statusTable;

        public CreateLayerController(IMapData mapData,
            IResourceLinksGenerator resourceLinksGenerator,
            IGdConversionQueue gdConversionQueue,
            IStatusTable statusTable)
        {
            _mapData = mapData;
            _resourceLinksGenerator = resourceLinksGenerator;
            _gdConversionQueue = gdConversionQueue;
            _statusTable = statusTable;
        }

        [HttpPost(Resources.Layers)]
        public async Task<LayerDto> Invoke([FromBody] CreateLayerDto dto)
        {
            var id = Guid.NewGuid().ToString();
            ConversionMessageData messageData = new ConversionMessageData
            {
                LayerId = id,
                LayerName = dto.Name,
                DataLocation = dto.DataLocation,
                Description = dto.Description
            };
            await _gdConversionQueue.AddMessage(JsonConvert.SerializeObject(messageData));
            await _statusTable.UpdateStatus(id, LayerStatus.Processing);
            return new LayerDto
            {
                Id = id,
                Status = LayerStatus.Processing,
                Links = _resourceLinksGenerator.GenerateResourceLinks(id),
            };
        }
    }
}