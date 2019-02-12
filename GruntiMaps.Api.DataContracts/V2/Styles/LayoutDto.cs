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
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class LayoutDto
    {
        // General
        [JsonProperty("visibility")]
        public Visibility? Visibility { get; set; }

        // line
        [JsonProperty("line-cap")]
        public string LineCap { get; set; }

        [JsonProperty("line-join")]
        public string LineJoin { get; set; }

        [JsonProperty("line-miter-limit")]
        public double? LineMiterLimit { get; set; }

        [JsonProperty("line-round-limit")]
        public double? LineRoundLimit { get; set; }

        // Symbol
        [JsonProperty("symbol-placement")]
        public string SymbolPlacement { get; set; }

        [JsonProperty("symbol-spacing")]
        [Range(1, double.MaxValue)]
        public double? SymbolSpacing { get; set; }

        [JsonProperty("symbol-sort-key")]
        [Range(1, double.MaxValue)]
        public double? SymbolSortKey { get; set; }

        [JsonProperty("symbol-avoid-edges")]
        public bool? SymbolAvoidEdges { get; set; }

        [JsonProperty("symbol-z-order")]
        public string SymbolZOrder { get; set; }

        [JsonProperty("icon-allow-overlap")]
        public bool? IconAllowOverlap { get; set; }

        [JsonProperty("icon-optional")]
        public bool? IconOptional { get; set; }

        [JsonProperty("icon-rotation-alignment")]
        public RelativeAlignmentWithAuto? IconRotationAlignment { get; set; }

        [JsonProperty("icon-size")]
        [Range(0, double.MaxValue)]
        public double? IconSize { get; set; }

        [JsonProperty("icon-text-fit")]
        public string IconTextFit { get; set; }

        [JsonProperty("icon-text-fit-padding")]
        public double[] IconTextFitPadding { get; set; }

        [JsonProperty("icon-image")]
        public string IconImage { get; set; }

        [JsonProperty("icon-padding")]
        [Range(0, double.MaxValue)]
        public double? IconPadding { get; set; }

        [JsonProperty("icon-keep-upright")]
        public bool? IconKeepUpright { get; set; }

        [JsonProperty("icon-offset")]
        public double[] IconOffset { get; set; }

        [JsonProperty("icon-anchor")]
        public RelativeAnchor? IconAnchor { get; set; }

        [JsonProperty("icon-pitch-alignment")]
        public RelativeAlignmentWithAuto? IconPitchAlignment { get; set; }

        [JsonProperty("text-pitch-alignment")]
        public RelativeAlignmentWithAuto? TextPitchAlignment { get; set; }

        [JsonProperty("text-rotation-alignment")]
        public RelativeAlignmentWithAuto? TextRotationAlignment { get; set; }

        [JsonProperty("text-field")]
        public JObject TextField { get; set; }

        [JsonProperty("text-font")]
        public string[] TextFont { get; set; }

        [JsonProperty("text-size")]
        [Range(0, double.MaxValue)]
        public double? TextSize { get; set; }

        [JsonProperty("text-max-width")]
        [Range(0, double.MaxValue)]
        public double? TextMaxWidth { get; set; }

        [JsonProperty("text-line-height")]
        public double? TextLineHeight { get; set; }

        [JsonProperty("text-letter-spacing")]
        public double? TextLetterSpacing { get; set; }

        [JsonProperty("text-justify")]
        public string TextJustify { get; set; }

        [JsonProperty("text-anchor")]
        public RelativeAlignmentWithAuto? TextAnchor { get; set; }

        [JsonProperty("text-max-angle")]
        public double? TextMaxAngle { get; set; }

        [JsonProperty("text-rotate")]
        public double? TextRotate { get; set; }

        [JsonProperty("text-padding")]
        public double? TextPadding { get; set; }

        [JsonProperty("text-keep-upright")]
        public bool? TextKeepUpright { get; set; }

        [JsonProperty("text-transform")]
        public string TextTransform { get; set; }

        [JsonProperty("text-offset")]
        public double[] TextOffset { get; set; }

        [JsonProperty("text-allow-overlap")]
        public bool? TextAllowOverlap { get; set; }

        [JsonProperty("text-ignore-placement")]
        public bool? TextIgnorePlacement { get; set; }

        [JsonProperty("text-optional")]
        public bool? TextOptional { get; set; }
    }
}
