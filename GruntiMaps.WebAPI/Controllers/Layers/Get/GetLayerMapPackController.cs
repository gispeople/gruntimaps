using System;
using System.IO;
using GruntiMaps.Api.Common.Configuration;
using GruntiMaps.Api.Common.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GruntiMaps.WebAPI.Controllers.Layers.Get
{
    public class GetLayerMapPackController : ApiControllerBase
    {
        private readonly ILogger _logger;
        private readonly PathOptions _pathOptions;

        public GetLayerMapPackController(ILogger logger,
            IOptions<PathOptions> pathOptions)
        {
            _logger = logger;
            _pathOptions = pathOptions.Value;
        }

        [HttpGet("layers/{id}/mappack", Name = RouteNames.GetLayerMapPack)]
        public ActionResult Invoke(string id)
        {
            var zipFileName = $"{id}.zip";
            var path = Path.Combine(_pathOptions.Packs, $@"{zipFileName}");
            try
            {
                if (System.IO.File.Exists(path))
                {
                    return new FileContentResult(System.IO.File.ReadAllBytes(path), "application/zip")
                    {
                        // assign a file name to the download
                        FileDownloadName = $"{zipFileName}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to read from existing zip file ({path}). {ex}");
            }
            return NoContent();
        }
    }
}
