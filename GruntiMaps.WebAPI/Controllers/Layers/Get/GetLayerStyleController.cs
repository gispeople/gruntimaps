using GruntiMaps.Api.Common.Resources;
using GruntiMaps.Api.DataContracts.V2.Layers;
using GruntiMaps.Domain.Common.Exceptions;
using GruntiMaps.WebAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers.Layers.Get
{
    public class GetLayerStyleController : ApiControllerBase
    {
        private readonly IMapData _mapData;

        public GetLayerStyleController(IMapData mapData)
        {
            _mapData = mapData;
        }

        [HttpGet("layers/{id}/style", Name = RouteNames.GetLayerStyle)]
        public StyleDto[] Invoke(string id)
        {
            return _mapData.LayerDict.ContainsKey(id)
                ? _mapData.LayerDict[id].Styles
                : throw new EntityNotFoundException();
        }
    }
}
