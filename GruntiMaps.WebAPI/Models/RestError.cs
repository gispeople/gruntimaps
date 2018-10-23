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

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable InconsistentNaming as the names have to match our intended JSON

namespace GruntiMaps.WebAPI.Models
{
    internal class RestError
    {
        private class HttpErr
        {
            public string name;
            public string message;
        }
        private readonly Dictionary<int, HttpErr> _errs = new Dictionary<int, HttpErr>
        {
            {
                404,
                new HttpErr { name = "RESOURCE_NOT_FOUND", message = "The specified resource does not exist" }
            },
            {
                400, 
                new HttpErr { name = "INVALID_REQUEST", message = "Request is not well-formed, syntactically incorrect, or violates schema" }
            }
        };

        private readonly int code;
        public string name;
        public string message;
        public string information_link;
        public RestErrorDetails[] details;

        public RestError(int number, RestErrorDetails[] newDetails)
        {
            code = number;
            name = _errs[number].name;
            message = _errs[number].message;
            information_link = "https://www.gruntimaps.com";
            details = newDetails;
        }

        public JsonResult AsJsonResult()
        {
            var result = new JsonResult(this) {StatusCode = code};
            return result;
        }
    }
}
