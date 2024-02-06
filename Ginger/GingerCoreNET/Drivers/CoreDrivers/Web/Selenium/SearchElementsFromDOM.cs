using Amdocs.Ginger.Common.UIElement;
using GingerCore.Drivers.Common;
using OpenQA.Selenium;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Selenium
{
    class SearchElementsFromDOM
    {

        public static ReadOnlyCollection<T> GetAllChildNodes<T>(ISearchContext ShadowRoot, IWebDriver driver)
        {
            return (ReadOnlyCollection<T>)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].childNodes", ShadowRoot);
        }
        public static IWebElement FindElement(string XpathWithDividers , ISearchContext Driver , By by)
        {
            return FindElement(XpathWithDividers.Split(RenderXPath.XPATH_DIVIDER).Where((xpath) => !string.IsNullOrEmpty(xpath)).ToList(),  Driver , by);
        }
        private static IWebElement FindElement(IList<string>XPaths, ISearchContext Driver, By by)
        {
            int startPointer = XPaths.Count - 1;
            ISearchContext CurrentElement = Driver;
            while(startPointer > 0)
            {
                string XPath = XPaths[startPointer--];
                if (CurrentElement is ShadowRoot)
                {
                    CurrentElement = FindNodeBySelectorInShadowDOM((ShadowRoot)CurrentElement , XPath , (IWebDriver)Driver);
                }
                else
                {
                    CurrentElement = CurrentElement.FindElement(By.XPath(XPath));
                    CurrentElement = ShadowDOM.GetShadowRootIfExists((IWebElement)CurrentElement) ?? CurrentElement;
                }
            }
            
            if(CurrentElement is ShadowRoot)
            {
               return FindNodeBySelectorInShadowDOM((ShadowRoot)CurrentElement, XPaths[startPointer], (IWebDriver)Driver);
            }
            else
            {
                return CurrentElement.FindElement(by);
            }
        }

        public static IWebElement FindNodeBySelectorInShadowDOM(ShadowRoot ShadowRoot, string XPath, IWebDriver Driver)
        {
            ReadOnlyCollection<IWebElement> childNodes = GetAllChildNodes<IWebElement>(ShadowRoot, Driver);
            string XpathRelativeToParentNode = RenderXPath.ChangeXPathIfShadowDomExists(XPath, true);

            if (string.IsNullOrEmpty(XpathRelativeToParentNode))
            {
                int Count = 0;
                string TagName = string.Empty;
                RenderXPath.FindShadowDomChildTagNameAndCount(XPath , out Count, out TagName);  
                
                foreach (IWebElement child in childNodes)
                {
                    try
                    {
                        if (TagName.Equals(child.TagName) && Count == 1)
                        {
                            Count--;
                            return child;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            else
            {
                foreach (IWebElement child in childNodes)
                {

                    try
                    {
                        return child.FindElement(By.XPath(XpathRelativeToParentNode));
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            return null;
        }


    }
}
