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
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace GruntiMaps.Models
{
    public class Options
    {
        public Options(IConfiguration config, IHostingEnvironment env)
        {
            var pathConfig = config.GetSection("Paths");
            RootDir = pathConfig["contentRoot"] ?? Path.Combine(env.ContentRootPath, @"mbroot");
            StyleDir = pathConfig["styleDir"] ?? Path.Combine(RootDir, @"json");
            TileDir = pathConfig["tileDir"] ?? Path.Combine(RootDir, @"mbtiles");
            PackDir = pathConfig["packDir"] ?? Path.Combine(RootDir, @"zip");
            FontDir = pathConfig["fontDir"] ?? Path.Combine(RootDir, @"fonts");
            StorageAccount = config["globalStorageAccount"];
            StorageKey = config["globalStorageKey"];
            var providerCfg = config.GetSection("Provider");
            if (providerCfg["Type"] == null) StorageProvider = StorageProviders.Local; 
            else {
                switch (providerCfg["Type"].ToLower())
                {
                    case "azure":
                        StorageProvider = StorageProviders.Azure;
                        break;
                    case "local":
                        StorageProvider = StorageProviders.Local;
                        break;
                    default:
                        StorageProvider = StorageProviders.Local;
                        break;
                }
            }
            if (StorageProvider == StorageProviders.Local)
            {
                var localCfg = providerCfg.GetSection("LocalSettings");
                StoragePath = localCfg["Path"] ?? env.ContentRootPath;
            }
            var containerConfig = config.GetSection("Containers");
            StorageContainer = containerConfig["storage"];
            FontContainer = containerConfig["fonts"];
            PacksContainer = containerConfig["packs"];
            MbTilesContainer = containerConfig["mbtiles"];
            StyleContainer = containerConfig["styles"];
            GeoJsonContainer = containerConfig["geojson"];
            var queueCfg = config.GetSection("Queues");
            MbConvQueue = queueCfg["mvtConversion"];
            GdConvQueue = queueCfg["gdalConversion"];
            var servicesCfg = config.GetSection("Services");
            CheckUpdateTime = ParseConfigInt(servicesCfg["layerRefresh"]);
            CheckConvertTime = ParseConfigInt(servicesCfg["convertPolling"]);
        }



        private int ParseConfigInt(string configValue, int defaultSeconds = 10)
        {
            int val;
            if (!string.IsNullOrEmpty(configValue) && (val = int.Parse(configValue)) != 0)
            {
                return val * 1000;
            }
            return defaultSeconds * 1000;
            
        }
        public string RootDir { get; }

        public string StyleDir { get; }

        public string TileDir { get; }

        public string PackDir { get; }

        public string FontDir { get; }

        public int CheckUpdateTime { get; }

        public int CheckConvertTime { get; }

        public string StorageAccount { get; }

        public string StorageKey { get; }

        public string StorageContainer { get; }

        public string GeoJsonContainer { get; }
        
        public string FontContainer { get; }

        public string StyleContainer { get; }

        public string PacksContainer { get; }

        public string MbTilesContainer { get; }

        public string GdConvQueue { get; }

        public string MbConvQueue { get; }

        /* 
        StorageProvider determines whether to use Azure Storage/Queues or local file system. 
        It is intended to be the basis for adding support for AWS, Google cloud, IBM, etc etc.
         */
        public StorageProviders StorageProvider { get; }

        public string TilePath => TileDir;
        public string PackPath => PackDir;
        public string StylePath => StyleDir;
        public string FontPath => FontDir;

        public string StoragePath { get; }
    }
}