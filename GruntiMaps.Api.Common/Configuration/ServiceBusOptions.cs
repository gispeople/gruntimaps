namespace GruntiMaps.Api.Common.Configuration
{
    public class ServiceBusOptions
    {
        public string ConnectionString { get; set; }
        public string Topic { get; set; }
        public string Subscription { get; set; }
    }
}
