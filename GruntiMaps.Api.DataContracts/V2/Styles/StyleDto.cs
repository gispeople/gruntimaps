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
using GruntiMaps.Api.DataContracts.V2.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GruntiMaps.Api.DataContracts.V2.Styles
{
    public class StyleDto
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        public JObject MetaData { get; set; }
        [Required]
        public string Source { get; set; }

        [Required]
        [JsonProperty("source-layer")]
        public string SourceLayer { get; set; }

        [Required]
        public PaintType? Type { get; set; }

        public JObject Layout { get; set; }

        public JObject Paint { get; set; }
    }
}
