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
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using GruntiMaps.WebAPI.Interfaces;
using GruntiMaps.WebAPI.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace GruntiMaps.WebAPI.Controllers
{
    [Produces("application/json")]
    [Route("api")]
    public class RestApiController : Controller
    {
        private readonly IMapData _mapData;
        private readonly Options _options;
        private readonly IHostingEnvironment _hostingEnv;

        public RestApiController(IMapData mapData, Options options, IHostingEnvironment hostingEnv)
        {
            _options = options;
            _mapData = mapData;
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

        // RESTful layer list
        [HttpGet("layers")]
        public ActionResult GetCatalog()
        {
            string baseUrl = GetBaseUrl();

            var layerResources = new List<object>();

            foreach (var layer in _mapData.LayerDict)
            {
                layerResources.Add(new
                {
                    name = layer.Value.Source.description,
                    links = new
                    {
                        href = $"{baseUrl}/{layer.Key}",
                        rel = "self"
                    }
                });
            }

            return Json(new
            {
                content = layerResources,
                links = new { href = $"{baseUrl}", rel = "self" }
            });
        }

        // RESTful layer information
        [HttpGet("layers/{service}")]
        public ActionResult GetService(string service)
        {
            var baseUrl = $"{GetBaseHost()}/api/layers";
            if (!_mapData.LayerDict.ContainsKey(service))
            {
                return new RestError(404, new[] {
                    new RestErrorDetails { field = "service", issue = "Service does not exist" }
                }).AsJsonResult();
            }
            return Json(new
            {
                source = $"{baseUrl}/source/{service}",
                style = $"{baseUrl}/style/{service}",
                mappack = $"{baseUrl}/mappack/{service}",
                tiles = $"{baseUrl}/tiles/{service}",
                grid = $"{baseUrl}/grid/{service}",
                metadata = $"{baseUrl}/metadata/{service}",
                geojson = $"{baseUrl}/geojson/{service}",
                links = new List<object> {
                    new { href = $"{baseUrl}/{service}", rel = "self" }
                }
            });
        }

        [HttpPost("layers/{service}")]
        public ActionResult CreateLayer(string service)
        {
            return Json(new { });
        }

        // RESTful retrieve offline map pack.
        [HttpGet("layers/mappack/{service}")]
        public ActionResult MapPack(string service)
        {
            if (string.IsNullOrEmpty(service))
            {
                return new RestError(400, new[] {
                    new RestErrorDetails { field = "service", issue = "Service name must be supplied" }
                }).AsJsonResult();
            }

            var zipFileName = service;
            if (!zipFileName.EndsWith(".zip", true, CultureInfo.CurrentCulture)) zipFileName += ".zip";
            var path = Path.Combine(_options.PackPath, $@"{zipFileName}");
            if (!System.IO.File.Exists(path))
                return new RestError(404, new[] {
                    new RestErrorDetails { field = "service", issue = "Service does not exist" }
                }).AsJsonResult();
            var result = new FileContentResult(System.IO.File.ReadAllBytes(path), "application/zip")
            {
                // assign a file name to the download
                FileDownloadName = $"{zipFileName}"
            };
            return result;
        }

        // Retrieve tile. 
        [HttpGet("layers/tiles/{service}")]
        public ActionResult Tile(string service, int? x, int? y, byte? z)
        {
            if (!x.HasValue || !y.HasValue || !z.HasValue)
                return new RestError(400, IdentifyMissingCoordinates(x, y, z)).AsJsonResult();
            var bytes = GetTile(service, x, y, z);
            switch (_mapData.LayerDict[service].Source.format) {
                case "png": return File(bytes, "image/png"); 
                case "jpg": return File(bytes, "image/jpg"); 
                case "pbf": return File(Decompress(bytes), "application/vnd.mapbox-vector-tile");
            }
            return new RestError(400, IdentifyMissingCoordinates(x, y, z)).AsJsonResult();
        }

        private static RestErrorDetails[] IdentifyMissingCoordinates(int? x, int? y, byte? z)
        {
            var details = new List<RestErrorDetails>();
            if (!x.HasValue) details.Add(new RestErrorDetails { field = "x", issue = "x parameter must be supplied" });
            if (!y.HasValue) details.Add(new RestErrorDetails { field = "y", issue = "y parameter must be supplied" });
            if (!z.HasValue) details.Add(new RestErrorDetails { field = "z", issue = "z parameter must be supplied" });
            return details.ToArray();
        }

        // Get a mapbox grid.
        [HttpGet("layers/grid/{service}")]
        public ActionResult Grid(string service, int? x, int? y, byte? z)
        {
            if (x.HasValue && y.HasValue && z.HasValue) return Content(GetGrid(service, x, y, z), "application/json");
            return new RestError(400, IdentifyMissingCoordinates(x, y, z)).AsJsonResult();

        }

        // Retrieve the style snippet for this map service. 
        [HttpGet("layers/style/{service}")]
        public ActionResult Style(string service)
        {
            if (!_mapData.LayerDict.ContainsKey(service))
                return new RestError(404, new[] {
                    new RestErrorDetails{ field = "service", issue = "Service does not exist" }
                }).AsJsonResult();
            return Content(JsonPrettify(JsonConvert.SerializeObject(_mapData.LayerDict[service].Style)),
                "application/json");
        }

        // Retrieve the data json to help with setting up initial styling
        [HttpGet("layers/metadata/{service}")]
        public ActionResult DataJson(string service)
        {
            if (!_mapData.LayerDict.ContainsKey(service))
                return new RestError(404, new[] {
                    new RestErrorDetails{ field = "service", issue = "Service does not exist" }
                }).AsJsonResult();
            return Content(JsonPrettify(_mapData.LayerDict[service].DataJson.ToString()), "application/json");
        }

        // Retrieve a json snippet that defines the mapbox 'source' for this service
        [HttpGet("layers/source/{service}")]
        public ActionResult Source(string service)
        {
            if (GetBaseUrl() == null || service == null || !_mapData.LayerDict.ContainsKey(service))
                return new RestError(404, new[] {
                    new RestErrorDetails{ field = "service", issue = "Service does not exist" }
                }).AsJsonResult();
            var src = _mapData.LayerDict[service].Source;
            src.tiles[0] = src.tiles[0].Replace("#publicHost#", GetBaseHost());
            return Content(JsonConvert.SerializeObject(
                src,
                Formatting.Indented,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), "application/json");
        }

        // Retrieve the GeoJSON associated with this service
        [HttpGet("layers/geojson/{service}")]
        public ActionResult GeoJson(string service)
        {
            // not yet implemented
            return Json(new { });
        }

        [HttpPost("layers/create")]
        public async Task<ActionResult> CreateNewLayer([FromBody] CreateMapLayerDto dto)
        {
            ConversionMessageData messageData = new ConversionMessageData
            {
                LayerName = dto.Name,
                DataLocation = dto.DataLocation,
                Description = dto.Description
            };
            var requestId = await _mapData.CreateGdalConversionRequest(messageData);
            await _mapData.JobStatusTable.AddStatus(requestId, requestId);
            return Json(new
            {
                requestId
            });
        }

        [HttpGet("layers/create/{jobId}")]
        public async Task<ActionResult> GetCreationJobStatus(string jobId) 
        {
            var jobStatus = await _mapData.JobStatusTable.GetStatus(jobId);
            var status = jobStatus.HasValue ? jobStatus.ToString() : null;
            return Json(new
            {
                status
            });
        }

        // Retrieve a list of all available fonts.
        [HttpGet("fonts")]
        public ActionResult GetFonts()
        {
            var fontDir = new DirectoryInfo(_options.FontPath);
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

            var rangeDir = new DirectoryInfo(Path.Combine(_options.FontPath, faceFile));
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

                var path = Path.Combine(_options.FontPath, $@"{fontChoice}", $"{range}.pbf");
                if (System.IO.File.Exists(path))
                    return new FileContentResult(System.IO.File.ReadAllBytes(path), "application/x-protobuf");
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

        public static string JsonPrettify(string json)
        {
            if (json == null)
            {
                return "{}";
            }

            using (var stringReader = new StringReader(json))
            using (var stringWriter = new StringWriter())
            {
                var jsonReader = new JsonTextReader(stringReader);
                var jsonWriter = new JsonTextWriter(stringWriter) { Formatting = Formatting.Indented };
                jsonWriter.WriteToken(jsonReader);
                return stringWriter.ToString();
            }
        }

        // Get a tile from a mapbox tile database.
        private byte[] GetTile(string service, int? x, int? y, byte? z)
        {
            //validate input vars
            if (x == null || y == null || z == null || service == null || !_mapData.LayerDict.ContainsKey(service))
                return new byte[] { 0 };

            y = IntPow(2, (byte)z) - 1 - y;

            var conn = _mapData.LayerDict[service].Conn;

            using (var cmd = conn.CreateCommand())
            {
                var command =
                    $"select tile_data as t from tiles where zoom_level={z} and tile_column={x} and tile_row={y}";
                cmd.CommandText = command;

                var result = (byte[])cmd.ExecuteScalar();

                return result ?? new byte[] { 0 };
            }
        }

        // Get a grid from the database.
        private string GetGrid(string service, int? x, int? y, byte? z)
        {
            //validate input vars
            if (x == null || y == null || z == null || service == null ||
                !_mapData.LayerDict.ContainsKey(service)) return "{}";

            y = IntPow(2, (byte)z) - 1 - y;

            var conn = _mapData.LayerDict[service].Conn;

            using (var cmd = conn.CreateCommand())
            {
                try
                {
                    var command =
                        $"select grid as g from grids where zoom_level={z} and tile_column={x} and tile_row={y}";
                    cmd.CommandText = command;

                    var b = (byte[])cmd.ExecuteScalar();

                    if (b.Length == 0) return "{}";

                    var grid = Decompress(b);

                    var g = Encoding.UTF8.GetString(grid);

                    g = g.Substring(0, g.Length - 1);
                    g += ", \"data\":{";

                    var query =
                        $"SELECT key_name as key, key_json as json from grid_data where zoom_level={z} and tile_column={x} and tile_row={y}";

                    using (var keyCmd = new SqliteCommand(query, conn))
                    {
                        using (var rdr = keyCmd.ExecuteReader())
                        {
                            while (rdr.Read()) g += "\"" + rdr.GetString(0) + "\":" + rdr.GetString(1) + ",";
                        }
                    }

                    g = g.Trim(',') + "}}";
                    return g;
                }
                catch (SqliteException)
                {
                    // most likely there was no grid data for this layer.
                    return "";
                }
            }
        }

        private static int IntPow(int x, byte pow)
        {
            var ret = 1;
            while (pow != 0)
            {
                if ((pow & 1) == 1)
                    ret *= x;
                x *= x;
                pow >>= 1;
            }

            return ret;
        }

        private static byte[] Decompress(byte[] zLibCompressedBuffer)
        {
            byte[] resBuffer;

            if (zLibCompressedBuffer.Length <= 1)
                return zLibCompressedBuffer;

            var mInStream = new MemoryStream(zLibCompressedBuffer);
            var mOutStream = new MemoryStream(zLibCompressedBuffer.Length);
            var infStream = new GZipStream(mInStream, CompressionMode.Decompress);

            mInStream.Position = 0;

            try
            {
                infStream.CopyTo(mOutStream);

                resBuffer = mOutStream.ToArray();
            }
            finally
            {
                infStream.Flush();
                mInStream.Flush();
                mOutStream.Flush();
            }

            return resBuffer;
        }

        
        private string GetBaseHost()
        {
            // if X-Forwarded-Proto or X-Forwarded-Host headers are set, use them to build the self-referencing URLs
            var proto = string.IsNullOrWhiteSpace(Request.Headers["X-Forwarded-Proto"])
                ? Request.Scheme
                :(string) Request.Headers["X-Forwarded-Proto"];
            var host = string.IsNullOrWhiteSpace(Request.Headers["X-Forwarded-Host"])
                ? Request.Host.ToUriComponent()
                : (string) Request.Headers["X-Forwarded-Host"];
            return $"{proto}://{host}";
        }

        private string GetBaseUrl()
        {
            return $"{GetBaseHost()}/{new Uri(Request.GetDisplayUrl()).GetComponents(UriComponents.Path, UriFormat.UriEscaped)}";
        }

        #endregion
    }
}

