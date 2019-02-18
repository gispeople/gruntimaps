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

        public UrlGenerator(IActionContextAccessor actionAccessor)
        {
            _actionAccessor = actionAccessor;
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
