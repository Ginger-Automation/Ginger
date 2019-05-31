using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.Web
{
    public interface ICookiesHandling
    {

        List<string> GetAllCookies();
        string GetnamedCookie();
        void AddCookie(string Cookie);
        void DeleteCookie(string Cookie);

        void DeleteAllCookies();
    }
}
