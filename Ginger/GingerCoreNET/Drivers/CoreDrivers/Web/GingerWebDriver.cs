using Amdocs.Ginger.Common.Drivers.CoreDrivers.Web;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.POM;
using Amdocs.Ginger.CoreNET.GeneralLib;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Drivers;
using GingerCore.Drivers.Common;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using HtmlAgilityPack;
using Microsoft.Graph;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static GingerCore.Platforms.PlatformsInfo.PlatformInfoBase;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web
{
    public abstract class GingerWebDriver : DriverBase
    {
        //split path by comma outside of brackets
        private static readonly Regex IFramePathSplitRegex = new(@",(?![^\[]*[\]])");

        private static readonly IEnumerable<WebBrowserType> SeleniumSupportedBrowserTypes = new List<WebBrowserType>()
        {
            WebBrowserType.Chrome,
            WebBrowserType.FireFox,
            WebBrowserType.Edge,
            WebBrowserType.Brave,
            WebBrowserType.InternetExplorer,
            WebBrowserType.RemoteWebDriver,
        };
        private static readonly IEnumerable<WebBrowserType> PlaywrightSupportedBrowserTypes = new List<WebBrowserType>()
        {
            WebBrowserType.Chrome,
            WebBrowserType.FireFox,
            WebBrowserType.Edge
        };

        [UserConfigured]
        [UserConfiguredEnumType(typeof(WebBrowserType))]
        [UserConfiguredDefault("Chrome")]
        [UserConfiguredDescription("Browser Type")]
        public virtual WebBrowserType BrowserType { get; set; }

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
            await element.ExecuteJavascriptAsync("element => element.style.outline=''");
            await element.ExecuteJavascriptAsync("element => element.style.backgroundColor=''");
            await element.ExecuteJavascriptAsync("element => element.style.border=''");
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

            foreach (ElementLocator locator in element.Locators)
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

        private string EvaluateValueExpression(string value)
        {
            GingerCore.ValueExpression valueExpression = new(Environment, BusinessFlow);
            return valueExpression.Calculate(value);
        }

        private protected async Task<bool> IsLiveSpyScriptInjectedAsync(IBrowserTab tab)
        {
            string isInjected = "no";
            try
            {
                object? output = await tab.ExecuteJavascriptAsync("return GingerLibLiveSpy.IsLiveSpyExist();");
                string? outputString = output?.ToString();
                if (!string.IsNullOrEmpty(outputString))
                {
                    isInjected = outputString;
                }
            }
            catch (Exception) { }

            return !string.Equals(isInjected, "no");
        }

        private protected async Task InjectLiveSpyScriptAsync(IBrowserTab tab)
        {
            if (await IsLiveSpyScriptInjectedAsync(tab))
            {
                return;
            }

            string liveSpyScript = JavaScriptHandler.GetJavaScriptFileContent(JavaScriptHandler.eJavaScriptFile.GingerLiveSpy);
            await InjectScriptAsync(tab, liveSpyScript);

            //don't know why we execute below 2 scripts, copying it from SeleniumDriver
            await tab.ExecuteJavascriptAsync("define_GingerLibLiveSpy();");
            await tab.ExecuteJavascriptAsync($"return GingerLibLiveSpy.AddScript({JavaScriptHandler.GetJavaScriptFileContent(JavaScriptHandler.eJavaScriptFile.jquery_min)});");
        }

        private protected Task InjectScriptAsync(IBrowserTab tab, string script)
        {
            string injectionableScript = PrepareScriptForInjection(script);
            return tab.ExecuteJavascriptAsync(injectionableScript);
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
                lastXPathSegment = xpathSegments[xpathSegments.Length - 1];
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
              while(stack.length > 0) {
                let currentElement = stack.pop();
                let tag = currentElement.tagName;
                if(tag.toUpperCase() === ""html"".toUpperCase()) {
                  xpath = ""/""+tag+""[1]""+ xpath;
                  continue;
                }
                let parentElement = currentElement.parentElement;
                let count = 1;
                for (let i = 0; i < parentElement.childElementCount; i++) {
                  if (parentElement.children[i] === currentElement) {
                    break;
                  }
                  if (tag.toUpperCase() === parentElement.children[i].tagName.toUpperCase()) {
                    count++;
                  }
                }
                xpath = ""/""+tag+""[""+count+""]""+xpath;
                stack.push(parentElement);
              }
              return xpath;
            }";
            return element.ExecuteJavascriptAsync(script);
        }
    }
}
