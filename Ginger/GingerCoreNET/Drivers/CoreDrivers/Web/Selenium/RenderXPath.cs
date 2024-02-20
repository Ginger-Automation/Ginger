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
        public string GenerateXPathForDOMElement(ISearchContext IWE, IWebDriver Driver, string current, IList<string> XPathsToDetectShadowElement, bool isBrowserFireFox)
        {

            Stack<ISearchContext> stack = new();
            Stack<string> currentXpathStack = new();
            stack.Push(IWE);
            currentXpathStack.Push(string.Empty);

            ISearchContext parentElement = null;
            ReadOnlyCollection<IWebElement> childrenElements = null;
            bool isShadowRootDetected = false;

            while (stack.Count > 0)
            {

                ISearchContext context = stack.Pop();
                string currentXpath = currentXpathStack.Pop();
                if(context is IWebElement webElement)
                {
                    string tagName = webElement.TagName;

                    if (tagName.Equals("html"))
                    {
                        string resultXPath = $"/html[1]{currentXpath}";
                        currentXpathStack.Push(resultXPath);
                        
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
                                    resultXPath = currentXpath;
                                }
                                else
                                {
                                    resultXPath = $"/{tagName}[{count}]{currentXpath}";
                                }
                                stack.Push(parentElement);
                                currentXpathStack.Push(resultXPath);
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
                    XPathsToDetectShadowElement.Add(currentXpath);
                    stack.Push(parentElement);
                    currentXpathStack.Push(string.Empty);   
                    isShadowRootDetected = true;
                }

            }

            return currentXpathStack.Count > 0 ? currentXpathStack.Pop() : string.Empty;
        }

    }


}
