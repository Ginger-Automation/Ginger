#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using OpenQA.Selenium;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web
{
    public class ShadowDOM
    {
        /// <summary>
        /// returns the shadow root if exists in the given ISearchContext
        /// </summary>
        /// <param name="webElement">under which shadow root is searched for</param>
        /// <returns></returns>
        public ISearchContext GetShadowRootIfExists(ISearchContext webElement)
        {

            if (webElement is null or not IWebElement)
            {
                return null;
            }

            try
            {
                return ((IWebElement)webElement).GetShadowRoot();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the inner HTML of the giver root
        /// </summary>
        /// <param name="root"></param>
        /// <param name="driver"></param>
        /// <returns></returns>
        public string GetHTML(ISearchContext root, IWebDriver driver)
        {

            return (string)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].innerHTML", root);
        }


        public string ConvertXPathToCssSelector(string XPath)
        {
            IEnumerable<string> tags = XPath.Split('/').Where((x) => !string.IsNullOrEmpty(x));
            StringBuilder strBuilder = new();

            foreach (string tag in tags)
            {
                int indexOfOpenBracket = tag.IndexOf('[');

                if (indexOfOpenBracket != -1)
                {
                    string tagName = tag[..indexOfOpenBracket];
                    string count = tag[indexOfOpenBracket..];
                    count = count.Replace('[', '(');
                    count = count.Replace(']', ')');
                    strBuilder.Append($"{tagName}:nth-of-type{count} ");
                }

                else
                {
                    strBuilder.Append($"{tag} ");
                }
            }

            return strBuilder.ToString();
        }

    }

}


