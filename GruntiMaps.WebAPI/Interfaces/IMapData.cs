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

using System.Threading.Tasks;

namespace GruntiMaps.WebAPI.Interfaces
{
    public interface IMapData
    {
        /// <summary>
        /// Get the ILayer object
        /// </summary>
        /// <param name="workspaceId">workspace id</param>
        /// <param name="id">layer id</param>
        /// <returns>layer</returns>
        ILayer GetLayer(string workspaceId, string id);

        /// <summary>
        /// Get all active layers within a workspace
        /// </summary>
        /// <param name="workspaceId">workspace id</param>
        /// <returns>active layers as array</returns>
        ILayer[] GetAllActiveLayers(string workspaceId);

        /// <summary>
        /// Check existance of a layer
        /// </summary>
        /// <param name="workspaceId">workspace id</param>
        /// <param name="id">layer id</param>
        /// <returns>the existance of the layer</returns>
        bool HasLayer(string workspaceId, string id);

        /// <summary>
        /// Upload local layer to hosted storage to publish changes
        /// </summary>
        /// <param name="workspaceId">workspace id</param>
        /// <param name="id">layer id</param>
        void UploadLocalLayer(string workspaceId, string id);

        /// <summary>
        /// Update/Create layer if there's update in remote storage
        /// </summary>
        /// <param name="workspaceId">workspace id</param>
        /// <param name="layerId">layer id</param>
        /// <returns></returns>
        Task UpdateLayer(string workspaceId, string layerId);

        /// <summary>
        /// Refresh active layer to sync with remote storage
        /// </summary>
        /// <returns></returns>
        Task RefreshLayers();

        /// <summary>
        /// Delete an active layer
        /// </summary>
        /// <param name="workspaceId">workspace id</param>
        /// <param name="layerId">layer id</param>
        /// <returns></returns>
        Task DeleteLayer(string workspaceId, string layerId);
    }
}