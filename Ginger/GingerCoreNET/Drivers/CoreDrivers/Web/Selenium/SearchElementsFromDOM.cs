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
    public class SearchElementsFromDOM
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

        /// <summary>
        /// Gets all the Child nodes of the searchContext
        /// </summary>
        /// <param name="searchContext"></param>
        /// <param name="driver"></param>
        /// <returns></returns>
        public IList<IWebElement> GetAllChildNodes(ISearchContext searchContext, IWebDriver driver)         
        {
            var childNodes = ((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].childNodes", searchContext);

            if(childNodes is ReadOnlyCollection<object> objectChildNodes)
            {
                return objectChildNodes.Where((childNode) => childNode is IWebElement).Select((childNode) => (IWebElement)childNode).ToList();
            }

            else if(childNodes is ReadOnlyCollection<IWebElement> webElementChildNodes)
            {
                return webElementChildNodes.ToList();
            }

            return null;
        }
       
        /// <summary>
        /// Finds Element when element is detected by live spy
        /// </summary>
        /// <param name="XPaths">list of xpaths if the shadow dom exists</param>
        /// <param name="ElementToBeFoundLocator">locator info of the element to be found</param>
        /// <returns></returns>
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
        /// <summary>
        /// It first checks if the direct child is the element that is to be found. if not then finds the element in the descendants of the direct child nodes of the shadow root
        /// </summary>
        /// <param name="shadowRoot"></param>
        /// <param name="locator">locator of the element to found</param>
        /// <returns></returns>
        private IWebElement FindNodeBySelectorInShadowDOM(ShadowRoot shadowRoot, ElementLocator locator)
        {
            IList<IWebElement> childNodes = GetAllChildNodes(shadowRoot, seleniumDriver.mDriver);

            // 1. check if element exists in the direct childNodes of the shadow dom
            // 2. check if the element exists in the childNodes' DOM

            IWebElement webElement = null;
            if(childNodes == null)
            {
                return webElement;
            }

            foreach (IWebElement child in childNodes)
            {
                
                webElement = shadowDOM.FindShadowRootDirectChild(shadowRoot, locator, seleniumDriver.mDriver, child.TagName) ?? locateWebElement.LocateElementByLocator(locator, child, null, true);

                if (webElement != null)
                {
                    break;
                }
            }


            return webElement;
        }



    }
}
