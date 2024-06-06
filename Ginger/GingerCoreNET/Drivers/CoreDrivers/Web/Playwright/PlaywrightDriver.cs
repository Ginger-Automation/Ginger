using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright.ActionHandlers;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Drivers.Selenium.SeleniumBMP;
using GingerCore.Environments;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Microsoft.Playwright;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Profile;
using NPOI.OpenXmlFormats.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPlaywrightBrowser = Microsoft.Playwright.IBrowser;
using IPlaywrightBrowserContext = Microsoft.Playwright.IBrowserContext;
using IPlaywrightPage = Microsoft.Playwright.IPage;
using IPlaywrightDialog = Microsoft.Playwright.IDialog;
using IPlaywrightLocator = Microsoft.Playwright.ILocator;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using System.Reflection;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Exceptions;
using GingerCore.Actions.Common;
using OpenQA.Selenium.DevTools.V119.DOM;
using System.Drawing;
using NPOI.OpenXmlFormats.Dml.Chart;
using Amdocs.Ginger.Common.Drivers.CoreDrivers.Web;
using Amdocs.Ginger.CoreNET.RunLib;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright
{
    public sealed class PlaywrightDriver : GingerWebDriver, IVirtualDriver
    {

        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("Only for Chrome & Firefox | Set \"true\" to run the browser in background (headless mode) for faster Execution")]
        public bool HeadlessBrowserMode { get; set; }

        [UserConfigured]
        [UserConfiguredDescription("Proxy Server:Port")]
        public string? Proxy { get; set; }

        private PlaywrightBrowser? _browser;

        public override void StartDriver()
        {
            ValidateBrowserTypeSupport(BrowserType);

            IPlaywright playwright = Microsoft.Playwright.Playwright.CreateAsync().Result;

            BrowserTypeLaunchOptions launchOptions = BuildLaunchOptions();
            IPlaywrightBrowser playwrightBrowser = LaunchBrowser(playwright, BrowserType, launchOptions);

            _browser = new(playwrightBrowser, OnBrowserClose);
        }

        private void ValidateBrowserTypeSupport(WebBrowserType browserType)
        {
            IEnumerable<WebBrowserType> supportedBrowserTypes = GetSupportedBrowserTypes(Agent.eDriverType.Playwright);
            if (!supportedBrowserTypes.Contains(browserType))
            {
                throw new InvalidOperationException($"Browser type '{browserType}' is not supported for Playwright driver.");
            }
        }

        private BrowserTypeLaunchOptions BuildLaunchOptions()
        {
            BrowserTypeLaunchOptions launchOptions = new()
            {
                Args = new[] { "--start-maximized" },
                Headless = HeadlessBrowserMode,
                Timeout = DriverLoadWaitingTime * 1000, //convert to milliseconds
            };

            if (!string.IsNullOrEmpty(Proxy))
            {
                launchOptions.Proxy = new Proxy()
                {
                    Server = Proxy
                };
            }

            if (BrowserType == WebBrowserType.Chrome)
            {
                launchOptions.Channel = "chrome";
            }
            else if (BrowserType == WebBrowserType.Edge)
            {
                launchOptions.Channel = "msedge";
            }

            return launchOptions;
        }

        private IPlaywrightBrowser LaunchBrowser(IPlaywright playwright, WebBrowserType browserType, BrowserTypeLaunchOptions? launchOptions = null)
        {
            if (browserType == WebBrowserType.Chrome)
            {
                return playwright.Chromium.LaunchAsync(launchOptions).Result;
            }
            else if (browserType == WebBrowserType.FireFox)
            {
                return playwright.Firefox.LaunchAsync(launchOptions).Result;
            }
            else if (browserType == WebBrowserType.Edge)
            {
                return playwright.Chromium.LaunchAsync(launchOptions).Result;
            }
            else
            {
                throw new ArgumentException($"Unknown browser type '{BrowserType}'");
            }
        }

        public override bool IsRunning()
        {
            if (_browser == null)
            {
                return false;
            }

            if (_browser.IsClosed)
            {
                _browser = null;
                return false;
            }

            return true;
        }

        public override void CloseDriver()
        {
            if (!IsRunning())
            {
                return;
            }

            _browser?.CloseAsync().Wait();
            _browser = null;
        }

        private Task OnBrowserClose(IBrowser closedBrowser)
        {
            if (closedBrowser == _browser)
            {
                _browser = null;
            }

            return Task.CompletedTask;
        }

        public override Act GetCurrentElement()
        {
            throw new NotImplementedException();
        }

        public override string GetURL()
        {
            throw new NotImplementedException();
        }

        public override void HighlightActElement(Act act)
        {
            throw new NotImplementedException();
        }

        public override void RunAction(Act act)
        {
            if (!IsRunning())
            {
                throw new InvalidOperationException("cannot run action when driver is not started");
            }

            switch (act)
            {
                case ActBrowserElement actBrowserElement:
                    ActBrowserElementHandler actBrowserElementHandler = new(actBrowserElement, _browser!, new ActBrowserElementHandler.Context
                    {
                        BusinessFlow = BusinessFlow,
                        Environment = Environment,
                    });
                    actBrowserElementHandler.HandleAsync().Wait();
                    break;
                case ActUIElement actUIElement:
                    ActUIElementHandler actUIElementHandler = new(actUIElement, _browser!);
                    actUIElementHandler.HandleAsync().Wait();
                    break;
                default:
                    act.Error = $"Run Action Failed due to unrecognized action type - {act.GetType().Name}";
                    break;
            }
        }

        public bool CanStartAnotherInstance(out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }

        private sealed class PlaywrightBrowser : IBrowser
        {
            private readonly IPlaywrightBrowser _playwrightBrowser;
            private readonly IBrowser.OnBrowserClose _onBrowserClose;
            private readonly LinkedList<IBrowserWindow> _windows = [];
            private IBrowserWindow _currentWindow;
            private bool _isClosed = false;

            public IEnumerable<IBrowserWindow> Windows => _windows;

            public IBrowserWindow CurrentWindow => _currentWindow;

            public bool IsClosed => _isClosed;

            internal PlaywrightBrowser(IPlaywrightBrowser playwrightBrowser, IBrowser.OnBrowserClose onBrowserClose)
            {
                _playwrightBrowser = playwrightBrowser;
                _onBrowserClose = onBrowserClose;

                List<IPlaywrightBrowserContext> contexts = new(_playwrightBrowser.Contexts);
                foreach (IPlaywrightBrowserContext context in contexts)
                {
                    PlaywrightBrowserWindow window = new(context, OnWindowClose);
                    _windows.AddLast(window);
                }

                if (_windows.Count > 0)
                {
                    _currentWindow = _windows.Last!.Value;
                }
                else
                {
                    _currentWindow = Task.Run(() =>
                    {
                        //this code needs to be executed in a separate Task otherwise, it will cause a deadlock and freeze the calling thread
                        //check this for an example https://stackoverflow.com/a/43912280/12190808
                        return NewWindowAsync().Result;
                    }).Result;
                }
            }

            public async Task<IBrowserWindow> NewWindowAsync()
            {
                ThrowIfClosed();

                IPlaywrightBrowserContext context = await _playwrightBrowser.NewContextAsync(new BrowserNewContextOptions()
                {
                    ViewportSize = ViewportSize.NoViewport,
                });
                PlaywrightBrowserWindow window = new(context, OnWindowClose);
                _windows.AddLast(window);
                return window;
            }

            private Task OnWindowClose(IBrowserWindow closedWindow)
            {
                _windows.Remove(closedWindow);

                if (_windows.Count <= 0)
                {
                    return ClosePlaywrightBrowser();
                }

                if (closedWindow == _currentWindow)
                {
                    _currentWindow = _windows.Last!.Value;
                }

                return Task.CompletedTask;
            }

            public Task CloseAsync()
            {
                return CloseAllWindows();
            }

            private Task CloseAllWindows()
            {
                Task[] windowCloseTasks = new Task[_windows.Count];
                List<IBrowserWindow> windowsToClose = new(_windows);
                for (int index = 0; index < windowsToClose.Count; index++)
                {
                    windowCloseTasks[index] = windowsToClose[index].CloseAsync();
                }
                return Task.WhenAll(windowCloseTasks);
            }

            private async Task ClosePlaywrightBrowser()
            {
                if (_isClosed)
                {
                    return;
                }

                _isClosed = true;

                await _playwrightBrowser.CloseAsync();
                await _playwrightBrowser.DisposeAsync();
                await _onBrowserClose.Invoke(closedBrowser: this);
            }

            private void ThrowIfClosed()
            {
                if (_isClosed)
                {
                    throw new InvalidOperationException("Cannot perform operation, browser is already closed.");
                }
            }
        }

        private sealed class PlaywrightBrowserWindow : IBrowserWindow
        {
            private readonly IPlaywrightBrowserContext _playwrightBrowserContext;
            private readonly IBrowserWindow.OnWindowClose _onWindowClose;
            private readonly LinkedList<IBrowserTab> _tabs = [];
            private IBrowserTab _currentTab;
            private bool _isClosed = false;

            public IEnumerable<IBrowserTab> Tabs => _tabs;

            public IBrowserTab CurrentTab => _currentTab;

            public bool IsClosed => _isClosed;

            internal PlaywrightBrowserWindow(IPlaywrightBrowserContext playwrightBrowserContext, IBrowserWindow.OnWindowClose onWindowClose)
            {
                _playwrightBrowserContext = playwrightBrowserContext;
                _onWindowClose = onWindowClose;

                List<IPlaywrightPage> pages = new(_playwrightBrowserContext.Pages);
                foreach (IPlaywrightPage page in pages)
                {
                    PlaywrightBrowserTab tab = new(page, OnTabClosed);
                    _tabs.AddLast(tab);
                }

                if (_tabs.Count > 0)
                {
                    _currentTab = _tabs.Last!.Value;
                }
                else
                {
                    _currentTab = NewTabAsync().Result;
                }
            }

            public async Task<IBrowserTab> NewTabAsync(bool setAsCurrent = true)
            {
                ThrowIfClosed();

                IPlaywrightPage page = await _playwrightBrowserContext.NewPageAsync();
                PlaywrightBrowserTab tab = new(page, OnTabClosed);
                _tabs.AddLast(tab);

                if (setAsCurrent)
                {
                    _currentTab = tab;
                }

                return tab;
            }

            public async Task SetTabAsync(IBrowserTab tab) 
            {
                if (tab == null)
                {
                    throw new ArgumentNullException(paramName: nameof(tab));
                }

                IEnumerable<IBrowserTab> tabs = new List<IBrowserTab>(_tabs);
                
                IBrowserTab? tabToSwitch = null;
                foreach (IBrowserTab currentTab in tabs)
                {
                    if (currentTab == tab)
                    {
                        tabToSwitch = currentTab;
                        break;
                    }
                }

                if (tabToSwitch == null)
                {
                    throw new ArgumentException($"No matching tab found in the list of window tabs");
                }

                _currentTab = tabToSwitch;
                await ((PlaywrightBrowserTab)_currentTab).BringToFrontAsync();
            }

            public Task DeleteCookiesAsync()
            {
                ThrowIfClosed();

                return _playwrightBrowserContext.ClearCookiesAsync();
            }

            private Task OnTabClosed(IBrowserTab closedTab)
            {
                _tabs.Remove(closedTab);

                if (_tabs.Count <= 0)
                {
                    return ClosePlaywrightBrowserContext();
                }

                if (closedTab == _currentTab)
                {
                    _currentTab = _tabs.Last!.Value;
                }

                return Task.CompletedTask;
            }

            public Task CloseAsync()
            {
                return CloseAllTabs();
            }

            private Task CloseAllTabs()
            {
                Task[] tabCloseTasks = new Task[_tabs.Count];
                List<IBrowserTab> tabsToClose = new(_tabs);
                for (int index = 0; index < tabsToClose.Count; index++)
                {
                    tabCloseTasks[index] = tabsToClose[index].CloseAsync();
                }
                return Task.WhenAll(tabCloseTasks);
            }

            private async Task ClosePlaywrightBrowserContext() 
            {
                if (_isClosed)
                {
                    return;
                }

                _isClosed = true;

                await _playwrightBrowserContext.CloseAsync();
                await _onWindowClose.Invoke(closedWindow: this);
            }

            private void ThrowIfClosed()
            {
                if (_isClosed)
                {
                    throw new InvalidOperationException("Cannot perform operation, window is already closed.");
                }
            }
        }

        private sealed class PlaywrightBrowserTab : IBrowserTab
        {
            private readonly IPlaywrightPage _playwrightPage;
            private readonly IBrowserTab.OnTabClose _onTabClose;
            private readonly LinkedList<string> _consoleMessages = [];
            //private IFrameLocator? _currentFrameLocator;
            private IFrame _currentFrame;
            private bool _isClosed = false;

            public bool IsClosed => _isClosed;

            internal PlaywrightBrowserTab(IPlaywrightPage playwrightPage, IBrowserTab.OnTabClose onTabClose)
            {
                _playwrightPage = playwrightPage;
                _onTabClose = onTabClose;
                //_currentFrameLocator = null;
                _currentFrame = _playwrightPage.MainFrame;
                _playwrightPage.Console += OnConsoleMessage;
            }

            private void OnConsoleMessage(object? sender, IConsoleMessage e)
            {
                //TODO: Playwright - Selenium console logs contain timestamp and level. Try adding those in these as well so that we can have similar log structure
                _consoleMessages.AddLast(e.Text);
            }

            public Task BringToFrontAsync()
            {
                return _playwrightPage.BringToFrontAsync();
            }

            public Task<string> ExecuteJavascriptAsync(string script)
            {
                ThrowIfClosed();
                return _playwrightPage.EvaluateAsync<string>(script);
            }

            public Task<string> GetPageSourceAsync()
            {
                ThrowIfClosed();
                return _playwrightPage.ContentAsync();
            }

            public Task<string> GetTitleAsync()
            {
                ThrowIfClosed();
                return _playwrightPage.TitleAsync();
            }

            public Task<string> GetURLAsync()
            {
                ThrowIfClosed();
                return Task.FromResult(_playwrightPage.Url);
            }

            public async Task GoToURLAsync(string url)
            {
                ThrowIfClosed();
                await _playwrightPage.GotoAsync(url);
                _currentFrame = _playwrightPage.MainFrame;
            }

            public Task NavigateBackAsync()
            {
                ThrowIfClosed();
                return _playwrightPage.GoBackAsync();
            }

            public Task NavigateForwardAsync()
            {
                ThrowIfClosed();
                return _playwrightPage.GoForwardAsync();
            }

            public Task RefreshAsync()
            {
                ThrowIfClosed();
                return _playwrightPage.ReloadAsync();
            }

            public Task WaitTillLoadedAsync()
            {
                return _playwrightPage.WaitForLoadStateAsync(LoadState.Load);
            }

            public Task<string> GetConsoleLogsAsync()
            {
                return Task.FromResult(string.Join('\n', _consoleMessages));
            }

            public async Task<string> GetBrowserLogsAsync()
            {
                string script = "var performance = window.performance || window.mozPerformance || window.msPerformance || window.webkitPerformance || {}; var network = performance.getEntries() || {}; network;";

                JsonElement? logs = await _playwrightPage.EvaluateAsync(script);
                string rawLogs = string.Empty;
                if (logs.HasValue)
                {
                    JsonNode? jsonLogs = JsonNode.Parse(logs.Value.ToString());

                    if (jsonLogs != null && jsonLogs.GetValueKind() == JsonValueKind.Array)
                    {
                        JsonArray jsonArray = jsonLogs.AsArray();
                        foreach (JsonNode? item in jsonArray)
                        {
                            if (item == null || item.GetValueKind() != JsonValueKind.Object)
                            {
                                continue;
                            }
                        ((JsonObject)item).Remove("$id");
                        }
                    }

                    if (jsonLogs != null)
                    {
                        rawLogs = jsonLogs.ToJsonString();
                    }
                }
                return rawLogs;
            }

            public async Task<bool> SwitchFrameAsync(eLocateBy locateBy, string value)
            {
                IFrameLocator frameLocator;
                switch (locateBy)
                {
                    case eLocateBy.ByID:
                        //frameLocator = _playwrightPage.FrameLocator($"css=#{locateValue}");
                        frameLocator = _currentFrame.FrameLocator($"css=#{value}");
                        break;
                    case eLocateBy.ByTitle:
                        //frameLocator = _playwrightPage.FrameLocator($"css=iframe[title='{locateValue}']");
                        frameLocator = _currentFrame.FrameLocator($"css=iframe[title='{value}']");
                        break;
                    case eLocateBy.ByXPath:
                        //frameLocator = _playwrightPage.FrameLocator($"xpath={locateValue}");
                        frameLocator = _currentFrame.FrameLocator($"xpath={value}");
                        break;
                    case eLocateBy.ByUrl:
                        //frameLocator = _playwrightPage.FrameLocator($"css=iframe[src='{locateValue}']");
                        frameLocator = _currentFrame.FrameLocator($"css=iframe[src='{value}']");
                        break;
                    default:
                        throw new ArgumentException($"Frame locator '{locateBy}' is not supported for frames.");
                }

                bool wasLocated = await frameLocator.Owner.CountAsync() > 0;
                if (!wasLocated)
                {
                    return false;
                    
                }

                //_currentFrameLocator = frameLocator;

                IJSHandle jsHandle = await frameLocator.Owner.EvaluateHandleAsync("element => element");
                IElementHandle? elementHandle = jsHandle.AsElement();
                if (elementHandle == null)
                {
                    return false;
                }

                IFrame? frame = await elementHandle.ContentFrameAsync();
                if (frame == null)
                {
                    return false;
                }

                _currentFrame = frame;

                return true;
            }

            public Task SwitchToMainFrameAsync()
            {
                //_currentFrameLocator = null;
                _currentFrame = _playwrightPage.MainFrame;
                return Task.CompletedTask;
            }

            public Task SwitchToParentFrameAsync()
            {
                //if (_currentFrameLocator == null)
                //{
                //    return Task.CompletedTask;
                //}

                IFrame? parentFrame = _currentFrame.ParentFrame;
                if (parentFrame != null)
                {
                    _currentFrame = parentFrame;
                }

                return Task.CompletedTask;
            }

            public async Task<IEnumerable<IBrowserElement>> GetElementsAsync(eLocateBy locateBy, string value)
            {
                IPlaywrightLocator locator = await LocateElementAsync(locateBy, value);

                int matchedElementCount = await locator.CountAsync();
                IBrowserElement[] elements = new IBrowserElement[matchedElementCount];

                for (int index = 0; index < matchedElementCount; index++)
                {
                    elements[index] = new PlaywrightBrowserElement(locator.Nth(index));
                }

                return elements;
            }
                 
            private Task<IPlaywrightLocator> LocateElementAsync(eLocateBy locateBy, string value)
            {
                //if (_currentFrameLocator == null)
                //{
                //    return LocateElementInMainFrameAsync(locateBy, value);
                //}
                //else
                //{
                    return LocateElementInCurrentFrameAsync(locateBy, value);
                //}
            }

            private Task<IPlaywrightLocator> LocateElementInMainFrameAsync(eLocateBy locateBy, string value)
            {
                IPlaywrightLocator locator;
                switch (locateBy)
                {
                    case eLocateBy.ByID:
                        locator = _playwrightPage.MainFrame.Locator($"css=#{value}");
                        break;
                    case eLocateBy.ByCSS:
                        locator = _playwrightPage.MainFrame.Locator($"css={value}");
                        break;
                    case eLocateBy.ByXPath:
                        locator = _playwrightPage.MainFrame.Locator($"xpath={value}");
                        break;
                    default:
                        throw new ArgumentException($"Element locator '{locateBy}' is not supported.");
                }

                return Task.FromResult(locator);
            }

            private Task<IPlaywrightLocator> LocateElementInCurrentFrameAsync(eLocateBy locateBy, string value)
            {
                //if (_currentFrameLocator == null)
                //{
                //    throw new InvalidOperationException("Current frame is null");
                //}

                IPlaywrightLocator locator;
                switch (locateBy)
                {
                    case eLocateBy.ByID:
                        //locator = _currentFrameLocator.Locator($"css=#{value}");
                        locator = _currentFrame.Locator($"css=#{value}");
                        break;
                    case eLocateBy.ByCSS:
                        //locator = _currentFrameLocator.Locator($"css={value}");
                        locator = _currentFrame.Locator($"css={value}");
                        break;
                    case eLocateBy.ByXPath:
                        locator = _currentFrame.Locator($"xpath={value}");
                        break;
                    default:
                        throw new ArgumentException($"Element locator '{locateBy}' is not supported.");
                }

                return Task.FromResult(locator);
            }

            private static async Task<bool> DoesLocatorExistsAsync(IPlaywrightLocator locator)
            {
                return await locator.CountAsync() > 0;
            }

            public async Task CloseAsync()
            {
                if (_isClosed)
                {
                    return;
                }
                _isClosed = true;
                //_playwrightPage.Dialog -= OnDialog;
                await _playwrightPage.CloseAsync();
                await _onTabClose.Invoke(closedTab: this);
            }

            private void ThrowIfClosed()
            {
                if (_isClosed)
                {
                    throw new InvalidOperationException("Cannot perform operation, tab is already closed.");
                }
            }
        }

        private sealed class PlaywrightBrowserElement : IBrowserElement
        {
            private readonly IPlaywrightLocator _locator;

            internal PlaywrightBrowserElement(IPlaywrightLocator locator)
            {
                _locator = locator;
            }

            public Task ClickAsync()
            {
                return _locator.ClickAsync(new LocatorClickOptions
                {
                    Button = MouseButton.Left
                });
            }

            public Task ClickAsync(int x, int y)
            {
                return _locator.ClickAsync(new LocatorClickOptions
                {
                    Button = MouseButton.Left,
                    Position = new()
                    {
                        X = x,
                        Y = y
                    }
                });

            }

            public Task DoubleClickAsync()
            {
                return _locator.DblClickAsync(new LocatorDblClickOptions()
                {
                    Button = MouseButton.Left
                });
            }

            public Task DoubleClickAsync(int x, int y)
            {
                return _locator.DblClickAsync(new LocatorDblClickOptions
                {
                    Button = MouseButton.Left,
                    Position = new()
                    {
                        X = x,
                        Y = y
                    }
                });

            }

            public Task HoverAsync()
            {
                return _locator.HoverAsync();
            }

            public Task<bool> IsVisibleAsync()
            {
                return _locator.IsVisibleAsync();
            }

            public Task<bool> IsEnabledAsync()
            {
                return _locator.IsEnabledAsync();
            }

            public async Task<string> AttributeValueAsync(string name)
            {
                string? attributeValue = await _locator.GetAttributeAsync(name);
                if (attributeValue == null)
                {
                    return string.Empty;
                }

                return attributeValue;
            }

            public Task SetAttributeValueAsync(string name, string value)
            {
                return _locator.EvaluateAsync<string>($"element => element.setAttribute('{name}', '{value}')");
            }

            public async Task<Size> SizeAsync()
            {
                LocatorBoundingBoxResult? boundingBox = await _locator.BoundingBoxAsync();
                if (boundingBox == null)
                {
                    return new Size(width: 0, height: 0);
                }

                return new Size((int)boundingBox.Width, (int)boundingBox.Height);
            }

            public async Task<string> TextContentAsync()
            {
                string? content = await _locator.TextContentAsync();
                if (content == null)
                {
                    return string.Empty;
                }

                return content;
            }

            public Task<string> ExecuteJavascriptAsync(string script)
            {
                return _locator.EvaluateAsync<string>(script);
            }

            public Task<string> InnerTextAsync()
            {
                return _locator.InnerTextAsync();
            }

            public Task<string> InputValueAsync()
            {
                return _locator.InputValueAsync();
            }

            public Task RightClickAsync()
            {
                return _locator.ClickAsync(new LocatorClickOptions()
                {
                    Button = MouseButton.Right
                });
            }

            public Task<string> TagNameAsync()
            {
                return _locator.EvaluateAsync<string>("elem => elem.tagName");
            }

            public Task ScrollToViewAsync()
            {
                return _locator.ScrollIntoViewIfNeededAsync();
            }

            public Task FocusAsync()
            {
                return _locator.FocusAsync();
            }

            public Task ClearAsync()
            {
                return _locator.ClearAsync();
            }

            public async Task SelectByValueAsync(string value)
            {
                await AssertTagNameAsync(IBrowserElement.SelectTagName);
                await _locator.SelectOptionAsync(new SelectOptionValue()
                {
                    Value = value
                });
            }

            public async Task SelectByTextAsync(string text)
            {
                await AssertTagNameAsync(IBrowserElement.SelectTagName);
                await _locator.SelectOptionAsync(new SelectOptionValue()
                {
                    Label = text
                });
            }

            public async Task SelectByIndexAsync(int index)
            {
                await AssertTagNameAsync(IBrowserElement.SelectTagName);
                await _locator.SelectOptionAsync(new SelectOptionValue()
                {
                    Index = index
                });
            }

            public async Task SetCheckboxAsync(bool check)
            {
                await AssertTagNameAsync(IBrowserElement.InputTagName);
                await AssertTypeAttributeAsync("checkbox");

                await ExecuteJavascriptAsync($"element => element.checked={check.ToString().ToLower()}");
            }

            public Task SetTextAsync(string text)
            {
                return _locator.FillAsync(text);
            }

            private async Task AssertTagNameAsync(string expected)
            {
                string tagName = await TagNameAsync();
                if (!string.Equals(tagName, expected, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception($"Expected '{expected}' element but found '{tagName}'");
                }
            }

            public async Task AssertTypeAttributeAsync(string expected)
            {
                string type = await AttributeValueAsync(name: "type");
                if (!string.Equals(type, expected, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception($"Expected '{expected}' type but found '{type}'");
                }
            }
        }

        private sealed class PlaywrightBrowserDialog : IBrowserDialog
        {
            private readonly IPlaywrightDialog _playwrightDialog;
            private readonly IBrowserDialog.OnDialogHandle _onDialogHandle;

            internal PlaywrightBrowserDialog(IPlaywrightDialog playwrightDialog, IBrowserDialog.OnDialogHandle onDialogHandle)
            {
                _playwrightDialog = playwrightDialog;
                _onDialogHandle = onDialogHandle;
            }

            public Task<string> GetMessageAsync()
            {
                return Task.FromResult(_playwrightDialog.Message);
            }

            public async Task AcceptAsync()
            {
                await _playwrightDialog.AcceptAsync();
                await _onDialogHandle.Invoke(handledDialog: this);
            }

            public async Task DismissAsync()
            {
                await _playwrightDialog.DismissAsync();
                await _onDialogHandle.Invoke(handledDialog: this);
            }
        }
    }
}
