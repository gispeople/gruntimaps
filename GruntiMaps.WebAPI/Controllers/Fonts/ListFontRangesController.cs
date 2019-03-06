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

using System.Threading.Tasks;
using GruntiMaps.ResourceAccess.GlobalCache;
using GruntiMaps.WebAPI.Domain.Validators;
using Microsoft.AspNetCore.Mvc;

namespace GruntiMaps.WebAPI.Controllers.Fonts
{
    public class ListFontRangesController : FontControllerBase
    {
        private readonly IGlobalFontCache _cache;
        private readonly IFontFaceValidator _faceValidator;

        public ListFontRangesController(IGlobalFontCache cache,
            IFontFaceValidator faceValidator)
        {
            _cache = cache;
            _faceValidator = faceValidator;
        }

        [Route("{face}")]
        public async Task<string[]> Invoke(string face)
        {
            await _faceValidator.Validate(face);
            return _cache.ListFontRanges(face);
        }
    }
}
