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
using GruntiMaps.Api.Common.Configuration;
using Microsoft.Extensions.Options;

namespace GruntiMaps.ResourceAccess.WorkspaceCache
{
    public class WorkspaceStyleCache : WorkspaceCache, IWorkspaceStyleCache
    {
        private const string DefaultExtension = "json";
        public WorkspaceStyleCache(IOptions<PathOptions> pathOptions)
            : base(pathOptions.Value.Styles, DefaultExtension)
        {
        }
    }
}
