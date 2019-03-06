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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers
{
    [Produces("application/json")]
    [Route("api")]
    public class RestApiController : Controller
    {
        private readonly IHostingEnvironment _hostingEnv;

        public RestApiController(IHostingEnvironment hostingEnv)
        {
            _hostingEnv = hostingEnv;
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

