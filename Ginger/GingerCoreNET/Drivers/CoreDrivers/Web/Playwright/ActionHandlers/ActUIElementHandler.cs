using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Exceptions;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright.ActionHandlers
{
    internal sealed class ActUIElementHandler
    {
        private readonly ActUIElement _act;
        private readonly IBrowser _browser;

        internal ActUIElementHandler(ActUIElement act, IBrowser browser)
        {
            _act = act;
            _browser = browser;
        }

        internal Task HandleAsync()
        {
            Task operationTask = Task.CompletedTask;
            try
            {
                switch (_act.ElementAction)
                {
                    case ActUIElement.eElementAction.Click:
                        operationTask = HandleClickOperationAsync();
                        break;
                    case ActUIElement.eElementAction.DoubleClick:
                        operationTask = HandleDoubleClickOperationAsync();
                        break;
                    case ActUIElement.eElementAction.Hover:
                        operationTask = HandleHoverOperationAsync();
                        break;
                    case ActUIElement.eElementAction.IsVisible:
                        operationTask = HandleIsVisibleOperationAsync();
                        break;
                    case ActUIElement.eElementAction.IsEnabled:
                        operationTask = HandleIsEnabledOperationAsync();
                        break;
                    case ActUIElement.eElementAction.GetAttrValue:
                        operationTask = HandleGetAttributeOperationAsync();
                        break;
                    case ActUIElement.eElementAction.GetText:
                        operationTask = HandleGetTextOperationAsync();
                        break;
                    case ActUIElement.eElementAction.MouseRightClick:
                        operationTask = HandleRightClickOperationAsync();
                        break;
                    case ActUIElement.eElementAction.IsValuePopulated:
                        operationTask = HandleIsValuePopulatedOperationAsync();
                        break;
                    default:
                        _act.Error = $"Unknown operation type - {_act.ElementAction}";
                        break;
                }
            }
            catch (InvalidActionConfigurationException ex)
            {
                _act.Error = ex.Message;
            }
            return operationTask;
        }

        private async Task HandleClickOperationAsync()
        {
            try
            {
                await _browser.CurrentWindow.CurrentTab.ClickAsync(_act.ElementLocateBy, _act.ElementLocateValueForDriver);
            }
            catch (NotFoundException ex)
            {
                _act.Error = ex.Message;
            }
        }

        private async Task HandleDoubleClickOperationAsync()
        {
            try
            {
                await _browser.CurrentWindow.CurrentTab.DoubleClickAsync(_act.ElementLocateBy, _act.ElementLocateValueForDriver);
            }
            catch (NotFoundException ex)
            {
                _act.Error = ex.Message;
            }
        }

        private async Task HandleHoverOperationAsync()
        {
            try
            {
                await _browser.CurrentWindow.CurrentTab.HoverAsync(_act.ElementLocateBy, _act.ElementLocateValueForDriver);
            }
            catch (NotFoundException ex)
            {
                _act.Error = ex.Message;
            }
        }

        private async Task HandleIsVisibleOperationAsync()
        {
            bool isVisible = await _browser.CurrentWindow.CurrentTab.IsVisibleAsync(_act.ElementLocateBy, _act.ElementLocateValueForDriver);
            _act.AddOrUpdateReturnParamActual("Actual", isVisible.ToString());
        }

        private async Task HandleIsEnabledOperationAsync()
        {
            bool isEnabled = await _browser.CurrentWindow.CurrentTab.IsEnabledAsync(_act.ElementLocateBy, _act.ElementLocateValueForDriver);
            _act.AddOrUpdateReturnParamActual("Actual", isEnabled.ToString());
        }

        private async Task HandleGetAttributeOperationAsync()
        {
            try
            {
                string? attributeValue = await _browser.CurrentWindow.CurrentTab.GetAttributeValueAsync(_act.ElementLocateBy, _act.ElementLocateValueForDriver, _act.ValueForDriver);
                _act.AddOrUpdateReturnParamActual("Actual", attributeValue);
            }
            catch (NotFoundException ex)
            {
                _act.Error = ex.Message;
            }

        }

        private async Task HandleGetTextOperationAsync()
        {
            try
            {
                string? text = await _browser.CurrentWindow.CurrentTab.GetTextContentAsync(_act.ElementLocateBy, _act.ElementLocateValueForDriver);
                if (string.IsNullOrEmpty(text))
                {
                    text = await _browser.CurrentWindow.CurrentTab.GetInnerTextAsync(_act.ElementLocateBy, _act.ElementLocateValueForDriver);
                }
                if (string.IsNullOrEmpty(text))
                {
                    text = await _browser.CurrentWindow.CurrentTab.GetInputValueAsync(_act.ElementLocateBy, _act.ElementLocateValueForDriver);
                }
                if (text == null)
                {
                    text = string.Empty;
                }
                _act.AddOrUpdateReturnParamActual("Actual", text);
            }
            catch (NotFoundException ex)
            {
                _act.Error = ex.Message;
            }
        }

        private async Task HandleRightClickOperationAsync()
        {
            try
            {
                await _browser.CurrentWindow.CurrentTab.RightClickAsync(_act.ElementLocateBy, _act.ElementLocateValueForDriver);
            }
            catch (NotFoundException ex)
            {
                _act.Error = ex.Message;
            }

        }

        private async Task HandleIsValuePopulatedOperationAsync()
        {
            try
            {
                await _browser.CurrentWindow.CurrentTab.
            }
            catch (NotFoundException ex)
            {
                _act.Error = ex.Message;
            }
        }
    }
}
