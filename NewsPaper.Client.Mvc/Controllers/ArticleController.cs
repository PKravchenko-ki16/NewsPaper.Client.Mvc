using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Calabonga.OperationResults;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsPaper.Client.Mvc.ViewModels;
using NewsPaper.Client.Mvc.ViewModels.Article;
using Newtonsoft.Json;

namespace NewsPaper.Client.Mvc.Controllers
{
    public class ArticleController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ArticleController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> GetArticles()
        {
            var model = new ClaimManager(HttpContext, User);
            OperationResult<IEnumerable<ArticleViewModel>> listArticle;
            try
            {
                listArticle = await RequestGetAtriclesAsync(model);
                return View(listArticle);
            }
            catch(Exception exception)
            {
                await RefreshToken(model.RefreshToken);
                model = new ClaimManager(HttpContext, User);
                listArticle = await RequestGetAtriclesAsync(model);
                listArticle.AddError(exception);
            }
            return View(listArticle);
        }

        [Authorize]
        public async Task<IActionResult> GetArticleById(Guid articleGuid)
        {
            var model = new ClaimManager(HttpContext, User);
            OperationResult<ArticleViewModel> article;
            try
            {
                article = await RequestGetArticleByIdAsync(model, articleGuid);
                return View(article);
            }
            catch (Exception exception)
            {
                await RefreshToken(model.RefreshToken);
                model = new ClaimManager(HttpContext, User);
                article = await RequestGetArticleByIdAsync(model, articleGuid);
                article.AddError(exception);
            }
            return View(article);
        }

        [Authorize]
        public async Task<IActionResult> GetArticlesByIdAuthor(string authorNikeName)
        {
            var model = new ClaimManager(HttpContext, User);
            OperationResult<IEnumerable<ArticleViewModel>> listArticle;
            try
            {
                listArticle = await RequestGetArticlesByIdAuthorAsync(model, authorNikeName);
                return View("GetArticles",listArticle);
            }
            catch (Exception exception)
            {
                await RefreshToken(model.RefreshToken);
                model = new ClaimManager(HttpContext, User);
                listArticle = await RequestGetArticlesByIdAuthorAsync(model, authorNikeName);
                listArticle.AddError(exception);
            }
            return View("GetArticles", listArticle);
        }

        private async Task<OperationResult<IEnumerable<ArticleViewModel>>> RequestGetAtriclesAsync(ClaimManager model)
        {
            var client = _httpClientFactory.CreateClient();
            client.SetBearerToken(model.AccessToken);

            HttpResponseMessage response =
                (await client.GetAsync("https://localhost:5001/api/article/getarticles/")).EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            OperationResult<IEnumerable<ArticleViewModel>> operation = JsonConvert.DeserializeObject<OperationResult<IEnumerable<ArticleViewModel>>>(responseBody);

            return operation;
        }

        private async Task<OperationResult<ArticleViewModel>> RequestGetArticleByIdAsync(ClaimManager model, Guid articleGuid)
        {
            var client = _httpClientFactory.CreateClient();
            client.SetBearerToken(model.AccessToken);

            HttpResponseMessage response =
                (await client.GetAsync($"https://localhost:5001/api/article/getarticlebyid?articleGuid={articleGuid}")).EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            OperationResult<ArticleViewModel> operation = JsonConvert.DeserializeObject<OperationResult<ArticleViewModel>>(responseBody);

            return operation;
        }

        private async Task<OperationResult<IEnumerable<ArticleViewModel>>> RequestGetArticlesByIdAuthorAsync(ClaimManager model, string authorNikeName)
        {
            var client = _httpClientFactory.CreateClient();
            client.SetBearerToken(model.AccessToken);

            HttpResponseMessage responseGuid =
                (await client.GetAsync($"https://localhost:5001/api/author/getguidauthor?nikeNameAuthor={authorNikeName}")).EnsureSuccessStatusCode();

            string responseBodyGuid = await responseGuid.Content.ReadAsStringAsync();

            OperationResult<Guid> operationGuid = JsonConvert.DeserializeObject<OperationResult<Guid>>(responseBodyGuid);

            var authorGuid = operationGuid.Result;

            HttpResponseMessage responseArticles =
                (await client.GetAsync($"https://localhost:5001/api/article/getarticlesbyidauthor?authorGuid={authorGuid}")).EnsureSuccessStatusCode();

            string responseBodyArticles = await responseArticles.Content.ReadAsStringAsync();

            OperationResult<IEnumerable<ArticleViewModel>> operationArticles = JsonConvert.DeserializeObject<OperationResult<IEnumerable<ArticleViewModel>>>(responseBodyArticles);

            return operationArticles;
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
