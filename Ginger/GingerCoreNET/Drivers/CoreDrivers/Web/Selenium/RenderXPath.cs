using Amdocs.Ginger.Common.UIElement;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.DevTools.V119.Accessibility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Selenium
{
    public class RenderXPath
    {
        // Convert this to a iterative code


        public static string GenerateXPathForDOMElement(ISearchContext IWE, IWebDriver Driver, string current, IList<string> XPathsToDetectShadowElement, bool isBrowserFireFox)
        {
            string tagName = string.Empty;            
            ISearchContext parentElement = null;
            ReadOnlyCollection<IWebElement> childrenElements = null;

            if (IWE is IWebElement)
            {
                tagName = ((IWebElement)IWE).TagName;
                if (tagName.Equals("html"))
                {
                    return "/" + tagName + "[1]" + current;
                }
                parentElement = (ISearchContext)((IJavaScriptExecutor)Driver).ExecuteScript("return arguments[0].parentNode", IWE);
                if (parentElement is ShadowRoot)
                {
                    childrenElements = parentElement.FindElements(By.CssSelector(tagName));
                }
                else
                {
                    childrenElements = parentElement.FindElements(By.XPath("./" + tagName));
                }

            }
            else if (IWE is ShadowRoot)
            {
                parentElement = (IWebElement)((IJavaScriptExecutor)Driver).ExecuteScript("return arguments[0].host", IWE);
                current = ChangeXPathIfShadowDomExists(current, true) ?? current;

                XPathsToDetectShadowElement.Add(current);

                string xpathBeforeCurrentShadowRoot = GenerateXPathForDOMElement(parentElement, Driver, string.Empty, XPathsToDetectShadowElement, isBrowserFireFox);

                XPathsToDetectShadowElement.Add(xpathBeforeCurrentShadowRoot);

                return xpathBeforeCurrentShadowRoot;
            }

            int count = 1;
            string resultXPath = string.Empty;

            foreach (IWebElement childElement in childrenElements)
            {
                try
                {
                    if (IWE.Equals(childElement))
                    {
                        if (string.IsNullOrEmpty(tagName))
                        {
                            resultXPath = current;
                        }
                        else
                        {
                            resultXPath = "/" + tagName + "[" + count + "]" + current;
                        }
                        return GenerateXPathForDOMElement(parentElement, Driver, resultXPath, XPathsToDetectShadowElement, isBrowserFireFox);
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
                        throw ex;
                    }
                }
            }

            return resultXPath;
        }



        public static string ChangeXPathIfShadowDomExists(string Xpath, bool isShadowRootDetected)
        {
            if (isShadowRootDetected)
            {
                string[] xPathSplit = Xpath.Split('/');
                if (xPathSplit.Length > 2)
                {
                    return Xpath[(Xpath.Split('/')[1].Length + 2)..];
                }
                return string.Empty;
            }
            return Xpath;
        }

        public static void FindShadowDomChildTagNameAndCount(string Xpath , out int Count , out string TagName)
        {
            string[] xPathSplit = Xpath.Split('/');

            TagName = xPathSplit[1].Substring(0, Xpath.IndexOf('[') - 1);
            Count = int.Parse(xPathSplit[1].Substring(Xpath.IndexOf('['), Xpath.IndexOf(']') - Xpath.IndexOf('[') - 1));
        }

    }


}
