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
    public class MapSource
    {
        private TileConfig _tileJson;
        private SqliteConnection _conn;
        private Options _options;
        private string Path;
        private string _sourceId;
        public SqliteConnection Conn {get => _conn; }
        public TileConfig TileJSON {get => _tileJson; }
        public MapSource(Options options, string sourceId) {
            _options = options;
            _sourceId = sourceId;
            // open db connection
            _conn = GetConnection();
            // get metadata and populate tileJson
            _tileJson = new TileConfig {

            };
        }

        private SqliteConnection GetConnection()
        {
            var connStr = "";
            try
            {
                if (_sourceId == null) return null;
                var builder = new SqliteConnectionStringBuilder
                {
                    Mode = SqliteOpenMode.ReadOnly,
                    Cache = SqliteCacheMode.Shared,
                    DataSource = System.IO.Path.Combine(_options.TileDir, $"{_sourceId}.mbtiles")
                };
                Path = builder.DataSource;
                connStr = builder.ConnectionString;
                var dbConnection = new SqliteConnection(connStr);
                dbConnection.Open();
                return dbConnection;
            }
            catch (Exception e)
            {
                throw new Exception(
                    $"Failed to open database for source {_sourceId} (connection string={connStr}) exception={e}", e);
            }
        }
    }
}