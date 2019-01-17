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

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using GruntiMaps.Api.DataContracts.V2.Layers;
using GruntiMaps.ResourceAccess.WorkspaceCache;
using GruntiMaps.WebAPI.Interfaces;
using GruntiMaps.WebAPI.Utils;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GruntiMaps.WebAPI.Models
{
    public class Layer : ILayer
    {
        private readonly SqliteConnection _connection;
        private readonly IWorkspaceTileCache _tileCache;
        private readonly IWorkspaceStyleCache _styleCache;


        private JObject _dataJson;

        public string Id { get; }
        public string WorkspaceId { get; }
        public string Name => Source.Name;
        public SourceDto Source { get; }
        public StyleDto[] Styles { get; }

        public Layer(string workspaceId, string layerId, IWorkspaceTileCache tileCache, IWorkspaceStyleCache styleCache)
        {
            Id = layerId;
            WorkspaceId = workspaceId;
            _tileCache = tileCache;
            _styleCache = styleCache;
            try
            {
                _connection = GetConnection(true);
                Source = PopulateSourceInfo();
                Styles = TryFetchLocalStyleInfo() ?? PopulateStyleInfo();
            }
            catch (Exception e)
            {
                throw new Exception("Could not create layer source object", e);
            }
        }

        private SqliteConnection GetConnection(bool writable = false)
        {
            if (Id == null) return null;
            var builder = new SqliteConnectionStringBuilder
            {
                Mode = writable ? SqliteOpenMode.ReadWrite : SqliteOpenMode.ReadOnly,
                Cache = SqliteCacheMode.Private,
                DataSource = _tileCache.GetFilePath(WorkspaceId, Id)
            };
            var connStr = builder.ConnectionString;
            try
            {
                var dbConnection = new SqliteConnection(connStr);
                dbConnection.Open();
                return dbConnection;
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to open database for service {Id} (connection string={connStr}) exception={e}", e);
            }
        }

        private SourceDto PopulateSourceInfo()
        {
            if (_connection == null)
                throw new Exception($"Failed to popluate source info for {Id} as no connection exist");
            try
            {
                var source = new SourceDto();
                using (var metaCmd = _connection.CreateCommand())
                {
                    metaCmd.CommandText = "select name, value from metadata";
                    using (var rdr = metaCmd.ExecuteReader())
                    {
                        while (rdr.Read())
                            switch (rdr.GetString(0))
                            {
                                case "bounds":
                                    source.Bounds = rdr.GetString(1).Split(',').Select(Convert.ToDouble).ToArray();
                                    break;
                                case "center":
                                    var cen = rdr.GetString(1).Split(',');
                                    source.Center = new double[3];
                                    source.Center[0] = Convert.ToDouble(cen[0]);
                                    source.Center[1] = Convert.ToDouble(cen[1]);
                                    source.Center[2] = Convert.ToInt16(cen[2]);
                                    break;
                                case "maxzoom":
                                    source.MaxZoom = Convert.ToInt16(rdr.GetString(1));
                                    break;
                                case "minzoom":
                                    source.MinZoom = Convert.ToInt16(rdr.GetString(1));
                                    break;
                                case "name":
                                    source.Name = rdr.GetString(1);
                                    break;
                                case "description":
                                    source.Description = rdr.GetString(1);
                                    break;
                                case "version":
                                    source.Version = rdr.GetString(1);
                                    break;
                                case "attribution":
                                    source.Attribution = rdr.GetString(1);
                                    break;
                                case "format":
                                    source.Format = rdr.GetString(1);
                                    break;
                                case "tilejson":
                                    source.TileJsonVersion = rdr.GetString(1);
                                    break;
                                case "template":
                                    source.Template = rdr.GetString(1);
                                    break;
                                case "scheme":
                                    source.Scheme = rdr.GetString(1);
                                    break;
                            }
                    }

                    source.TileJsonVersion = "2.0.0";
                    source.Scheme = source.Scheme ?? "xyz";
                    source.Type = source.Format == "pbf" ? "vector" : "raster";
                    source.Attribution = string.IsNullOrWhiteSpace(source.Attribution) ?
                        "Served on GruntiMaps" : source.Attribution;
                    // tiles url to be generated when user calls
                }
                return source;
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to popluate source info for {Id}", e);
            }
        }

        private StyleDto[] TryFetchLocalStyleInfo()
        {
            try
            {
                if (_styleCache.FileExists(WorkspaceId, Id))
                {
                    using (var f = new StreamReader(_styleCache.GetFilePath(WorkspaceId, Id)))
                    {
                        var styleStr = f.ReadToEnd();
                        var styles = JsonConvert.DeserializeObject<StyleDto[]>(styleStr);
                        return styles;
                    }
                }
            }
            catch
            {
                // ignored
            }
            return null;
        }

        private StyleDto[] PopulateStyleInfo()
        {
            var styles = new List<StyleDto>();

            JObject data = GetDataJson();
            JObject tilestats = (JObject)data["tilestats"];

            if (tilestats != null && (int) tilestats["layerCount"] > 0)
            {
                // has to have layers represented in tilestats to base our style on
                JArray layers = (JArray)tilestats["layers"];
                foreach (var layer in layers)
                {
                    var layerName = (string)layer["layer"];

                    switch ((string)layer["geometry"])
                    {
                        case "Point":
                            if (!styles.Exists(style => style.Type == "circle"))
                            {
                                styles.Add(DefaultLayerStyles.Circle(Id, layerName));
                            }
                            break;
                        case "LineString":
                            if (!styles.Exists(style => style.Type == "line"))
                            {
                                styles.Add(DefaultLayerStyles.Line(Id, layerName));
                            }
                            break;
                        case "Polygon":
                            if (!styles.Exists(style => style.Type == "fill"))
                            {
                                styles.Add(DefaultLayerStyles.Fill(Id, layerName));
                            }
                            break;
                    }
                }
            }

            return styles.ToArray();
        }

        public JObject DataJson => GetDataJson();

        private JObject GetDataJson()
        {
            if (_dataJson != null) return _dataJson;
            // retrieve from db and return 
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "select value from metadata where name='json'";
            var result = cmd.ExecuteScalar();
            if (result != null && (string)result != "")
            {
                _dataJson = JObject.Parse(Convert.ToString(result));
            }
            else
            {
                // if there's no json key, ExecuteScalar() returns null
                _dataJson = new JObject();
            }

            return _dataJson;
        }

        public byte[] Tile(int x, int y, int z)
        {
            using (var cmd = _connection.CreateCommand())
            {
                var command =
                    $"select tile_data as t from tiles where zoom_level={z} and tile_column={x} and tile_row={y}";
                cmd.CommandText = command;

                var result = (byte[])cmd.ExecuteScalar();

                return result ?? new byte[] { 0 };
            }
        }

        public string Grid(int x, int y, int z)
        {
            using (var cmd = _connection.CreateCommand())
            {
                try
                {
                    var command =
                        $"select grid as g from grids where zoom_level={z} and tile_column={x} and tile_row={y}";
                    cmd.CommandText = command;

                    var b = (byte[])cmd.ExecuteScalar();

                    if (b.Length == 0) return "{}";

                    var grid = Decompressor.Decompress(b);

                    var g = Encoding.UTF8.GetString(grid);

                    g = g.Substring(0, g.Length - 1);
                    g += ", \"data\":{";

                    var query =
                        $"SELECT key_name as key, key_json as json from grid_data where zoom_level={z} and tile_column={x} and tile_row={y}";

                    using (var keyCmd = new SqliteCommand(query, _connection))
                    {
                        using (var rdr = keyCmd.ExecuteReader())
                        {
                            while (rdr.Read()) g += "\"" + rdr.GetString(0) + "\":" + rdr.GetString(1) + ",";
                        }
                    }

                    g = g.Trim(',') + "}}";
                    return g;
                }
                catch (SqliteException)
                {
                    // most likely there was no grid data for this layer.
                    return "";
                }
            }
        }

        public void UpdateNameDescription(string name, string description)
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "UPDATE metadata SET value = $DescriptionValue WHERE name = $DescriptionName; " +
                                  "UPDATE metadata SET value = $NameValue WHERE name = $NameName;";

                cmd.Parameters.AddWithValue("$DescriptionValue", description ?? Source.Description);
                cmd.Parameters.AddWithValue("$DescriptionName", nameof(description));
                cmd.Parameters.AddWithValue("$NameValue", name ?? Source.Name);
                cmd.Parameters.AddWithValue("$NameName", nameof(name));

                cmd.ExecuteScalar();
            }
        }

        ~Layer()
        {
            if (_connection.State != ConnectionState.Closed) _connection.Close();
            _connection.Dispose();
        }

        public void Close()
        {
            if (_connection.State != ConnectionState.Closed) _connection.Close();
            _connection.Dispose();
        }

        //        public int DataVersion
        //        {
        //            get => GetDataVersion();
        //            set => SetDataVersion(value);
        //        }

        //        public JArray Style
        //        {
        //            get => GetStyle();
        //            set => _style = value;
        //        }

        //        private string GetName() {
        //            if (!string.IsNullOrEmpty(_name)) return _name;
        //            // retrieve from db and return 
        //            var cmd = Conn.CreateCommand();
        //            cmd.CommandText = "select value from metadata where name='name'";
        //            var result = cmd.ExecuteScalar();
        //            if (result != null)
        //            {
        //                _name = Convert.ToString(result);
        //            }
        //
        //            return _name ?? (_name = Id);
        //        }
        //        private void SetName(string value) {
        //            using (var rwConn = GetConnection(true)) {
        //                using (var rwCmd = rwConn.CreateCommand()) {
        //                    rwCmd.CommandText = "insert into metadata(name, value) values('name', $name)";
        //                    rwCmd.Parameters.AddWithValue("$name", value);
        //                    rwCmd.ExecuteNonQuery();
        //                }
        //            }
        //            _name = value;
        //        }

        //        private void SetDataVersion(int value)
        //        {
        //            // update db and then update our version
        //            // open database as writable so we can insert value
        //            using (var rwConn = GetConnection(true))
        //            {
        //                using (var rwCmd = rwConn.CreateCommand())
        //                {
        //                    rwCmd.CommandText =
        //                        "insert into metadata (name, value) values ('data_version', $data_version)";
        //                    rwCmd.Parameters.AddWithValue("$data_version", value);
        //                    rwCmd.ExecuteNonQuery();
        //                }
        //            }
        //
        //            _dataVersion = value;
        //        }
        //
        //        private int GetDataVersion()
        //        {
        //            if (_dataVersion != 0) return _dataVersion;
        //            // retrieve from db and return 
        //            var cmd = Conn.CreateCommand();
        //            cmd.CommandText = "select value from metadata where name='data_version'";
        //            var result = cmd.ExecuteScalar();
        //            if (result != null)
        //            {
        //                _dataVersion = Convert.ToInt32(result);
        //            }
        //            else
        //            {
        //                // if there's no data_version key, ExecuteScalar() returns null
        //                _dataVersion = 1;
        //                DataVersion = _dataVersion;
        //            }
        //
        //            return _dataVersion;
        //        }


        //
        //        /// Retrieve the metadata rows (excluding the name and json keys)
        //        /// <return>A Dictionary of key/value pairs from the MVT metadata</return>
        //        public Dictionary<string, string> GetMetadata()
        //        {
        //            var meta = new Dictionary<string, string>();
        //
        //            var metaCmd = Conn.CreateCommand();
        //            metaCmd.CommandText = "select name, value from metadata where name not in ('name', 'json')";
        //            using (var reader = metaCmd.ExecuteReader())
        //            {
        //                while (reader.Read()) meta.Add(reader["name"].ToString(), reader["value"].ToString());
        //            }
        //
        //            return meta;
        //        }
    }
}