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

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GruntiMaps.Api.DataContracts.V2.Styles
{
    public class StyleDto
    {
        public string Id { get; set; }

        public string Name { get; set; }

        [JsonProperty("metadata")]
        public JObject MetaData { get; set; }

        public string Source { get; set; }

        [JsonProperty("maxzoom")]
        public int? MaxZoom { get; set; }

        [JsonProperty("minzoom")]
        public int? MinZoom { get; set; }

        [JsonProperty("source-layer")]
        public string SourceLayer { get; set; }

        [Required]
        public StyleType? Type { get; set; }

        public JArray Filter { get; set; }

        public JObject Layout { get; set; }

        public JObject Paint { get; set; }
    }
}
