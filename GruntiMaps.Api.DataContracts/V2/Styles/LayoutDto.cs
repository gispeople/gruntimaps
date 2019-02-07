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
    public class LayoutDto
    {
        // General
        [JsonProperty("visibility")]
        public bool? Visibility { get; set; }

        // Symbol
        [JsonProperty("symbol-placement")]
        public string SymbolPlacement { get; set; }

        [JsonProperty("symbol-spacing")]
        [Range(1, 65535)]
        public double? SymbolSpacing { get; set; }

        [JsonProperty("symbol-avoid-edges")]
        public bool? SymbolAvoidEdges { get; set; }

        [JsonProperty("symbol-z-order")]
        public string SymbolZOrder { get; set; }

        [JsonProperty("icon-allow-overlap")]
        public bool? IconAllowOverlap { get; set; }

        [JsonProperty("icon-optional")]
        public bool? IconOptional { get; set; }

        [JsonProperty("icon-rotation-alignment")]
        public string IconRotationAlignment { get; set; }

        [JsonProperty("icon-size")]
        [Range(0, 65535)]
        public double? IconSize { get; set; }

        [JsonProperty("icon-text-fit")]
        public string IconTextFit { get; set; }

        [JsonProperty("icon-text-fit-padding")]
        public double[] IconTextFitPadding { get; set; }

        [JsonProperty("icon-image")]
        public string IconImage { get; set; }

        [JsonProperty("icon-padding")]
        [Range(0, 65535)]
        public double? IconPadding { get; set; }

        [JsonProperty("icon-keep-upright")]
        public bool? IconKeepUpright { get; set; }

        [JsonProperty("icon-offset")]
        public double[] IconOffset { get; set; }

        [JsonProperty("icon-anchor")]
        public string IconAnchor { get; set; }

        [JsonProperty("icon-pitch-alignment")]
        public string IconPitchAlignment { get; set; }

        [JsonProperty("text-pitch-alignment")]
        public string TextPitchAlignment { get; set; }

        [JsonProperty("text-rotation-alignment")]
        public string TextRotationAlignment { get; set; }

        [JsonProperty("text-field")]
        public JObject TextField { get; set; }
        // still many

    }
}
