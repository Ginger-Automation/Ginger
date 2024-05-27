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

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright
{
    public sealed class PlaywrightDriver : GingerWebDriver
    {
        private PlaywrightBrowser? _browser;

        public override void StartDriver()
        {
            IPlaywright playwright = Microsoft.Playwright.Playwright.CreateAsync().Result;
            IPlaywrightBrowser playwrightBrowser = playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Args = new[] { "--start-maximized" },
                Headless = false,
            }).Result;

            _browser = new(playwrightBrowser, OnBrowserClose);
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
                    ActUIElementHandler actUIElementHandler = new(actUIElement, _browser);
                    actUIElementHandler.HandleAsync().Wait();
                    break;
                default:
                    act.Error = $"Run Action Failed due to unrecognized action type - {act.GetType().Name}";
                    break;
            }
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
                    _currentWindow = NewWindowAsync().Result;
                }
            }

            public async Task<IBrowserWindow> NewWindowAsync(bool setAsCurrent = true)
            {
                ThrowIfClosed();

                //getting stuck at below line when Driver gets started because of action execution
                IPlaywrightBrowserContext context = await _playwrightBrowser.NewContextAsync(new BrowserNewContextOptions()
                {
                    ViewportSize = ViewportSize.NoViewport,
                });
                PlaywrightBrowserWindow window = new(context, OnWindowClose);
                _windows.AddLast(window);
                if (setAsCurrent)
                {
                    _currentWindow = window;
                }
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
            private IFrameLocator? _currentFrame;
            private bool _isClosed = false;

            public bool IsClosed => _isClosed;

            internal PlaywrightBrowserTab(IPlaywrightPage playwrightPage, IBrowserTab.OnTabClose onTabClose)
            {
                _playwrightPage = playwrightPage;
                _onTabClose = onTabClose;
                _currentFrame = null;
                _playwrightPage.Console += OnConsoleMessage;
            }

            private void OnConsoleMessage(object? sender, IConsoleMessage e)
            {
                //TODO: Playwright - Selenium console logs contain timestamp and level. Try adding those in these as well so that we can have similar log structure
                _consoleMessages.AddLast(e.Text);
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

            public Task GoToURLAsync(string url)
            {
                ThrowIfClosed();
                return _playwrightPage.GotoAsync(url);
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

            public async Task<bool> SwitchFrame(eLocateBy locateBy, string locateValue)
            {
                IFrameLocator frameLocator;
                switch (locateBy)
                {
                    case eLocateBy.ByID:
                        frameLocator = _playwrightPage.FrameLocator($"css=#{locateValue}");
                        break;
                    case eLocateBy.ByTitle:
                        frameLocator = _playwrightPage.FrameLocator($"css=iframe[title='{locateValue}']");
                        break;
                    case eLocateBy.ByXPath:
                        frameLocator = _playwrightPage.FrameLocator($"xpath={locateValue}");
                        break;
                    case eLocateBy.ByUrl:
                        frameLocator = _playwrightPage.FrameLocator($"css=iframe[src='{locateValue}']");
                        break;
                    default:
                        throw new ArgumentException($"Frame locator '{locateBy}' is not supported for frames.");
                }

                bool wasLocated = await frameLocator.Owner.CountAsync() > 0;
                if (wasLocated)
                {
                    _currentFrame = frameLocator;
                }

                return true;
            }

            public async Task ClickAsync(eLocateBy locateBy, string locateValue)
            {
                IPlaywrightLocator locator = await LocateElementAsync(locateBy, locateValue);
                bool wasFound = await DoesLocatorExistsAsync(locator);
                if (!wasFound)
                {
                    throw new NotFoundException($"No element found by locator '{locateBy}' and value '{locateValue}'");
                }

                await locator.ClickAsync(new LocatorClickOptions
                {
                    Button = MouseButton.Left
                });
            }

            public async Task DoubleClickAsync(eLocateBy locateBy, string locateValue)
            {
                IPlaywrightLocator locator = await LocateElementAsync(locateBy, locateValue);
                bool wasFound = await DoesLocatorExistsAsync(locator);
                if (!wasFound)
                {
                    throw NewElementNotFoundException(locateBy, locateValue);
                }

                await locator.DblClickAsync();
            }

            public async Task HoverAsync(eLocateBy locateBy, string locateValue)
            {
                IPlaywrightLocator locator = await LocateElementAsync(locateBy, locateValue);
                bool wasFound = await DoesLocatorExistsAsync(locator);
                if (!wasFound)
                {
                    throw NewElementNotFoundException(locateBy, locateValue);
                }

                await locator.HoverAsync();
            }

            public async Task<bool> IsVisibleAsync(eLocateBy locateBy, string locateValue)
            {
                IPlaywrightLocator locator = await LocateElementAsync(locateBy, locateValue);
                return await locator.IsVisibleAsync();
            }

            public async Task<bool> IsEnabledAsync(eLocateBy locateBy, string locateValue)
            {
                IPlaywrightLocator locator = await LocateElementAsync(locateBy, locateValue);
                return await locator.IsEnabledAsync();
            }

            public async Task<string?> GetAttributeValueAsync(eLocateBy locateBy, string locateValue, string attributeName)
            {
                IPlaywrightLocator locator = await LocateElementAsync(locateBy, locateValue);
                bool wasFound = await DoesLocatorExistsAsync(locator);
                if (!wasFound)
                {
                    throw NewElementNotFoundException(locateBy, locateValue);
                }

                return await locator.GetAttributeAsync(attributeName);
            }

            public async Task RightClickAsync(eLocateBy locateBy, string locateValue)
            {
                IPlaywrightLocator locator = await LocateElementAsync(locateBy, locateValue);
                bool wasFound = await DoesLocatorExistsAsync(locator);
                if (!wasFound)
                {
                    throw NewElementNotFoundException(locateBy, locateValue);
                }

                await locator.ClickAsync(new LocatorClickOptions()
                {
                    Button = MouseButton.Right
                });
            }

            private Task<IPlaywrightLocator> LocateElementAsync(eLocateBy locateBy, string value)
            {
                if (_currentFrame == null)
                {
                    return LocateElementInMainFrameAsync(locateBy, value);
                }
                else
                {
                    return LocateElementInCurrentFrameAsync(locateBy, value);
                }
            }

            private Task<IPlaywrightLocator> LocateElementInMainFrameAsync(eLocateBy locateBy, string value)
            {
                IPlaywrightLocator locator;
                switch (locateBy)
                {
                    case eLocateBy.ByID:
                        locator = _playwrightPage.MainFrame.Locator($"css=#{value}");
                        break;
                    default:
                        throw new ArgumentException($"Element locator '{locateBy}' is not supported.");
                }

                return Task.FromResult(locator);
            }

            private Task<IPlaywrightLocator> LocateElementInCurrentFrameAsync(eLocateBy locateBy, string value)
            {
                if (_currentFrame == null)
                {
                    throw new InvalidOperationException("Current frame is null");
                }

                IPlaywrightLocator locator;
                switch (locateBy)
                {
                    case eLocateBy.ByID:
                        locator = _currentFrame.Locator($"css=#{value}");
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

            private static NotFoundException NewElementNotFoundException(eLocateBy locateBy, string value)
            {
                return new NotFoundException($"No element found by locator '{locateBy}' and value '{value}'");
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
