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
                    ActBrowserElementHandler handler = new(actBrowserElement, _browser!, new ActBrowserElementHandler.Context
                    {
                        BusinessFlow = BusinessFlow,
                        Environment = Environment,
                    });
                    handler.HandleAsync().Wait();
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
            private bool _isClosed = false;

            public bool IsClosed => _isClosed;

            internal PlaywrightBrowserTab(IPlaywrightPage playwrightPage, IBrowserTab.OnTabClose onTabClose)
            {
                _playwrightPage = playwrightPage;
                _onTabClose = onTabClose;
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

            public async Task CloseAsync()
            {
                if (_isClosed)
                {
                    return;
                }
                _isClosed = true;
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

        public sealed class ActHandleContext
        {
            public required ProjEnvironment Environment { get; init; }

            public required BusinessFlow BusinessFlow { get; init; }
            
            public required IPlaywrightBrowser Browser { get; init; }

            public IPlaywrightBrowserContext? CurrentBrowserContext { get; private set; }

            public IPlaywrightPage? CurrentPage { get; private set; }

            public ActHandleContext() { }

            public async Task<IPlaywrightPage> CreatePageAsync()
            {
                if (CurrentBrowserContext == null)
                {
                    await CreateBrowserContextAsync();
                }
                CurrentPage = await CurrentBrowserContext!.NewPageAsync();
                return CurrentPage;
            }

            public async Task<IPlaywrightBrowserContext> CreateBrowserContextAsync()
            {
                CurrentBrowserContext = await Browser.NewContextAsync(new BrowserNewContextOptions()
                {
                    ViewportSize = ViewportSize.NoViewport
                });
                return CurrentBrowserContext;
            }
        }
    }
}
