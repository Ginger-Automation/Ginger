using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPlaywrightBrowserContext = Microsoft.Playwright.IBrowserContext;
using IPlaywrightBrowserType = Microsoft.Playwright.IBrowserType;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Drivers.CoreDrivers.Web;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright
{
    internal sealed class PlaywrightBrowser : IBrowser
    {
        internal sealed class Options
        {
            internal IEnumerable<string>? Args { get; set; }

            internal bool Headless { get; set; }

            internal float? Timeout { get; set; }

            internal Proxy? Proxy { get; set; }
        }

        private readonly IPlaywright _playwright;
        private readonly WebBrowserType _browserType;
        private readonly Options? _options;
        private readonly LinkedList<IBrowserWindow> _windows;
        private readonly IBrowser.OnBrowserClose? _onBrowserClose;
        private IBrowserWindow _currentWindow;
        private bool _isClosed = false;

        public IEnumerable<IBrowserWindow> Windows => _windows;

        public IBrowserWindow CurrentWindow => _currentWindow;

        public bool IsClosed => _isClosed;

        internal PlaywrightBrowser(IPlaywright playwright, WebBrowserType browserType, Options? options = null, IBrowser.OnBrowserClose? onBrowserClose = null)
        {
            _playwright = playwright;
            _browserType = browserType;
            _options = options;
            _onBrowserClose = onBrowserClose;

            _windows = [];

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

        public async Task<IBrowserWindow> NewWindowAsync(bool setAsCurrent = true)
        {
            ThrowIfClosed();

            IPlaywrightBrowserType playwrightBrowserType = GetPlaywrightBrowserType();
            BrowserTypeLaunchPersistentContextOptions? launchOptions = BuildPlaywrightBrowserContextLaunchOptions();

            IPlaywrightBrowserContext context = await playwrightBrowserType.LaunchPersistentContextAsync(userDataDir: string.Empty, launchOptions);

            PlaywrightBrowserWindow window = new(context, OnWindowClose);
            _windows.AddLast(window);

            if (setAsCurrent)
            {
                _currentWindow = window;
            }

            return window;
        }

        private BrowserTypeLaunchPersistentContextOptions? BuildPlaywrightBrowserContextLaunchOptions()
        {
            if (_options == null)
            {
                return null;
            }

            BrowserTypeLaunchPersistentContextOptions launchOptions = new()
            {
                Args = _options.Args,
                Headless = _options.Headless,
                Timeout = _options.Timeout,
                Proxy = _options.Proxy,
                ViewportSize = ViewportSize.NoViewport,
            };

            if (_browserType == WebBrowserType.Chrome)
            {
                launchOptions.Channel = "chrome";
            }
            else if (_browserType == WebBrowserType.Edge)
            {
                launchOptions.Channel = "msedge";
            }

            return launchOptions;
        }

        private IPlaywrightBrowserType GetPlaywrightBrowserType()
        {
            switch (_browserType)
            {
                case WebBrowserType.Chrome:
                case WebBrowserType.Edge:
                    return _playwright.Chromium;
                case WebBrowserType.FireFox:
                    return _playwright.Firefox;
                default:
                    throw new InvalidOperationException();
            }
        }

        public async Task SetWindowAsync(IBrowserWindow window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(paramName: nameof(window));
            }

            IEnumerable<IBrowserWindow> windows = new List<IBrowserWindow>(_windows);

            IBrowserWindow? windowToSwitch = null;
            foreach (IBrowserWindow currentWindow in windows)
            {
                if (currentWindow == window)
                {
                    windowToSwitch = currentWindow;
                    break;
                }
            }

            if (windowToSwitch == null)
            {
                throw new ArgumentException($"No matching window found in the list of browser windows");
            }

            _currentWindow = windowToSwitch;
            await ((PlaywrightBrowserWindow)_currentWindow).BringToFrontAsync();
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

            _playwright.Dispose();
            if (_onBrowserClose != null)
            {
                await _onBrowserClose.Invoke(closedBrowser: this);
            }
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
