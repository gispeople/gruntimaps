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
namespace GruntiMaps.ResourceAccess.WorkspaceCache
{
    public interface IWorkspaceTileCache : IWorkspaceCache
    {
        /// <summary>
        /// For the moment we validate a MBTile file by the presence of the metadata and tiles tables.
        /// We could possibly check for the presence of entries in both but it is probably valid to have (at least) an empty tiles table?
        /// </summary>
        /// <param name="workspaceId">workspace id</param>
        /// <param name="id">id</param>
        /// <param name="extension">optional (e.g. "txt"), will take default extension if null</param>
        /// <returns>true/false for if file is a valid mbtile</returns>
        bool FileIsValidMbTile(string workspaceId, string id, string extension = null);
    }
}
