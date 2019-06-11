using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.Web
{

    /// <summary>
    /// Exposes Cookies Functionalities
    /// </summary>
    public interface ICookiesHandling
    {
        /// <summary>
        /// Gets all the cookies of current window.
        /// </summary>
        /// <returns></returns>
        List<string> GetAllCookies();
        /// <summary>
        /// Get Named Cookie
        /// </summary>
        /// <returns></returns>
        string GetnamedCookie();

        /// <summary>
        /// Add Cookie to Current session.
        /// </summary>
        /// <param name="Cookie"></param>
        void AddCookie(string Cookie);


        /// <summary>
        /// Delete a cookie by name
        /// </summary>
        /// <param name="Cookie"></param>
        void DeleteCookie(string Cookie);
        /// <summary>
        /// Deletes AllCookies 
        /// </summary>
        void DeleteAllCookies();
    }
}
