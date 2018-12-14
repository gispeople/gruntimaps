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
using Microsoft.AspNetCore.Routing;

namespace GruntiMaps.Api.Common.Extensions
{
    public static class RouteDataExtensions
    {
        public const string WorkspaceIdParameter = "workspaceId";
        public const string LayerIdParameter = "layerId";

        public static bool IsWorkspaceRoute(this RouteData data)
        {
            return data.Values.ContainsKey(WorkspaceIdParameter);
        }

        /// <summary>
        /// Gets the Workspace Id for this Route. Should only be used if it's known that this is a Workspace Route.
        /// </summary>
        public static string GetWorkspaceId(this RouteData data)
        {
            return data.Values[WorkspaceIdParameter].ToString();
        }

        /// <summary>
        /// Gets the Workspace Id for this Route or null if it's not a Workspace route.
        /// </summary>
        public static string GetWorkspaceIdOrNull(this RouteData data)
        {
            return data.IsWorkspaceRoute() ? data.Values[WorkspaceIdParameter].ToString() : null;
        }

        
        public static bool IsLayerRoute(this RouteData data)
        {
            return data.Values.ContainsKey(LayerIdParameter);
        }

        /// <summary>
        /// Gets the Layer Id for this Route. Should only be used if it's known that this is a Layer Route.
        /// </summary>
        public static string GetLayerId(this RouteData data)
        {
            return data.Values[LayerIdParameter].ToString();
        }

        /// <summary>
        /// Gets the Layer Id for this Route or null if it's not a Layer route.
        /// </summary>
        public static string GetLayerIdOrNull(this RouteData data)
        {
            return data.IsLayerRoute() ? data.Values[LayerIdParameter].ToString() : null;
        }
    }
}
