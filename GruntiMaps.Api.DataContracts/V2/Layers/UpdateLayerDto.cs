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
using System.ComponentModel.DataAnnotations;
using GruntiMaps.Api.DataContracts.V2.Styles;

namespace GruntiMaps.Api.DataContracts.V2.Layers
{
    public class UpdateLayerDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }

        public string DataLocation { get; set; }

        public StyleDto[] Styles { get; set; }
    }
}
