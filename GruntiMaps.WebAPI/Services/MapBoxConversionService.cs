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
using System.Threading.Tasks;
using GruntiMaps.Api.Common.Configuration;
using GruntiMaps.Common.Enums;
using GruntiMaps.Common.Extensions;
using GruntiMaps.Common.Services;
using GruntiMaps.ResourceAccess.Queue;
using GruntiMaps.ResourceAccess.Storage;
using GruntiMaps.ResourceAccess.Table;
using GruntiMaps.ResourceAccess.TopicSubscription;
using GruntiMaps.WebAPI.Models;
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
        private const int RetryLimit = 2;
        private const string ConverterFileName = "tippecanoe";

        private readonly ILogger<MapBoxConversionService> _logger;
        private readonly ServiceOptions _serviceOptions;

        private readonly IMbConversionQueue _mbConversionQueue;
        private readonly IStatusTable _statusTable;
        private readonly ITileStorage _tileStorage;
        private readonly IMapLayerUpdateTopicClient _topicClient;

        public MapBoxConversionService(ILogger<MapBoxConversionService> logger, 
            IOptions<ServiceOptions> serviceOptions,
            IMbConversionQueue mbConversionQueue,
            IStatusTable statusTable,
            ITileStorage tileStorage,
            IMapLayerUpdateTopicClient topicClient)
        {
            _logger = logger;
            _serviceOptions = serviceOptions.Value;
            _mbConversionQueue = mbConversionQueue;
            _statusTable = statusTable;
            _tileStorage = tileStorage;
            _topicClient = topicClient;
        }

        protected override async Task Process()
        {
            // there's two types of conversion to consider.
            // 1. spatial source data arrives and is placed in storage, we get a message and convert it 
            //    to geojson using gdal, and put the result in storage. We add a new req to the queue to 
            //    convert the geojson to mbtile.
            // 2. the geojson from the previous step (or possibly geojson directly) is in storage, we get
            //    a message and convert to mbtile and place result in storage.

            QueuedConversionJob queued = null;
            try
            {
                queued = await _mbConversionQueue.GetJob();
            }
            catch (Exception ex)
            {
                _logger.LogError($"MapBox Conversion failed to retrieve queued job", ex);
            }

            if (queued != null) // if no job queued, don't try
            {
                using (var workFolder = new TemporaryWorkFolder())
                {
                    try
                    {
                        var job = queued.Content;
                        if (job?.DataLocation != null && job?.LayerId != null && job?.WorkspaceId != null)
                        // if the job has missing values, don't process it, just delete it from queue.
                        {
                            // convert the geoJSON to a mapbox dataset
                            var timer = new Stopwatch();
                            timer.Start();

                            // retrieve the geoJSON file from the supplied URI 
                            var inputFilePath = await new Uri(job.DataLocation).DownloadToLocal(workFolder.Path);
                            var mbTilesFilePath = Path.Combine(workFolder.Path, $"{job.LayerId}.mbtiles");

                            var processArgument = GetProcessArgument(job.LayerName, job.Description, mbTilesFilePath, inputFilePath);
                            _logger.LogDebug($"executing tippecanoe process with argument {processArgument}");
                            var executionResult =
                                ProcessExecutionService.ExecuteProcess(ConverterFileName, processArgument);
                            if (executionResult.success)
                            {
                                _logger.LogDebug($"tippecanoe process successfully executed");
                            }
                            else
                            {
                                _logger.LogError($"tippecanoe process failed: {executionResult.error}");
                            }

                            // now we need to put the converted mbtile file into storage
                            await _tileStorage.Store($"{job.WorkspaceId}/{job.LayerId}.mbtiles", mbTilesFilePath);
                            _logger.LogDebug("Upload of mbtile file to storage complete.");

                            timer.Stop();
                            _logger.LogDebug($"MapBox Conversion finished for Layer {job.LayerId} in {timer.ElapsedMilliseconds} ms.");

                            try
                            {
                                await _statusTable.UpdateStatus(job.WorkspaceId, job.LayerId, LayerStatus.Finished);
                                _logger.LogDebug($"Layer {job.LayerId} status updated to Finished");
                            }
                            catch(Exception ex)
                            {
                                _logger.LogError($"Error when updating Layer {job.LayerId} status to Finished", ex);
                                throw;
                            }
                            
                            await _topicClient.SendMessage(new MapLayerUpdateData
                            {
                                MapLayerId = job.LayerId,
                                WorkspaceId = job.WorkspaceId,
                                Type = MapLayerUpdateType.Update
                            });
                        }
                        await _mbConversionQueue.DeleteJob(queued);
                        _logger.LogDebug("Deleted MapBox Conversion message");
                    }
                    catch (Exception ex)
                    {
                        if (queued.DequeueCount >= RetryLimit)
                        {
                            try
                            {
                                await _mbConversionQueue.DeleteJob(queued);
                                if (queued.Content?.LayerId != null && queued.Content?.WorkspaceId != null)
                                {
                                    await _statusTable.UpdateStatus(queued.Content.WorkspaceId, queued.Content.LayerId, LayerStatus.Failed);
                                }
                                _logger.LogError($"MapBox Conversion failed for layer {queued.Content?.LayerId} after reaching retry limit", ex);
                            }
                            catch (Exception e)
                            {
                                _logger.LogError($"MapBox Conversion failed to clear bad conversion for layer {queued.Content?.LayerId}", e);
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"MapBox Conversion failed for layer {queued.Content?.LayerId} and will retry later", ex);
                        }
                    }
                }
            }
            else
            {
                await Task.Delay(_serviceOptions.ConvertPolling);
            }
        }

        private string GetProcessArgument(string name, string description, string mbTilesPath, string inputPath)
        {
            // TODO: need to consider whether *all* of these arguments are good for us *all* of the time.
            // e.g. detect shared borders isn't what we want for building footprints. 
            // we also don't want to drop point data at all, in general.
            // max zoom should be -zg sometimes as well?

            return $"-o {mbTilesPath} " + //$"--output={outputFile} " + 
                   $"-n \"{name}\" " + // $"--name=\"{name}\" "+
                   $"-N \"{description}\" " + // $"--description=\"{description}\" "+
                   $"-l \"{name}\" " + // $"--layer=\"{name}\" " + 
                   // "-z18 " + // $"--maximum-zoom=18 " + 
                   "-zg " + // $"--maximum-zoom=g " + // let's go back to guessing. It's insanely slow for z18 with big datasets.
                   "-Bg " + // $"--base-zoom=g " +
                   "-rg " + // $"--drop-rate=g " + 
                   "-ae " + // $"--extend-zooms-if-still-dropping " + 
                   "-as " + // $"--drop-densest-as-needed " +
                   "-pS " + // $"--simplify-only-low-zooms "+
                   "-ab " + // $"--detect-shared-borders "+
                   "-aw " + // $"--detect-longitude-wraparound "
                   $"{inputPath}";
        }
    }
}