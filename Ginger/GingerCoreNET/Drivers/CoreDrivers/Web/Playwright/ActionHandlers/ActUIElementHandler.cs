using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Exceptions;
using Applitools.Utils;
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
        private static readonly IEnumerable<ActUIElement.eElementAction> SupportedOperations = new List<ActUIElement.eElementAction>()
        {
            ActUIElement.eElementAction.Click,
            ActUIElement.eElementAction.DoubleClick,
            ActUIElement.eElementAction.Hover,
            ActUIElement.eElementAction.IsVisible,
            ActUIElement.eElementAction.IsEnabled,
            ActUIElement.eElementAction.GetAttrValue,
            ActUIElement.eElementAction.GetText,
            ActUIElement.eElementAction.MouseRightClick,
            ActUIElement.eElementAction.IsValuePopulated,
            ActUIElement.eElementAction.GetHeight,
            ActUIElement.eElementAction.GetWidth,
            ActUIElement.eElementAction.GetSize,
            ActUIElement.eElementAction.GetStyle,
            ActUIElement.eElementAction.GetValue,
            ActUIElement.eElementAction.GetItemCount,
            ActUIElement.eElementAction.ScrollToElement,
            ActUIElement.eElementAction.SetFocus,
            ActUIElement.eElementAction.IsDisabled,
            ActUIElement.eElementAction.Submit,
            ActUIElement.eElementAction.MultiClicks,
            ActUIElement.eElementAction.ClickXY,
            ActUIElement.eElementAction.DoubleClickXY,
            ActUIElement.eElementAction.ClearValue,
            ActUIElement.eElementAction.Select,
            ActUIElement.eElementAction.SelectByText,
            ActUIElement.eElementAction.SelectByIndex,
            ActUIElement.eElementAction.SetValue,
        };

        private static readonly IEnumerable<string> SupportedInputTypesForIsValuePopulated = new List<string>()
        {
            "date",
            "datetime-local",
            "email",
            "month",
            "number",
            "password",
            "search",
            "tel",
            "text",
            "time",
            "url",
            "week"
        };

        private readonly ActUIElement _act;
        private readonly IBrowser _browser;

        internal ActUIElementHandler(ActUIElement act, IBrowser browser)
        {
            _act = act;
            _browser = browser;
        }

        public static bool IsOperationSupported(ActUIElement.eElementAction operation)
        {
            return SupportedOperations.Contains(operation);
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
                    case ActUIElement.eElementAction.ClearValue:
                        operationTask = HandleClearValueOperationAsync();
                        break;
                    case ActUIElement.eElementAction.Select:
                        operationTask = HandleSelectOperationAsync();
                        break;
                    case ActUIElement.eElementAction.SelectByText:
                        operationTask = HandleSelectByTextOperationAsync();
                        break;
                    case ActUIElement.eElementAction.SelectByIndex:
                        operationTask = HandleSelectByIndexOperationAsync();
                        break;
                    case ActUIElement.eElementAction.SetValue:
                        operationTask = HandleSetValueOperationAsync();
                        break;
                    default:
                        string operationName = Common.GeneralLib.General.GetEnumValueDescription(typeof(ActUIElement.eElementAction), _act.ElementAction);
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
                text = await element.InputValueAsync(); //probably only supported for input, textarea, select elements.
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
            string value;
            if (string.Equals(tagName, IBrowserElement.SelectTagName, StringComparison.OrdinalIgnoreCase))
            {
                string script = "element => element.options[element.selectedIndex].text";
                value = await element.ExecuteJavascriptAsync(script);
            }
            else if (string.Equals(tagName, IBrowserElement.TextAreaTagName, StringComparison.OrdinalIgnoreCase))
            {
                value = await element.InputValueAsync();
            }
            else if (string.Equals(tagName, IBrowserElement.InputTagName, StringComparison.OrdinalIgnoreCase))
            {
                string typeAttrValue = await element.AttributeValueAsync(name: "type");
                if (!SupportedInputTypesForIsValuePopulated.Any(supportedInputType => string.Equals(supportedInputType, typeAttrValue, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException($"Operation '{nameof(ActUIElement.eElementAction.IsValuePopulated)}' is not supported for 'input' element with type '{typeAttrValue}'.");
                }

                value = await element.InputValueAsync();
            }
            else
            {
                throw new InvalidActionConfigurationException($"Operation '{nameof(ActUIElement.eElementAction.IsValuePopulated)}' is not supported for element type '{tagName}'.");
            }

            bool containsValue = !string.IsNullOrEmpty(value);

            _act.AddOrUpdateReturnParamActual("Actual", containsValue.ToString());
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
                string href = await element.AttributeValueAsync(name: "href");
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
            string style = await element.AttributeValueAsync(name: "style");
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
            bool isTypeSubmit = string.Equals(await element.AttributeValueAsync(name: "type"), "submit", StringComparison.OrdinalIgnoreCase);
            
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
                if (await element.IsVisibleAsync() && await element.IsEnabledAsync())
                {
                    await element.ClickAsync();
                }
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
                throw new InvalidActionConfigurationException($"Y-Coordinate must be a valid integer");
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
                throw new InvalidActionConfigurationException($"X-Coordinate must be a integer");
            }
            if (!int.TryParse(yString, out int y))
            {
                throw new InvalidActionConfigurationException($"X-Coordinate must be a integer");
            }

            IBrowserElement element = await GetFirstMatchingElementAsync();
            await element.DoubleClickAsync(x, y);
        }

        private async Task HandleClearValueOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            await element.ClearAsync();
        }

        private async Task HandleSelectOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            string value = _act.GetInputParamCalculatedValue(ActUIElement.Fields.ValueToSelect);
            await element.SelectByValueAsync(value);
        }

        private async Task HandleSelectByTextOperationAsync()
        {

            IBrowserElement element = await GetFirstMatchingElementAsync();
            string text = _act.GetInputParamCalculatedValue(ActUIElement.Fields.Value);
            await element.SelectByTextAsync(text);
        }

        private async Task HandleSelectByIndexOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            if (!int.TryParse(_act.GetInputParamCalculatedValue(ActUIElement.Fields.ValueToSelect), out int index))
            {
                throw new InvalidActionConfigurationException($"Index to select must be a integer");
            }
            await element.SelectByIndexAsync(index);
        }

        private async Task HandleSetValueOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            string tagName = await element.TagNameAsync();
            string type = await element.AttributeValueAsync(name: "type");

            bool isSelectElement = string.Equals(tagName, IBrowserElement.SelectTagName, StringComparison.OrdinalIgnoreCase);
            if (isSelectElement)
            {
                string text = _act.GetInputParamCalculatedValue(ActUIElement.Fields.Value);
                await element.SelectByTextAsync(text);
                return;
            }

            bool isInputElement = string.Equals(tagName, IBrowserElement.InputTagName, StringComparison.OrdinalIgnoreCase);
            bool isTypeCheckbox = string.Equals(type, "checkbox", StringComparison.OrdinalIgnoreCase);
            if (isInputElement && isTypeCheckbox)
            {
                string checkString = _act.ValueForDriver;
                if (!bool.TryParse(checkString, out bool check))
                {
                    throw new InvalidActionConfigurationException($"Expected value to be 'true/false' for checkbox");
                }
                await element.SetCheckboxAsync(check);
                return;
            }

            bool isTypeText = string.Equals(type, "text", StringComparison.OrdinalIgnoreCase);
            if (isInputElement && isTypeText)
            {
                await element.ClearAsync();
                string text = _act.GetInputParamCalculatedValue("Value");
                await element.SetTextAsync(text);
                return;
            }

            string value = _act.GetInputParamCalculatedValue("Value");
            await element.SetAttributeValueAsync(name: "value", value);
        }
    }
}
