using GruntiMaps.Api.Common.Resources;
using GruntiMaps.WebAPI.Interfaces;
using GruntiMaps.WebAPI.Utils;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers.Layers.Get
{
    public class GetLayerMetaDataController : ApiControllerBase
    {
        private readonly IMapData _mapData;

        public GetLayerMetaDataController(IMapData mapData)
        {
            _mapData = mapData;
        }

        [HttpGet("layers/{id}/metadata", Name = RouteNames.GetLayerMetaData)]
        public ActionResult Invoke(string id)
        {
            if (!_mapData.HasLayer(id))
            {
                return NoContent();
            }
            return Content(JsonUtils.JsonPrettify(_mapData.GetLayer(id).DataJson.ToString()), "application/json");
        }
    }
}
