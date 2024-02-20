using Amdocs.Ginger.Common.UIElement;
using MongoDB.Driver.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.DevTools.V119.Accessibility;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Selenium
{
    public class RenderXPath
    {
        /// <summary>
        /// The algorithm to find the xpath uses the bottom to top DFS approach until the 'html' tag is located viz the topmost element in HTML Document.
        /// </summary>
        /// <param name="IWE">Web Element for which the Xpath should be generated</param>
        /// <param name="Driver"></param>
        /// <param name="current">Current Xpath</param>
        /// <param name="XPathsToDetectShadowElement">if shadow dom is detected, the xpath is added to the list (which as of now is only used for Live Spy)</param>
        /// <param name="isBrowserFireFox"></param>
        /// <returns>the xpath relative to the  nearest shadow dom else returns the full xpath</returns>
        public string GenerateXPathForDOMElement(ISearchContext IWE, IWebDriver Driver, string current, IList<string> XPathsToDetectShadowElement, bool isBrowserFireFox)
        {

            Stack<ISearchContext> stack = new();
            stack.Push(IWE);
            ISearchContext parentElement = null;
            ReadOnlyCollection<IWebElement> childrenElements = null;
            bool isShadowRootDetected = false;

            while (stack.Count > 0)
            {

                ISearchContext context = stack.Pop();
                if (context is IWebElement webElement)
                {
                    string tagName = webElement.TagName;

                    if (tagName.Equals("html"))
                    {
                        string resultXPath = $"/html[1]{current}";
                        current = resultXPath;

                        if (isShadowRootDetected)
                        {
                            XPathsToDetectShadowElement.Add(resultXPath);
                        }
                        continue;
                    }
                    parentElement = (ISearchContext)((IJavaScriptExecutor)Driver).ExecuteScript("return arguments[0].parentNode", webElement);

                    if (parentElement is ShadowRoot)
                    {
                        childrenElements = parentElement.FindElements(By.CssSelector(tagName));
                    }
                    else
                    {
                        childrenElements = parentElement.FindElements(By.XPath("./" + tagName));
                    }
                    int count = 1;

                    foreach (IWebElement childElement in childrenElements)
                    {

                        try
                        {
                            if (context.Equals(childElement))
                            {
                                string resultXPath = string.Empty;
                                if (string.IsNullOrEmpty(tagName))
                                {
                                    resultXPath = current;
                                }
                                else
                                {
                                    resultXPath = $"/{tagName}[{count}]{current}";
                                }
                                stack.Push(parentElement);
                                current = resultXPath;
                                break;
                            }
                            else
                            {
                                count++;
                            }

                        }

                        catch (Exception ex)
                        {
                            if (isBrowserFireFox && ex.Message != null && ex.Message.Contains("did not match a known command"))
                            {
                                continue;
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }


                else if (context is ShadowRoot shadowRoot)
                {
                    parentElement = (IWebElement)((IJavaScriptExecutor)Driver).ExecuteScript("return arguments[0].host", shadowRoot);
                    XPathsToDetectShadowElement.Add(current);
                    stack.Push(parentElement);
                    current = string.Empty;
                    isShadowRootDetected = true;
                }

            }

            return XPathsToDetectShadowElement.Count > 0 ? XPathsToDetectShadowElement[0] : current;
        }

    }


}
