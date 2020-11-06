﻿using System;
using System.Collections.Generic;
using System.Net.Http;
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
                await _retrieveToIdentityServer.RefreshToken(model.RefreshToken, _httpClientFactory, HttpContext);
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

        private async Task<OperationResult<IEnumerable<ArticleViewModel>>> RequestGetAtriclesAsync(ClaimManager model)
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
