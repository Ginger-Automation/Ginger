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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.External.Configurations;
using Amdocs.Ginger.Common.VariablesLib;
using Amdocs.Ginger.CoreNET.External.ZAP;
using GingerCore;
using OWASPZAPDotNetAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GingerCoreNET.External.ZAP
{
    public class ZapProxyService
    {
        private readonly IZapClient _zapClient;
        private readonly ZAPConfiguration zAPConfiguration;
        private readonly string _zapHost;
        private readonly int _zapPort;
        private readonly string _zapApiKey;

        #region Constructors

        // Default constructor uses the real ZAP client
        public ZapProxyService()
        {
            zAPConfiguration = WorkSpace.Instance.SolutionRepository
                .GetAllRepositoryItems<ZAPConfiguration>().Count == 0
                ? new ZAPConfiguration()
                : WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<ZAPConfiguration>();

            _zapHost = GetHostFromUrl(ValueExpression.PasswordCalculation(zAPConfiguration.ZAPUrl));
            _zapPort = (int)GetPortFromUrl(ValueExpression.PasswordCalculation(zAPConfiguration.ZAPUrl));
            _zapApiKey = ValueExpression.PasswordCalculation(zAPConfiguration.ZAPApiKey);

            _zapClient = new ZapClientWrapper(_zapHost, _zapPort, _zapApiKey);
        }

        // Overloaded constructor for dependency injection (used in unit tests)
        public ZapProxyService(IZapClient zapClient)
        {
            _zapClient = zapClient;
        }

        #endregion
        /// <summary>
        /// Checks if OWASP ZAP is running and accessible via its API.
        /// </summary>
        /// <returns>True if ZAP is running and accessible, false otherwise.</returns>
        public bool IsZapRunning()
        {
            try
            {
                // A simple API call to check connectivity [4]
                IApiResponse version = _zapClient.Version();
                return !string.IsNullOrEmpty(version.ToString());
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error connecting to ZAP: {ex.Message}", ex);
                return false;
            }
        }

        public async Task WaitTillPassiveScanCompleted()
        {
            try
            {
                IApiResponse apiResponse = _zapClient.RecordsToScan();
                string tempVal = ((ApiResponseElement)apiResponse).Value;
                while (!tempVal.Equals("0"))
                {
                    Thread.Sleep(1000);
                    apiResponse = _zapClient.RecordsToScan();
                    tempVal = ((ApiResponseElement)apiResponse).Value;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error during passive scan: " + ex.Message, ex);
            }
        }

        public void GenerateZapReport(string siteToTest, string reportDir, string reportfilename)
        {

            try
            {
                _zapClient.GenerateReport(
                    "Security Test Report", "traditional-html", "", "Security scan report", "", siteToTest, "", "", "",
                    reportfilename, "", reportDir, ""
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating Security scan report: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Extracts the port number from a given URL or host:port string.
        /// Returns null if no port is specified or if parsing fails.
        /// </summary>
        /// <param name="url">The URL or host:port string.</param>
        /// <returns>The port number as string, or null if not found.</returns>
        public static int? GetPortFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            try
            {
                // Try to parse as Uri
                if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    if (uri.IsDefaultPort)
                    {
                        return null;
                    }
                    // Return port if it's not the default port (80 for HTTP, 443 for HTTPS)
                    return uri.Port;
                }
                // If not a full URL, try host:port
                var parts = url.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[1], out int port))
                {
                    return port;
                }
                // IPv6 [::1]:8080
                if (url.StartsWith("[") && url.Contains("]:"))
                {
                    int idx = url.LastIndexOf("]:");
                    if (idx > 0 && int.TryParse(url.Substring(idx + 2), out port))
                    {
                        return port;
                    }
                }
            }
            catch
            {
                // Ignore and return null
            }
            return null;
        }

        /// <summary>
        /// Extracts the host from a given URL or host:port string.
        /// </summary>
        /// <param name="url">The URL or host:port string.</param>
        /// <returns>The host as string, or null if not found.</returns>
        public static string GetHostFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            try
            {
                // Try to parse as Uri
                if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    return uri.Host;
                }
                // If not a full URL, try host:port
                // IPv6 [::1]:8080
                if (url.StartsWith("[") && url.Contains("]:"))
                {
                    int idx = url.LastIndexOf("]:");
                    if (idx > 0)
                    {
                        return url.Substring(0, idx + 1);
                    }
                }
                // host:port
                var parts = url.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[1], out _))
                {
                    return parts[0];
                }
                // Just host
                return url;
            }
            catch
            {
                // Ignore and return null
            }
            return null;
        }

        /// <summary>
        /// Adds a URL to the ZAP scan tree.
        /// Throws an exception if the URL is not added successfully.
        /// </summary>
        /// <param name="siteToTest">The site URL to add to the scan tree.</param>
        public void AddUrlToScanTree(string siteToTest)
        {
            _zapClient.AccessUrl(siteToTest, "false");
            var urls = GetUrlsFromScanTree(siteToTest);
            if (urls.Contains(siteToTest))
            {
                Reporter.ToLog(eLogLevel.INFO, $"{siteToTest} has been added to scan tree");
            }
            else
            {
                throw new InvalidOperationException($"{siteToTest} not added to scan tree, active scan will not be possible");
            }

        }

        /// <summary>
        /// Gets the list of URLs from the ZAP scan tree.
        /// </summary>
        /// <returns>List of URLs as strings.</returns>
        public List<string> GetUrlsFromScanTree(string baseUrl)
        {
            var apiResponse = _zapClient.Urls(baseUrl);
            var responses = ((ApiResponseList)apiResponse).List;
            return responses.Select(r => ((ApiResponseElement)r).Value).ToList();
        }

        /// <summary>
        /// Performs an active scan on the specified site.
        /// </summary>
        /// <param name="siteToTest">The site URL to scan.</param>
        public void PerformActiveScan(string siteToTest)
        {
            string url = siteToTest;
            string recurse = "true";
            string inscopeonly = "false";
            string scanpolicyname = ""; // or "Default Policy"
            string method = "";         // or "GET", "POST"
            string postdata = "";       // or actual POST data
            string contextId = "";      // or context ID as string

            var apiResponse = _zapClient.Scan(url, recurse, inscopeonly, scanpolicyname, method, postdata, contextId);
            string scanId = ((ApiResponseElement)apiResponse).Value;

            WaitTillActiveScanIsCompleted(scanId);
        }

        /// <summary>
        /// Waits until the active scan is completed for the given scan ID.
        /// </summary>
        /// <param name="scanId">The scan ID.</param>
        private void WaitTillActiveScanIsCompleted(string scanId)
        {
            var apiResponse = _zapClient.ScanStatus(scanId);
            string status = ((ApiResponseElement)apiResponse).Value;

            while (!status.Equals("100"))
            {
                Thread.Sleep(1000);
                apiResponse = _zapClient.ScanStatus(scanId);
                status = ((ApiResponseElement)apiResponse).Value;
                Reporter.ToLog(eLogLevel.INFO, "Active scan is in progress");
            }

            Reporter.ToLog(eLogLevel.INFO, "Active scan has completed");
        }

        public void ActiveScanAPI(string targetHost)
        {
            AddUrlToScanTree(targetHost);


            PerformActiveScan(targetHost);
        }
        public bool EvaluateScanResultWeb(string targetUrl, ObservableList<OperationValues> allowedAlertNames)
        {
            try
            {
                var summaryResponse = _zapClient.AlertsSummary(targetUrl);
                var alertSummary = (ApiResponseSet)summaryResponse;

                bool testPassed = true;

                foreach (var alertEntry in alertSummary.Dictionary)
                {
                    string alertName = alertEntry.Key;
                    string valueString = alertEntry.Value is ApiResponseElement element ? element.Value : alertEntry.Value?.ToString();
                    if (!int.TryParse(valueString, out int count))
                    {
                        continue;
                    }

                    if (!allowedAlertNames.Any(a => a.Value == alertName))
                    {
                        testPassed = false;
                    }
                }
                return testPassed;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error evaluating scan result for Web: {ex.Message}", ex);
                return false;
            }
        }

        public bool EvaluateScanResultAPI(string targetUrl, ObservableList<OperationValues> VulnerabilityThresholdList)
        {
            try
            {
                var summaryResponse = _zapClient.AlertsSummary(targetUrl);
                var alertSummary = (ApiResponseSet)summaryResponse;

                bool testPassed = true;

                foreach (var alertEntry in alertSummary.Dictionary)
                {
                    string alertName = alertEntry.Key;
                    string valueString = alertEntry.Value is ApiResponseElement element ? element.Value : alertEntry.Value?.ToString();
                    if (!int.TryParse(valueString, out int count))
                    {
                        continue;
                    }

                    if (count > 0 && (VulnerabilityThresholdList?.Any(a => string.Equals(a.Value, alertName, StringComparison.OrdinalIgnoreCase)) ?? false))
                    {
                        testPassed = false;
                    }
                }
                return testPassed;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error evaluating scan result for API: {ex.Message}", ex);
                return false;
            }
        }

        public bool EvaluateScanResult(string targetUrl)
        {
            var summaryResponse = _zapClient.AlertsSummary(targetUrl);
            var alertSummary = (ApiResponseSet)summaryResponse;
            return !string.IsNullOrEmpty(alertSummary.ToString());

        }

        public List<(string AlertName, int Count)> GetAlertSummary(string targetUrl)
        {
            var summaryResponse = _zapClient.AlertsSummary(targetUrl);
            var alertSummary = (ApiResponseSet)summaryResponse;
            var result = new List<(string, int)>();

            foreach (var alertEntry in alertSummary.Dictionary)
            {
                string alertName = alertEntry.Key;
                string valueString = alertEntry.Value is ApiResponseElement element ? element.Value : alertEntry.Value?.ToString();
                if (int.TryParse(valueString, out int count))
                {
                    result.Add((alertName, count));
                }
            }
            return result;
        }

    }
}