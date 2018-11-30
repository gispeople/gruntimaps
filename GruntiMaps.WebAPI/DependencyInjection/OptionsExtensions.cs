using GruntiMaps.Api.Common.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GruntiMaps.WebAPI.DependencyInjection
{
    public static class OptionsExtensions
    {
        public static void AddOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<QueuesOptions>(configuration.GetSection("Queues"));
        }
    }
}
