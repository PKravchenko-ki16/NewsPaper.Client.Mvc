using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Calabonga.OperationResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsPaper.Client.Mvc.Infrastructure;
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
        private readonly SetBearerTokenRequestClient _retrieveToIdentityServer;

        public AccountController(IHttpClientFactory httpClientFactory, SetBearerTokenRequestClient retrieveToIdentityServer)
        {
            _httpClientFactory = httpClientFactory;
            _retrieveToIdentityServer = retrieveToIdentityServer;
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
                await _retrieveToIdentityServer.RefreshToken(model.RefreshToken, _httpClientFactory, HttpContext);
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
                await _retrieveToIdentityServer.RefreshToken(model.RefreshToken, _httpClientFactory, HttpContext);
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
                await _retrieveToIdentityServer.RefreshToken(model.RefreshToken, _httpClientFactory, HttpContext);
                model = new ClaimManager(HttpContext, User);
                listUsers = await RequestGetUsersAsync(model);
                listUsers.AddError(exception);
            }
            return View(listUsers);
        }

        private async Task<OperationResult<IEnumerable<AuthorViewModel>>> RequestGetAuthorsAsync(ClaimManager model)
        {
            HttpClient client = _retrieveToIdentityServer.RetrieveToIdentityServer(_httpClientFactory, model.AccessToken);

            HttpResponseMessage response =
                (await client.GetAsync("https://localhost:5001/api/author/getauthors/")).EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            OperationResult<IEnumerable<AuthorViewModel>> operation = JsonConvert.DeserializeObject<OperationResult<IEnumerable<AuthorViewModel>>>(responseBody);

            return operation;
        }

        private async Task<OperationResult<IEnumerable<EditorViewModel>>> RequestGetEditorsAsync(ClaimManager model)
        {
            HttpClient client = _retrieveToIdentityServer.RetrieveToIdentityServer(_httpClientFactory, model.AccessToken);

            HttpResponseMessage response =
                (await client.GetAsync("https://localhost:5001/api/editor/geteditors/")).EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            OperationResult<IEnumerable<EditorViewModel>> operation = JsonConvert.DeserializeObject<OperationResult<IEnumerable<EditorViewModel>>>(responseBody);

            return operation;
        }

        private async Task<OperationResult<IEnumerable<UserViewModel>>> RequestGetUsersAsync(ClaimManager model)
        {
            HttpClient client = _retrieveToIdentityServer.RetrieveToIdentityServer(_httpClientFactory, model.AccessToken);

            HttpResponseMessage response =
                (await client.GetAsync("https://localhost:5001/api/user/getusers/")).EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            OperationResult<IEnumerable<UserViewModel>> operation = JsonConvert.DeserializeObject<OperationResult<IEnumerable<UserViewModel>>>(responseBody);

            return operation;
        }
    }
}
