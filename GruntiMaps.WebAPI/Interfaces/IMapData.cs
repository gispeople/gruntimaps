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
using GruntiMaps.WebAPI.Models;

namespace GruntiMaps.WebAPI.Interfaces
{
    public interface IMapData
    {
        /// <summary>
        /// Get the ILayer object
        /// </summary>
        /// <param name="id">layer id</param>
        /// <returns>layer</returns>
        ILayer GetLayer(string id);

        /// <summary>
        /// Get all active layers
        /// </summary>
        /// <returns>all active layers as array</returns>
        ILayer[] AllActiveLayers { get; }

        /// <summary>
        /// Check existance of a layer
        /// </summary>
        /// <param name="id">layer id</param>
        /// <returns>the existance of the layer</returns>
        bool HasLayer(string id);

        /// <summary>
        /// Refresh active layer to sync with remote storage
        /// </summary>
        /// <returns></returns>
        Task RefreshLayers();

        /// <summary>
        /// Delete an active layer
        /// </summary>
        /// <param name="id">layer id</param>
        /// <returns></returns>
        Task DeleteLayer(string id);
    }
}