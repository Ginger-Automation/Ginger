using OpenQA.Selenium;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Selenium
{
    public class ShadowDOM
    {
        public static ISearchContext GetShadowRootIfExists(ISearchContext webElement)
        {

            if (webElement is not IWebElement) return null;


            try
            {
                return ((IWebElement)webElement).GetShadowRoot();
            }
            catch
            {
                return null;
            }
        }
        public static string GetHTML(IWebElement root, IWebDriver driver)
        {

            return (string)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].outerHTML", root);
        }

        public static IWebElement FindShadowRootDirectChild(ISearchContext shadowRoot , string cssSelector, IWebDriver Driver)
        {
            try
            {
                return (IWebElement)((IJavaScriptExecutor)Driver).ExecuteScript("return arguments[0].querySelector(arguments[1])" , shadowRoot , cssSelector);
            }
            catch 
            { 
                return null; 
            }
        }
    }

}


