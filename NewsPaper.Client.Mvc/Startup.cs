using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NewsPaper.Client.Mvc.Configure;
using NewsPaper.Client.Mvc.ConfigureServices;
using NewsPaper.Client.Mvc.Infrastructure.DependencyInjection;

namespace NewsPaper.Client.Mvc
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureServicesBase.ConfigureServices(services);
            ConfigureServicesAuthentication.ConfigureService(services);
            ConfigureServicesControllers.ConfigureServices(services);
            DependencyContainerRegistrations.Common(services);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ConfigureCommon.Configure(app,env);
            ConfigureEndpoints.Configure(app);
        }
    }
}
