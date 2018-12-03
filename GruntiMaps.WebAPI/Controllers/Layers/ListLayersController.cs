using System.Linq;
using GruntiMaps.Api.Common.Services;
using GruntiMaps.Api.DataContracts.V2.Layers;
using GruntiMaps.Common.Enums;
using GruntiMaps.WebAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers.Layers
{
    public class ListLayersController : ApiControllerBase
    {
        private readonly IMapData _mapData;
        private readonly IResourceLinksGenerator _resourceLinksGenerator;

        public ListLayersController(IMapData mapData,
            IResourceLinksGenerator resourceLinksGenerator)
        {
            _mapData = mapData;
            _resourceLinksGenerator = resourceLinksGenerator;
        }

        [HttpGet("layers")]
        public LayerDto[] Invoke()
        {
            return _mapData.LayerDict.Values.Select(layer => new LayerDto()
            {
                Id = layer.Id,
                Name = layer.Name,
                Description = layer.Source.Description,
                Status = LayerStatus.Finished,
                Links = _resourceLinksGenerator.GenerateResourceLinks(layer.Id)
            }).ToArray();
        }
    }
}
