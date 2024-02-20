using Amdocs.Ginger.Common.UIElement;
using OpenQA.Selenium;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Selenium
{
    public class ShadowDOM
    {
        public ISearchContext GetShadowRootIfExists(ISearchContext webElement)
        {

            if (webElement == null || webElement is not IWebElement) return null;


            try
            {
                return ((IWebElement)webElement).GetShadowRoot();
            }
            catch
            {
                return null;
            }
        }
        public string GetHTML(ISearchContext root, IWebDriver driver)
        {

            return (string)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].innerHTML", root);
        }

        public IWebElement FindShadowRootDirectChild(ISearchContext shadowRoot, ElementLocator locator, IWebDriver Driver, string childTagName)
        {
            try
            {
                string cssSelector = GetCssSelectorForShadowDOMChild(locator, childTagName);
                if (string.IsNullOrEmpty(cssSelector))
                {
                    return null;
                }
                return (IWebElement)((IJavaScriptExecutor)Driver).ExecuteScript("return arguments[0].querySelector(arguments[1])", shadowRoot, cssSelector);
            }
            catch
            {
                return null;
            }
        }

        public string GetCssSelectorForShadowDOMChild(ElementLocator locator, string tagName)
        {
            tagName = tagName.ToLower();

            if (locator.LocateBy.Equals(eLocateBy.ByID))
            {
                return $"{tagName}#{locator.LocateValue}";
            }

            else if (locator.LocateBy.Equals(eLocateBy.ByClassName))
            {
                return $"{tagName}.{locator.LocateValue}";
            }
            else if (locator.LocateBy.Equals(eLocateBy.ByName))
            {
                return $"{tagName}[name='{locator.LocateValue}']";
            }
            else if (locator.LocateBy.Equals(eLocateBy.ByCSS) || locator.LocateBy.Equals(eLocateBy.ByTagName))
            {
                return locator.LocateValue;
            }

            return null;
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


