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

using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Exceptions;
using GingerCore.Actions.Common;
using GingerCore.Platforms.PlatformsInfo;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;


#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.ActionHandlers
{
    internal sealed class ActUIElementHandler
    {
        private static readonly IEnumerable<string> SupportedInputTypesForIsValuePopulated =
        [
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
        ];

        private readonly ActUIElement _act;
        private readonly IBrowserTab _browserTab;
        private readonly IBrowserElementLocator _elementLocator;

        internal ActUIElementHandler(ActUIElement act, IBrowserTab browserTab, IBrowserElementLocator elementLocator)
        {
            _act = act;
            _browserTab = browserTab;
            _elementLocator = elementLocator;
        }

        internal async Task HandleAsync()
        {
            try
            {
                switch (_act.ElementAction)
                {
                    case ActUIElement.eElementAction.Click:
                        await HandleClickOperationAsync();
                        break;
                    case ActUIElement.eElementAction.DoubleClick:
                        await HandleDoubleClickOperationAsync();
                        break;
                    case ActUIElement.eElementAction.Hover:
                        await HandleHoverOperationAsync();
                        break;
                    case ActUIElement.eElementAction.IsVisible:
                        await HandleIsVisibleOperationAsync();
                        break;
                    case ActUIElement.eElementAction.IsEnabled:
                        await HandleIsEnabledOperationAsync();
                        break;
                    case ActUIElement.eElementAction.GetAttrValue:
                        await HandleGetAttributeOperationAsync();
                        break;
                    case ActUIElement.eElementAction.GetText:
                        await HandleGetTextOperationAsync();
                        break;
                    case ActUIElement.eElementAction.MouseRightClick:
                        await HandleRightClickOperationAsync();
                        break;
                    case ActUIElement.eElementAction.IsValuePopulated:
                        await HandleIsValuePopulatedOperationAsync();
                        break;
                    case ActUIElement.eElementAction.GetHeight:
                        await HandleGetHeightOperationAsync();
                        break;
                    case ActUIElement.eElementAction.GetWidth:
                        await HandleGetWidthOperationAsync();
                        break;
                    case ActUIElement.eElementAction.GetSize:
                        await HandleGetSizeOperationAsync();
                        break;
                    case ActUIElement.eElementAction.GetStyle:
                        await HandleGetStyleOperationAsync();
                        break;
                    case ActUIElement.eElementAction.GetValue:
                        await HandleGetValueOperationAsync();
                        break;
                    case ActUIElement.eElementAction.GetItemCount:
                        await HandleGetItemCountOperationAsync();
                        break;
                    case ActUIElement.eElementAction.ScrollToElement:
                        await HandleScrollToElementOperationAsync();
                        break;
                    case ActUIElement.eElementAction.SetFocus:
                        await HandleSetFocusOperationAsync();
                        break;
                    case ActUIElement.eElementAction.IsDisabled:
                        await HandleIsDisabledOperationAsync();
                        break;
                    case ActUIElement.eElementAction.Submit:
                        await HandleSubmitOperationAsync();
                        break;
                    case ActUIElement.eElementAction.MultiClicks:
                        await HandleMultiClicksOperationAsync();
                        break;
                    case ActUIElement.eElementAction.ClickXY:
                        await HandleClickXYOperationAsync();
                        break;
                    case ActUIElement.eElementAction.DoubleClickXY:
                        await HandleDoubleClickXYOperationAsync();
                        break;
                    case ActUIElement.eElementAction.ClearValue:
                        await HandleClearValueOperationAsync();
                        break;
                    case ActUIElement.eElementAction.Select:
                        await HandleSelectOperationAsync();
                        break;
                    case ActUIElement.eElementAction.SelectByText:
                        await HandleSelectByTextOperationAsync();
                        break;
                    case ActUIElement.eElementAction.SelectByIndex:
                        await HandleSelectByIndexOperationAsync();
                        break;
                    case ActUIElement.eElementAction.SetValue:
                        await HandleSetValueOperationAsync();
                        break;
                    case ActUIElement.eElementAction.ClickAndValidate:
                        await HandleClickAndValidateAsync();
                        break;
                    case ActUIElement.eElementAction.JavaScriptClick:
                        await HandleJavaScriptClickAsync();
                        break;
                    case ActUIElement.eElementAction.SetText:
                        await HandleSetTextAsync();
                        break;
                    case ActUIElement.eElementAction.SendKeys:
                        await HandleSendKeysAsync();
                        break;
                    case ActUIElement.eElementAction.SendKeysXY:
                        await HandleSendKeysXYAsync();
                        break;
                    case ActUIElement.eElementAction.RunJavaScript:
                        await HandleRunJavaScriptAsync();
                        break;
                    case ActUIElement.eElementAction.AsyncClick:
                        await HandleAsyncClickAsync();
                        break;
                    case ActUIElement.eElementAction.GetCustomAttribute:
                        await HandleGetCustomAttributeAsync();
                        break;
                    case ActUIElement.eElementAction.GetFont:
                        await HandleGetFontAsync();
                        break;
                    case ActUIElement.eElementAction.MousePressRelease:
                        await HandleMousePressReleaseAsync();
                        break;
                    case ActUIElement.eElementAction.MouseClick:
                        await HandleMouseClickAsync();
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
        }

        /// <summary>
        /// Retrieves the first matching browser element asynchronously.
        /// </summary>
        /// <returns>The first matching browser element.</returns>
        private async Task<IBrowserElement> GetFirstMatchingElementAsync()
        {
            IEnumerable<IBrowserElement> elements = await _elementLocator.FindMatchingElements(_act.ElementLocateBy, _act.ElementLocateValueForDriver);

            IBrowserElement? firstElement = elements.FirstOrDefault();
            if (firstElement == null)
            {
                throw new EntityNotFoundException($"No element found by locator '{_act.ElementLocateBy}' and value '{_act.ElementLocateValueForDriver}'");
            }

            return firstElement;
        }

        /// <summary>
        /// Retrieves all the matching browser elements asynchronously.
        /// </summary>
        /// <returns>All the matching browser elements.</returns>
        private async Task<IEnumerable<IBrowserElement>> GetAllMatchingElementsAsync()
        {
            return await _elementLocator.FindMatchingElements(_act.ElementLocateBy, _act.ElementLocateValueForDriver);
        }

        /// <summary>
        /// Handles the click operation asynchronously.
        /// </summary>
        private async Task HandleClickOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            await element.ClickAsync();
        }

        /// <summary>
        /// Handles the double click operation asynchronously.
        /// </summary>
        private async Task HandleDoubleClickOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            await element.DoubleClickAsync();
        }

        /// <summary>
        /// Handles the hover operation asynchronously.
        /// </summary>
        private async Task HandleHoverOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            await element.HoverAsync();
        }

        /// <summary>
        /// Handles the is visible operation asynchronously.
        /// </summary>
        private async Task HandleIsVisibleOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            bool isVisible = await element.IsVisibleAsync();
            _act.AddOrUpdateReturnParamActual("Actual", isVisible.ToString());
        }

        /// <summary>
        /// Handles the IsEnabled operation for the UI element.
        /// </summary>
        private async Task HandleIsEnabledOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            bool isEnabled = await element.IsEnabledAsync();
            _act.AddOrUpdateReturnParamActual("Actual", isEnabled.ToString());
        }

        /// <summary>
        /// Handles the GetAttribute operation for the UI element.
        /// </summary>
        private async Task HandleGetAttributeOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            string attributeValue = await element.AttributeValueAsync(_act.ValueForDriver);
            _act.AddOrUpdateReturnParamActual("Actual", attributeValue);
        }

        /// <summary>
        /// Handles the GetText operation for the UI element.
        /// </summary>
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

        /// <summary>
        /// Handles the RightClick operation for the UI element.
        /// </summary>
        private async Task HandleRightClickOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            await element.RightClickAsync();
        }

        /// <summary>
        /// Handles the IsValuePopulated operation for the UI element.
        /// </summary>
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

        /// <summary>
        /// Handles the asynchronous operation for getting the height of the UI element.
        /// </summary>
        private async Task HandleGetHeightOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            Size size = await element.SizeAsync();
            _act.AddOrUpdateReturnParamActual("Actual", size.Height.ToString());
        }

        /// <summary>
        /// Handles the asynchronous operation for getting the width of the UI element.
        /// </summary>
        private async Task HandleGetWidthOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            Size size = await element.SizeAsync();
            _act.AddOrUpdateReturnParamActual("Actual", size.Width.ToString());
        }

        /// <summary>
        /// Handles the asynchronous operation for getting the size (width and height) of the UI element.
        /// </summary>
        private async Task HandleGetSizeOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            Size size = await element.SizeAsync();
            _act.AddOrUpdateReturnParamActual("Actual", $"{size.Width}x{size.Height}");
        }

        /// <summary>
        /// Handles the asynchronous operation for getting the value of the UI element.
        /// </summary>
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

        /// <summary>
        /// Handles the asynchronous operation for getting the style attribute of the UI element.
        /// </summary>
        private async Task HandleGetStyleOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            string style = await element.AttributeValueAsync(name: "style");
            _act.AddOrUpdateReturnParamActual("Actual", style);
        }

        /// <summary>
        /// Handles the operation to get the item count of the matching elements.
        /// </summary>
        private async Task HandleGetItemCountOperationAsync()
        {
            IEnumerable<IBrowserElement> elements = await GetAllMatchingElementsAsync();
            _act.AddOrUpdateReturnParamActual("Elements Count", elements.Count().ToString());
        }

        /// <summary>
        /// Handles the operation to scroll to the first matching element.
        /// </summary>
        private async Task HandleScrollToElementOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            await element.ScrollToViewAsync();
        }

        /// <summary>
        /// Handles the operation to set focus on the first matching element.
        /// </summary>
        private async Task HandleSetFocusOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            await element.FocusAsync();
        }

        /// <summary>
        /// Handles the operation to check if the first matching element is disabled.
        /// </summary>
        private async Task HandleIsDisabledOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            bool isEnabled = await element.IsEnabledAsync();
            _act.AddOrUpdateReturnParamActual("Actual", (!isEnabled).ToString());
        }

        /// <summary>
        /// Handles the operation to submit the first matching element.
        /// </summary>
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

        /// <summary>
        /// Handles the operation to perform multiple clicks on all matching elements.
        /// </summary>
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

        /// <summary>
        /// Handles the ClickXY operation asynchronously.
        /// </summary>
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
            await element.ClickAsync(new Point(x, y));
        }

        /// <summary>
        /// Handles the DoubleClickXY operation asynchronously.
        /// </summary>
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

        /// <summary>
        /// Handles the ClearValue operation asynchronously.
        /// </summary>
        private async Task HandleClearValueOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            await element.ClearAsync();
        }

        /// <summary>
        /// Handles the Select operation asynchronously.
        /// </summary>
        private async Task HandleSelectOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            string value = _act.GetInputParamCalculatedValue(ActUIElement.Fields.ValueToSelect);
            await element.SelectByValueAsync(value);
        }

        /// <summary>
        /// Handles the SelectByText operation asynchronously.
        /// </summary>
        private async Task HandleSelectByTextOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            string text = _act.GetInputParamCalculatedValue(ActUIElement.Fields.Value);
            await element.SelectByTextAsync(text);
        }

        /// <summary>
        /// Handles the SelectByIndex operation asynchronously.
        /// </summary>
        private async Task HandleSelectByIndexOperationAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            if (!int.TryParse(_act.GetInputParamCalculatedValue(ActUIElement.Fields.ValueToSelect), out int index))
            {
                throw new InvalidActionConfigurationException($"Index to select must be a integer");
            }
            await element.SelectByIndexAsync(index);
        }

        /// <summary>
        /// Handles the SetValue operation asynchronously.
        /// </summary>
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

        /// <summary>
        /// Handles the ClickAndValidateAsync operation.
        /// </summary>
        private async Task HandleClickAndValidateAsync()
        {
            string clickTypeAsString = _act.GetInputParamValue(ActUIElement.Fields.ClickType).ToString() ?? "";
            if (!Enum.TryParse(clickTypeAsString, out ActUIElement.eElementAction clickType))
            {
                _act.Error = $"Unknown click type '{clickTypeAsString}'";
                return;
            }

            string validationElementLocateByAsString = _act.GetInputParamValue(ActUIElement.Fields.ValidationElementLocateBy).ToString() ?? "";
            if (!Enum.TryParse(validationElementLocateByAsString, out eLocateBy validationElementLocateBy))
            {
                _act.Error = $"Unknown locate by '{validationElementLocateByAsString}' for validation element";
                return;
            }
            string validationElementLocateValue = _act.GetInputParamValue(ActUIElement.Fields.ValidationElementLocatorValue).ToString() ?? "";

            string validationTypeAsString = _act.GetInputParamValue(ActUIElement.Fields.ValidationType).ToString() ?? "";
            if (!Enum.TryParse(validationTypeAsString, out ActUIElement.eElementAction validationType))
            {
                _act.Error = $"Unsupported validation type '{validationTypeAsString}'";
                return;
            }

            string loopThroughClicksAsString = _act.GetInputParamValue(ActUIElement.Fields.LoopThroughClicks);
            if (!bool.TryParse(loopThroughClicksAsString, out bool loopThroughClicks))
            {
                _act.Error = $"Loop Through '{loopThroughClicksAsString}' is not a valid boolean value";
                return;
            }

            IBrowserElement? validationElement = null;

            IEnumerable<ActUIElement.eElementAction> clicks = [clickType, ..new WebPlatform().GetPlatformUIClickTypeList()];
            foreach (ActUIElement.eElementAction click in clicks)
            {
                switch (click)
                {
                    case ActUIElement.eElementAction.Click:
                        await HandleClickOperationAsync();
                        break;
                    case ActUIElement.eElementAction.JavaScriptClick:
                        await HandleJavaScriptClickAsync();
                        break;
                    case ActUIElement.eElementAction.MouseClick:
                        await HandleMouseClickAsync();
                        break;
                    case ActUIElement.eElementAction.MousePressRelease:
                        await HandleMousePressReleaseAsync();
                        break;
                    case ActUIElement.eElementAction.AsyncClick:
                        await HandleAsyncClickAsync();
                        break;
                    default:
                        _act.Error = $"Click type '{click}' is not supported";
                        return;
                }

                validationElement = (await _elementLocator.FindMatchingElements(validationElementLocateBy, validationElementLocateValue)).FirstOrDefault();
                if (validationElement != null)
                {
                    bool validationResult = validationType switch
                    {
                        ActUIElement.eElementAction.IsEnabled => await validationElement.IsEnabledAsync(),
                        ActUIElement.eElementAction.IsVisible => await validationElement.IsVisibleAsync(),
                        _ => throw new NotImplementedException($"Validation type '{validationType}' is not implemented")
                    };

                    if (!validationResult)
                    {
                        _act.Error = $"Validation {validationType} failed";
                    }
                    break;
                }

                if(!loopThroughClicks)
                {
                    break;
                }
            }

            if (validationElement == null)
            {
                _act.Error = $"Validation element not found by locator '{validationElementLocateBy}' and value '{validationElementLocateValue}'";
                return;
            }            
        }

        /// <summary>
        /// Handles the asynchronous execution of a JavaScript click operation on a web element.
        /// </summary>
        private async Task HandleJavaScriptClickAsync()
        {
            IBrowserElement elementToClick = await GetFirstMatchingElementAsync();
            const string script = "element => element.click()";
            await elementToClick.ExecuteJavascriptAsync(script);
        }
        /// <summary>
        /// Handles the asynchronous execution of setting text on a web element.
        /// </summary>
        private async Task HandleSetTextAsync()
        {
            IBrowserElement elementToClick = await GetFirstMatchingElementAsync();
            string text = _act.GetInputParamCalculatedValue(ActUIElement.Fields.Value);
            await elementToClick.ClearAsync();
            await elementToClick.TypeTextAsync(text);
        }

        /// <summary>
        /// Handles the asynchronous execution of running JavaScript code on a web element.
        /// </summary>
        private async Task HandleRunJavaScriptAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            string script = _act.GetInputParamCalculatedValue(ActUIElement.Fields.Value);
            await element.ExecuteJavascriptAsync(script);
        }

        /// <summary>
        /// Handles the asynchronous execution of a delayed click operation on a web element.
        /// </summary>
        private async Task HandleAsyncClickAsync()
        {
            await _browserTab.StartListenDialogsAsync();
            const string script = "element => setTimeout(function() { element.click(); }, 100);";
            IBrowserElement element = await GetFirstMatchingElementAsync();
            await element.ExecuteJavascriptAsync(script);
        }

        /// <summary>
        /// Handles the asynchronous execution of getting a custom attribute value from a web element.
        /// </summary>
        private async Task HandleGetCustomAttributeAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            string attributeName = _act.GetInputParamCalculatedValue(ActUIElement.Fields.Value);
            string attributeValue = await element.AttributeValueAsync(attributeName);
            _act.AddOrUpdateReturnParamActual("Actual", attributeValue);
        }

        /// <summary>
        /// Handles the asynchronous execution of getting the font attribute value from a web element.
        /// </summary>
        private async Task HandleGetFontAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            string fontAttributeValue = await element.AttributeValueAsync("font");
            _act.AddOrUpdateReturnParamActual("Actual", fontAttributeValue);
        }


        /// <summary>
        /// Handles sending keys to a browser element. 
        /// If the element is a file input, it sets the file value. 
        /// For other input types, it clears the current value and performs an operation based on the provided keys.
        /// </summary>
        private async Task HandleSendKeysAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            string keys = _act.GetInputParamCalculatedValue("Value");
            string tagName = await element.TagNameAsync();
            string type = await element.AttributeValueAsync("type");
            if (string.Equals(tagName, IBrowserElement.InputTagName, StringComparison.OrdinalIgnoreCase) && string.Equals(type, "file", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrEmpty(keys))
                {
                    await element.SetFileValueAsync(keys.Split(',').Select(path => path.Replace("\"", "").Trim()).ToArray());
                }
            }
            else
            {
                await element.ClearAsync();
                Func<Task> elementOperation = ConvertSeleniumKeyIdentifierToElementOperation(keys, element);
                await elementOperation();
            }
        }
        /// <summary>
        /// Converts a Selenium key identifier to the corresponding element operation.
        /// Maps the key identifier to the appropriate key press action on the element.
        /// </summary>
        /// <param name="key">The Selenium key identifier.</param>
        /// <param name="element">The browser element to perform the action on.</param>
        /// <returns>A function representing the element operation.</returns>
        private Func<Task> ConvertSeleniumKeyIdentifierToElementOperation(string key, IBrowserElement element)
        {
            return key switch
            {
                "Keys.Alt" => () => element.PressKeysAsync(["Alt", "Enter", "F1", "F2"]),
                "Keys.ArrowDown" => () => element.PressKeysAsync(["ArrowDown"]),
                "Keys.ArrowLeft" => () => element.PressKeysAsync(["ArrowLeft"]),
                "Keys.ArrowRight" => () => element.PressKeysAsync(["ArrowRight"]),
                "Keys.ArrowUp" => () => element.PressKeysAsync(["ArrowUp"]),
                "Keys.Backspace" => () => element.PressKeysAsync(["Backspace"]),
                "Keys.Cancel" => () => element.PressKeysAsync(["Cancel"]),
                "Keys.Clear" => () => element.PressKeysAsync(["Clear"]),
                "Keys.Command" => () => element.PressKeysAsync(["Command"]),
                "Keys.Control" => () => element.PressKeysAsync(["Control"]),
                "Keys.Decimal" => () => element.PressKeysAsync(["Decimal"]),
                "Keys.Delete" => () => element.PressKeysAsync(["Delete"]),
                "Keys.Divide" => () => element.PressKeysAsync(["Divide"]),
                "Keys.Down" => () => element.PressKeysAsync(["Down"]),
                "Keys.End" => () => element.PressKeysAsync(["End"]),
                "Keys.Enter" => () => element.PressKeysAsync(["Enter"]),
                "Keys.Equal" => () => element.PressKeysAsync(["Equal"]),
                "Keys.Escape" => () => element.PressKeysAsync(["Escape"]),
                "Keys.F1" => () => element.PressKeysAsync(["F1"]),
                "Keys.F10" => () => element.PressKeysAsync(["F10"]),
                "Keys.F11" => () => element.PressKeysAsync(["F11"]),
                "Keys.F12" => () => element.PressKeysAsync(["F12"]),
                "Keys.F2" => () => element.PressKeysAsync(["F2"]),
                "Keys.F3" => () => element.PressKeysAsync(["F3"]),
                "Keys.F4" => () => element.PressKeysAsync(["F4"]),
                "Keys.F5" => () => element.PressKeysAsync(["F5"]),
                "Keys.F6" => () => element.PressKeysAsync(["F6"]),
                "Keys.F7" => () => element.PressKeysAsync(["F7"]),
                "Keys.F8" => () => element.PressKeysAsync(["F8"]),
                "Keys.F9" => () => element.PressKeysAsync(["F9"]),
                "Keys.Help" => () => element.PressKeysAsync(["Help"]),
                "Keys.Home" => () => element.PressKeysAsync(["Home"]),
                "Keys.Insert" => () => element.PressKeysAsync(["Insert"]),
                "Keys.Left" => () => element.PressKeysAsync(["Left"]),
                "Keys.LeftAlt" => () => element.PressKeysAsync(["LeftAlt"]),
                "Keys.LeftControl" => () => element.PressKeysAsync(["LeftControl"]),
                "Keys.LeftShift" => () => element.PressKeysAsync(["LeftShift"]),
                "Keys.Meta" => () => element.PressKeysAsync(["Meta"]),
                "Keys.Multiply" => () => element.PressKeysAsync(["Multiply"]),
                "Keys.Null" => () => element.PressKeysAsync(["Null"]),
                "Keys.NumberPad0" => () => element.PressKeysAsync(["NumberPad0"]),
                "Keys.NumberPad1" => () => element.PressKeysAsync(["NumberPad1"]),
                "Keys.NumberPad2" => () => element.PressKeysAsync(["NumberPad2"]),
                "Keys.NumberPad3" => () => element.PressKeysAsync(["NumberPad3"]),
                "Keys.NumberPad4" => () => element.PressKeysAsync(["NumberPad4"]),
                "Keys.NumberPad5" => () => element.PressKeysAsync(["NumberPad5"]),
                "Keys.NumberPad6" => () => element.PressKeysAsync(["NumberPad6"]),
                "Keys.NumberPad7" => () => element.PressKeysAsync(["NumberPad7"]),
                "Keys.NumberPad8" => () => element.PressKeysAsync(["NumberPad8"]),
                "Keys.NumberPad9" => () => element.PressKeysAsync(["NumberPad9"]),
                "Keys.PageDown" => () => element.PressKeysAsync(["PageDown"]),
                "Keys.PageUp" => () => element.PressKeysAsync(["PageUp"]),
                "Keys.Pause" => () => element.PressKeysAsync(["Pause"]),
                "Keys.Return" => () => element.PressKeysAsync(["Return"]),
                "Keys.Right" => () => element.PressKeysAsync(["Right"]),
                "Keys.Semicolon" => () => element.PressKeysAsync(["Semicolon"]),
                "Keys.Separator" => () => element.PressKeysAsync(["Separator"]),
                "Keys.Shift" => () => element.PressKeysAsync(["Shift"]),
                "Keys.Space" => () => element.PressKeysAsync(["Space"]),
                "Keys.Subtract" => () => element.PressKeysAsync(["Subtract"]),
                "Keys.Tab" => () => element.PressKeysAsync(["Tab"]),
                "Keys.Up" => () => element.PressKeysAsync(["Up"]),
                _ => () => element.TypeTextAsync(key),
            };
        }


        /// <summary>
        /// Handles sending keys to a browser element at a specified offset position.
        /// Retrieves the element, calculates the target position using the offsets,
        /// moves the mouse to the target position, clears the element, and sends the specified keys.
        /// </summary>
        private async Task HandleSendKeysXYAsync()
        {
            string offsetXAsString = _act.GetOrCreateInputParam(ActUIElement.Fields.XCoordinate).ValueForDriver;
            if (!int.TryParse(offsetXAsString, out int offsetX) || offsetX < 0)
            {
                throw new InvalidActionConfigurationException($"Offset-X '{offsetXAsString}' is not a valid X coordinate position");
            }
            string offsetYAsString = _act.GetOrCreateInputParam(ActUIElement.Fields.YCoordinate).ValueForDriver;
            if (!int.TryParse(offsetXAsString, out int offsetY) || offsetY < 0)
            {
                throw new InvalidActionConfigurationException($"Offset-X '{offsetYAsString}' is not a valid Y coordinate position");
            }
            IBrowserElement element = await GetFirstMatchingElementAsync();
            Point elementPosition = await element.PositionAsync();
            Point mouseTargetPosition = new(elementPosition.X + offsetX, elementPosition.Y + offsetY);

            await _browserTab.MoveMouseAsync(mouseTargetPosition);
            await element.ClearAsync();
            string keys = _act.GetInputParamCalculatedValue("Value");
            Func<Task> elementOperation = ConvertSeleniumKeyIdentifierToElementOperation(keys, element);
            await elementOperation();
        }

        /// <summary>
        /// Handles a mouse press and release action on a browser element.
        /// Retrieves the element, calculates its center position, and simulates a mouse click at the center.
        /// </summary>
        private async Task HandleMousePressReleaseAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            Size size = await element.SizeAsync();
            Point position = await element.PositionAsync();
            Point centerOfElement = new Point(position.X + (int)Math.Round(size.Width / 2.0), position.Y + (int)Math.Round(size.Height / 2.0));
            await _browserTab.MouseClickAsync(centerOfElement);
        }

        /// <summary>
        /// Handles a mouse click action on a browser element.
        /// Retrieves the element, calculates its center position, and simulates a mouse click at the center.
        /// </summary>
        private async Task HandleMouseClickAsync()
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            Size size = await element.SizeAsync();
            Point position = await element.PositionAsync();
            Point centerOfElement = new Point(position.X + (int)Math.Round(size.Width / 2.0), position.Y + (int)Math.Round(size.Height / 2.0));
            await _browserTab.MouseClickAsync(centerOfElement);
        }
    }
}


