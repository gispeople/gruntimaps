using GruntiMaps.ResourceAccess.GlobalCache;

namespace GruntiMaps.WebAPI.Controllers.Fonts
{
    public class ListFontFacesController : FontControllerBase
    {
        private readonly IGlobalFontCache _cache;

        public ListFontFacesController(IGlobalFontCache cache)
        {
            _cache = cache;
        }

        public string[] Invoke()
        {
            return _cache.ListFontFaces();
        }
    }
}
