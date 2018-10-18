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
            
            _tileJson = GetTileConfig();
        }

        private TileConfig GetTileConfig()
        {
            TileConfig result = new TileConfig();
            try
            {
                using (var metaCmd = _conn.CreateCommand())
                {
                    metaCmd.CommandText = "select name, value from metadata";
                    using (var rdr = metaCmd.ExecuteReader())
                    {
                        while (rdr.Read())
                            switch (rdr.GetString(0))
                            {
                                case "bounds":
                                    result.bounds = new double[4];
                                    var x = rdr.GetString(1).Split(',');

                                    for (var i = 0; i < 4; i++) result.bounds[i] = Convert.ToDouble(x[i]);

                                    break;
                                case "center":
                                    var cen = rdr.GetString(1).Split(',');
                                    result.center = new double[3];
                                    result.center[0] = Convert.ToDouble(cen[0]);
                                    result.center[1] = Convert.ToDouble(cen[1]);
                                    result.center[2] = Convert.ToInt16(cen[2]);

                                    break;
                                case "maxzoom":
                                    result.maxzoom = Convert.ToInt16(rdr.GetString(1));
                                    break;
                                case "minzoom":
                                    result.minzoom = Convert.ToInt16(rdr.GetString(1));
                                    break;
                                case "name":
                                    //name = rdr.GetString(1);
                                    result.name = _sourceId; // the internal source name is not always as expected.
                                    break;
                                case "description":
                                    result.description = rdr.GetString(1);
                                    break;
                                case "version":
                                    result.version = rdr.GetString(1);
                                    break;
                                case "attribution":
                                    result.attribution = rdr.GetString(1);
                                    break;
                                case "format":
                                    result.format = rdr.GetString(1);
                                    break;
                            }
                    }
                }
            }
            catch (SqliteException e)
            {
                throw new Exception("Problem with metadata", e);
            }
            result.tilejson = "2.0.0";
            result.scheme = "xyz";
            result.type = "vector";
            result.tiles = new string[1];
            result.tiles[0] = $"#publicHost#/v2/sources/{_sourceId}/tiles?x={{x}}&y={{y}}&z={{z}}";        
            return result;
        }
    

        private SqliteConnection GetConnection()
        {
            var connStr = "";
            try
            {
                // we need a sourceId
                if (_sourceId == null) return null;
                Path = System.IO.Path.Combine(_options.TileDir, $"{_sourceId}.mbtiles");
                // we need to have a mbtiles file
                if (!File.Exists(Path)) return null;
                var builder = new SqliteConnectionStringBuilder
                {
                    Mode = SqliteOpenMode.ReadOnly,
                    Cache = SqliteCacheMode.Shared,
                    DataSource = Path
                };
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