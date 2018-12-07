using System;
using GruntiMaps.Api.Common.Resources;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers.Layers.Get
{
    public class GetLayerGeoJsonController : ApiControllerBase
    {
        public GetLayerGeoJsonController()
        {
        }

        [HttpGet("layers/{id}/geojson", Name = RouteNames.GetLayerGeoJson)]
        public ActionResult GetLayerGeoJson(string id)
        {
            throw new NotImplementedException();
        }
    }
}
