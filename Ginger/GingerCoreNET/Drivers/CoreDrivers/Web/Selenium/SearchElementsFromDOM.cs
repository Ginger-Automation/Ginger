using Amdocs.Ginger.Common.UIElement;
using GingerCore.Drivers;
using GingerCore.Drivers.Common;
using NPOI.OpenXmlFormats.Shared;
using OpenQA.Selenium;
using OpenQA.Selenium.DevTools.V119.Debugger;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Selenium
{
    class SearchElementsFromDOM
    {

        private readonly SeleniumDriver seleniumDriver;
        private readonly LocateWebElement locateWebElement;
        private readonly ShadowDOM shadowDOM = new();
        public SearchElementsFromDOM() { }

        public SearchElementsFromDOM(SeleniumDriver seleniumDriver)
        {
            this.seleniumDriver = seleniumDriver;
            this.locateWebElement = new(this.seleniumDriver);
        }


        public IList<IWebElement> GetAllChildNodes(ISearchContext ShadowRoot, IWebDriver driver)         
        {
            try
            {
                ReadOnlyCollection<object> childNodes = (ReadOnlyCollection<object>)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].childNodes", ShadowRoot);
                return childNodes.Where((childNode) => childNode is IWebElement).Select((childNode)=>(IWebElement)childNode).ToList();
            }

            catch(InvalidCastException)
            {
                ReadOnlyCollection<IWebElement> childNodes = (ReadOnlyCollection<IWebElement>)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].childNodes", ShadowRoot);
                return childNodes.ToList();
            }

        }
        public IWebElement FindElement(IList<string> XPaths, ElementLocator ElementToBeFoundLocator)
        {
            if (XPaths == null || XPaths.Count == 0)
            {
                return locateWebElement.LocateElementByLocator(ElementToBeFoundLocator, seleniumDriver.mDriver, null, false);
            }

            int startPointer = XPaths.Count - 1;
            ISearchContext CurrentElement = seleniumDriver.mDriver;
            while (startPointer >= 0 && CurrentElement != null)
            {
                string XPath = XPaths[startPointer];
                ElementLocator elementLocator = this.GetElementLocatorForWebElement(startPointer, XPath, ElementToBeFoundLocator);

                if (CurrentElement is ShadowRoot shadowRoot)
                {
                    CurrentElement = FindNodeBySelectorInShadowDOM(shadowRoot, elementLocator);
                }
                else
                {
                    CurrentElement = locateWebElement.LocateElementByLocator(elementLocator, CurrentElement, null, false);
                    CurrentElement = shadowDOM.GetShadowRootIfExists((IWebElement)CurrentElement) ?? CurrentElement;
                }
                startPointer--;
            }


            return (IWebElement)CurrentElement;
        }
        private ElementLocator GetElementLocatorForWebElement(int startPointer, string XPath, ElementLocator ElementToBeFoundLocator)
        {

            ElementLocator locator = new();

            if (startPointer == 0)
            {
                locator = ElementToBeFoundLocator;
            }

            else
            {
                locator.LocateBy = eLocateBy.ByXPath;
                locator.LocateValue = XPath;
            }
            return locator;
        }
        private IWebElement FindNodeBySelectorInShadowDOM(ShadowRoot shadowRoot, ElementLocator locator)
        {
            IList<IWebElement> childNodes = GetAllChildNodes(shadowRoot, seleniumDriver.mDriver);

            // 1. check if element exists in the direct childNodes of the shadow dom
            // 2. check if the element exists in the childNodes' DOM

            IWebElement webElement = null;

            foreach (IWebElement child in childNodes)
            {
                try
                {
                    webElement = shadowDOM.FindShadowRootDirectChild(shadowRoot, locator, seleniumDriver.mDriver, child.TagName) ?? locateWebElement.LocateElementByLocator(locator, child, null, false);
                }
                catch
                {
                    continue;
                }

            }


            return webElement;
        }



    }
}
