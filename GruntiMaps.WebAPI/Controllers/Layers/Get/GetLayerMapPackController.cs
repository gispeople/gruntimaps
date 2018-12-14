using System;
using System.IO;
using GruntiMaps.Api.Common.Configuration;
using GruntiMaps.Api.Common.Resources;
using GruntiMaps.Api.DataContracts.V2;
using GruntiMaps.Domain.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GruntiMaps.WebAPI.Controllers.Layers.Get
{
    public class GetLayerMapPackController : WorkspaceLayerControllerBase
    {
        private readonly ILogger _logger;
        private readonly PathOptions _pathOptions;

        public GetLayerMapPackController(ILogger logger,
            IOptions<PathOptions> pathOptions)
        {
            _logger = logger;
            _pathOptions = pathOptions.Value;
        }

        [HttpGet(Resources.MapPackSubResource, Name = RouteNames.GetLayerMapPack)]
        public ActionResult Invoke()
        {
//            var fileName = $"{LayerId}.zip";
//            var path = Path.Combine(_pathOptions.Packs, $@"{fileName}");
//            try
//            {
//                return System.IO.File.Exists(path)
//                    ? new FileContentResult(System.IO.File.ReadAllBytes(path), "application/zip")
//                    {
//                        // assign a file name to the download
//                        FileDownloadName = $"{fileName}"
//                    }
//                    : throw new EntityNotFoundException();
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError($"Failed to read from existing zip file ({path}). {ex}");
//                throw;
//            }
            throw new NotImplementedException();
        }
    }
}
