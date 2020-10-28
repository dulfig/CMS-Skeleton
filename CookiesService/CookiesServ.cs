using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace CookiesService
{
    public class CookiesServ : ICookiesServ
    {
        private readonly CookieOptions cookieOptions;
        private readonly IHttpContextAccessor httpContext;
        
        public CookiesServ(CookieOptions cookOp, IHttpContextAccessor hContextAccessor)
        {
            cookieOptions = cookOp;
            httpContext = hContextAccessor;
        }
        public string Get(string key)
        {
            return httpContext.HttpContext.Request.Cookies[key];
        }
        public void Set(string key, string val, int? expireTime, bool isSecure, bool isHttpOnly) 
        {
            cookieOptions.Expires = expireTime.HasValue ? DateTime.Now.AddMinutes(expireTime.Value) : DateTime.Now.AddMilliseconds(10);
            cookieOptions.Secure = isSecure;
            cookieOptions.HttpOnly = isHttpOnly;
            httpContext.HttpContext.Response.Cookies.Append(key, val, cookieOptions);
        }
        public void Set(string key, string val, int? expireTime)
        {
            cookieOptions.Secure = true;
            cookieOptions.Expires = expireTime.HasValue ? DateTime.Now.AddMinutes(expireTime.Value) : DateTime.Now.AddMilliseconds(10);
            cookieOptions.HttpOnly = true;
            cookieOptions.SameSite = SameSiteMode.Strict;
            httpContext.HttpContext.Response.Cookies.Append(key, val, cookieOptions);
        }
        public void DeleteCookie(string key) 
        {
            httpContext.HttpContext.Response.Cookies.Delete(key);
        }
        public void DeleteAllCookies(IEnumerable<string> cookToDelete) 
        {
            foreach (var key in cookToDelete)
                httpContext.HttpContext.Response.Cookies.Delete(key);
        }
        public string GetUserIP() 
        { 
            return String.Empty; 
        }
        public string GetUserCountry() 
        {
            return String.Empty;
        }
        public string GetUserOs() 
        {
            return String.Empty;
        }
    }
}
