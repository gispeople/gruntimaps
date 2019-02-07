﻿/*

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
using System;
using GruntiMaps.Api.DataContracts.V2.Enums;
using GruntiMaps.Api.DataContracts.V2.Styles;
using Newtonsoft.Json.Linq;

namespace GruntiMaps.WebAPI.Models
{
    public static class DefaultLayerStyles
    {
        // this set of colours has been around for about 50 years and was designed
        // to be distinctive.
        private static readonly string[] KellyColors = {
            "#222222", "#F3C300", "#875692", "#F38400", "#A1CAF1",
            "#BE0032", "#C2B280", "#848482", "#008856", "#E68FAC", "#0067A5",
            "#F99379", "#604E97", "#F6A600", "#B3446C", "#DCD300", "#882D17",
            "#8DB600", "#654522", "#E25822", "#2B3D26", "#F2F3F4" };

        private static readonly JObject MetaData = new JObject {{"autogenerated", true}};

        public static StyleDto Circle(string id, string layerName)
            => new StyleDto
            {
                Id = $"{id}-circle",
                Name = $"{layerName}-circle",
                MetaData = MetaData,
                Source = id,
                SourceLayer = layerName,
                Type = PaintType.Circle,
                Paint = new JObject
                {
                    { "circle-stroke-color", "white" },
                    {
                        "circle-color",
                        KellyColors[Math.Abs(layerName.GetHashCode()) % KellyColors.Length]
                    },
                    { "circle-stroke-width", 1 }
                }
            };

        public static StyleDto Line(string id, string layerName)
            => new StyleDto
            {
                Id = $"{id}-line",
                Name = $"{layerName}-line",
                MetaData = MetaData,
                Source = id,
                SourceLayer = layerName,
                Type = PaintType.Line,
                Paint = new JObject
                {
                    {
                        "line-color",
                        KellyColors[Math.Abs(layerName.GetHashCode()) % KellyColors.Length]
                    },
                    { "line-width", 2 }
                }
            };

        public static StyleDto Fill(string id, string layerName)
            => new StyleDto
            {
                Id = $"{id}-fill",
                Name = $"{layerName}-fill",
                MetaData = MetaData,
                Source = id,
                SourceLayer = layerName,
                Type = PaintType.Fill,
                Paint = new JObject
                {
                    {
                        "fill-color",
                        KellyColors[Math.Abs(layerName.GetHashCode()) % KellyColors.Length]
                    },
                    { "fill-outline-color", "white" },
                    { "fill-opacity", 0.2 }
                }
            };
    }
}
