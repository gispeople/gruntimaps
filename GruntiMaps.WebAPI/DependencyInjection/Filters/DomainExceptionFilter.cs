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
