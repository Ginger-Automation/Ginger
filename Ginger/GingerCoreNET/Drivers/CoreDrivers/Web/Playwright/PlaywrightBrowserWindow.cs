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
using Amdocs.Ginger.Common;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright
{
    internal sealed class PlaywrightBrowserWindow : IBrowserWindow
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
                IBrowserTab? newTab = Task.Run(() =>
                {
                    try
                    {
                        return NewTabAsync().Result;
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Error occurred while creating new {nameof(IBrowserTab)}", ex);
                        return null;
                    }
                }).Result;

                if (newTab == null)
                {
                    throw new Exception($"Error occurred while creating a new {nameof(IBrowserTab)}");
                }
                _currentTab = newTab;
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

        public Task SetTabAsync(IBrowserTab tab)
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
            return ((PlaywrightBrowserTab)_currentTab).BringToFrontAsync();
        }

        public Task BringToFrontAsync()
        {
            return ((PlaywrightBrowserTab)_currentTab).BringToFrontAsync();
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

}
