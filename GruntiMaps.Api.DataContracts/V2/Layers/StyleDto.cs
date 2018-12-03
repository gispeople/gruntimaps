using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GruntiMaps.Api.DataContracts.V2.Layers
{
    public class StyleDto
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public JObject MetaData { get; set; }

        public string Source { get; set; }

        [JsonProperty("source-layer")]
        public string SourceLayer { get; set; }

        public string Type { get; set; }

        public JObject Paint { get; set; }
    }
}
