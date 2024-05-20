using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Exceptions;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Environments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright.ActionHandlers
{
    internal sealed class ActBrowserElementHandler
    {
        internal readonly struct Context
        {
            internal required ProjEnvironment Environment { get; init; }
        
            internal required BusinessFlow BusinessFlow { get; init; }
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
                    case ActBrowserElement.eControlAction.Maximize:
                        _act.Error = "This operation is not supported via current driver.";
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
                    case ActBrowserElement.eControlAction.GetBrowserLog:
                        operationTask = HandleGetBrowserLogOperationAsync();
                        break;
                    //case ActBrowserElement.eControlAction.GetMessageBoxText:
                    //    operationTask = HandleGetMessageBoxTextAsync();
                    //    break;
                    default:
                        _act.Error = $"Unknown operation type - {_act.ControlAction}";
                        break;
                }
            }
            catch (InvalidActionConfigurationException ex)
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
                await _browser.NewWindowAsync();
                await _browser.CurrentWindow.NewTabAsync();
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
            if (string.IsNullOrEmpty(_act.ValueForDriver) && string.IsNullOrEmpty(_act.LocateValueCalculated))
            {
                throw new InvalidActionConfigurationException("Error: The window title to search for is missing.");
            }

            string excludedWindowTitle;
            if (!string.IsNullOrEmpty(_act.LocateValueCalculated))
            {
                excludedWindowTitle = _act.LocateValueCalculated;
            }
            else
            {
                excludedWindowTitle = _act.ValueForDriver;
            }

            List<IBrowserTab> tabsToClose = [];
            foreach (IBrowserWindow window in _browser.Windows)
            {
                foreach (IBrowserTab tab in window.Tabs)
                {
                    string tabTitle = await tab.GetTitleAsync();
                    if (!string.IsNullOrEmpty(tabTitle) && tabTitle.Contains(excludedWindowTitle, StringComparison.OrdinalIgnoreCase))
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

        private Task HandleGetBrowserLogOperationAsync()
        {
        }

        private async Task HandleGetMessageBoxTextAsync()
        {
            IBrowserDialog? dialog = _browser.CurrentWindow.CurrentTab.UnhandledDialogs.FirstOrDefault();
            string message = string.Empty;
            if (dialog != null)
            {
                message = await dialog.GetMessageAsync();
            }
            _act.AddOrUpdateReturnParamActual("Actual", message);
        }
    }
}
