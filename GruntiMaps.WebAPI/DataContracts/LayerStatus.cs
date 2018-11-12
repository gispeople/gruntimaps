using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GruntiMaps.WebAPI.DataContracts
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LayerStatus
    {
        Processing,
        Finished,
        Failed
    }
}
