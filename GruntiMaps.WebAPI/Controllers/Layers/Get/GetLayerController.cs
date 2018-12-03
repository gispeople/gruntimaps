using System.Threading.Tasks;
using GruntiMaps.Api.Common.Resources;
using GruntiMaps.Api.Common.Services;
using GruntiMaps.Api.DataContracts.V2;
using GruntiMaps.Api.DataContracts.V2.Layers;
using GruntiMaps.Common.Enums;
using GruntiMaps.Domain.Common.Exceptions;
using GruntiMaps.ResourceAccess.Table;
using GruntiMaps.WebAPI.Interfaces;
using GruntiMaps.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers.Layers.Get
{
    public class GetLayerController : ApiControllerBase
    {
        private readonly IMapData _mapData;
        private readonly IStatusTable _statusTable;
        private readonly IResourceLinksGenerator _resourceLinksGenerator;

        public GetLayerController(IMapData mapData,
            IStatusTable statusTable,
            IResourceLinksGenerator resourceLinksGenerator)
        {
            _mapData = mapData;
            _statusTable = statusTable;
            _resourceLinksGenerator = resourceLinksGenerator;
        }

        [HttpGet(Resources.Layers + "/{id}", Name = RouteNames.GetLayer)]
        public async Task<LayerDto> Invoke(string id)
        {
            var status = await _statusTable.GetStatus(id);
            if (!_mapData.LayerDict.ContainsKey(id))
            {
                if (status.HasValue)
                {
                    return new LayerDto()
                    {
                        Id = id,
                        Status = status.Value
                    };
                }
                else
                {
                    throw new EntityNotFoundException();
                }
            }

            var layer = (Layer)_mapData.LayerDict[id];

            return new LayerDto()
            {
                Id = layer.Id,
                Name = layer.Name,
                Description = layer.Source.Description,
                Status = status ?? LayerStatus.Finished,
                Links = _resourceLinksGenerator.GenerateResourceLinks(id)
            };
        }

    }
}
