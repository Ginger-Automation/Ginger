using GingerCore.Drivers;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Selenium
{
    public class ShadowDOM
    {
        public static ISearchContext GetShadowRootIfExists(IWebElement webElement)
        {

            try
            {
                return webElement.GetShadowRoot();
            }
            catch
            {
                return null;
            }
        }
        public static string GetInnerHTML(IWebElement root , IWebDriver driver)
        {

            return (string)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].getInnerHTML()", root);
        }
        public static ReadOnlyCollection<IWebElement> GetAllChildNodesFromShadow(ISearchContext ShadowRoot, IWebDriver driver)
        {
            return (ReadOnlyCollection<IWebElement>)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].childNodes", ShadowRoot);
        }
        public static string ChangeXPathIfShadowDomExists(string Xpath , bool isShadowRootDetected)
        {
            if (isShadowRootDetected) return Xpath[1..];

            return Xpath;
        }


    }
}
