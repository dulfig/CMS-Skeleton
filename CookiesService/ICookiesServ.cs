using System;
using System.Collections.Generic;
using System.Text;

namespace CookiesService
{
    public interface ICookiesServ
    {
        string Get(string key);
        void Set(string key, string val, int? expireTime, bool isSecure, bool isHttpOnly);
        void Set(string key, string val, int? expireTime);
        void DeleteCookie(string key);
        void DeleteAllCookies(IEnumerable<string> cookToDelete);
        string GetUserIP();
        string GetUserCountry();
        string GetUserOs();
    }
}
