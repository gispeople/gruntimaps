using GruntiMaps.ResourceAccess.GlobalCache;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers.Fonts
{
    public class GetFontController : FontControllerBase
    {
        private readonly IGlobalFontCache _cache;

        public GetFontController(IGlobalFontCache cache)
        {
            _cache = cache;
        }

        [Route("{face}/{range}")]
        public FileContentResult Invoke(string face, string range)
        {
            return new FileContentResult(_cache.GetFileContent(face, range), "application/x-protobuf");
        }
    }
}
