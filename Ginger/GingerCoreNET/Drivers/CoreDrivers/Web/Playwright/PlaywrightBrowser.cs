using Microsoft.Playwright;
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
    internal sealed class PlaywrightBrowser : IBrowser
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
                IBrowserWindow? newWindow = Task.Run(() =>
                {
                    try
                    {
                        //this code needs to be executed in a separate Task otherwise, it will cause a deadlock and freeze the calling thread
                        //check this for an example https://stackoverflow.com/a/43912280/12190808
                        return NewWindowAsync().Result;
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Error occurred while creating {nameof(IBrowserWindow)}", ex);
                        return null!;
                    }
                }).Result;

                if (newWindow == null)
                {
                    throw new Exception($"Error occurred while creating {nameof(IBrowserWindow)}");
                }
                _currentWindow = newWindow;
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
}
