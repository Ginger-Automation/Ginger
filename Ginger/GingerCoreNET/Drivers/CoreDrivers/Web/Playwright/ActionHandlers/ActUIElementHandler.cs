using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Exceptions;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
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
                    case ActUIElement.eElementAction.GetHeight:
                        operationTask = HandleGetHeightOperationAsync();
                        break;
                    case ActUIElement.eElementAction.GetWidth:
                        operationTask = HandleGetWidthOperationAsync();
                        break;
                    case ActUIElement.eElementAction.GetSize:
                        operationTask = HandleGetSizeOperationAsync();
                        break;
                    case ActUIElement.eElementAction.GetStyle:
                        operationTask = HandleGetStyleOperationAsync();
                        break;
                    case ActUIElement.eElementAction.GetValue:
                        operationTask = HandleGetValueOperationAsync();
                        break;
                    case ActUIElement.eElementAction.GetItemCount:
                        operationTask = HandleGetItemCountOperationAsync();
                        break;
                    case ActUIElement.eElementAction.ScrollToElement:
                        operationTask = HandleScrollToElementOperationAsync();
                        break;
                    case ActUIElement.eElementAction.SetFocus:
                        operationTask = HandleSetFocusOperationAsync();
                        break;
                    case ActUIElement.eElementAction.IsDisabled:
                        operationTask = HandleIsDisabledOperationAsync();
                        break;
                    case ActUIElement.eElementAction.Submit:
                        operationTask = HandleSubmitOperationAsync();
                        break;
                    case ActUIElement.eElementAction.MultiClicks:
                        operationTask = HandleMultiClicksOperationAsync();
                        break;
                    case ActUIElement.eElementAction.ClickXY:
                        operationTask = HandleClickXYOperationAsync();
                        break;
                    case ActUIElement.eElementAction.DoubleClickXY:
                        operationTask = HandleDoubleClickXYOperationAsync();
                        break;
                    default:
                        _act.Error = $"Unknown operation type - {_act.ElementAction}";
                        break;
                }
            }
            catch (Exception ex)
            {
                _act.Error = ex.Message;
            }
            return operationTask;
        }

        private async Task<IBrowserElement> GetFirstMatchingElementAsync()
        {
            IEnumerable<IBrowserElement> elements = await GetAllMatchingElementsAsync();

            IBrowserElement? firstElement = elements.FirstOrDefault();
            if (firstElement == null)
            {
                throw new NotFoundException($"No element found by locator '{_act.ElementLocateBy}' and value '{_act.ElementLocateValueForDriver}'");
            }

            return firstElement;
        }
        private Task<IEnumerable<IBrowserElement>> GetAllMatchingElementsAsync()
        {
            return _browser
                .CurrentWindow
                .CurrentTab
                .GetElementsAsync(_act.ElementLocateBy, _act.ElementLocateValueForDriver);
        }

        private async Task HandleClickOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            await element.ClickAsync();
        }

        private async Task HandleDoubleClickOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            await element.DoubleClickAsync();
        }

        private async Task HandleHoverOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            await element.HoverAsync();
        }

        private async Task HandleIsVisibleOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            bool isVisible = await element.IsVisibleAsync();
            _act.AddOrUpdateReturnParamActual("Actual", isVisible.ToString());
        }

        private async Task HandleIsEnabledOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            bool isEnabled = await element.IsEnabledAsync();
            _act.AddOrUpdateReturnParamActual("Actual", isEnabled.ToString());
        }

        private async Task HandleGetAttributeOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            string attributeValue = await element.AttributeValueAsync(_act.ValueForDriver);
            _act.AddOrUpdateReturnParamActual("Actual", attributeValue);
        }

        private async Task HandleGetTextOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            string text = await element.TextContentAsync();
            if (string.IsNullOrEmpty(text))
            {
                text = await element.InnerTextAsync();
            }
            if (string.IsNullOrEmpty(text))
            {
                text = await element.InputValueAsync();
            }
            if (text == null)
            {
                text = string.Empty;
            }
            _act.AddOrUpdateReturnParamActual("Actual", text);
        }

        private async Task HandleRightClickOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            await element.RightClickAsync();
        }

        private async Task HandleIsValuePopulatedOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();

            string tagName = await element.TagNameAsync();
            if (string.Equals(tagName, IBrowserElement.SelectTagName, StringComparison.OrdinalIgnoreCase))
            {
                string script = "element => element.options[element.selectedIndex].text";
                string value = await element.ExecuteJavascriptAsync(script);
                _act.AddOrUpdateReturnParamActual("Actual", value);
            }
            else
            {
                string value = await element.InputValueAsync();
                _act.AddOrUpdateReturnParamActual("Actual", value);
            }
        }

        private async Task HandleGetHeightOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            Size size = await element.SizeAsync();
            _act.AddOrUpdateReturnParamActual("Actual", size.Height.ToString());
        }

        private async Task HandleGetWidthOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            Size size = await element.SizeAsync();
            _act.AddOrUpdateReturnParamActual("Actual", size.Width.ToString());
        }

        private  async Task HandleGetSizeOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            Size size = await element.SizeAsync();
            _act.AddOrUpdateReturnParamActual("Actual", $"{size.Width}x{size.Height}");
        }

        private async Task HandleGetValueOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            string tagName = await element.TagNameAsync();
            if (string.Equals(tagName, IBrowserElement.AnchorTagName))
            {
                string href = await element.AttributeValueAsync(attributeName: "href");
                _act.AddOrUpdateReturnParamActual("Actual", href);
                return;
            }
            string value = await element.TextContentAsync();
            if (!string.IsNullOrEmpty(value))
            {
                _act.AddOrUpdateReturnParamActual("Actual", value);
                return;
            }
            value = await element.InputValueAsync();
            _act.AddOrUpdateReturnParamActual("Actual", value);
        }

        private async Task HandleGetStyleOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            string style = await element.AttributeValueAsync(attributeName: "style");
            _act.AddOrUpdateReturnParamActual("Actual", style);
        }

        private async Task HandleGetItemCountOperationAsync()
        {
            IEnumerable<IBrowserElement> elements = await GetAllMatchingElementsAsync();
            _act.AddOrUpdateReturnParamActual("Elements Count", elements.Count().ToString());
        }

        private async Task HandleScrollToElementOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            await element.ScrollToViewAsync();
        }

        private async Task HandleSetFocusOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            await element.FocusAsync();
        }

        private async Task HandleIsDisabledOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            bool isEnabled = await element.IsEnabledAsync();
            _act.AddOrUpdateReturnParamActual("Actual", (!isEnabled).ToString());
        }

        private async Task HandleSubmitOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            string tagName = await element.TagNameAsync();
            bool isInputElement = string.Equals(tagName, IBrowserElement.InputTagName, StringComparison.OrdinalIgnoreCase);
            bool isButtonElement = string.Equals(tagName, IBrowserElement.ButtonTagName, StringComparison.OrdinalIgnoreCase);
            bool isTypeSubmit = string.Equals(await element.AttributeValueAsync(attributeName: "type"), "submit", StringComparison.OrdinalIgnoreCase);
            
            if ((isInputElement || isButtonElement) && isTypeSubmit)
            {
                await element.ClickAsync();
            }
            else
            {
                throw new InvalidOperationException($"Expected '{IBrowserElement.InputTagName}/{IBrowserElement.ButtonTagName}' element with type 'submit'");
            }
        }

        private async Task HandleMultiClicksOperationAsync()
        {
            IEnumerable<IBrowserElement> elements = new List<IBrowserElement>(await GetAllMatchingElementsAsync());
            
            foreach (IBrowserElement element in elements)
            {
                await element.ClickAsync();
            }
        }

        private async Task HandleClickXYOperationAsync()
        {
            string xString = _act.GetOrCreateInputParam(ActUIElement.Fields.XCoordinate).ValueForDriver;
            string yString = _act.GetOrCreateInputParam(ActUIElement.Fields.YCoordinate).ValueForDriver;
            if (!int.TryParse(xString, out int x))
            {
                throw new InvalidActionConfigurationException($"X-Coordinate must be a valid integer");
            }
            if (!int.TryParse(yString, out int y))
            {
                throw new InvalidActionConfigurationException($"X-Coordinate must be a valid integer");
            }

            IBrowserElement element = await GetFirstMatchingElementAsync();
            await element.ClickAsync(x, y);
        }

        private async Task HandleDoubleClickXYOperationAsync()
        {
            string xString = _act.GetOrCreateInputParam(ActUIElement.Fields.XCoordinate).ValueForDriver;
            string yString = _act.GetOrCreateInputParam(ActUIElement.Fields.YCoordinate).ValueForDriver;
            if (!int.TryParse(xString, out int x))
            {
                throw new InvalidActionConfigurationException($"X-Coordinate must be a valid integer");
            }
            if (!int.TryParse(yString, out int y))
            {
                throw new InvalidActionConfigurationException($"X-Coordinate must be a valid integer");
            }

            IBrowserElement element = await GetFirstMatchingElementAsync();
            await element.DoubleClickAsync(x, y);
        }
    }
}
