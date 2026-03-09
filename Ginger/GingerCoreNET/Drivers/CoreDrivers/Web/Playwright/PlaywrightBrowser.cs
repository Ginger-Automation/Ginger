#region License
/*
Copyright © 2014-2026 European Support Limited
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
using Amdocs.Ginger.Common.Drivers.CoreDrivers.Web;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright
{
    internal abstract class PlaywrightBrowser : IBrowser
    {
        private protected readonly string BrowserExecutableNotFoundErrorMessage = "Executable doesn't exist at";
        private protected readonly string FailedToInstallComponentErrorMessage = "Error occurred while executing playwright installation command for component";

        internal sealed class Options
        {
            internal IEnumerable<string>? Args { get; set; }

            internal bool Headless { get; set; }

            internal float? Timeout { get; set; }

            internal Proxy? Proxy { get; set; }

            public bool EnableVideoRecording { get; set; }
            internal string? RecordVideoDir { get; set; }
            internal RecordVideoSize RecordVideoSize { get; set; }
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
            var browserTypeString = browserType switch
            {
                WebBrowserType.Chrome or WebBrowserType.Edge => BrowserType.Chromium,
                WebBrowserType.FireFox => BrowserType.Firefox,
                _ => throw new ArgumentException($"Unknown browser type '{browserType}'"),
            };
            ExecutePlaywrightInstallation(browserTypeString);
        }

        private protected static void ExecutePlaywrightInstallation(string componentToInstall)
        {
            int exitCode = Microsoft.Playwright.Program.Main(new[] { "install", componentToInstall });
            if (exitCode != 0)
            {
                throw new PlaywrightException($"Error occurred while executing playwright installation command for component {componentToInstall}, exited with code {exitCode}");
            }
        }
    }
}
