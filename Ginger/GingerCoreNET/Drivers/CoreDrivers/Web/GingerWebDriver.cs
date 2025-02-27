#region License
/*
Copyright © 2014-2025 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Drivers.CoreDrivers.Web;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.POM;
using Amdocs.Ginger.CoreNET.GeneralLib;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Drivers;
using GingerCore.Drivers.Common;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web
{
    public abstract class GingerWebDriver : DriverBase
    {
        //split path by comma outside of brackets
        private static readonly Regex IFramePathSplitRegex = new(@",(?![^\[]*[\]])");

        private static readonly IEnumerable<WebBrowserType> SeleniumSupportedBrowserTypes =
        [
            WebBrowserType.Chrome,
            WebBrowserType.FireFox,
            WebBrowserType.Edge,
            WebBrowserType.Brave,
            WebBrowserType.InternetExplorer,
            WebBrowserType.RemoteWebDriver,
        ];
        private static readonly IEnumerable<WebBrowserType> PlaywrightSupportedBrowserTypes =
        [
            WebBrowserType.Chrome,
            WebBrowserType.FireFox,
            WebBrowserType.Edge
        ];

        [UserConfigured]
        [UserConfiguredDescription("Proxy Server:Port")]
        public string? Proxy { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("http://127.0.0.1;http://localhost;")]
        [UserConfiguredDescription("Set multiple By Pass Proxy URLs separated with ';'|| By Pass Proxy works only when Proxy URL is mentioned")]
        public string? ByPassProxy { get; set; }

        [UserConfigured]
        [UserConfiguredEnumType(typeof(WebBrowserType))]
        [UserConfiguredDefault("Chrome")]
        [UserConfiguredDescription("Browser Type")]
        public virtual WebBrowserType BrowserType { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("Use Browser In Private/Incognito Mode (Please use 64bit Browse with Internet Explorer ")]
        public bool BrowserPrivateMode { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("Set \"true\" to run the browser in background (headless mode) for faster Execution")]
        public bool HeadlessBrowserMode { get; set; }

        public override ePlatformType Platform => ePlatformType.Web;

        public override ePomElementCategory? PomCategory
        {
            get
            {
                if (base.PomCategory == null)
                {
                    return ePomElementCategory.Web;
                }
                else
                {
                    return base.PomCategory;
                }
            }

            set => base.PomCategory = value;
        }

        public static IEnumerable<WebBrowserType> GetSupportedBrowserTypes(Agent.eDriverType driverType)
        {
            if (driverType == Agent.eDriverType.Selenium)
            {
                return SeleniumSupportedBrowserTypes;
            }
            else if (driverType == Agent.eDriverType.Playwright)
            {
                return PlaywrightSupportedBrowserTypes;
            }
            else
            {
                throw new ArgumentException($"Unknown web driver type '{driverType}'");
            }
        }

        private protected abstract IBrowser GetBrowser();

        private protected async Task<IEnumerable<AppWindow>> GetAppWindowsAsync()
        {
            IBrowser browser = GetBrowser();

            List<AppWindow> appWindows = [];

            List<IBrowserWindow> browserWindows = new(browser.Windows);
            foreach (IBrowserWindow browserWindow in browserWindows)
            {
                List<IBrowserTab> browserTabs = new(browserWindow.Tabs);
                foreach (IBrowserTab browserTab in browserTabs)
                {
                    string title = await browserTab.TitleAsync();
                    appWindows.Add(new()
                    {
                        Title = title,
                        WindowType = AppWindow.eWindowType.WebPage,
                    });
                }
            }

            return appWindows;
        }

        private protected async Task<AppWindow> GetActiveWindowAsync()
        {
            IBrowser browser = GetBrowser();
            string title = await browser.CurrentWindow.CurrentTab.TitleAsync();
            return new AppWindow()
            {
                Title = title,
            };
        }

        private protected async Task SwitchTabAsync(Func<IBrowserWindow, IBrowserTab, Task<bool>> filter)
        {
            IBrowser browser = GetBrowser();

            IBrowserWindow? targetWindow = null;
            IBrowserTab? targetTab = null;

            List<IBrowserWindow> windows = new(browser.Windows);
            foreach (IBrowserWindow window in windows)
            {
                List<IBrowserTab> tabs = new(window.Tabs);
                foreach (IBrowserTab tab in tabs)
                {
                    bool isMatch = await filter(window, tab);
                    if (isMatch)
                    {
                        targetWindow = window;
                        targetTab = tab;
                        break;
                    }
                }
            }

            if (targetWindow == null || targetTab == null)
            {
                return;
            }

            await browser.SetWindowAsync(targetWindow);
            await targetWindow.SetTabAsync(targetTab);
        }

        private protected async Task HighlightElementAsync(IBrowserElement element)
        {
            await element.ExecuteJavascriptAsync("element => element.style.outline='3px dashed rgb(239, 183, 247)'");
            await element.ExecuteJavascriptAsync("element => element.style.backgroundColor='rgb(239, 183, 247)'");
            await element.ExecuteJavascriptAsync("element => element.style.border='3px dashed rgb(239, 183, 247)'");
        }

        private protected async Task UnhighlightElementAsync(IBrowserElement element)
        {
            try
            {
                await element.ExecuteJavascriptAsync("element => element.style.outline=''");
                await element.ExecuteJavascriptAsync("element => element.style.backgroundColor=''");
                await element.ExecuteJavascriptAsync("element => element.style.border=''");
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "error while unhighlighting element", ex);
            }
        }

        private protected Task SwitchToFrame(eLocateBy locateBy, string locateValue)
        {
            IBrowser browser = GetBrowser();
            return browser.CurrentWindow.CurrentTab.SwitchFrameAsync(locateBy, locateValue);
        }

        private protected async Task SwitchToFrameOfElementAsync(ElementInfo element)
        {
            if (string.IsNullOrEmpty(element.Path))
            {
                return;
            }

            string path = element.Path;

            if (!element.IsAutoLearned)
            {
                path = EvaluateValueExpression(element.Path);
            }

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            string[] iframePaths = IFramePathSplitRegex.Split(path);
            foreach (string iframePath in iframePaths)
            {
                await SwitchToFrame(eLocateBy.ByXPath, iframePath);
            }
        }

        private protected async Task<IBrowserElement?> FindBrowserElementAsync(ElementInfo element)
        {
            //TODO: handle shadow root
            foreach (ElementLocator locator in element.Locators)
            {
                locator.StatusError = string.Empty;
                locator.LocateStatus = ElementLocator.eLocateStatus.Pending;
            }

            IBrowserElement? browserElement = null;

            foreach (ElementLocator locator in element.Locators.Where(l => l.Active))
            {
                eLocateBy locateBy = locator.LocateBy;
                string locateValue = locator.LocateValue;
                if (!element.IsAutoLearned)
                {
                    locateValue = EvaluateValueExpression(locateValue);
                }

                browserElement = await FindBrowserElementAsync(locateBy, locateValue);

                if (browserElement != null)
                {
                    locator.LocateStatus = ElementLocator.eLocateStatus.Passed;
                    break;
                }
                else
                {
                    locator.LocateStatus = ElementLocator.eLocateStatus.Failed;
                }
            }

            return browserElement;
        }

        private protected async Task<bool> TestElementLocatorsAsync(ElementInfo element, bool tillFirstPassed = false)
        {
            //TODO: handle shadow root
            foreach (ElementLocator locator in element.Locators)
            {
                locator.StatusError = string.Empty;
                locator.LocateStatus = ElementLocator.eLocateStatus.Pending;
            }

            IBrowserElement? browserElement = null;

            foreach (ElementLocator locator in element.Locators.Where(l => l.Active))
            {
                eLocateBy locateBy = locator.LocateBy;
                string locateValue = locator.LocateValue;
                if (!element.IsAutoLearned)
                {
                    locateValue = EvaluateValueExpression(locateValue);
                }

                browserElement = await FindBrowserElementAsync(locateBy, locateValue);

                if (browserElement != null)
                {
                    locator.LocateStatus = ElementLocator.eLocateStatus.Passed;
                    if (tillFirstPassed)
                    {
                        return true;
                    }
                }
                else
                {
                    locator.LocateStatus = ElementLocator.eLocateStatus.Failed;
                }
            }

            return element.Locators.Where(l => l.Active).All(l => l.LocateStatus == ElementLocator.eLocateStatus.Passed);
        }

        private string EvaluateValueExpression(string value)
        {
            GingerCore.ValueExpression valueExpression = new(Environment, BusinessFlow);
            return valueExpression.Calculate(value);
        }

        private protected async Task<bool> IsLiveSpyScriptInjectedAsync(IBrowserTab tab, bool isFromIframe = false)
        {
            string isInjected = "no";
            try
            {
                string? outputString = string.Empty;
                if (isFromIframe)
                {
                    object? output = await tab.ExecuteJavascriptIframeAsync("GingerLibLiveSpy.IsLiveSpyExist();");
                    outputString = output?.ToString();
                }
                else
                {
                    object? output = await tab.ExecuteJavascriptAsync("GingerLibLiveSpy.IsLiveSpyExist();");
                    outputString = output?.ToString();
                }
                if (!string.IsNullOrEmpty(outputString))
                {
                    isInjected = outputString;
                }
            }
            catch (Exception) { }

            return !string.Equals(isInjected, "no");
        }

        private protected async Task InjectLiveSpyScriptAsync(IBrowserTab tab, bool isFromIframe = false)
        {
            if (await IsLiveSpyScriptInjectedAsync(tab, isFromIframe))
            {
                return;

            }

            string liveSpyScript = JavaScriptHandler.GetJavaScriptFileContent(JavaScriptHandler.eJavaScriptFile.GingerLiveSpy);
            await InjectScriptAsync(tab, liveSpyScript, isFromIframe);
            if (isFromIframe)
            {
                await tab.ExecuteJavascriptIframeAsync("define_GingerLibLiveSpy();");
                await tab.ExecuteJavascriptIframeAsync($"arg => GingerLibLiveSpy.AddScript(arg);", JavaScriptHandler.GetJavaScriptFileContent(JavaScriptHandler.eJavaScriptFile.jquery_min));
                await tab.ExecuteJavascriptIframeAsync("GingerLibLiveSpy.StartEventListner();");
            }
            else
            {
                await tab.ExecuteJavascriptAsync("define_GingerLibLiveSpy();");
                await tab.ExecuteJavascriptAsync($"arg => GingerLibLiveSpy.AddScript(arg);", JavaScriptHandler.GetJavaScriptFileContent(JavaScriptHandler.eJavaScriptFile.jquery_min));
                await tab.ExecuteJavascriptAsync("GingerLibLiveSpy.StartEventListner();");
            }

        }

        private protected Task InjectScriptAsync(IBrowserTab tab, string script, bool isFromIframe = false)
        {
            string injectionableScript = PrepareScriptForInjection(script);
            if (isFromIframe)
            {
                return tab.InjectJavascriptIframeAsync(injectionableScript);
            }
            else
            {
                return tab.InjectJavascriptAsync(injectionableScript);
            }

        }

        private string PrepareScriptForInjection(string script)
        {
            script = JavaScriptHandler.MinifyJavaScript(script);

            //script that injects that provided parameter script
            string injectorScript = JavaScriptHandler.GetJavaScriptFileContent(JavaScriptHandler.eJavaScriptFile.InjectJavaScript);
            injectorScript = JavaScriptHandler.MinifyJavaScript(injectorScript);

            //Note minifier change ' to ", so we change it back, so the script can have ", but we wrap it all with '
            return injectorScript.Replace("\"%SCRIPT%\"", "'" + script + "'");
        }

        private protected abstract Task<IBrowserElement?> FindBrowserElementAsync(eLocateBy locateBy, string locateValue);

        private protected string GenerateXPathFromHTMLElementInfo(HTMLElementInfo htmlElementInfo)
        {
            if (!string.IsNullOrEmpty(htmlElementInfo.XPath) && !string.Equals(htmlElementInfo.XPath, "/"))
            {
                return htmlElementInfo.XPath;
            }

            string lastXPathSegment = string.Empty;
            if (!string.IsNullOrEmpty(htmlElementInfo.Path))
            {
                string[] xpathSegments = htmlElementInfo.Path.Split('/');
                lastXPathSegment = xpathSegments[^1];
            }
            string xpath = string.Empty;
            if (!lastXPathSegment.Contains("frame"))
            {
                xpath = htmlElementInfo.Path;
            }

            Stack<HtmlNode> nodes = [];
            nodes.Push(htmlElementInfo.HTMLElementObject);

            while (nodes.Count > 0)
            {
                HtmlNode currentNode = nodes.Pop();
                string tag = currentNode.Name;

                if (string.Equals(tag, "html", StringComparison.OrdinalIgnoreCase))
                {
                    xpath = $"/html[1]{xpath}";
                    continue;
                }

                HtmlNode parentNode = currentNode.ParentNode;
                int count = 1;
                foreach (HtmlNode childNode in parentNode.ChildNodes)
                {
                    if (childNode == currentNode)
                    {
                        break;
                    }
                    if (string.Equals(tag, childNode.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        count++;
                    }
                }

                xpath = $"/{tag}[{count}]{xpath}";
                nodes.Push(parentNode);
            }

            return xpath;
        }

        private protected Task<string> GenerateXPathFromBrowserElementAsync(IBrowserElement element)
        {
            string script =
            @"element => {
              let xpath = """";
              let stack = [];
              stack.push(element);
              while (stack.length > 0) {
                let currentElement = stack.pop();
                let tag = currentElement.tagName;
                if (tag.toUpperCase() === ""html"".toUpperCase()) {
                  xpath = ""/"" + tag + ""[1]"" + xpath;
                  continue;
                }
                let parentElement = currentElement.parentElement;

                // Handle elements within shadow DOM by using the shadow root's host as the parent
                // This ensures correct XPath generation for elements inside shadow DOM
                if (!parentElement && currentElement.getRootNode() instanceof ShadowRoot) {
                  parentElement = currentElement.getRootNode().host;
                }

                let count = 1;
                if (parentElement) {
                  for (let i = 0; i < parentElement.childElementCount; i++) {
                    if (parentElement.children[i] === currentElement) {
                      break;
                    }
                    if (tag.toUpperCase() === parentElement.children[i].tagName.toUpperCase()) {
                      count++;
                    }
                  }
                  xpath = ""/"" + tag + ""["" + count + ""]"" + xpath;
                  stack.push(parentElement);
                }
              }
              return xpath;
            }";
            return element.ExecuteJavascriptAsync(script);
        }

        private protected async Task<HTMLElementInfo> CreateHtmlElementAsync(IBrowserElement browserElement)
        {
            string tag = await browserElement.TagNameAsync();
            string nameAttributeValue = await browserElement.AttributeValueAsync("name");
            string idAttributeValue = await browserElement.AttributeValueAsync("id");
            string valueAttributeValue = await browserElement.AttributeValueAsync("value");
            string typeAttributeValue = await browserElement.AttributeValueAsync("type");

            string elementTitle = tag;
            if (string.Equals(tag, "table", StringComparison.OrdinalIgnoreCase))
            {
                elementTitle = "Table";
            }
            else if (!string.IsNullOrEmpty(nameAttributeValue))
            {
                elementTitle = $"{nameAttributeValue} {tag}";
            }
            else if (!string.IsNullOrEmpty(idAttributeValue))
            {
                elementTitle = $"{idAttributeValue} {tag}";
            }
            else if (!string.IsNullOrEmpty(valueAttributeValue))
            {
                elementTitle = $"{(valueAttributeValue.Length > 50 ? valueAttributeValue[..50] + "..." : valueAttributeValue)} {tag}";
            }

            string elementName = tag;
            if (string.IsNullOrEmpty(elementName))
            {
                elementName = nameAttributeValue;
            }

            string elementId = idAttributeValue;

            string elementValue = string.Empty;
            if (string.Equals(tag, "select", StringComparison.OrdinalIgnoreCase))
            {
                elementValue = $"set to {await browserElement.ExecuteJavascriptAsync("element => element.options[element.selectedIndex].text")}";
            }
            else if (string.Equals(tag, "span", StringComparison.OrdinalIgnoreCase))
            {
                elementValue = $"set to {await browserElement.TextContentAsync()}";
            }
            else if (string.Equals(tag, "input", StringComparison.OrdinalIgnoreCase) && string.Equals(typeAttributeValue, "checkbox", StringComparison.OrdinalIgnoreCase))
            {
                elementValue = $"set to {await browserElement.ExecuteJavascriptAsync("element => element.checked.toString()")}";
            }
            else
            {
                elementValue = valueAttributeValue;
            }

            string elementType = string.Empty;
            if (string.Equals(tag, "input", StringComparison.OrdinalIgnoreCase))
            {
                elementType = $"{tag}.{typeAttributeValue}";
            }
            else if (string.Equals(tag, "a", StringComparison.OrdinalIgnoreCase) || string.Equals(tag, "li", StringComparison.OrdinalIgnoreCase))
            {
                elementType = "link";
            }
            else
            {
                elementType = tag;
            }

            Size size = await browserElement.SizeAsync();
            Point position = await browserElement.PositionAsync();

            HTMLElementInfo newHtmlElement = new()
            {
                ElementObject = browserElement,
                ElementTitle = elementTitle ?? string.Empty,
                Name = elementName ?? string.Empty,
                ID = elementId ?? string.Empty,
                Value = elementValue ?? string.Empty,
                XPath = await GenerateXPathFromBrowserElementAsync(browserElement),
                ElementType = elementType ?? string.Empty,
                ElementTypeEnum = POMLearner.GetElementType(tag, typeAttributeValue),
                Width = size.Width,
                Height = size.Height,
                X = position.X,
                Y = position.Y,
            };

            return newHtmlElement;
        }
    }
}
