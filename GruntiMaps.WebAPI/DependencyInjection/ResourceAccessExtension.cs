using System;
using GruntiMaps.Common.Enums;
using GruntiMaps.ResourceAccess.Azure;
using GruntiMaps.ResourceAccess.Local;
using GruntiMaps.ResourceAccess.Queue;
using GruntiMaps.ResourceAccess.Storage;
using GruntiMaps.ResourceAccess.Table;
using GruntiMaps.WebAPI.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GruntiMaps.WebAPI.DependencyInjection
{
    public static class ResourceAccessExtension
    {
        public static void AddResourceAccess(this IServiceCollection services)
        {
            services.AddSingleton<IMbConversionQueue>(provider =>
            {
                var options = provider.GetService<Options>();
                switch (options.StorageProvider)
                {
                    case StorageProviders.Azure:
                        return new AzureMbConversionQueue(options.StorageKey, options.StorageAccount,
                            options.MbConvQueue);
                    case StorageProviders.Local:
                        return new LocalMbConversionQueue(options.StoragePath, options.QueueTimeLimit,
                            options.QueueEntryTries, options.MbConvQueue);
                    default:
                        throw new NotImplementedException();
                }
            });
            services.AddSingleton<IGdConversionQueue>(provider =>
            {
                var options = provider.GetService<Options>();
                switch (options.StorageProvider)
                {
                    case StorageProviders.Azure:
                        return new AzureGdConversionQueue(options.StorageKey, options.StorageAccount,
                            options.GdConvQueue);
                    case StorageProviders.Local:
                        return new LocalGdConversionQueue(options.StoragePath, options.QueueTimeLimit,
                            options.QueueEntryTries, options.GdConvQueue);
                    default:
                        throw new NotImplementedException();
                }
            });
            services.AddSingleton<IPackStorage>(provider =>
            {
                var options = provider.GetService<Options>();
                switch (options.StorageProvider)
                {
                    case StorageProviders.Azure:
                        return new AzurePackStorage(options.StorageAccount, options.StorageKey,
                            options.StorageContainer, provider.GetService<ILogger>());
                    case StorageProviders.Local:
                        return new LocalPackStorage(options.StoragePath, options.StorageContainer);
                    default:
                        throw new NotImplementedException();
                }
            });
            services.AddSingleton<ITileStorage>(provider =>
            {
                var options = provider.GetService<Options>();
                switch (options.StorageProvider)
                {
                    case StorageProviders.Azure:
                        return new AzureTileStorage(options.StorageAccount, options.StorageKey,
                            options.MbTilesContainer, provider.GetService<ILogger>());
                    case StorageProviders.Local:
                        return new LocalTileStorage(options.StoragePath, options.MbTilesContainer);
                    default:
                        throw new NotImplementedException();
                }
            });
            services.AddSingleton<IGeoJsonStorage>(provider =>
            {
                var options = provider.GetService<Options>();
                switch (options.StorageProvider)
                {
                    case StorageProviders.Azure:
                        return new AzureGeoJsonStorage(options.StorageAccount, options.StorageKey,
                            options.GeoJsonContainer, provider.GetService<ILogger>());
                    case StorageProviders.Local:
                        return new LocalGeoJsonStorage(options.StoragePath, options.GeoJsonContainer);
                    default:
                        throw new NotImplementedException();
                }
            });
            services.AddSingleton<IFontStorage>(provider =>
            {
                var options = provider.GetService<Options>();
                switch (options.StorageProvider)
                {
                    case StorageProviders.Azure:
                        return new AzureFontStorage(options.StorageAccount, options.StorageKey,
                            options.FontContainer, provider.GetService<ILogger>());
                    case StorageProviders.Local:
                        return new LocalFontStorage(options.StoragePath, options.FontContainer);
                    default:
                        throw new NotImplementedException();
                }
            });
            services.AddSingleton<IStatusTable>(provider =>
            {
                var options = provider.GetService<Options>();
                switch (options.StorageProvider)
                {
                    case StorageProviders.Azure:
                        return new AzureStatusTable(options.StorageAccount, options.StorageKey,
                            options.JobStatusTable);
                    case StorageProviders.Local:
                        return new LocalStatusTable(options.StoragePath, options.JobStatusTable);
                    default:
                        throw new NotImplementedException();
                }
            });

        }
    }
}
