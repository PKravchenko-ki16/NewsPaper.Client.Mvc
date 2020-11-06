using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace NewsPaper.Client.Mvc.Infrastructure
{
    public class SetBearerTokenRequestClient
    {
        public HttpClient RetrieveToIdentityServer(IHttpClientFactory httpClientFactory, string accessToken)
        {
            var client = httpClientFactory.CreateClient();

            client.SetBearerToken(accessToken);

            return client;
        }

        public async Task RefreshToken(string refreshToken, IHttpClientFactory httpClientFactory, HttpContext httpContext)
        {
            var refreshClient = httpClientFactory.CreateClient();

            var discoveryDocument = await refreshClient.GetDiscoveryDocumentAsync("https://localhost:10001");

            var resultRefreshTokenAsync = await refreshClient.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = discoveryDocument.TokenEndpoint,
                ClientId = "client_id_mvc",
                ClientSecret = "client_secret_mvc",
                RefreshToken = refreshToken,
                Scope = "openid OrdersAPI offline_access"
            });

            await UpdateAuthContextAsync(resultRefreshTokenAsync.AccessToken, resultRefreshTokenAsync.RefreshToken, httpContext);
        }

        private async Task UpdateAuthContextAsync(string accessTokenNew, string refreshTokenNew, HttpContext httpContext)
        {
            var authenticate = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            authenticate.Properties.UpdateTokenValue("access_token", accessTokenNew);
            authenticate.Properties.UpdateTokenValue("refresh_token", refreshTokenNew);

            await httpContext.SignInAsync(authenticate.Principal, authenticate.Properties);
        }
    }
}