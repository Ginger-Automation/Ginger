using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPlaywrightLocator = Microsoft.Playwright.ILocator;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright
{
    internal sealed class PlaywrightBrowserShadowRoot : IBrowserShadowRoot
    {
        private readonly IPlaywrightLocator _playwrightLocator;

        internal PlaywrightBrowserShadowRoot(IPlaywrightLocator playwrightLocator)
        {
            _playwrightLocator = playwrightLocator;
        }

        public async Task<string> HTML()
        {
            if (!await ShadowRootExists())
            {
                throw new InvalidOperationException($"No shadow root exists for the element");
            }

            return await _playwrightLocator.EvaluateAsync<string>("element => element.shadowRoot.getHTML()");
        }

        private Task<bool> ShadowRootExists()
        {
            return _playwrightLocator.EvaluateAsync<bool>("element => element.shadowRoot != null");
        }
    }
}
