using Amdocs.Ginger.Common.UIElement;
using OpenQA.Selenium;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Selenium
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

            if (webElement == null || webElement is not IWebElement)
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
            IEnumerable<string> tags = XPath.Split('/').Where((x)=>!string.IsNullOrEmpty(x));
            StringBuilder strBuilder = new();

            foreach (string tag in tags)
            {
                int indexOfOpenBracket = tag.IndexOf('[');

                if(indexOfOpenBracket!= -1)
                {
                    string tagName = tag.Substring(0, indexOfOpenBracket);
                    string count = tag.Substring(indexOfOpenBracket);
                    count = count.Replace('[', '(');
                    count = count.Replace(']', ')');
                    strBuilder.Append($"{tagName}:nth-of-type{count} ");
                }

                else
                {
                    strBuilder.Append(tag);
                }
            }

            return strBuilder.ToString();
        }

    }

}


