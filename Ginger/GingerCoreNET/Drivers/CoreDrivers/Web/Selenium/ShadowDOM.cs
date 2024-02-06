using OpenQA.Selenium;

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
        public static string GetHTML(IWebElement root, IWebDriver driver)
        {

            return (string)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].outerHTML", root);
        }
    }

}


