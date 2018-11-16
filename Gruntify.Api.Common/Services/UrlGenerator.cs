using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System;

namespace Gruntify.Api.Common.Services
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
            => _urlHelperFactory.GetUrlHelper(_actionAccessor.ActionContext).Link(routeName, values);
    }
}
