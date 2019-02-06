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

namespace GruntiMaps.Api.DataContracts.V2.Styles
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class PaintDto
    {
        // Background
        [JsonProperty("background-color")]
        public string BackgroundColor { get; set; }

        [JsonProperty("background-pattern")]
        public string BackgroundPattern { get; set; }

        [JsonProperty("background-opacity")]
        [Range(0, 1)]
        public double? BackgroundOpacity { get; set; }

        // Line
        [JsonProperty("line-opacity")]
        [Range(0, 1)]
        public double? LineOpacity { get; set; }

        [JsonProperty("line-color")]
        public string LineColor { get; set; }

        [JsonProperty("line-translate")]
        public double[] LineTranslate { get; set; }

        [JsonProperty("line-translate-anchor")]
        public RelativeAnchor? LineRelativeAnchor { get; set; }

        [JsonProperty("line-width")]
        [Range(0, 65535)]
        public double? LineWidth { get; set; }

        [JsonProperty("line-gap-width")]
        [Range(0, 65535)]
        public double? LineGapWidth { get; set; }

        [JsonProperty("line-offset")]
        public double? LineOffset { get; set; }

        [JsonProperty("line-blur")]
        [Range(0, 65535)]
        public double? LineBlur { get; set; }

        [JsonProperty("line-dasharray")]
        public double[] LineDasharray { get; set; }

        [JsonProperty("line-pattern")]
        public string LinePattern { get; set; }

        [JsonProperty("line-gradient")]
        public string LineGradient { get; set; }

        // Fill
        [JsonProperty("fill-antialias")]
        public bool? FillAntialias { get; set; }

        [JsonProperty("fill-opacity")]
        [Range(0, 1)]
        public double? FillOpacity { get; set; }

        [JsonProperty("fill-color")]
        public string FillColor { get; set; }

        [JsonProperty("fill-outline-color")]
        public string FillOutlineColor { get; set; }

        [JsonProperty("fill-translate")]
        public double[] FillTranslate { get; set; }

        [JsonProperty("fill-translate-anchor")]
        public RelativeAnchor? FillRelativeAnchor { get; set; }

        [JsonProperty("fill-pattern")]
        public string FillPattern { get; set; }

        // Circle
        [JsonProperty("circle-radius")]
        [Range(0, 65535)]
        public double? CircleRadius { get; set; }

        [JsonProperty("circle-color")]
        public string CircleColor { get; set; }

        [JsonProperty("circle-blur")]
        [Range(0, 65535)]
        public double? CircleBlur { get; set; }

        [JsonProperty("circle-opacity")]
        [Range(0, 1)]
        public double? CircleOpacity { get; set; }

        [JsonProperty("circle-translate")]
        public double[] CircleTranslate { get; set; }

        [JsonProperty("circle-translate-anchor")]
        public RelativeAnchor? CircleRelativeAnchor { get; set; }

        [JsonProperty("circle-pitch-scale")]
        public RelativeAnchor? CirclePitchScale { get; set; }

        [JsonProperty("circle-pitch-alignment")]
        public RelativeAnchor? CirclePitchAlignment { get; set; }

        [JsonProperty("circle-stroke-width")]
        [Range(0, 65535)]
        public double? CircleStrokeWidth { get; set; }

        [JsonProperty("circle-stroke-color")]
        public string CircleStrokeColor { get; set; }

        [JsonProperty("circle-stroke-opacity")]
        [Range(0, 1)]
        public double? CircleStrokeOpacity { get; set; }
    }
}
