using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GruntiMaps.Api.DataContracts.V2.Styles
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Resampling
    {
        [EnumMember(Value = "linear")]
        Linear,
        [EnumMember(Value = "nearest")]
        Nearest,
    }
}
