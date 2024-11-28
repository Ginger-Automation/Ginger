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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Exceptions;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Environments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.ActionHandlers
{
    internal sealed class ActBrowserElementHandler
    {
        internal readonly struct Context : IEquatable<Context>
        {
            internal required ProjEnvironment Environment { get; init; }

            internal required BusinessFlow BusinessFlow { get; init; }

            public bool Equals(Context other)
            {
                return
                    Environment == other.Environment &&
                    BusinessFlow == other.BusinessFlow;
            }

            public override bool Equals(object? obj)
            {
                if (obj is not Context other)
                {
                    return false;
                }

                return Equals(other);
            }

            public override int GetHashCode()
            {
                HashCode hashCode = new();
                hashCode.Add(Environment.GetHashCode());
                hashCode.Add(BusinessFlow.GetHashCode());
                return hashCode.ToHashCode();
            }
        }

        private readonly ActBrowserElement _act;
        private readonly IBrowser _browser;
        private readonly Context _context;

        internal ActBrowserElementHandler(ActBrowserElement act, IBrowser browser, Context context)
        {
            _act = act;
            _browser = browser;
            _context = context;
        }

        internal async Task HandleAsync()
        {
            try
            {
                switch (_act.ControlAction)
                {
                    case ActBrowserElement.eControlAction.GotoURL:
                        await HandleGotoUrlOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.OpenURLNewTab:
                        await HandleOpenUrlInNewTabOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.GetPageURL:
                        await HandleGetPageUrlOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.GetWindowTitle:
                        await HandleGetWindowTitleOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.NavigateBack:
                        await HandleNavigateBackOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.Refresh:
                        await HandleRefreshOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.DeleteAllCookies:
                        await HandleDeleteAllCookiesOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.RunJavaScript:
                        await HandleRunJavascriptionOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.GetPageSource:
                        await HandleGetPageSourceOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.Close:
                        await HandleCloseOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.CloseTabExcept:
                        await HandleCloseTabExceptOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.CloseAll:
                        await HandleCloseAllOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.CheckPageLoaded:
                        await HandleCheckPageLoadedOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.GetConsoleLog:
                        await HandleGetConsoleLogOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.GetBrowserLog:
                        await HandleGetBrowserLogOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.SwitchFrame:
                        await HandleSwitchFrameOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.SwitchToDefaultFrame:
                        await HandleSwitchToDefaultFrameOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.SwitchToParentFrame:
                        await HandleSwitchToParentFrameOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.SwitchWindow:
                        await HandleSwitchWindowOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.SwitchToDefaultWindow:
                        await HandleSwitchToDefaultWindowOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.AcceptMessageBox:
                        await HandleAcceptMessageBoxOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.DismissMessageBox:
                        await HandleDismissMessageBoxOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.GetMessageBoxText:
                        string AlertBoxText = HandleGetMessageBoxTextOperation();
                        _act.AddOrUpdateReturnParamActual("Actual", AlertBoxText);
                        if (_act.GetReturnParam("Actual") == null)
                        {
                            _act.AddOrUpdateReturnParamActual("Actual", AlertBoxText);
                        }
                        break;
                    case ActBrowserElement.eControlAction.SetAlertBoxText:
                        string MessageBoxText = _act.GetInputParamCalculatedValue("Value");
                        await HandleSetMessageBoxTextOperationAsync(MessageBoxText);
                        break;

                    case ActBrowserElement.eControlAction.StartMonitoringNetworkLog:
                        await HandleStartMonitoringNetworkLogOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.GetNetworkLog:
                        await HandleGetNetworkLogOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.StopMonitoringNetworkLog:
                        await HandleStopMonitoringNetworkLogOperationAsync();
                        break;
                    default:
                        string operationName = Common.GeneralLib.General.GetEnumValueDescription(typeof(ActBrowserElement.eControlAction), _act.ControlAction);
                        _act.Error = $"Operation '{operationName}' is not supported";
                        break;
                }
            }
            catch (Exception ex)
            {
                _act.Error = ex.Message;
            }
        }

        private async Task HandleGotoUrlOperationAsync()
        {
            string url = GetTargetUrl();
            ActBrowserElement.eGotoURLType gotoUrlType = Enum.Parse<ActBrowserElement.eGotoURLType>(_act.GetInputParamValue(ActBrowserElement.Fields.GotoURLType));

            if (gotoUrlType == ActBrowserElement.eGotoURLType.NewTab)
            {
                await _browser.CurrentWindow.NewTabAsync();
            }
            else if (gotoUrlType == ActBrowserElement.eGotoURLType.NewWindow)
            {
                await _browser.NewWindowAsync(setAsCurrent: true);
            }
            await _browser.CurrentWindow.CurrentTab.GoToURLAsync(url);
        }

        private string GetTargetUrl()
        {
            string url = _act.GetInputParamValue(ActBrowserElement.Fields.URLSrc);

            bool urlconfiguredViaPOM =
                !string.IsNullOrEmpty(url) &&
                string.Equals(url, ActBrowserElement.eURLSrc.UrlPOM.ToString());

            if (urlconfiguredViaPOM)
            {
                string pomIdString = _act.GetInputParamCalculatedValue(ActBrowserElement.Fields.PomGUID);
                bool pomIdIsValid = Guid.TryParse(pomIdString, out Guid pomId);

                if (!pomIdIsValid)
                {
                    throw new InvalidActionConfigurationException("Error: Selected POM not found (Empty or Invalid POM Guid). Please select valid POM.");
                }

                url = GetTargetUrlFromPOM(pomId);
            }
            else
            {
                url = _act.GetInputParamCalculatedValue("Value");
            }

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

        private string GetTargetUrlFromPOM(Guid pomId)
        {
            ApplicationPOMModel pom = WorkSpace
                .Instance
                .SolutionRepository
                .GetRepositoryItemByGuid<ApplicationPOMModel>(pomId);

            if (pom == null)
            {
                throw new InvalidActionConfigurationException("Error: Selected POM not found. Please select valid POM.");
            }

            string url = GingerCore.ValueExpression.Calculate(_context.Environment, _context.BusinessFlow, pom.PageURL, DSList: null);
            return url;
        }

        private async Task HandleOpenUrlInNewTabOperationAsync()
        {
            string url = GetTargetUrl();

            await _browser.CurrentWindow.NewTabAsync();
            await _browser.CurrentWindow.CurrentTab.GoToURLAsync(url);
        }

        private async Task HandleGetPageUrlOperationAsync()
        {
            string url = await _browser.CurrentWindow.CurrentTab.URLAsync();

            _act.AddOrUpdateReturnParamActual("PageURL", url);

            Uri? uri = null;
            try
            {
                uri = new(url);
            }
            catch (Exception ex) when (ex is ArgumentNullException or UriFormatException) { }

            if (uri != null)
            {
                _act.AddOrUpdateReturnParamActual("Host", uri.Host);
                _act.AddOrUpdateReturnParamActual("Path", uri.LocalPath);
                _act.AddOrUpdateReturnParamActual("PathWithQuery", uri.PathAndQuery);
            }
        }

        private async Task HandleGetWindowTitleOperationAsync()
        {
            string title = await _browser.CurrentWindow.CurrentTab.TitleAsync();
            _act.AddOrUpdateReturnParamActual("Actual", title);
        }

        private Task HandleNavigateBackOperationAsync()
        {
            return _browser.CurrentWindow.CurrentTab.NavigateBackAsync();
        }

        private Task HandleRefreshOperationAsync()
        {
            return _browser.CurrentWindow.CurrentTab.RefreshAsync();
        }

        private Task HandleDeleteAllCookiesOperationAsync()
        {
            return _browser.CurrentWindow.DeleteCookiesAsync();
        }

        private async Task HandleRunJavascriptionOperationAsync()
        {
            string script = _act.GetInputParamCalculatedValue("Value");
            if (string.IsNullOrEmpty(script))
            {
                return;
            }

            string? output = null;
            try
            {
                output = await _browser.CurrentWindow.CurrentTab.ExecuteJavascriptAsync(script);
            }
            catch (Exception ex)
            {
                _act.Error = "Error: Failed to run the provided Javascript";
                Reporter.ToLog(eLogLevel.ERROR, "Error: Failed to run the provided Javascript", ex);
            }

            if (!string.IsNullOrEmpty(output))
            {
                _act.AddOrUpdateReturnParamActual("Actual", output);
            }
        }

        private async Task HandleGetPageSourceOperationAsync()
        {
            string content = await _browser.CurrentWindow.CurrentTab.PageSourceAsync();
            if (!string.IsNullOrEmpty(content))
            {
                if (content.StartsWith("<!DOCTYPE html>"))
                {
                    content = content["<!DOCTYPE html>".Length..];
                }
                _act.AddOrUpdateReturnParamActual("PageSource", content);
            }
        }

        private Task HandleCloseOperationAsync()
        {
            return _browser.CurrentWindow.CurrentTab.CloseAsync();
        }

        private async Task HandleCloseTabExceptOperationAsync()
        {
            if (_act.LocateBy is not eLocateBy.ByTitle and not eLocateBy.ByUrl)
            {
                throw new InvalidActionConfigurationException($"Error: Locator {_act.LocateBy} is not supported, use {eLocateBy.ByTitle} or {eLocateBy.ByUrl}.");
            }

            if (string.IsNullOrEmpty(_act.ValueForDriver) && string.IsNullOrEmpty(_act.LocateValueCalculated))
            {
                throw new InvalidActionConfigurationException("Error: The window value to search for is missing.");
            }

            string excludedValue;
            if (!string.IsNullOrEmpty(_act.LocateValueCalculated))
            {
                excludedValue = _act.LocateValueCalculated;
            }
            else
            {
                excludedValue = _act.ValueForDriver;
            }

            List<IBrowserTab> tabsToClose = [];
            foreach (IBrowserWindow window in _browser.Windows)
            {
                foreach (IBrowserTab tab in window.Tabs)
                {
                    string tabValue = string.Empty;
                    if (_act.LocateBy == eLocateBy.ByTitle)
                    {
                        tabValue = await tab.TitleAsync();

                    }
                    else if (_act.LocateBy == eLocateBy.ByUrl)
                    {
                        tabValue = await tab.URLAsync();
                    }

                    if (!string.IsNullOrEmpty(tabValue) && tabValue.Contains(excludedValue, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    tabsToClose.Add(tab);
                }
            }

            Task[] tabCloseTasks = new Task[tabsToClose.Count];
            for (int index = 0; index < tabsToClose.Count; index++)
            {
                tabCloseTasks[index] = tabsToClose[index].CloseAsync();
            }
            await Task.WhenAll(tabCloseTasks);
        }

        private Task HandleCloseAllOperationAsync()
        {
            List<IBrowserWindow> windows = new(_browser.Windows);
            Task[] closeWindowTasks = new Task[windows.Count];
            for (int index = 0; index < windows.Count; index++)
            {
                IBrowserWindow window = windows[index];
                closeWindowTasks[index] = window.CloseAsync();
            }
            return Task.WhenAll(closeWindowTasks);
        }

        private Task HandleCheckPageLoadedOperationAsync()
        {
            return _browser.CurrentWindow.CurrentTab.WaitTillLoadedAsync();
        }

        private async Task HandleGetConsoleLogOperationAsync()
        {
            string logs = await _browser.CurrentWindow.CurrentTab.ConsoleLogsAsync();
            _act.AddOrUpdateReturnParamActual("Console logs", logs);
        }

        private async Task HandleGetBrowserLogOperationAsync()
        {
            string logs = await _browser.CurrentWindow.CurrentTab.BrowserLogsAsync();
            if (string.IsNullOrEmpty(logs))
            {
                return;
            }

            _act.AddOrUpdateReturnParamActual("Raw Response", Newtonsoft.Json.JsonConvert.SerializeObject(logs));

            JsonNode? jsonLogs = JsonNode.Parse(logs);
            if (jsonLogs == null || jsonLogs.GetValueKind() != JsonValueKind.Array)
            {
                return;
            }

            JsonArray jsonArray = jsonLogs.AsArray();
            foreach (JsonNode? item in jsonArray)
            {
                if (item == null || item.GetValueKind() != JsonValueKind.Object)
                {
                    continue;
                }

                JsonObject jsonObject = item.AsObject();
                if (!jsonObject.TryGetPropertyValue("name", out JsonNode? nameProperty) || nameProperty == null || nameProperty.GetValueKind() != JsonValueKind.String)
                {
                    continue;
                }

                var urlArray = nameProperty.GetValue<string>().Split('/');
                var urlString = string.Empty;
                if (urlArray.Length <= 0)
                {
                    continue;
                }

                urlString = urlArray[^1];
                if (string.IsNullOrEmpty(urlString) && urlArray.Length > 1)
                {
                    urlString = urlArray[^2];
                }
                foreach (var property in jsonObject)
                {
                    if (property.Value == null)
                    {
                        continue;
                    }
                    _act.AddOrUpdateReturnParamActual($"{urlString}:[{property.Key}]", property.Value.ToString());
                }
            }
        }

        private async Task HandleSwitchFrameOperationAsync()
        {
            bool wasSwitched = await _browser.CurrentWindow.CurrentTab.SwitchFrameAsync(_act.LocateBy, _act.LocateValueCalculated);
            if (!wasSwitched)
            {
                _act.Error = $"Failed to switch with locate '{_act.LocateBy}' and value '{_act.LocateValueCalculated}'";
            }
        }

        private Task HandleSwitchToDefaultFrameOperationAsync()
        {
            return _browser.CurrentWindow.CurrentTab.SwitchToMainFrameAsync();
        }

        private Task HandleSwitchToParentFrameOperationAsync()
        {
            return _browser.CurrentWindow.CurrentTab.SwitchToParentFrameAsync();
        }

        private async Task HandleSwitchWindowOperationAsync()
        {
            string targetTitle = _act.LocateValueCalculated;
            if (string.IsNullOrEmpty(targetTitle))
            {
                targetTitle = _act.ValueForDriver;
            }

            if (string.IsNullOrEmpty(targetTitle))
            {
                _act.Error = "Error: The window title to search for is missing.";
                return;
            }

            IBrowserWindow? targetWindow = null;
            IBrowserTab? targetTab = null;

            IEnumerable<IBrowserWindow> windows = new List<IBrowserWindow>(_browser.Windows);
            foreach (IBrowserWindow window in windows)
            {
                IEnumerable<IBrowserTab> tabs = new List<IBrowserTab>(window.Tabs);
                foreach (IBrowserTab tab in tabs)
                {
                    string tabTitle = await tab.TitleAsync();
                    if (tabTitle != null && tabTitle.Contains(targetTitle, StringComparison.OrdinalIgnoreCase))
                    {
                        targetWindow = window;
                        targetTab = tab;
                        break;
                    }
                }
            }

            if (targetWindow == null || targetTab == null)
            {
                _act.Error = $"Error: Window with the title '{targetTitle}' was not found.";
                return;
            }

            if (targetWindow != _browser.CurrentWindow)
            {
                await _browser.SetWindowAsync(targetWindow);
            }

            if (targetTab != _browser.CurrentWindow.CurrentTab)
            {
                await _browser.CurrentWindow.SetTabAsync(targetTab);
            }
        }

        private async Task HandleSwitchToDefaultWindowOperationAsync()
        {
            IBrowserTab? firstTab = _browser.CurrentWindow.Tabs.FirstOrDefault();
            if (firstTab == null)
            {
                _act.Error = "Unable to find the default tab.";
                return;
            }

            await _browser.CurrentWindow.SetTabAsync(firstTab);
        }
        /// <summary>
        /// This asynchronous method accepts a message box (such as a dialog box) in the current browser tab. 
        /// If an error occurs, it logs the error and updates the Error property.
        /// </summary>
        /// <returns></returns>
        private async Task HandleAcceptMessageBoxOperationAsync()
        {
            try
            {
                await ((PlaywrightBrowserTab)_browser!.CurrentWindow.CurrentTab).AcceptMessageBox();
            }
            catch (Exception ex)
            {
                _act.Error = $"Error when {MethodBase.GetCurrentMethod().Name}.";
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return;
            }
        }
        /// <summary>
        /// This asynchronous method dismisses a message box (closes it) in the current browser tab. 
        /// If an error occurs, it logs the error and updates the Error property.
        /// </summary>
        /// <returns></returns>
        private async Task HandleDismissMessageBoxOperationAsync()
        {
            try
            {
                await ((PlaywrightBrowserTab)_browser!.CurrentWindow.CurrentTab).DismissMessageBox();
            }
            catch (Exception ex)
            {
                _act.Error = $"Error when {MethodBase.GetCurrentMethod().Name}.";
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return;
            }
        }
        /// <summary>
        /// This method retrieves the text of the message box from the current browser tab. 
        /// If an error occurs, it logs the error and returns an empty string.

        /// </summary>
        /// <returns></returns>
        private string HandleGetMessageBoxTextOperation()
        {
            try
            {
                return ((PlaywrightBrowserTab)_browser!.CurrentWindow.CurrentTab).GetMessageBoxText();
            }
            catch (Exception ex)
            {
                _act.Error = $"Error when {MethodBase.GetCurrentMethod().Name}.";
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return string.Empty;
            }
        }
        /// <summary>
        /// This asynchronous method sets the text of the message box in the current browser tab. 
        /// If an error occurs, it logs the error.
        /// </summary>
        /// <param name="MessageBoxText"></param>
        /// <returns></returns>
        private async Task HandleSetMessageBoxTextOperationAsync(string MessageBoxText)
        {
            try
            {
                await ((PlaywrightBrowserTab)_browser!.CurrentWindow.CurrentTab).SetMessageBoxText(MessageBoxText);
            }
            catch (Exception ex)
            {
                _act.Error = $"Error when {MethodBase.GetCurrentMethod().Name}.";
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }
        }
        /// <summary>
        /// This asynchronous method starts monitoring and capturing network logs in the current browser tab. If an error occurs, 
        /// it logs the error and updates the Error property.
        /// </summary>
        /// <returns></returns>
        private async Task HandleStartMonitoringNetworkLogOperationAsync()
        {
            try
            {
               await ((PlaywrightBrowserTab)_browser!.CurrentWindow.CurrentTab).StartCaptureNetworkLog(_act);
            }
            catch (Exception ex)
            {
                _act.Error = $"Error when {MethodBase.GetCurrentMethod().Name}.";
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return;
            }
        }
        /// <summary>
        /// This asynchronous method retrieves the captured network logs from the current browser tab. 
        /// If an error occurs, it logs the error and updates the Error property.
        /// </summary>
        /// <returns></returns>
        private async Task HandleGetNetworkLogOperationAsync()
        {
            try
            {
                await ((PlaywrightBrowserTab)_browser!.CurrentWindow.CurrentTab).GetCaptureNetworkLog(_act);
            }
            catch (Exception ex)
            {
                _act.Error = $"Error when {MethodBase.GetCurrentMethod().Name}.";
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return;
            }
        }
        /// <summary>
        /// This asynchronous method stops monitoring and capturing network logs in the current browser tab. 
        /// If an error occurs, it logs the error and updates the Error property.
        /// </summary>
        /// <returns></returns>
        private async Task HandleStopMonitoringNetworkLogOperationAsync()
        {
            try
            {
                await ((PlaywrightBrowserTab)_browser!.CurrentWindow.CurrentTab).StopCaptureNetworkLog(_act);
            }
            catch (Exception ex)
            {
                _act.Error = $"Error when {MethodBase.GetCurrentMethod().Name}.";
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return;
            }
        }
    }
}
