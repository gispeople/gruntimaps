using GruntiMaps.Api.Common.Resources;
using GruntiMaps.Api.Common.Services;
using GruntiMaps.Api.DataContracts.V2.Layers;
using GruntiMaps.Domain.Common.Exceptions;
using GruntiMaps.WebAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers.Layers.Get
{
    public class GetLayerSourceController : ApiControllerBase
    {
        private readonly IMapData _mapData;
        private readonly IUrlGenerator _urlGenerator;

        public GetLayerSourceController(IMapData mapData,
            IUrlGenerator urlGenerator)
        {
            _mapData = mapData;
            _urlGenerator = urlGenerator;
        }

        [HttpGet("layers/{id}/source", Name = RouteNames.GetLayerSource)]
        public SourceDto GetLayerSource(string id)
        {
            if (!_mapData.LayerDict.ContainsKey(id))
                throw new EntityNotFoundException();
            var src = _mapData.LayerDict[id].Source;
            src.Tiles = new[] { $"{_urlGenerator.BuildUrl(RouteNames.GetLayer, new { id })}/tile/" + "{x}/{y}/{z}" };
            return src;
        }
    }
}
