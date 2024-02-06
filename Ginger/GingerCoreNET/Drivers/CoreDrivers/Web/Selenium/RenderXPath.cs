using GingerCore.Drivers.Common;
using OpenQA.Selenium;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Selenium
{
    public class RenderXPath
    {
        public static readonly char XPATH_DIVIDER = '$';

        public static string GenerateXPathForDOMElement(ISearchContext IWE, IWebDriver Driver, string current, StringBuilder XPathsForDetectedShadowElement, bool isBrowserFireFox)
        {
            string tagName = string.Empty;
            ReadOnlyCollection<IWebElement> childrenElements = null;
            ISearchContext parentElement = null;

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
                XPathsForDetectedShadowElement.Append($"{XPATH_DIVIDER}{current}{XPATH_DIVIDER}");
                string xpathBeforeCurrentShadowRoot = GenerateXPathForDOMElement(parentElement, Driver,string.Empty, XPathsForDetectedShadowElement, isBrowserFireFox);
                XPathsForDetectedShadowElement.Append(xpathBeforeCurrentShadowRoot);
                return xpathBeforeCurrentShadowRoot;
            }


            int count = 1;
            foreach (IWebElement childElement in childrenElements)
            {
                try
                {
                    if (IWE.Equals(childElement))
                    {
                        string resultXPath = string.Empty;
                        if (string.IsNullOrEmpty(tagName))
                        {
                            resultXPath = current;
                        }
                        else
                        {
                            resultXPath = "/" + tagName + "[" + count + "]" + current;
                        }
                        return GenerateXPathForDOMElement(parentElement,Driver ,resultXPath, XPathsForDetectedShadowElement, isBrowserFireFox);
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
            return current;
        }

        public static string ConvertXpathListToString(IList<string> XPathList)
        {
            StringBuilder ResultXPath = new();

            int startPointer = XPathList.Count - 1;

            while(startPointer >= 0)
            {
                ResultXPath.Append(XPathList[startPointer--]);
            }

            return ResultXPath.ToString();
        }

        public static void AddToXPathIfParentXPathExists(HTMLElementInfo foundElementInfo, string ParentXPath)
        {

            if (!string.IsNullOrEmpty(ParentXPath))
            {
                foundElementInfo.XPath = $"{foundElementInfo.XPath}{XPATH_DIVIDER}{ParentXPath}";

            }
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
