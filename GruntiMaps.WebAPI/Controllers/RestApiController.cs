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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using GruntiMaps.Api.Common.Configuration;
using GruntiMaps.WebAPI.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GruntiMaps.WebAPI.Controllers
{
    [Produces("application/json")]
    [Route("api")]
    public class RestApiController : Controller
    {
        private readonly IHostingEnvironment _hostingEnv;
        private readonly PathOptions _pathOptions;
        private readonly ILogger _logger;

        public RestApiController(IHostingEnvironment hostingEnv,
                                 IOptions<PathOptions> options,
                                 ILogger<RestApiController> logger)
        {
            _hostingEnv = hostingEnv;
            _logger = logger;
            _pathOptions = options.Value;
        }

        // RESTful api root
        [HttpGet]
        public ActionResult GetRoot()
        {
            var baseUrl = GetBaseUrl();
            return Json(new
            {
                links = new List<object>
                {
                    new {href = baseUrl, rel = "self"},
                    new {href = $"{baseUrl}/layers", rel = "collection", title = "layers"},
                    new {href = $"{baseUrl}/fonts", rel = "collection", title = "fonts"},
                    new {href = $"{baseUrl}/sprites", rel = "collection", title = "sprites"}
                }
            });
        }


        // Retrieve a list of all available fonts.
        [HttpGet("fonts")]
        public ActionResult GetFonts()
        {
            var fontDir = new DirectoryInfo(_pathOptions.Fonts);
            var dirInfo = fontDir.GetDirectories();
            var resources = dirInfo.Select(di => new
            {
                name = di.Name,
                links = new
                {
                    href = $"{GetBaseUrl()}/{HttpUtility.UrlEncode(di.Name)}",
                    rel = "collection"
                }
            }).ToList();
            //resources.Add(new RestLink {href = Request.GetDisplayUrl(), rel = "self", title = "self"});
            return Json(new
            {
                content = resources,
                links = new
                {
                    href = GetBaseUrl(),
                    rel = "self"
                }
            });
        }

        // retrieve possible ranges for mapbox font.
        [HttpGet("fonts/{face}")]
        public ActionResult Font(string face)
        {
            if (face == null)
            {
                return new RestError(400, new[] {
                    new RestErrorDetails { field = "face", issue = "Font face must be supplied" }
                }).AsJsonResult();
            }

            var faceFile = HttpUtility.UrlDecode(face);
            // validate font
            if (!Regex.IsMatch(faceFile, "^[a-zA-Z ]+$"))
            {
                return new RestError(400, new[] {
                    new RestErrorDetails { field = "face", issue = "Font face is invalid" }
                }).AsJsonResult();
            }

            var rangeDir = new DirectoryInfo(Path.Combine(_pathOptions.Fonts, faceFile));
            var ranges = rangeDir.GetFiles("*.pbf", SearchOption.TopDirectoryOnly);
            var resources = ranges.Select(fr => new
            {
                name = $"{faceFile}, glyphs {Path.GetFileNameWithoutExtension(fr.Name)}",
                links = new
                {
                    href = $"{GetBaseUrl()}/{Path.GetFileNameWithoutExtension(fr.Name)}",
                    rel = "item"
                }
            }).ToList();

            return Json(new
            {
                content = resources,
                links = new
                {
                    href = GetBaseUrl(),
                    rel = "self"
                }
            });
        }

        // retrieve a mapbox font. 
        // needs to support multiple fonts listed, as so:
        // api/fonts/Open%20Sans%20Regular,Arial%20Unicode%20MS%20Regular/0-255 which means try the first font, and if not found, try the second.
        [HttpGet("fonts/{face}/{range}")]
        public ActionResult Font(string face, string range)
        {
            var details = new List<RestErrorDetails>();
            if (face == null || range == null)
            {
                if (face == null) details.Add(new RestErrorDetails { field = "face", issue = "Face must be supplied" });
                if (range == null) details.Add(new RestErrorDetails { field = "range", issue = "Range must be supplied" });
                return new RestError(400, details.ToArray()).AsJsonResult();
            }

            var faceFile = HttpUtility.UrlDecode(face);
            var fontChoices = faceFile.Split(",");
            foreach (var fontChoice in fontChoices)
            {
                // validate font and range
                if (!Regex.IsMatch(fontChoice, "^[a-zA-Z ]+$"))
                    details.Add(new RestErrorDetails { field = "face", issue = "Font face is invalid" });

                if (!Regex.IsMatch(range, "^[0-9]{1,5}-[0-9]{1,5}$"))
                    details.Add(new RestErrorDetails { field = "range", issue = "Font range is invalid" });
                // if there were errors for this font, skip to the next one (if it exists)
                if (details.Count > 0) continue;

                var path = Path.Combine(_pathOptions.Fonts, $@"{fontChoice}", $"{range}.pbf");
                if (System.IO.File.Exists(path))
                    try
                    {
                        return new FileContentResult(System.IO.File.ReadAllBytes(path), "application/x-protobuf");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Unexpectedly could not read font file at {path}. {ex}");
                    }
            }
            if (details.Count > 0) return new RestError(400, details.ToArray()).AsJsonResult();
            return new RestError(404, new[]
            {
                new RestErrorDetails {field = "face", issue = "Font resource not found"}
            }).AsJsonResult();
        }

        // return the sprite sets available.
        [HttpGet("sprites")]
        public ActionResult GetSprites()
        {
            var webRootPath = _hostingEnv.WebRootPath;
            var spriteDir = new DirectoryInfo(Path.Combine(webRootPath, "sprites"));
            var spriteInfo = spriteDir.GetFiles("*.json", SearchOption.TopDirectoryOnly);
            // we don't want to get the names of the files that have @2x and @4x in their names, just the base names.
            var resources = spriteInfo
                .Where(f => !f.Name.Contains("@"))
                .Select(sp => new
                {
                    href = $"{GetBaseHost()}/sprites/{Path.GetFileNameWithoutExtension(sp.Name)}",
                    rel = "item",
                    title = Path.GetFileNameWithoutExtension(sp.Name)
                }).ToList();
            resources.Add(new { href = GetBaseUrl(), rel = "self", title = "self" });
            return Json(new { links = resources });
        }

        #region Internals

        private string GetBaseHost()
        {
            // if X-Forwarded-Proto or X-Forwarded-Host headers are set, use them to build the self-referencing URLs
            var proto = string.IsNullOrWhiteSpace(Request.Headers["X-Forwarded-Proto"])
                ? Request.Scheme
                : (string)Request.Headers["X-Forwarded-Proto"];
            var host = string.IsNullOrWhiteSpace(Request.Headers["X-Forwarded-Host"])
                ? Request.Host.ToUriComponent()
                : (string)Request.Headers["X-Forwarded-Host"];
            return $"{proto}://{host}";
        }

        private string GetBaseUrl()
        {
            return $"{GetBaseHost()}/{new Uri(Request.GetDisplayUrl()).GetComponents(UriComponents.Path, UriFormat.UriEscaped)}";
        }

        #endregion
    }
}

