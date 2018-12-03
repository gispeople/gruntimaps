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
