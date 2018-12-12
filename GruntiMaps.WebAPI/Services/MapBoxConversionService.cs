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
using GruntiMaps.WebAPI.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace GruntiMaps.WebAPI.Services
{
    /// <inheritdoc />
    /// <summary>
    ///     MapBoxConversionService monitors azure queue and performs data conversions via MapBox Tippecanoe
    /// </summary>
    public class MapBoxConversionService : BackgroundService
    {
        private readonly ILogger<MapBoxConversionService> _logger;
        private readonly ServiceOptions _serviceOptions;

        private readonly IMbConversionQueue _mbConversionQueue;
        private readonly IStatusTable _statusTable;
        private readonly ITileStorage _tileStorage;

        /// <summary>
        ///     Create a new MapBoxConversionService instance.
        /// </summary>
        /// <param name="logger">system logger</param>
        /// <param name="mbConversionQueue"></param>
        /// <param name="statusTable"></param>
        /// <param name="tileStorage"></param>
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
            // _logger.LogDebug("MapBoxConversion process starting.");

            // there's two types of conversion to consider.
            // 1. spatial source data arrives and is placed in storage, we get a message and convert it 
            //    to geojson using gdal, and put the result in storage. We add a new req to the queue to 
            //    convert the geojson to mbtile.
            // 2. the geojson from the previous step (or possibly geojson directly) is in storage, we get
            //    a message and convert to mbtile and place result in storage.
            var mbMsg = await _mbConversionQueue.GetMessage();
            if (mbMsg != null) // if no message, don't try
            {
                ConversionMessageData mbData = null;
                try
                {
                    try
                    {
                        mbData = JsonConvert.DeserializeObject<ConversionMessageData>(mbMsg.Content);
                    }
                    catch (JsonReaderException e)
                    {
                        _logger.LogError($"failed to decode JSON message on queue {e}");
                        return;
                    }
                    if (mbData.DataLocation != null && mbData.LayerName != null && mbData.Description != null)
                    // if the mbData had missing values, don't process it, just delete it from queue.
                    {
                        // convert the geoJSON to a mapbox dataset
                        var start = DateTime.UtcNow;
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

                        // retrieve the geoJSON file from the supplied URI 
                        var geoJsonFilename = $"{mbData.LayerId}.geojson";
                        var inputFile = Path.Combine(tempPath, geoJsonFilename);
                        _logger.LogDebug($"Downloading {mbData.DataLocation} to {inputFile}");
                        WebClient myWebClient = new WebClient();
                        myWebClient.DownloadFile(mbData.DataLocation, inputFile);
                        var mbtilesFilename = $"{mbData.LayerId}.mbtiles";
                        var mbtileFile = Path.Combine(tempPath, mbtilesFilename);
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
                                    $"-o {mbtileFile} " + //$"--output={outputFile} " + 
                                    $"-n \"{mbData.LayerName}\" " + // $"--name=\"{name}\" "+
                                    $"-N \"{mbData.Description}\" " + // $"--description=\"{description}\" "+
                                    $"-l \"{mbData.LayerName}\" " + // $"--layer=\"{name}\" " + 
                                    // "-z18 " + // $"--maximum-zoom=18 " + 
                                    "-zg " + // $"--maximum-zoom=g " + // let's go back to guessing. It's insanely slow for z18 with big datasets.
                                    "-Bg " + // $"--base-zoom=g " +
                                    "-rg " + // $"--drop-rate=g " + 
                                    "-ae " + // $"--extend-zooms-if-still-dropping " + 
                                    "-as " + // $"--drop-densest-as-needed " +
                                    "-pS " + // $"--simplify-only-low-zooms "+
                                    "-ab " + // $"--detect-shared-borders "+
                                    "-aw " + // $"--detect-longitude-wraparound "
                                    $"{inputFile}",
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

                        _logger.LogDebug($"mbtile file is in {mbtileFile}");
                        // now we need to put the converted mbtile file into storage
                        await _tileStorage.Store($"{mbData.LayerId}.mbtiles", mbtileFile);
                        _logger.LogDebug("Upload of mbtile file to storage complete.");
                        var end = DateTime.UtcNow;
                        var duration = end - start;
                        _logger.LogDebug($"MapBoxConversion took {duration.TotalMilliseconds} ms.");
                    }
                    await _mbConversionQueue.DeleteMessage(mbMsg);
                    _logger.LogDebug("Deleted MapBoxConversion message");
                    await _statusTable.UpdateStatus(mbData.LayerId, LayerStatus.Finished);
                }
                catch (Exception)
                {
                    if (mbData != null)
                    {
                        await _statusTable.UpdateStatus(mbData.LayerId, LayerStatus.Failed);
                    }
                    throw;
                }
            }
            else
            {
                await Task.Delay(_serviceOptions.ConvertPolling);
            }

            // _logger.LogDebug("MapBoxConversion process complete.");

        }
    }
}