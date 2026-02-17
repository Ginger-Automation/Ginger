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
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Exceptions;
using GingerCore.Actions;
using System;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.ActionHandlers
{
    internal sealed class ActGotoURLHandler
    {
        private readonly ActGotoURL _act;
        private readonly IBrowser _browser;

        internal ActGotoURLHandler(ActGotoURL act, IBrowser browser)
        {
            _act = act;
            _browser = browser;
        }

        internal async Task HandleAsync()
        {
            try
            {
                string url = GetTargetUrl();
                await _browser.CurrentWindow.CurrentTab.GoToURLAsync(url);
            }
            catch (Exception ex)
            {
                _act.Error = ex.Message;
            }
        }

        private string GetTargetUrl()
        {
            string url = _act.GetInputParamCalculatedValue("Value");

            if (string.IsNullOrEmpty(url))
            {
                throw new InvalidActionConfigurationException("Error: Provided URL is empty. Please provide valid URL.");
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                string httpsUrlFormat = "https://{0}";
                if (Uri.TryCreate(string.Format(httpsUrlFormat, url), UriKind.Absolute, out _))
                {
                    url = string.Format(httpsUrlFormat, url);
                }
                else
                {
                    throw new InvalidActionConfigurationException("Error: Invalid URL. Give valid URL(Complete URL)");
                }
            }

            return url;
        }
    }
}
