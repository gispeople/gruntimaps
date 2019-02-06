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
using GruntiMaps.Common.Exceptions;
using GruntiMaps.Domain.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace GruntiMaps.WebAPI.Filters
{
    public class DomainExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            object content = null;
            HttpStatusCode? statusCode = null;

            switch (context.Exception)
            {
                case ValidatorException ex:
                    statusCode = HttpStatusCode.BadRequest;
                    content = ex.Errors;
                    break;
                case BadRequestException _:
                    statusCode = HttpStatusCode.BadRequest;
                    content = BuildErrorResult(context.Exception.Message);
                    break;
                case EntityNotFoundException _:
                    statusCode = HttpStatusCode.NotFound;
                    break;
            }

            if (statusCode == null) return;

            context.Result = new JsonResult(content)
            {
                StatusCode = (int)statusCode.Value,
                SerializerSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented
                }
            };

            context.ExceptionHandled = true;
        }

        private static ErrorResultContent BuildErrorResult(string message)
        {
            return new ErrorResultContent
            {
                Errors = new[] { message }
            };
        }
    }
}
