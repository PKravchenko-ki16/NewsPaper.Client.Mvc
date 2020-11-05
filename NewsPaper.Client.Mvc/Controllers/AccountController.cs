using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Calabonga.OperationResults;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsPaper.Client.Mvc.ViewModels;
using NewsPaper.Client.Mvc.ViewModels.Author;
using NewsPaper.Client.Mvc.ViewModels.Editor;
using NewsPaper.Client.Mvc.ViewModels.User;
using Newtonsoft.Json;

namespace NewsPaper.Client.Mvc.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> GetAuthors()
        {
            var model = new ClaimManager(HttpContext, User);
            OperationResult<IEnumerable<AuthorViewModel>> listAuthors;
            try
            {
                listAuthors = await RequestGetAuthorsAsync(model);
                return View(listAuthors);
            }
            catch (Exception exception)
            {
                await RefreshToken(model.RefreshToken);
                model = new ClaimManager(HttpContext, User);
                listAuthors = await RequestGetAuthorsAsync(model);
                listAuthors.AddError(exception);
            }
            return View(listAuthors);
        }

        [Authorize]
        public async Task<IActionResult> GetEditors()
        {
            var model = new ClaimManager(HttpContext, User);
            OperationResult<IEnumerable<EditorViewModel>> listEditors;
            try
            {
                listEditors = await RequestGetEditorsAsync(model);
                return View(listEditors);
            }
            catch (Exception exception)
            {
                await RefreshToken(model.RefreshToken);
                model = new ClaimManager(HttpContext, User);
                listEditors = await RequestGetEditorsAsync(model);
                listEditors.AddError(exception);
            }
            return View(listEditors);
        }

        [Authorize]
        public async Task<IActionResult> GetUsers()
        {
            var model = new ClaimManager(HttpContext, User);
            OperationResult<IEnumerable<UserViewModel>> listUsers;
            try
            {
                listUsers = await RequestGetUsersAsync(model);
                return View(listUsers);
            }
            catch (Exception exception)
            {
                await RefreshToken(model.RefreshToken);
                model = new ClaimManager(HttpContext, User);
                listUsers = await RequestGetUsersAsync(model);
                listUsers.AddError(exception);
            }
            return View(listUsers);
        }

        private async Task<OperationResult<IEnumerable<AuthorViewModel>>> RequestGetAuthorsAsync(ClaimManager model)
        {
            var clientAuthor = _httpClientFactory.CreateClient();
            clientAuthor.SetBearerToken(model.AccessToken);

            HttpResponseMessage response =
                (await clientAuthor.GetAsync("https://localhost:5001/api/author/getauthors/")).EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            OperationResult<IEnumerable<AuthorViewModel>> operation = JsonConvert.DeserializeObject<OperationResult<IEnumerable<AuthorViewModel>>>(responseBody);

            return operation;
        }

        private async Task<OperationResult<IEnumerable<EditorViewModel>>> RequestGetEditorsAsync(ClaimManager model)
        {
            var clientEditor = _httpClientFactory.CreateClient();
            clientEditor.SetBearerToken(model.AccessToken);

            HttpResponseMessage response =
                (await clientEditor.GetAsync("https://localhost:5001/api/editor/geteditors/")).EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            OperationResult<IEnumerable<EditorViewModel>> operation = JsonConvert.DeserializeObject<OperationResult<IEnumerable<EditorViewModel>>>(responseBody);

            return operation;
        }

        private async Task<OperationResult<IEnumerable<UserViewModel>>> RequestGetUsersAsync(ClaimManager model)
        {
            var clientUser = _httpClientFactory.CreateClient();
            clientUser.SetBearerToken(model.AccessToken);

            HttpResponseMessage response =
                (await clientUser.GetAsync("https://localhost:5001/api/user/getusers/")).EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            OperationResult<IEnumerable<UserViewModel>> operation = JsonConvert.DeserializeObject<OperationResult<IEnumerable<UserViewModel>>>(responseBody);

            return operation;
        }

        private async Task RefreshToken(string refreshToken)
        {
            var refreshClient = _httpClientFactory.CreateClient();
            var resultRefreshTokenAsync = await refreshClient.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = "https://localhost:10001/connect/token",
                ClientId = "client_id_mvc",
                ClientSecret = "client_secret_mvc",
                RefreshToken = refreshToken,
                Scope = "openid OrdersAPI offline_access"
            });

            await UpdateAuthContextAsync(resultRefreshTokenAsync.AccessToken, resultRefreshTokenAsync.RefreshToken);
        }

        private async Task UpdateAuthContextAsync(string accessTokenNew, string refreshTokenNew)
        {
            var authenticate = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            authenticate.Properties.UpdateTokenValue("access_token", accessTokenNew);
            authenticate.Properties.UpdateTokenValue("refresh_token", refreshTokenNew);

            await HttpContext.SignInAsync(authenticate.Principal, authenticate.Properties);
        }
    }
}
