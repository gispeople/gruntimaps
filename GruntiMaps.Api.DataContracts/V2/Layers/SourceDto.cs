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
using Newtonsoft.Json;

namespace GruntiMaps.Api.DataContracts.V2.Layers
{
    public class SourceDto
    {
        [JsonProperty("tilejson")]
        public string TileJsonVersion { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Version { get; set; }

        public string Attribution { get; set; }

        public string Template { get; set; }

        public string Legend { get; set; }

        public string Scheme { get; set; }

        public string[] Tiles { get; set; }

        public string[] Grids { get; set; }

        public string[] Data { get; set; }

        [JsonProperty("minzoom")]
        public int MinZoom { get; set; }

        [JsonProperty("maxzoom")]
        public int MaxZoom { get; set; }

        public double[] Bounds { get; set; }

        public double[] Center { get; set; }

        // properties not from TileJson spec from https://github.com/mapbox/tilejson-spec
        public string Type { get; set; }

        public string Format { get; set; }
    }
}
