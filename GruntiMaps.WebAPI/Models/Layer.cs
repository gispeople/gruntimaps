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
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Text;
using GruntiMaps.Api.DataContracts.V2.Layers;
using GruntiMaps.WebAPI.Interfaces;
using GruntiMaps.WebAPI.Util;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json.Linq;

namespace GruntiMaps.WebAPI.Models
{
    public class Layer : ILayer
    {
        private readonly string _id;
        private readonly StyleDto[] _styles;
        private readonly SourceDto _source;
        private readonly SqliteConnection _connection;
        private readonly Options _options;

        private JObject _dataJson;

        public Layer(Options options, string id)
        {
            _options = options;
            try
            {
                _id = id;
                _connection = GetConnection();
                _source = PopulateSourceInfo();
                _styles = PopulateStyleInfo();
            }
            catch (Exception e)
            {
                throw new Exception("Could not create layer source object", e);
            }
        }

        public string Id => _id;
        public string Name => _source.Name;
        public SourceDto Source => _source;
        public StyleDto[] Styles => _styles;

        public void Close()
        {
            _connection.Close();
        }
        
        // don't know what's this for
//        public string Path { get; set; }

        private SqliteConnection GetConnection(bool writeable = false)
        {
            if (_id == null) return null;
            var builder = new SqliteConnectionStringBuilder
            {
                Mode = writeable ? SqliteOpenMode.ReadWrite : SqliteOpenMode.ReadOnly,
                Cache = SqliteCacheMode.Shared,
                DataSource = System.IO.Path.Combine(_options.TileDir, $"{Id}.mbtiles")
            };
//            Path = builder.DataSource;
            var connStr = builder.ConnectionString;
            try
            {
                var dbConnection = new SqliteConnection(connStr);
                dbConnection.Open();
                return dbConnection;
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to open database for service {_id} (connection string={connStr}) exception={e}", e);
            }
        }

        private SourceDto PopulateSourceInfo()
        {
            if (_connection == null)
                throw new Exception($"Failed to popluate source info for {_id} as no connection exist");
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
                        "GruntiMaps Autogenerated" : source.Attribution;
                    // tiles url to be generated when user calls
                }
                return source;
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to popluate source info for {_id}", e);
            }
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

        ~Layer()
        {
            if (_connection.State != ConnectionState.Closed) _connection.Close();
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
        //            // open database as writeable so we can insert value
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



        //        private JArray GetStyle()
        //        {
        ////            if (_style != null) return _style;
        //            var styleFile = System.IO.Path.Combine(_options.StyleDir, $"{Id}.json");
        //            if (!File.Exists(styleFile))
        //            {
        //                _style = new JArray();
        //
        //                JObject data = GetDataJson();
        //                JObject tilestats = (JObject)data["tilestats"];
        //                // if no tilestats, we have nothing to base our style on
        //                if (tilestats == null) return _style;
        //                // if no layers represented in tilestats, still nothing to base on
        //                if ((int) tilestats["layerCount"] <= 0) return _style;
        //                JArray layers = (JArray)tilestats["layers"];
        //                foreach (var layer in layers)
        //                {
        //                    var layerName = (string)layer["layer"];
        //                    //var count = (int)layer["count"];
        //                    var geometry = (string)layer["geometry"];
        //                    //var attribCount = (int)layer["attributeCount"];
        //                    //var attributes = layer["attributes"];
        //                    JObject styleLayer = new JObject();
        //                    JObject metadata = new JObject();
        //                    JObject gruntimaps = new JObject {{"autogenerated", true}};
        //                    metadata.Add("gruntimaps", gruntimaps);
        //                    styleLayer.Add("metadata", metadata);
        //                    styleLayer.Add("source", layerName);
        //                    styleLayer.Add("source-layer", layerName);
        //                    switch (geometry)
        //                    {
        //                        case "Point":
        //                            styleLayer.Add("id", layerName + "-circle");
        //                            styleLayer.Add("type", "circle");
        //                            JObject circle = new JObject
        //                            {
        //                                { "circle-stroke-color", "white" },
        //                                {
        //                                    "circle-color",
        //                                    _kellyColors[Math.Abs(layerName.GetHashCode()) % _kellyColors.Length]
        //                                },
        //                                { "circle-stroke-width", 1 }
        //                            };
        //                            styleLayer.Add("paint", circle);
        //                            break;
        //                        case "LineString":
        //                            styleLayer.Add("id", layerName + "-line");
        //                            JObject line = new JObject
        //                            {
        //                                {
        //                                    "line-color",
        //                                    _kellyColors[Math.Abs(layerName.GetHashCode()) % _kellyColors.Length]
        //                                },
        //                                { "line-width", 2 }
        //                            };
        //                            styleLayer.Add("paint", line);
        //                            styleLayer.Add("type", "line");
        //                            break;
        //                        case "Polygon":
        //                            styleLayer.Add("id", layerName + "-fill");
        //                            styleLayer.Add("type", "fill");
        //                            JObject fill = new JObject
        //                            {
        //                                {
        //                                    "fill-color",
        //                                    _kellyColors[Math.Abs(layerName.GetHashCode()) % _kellyColors.Length]
        //                                },
        //                                { "fill-outline-color", "white" },
        //                                { "fill-opacity", 0.2 }
        //                            };
        //                            styleLayer.Add("paint", fill);
        //                            break;
        //                    }
        //
        //                    _style.Add(styleLayer);
        //                }
        //            }
        //            else
        //            {
        //                using (var f = new StreamReader(styleFile))
        //                {
        //                    var styleStr = f.ReadToEnd();
        //                    _style = JArray.Parse(styleStr);
        //                    foreach (var layer in _style)
        //                    {
        //                        layer["source"] = Source.name;
        //                    }
        //                }
        //            }
        //
        //            return _style;
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