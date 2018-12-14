using System;
using GruntiMaps.Api.Common.Resources;
using GruntiMaps.Api.DataContracts.V2;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers.Layers.Get
{
    public class GetLayerGeoJsonController : WorkspaceLayerControllerBase
    {
        public GetLayerGeoJsonController()
        {
        }

        [HttpGet(Resources.GeoJsonSubResource, Name = RouteNames.GetLayerGeoJson)]
        public ActionResult Invoke()
        {
            throw new NotImplementedException();
        }
    }
}
