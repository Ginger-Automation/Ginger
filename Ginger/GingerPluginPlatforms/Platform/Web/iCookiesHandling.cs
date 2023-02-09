#region License
/*
Copyright © 2014-2023 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

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
