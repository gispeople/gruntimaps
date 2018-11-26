using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GruntiMaps.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LayerStatus
    {
        Processing,
        Finished,
        Failed
    }
}
