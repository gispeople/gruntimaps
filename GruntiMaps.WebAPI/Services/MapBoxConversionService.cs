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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using GruntiMaps.Api.Common.Configuration;
using GruntiMaps.Common.Enums;
using GruntiMaps.ResourceAccess.Queue;
using GruntiMaps.ResourceAccess.Storage;
using GruntiMaps.ResourceAccess.Table;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GruntiMaps.WebAPI.Services
{
    /// <inheritdoc />
    /// <summary>
    ///     MapBoxConversionService monitors azure queue and performs data conversions via MapBox Tippecanoe
    /// </summary>
    public class MapBoxConversionService : BackgroundService
    {
        private const int RetryLimit = 3;

        private readonly ILogger<MapBoxConversionService> _logger;
        private readonly ServiceOptions _serviceOptions;

        private readonly IMbConversionQueue _mbConversionQueue;
        private readonly IStatusTable _statusTable;
        private readonly ITileStorage _tileStorage;

        public MapBoxConversionService(ILogger<MapBoxConversionService> logger, 
            IOptions<ServiceOptions> serviceOptions,
            IMbConversionQueue mbConversionQueue,
            IStatusTable statusTable,
            ITileStorage tileStorage)
        {
            _logger = logger;
            _serviceOptions = serviceOptions.Value;
            _mbConversionQueue = mbConversionQueue;
            _statusTable = statusTable;
            _tileStorage = tileStorage;
        }

        protected override async Task Process()
        {
            // there's two types of conversion to consider.
            // 1. spatial source data arrives and is placed in storage, we get a message and convert it 
            //    to geojson using gdal, and put the result in storage. We add a new req to the queue to 
            //    convert the geojson to mbtile.
            // 2. the geojson from the previous step (or possibly geojson directly) is in storage, we get
            //    a message and convert to mbtile and place result in storage.
            var queued = await _mbConversionQueue.GetJob();
            if (queued != null) // if no job queued, don't try
            {

                try
                {
                    var job = queued.Content;
                    if (job?.DataLocation != null && job.LayerId != null && job.WorkspaceId != null)
                    // if the job has missing values, don't process it, just delete it from queue.
                    {
                        // convert the geoJSON to a mapbox dataset
                        var timer = new Stopwatch();
                        timer.Start();
                        _logger.LogDebug($"Processing MbConversion for Layer {queued.Content.LayerId} within Queue Message {queued.Id}");

                        // it will be in the system's temporary directory
                        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
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

                        // retrieve the geoJSON file from the supplied URI 
                        var geoJsonFilename = $"{job.LayerId}.geojson";
                        var inputFilePath = Path.Combine(tempPath, geoJsonFilename);
                        _logger.LogDebug($"Downloading {job.DataLocation} to {inputFilePath}");
                        using (var webClient = new WebClient())
                        {
                            webClient.DownloadFile(job.DataLocation, inputFilePath);
                        }

                        var mbTilesFilePath = Path.Combine(tempPath, $"{job.LayerId}.mbtiles");
                        var tippecanoe = new Process
                        {
                            // TODO: need to consider whether *all* of these arguments are good for us *all* of the time.
                            // e.g. detect shared borders isn't what we want for building footprints. 
                            // we also don't want to drop point data at all, in general.
                            // max zoom should be -zg sometimes as well?

                            StartInfo =
                            {
                                FileName = "tippecanoe",
                                Arguments =
                                    $"-o {mbTilesFilePath} " + //$"--output={outputFile} " + 
                                    $"-n \"{job.LayerName}\" " + // $"--name=\"{name}\" "+
                                    $"-N \"{job.Description}\" " + // $"--description=\"{description}\" "+
                                    $"-l \"{job.LayerName}\" " + // $"--layer=\"{name}\" " + 
                                    // "-z18 " + // $"--maximum-zoom=18 " + 
                                    "-zg " + // $"--maximum-zoom=g " + // let's go back to guessing. It's insanely slow for z18 with big datasets.
                                    "-Bg " + // $"--base-zoom=g " +
                                    "-rg " + // $"--drop-rate=g " + 
                                    "-ae " + // $"--extend-zooms-if-still-dropping " + 
                                    "-as " + // $"--drop-densest-as-needed " +
                                    "-pS " + // $"--simplify-only-low-zooms "+
                                    "-ab " + // $"--detect-shared-borders "+
                                    "-aw " + // $"--detect-longitude-wraparound "
                                    $"{inputFilePath}",
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                CreateNoWindow = true
                            }
                        };
                        _logger.LogDebug($"Tippecanoe arguments are {tippecanoe.StartInfo.Arguments}");

                        tippecanoe.Start();
                        var errmsg = "";
                        while (!tippecanoe.StandardError.EndOfStream) errmsg += tippecanoe.StandardError.ReadLine();
                        tippecanoe.WaitForExit();
                        var exitCode = tippecanoe.ExitCode;
                        _logger.LogDebug($"Tippecanoe returned exit code {exitCode}");
                        if (exitCode != 0)
                        {
                            _logger.LogError($"GeoJSON to mbtiles conversion failed (errcode={exitCode}), msgs = {errmsg}");
                            throw new Exception($"GeoJSON to mbtiles conversion failed. {errmsg}");
                        }

                        _logger.LogDebug($"mbtile file is in {mbTilesFilePath}");
                        // now we need to put the converted mbtile file into storage
                        await _tileStorage.Store($"{job.WorkspaceId}/{job.LayerId}.mbtiles", mbTilesFilePath);
                        _logger.LogDebug("Upload of mbtile file to storage complete.");

                        timer.Stop();
                        _logger.LogDebug($"MapBoxConversion finshed for Layer {job.LayerId} in {timer.ElapsedMilliseconds} ms.");
                    }
                    await _mbConversionQueue.DeleteJob(queued);
                    _logger.LogDebug("Deleted MapBoxConversion message");
                    if (job?.LayerId != null && job?.WorkspaceId != null)
                    {
                        await _statusTable.UpdateStatus(job.WorkspaceId, job.LayerId, LayerStatus.Finished);
                    }
                }
                catch (Exception ex)
                {
                    if (queued.DequeueCount >= RetryLimit)
                    {
                        await _mbConversionQueue.DeleteJob(queued);
                        if (queued.Content?.LayerId != null && queued.Content?.WorkspaceId != null)
                        {
                            await _statusTable.UpdateStatus(queued.Content.WorkspaceId, queued.Content.LayerId, LayerStatus.Failed);
                        }
                        _logger.LogError($"MbConversion failed for layer {queued.Content?.LayerId} after reaching retry limit", ex);
                    }
                    else
                    {
                        _logger.LogWarning($"MbConversion failed for layer {queued.Content?.LayerId} and will retry later", ex);
                    }
                }
            }
            else
            {
                await Task.Delay(_serviceOptions.ConvertPolling);
            }
        }
    }
}