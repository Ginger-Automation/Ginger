#region License
/*
Copyright © 2014-2024 European Support Limited

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IPlaywrightBrowserContext = Microsoft.Playwright.IBrowserContext;
using IPlaywrightPage = Microsoft.Playwright.IPage;

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

            _playwrightBrowserContext.Page += OnNewPlaywrightPage;

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

        private void OnNewPlaywrightPage(object? sender, IPlaywrightPage newPlaywrightPage)
        {
            List<PlaywrightBrowserTab> tabs = new(_tabs.Cast<PlaywrightBrowserTab>());
            foreach (PlaywrightBrowserTab tab in tabs)
            {
                if (tab.PlaywrightPageEquals(newPlaywrightPage))
                {
                    return;
                }
            }
            PlaywrightBrowserTab newTab = new(newPlaywrightPage, OnTabClosed);
            _tabs.AddLast(newTab);

            _currentTab = newTab;
        }

        public async Task<IBrowserTab> NewTabAsync(bool setAsCurrent = true)
        {
            ThrowIfClosed();

            IPlaywrightPage newPlaywrightPage = await _playwrightBrowserContext.NewPageAsync();

            List<PlaywrightBrowserTab> tabs = new(_tabs.Cast<PlaywrightBrowserTab>());
            foreach (PlaywrightBrowserTab tab in tabs)
            {
                if (tab.PlaywrightPageEquals(newPlaywrightPage))
                {
                    return tab;
                }
            }

            PlaywrightBrowserTab newTab = new(newPlaywrightPage, OnTabClosed);
            _tabs.AddLast(newTab);

            if (setAsCurrent)
            {
                _currentTab = newTab;
            }

            return newTab;
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

            _playwrightBrowserContext.Page -= OnNewPlaywrightPage;
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
