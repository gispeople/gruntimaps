/*

Copyright 2016, 2017, 2018 GIS People Pty Ltd

This file is part of GruntiMaps.

GruntiMaps is free software: you can redistribute it and/or modify it under 
the terms of the GNU Affero General Public License as published by the Free
Software Foundation, either version 3 of the License, or (at your option) any
later version.

GruntiMaps is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR
A PARTICULAR PURPOSE. See the GNU Affero General Public License for more 
details.

You should have received a copy of the GNU Affero General Public License along
with GruntiMaps.  If not, see <https://www.gnu.org/licenses/>.

*/
using System.Collections.Generic;
using GruntiMaps.Api.Common.Resources;
using GruntiMaps.Api.DataContracts.V2;

namespace GruntiMaps.Api.Common.Services
{
    public interface IResourceLinksGenerator
    {
        IList<LinkDto> GenerateResourceLinks(string workspaceId, string layerId);
    }

    public class ResourceLinksGenerator : IResourceLinksGenerator
    {
        private readonly IUrlGenerator _urlGenerator;

        public ResourceLinksGenerator(IUrlGenerator urlGenerator)
        {
            _urlGenerator = urlGenerator;
        }

        public IList<LinkDto> GenerateResourceLinks(string workspaceId, string layerId) {
            var links = new List<LinkDto>();

            var baseUrl = _urlGenerator.BuildUrl(RouteNames.GetLayer, new { workspaceId, layerId });

            links.Add(new LinkDto(LinkRelations.Self, baseUrl));
            links.Add(new LinkDto(LinkRelations.Source, _urlGenerator.BuildUrl(RouteNames.GetLayerSource, new { workspaceId, layerId })));
            links.Add(new LinkDto(LinkRelations.Style, _urlGenerator.BuildUrl(RouteNames.GetLayerStyle, new { workspaceId, layerId })));
            links.Add(new LinkDto(LinkRelations.Status, _urlGenerator.BuildUrl(RouteNames.GetLayerStatus, new { workspaceId, layerId })));
            links.Add(new LinkDto(LinkRelations.Mappack, _urlGenerator.BuildUrl(RouteNames.GetLayerMapPack, new { workspaceId, layerId })));
            links.Add(new LinkDto(LinkRelations.Metadata, _urlGenerator.BuildUrl(RouteNames.GetLayerMetaData, new { workspaceId, layerId })));
            links.Add(new LinkDto(LinkRelations.Geojson, _urlGenerator.BuildUrl(RouteNames.GetLayerGeoJson, new { workspaceId, layerId })));
            links.Add(new LinkDto(LinkRelations.Tile, $"{baseUrl}/{DataContracts.V2.Resources.TileSubResource}/" + "{x}/{y}/{z}"));
            links.Add(new LinkDto(LinkRelations.Grid, $"{baseUrl}/{DataContracts.V2.Resources.GridSubResource}/" + "{x}/{y}/{z}"));

            return links;
        }
    }
}
