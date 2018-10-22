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
using GruntiMaps.Interfaces;
using Microsoft.Data.Sqlite;

namespace GruntiMaps.Models
{
    public class MapBoxSource : IMapBoxSource
    {
        public MapBoxSource(SqliteConnection connection, string serviceName)
        {
            try
            {
                using (var metaCmd = connection.CreateCommand())
                {
                    metaCmd.CommandText = "select name, value from metadata";
                    using (var rdr = metaCmd.ExecuteReader())
                    {
                        while (rdr.Read())
                            switch (rdr.GetString(0))
                            {
                                case "bounds":
                                    bounds = new double[4];
                                    var x = rdr.GetString(1).Split(',');

                                    for (var i = 0; i < 4; i++) bounds[i] = Convert.ToDouble(x[i]);

                                    break;
                                case "center":
                                    var cen = rdr.GetString(1).Split(',');
                                    center = new double[3];
                                    center[0] = Convert.ToDouble(cen[0]);
                                    center[1] = Convert.ToDouble(cen[1]);
                                    center[2] = Convert.ToInt16(cen[2]);

                                    break;
                                case "maxzoom":
                                    maxzoom = Convert.ToInt16(rdr.GetString(1));
                                    break;
                                case "minzoom":
                                    minzoom = Convert.ToInt16(rdr.GetString(1));
                                    break;
                                case "name":
                                    //name = rdr.GetString(1);
                                    name = serviceName; // the internal source name is not always as expected.
                                    break;
                                case "description":
                                    description = rdr.GetString(1);
                                    break;
                                case "version":
                                    version = rdr.GetString(1);
                                    break;
                                case "attribution":
                                    attribution = rdr.GetString(1);
                                    break;
                                case "format":
                                    format = rdr.GetString(1);
                                    break;
                                case "tilejson": 
                                    tilejson = rdr.GetString(1);
                                    break;
                                case "template":
                                    template = rdr.GetString(1);
                                    break;
                                case "scheme":
                                    scheme = rdr.GetString(1);
                                    break;
                                case "data_version":
                                    data_version = Convert.ToInt16(rdr.GetString(1));
                                    break;
                            }
                    }
                }
            }
            catch (SqliteException e)
            {
                throw new Exception("Problem with metadata", e);
            }
            if (name == null) name = serviceName;
            if (tilejson == null) tilejson = "2.0.0";
            if (scheme == null) scheme = "xyz";
            if (format == "pbf") type = "vector"; else type = "raster";
            tiles = new string[1];
            tiles[0] = $"#publicHost#/api/layers/tiles/{serviceName}?x={{x}}&y={{y}}&z={{z}}";
        }

        public string tilejson { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        public string version { get; set; }

        public string attribution { get; set; }

        public string template { get; set; }

        public string legend { get; set; }

        public string scheme { get; set; }

        public string[] tiles { get; set; }

        public string[] grids { get; set; }

        public string[] data { get; set; }

        public double minzoom { get; set; }

        public double maxzoom { get; set; }

        public double[] bounds { get; set; }

        public double[] center { get; set; }

        public double data_version {get; set; }

        public string type { get; set; }

        public string format { get; set; }
    }
}