using Microsoft.Extensions.DependencyInjection;

namespace NewsPaper.Client.Mvc.ConfigureServices
{
    public class ConfigureServicesControllers
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
        }
    }
}
