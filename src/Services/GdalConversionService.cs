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
using GruntiMaps.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO.Compression;
using System.Collections.Generic;

namespace GruntiMaps.Services
{
    public class GdalConversionService : BackgroundService
    {
        private readonly ILogger<GdalConversionService> _logger;
        private readonly IMapData _mapdata;
        private readonly Options _options;
//        private static DocumentClient _client;

        /// <summary>
        ///     Create a new GdalConversionService instance.
        /// </summary>
        /// <param name="logger">system logger</param>
        /// <param name="options">global options for the Map Server</param>
        /// <param name="mapdata">Map data layers</param>
        public GdalConversionService(ILogger<GdalConversionService> logger, Options options, IMapData mapdata)
        {
            _logger = logger;
            _mapdata = mapdata;
            _options = options;
//            var EndpointUrl = "https://localhost:8081";
//            var AuthorisationKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
//            _client = new DocumentClient(new Uri(EndpointUrl), AuthorisationKey);
        }

        protected override async Task Process()
        {
            //            _logger.LogDebug("GDALConversion process starting.");

            // For the enlightenment of other, later, readers: 
            // ogr2ogr will be used to process not only obvious conversion sources (eg shape files) but also
            // geojson files. Why, you might ask, because tippecanoe can import GeoJSON directly? It's because
            // passing the GeoJSON through ogr2ogr will ensure that the final GeoJSON is in the correct projection
            // and that it should be valid GeoJSON as well.
            var queueClient = _mapdata.CloudAccount.CreateCloudQueueClient();
            // _logger.LogDebug($"Monitoring {_mapdata.CurrentOptions.GdConvQueue}");
            var gdalQueue = queueClient.GetQueueReference(_mapdata.CurrentOptions.GdConvQueue);
            await gdalQueue.CreateIfNotExistsAsync();
            // if there is a job on the gdal queue, process it.
            var gdalMsg = await gdalQueue.GetMessageAsync();
            if (gdalMsg != null)
            {
                var gdalStr = gdalMsg.AsString;
                _logger.LogDebug($"GDALConversion msg found on gdal queue = {gdalStr}");
                ConversionMessageData gdalData;
                try
                {
                    gdalData = JsonConvert.DeserializeObject<ConversionMessageData>(gdalStr);
                }
                catch (JsonReaderException e)
                {
                    _logger.LogError($"failed to decode JSON message on queue {e}");
                    return;
                }

                var start = DateTime.UtcNow;
                _logger.LogDebug("About to convert Gdal");

                var randomFolderName = Path.GetRandomFileName();
                _logger.LogDebug($"folder name = {randomFolderName}");
                // it will be in the system's temporary directory
                var tempPath = Path.Combine(Path.GetTempPath(), randomFolderName);
                _logger.LogDebug($"temporary path = {tempPath}");

                // If the directory already existed, throw an error - this should never happen, but just in case.
                if (Directory.Exists(tempPath))
                {
                    _logger.LogError($"The temporary path '{tempPath}' already existed.");
                    throw new Exception("The temporary path already existed.");
                }

                // Try to create the directory.
                Directory.CreateDirectory(tempPath);
                _logger.LogDebug($"The directory was created successfully at {Directory.GetCreationTime(tempPath)}.");

                // we need to keep source and dest separate in case there's a collision in filenames.
                var sourcePath = Path.Combine(tempPath, "source");
                Directory.CreateDirectory(sourcePath);

                var destPath = Path.Combine(tempPath, "dest");
                Directory.CreateDirectory(destPath);

                if (gdalData.DataLocation != null) // if it was null we don't want to do anything except remove the job from queue
                {
                    // retrieve the source data file from the supplied URI 
                    var remoteUri = new Uri(gdalData.DataLocation);
                    // we will need to know if this is a zip file later so that we can tell GDAL to use the zip virtual file system.
                    var isZip = Path.GetExtension(remoteUri.AbsolutePath).ToUpper() == ".ZIP";
                    var localFile = Path.Combine(sourcePath, Path.GetFileName(remoteUri.AbsolutePath));
                    WebClient myWebClient = new WebClient();
                    _logger.LogDebug($"Downloading {gdalData.DataLocation} to {localFile}");
                    myWebClient.DownloadFile(gdalData.DataLocation, localFile);
                    List<string> filesToProcess = new List<string>();   // a list of files to convert 
                                                                        // There could be multiple files if we were given a zip file - currently we look for shp and gdb. 
                    if (isZip)
                    {
                        // if it was a zip file we will extract it and use the content as the input.
                        using (var zip = ZipFile.OpenRead(localFile))
                        {
                            zip.ExtractToDirectory(sourcePath);
                        }
                        // Shape files are handled differently because there are usually multiple files per shape and we don't want to process the supporting files.
                        filesToProcess.AddRange(Directory.GetFiles(sourcePath, "*.shp"));
                        // GDB is also processed differently because they are actually directories, not files, but are treated by GDAL like they were files.
                        filesToProcess.AddRange(Directory.GetDirectories(sourcePath, "*.gdb"));
                        // for the moment we'll add geojson too. There's bound to be others we'll want to support but this should get us going.
                        filesToProcess.AddRange(Directory.GetFiles(sourcePath, "*.geojson"));
                    }
                    else
                    {
                        // not a zip file so we will assume that it's a single file to convert.
                        filesToProcess.Add(localFile);
                    }

                    foreach (var sourceFile in filesToProcess)
                    {
                        var layerName = Path.GetFileNameWithoutExtension(sourceFile);

                        var geoJsonFile = Path.Combine(destPath, $"{layerName}.geojson");
                        var gdalProcess = new Process
                        {
                            StartInfo = {
                                FileName = "ogr2ogr",
                                Arguments =
                                    "-f \"GeoJSON\" " +    // always converting to GeoJSON
                                    $"-nln \"{layerName}\" " +
                                    "-t_srs \"EPSG:4326\" " +  // always transform to WGS84
                                    $"{geoJsonFile} " +
                                    $"{sourceFile}",
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                CreateNoWindow = true
                            }
                        };
                        _logger.LogDebug($"ogr2ogr arguments are {gdalProcess.StartInfo.Arguments}");

                        gdalProcess.Start();
                        var errmsg = "";
                        while (!gdalProcess.StandardError.EndOfStream) errmsg += gdalProcess.StandardError.ReadLine();
                        gdalProcess.WaitForExit();
                        var exitCode = gdalProcess.ExitCode;
                        _logger.LogDebug($"og2ogr returned exit code {exitCode}");
                        if (exitCode != 0)
                        {
                            _logger.LogError($"Spatial data to GeoJSON conversion failed (errcode={exitCode}), msgs = {errmsg}");
                            throw new Exception($"Spatial data to GeoJSON conversion failed. {errmsg}");
                        }


                        _logger.LogDebug($"geojson file is in {geoJsonFile}");
                        // now we need to put the converted geojson file into storage
                        // var client = _mapdata.CloudAccount.CreateCloudBlobClient();
                        // var geojsonContainer = _mapdata.CloudClient.GetContainerReference(_mapdata.CurrentOptions.GeoJsonContainer);
                        // await geojsonContainer.CreateIfNotExistsAsync();
                        // await geojsonContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
                        CloudBlockBlob blob = _mapdata.GeojsonContainer.GetBlockBlobReference($"{layerName}.geojson");
                        _logger.LogDebug("Uploading geojson file to storage");
                        using (var fileStream = File.OpenRead(geoJsonFile))
                        {
                            await blob.UploadFromStreamAsync(fileStream);
//                            await _client.CreateAttachmentAsync(UriFactory.CreateDocumentCollectionUri("db", "regions"), fileStream);
                        }
                        _logger.LogDebug("Upload of geojson file to storage complete.");

                        var end = DateTime.UtcNow;
                        var duration = end - start;
                        _logger.LogDebug($"GDALConversion took {duration.TotalMilliseconds} ms.");

                        // we created geoJson so we can put a request in for geojson to mvt conversion.
                        await _mapdata.CreateMbConversionRequest(new ConversionMessageData
                        {
                            DataLocation = blob.Uri.ToString(),
                            Description = gdalData.Description,
                            LayerName = layerName
                        });
                    }
                }
                // we completed GDAL conversion and creation of MVT conversion request, so remove the GDAL request from the queue
                _logger.LogDebug("deleting gdal message from queue");
                await gdalQueue.DeleteMessageAsync(gdalMsg);
            }
            await Task.Delay(_options.CheckConvertTime);
        }
        //        }

    }
}
