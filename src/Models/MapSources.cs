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
using GruntiMaps.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using static System.IO.Directory;

namespace GruntiMaps.Models
{
    public class MapSources
    {
        private Dictionary<string, MapSource> _sources;

        public TileConfig SourceJson(string sourceId) {
            return _sources[sourceId].TileJSON;
        }
        public SqliteConnection Conn(string sourceId) {
            return _sources[sourceId].Conn;
        }
        public bool Exists(string sourceId) {
            return _sources.ContainsKey(sourceId);
        }

        private readonly Options _options;
        ILogger<MapSources> _logger;
        public MapSources(ILogger<MapSources> logger, Options options) {
            _options = options;
            _logger = logger;
            // look for sources and add them to dictionary
        }

    }
}