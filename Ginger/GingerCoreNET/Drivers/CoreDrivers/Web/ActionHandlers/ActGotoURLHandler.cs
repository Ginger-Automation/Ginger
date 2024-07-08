using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Exceptions;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
