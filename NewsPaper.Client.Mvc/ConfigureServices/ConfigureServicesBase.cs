using Microsoft.Extensions.DependencyInjection;

namespace NewsPaper.Client.Mvc.ConfigureServices
{
    public class ConfigureServicesBase
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
        }
    }
}
