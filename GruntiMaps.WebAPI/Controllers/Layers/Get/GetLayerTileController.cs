using GruntiMaps.Api.Common.Resources;
using GruntiMaps.Domain.Common.Exceptions;
using GruntiMaps.WebAPI.Interfaces;
using GruntiMaps.WebAPI.Util;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers.Layers.Get
{
    public class GetLayerTileController : ApiControllerBase
    {
        private readonly IMapData _mapData;

        public GetLayerTileController(IMapData mapData)
        {
            _mapData = mapData;
        }

        [HttpGet("layers/{id}/tile/{x}/{y}/{z}", Name = RouteNames.GetLayerTile)]
        public ActionResult Invoke(string id, int x, int y, int z)
        {
            y = (1 << z) - y - 1; // convert xyz to tms
            var bytes = _mapData.LayerDict[id].Tile(x, y, z);
            switch (_mapData.LayerDict[id].Source.Format)
            {
                case "png": return File(bytes, "image/png");
                case "jpg": return File(bytes, "image/jpg");
                case "pbf": return File(Decompressor.Decompress(bytes), "application/vnd.mapbox-vector-tile");
            }
            throw new EntityNotFoundException();
        }
    }
}
