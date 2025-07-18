using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.VariablesLib;
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
        private readonly ClientApi _zapClient;
        private readonly string _zapApiKey;
        private readonly string _zapHost;
        private readonly int _zapPort;
        public ZapProxyService()
        {
            //zAPConfiguration = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ZAPConfiguration>().Count == 0 ? new ZAPConfiguration() : WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<ZAPConfiguration>();
            //if (zAPConfiguration == null)
            //{
            //    throw new ArgumentNullException(nameof(zAPConfiguration), "ZAP Configuration cannot be null.");
            //}
            _zapHost = "127.0.0.1";
            _zapPort = 8080;
            _zapApiKey = "lq79gvufmrchhjr54a2qc7ruth";
            _zapClient = new ClientApi(_zapHost, _zapPort, _zapApiKey);

        }

        /// <summary>
        /// Checks if OWASP ZAP is running and accessible via its API.
        /// </summary>
        /// <returns>True if ZAP is running and accessible, false otherwise.</returns>
        public async Task<bool> IsZapRunningAsync()
        {
            try
            {
                // A simple API call to check connectivity [4]
                IApiResponse version = await Task.Run(() => _zapClient.core.version());
                return !string.IsNullOrEmpty(version.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to ZAP: {ex.Message}");
                return false;
            }
        }

        public void WaitTillPassiveScanCompleted()
        {
            try
            {
                IApiResponse apiResponse = _zapClient.pscan.recordsToScan();
                string tempVal = ((ApiResponseElement)apiResponse).Value;
                while (!tempVal.Equals("0"))
                {
                    Thread.Sleep(1000);
                    apiResponse = _zapClient.pscan.recordsToScan();
                    tempVal = ((ApiResponseElement)apiResponse).Value;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error during passive scan: " + ex.Message, ex);
            }
        }

        public void GenerateZapReport(string siteToTest, string reportDir, string reportfilename)
        {
            string title = "Security Test Report";
            string template = "traditional-html";
            try
            {
                _zapClient.reports.generate(
                    title, template, "", "Security scan report", "", siteToTest, "", "", "",
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
                return null;

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
            _zapClient.core.accessUrl(siteToTest, "false");
            var urls = GetUrlsFromScanTree(siteToTest);
            if (urls.Contains(siteToTest))
            {
                Console.WriteLine($"{siteToTest} has been added to scan tree");
            }
            else
            {
                throw new Exception($"{siteToTest} not added to scan tree, active scan will not be possible");
            }
        }

        /// <summary>
        /// Gets the list of URLs from the ZAP scan tree.
        /// </summary>
        /// <returns>List of URLs as strings.</returns>
        public List<string> GetUrlsFromScanTree(string baseUrl)
        {
            var apiResponse = _zapClient.core.urls(baseUrl);
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

            var apiResponse = _zapClient.ascan.scan(url, recurse, inscopeonly, scanpolicyname, method, postdata, contextId);
            string scanId = ((ApiResponseElement)apiResponse).Value;

            WaitTillActiveScanIsCompleted(scanId);
        }

        /// <summary>
        /// Waits until the active scan is completed for the given scan ID.
        /// </summary>
        /// <param name="scanId">The scan ID.</param>
        private void WaitTillActiveScanIsCompleted(string scanId)
        {
            var apiResponse = _zapClient.ascan.status(scanId);
            string status = ((ApiResponseElement)apiResponse).Value;

            while (!status.Equals("100"))
            {
                Thread.Sleep(1000);
                apiResponse = _zapClient.ascan.status(scanId);
                status = ((ApiResponseElement)apiResponse).Value;
                Console.WriteLine("Active scan is in progress");
            }

            Console.WriteLine("Active scan has completed");
        }

        public bool EvaluateScanResult(string targetUrl, ObservableList<OperationValues> allowedAlertNames)
        {
            var summaryResponse = _zapClient.alert.alertsSummary(targetUrl);
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


    }
}