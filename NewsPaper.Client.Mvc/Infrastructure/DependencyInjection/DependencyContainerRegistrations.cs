using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NewsPaper.Client.Mvc.Infrastructure.Auth;

namespace NewsPaper.Client.Mvc.Infrastructure.DependencyInjection
{
    public class DependencyContainerRegistrations
    {
        public static void Common(IServiceCollection services)
        {
            services.AddTransient<SetBearerTokenRequestClient>();
            services.AddSingleton<IAuthorizationHandler, AccessForAuthorRequirementHandler>();
            services.AddSingleton<IAuthorizationHandler, AccessForEditorRequirementHandler>();
        }
    }
}
