using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GruntiMaps.Api.DataContracts.V2.Styles
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RelativeAnchor
    {
        [EnumMember(Value = "center")]
        Center,
        [EnumMember(Value = "left")]
        Left,
        [EnumMember(Value = "right")]
        Right,
        [EnumMember(Value = "top")]
        Top,
        [EnumMember(Value = "bottom")]
        Bottom,
        [EnumMember(Value = "top-left")]
        TopLeft,
        [EnumMember(Value = "top-right")]
        TopRight,
        [EnumMember(Value = "bottom-left")]
        BottomLeft,
        [EnumMember(Value = "bottom-right")]
        BottomRight
    }
}
