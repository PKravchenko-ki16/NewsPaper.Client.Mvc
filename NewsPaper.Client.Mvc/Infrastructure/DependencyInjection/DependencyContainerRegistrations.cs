using Microsoft.Extensions.DependencyInjection;

namespace NewsPaper.Client.Mvc.Infrastructure.DependencyInjection
{
    public class DependencyContainerRegistrations
    {
        public static void Common(IServiceCollection services)
        {
            services.AddTransient<SetBearerTokenRequestClient>();
        }
    }
}
