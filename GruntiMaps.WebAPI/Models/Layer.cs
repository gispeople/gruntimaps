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
using GruntiMaps.WebAPI.Interfaces;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json.Linq;

namespace GruntiMaps.WebAPI.Models
{
    public class Layer : ILayer
    {
        private int _dataVersion;
        private string _name;

        private SqliteConnection _internalConnection;
        private readonly Options _options;
        private JArray _style;

        private JObject _dataJson;

        // this set of colours has been around for about 50 years and was designed
        // to be distinctive.
        private readonly string[] _kellyColors = {
            "#222222", "#F3C300", "#875692", "#F38400", "#A1CAF1", 
            "#BE0032", "#C2B280", "#848482", "#008856", "#E68FAC", "#0067A5",
            "#F99379", "#604E97", "#F6A600", "#B3446C", "#DCD300", "#882D17", 
            "#8DB600", "#654522", "#E25822", "#2B3D26", "#F2F3F4" };

        public Layer(Options options, string id)
        {
            _options = options;
            try
            {
                Id = id;
//                var dataName = DataJson["vector_layers"][0]["id"].ToString();
//                Name = dataName;
                Source = new MapBoxSource(Conn, Name, id);
            }
            catch (Exception e)
            {
                throw new Exception("Could not create layer source object", e);
            }
        }

        public string Name {
            get => GetName();
            set => SetName(value); 
        }

        public string Id { get; set; }

        public string Path { get; set; }

        public int DataVersion
        {
            get => GetDataVersion();
            set => SetDataVersion(value);
        }

        public SqliteConnection Conn => _internalConnection ?? (_internalConnection = GetConnection());
        public IMapBoxSource Source { get; set; }

        public JArray Style
        {
            get => GetStyle();
            set => _style = value;
        }

        public JObject DataJson => GetDataJson();

        ~Layer()
        {
            if (Conn.State != ConnectionState.Closed) Conn.Close();
        }

        private SqliteConnection GetConnection(bool writeable = false)
        {
            var connStr = "";
            try
            {
                if (Id == null) return null;
                var builder = new SqliteConnectionStringBuilder
                {
                    Mode = SqliteOpenMode.ReadOnly,
                    Cache = SqliteCacheMode.Shared,
                    DataSource = System.IO.Path.Combine(_options.TileDir, $"{Id}.mbtiles")
                };
                Path = builder.DataSource;
                if (writeable) builder.Mode = SqliteOpenMode.ReadWrite;
                connStr = builder.ConnectionString;
                var dbConnection = new SqliteConnection(connStr);
                dbConnection.Open();
                return dbConnection;
            }
            catch (Exception e)
            {
                throw new Exception(
                    $"Failed to open database for service {Id} (connection string={connStr}) exception={e}", e);
            }
        }

        private string GetName() {
            if (!string.IsNullOrEmpty(_name)) return _name;
            // retrieve from db and return 
            var cmd = Conn.CreateCommand();
            cmd.CommandText = "select value from metadata where name='name'";
            var result = cmd.ExecuteScalar();
            if (result != null)
            {
                _name = Convert.ToString(result);
            }

            return _name ?? (_name = Id);
        }
        private void SetName(string value) {
            using (var rwConn = GetConnection(true)) {
                using (var rwCmd = rwConn.CreateCommand()) {
                    rwCmd.CommandText = "insert into metadata(name, value) values('name', $name)";
                    rwCmd.Parameters.AddWithValue("$name", value);
                    rwCmd.ExecuteNonQuery();
                }
            }
            _name = value;
        }

        private void SetDataVersion(int value)
        {
            // update db and then update our version
            // open database as writeable so we can insert value
            using (var rwConn = GetConnection(true))
            {
                using (var rwCmd = rwConn.CreateCommand())
                {
                    rwCmd.CommandText =
                        "insert into metadata (name, value) values ('data_version', $data_version)";
                    rwCmd.Parameters.AddWithValue("$data_version", value);
                    rwCmd.ExecuteNonQuery();
                }
            }

            _dataVersion = value;
        }

        private int GetDataVersion()
        {
            if (_dataVersion != 0) return _dataVersion;
            // retrieve from db and return 
            var cmd = Conn.CreateCommand();
            cmd.CommandText = "select value from metadata where name='data_version'";
            var result = cmd.ExecuteScalar();
            if (result != null)
            {
                _dataVersion = Convert.ToInt32(result);
            }
            else
            {
                // if there's no data_version key, ExecuteScalar() returns null
                _dataVersion = 1;
                DataVersion = _dataVersion;
            }

            return _dataVersion;
        }

        private JObject GetDataJson()
        {
            if (_dataJson != null) return _dataJson;
            // retrieve from db and return 
            var cmd = Conn.CreateCommand();
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

        private JArray GetStyle()
        {
//            if (_style != null) return _style;
            var styleFile = System.IO.Path.Combine(_options.StyleDir, $"{Id}.json");
            if (!File.Exists(styleFile))
            {
                _style = new JArray();

                JObject data = GetDataJson();
                JObject tilestats = (JObject)data["tilestats"];
                if ((int) tilestats["layerCount"] <= 0) return _style;
                JArray layers = (JArray)tilestats["layers"];
                foreach (var layer in layers)
                {
                    var layerName = (string)layer["layer"];
                    //var count = (int)layer["count"];
                    var geometry = (string)layer["geometry"];
                    //var attribCount = (int)layer["attributeCount"];
                    //var attributes = layer["attributes"];
                    JObject styleLayer = new JObject();
                    JObject metadata = new JObject();
                    JObject gruntimaps = new JObject {{"autogenerated", true}};
                    metadata.Add("gruntimaps", gruntimaps);
                    styleLayer.Add("metadata", metadata);
                    styleLayer.Add("source", layerName);
                    styleLayer.Add("source-layer", layerName);
                    switch (geometry)
                    {
                        case "Point":
                            styleLayer.Add("id", layerName + "-circle");
                            styleLayer.Add("type", "circle");
                            JObject circle = new JObject
                            {
                                { "circle-stroke-color", "white" },
                                {
                                    "circle-color",
                                    _kellyColors[Math.Abs(layerName.GetHashCode()) % _kellyColors.Length]
                                },
                                { "circle-stroke-width", 1 }
                            };
                            styleLayer.Add("paint", circle);
                            break;
                        case "LineString":
                            styleLayer.Add("id", layerName + "-line");
                            JObject line = new JObject
                            {
                                {
                                    "line-color",
                                    _kellyColors[Math.Abs(layerName.GetHashCode()) % _kellyColors.Length]
                                },
                                { "line-width", 2 }
                            };
                            styleLayer.Add("paint", line);
                            styleLayer.Add("type", "line");
                            break;
                        case "Polygon":
                            styleLayer.Add("id", layerName + "-fill");
                            styleLayer.Add("type", "fill");
                            JObject fill = new JObject
                            {
                                {
                                    "fill-color",
                                    _kellyColors[Math.Abs(layerName.GetHashCode()) % _kellyColors.Length]
                                },
                                { "fill-outline-color", "white" },
                                { "fill-opacity", 0.2 }
                            };
                            styleLayer.Add("paint", fill);
                            break;
                    }

                    _style.Add(styleLayer);
                }
            }
            else
            {
                using (var f = new StreamReader(styleFile))
                {
                    var styleStr = f.ReadToEnd();
                    _style = JArray.Parse(styleStr);
                    foreach (var layer in _style)
                    {
                        layer["source"] = Source.name;
                    }
                }
            }

            return _style;
        }

        /// Retrieve the metadata rows (excluding the name and json keys)
        /// <return>A Dictionary of key/value pairs from the MVT metadata</return>
        public Dictionary<string, string> GetMetadata()
        {
            var meta = new Dictionary<string, string>();

            var metaCmd = Conn.CreateCommand();
            metaCmd.CommandText = "select name, value from metadata where name not in ('name', 'json')";
            using (var reader = metaCmd.ExecuteReader())
            {
                while (reader.Read()) meta.Add(reader["name"].ToString(), reader["value"].ToString());
            }

            return meta;
        }
    }
}