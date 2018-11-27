using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace GruntiMaps.Api.Common.Services
{
    public interface IUrlGenerator
    {
        string BuildUrl(string routeName, object values = null);
    }

    public class UrlGenerator : IUrlGenerator
    {
        private readonly IActionContextAccessor _actionAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public UrlGenerator(IActionContextAccessor actionAccessor, IUrlHelperFactory urlHelperFactory)
        {
            _actionAccessor = actionAccessor;
            _urlHelperFactory = urlHelperFactory;
        }

        public string BuildUrl(string routeName, object values)
        {
            var request = _actionAccessor.ActionContext.HttpContext.Request;
            var proto = string.IsNullOrWhiteSpace(request.Headers["X-Forwarded-Proto"])
                ? request.Scheme
                : (string)request.Headers["X-Forwarded-Proto"];
            var host = string.IsNullOrWhiteSpace(request.Headers["X-Forwarded-Host"])
                ? request.Host.ToUriComponent()
                : (string)request.Headers["X-Forwarded-Host"];
            return new UrlHelper(_actionAccessor.ActionContext)
                .RouteUrl(new UrlRouteContext()
            {
                Protocol = proto,
                Host = host,
                RouteName = routeName,
                Values = values
            });
        }
    }
}
