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
