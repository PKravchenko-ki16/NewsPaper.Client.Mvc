﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace NewsPaper.Client.Mvc.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GoodBye()
        {
            return View("Index");
        }

        public IActionResult Logout()
        {
            var parameters = new AuthenticationProperties
            {
                RedirectUri = "/Home/GoodBye"
            };
            return SignOut(
                parameters,
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}
