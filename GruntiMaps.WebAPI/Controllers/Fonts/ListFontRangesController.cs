using GruntiMaps.ResourceAccess.GlobalCache;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers.Fonts
{
    public class ListFontRangesController : FontControllerBase
    {
        private readonly IGlobalFontCache _cache;

        public ListFontRangesController(IGlobalFontCache cache)
        {
            _cache = cache;
        }

        [Route("{face}")]
        public string[] Invoke(string face)
        {
            return _cache.ListFontRanges(face);
        }
    }
}
