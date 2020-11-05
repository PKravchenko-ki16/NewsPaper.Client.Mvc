using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace NewsPaper.Client.Mvc.ConfigureServices
{
    public class ConfigureServicesAuthentication
    {
        public static void ConfigureService(IServiceCollection services)
        {
            services.AddAuthentication(config =>
                {
                    config.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    config.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, config =>
                {
                    config.Authority = "https://localhost:10001";
                    config.ClientId = "client_id_mvc";
                    config.ClientSecret = "client_secret_mvc";
                    config.SaveTokens = true;
                    config.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false
                    };

                    config.ResponseType = "code";

                    config.Scope.Add("OrdersAPI");
                    config.Scope.Add("offline_access");

                    config.GetClaimsFromUserInfoEndpoint = true;

                    config.ClaimActions.MapJsonKey(ClaimTypes.DateOfBirth, ClaimTypes.DateOfBirth);
                });

            services.AddAuthorization(config =>
            {
                config.AddPolicy("HasDateOfBirth", builder =>
                {
                    builder.RequireClaim(ClaimTypes.DateOfBirth);
                });
            });
        }
    }
}
