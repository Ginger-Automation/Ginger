using Amdocs.Ginger.Common.Drivers.CoreDrivers.Web;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright
{
    internal abstract class PlaywrightBrowser : IBrowser
    {
        private protected readonly string BrowserExecutableNotFoundErrorMessage = "Executable doesn't exist at";

        internal sealed class Options
        {
            internal IEnumerable<string>? Args { get; set; }

            internal bool Headless { get; set; }

            internal float? Timeout { get; set; }

            internal Proxy? Proxy { get; set; }
        }

        public IEnumerable<IBrowserWindow> Windows => _windows;
        public abstract IBrowserWindow CurrentWindow { get; }
        public bool IsClosed => _isClosed;

        private protected readonly IPlaywright _playwright;
        private protected readonly WebBrowserType _browserType;
        private protected readonly Options? _options;
        private protected readonly LinkedList<IBrowserWindow> _windows;
        private protected readonly IBrowser.OnBrowserClose? _onBrowserClose;
        private protected bool _isClosed = false;

        internal PlaywrightBrowser(IPlaywright playwright, WebBrowserType browserType, Options? options = null, IBrowser.OnBrowserClose? onBrowserClose = null)
        {
            _playwright = playwright;
            _browserType = browserType;
            _options = options;
            _onBrowserClose = onBrowserClose;

            _windows = [];
        }

        public abstract Task CloseAsync();
        public abstract Task<IBrowserWindow> NewWindowAsync(bool setAsCurrent = true);
        public abstract Task SetWindowAsync(IBrowserWindow window);

        private protected void ExecutePlaywrightInstallationCommand(WebBrowserType browserType)
        {
            string browserTypeString;
            switch (browserType)
            {
                case WebBrowserType.Chrome:
                case WebBrowserType.Edge:
                    browserTypeString = BrowserType.Chromium;
                    break;
                case WebBrowserType.FireFox:
                    browserTypeString = BrowserType.Firefox;
                    break;
                default:
                    throw new ArgumentException($"Unknown browser type '{browserType}'");
            }

            int exitCode = Program.Main(new[] { "install", browserTypeString });
            if (exitCode != 0)
            {
                throw new Exception($"Error occurred while executing playwright installation command, exited with code {exitCode}");
            }
        }
    }
}
