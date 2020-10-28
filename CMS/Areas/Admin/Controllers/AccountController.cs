using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService;
using CookiesService;
using DataService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ModelService;
using Serilog;

namespace CMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly AppSettings appSettings;
        private readonly DataProtectionKeys dataProtKeys;
        private readonly IServiceProvider provider;
        private readonly AppDbContext appDb;
        private readonly IAuthServ authServ;
        private readonly ICookiesServ cookieServ;
        private const string AccessToken = "access_token";
        private const string UserId = "user_id";
        string[] cookiesToRemove = { "twoFactorToken", "memberId", "rememberDevice", "user_id", "access_token" };
        public AccountController(IOptions<AppSettings> appSet, IServiceProvider prov, AppDbContext db, IAuthServ aServ, ICookiesServ cServ, IOptions<DataProtectionKeys> dpk)
        {
            appSettings = appSet.Value;
            provider = prov;
            appDb = db;
            dataProtKeys = dpk.Value;
            authServ = aServ;
            cookieServ = cServ;
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                try
                {
                    var jwtToken = await authServ.Auth(model);
                    const int expireTime = 60;
                    cookieServ.Set("access_token", jwtToken.Token, expireTime);
                    cookieServ.Set("user_id", jwtToken.UserId, expireTime);
                    cookieServ.Set("username", jwtToken.UserName, expireTime);
                    Log.Information($"User {model.Email} logged in");
                    return Ok("Sucess");

                }
                catch (Exception e)
                {
                    Log.Error("Error while loading database {Error} {StackTrace} {InnerException} {Source}",
                    e.Message, e.StackTrace, e.InnerException, e.Source);
                }
            }
            ModelState.AddModelError("", "Username/Password was entered incorrectly");
            Log.Error("Username/Password was entered incorrectly");
            return Unauthorized("Your Username or Password was entered incorrectly. Please try again");

        }
        public IActionResult FailPath()
        {
            return View();
        }
    }
}
