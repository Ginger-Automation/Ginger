using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Exceptions;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Environments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright.ActionHandlers
{
    internal sealed class ActBrowserElementHandler
    {
        private static readonly IEnumerable<ActBrowserElement.eControlAction> SupportedOperations = new List<ActBrowserElement.eControlAction>()
        {
            ActBrowserElement.eControlAction.GotoURL,
            ActBrowserElement.eControlAction.OpenURLNewTab,
            ActBrowserElement.eControlAction.GetPageURL,
            ActBrowserElement.eControlAction.GetWindowTitle,
            ActBrowserElement.eControlAction.NavigateBack,
            ActBrowserElement.eControlAction.Refresh,
            ActBrowserElement.eControlAction.DeleteAllCookies,
            ActBrowserElement.eControlAction.RunJavaScript,
            ActBrowserElement.eControlAction.GetPageSource,
            ActBrowserElement.eControlAction.Close,
            ActBrowserElement.eControlAction.CloseTabExcept,
            ActBrowserElement.eControlAction.CloseAll,
            ActBrowserElement.eControlAction.CheckPageLoaded,
            ActBrowserElement.eControlAction.GetConsoleLog,
            ActBrowserElement.eControlAction.GetBrowserLog,
            ActBrowserElement.eControlAction.SwitchFrame,
            ActBrowserElement.eControlAction.SwitchToDefaultFrame,
            ActBrowserElement.eControlAction.SwitchToParentFrame,
            ActBrowserElement.eControlAction.SwitchWindow,
            ActBrowserElement.eControlAction.SwitchToDefaultWindow,
        };

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

        public static bool IsOperationSupported(ActBrowserElement.eControlAction operation)
        {
            return SupportedOperations.Contains(operation);
        }

        internal Task HandleAsync()
        {
            Task operationTask = Task.CompletedTask;
            try
            {
                switch (_act.ControlAction)
                {
                    case ActBrowserElement.eControlAction.GotoURL:
                        operationTask = HandleGotoUrlOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.OpenURLNewTab:
                        operationTask = HandleOpenUrlInNewTabOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.GetPageURL:
                        operationTask = HandleGetPageUrlOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.GetWindowTitle:
                        operationTask = HandleGetWindowTitleOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.NavigateBack:
                        operationTask = HandleNavigateBackOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.Refresh:
                        operationTask = HandleRefreshOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.DeleteAllCookies:
                        operationTask = HandleDeleteAllCookiesOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.RunJavaScript:
                        operationTask = HandleRunJavascriptionOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.GetPageSource:
                        operationTask = HandleGetPageSourceOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.Close:
                        operationTask = HandleCloseOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.CloseTabExcept:
                        operationTask = HandleCloseTabExceptOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.CloseAll:
                        operationTask = HandleCloseAllOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.CheckPageLoaded:
                        operationTask = HandleCheckPageLoadedOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.GetConsoleLog:
                        operationTask = HandleGetConsoleLogOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.GetBrowserLog:
                        operationTask = HandleGetBrowserLogOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.SwitchFrame:
                        operationTask = HandleSwitchFrameOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.SwitchToDefaultFrame:
                        operationTask = HandleSwitchToDefaultFrameOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.SwitchToParentFrame:
                        operationTask = HandleSwitchToParentFrameOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.SwitchWindow:
                        operationTask = HandleSwitchWindowOperationAsync();
                        break;
                    case ActBrowserElement.eControlAction.SwitchToDefaultWindow:
                        operationTask = HandleSwitchToDefaultWindowOperationAsync();
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

            return operationTask;
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
            string url = await _browser.CurrentWindow.CurrentTab.GetURLAsync();

            _act.AddOrUpdateReturnParamActual("PageURL", url);

            Uri? uri = null;
            try
            {
                uri = new(url);
            }
            catch(Exception ex) when (ex is ArgumentNullException || ex is UriFormatException) { }

            if (uri != null)
            {
                _act.AddOrUpdateReturnParamActual("Host", uri.Host);
                _act.AddOrUpdateReturnParamActual("Path", uri.LocalPath);
                _act.AddOrUpdateReturnParamActual("PathWithQuery", uri.PathAndQuery);
            }
        }

        private async Task HandleGetWindowTitleOperationAsync()
        {
            string title = await _browser.CurrentWindow.CurrentTab.GetTitleAsync();
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
            string content = await _browser.CurrentWindow.CurrentTab.GetPageSourceAsync();
            if (!string.IsNullOrEmpty(content))
            {
                if (content.StartsWith("<!DOCTYPE html>"))
                {
                    content = content.Substring("<!DOCTYPE html>".Length);
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
            if (_act.LocateBy != eLocateBy.ByTitle && _act.LocateBy != eLocateBy.ByUrl)
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
                        tabValue = await tab.GetTitleAsync();
                        
                    }
                    else if (_act.LocateBy == eLocateBy.ByUrl)
                    {
                        tabValue = await tab.GetURLAsync();
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
            string logs = await _browser.CurrentWindow.CurrentTab.GetConsoleLogsAsync();
            _act.AddOrUpdateReturnParamActual("Console logs", logs);
        }

        private async Task HandleGetBrowserLogOperationAsync()
        {
            string logs = await _browser.CurrentWindow.CurrentTab.GetBrowserLogsAsync();
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
            foreach(JsonNode? item in jsonArray)
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

                urlString = urlArray[urlArray.Length - 1];
                if (string.IsNullOrEmpty(urlString) && urlArray.Length > 1)
                {
                    urlString = urlArray[urlArray.Length - 2];
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
                    string tabTitle = await tab.GetTitleAsync();
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
    }
}
