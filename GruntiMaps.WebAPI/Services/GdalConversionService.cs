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
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using GruntiMaps.Common.Extensions;
using GruntiMaps.Common.Services;
using GruntiMaps.WebAPI.Models;

namespace GruntiMaps.WebAPI.Services
{
    public class GdalConversionService : BackgroundService
    {
        private const int RetryLimit = 2;
        private const string ConverterFileName = "ogr2ogr";

        private readonly ILogger<GdalConversionService> _logger;
        private readonly ServiceOptions _serviceOptions;

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

            if (queued != null) // if no job queued, don't try
            {
                using (var workFolder = new TemporaryWorkFolder())
                {
                    try
                    {
                        var job = queued.Content;
                        if (job?.DataLocation != null && job.LayerId != null && job.WorkspaceId != null)
                        // if the job has missing values, don't process it, just delete it from queue.
                        {
                            var timer = new Stopwatch();
                            timer.Start();
                            _logger.LogDebug($"Processing GDAL Conversion for Layer {queued.Content.LayerId} within Queue Message {queued.Id}");

                            // Keep source and dest separate in case of file name collision.
                            var sourcePath = workFolder.CreateSubFolder("source");
                            var destPath = workFolder.CreateSubFolder("dest");

                            var downloadedFilePath = await new Uri(job.DataLocation).DownloadToLocal(sourcePath);
                            var inputFilePath = GetGdalInputFileParameter(downloadedFilePath, workFolder);
                            var geoJsonFile = Path.Combine(destPath, $"{job.LayerId}.geojson");

                            var processArgument = GetProcessArgument(job.LayerName, geoJsonFile, inputFilePath);
                            _logger.LogDebug($"executing ogr2ogr process with argument {processArgument}");
                            var executionResult =
                                ProcessExecutionService.ExecuteProcess(ConverterFileName, processArgument);
                            if (executionResult.success)
                            {
                                _logger.LogDebug($"ogr2ogr process successfully executed");
                            }
                            else
                            {
                                _logger.LogError($"ogr2ogr process failed: {executionResult.error}");
                            }

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
                        // we completed GDAL conversion and creation of MVT conversion request, so remove the GDAL request from the queue
                        await _gdConversionQueue.DeleteJob(queued);
                        _logger.LogDebug("GDAL Conversion message deleted ");
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

                                _logger.LogError($"GDAL Conversion failed for layer {queued.Content?.LayerId} after reaching retry limit", ex);
                            }
                            catch (Exception e)
                            {
                                _logger.LogError($"GDAL Conversion failed to clear bad conversion for layer {queued.Content?.LayerId}", e);
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"GDAL Conversion failed for layer {queued.Content?.LayerId} will retry later", ex);
                        }
                    }
                }

            }
            else
            {
                await Task.Delay(_serviceOptions.ConvertPolling);
            }
        }

        private string GetProcessArgument(string name, string geoJsonPath, string inputPath)
        {
            return "-f \"GeoJSON\" " + // always converting to GeoJSON
                   $"-nln \"{name}\" " +
                   "-t_srs \"EPSG:4326\" " + // always transform to WGS84
                   $"{geoJsonPath} " +
                   $"{inputPath}";
        }

        private string GetGdalInputFileParameter(string downloadedFilePath, TemporaryWorkFolder workFolder)
        {
            try
            {
                using (var zip = ZipFile.OpenRead(downloadedFilePath))
                {
                    if (zip.Entries.Count == 1)
                    {
                        // extract the source if has single entry
                        var singleEntry = zip.Entries.First();
                        using (var entryStream = zip.Entries.First().Open())
                        {
                            var extractedPath = Path.Combine(workFolder.CreateSubFolder("extracted"), singleEntry.Name);
                            using (var extractedStream = File.OpenWrite(extractedPath))
                            {
                                entryStream.CopyTo(extractedStream);
                                return extractedPath;
                            }
                        }
                    }

                    // use zip file if has multiple entries
                    return $@"/vsizip/{downloadedFilePath}";
                }
            }
            catch
            {
                // directly use downloaded file if not a zip file
                return downloadedFilePath;
            }
        }
    }
}
