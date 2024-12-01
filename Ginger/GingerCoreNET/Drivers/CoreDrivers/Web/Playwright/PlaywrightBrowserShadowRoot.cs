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

using System;
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
