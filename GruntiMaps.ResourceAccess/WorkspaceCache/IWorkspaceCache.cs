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
    public interface IWorkspaceCache
    {
        /// <summary>
        /// Get file path for specific workspace and layer.
        /// The directory is created unless it already exists.
        /// </summary>
        /// <param name="workspaceId">workspace id</param>
        /// <param name="id">file id</param>
        /// <param name="extension">optional (e.g. "txt"), will take default extension if null</param>
        /// <returns>file path</returns>
        string GetFilePath(string workspaceId, string id, string extension = null);

        /// <summary>
        /// Get MD5 hash of file for specific workspace and layer.
        /// </summary>
        /// <param name="workspaceId">workspace id</param>
        /// <param name="id">file id</param>
        /// <param name="extension">optional (e.g. "txt"), will take default extension if null</param>
        /// <returns>MD5 of file or null if file doesn't exist</returns>
        string GetFileMd5(string workspaceId, string id, string extension = null);

        /// <summary>
        /// Delete the file for specific workspace and layer if it exists.
        /// </summary>
        /// <param name="workspaceId">workspace id</param>
        /// <param name="id">file id</param>
        /// <param name="extension">optional (e.g. "txt"), will take default extension if null</param>
        /// <returns>true/false for file's existense before deletion</returns>
        bool DeleteIfExist(string workspaceId, string id, string extension = null);

        /// <summary>
        /// Check if file exists for specific workspace and layer
        /// </summary>
        /// <param name="workspaceId">workspace id</param>
        /// <param name="id">file id</param>
        /// <param name="extension">optional (e.g. "txt"), will take default extension if null</param>
        /// <returns>true/false for file's existense</returns>
        bool FileExists(string workspaceId, string id, string extension = null);

        /// <summary>
        /// Check if file is available for specific workspace and layer
        /// </summary>
        /// <param name="workspaceId">workspace id</param>
        /// <param name="id">file id</param>
        /// <param name="extension">optional (e.g. "txt"), will take default extension if null</param>
        /// <returns>true/false for file's availability</returns>
        bool FileIsAvailable(string workspaceId, string id, string extension = null);

        string[] ListFilePaths(string workspaceId);
        string[] ListFileIds(string workspaceId);
        string[] ListWorkspaces();
    }
}
