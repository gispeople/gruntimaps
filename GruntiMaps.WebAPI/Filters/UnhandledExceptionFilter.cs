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
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GruntiMaps.WebAPI.Filters
{
    /// <summary>
    /// Filter to return exception and stack trace in response.
    /// </summary>
    public class UnhandledExceptionFilter : IExceptionFilter
    {
        private readonly bool _returnExceptionDetails;

        /// <param name="returnExceptionDetails">Whether to return the exception stack trace. 
        /// Should not be enabled in production</param>
        public UnhandledExceptionFilter(bool returnExceptionDetails)
        {
            _returnExceptionDetails = returnExceptionDetails;
        }

        public void OnException(ExceptionContext context)
        {
            var content = _returnExceptionDetails
                ? context.Exception.ToString()
                : "Internal Error";

            context.Result = new JsonResult(new ErrorResultContent
            {
                Errors = new[] {content}
            })
            {
                StatusCode = (int) HttpStatusCode.InternalServerError,
            };

            context.ExceptionHandled = true;
        }
    }
}
