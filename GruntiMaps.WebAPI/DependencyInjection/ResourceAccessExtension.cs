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
using System;
using GruntiMaps.Api.Common.Configuration;
using GruntiMaps.Api.Common.Enums;
using GruntiMaps.ResourceAccess.Azure;
using GruntiMaps.ResourceAccess.GlobalCache;
using GruntiMaps.ResourceAccess.Local;
using GruntiMaps.ResourceAccess.Queue;
using GruntiMaps.ResourceAccess.Storage;
using GruntiMaps.ResourceAccess.Table;
using GruntiMaps.ResourceAccess.TopicSubscription;
using GruntiMaps.ResourceAccess.WorkspaceCache;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GruntiMaps.WebAPI.DependencyInjection
{
    public static class ResourceAccessExtension
    {
        public static void AddResourceAccess(this IServiceCollection services)
        {
            // Hosted Resources
            services.AddSingleton<IMbConversionQueue>(provider =>
            {
                var providerOptions = provider.GetService<IOptions<ProviderOptions>>().Value;
                var queue = provider.GetService<IOptions<QueueOptions>>().Value.MvtConversion;
                switch (providerOptions.Type)
                {
                    case ProviderType.Azure:
                        return new AzureMbConversionQueue(providerOptions.Azure.ConnectionString, queue);
                    case ProviderType.Local:
                        var local = providerOptions.Local;
                        return new LocalMbConversionQueue(local.Path, local.QueueTimeLimit,
                            local.QueueEntryLife, queue);
                    default:
                        throw new NotImplementedException();
                }
            });
            services.AddSingleton<IGdConversionQueue>(provider =>
            {
                var providerOptions = provider.GetService<IOptions<ProviderOptions>>().Value;
                var queue = provider.GetService<IOptions<QueueOptions>>().Value.GdalConversion;
                switch (providerOptions.Type)
                {
                    case ProviderType.Azure:
                        return new AzureGdConversionQueue(providerOptions.Azure.ConnectionString, queue);
                    case ProviderType.Local:
                        var local = providerOptions.Local;
                        return new LocalGdConversionQueue(local.Path, local.QueueTimeLimit,
                            local.QueueEntryLife, queue);
                    default:
                        throw new NotImplementedException();
                }
            });
            services.AddSingleton<IPackStorage>(provider =>
            {
                var providerOptions = provider.GetService<IOptions<ProviderOptions>>().Value;
                var container = provider.GetService<IOptions<ContainerOptions>>().Value.Packs;
                switch (providerOptions.Type)
                {
                    case ProviderType.Azure:
                        return new AzurePackStorage(providerOptions.Azure.ConnectionString,
                            container, provider.GetService<ILogger>());
                    case ProviderType.Local:
                        return new LocalPackStorage(providerOptions.Local.Path, container);
                    default:
                        throw new NotImplementedException();
                }
            });
            services.AddSingleton<ITileStorage>(provider =>
            {
                var providerOptions = provider.GetService<IOptions<ProviderOptions>>().Value;
                var container = provider.GetService<IOptions<ContainerOptions>>().Value.MbTiles;
                switch (providerOptions.Type)
                {
                    case ProviderType.Azure:
                        return new AzureTileStorage(providerOptions.Azure.ConnectionString,
                            container, provider.GetService<ILogger>());
                    case ProviderType.Local:
                        return new LocalTileStorage(providerOptions.Local.Path, container);
                    default:
                        throw new NotImplementedException();
                }
            });
            services.AddSingleton<IStyleStorage>(provider =>
            {
                var providerOptions = provider.GetService<IOptions<ProviderOptions>>().Value;
                var container = provider.GetService<IOptions<ContainerOptions>>().Value.Styles;
                switch (providerOptions.Type)
                {
                    case ProviderType.Azure:
                        return new AzureStyleStorage(providerOptions.Azure.ConnectionString,
                            container, provider.GetService<ILogger>());
                    case ProviderType.Local:
                        return new LocalStyleStorage(providerOptions.Local.Path, container);
                    default:
                        throw new NotImplementedException();
                }
            });
            services.AddSingleton<IGeoJsonStorage>(provider =>
            {
                var providerOptions = provider.GetService<IOptions<ProviderOptions>>().Value;
                var container = provider.GetService<IOptions<ContainerOptions>>().Value.Geojsons;
                switch (providerOptions.Type)
                {
                    case ProviderType.Azure:
                        return new AzureGeoJsonStorage(providerOptions.Azure.ConnectionString,
                            container, provider.GetService<ILogger>());
                    case ProviderType.Local:
                        return new LocalGeoJsonStorage(providerOptions.Local.Path, container);
                    default:
                        throw new NotImplementedException();
                }
            });
            services.AddSingleton<IFontStorage>(provider =>
            {
                var providerOptions = provider.GetService<IOptions<ProviderOptions>>().Value;
                var container = provider.GetService<IOptions<ContainerOptions>>().Value.Fonts;
                switch (providerOptions.Type)
                {
                    case ProviderType.Azure:
                        return new AzureFontStorage(providerOptions.Azure.ConnectionString,
                            container, provider.GetService<ILogger>());
                    case ProviderType.Local:
                        return new LocalFontStorage(providerOptions.Local.Path, container);
                    default:
                        throw new NotImplementedException();
                }
            });
            services.AddSingleton<IStatusTable>(provider =>
            {
                var providerOptions = provider.GetService<IOptions<ProviderOptions>>().Value;
                var table = provider.GetService<IOptions<TableOptions>>().Value.JobStatuses;
                switch (providerOptions.Type)
                {
                    case ProviderType.Azure:
                        return new AzureStatusTable(providerOptions.Azure.ConnectionString, table);
                    case ProviderType.Local:
                        return new LocalStatusTable(providerOptions.Local.Path, table);
                    default:
                        throw new NotImplementedException();
                }
            });

            // Local Cache (Workspace)
            services.AddSingleton<IWorkspaceTileCache, WorkspaceTileCache>();
            services.AddSingleton<IWorkspaceStyleCache, WorkspaceStyleCache>();
            services.AddSingleton<IWorkspacePackCache, WorkspacePackCache>();

            // Local Cache (Global)
            services.AddSingleton<IGlobalFontCache, GlobalFontCache>();

            // Service Bus Topic Subscription
            services.AddSingleton<IMapLayerUpdateTopicClient>(provider =>
            {
                var providerOptions = provider.GetService<IOptions<ProviderOptions>>().Value;
                switch (providerOptions.Type)
                {
                    case ProviderType.Azure:
                        return new AzureMapLayerUpdateTopicClient(providerOptions.Azure.ServiceBus.ConnectionString, providerOptions.Azure.ServiceBus.Topic);
                    default:
                        throw new NotImplementedException();
                }
            });

            services.AddSingleton<IMapLayerUpdateSubscriptionClient>(provider =>
            {
                var providerOptions = provider.GetService<IOptions<ProviderOptions>>().Value;
                switch (providerOptions.Type)
                {
                    case ProviderType.Azure:
                        return new AzureMapLayerUpdateSubscriptionClient(providerOptions.Azure.ServiceBus.ConnectionString, 
                            providerOptions.Azure.ServiceBus.Topic, 
                            providerOptions.Azure.ServiceBus.Subscription,
                            provider.GetService<ILogger>());
                    default:
                        throw new NotImplementedException();
                }
            });
        }
    }
}
