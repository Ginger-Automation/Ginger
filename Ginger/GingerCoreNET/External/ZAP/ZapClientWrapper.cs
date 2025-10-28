#region License
/*
Copyright © 2014-2025 European Support Limited

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

using OWASPZAPDotNetAPI;

namespace Amdocs.Ginger.CoreNET.External.ZAP
{
    public class ZapClientWrapper : IZapClient
    {
        private readonly ClientApi _client;

        public ZapClientWrapper(string host, int port, string apiKey)
        {
            _client = new ClientApi(host, port, apiKey);
        }

        public IApiResponse Version() => _client.core.version();
        public IApiResponse RecordsToScan() => _client.pscan.recordsToScan();

        // NOTE: ZAP API requires boolean flags as strings ("true"/"false") by design.
        public IApiResponse AccessUrl(string site, string recurse) => _client.core.accessUrl(site, recurse);
        public IApiResponse Urls(string baseUrl) => _client.core.urls(baseUrl);
        public IApiResponse Scan(string url, string recurse, string inscopeonly,
                                 string scanpolicyname, string method, string postdata, string contextId) =>
            _client.ascan.scan(url, recurse, inscopeonly, scanpolicyname, method, postdata, contextId);

        public IApiResponse ScanStatus(string scanId) => _client.ascan.status(scanId);
        public IApiResponse AlertsSummary(string targetUrl) => _client.alert.alertsSummary(targetUrl);

        public IApiResponse GenerateReport(string title, string template, string theme, string description, string contextName, string sites, string sections, string includedconfidences, string includedrisks, string reportfilename, string reportfilenamepattern, string reportdir, string display)
        {
            return _client.reports.generate(title, template, theme, description, contextName, sites,
                                           sections, includedconfidences, includedrisks,
                                           reportfilename, reportfilenamepattern, reportdir, display);
        }
    }

}
