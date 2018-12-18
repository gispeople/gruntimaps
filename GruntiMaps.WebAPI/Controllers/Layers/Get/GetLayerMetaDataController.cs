using GruntiMaps.Api.Common.Resources;
using GruntiMaps.Api.DataContracts.V2;
using GruntiMaps.Domain.Common.Exceptions;
using GruntiMaps.WebAPI.Interfaces;
using GruntiMaps.WebAPI.Utils;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers.Layers.Get
{
    public class GetLayerMetaDataController : WorkspaceLayerControllerBase
    {
        private readonly IMapData _mapData;

        public GetLayerMetaDataController(IMapData mapData)
        {
            _mapData = mapData;
        }

        [HttpGet(Resources.MetaDataSubResource, Name = RouteNames.GetLayerMetaData)]
        public ActionResult Invoke()
        {
            return _mapData.HasLayer(WorkspaceId, LayerId)
                ? Content(JsonUtils.JsonPrettify(_mapData.GetLayer(WorkspaceId, LayerId).DataJson.ToString()), "application/json")
                : throw new EntityNotFoundException();
        }
    }
}
