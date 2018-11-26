using Gruntify.Api.Common.Services;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace GruntiMaps.WebAPI.DependencyInjection
{
    public static class DomainServiceExtensions
    {
        public static void AddDomainServices(this IServiceCollection services)
        {
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IUrlGenerator, UrlGenerator>();
            services.AddSingleton<IResourceLinksGenerator, ResourceLinksGenerator>();
        }
    }
}
