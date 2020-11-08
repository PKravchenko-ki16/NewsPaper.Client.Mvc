using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Calabonga.OperationResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsPaper.Client.Mvc.Infrastructure;
using NewsPaper.Client.Mvc.ViewModels;
using NewsPaper.Client.Mvc.ViewModels.Article;
using Newtonsoft.Json;

namespace NewsPaper.Client.Mvc.Controllers
{
    public class ArticleController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SetBearerTokenRequestClient _retrieveToIdentityServer;

        public ArticleController(IHttpClientFactory httpClientFactory, SetBearerTokenRequestClient retrieveToIdentityServer)
        {
            _httpClientFactory = httpClientFactory;
            _retrieveToIdentityServer = retrieveToIdentityServer;
        }

        [Authorize]
        public IActionResult Index()
        {
            var model = new ClaimManager(HttpContext, User);
            ViewBag.Role = model.RoleClaim;
            return View("Index");
        }

        public IActionResult AccessDenied(string returnUrl)
        {
            return View("AccessDenied");
        }

        [Authorize]
        public async Task<IActionResult> GetArticles()
        {
            var model = new ClaimManager(HttpContext, User);
            ViewBag.Role = model.RoleClaim;
            OperationResult<IEnumerable<ArticleViewModel>> listArticle;
            try
            {
                listArticle = await RequestGetArticlesAsync(model);
                return View(listArticle);
            }
            catch(Exception exception)
            {
                await _retrieveToIdentityServer.RefreshToken(model.RefreshToken, _httpClientFactory, HttpContext);
                model = new ClaimManager(HttpContext, User);
                listArticle = await RequestGetArticlesAsync(model);
                listArticle.AddError(exception);
            }
            return View(listArticle);
        }

        [Authorize]
        public async Task<IActionResult> GetArticleById(Guid articleGuid)
        {
            var model = new ClaimManager(HttpContext, User);
            ViewBag.Role = model.RoleClaim;
            OperationResult<ArticleViewModel> article;
            try
            {
                article = await RequestGetArticleByIdAsync(model, articleGuid);
                return View(article);
            }
            catch (Exception exception)
            {
                await _retrieveToIdentityServer.RefreshToken(model.RefreshToken, _httpClientFactory, HttpContext);
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
            ViewBag.Role = model.RoleClaim;
            OperationResult<IEnumerable<ArticleViewModel>> listArticle;
            try
            {
                listArticle = await RequestGetArticlesByIdAuthorAsync(model, authorNikeName);
                return View("GetArticles",listArticle);
            }
            catch (Exception exception)
            {
                await _retrieveToIdentityServer.RefreshToken(model.RefreshToken, _httpClientFactory, HttpContext);
                model = new ClaimManager(HttpContext, User);
                listArticle = await RequestGetArticlesByIdAuthorAsync(model, authorNikeName);
                listArticle.AddError(exception);
            }
            return View("GetArticles", listArticle);
        }

        [Authorize]
        [Authorize(Policy = "AccessForAuthor")]
        public IActionResult CreateArticle()
        {
            var model = new ClaimManager(HttpContext, User);
            ViewBag.Role = model.RoleClaim;
            return View("CreateArticle", new ArticleViewModel());
        }

        [Authorize]
        [Authorize(Policy = "AccessForAuthor")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async  Task<IActionResult> CreateArticle([FromForm] ArticleViewModel article)
        {
            var model = new ClaimManager(HttpContext, User);
            ViewBag.Role = model.RoleClaim;
            article.AuthorGuid = model.IdentityId;
            try
            {
                await RequestCreateArticleAsync(model, article);
                return View("CreateArticleSuccessfully");
            }
            catch (Exception exception)
            {
                await _retrieveToIdentityServer.RefreshToken(model.RefreshToken, _httpClientFactory, HttpContext);
                model = new ClaimManager(HttpContext, User);
                await RequestCreateArticleAsync(model, article);
            }
            return View("CreateArticleSuccessfully");
        }

        private async Task RequestCreateArticleAsync(ClaimManager model, ArticleViewModel article)
        {
            HttpClient client = _retrieveToIdentityServer.RetrieveToIdentityServer(_httpClientFactory, model.AccessToken);

            string json = JsonConvert.SerializeObject(article);

            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response =
                (await client.PostAsync("https://localhost:5001/api/article/createarticle/", content)).EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            //OperationResult<ArticleViewModel> operation = JsonConvert.DeserializeObject<OperationResult<ArticleViewModel>>(responseBody); //use on the page to show the article to its creator
        }

        private async Task<OperationResult<IEnumerable<ArticleViewModel>>> RequestGetArticlesAsync(ClaimManager model)
        {
            HttpClient client = _retrieveToIdentityServer.RetrieveToIdentityServer(_httpClientFactory, model.AccessToken);

            HttpResponseMessage response =
                (await client.GetAsync("https://localhost:5001/api/article/getarticles/")).EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            OperationResult<IEnumerable<ArticleViewModel>> operation = JsonConvert.DeserializeObject<OperationResult<IEnumerable<ArticleViewModel>>>(responseBody);

            return operation;
        }

        private async Task<OperationResult<ArticleViewModel>> RequestGetArticleByIdAsync(ClaimManager model, Guid articleGuid)
        {
            HttpClient client = _retrieveToIdentityServer.RetrieveToIdentityServer(_httpClientFactory, model.AccessToken);

            HttpResponseMessage response =
                (await client.GetAsync($"https://localhost:5001/api/article/getarticlebyid?articleGuid={articleGuid}")).EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            OperationResult<ArticleViewModel> operation = JsonConvert.DeserializeObject<OperationResult<ArticleViewModel>>(responseBody);

            return operation;
        }

        private async Task<OperationResult<IEnumerable<ArticleViewModel>>> RequestGetArticlesByIdAuthorAsync(ClaimManager model, string authorNikeName)
        {
            HttpClient client = _retrieveToIdentityServer.RetrieveToIdentityServer(_httpClientFactory, model.AccessToken);

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
    }
}
