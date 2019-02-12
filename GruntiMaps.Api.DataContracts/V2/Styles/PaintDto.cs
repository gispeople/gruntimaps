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
        public RelativeAlignment? LineRelativeAnchor { get; set; }

        [JsonProperty("line-width")]
        [Range(0, double.MaxValue)]
        public double? LineWidth { get; set; }

        [JsonProperty("line-gap-width")]
        [Range(0, double.MaxValue)]
        public double? LineGapWidth { get; set; }

        [JsonProperty("line-offset")]
        public double? LineOffset { get; set; }

        [JsonProperty("line-blur")]
        [Range(0, double.MaxValue)]
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
        public RelativeAlignment? FillRelativeAnchor { get; set; }

        [JsonProperty("fill-pattern")]
        public string FillPattern { get; set; }

        // Symbol
        [JsonProperty("icon-opacity")]
        [Range(0, 1)]
        public double? IconOpacity { get; set; }

        [JsonProperty("icon-color")]
        public string IconColor { get; set; }

        [JsonProperty("icon-halo-color")]
        public string IconHaloColor { get; set; }

        [JsonProperty("icon-halo-width")]
        [Range(0, double.MaxValue)]
        public double? IconHaloWidth { get; set; }

        [JsonProperty("icon-halo-blur")]
        [Range(0, double.MaxValue)]
        public double? IconHaloBlur { get; set; }

        [JsonProperty("icon-translate")]
        public double[] IconTranslate { get; set; }

        [JsonProperty("icon-translate-anchor")]
        public RelativeAlignment? IconTranslateAnchor { get; set; }

        [JsonProperty("text-opacity")]
        [Range(0, 1)]
        public double? TextOpacity { get; set; }

        [JsonProperty("text-color")]
        public string TextColor { get; set; }

        [JsonProperty("text-halo-color")]
        public string TextHaloColor { get; set; }

        [JsonProperty("text-halo-width")]
        [Range(0, double.MaxValue)]
        public double? TextHaloWidth { get; set; }

        [JsonProperty("text-halo-blur")]
        [Range(0, double.MaxValue)]
        public double? TextHaloBlur { get; set; }

        [JsonProperty("text-translate")]
        public double[] TextTranslate { get; set; }

        [JsonProperty("text-translate-anchor")]
        public RelativeAlignment? TextTranslateAnchor { get; set; }

        // raster
        [JsonProperty("raster-opacity")]
        [Range(0, 1)]
        public double? RasterOpacity { get; set; }

        [JsonProperty("raster-hue-rotate")]
        public double? RasterHueRotate { get; set; }

        [JsonProperty("raster-brightness-min")]
        [Range(0, 1)]
        public double? RasterBrightnessMin { get; set; }

        [JsonProperty("raster-brightness-max")]
        [Range(0, 1)]
        public double? RasterBrightnessMax { get; set; }

        [JsonProperty("raster-saturation")]
        [Range(-1, 1)]
        public double? RasterSaturation { get; set; }

        [JsonProperty("raster-contrast")]
        [Range(-1, 1)]
        public double? RasterContrast { get; set; }

        [JsonProperty("raster-resampling")]
        public Resampling? RasterResampling { get; set; }

        [JsonProperty("raster-fade-duration")]
        [Range(0, double.MaxValue)]
        public double? RasterFadeDuration { get; set; }

        // Circle
        [JsonProperty("circle-radius")]
        [Range(0, double.MaxValue)]
        public double? CircleRadius { get; set; }

        [JsonProperty("circle-color")]
        public string CircleColor { get; set; }

        [JsonProperty("circle-blur")]
        [Range(0, double.MaxValue)]
        public double? CircleBlur { get; set; }

        [JsonProperty("circle-opacity")]
        [Range(0, 1)]
        public double? CircleOpacity { get; set; }

        [JsonProperty("circle-translate")]
        public double[] CircleTranslate { get; set; }

        [JsonProperty("circle-translate-anchor")]
        public RelativeAlignment? CircleTranslateAnchor { get; set; }

        [JsonProperty("circle-pitch-scale")]
        public RelativeAlignment? CirclePitchScale { get; set; }

        [JsonProperty("circle-pitch-alignment")]
        public RelativeAlignment? CirclePitchAlignment { get; set; }

        [JsonProperty("circle-stroke-width")]
        [Range(0, double.MaxValue)]
        public double? CircleStrokeWidth { get; set; }

        [JsonProperty("circle-stroke-color")]
        public string CircleStrokeColor { get; set; }

        [JsonProperty("circle-stroke-opacity")]
        [Range(0, 1)]
        public double? CircleStrokeOpacity { get; set; }

        // fill-extrusion
        [JsonProperty("fill-extrusion-opacity")]
        [Range(0, 1)]
        public double? FillExtrusionOpacity { get; set; }

        [JsonProperty("fill-extrusion-color")]
        public string FillExtrusionColor { get; set; }

        [JsonProperty("fill-extrusion-translate")]
        public double[] FillExtrusionTranslate { get; set; }

        [JsonProperty("fill-extrusion-translate-anchor")]
        public RelativeAlignment? FillExtrusionTranslateAnchor { get; set; }

        [JsonProperty("fill-extrusion-pattern")]
        public string FillExtrusionPattern { get; set; }

        [JsonProperty("fill-extrusion-height")]
        [Range(0, double.MaxValue)]
        public double? FillExtrusionHeight { get; set; }

        [JsonProperty("fill-extrusion-base")]
        [Range(0, double.MaxValue)]
        public double? FillExtrusionBase { get; set; }

        [JsonProperty("fill-extrusion-vertical-gradient")]
        public bool? FillExtrusionVerticalGradient { get; set; }

        // heatmap
        [JsonProperty("heatmap-radius")]
        [Range(1, double.MaxValue)]
        public double? HeatmapRadius { get; set; }

        [JsonProperty("heatmap-weight")]
        [Range(0, double.MaxValue)]
        public double? HeatmapWeight { get; set; }

        /// <summary>
        /// Defaults to ["interpolate",["linear"],["heatmap-density"],0,"rgba(0, 0, 255, 0)",0.1,"royalblue",0.3,"cyan",0.5,"lime",0.7,"yellow",1,"red"].
        /// Defines the color of each pixel based on its density value in a heatmap.Should be an expression that uses["heatmap-density"] as input.
        /// </summary>
        [JsonProperty("heatmap-color")]
        public JObject HeatmapColor { get; set; }

        [JsonProperty("heatmap-opacity")]
        [Range(0, 1)]
        public double? HeatmapOpacity { get; set; }

        // hillshade
        [JsonProperty("hillshade-illumination-direction")]
        [Range(0, 359)]
        public double? HillshadeIlluminationDirection { get; set; }

        [JsonProperty("hillshade-illumination-anchor")]
        public RelativeAlignment? HillshadeIlluminationAnchor { get; set; }

        [JsonProperty("hillshade-exaggeration")]
        [Range(0, 1)]
        public double? HillshadeExaggeration { get; set; }

        [JsonProperty("hillshade-shadow-color")]
        public string HillshadeShadowColor { get; set; }

        [JsonProperty("hillshade-height-color")]
        public string HillshadeHeightColor { get; set; }

        [JsonProperty("hillshade-accent-color")]
        public string HillshadeAccentColor { get; set; }
    }
}
