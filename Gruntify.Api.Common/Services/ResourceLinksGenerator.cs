using System.Collections.Generic;
using Gruntify.Api.Common.Resources;
using GruntiMaps.Api.DataContracts.V2;

namespace Gruntify.Api.Common.Services
{
    public interface IResourceLinksGenerator
    {
        IList<LinkDto> GenerateResourceLinks(string id);
    }

    public class ResourceLinksGenerator : IResourceLinksGenerator
    {
        private readonly IUrlGenerator _urlGenerator;

        public ResourceLinksGenerator(IUrlGenerator urlGenerator)
        {
            _urlGenerator = urlGenerator;
        }

        public IList<LinkDto> GenerateResourceLinks(string id) {
            var links = new List<LinkDto>();

            var baseUrl = _urlGenerator.BuildUrl(RouteNames.GetLayer, new {id});

            links.Add(new LinkDto(LinkRelations.Self, baseUrl));
            links.Add(new LinkDto(LinkRelations.Source, _urlGenerator.BuildUrl(RouteNames.GetLayerSource, new { id })));
            links.Add(new LinkDto(LinkRelations.Style, _urlGenerator.BuildUrl(RouteNames.GetLayerStyle, new { id })));
            links.Add(new LinkDto(LinkRelations.Mappack, _urlGenerator.BuildUrl(RouteNames.GetLayerMapPack, new { id })));
            links.Add(new LinkDto(LinkRelations.Metadata, _urlGenerator.BuildUrl(RouteNames.GetLayerMetaData, new { id })));
            links.Add(new LinkDto(LinkRelations.Geojson, _urlGenerator.BuildUrl(RouteNames.GetLayerGeoJson, new { id })));
            links.Add(new LinkDto(LinkRelations.Tile, $"{baseUrl}/tile/" + "{x}/{y}/{z}"));
            links.Add(new LinkDto(LinkRelations.Grid, $"{baseUrl}/grid/" + "{x}/{y}/{z}"));

            return links;
        }
    }
}
