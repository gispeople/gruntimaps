using System.Net;
using GruntiMaps.Api.Common.Resources;
using GruntiMaps.Domain.Common.Exceptions;
using GruntiMaps.WebAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers.Layers.Get
{
    public class GetLayerGridController : ApiControllerBase
    {
        private readonly IMapData _mapData;

        public GetLayerGridController(IMapData mapData)
        {
            _mapData = mapData;
        }

        [HttpGet("layers/{id}/grid/{x}/{y}/{z}", Name = RouteNames.GetLayerGrid)]
        public ActionResult Invoke(string id, int x, int y, int z)
        {
            y = (1 << z) - y - 1; // convert xyz to tms

            return _mapData.LayerDict.ContainsKey(id)
                ? new ContentResult
                {
                    StatusCode = (int) HttpStatusCode.OK,
                    Content = _mapData.LayerDict[id].Grid(x, y, z),
                    ContentType = "application/json"
                }
                : throw new EntityNotFoundException();
        }
    }
}
