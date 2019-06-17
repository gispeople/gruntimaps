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

using GruntiMaps.Api.Common.Configuration;
using GruntiMaps.Common.Enums;
using GruntiMaps.ResourceAccess.Queue;
using GruntiMaps.ResourceAccess.Storage;
using GruntiMaps.ResourceAccess.Table;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace GruntiMaps.WebAPI.Services
{
    public class GdalConversionService : BackgroundService
    {
        private const int RetryLimit = 3;

        private readonly ILogger<GdalConversionService> _logger;
        private readonly ServiceOptions _serviceOptions;
        private readonly List<string> _supportedFileTypes;

        private readonly IGdConversionQueue _gdConversionQueue;
        private readonly IMbConversionQueue _mbConversionQueue;
        private readonly IStatusTable _statusTable;
        private readonly IGeoJsonStorage _geoJsonStorage;

        public GdalConversionService(ILogger<GdalConversionService> logger,
            IOptions<ServiceOptions> serviceOptions,
            IGdConversionQueue gdConversionQueue,
            IMbConversionQueue mbConversionQueue,
            IStatusTable statusTable,
            IGeoJsonStorage geoJsonStorage)
        {
            _logger = logger;
            _serviceOptions = serviceOptions.Value;
            _supportedFileTypes = new List<string> { ".shp", ".geojson", ".gdb" };

            _gdConversionQueue = gdConversionQueue;
            _mbConversionQueue = mbConversionQueue;
            _statusTable = statusTable;
            _geoJsonStorage = geoJsonStorage;
        }

        protected override async Task Process()
        {
            // For the enlightenment of other, later, readers: 
            // ogr2ogr will be used to process not only obvious conversion sources (eg shape files) but also
            // geojson files. Why, you might ask, because tippecanoe can import GeoJSON directly? It's because
            // passing the GeoJSON through ogr2ogr will ensure that the final GeoJSON is in the correct projection
            // and that it should be valid GeoJSON as well.
            QueuedConversionJob queued = null;
            try
            {
                queued = await _gdConversionQueue.GetJob();
            }
            catch (Exception ex)
            {
                _logger.LogError($"GdalConversion failed to retrieve queued job", ex);
            }

            var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            _logger.LogDebug($"temporary path = {tempPath}");

            if (queued != null) // if no job queued, don't try
            {
                try
                {
                    var job = queued.Content;
                    if (job?.DataLocation != null && job.LayerId != null && job.WorkspaceId != null)
                    // if the job has missing values, don't process it, just delete it from queue.
                    {
                        var timer = new Stopwatch();
                        timer.Start();
                        _logger.LogDebug($"Processing Gdal Conversion for Layer {queued.Content.LayerId} within Queue Message {queued.Id}");

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

                        if (job.DataLocation != null) // if it was null we don't want to do anything except remove the job from queue
                        {
                            // retrieve the source data file from the supplied URI 
                            var remoteUri = new Uri(job.DataLocation);
                            // we will need to know if this is a supported file type
                            var fileType = Path.GetExtension(remoteUri.AbsolutePath).ToLower();
                            if (!_supportedFileTypes.Contains(fileType))
                            {
                                throw new Exception($"Unsupported file type: {fileType}");
                            }

                            var inputFilePath = Path.Combine(sourcePath, Path.GetFileName(remoteUri.AbsolutePath));
                            _logger.LogDebug($"Downloading {job.DataLocation} to {inputFilePath}");
                            using (var webClient = new WebClient())
                            {
                                webClient.DownloadFile(job.DataLocation, inputFilePath);
                            }

                            var geoJsonFile = Path.Combine(destPath, $"{job.LayerId}.geojson");
                            var gdalProcess = new Process
                            {
                                StartInfo =
                                {
                                    FileName = "ogr2ogr",
                                    Arguments =
                                        "-f \"GeoJSON\" " +    // always converting to GeoJSON
                                        $"-nln \"{job.LayerName}\" " +
                                        "-t_srs \"EPSG:4326\" " +  // always transform to WGS84
                                        $"{geoJsonFile} " +
                                        $"{inputFilePath}",
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
                            var location = await _geoJsonStorage.Store($"{job.WorkspaceId}/{job.LayerId}.geojson", geoJsonFile);
                            _logger.LogDebug("Upload of geojson file to storage complete.");

                            timer.Stop();
                            _logger.LogDebug($"GDAL Conversion finished for Layer {job.LayerId} in {timer.ElapsedMilliseconds} ms.");

                            // we created geoJson so we can put a request in for geojson to mvt conversion.
                            await _mbConversionQueue.Queue(new ConversionJobData
                            {
                                LayerId = job.LayerId,
                                WorkspaceId = job.WorkspaceId,
                                LayerName = job.LayerName,
                                Description = job.Description,
                                DataLocation = location
                            });
                        }
                    }
                    // we completed GDAL conversion and creation of MVT conversion request, so remove the GDAL request from the queue
                    await _gdConversionQueue.DeleteJob(queued);
                    _logger.LogDebug("Deleted GdalConversion message");
                }
                catch (Exception ex)
                {
                    if (queued.DequeueCount >= RetryLimit)
                    {
                        try
                        {
                            await _gdConversionQueue.DeleteJob(queued);
                            if (queued.Content?.LayerId != null && queued.Content?.WorkspaceId != null)
                            {
                                await _statusTable.UpdateStatus(queued.Content.WorkspaceId, queued.Content.LayerId,
                                    LayerStatus.Failed);
                            }

                            _logger.LogError($"GdalConversion failed for layer {queued.Content?.LayerId} after reaching retry limit", ex);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError($"GdalConversion failed to clear bad conversion for layer {queued.Content?.LayerId}", e);
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"GdalConversion failed for layer {queued.Content?.LayerId} will retry later", ex);
                    }
                }

                Directory.Delete(tempPath, true);
            }
            else
            {
                await Task.Delay(_serviceOptions.ConvertPolling);
            }
        }

    }
}
