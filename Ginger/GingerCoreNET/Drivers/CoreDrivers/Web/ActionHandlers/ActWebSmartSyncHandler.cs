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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.ActionHandlers
{
    /// <summary>
    /// Handles various web synchronization operations for the ActWebSmartSync action.
    /// </summary>
    internal class ActWebSmartSyncHandler
    {
        private ActWebSmartSync _actWebSmartSync;
        private IBrowserTab _browserTab;
        private IBrowserElementLocator _elementLocator;

        internal ActWebSmartSyncHandler(ActWebSmartSync actWebSmartSync, IBrowserTab browserTab, IBrowserElementLocator elementLocator)
        {
            _actWebSmartSync = actWebSmartSync;
            _browserTab = browserTab;
            _elementLocator = elementLocator;
        }

        /// <summary>
        /// Executes various web synchronization operations within a specified timeout.
        /// Sets an error message in the act object if the condition is not met in time.
        /// </summary>
        public async Task HandleAsync(Act act, float waitUntilTimeout)
        {
            try
            {
                switch (_actWebSmartSync.SyncOperations)
                {
                    case ActWebSmartSync.eSyncOperation.ElementIsVisible:
                        await HandleElementIsVisibleAsync(act, waitUntilTimeout);
                        break;

                    case ActWebSmartSync.eSyncOperation.ElementExists:
                        await HandleElementExistsAsync(act, waitUntilTimeout);
                        break;

                    case ActWebSmartSync.eSyncOperation.AlertIsPresent:
                        await HandleAlertIsPresentAsync(act, waitUntilTimeout);
                        break;

                    case ActWebSmartSync.eSyncOperation.ElementIsSelected:
                        await HandleElementIsSelectedAsync(act, waitUntilTimeout);
                        break;

                    case ActWebSmartSync.eSyncOperation.PageHasBeenLoaded:
                        await HandlePageHasBeenLoadedAsync(act, waitUntilTimeout);
                        break;

                    case ActWebSmartSync.eSyncOperation.ElementToBeClickable:
                        await HandleElementToBeClickableAsync(act, waitUntilTimeout);
                        break;

                    case ActWebSmartSync.eSyncOperation.TextMatches:
                        await HandleTextMatchesAsync(act, waitUntilTimeout);
                        break;

                    case ActWebSmartSync.eSyncOperation.AttributeMatches:
                        await HandleAttributeMatchesAsync(act, waitUntilTimeout);
                        break;

                    case ActWebSmartSync.eSyncOperation.EnabilityOfAllElementsLocatedBy:
                        await HandleEnabilityOfAllElementsLocatedByAsync(act, waitUntilTimeout);
                        break;

                    case ActWebSmartSync.eSyncOperation.FrameToBeAvailableAndSwitchToIt:
                        await HandleFrameToBeAvailableAndSwitchToItAsync(act, waitUntilTimeout);
                        break;

                    case ActWebSmartSync.eSyncOperation.InvisibilityOfAllElementsLocatedBy:
                        await HandleInvisibilityOfAllElementsLocatedByAsync(act, waitUntilTimeout);
                        break;

                    case ActWebSmartSync.eSyncOperation.InvisibilityOfElementLocated:
                        await HandleInvisibilityOfElementLocatedAsync(act, waitUntilTimeout);
                        break;

                    case ActWebSmartSync.eSyncOperation.PresenceOfAllElementsLocatedBy:
                        await HandlePresenceOfAllElementsLocatedByAsync(act, waitUntilTimeout);
                        break;

                    case ActWebSmartSync.eSyncOperation.SelectedOfAllElementsLocatedBy:
                        await HandleSelectedOfAllElementsLocatedByAsync(act, waitUntilTimeout);
                        break;

                    case ActWebSmartSync.eSyncOperation.UrlMatches:
                        await HandleUrlMatchesAsync(act, waitUntilTimeout);
                        break;

                    case ActWebSmartSync.eSyncOperation.VisibilityOfAllElementsLocatedBy:
                        await HandleVisibilityOfAllElementsLocatedByAsync(act, waitUntilTimeout);
                        break;

                    default:
                        act.Error = "Unsupported operation.";
                        break;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"Error in operation {_actWebSmartSync.SyncOperations}: {ex.Message} {ex.InnerException?.Message}");
                act.Error = ex.Message + ex.InnerException?.Message;
            }
        }

        /// <summary>
        /// Handles the operation to check if an element is visible within the given timeout.
        /// </summary>
        private async Task HandleElementIsVisibleAsync(Act act, float waitUntilTimeout)
        {
            IBrowserElement element = await GetFirstMatchingElementAsync();
            if (await element.ToBeVisibleAsync(waitUntilTimeout))
            {
                return;
            }
            act.Error = "Element is not visible within the given time.";
        }

        /// <summary>
        /// Handles the operation to check if an element exists within the given timeout.
        /// </summary>
        private async Task HandleElementExistsAsync(Act act, float waitUntilTimeout)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed.TotalSeconds < waitUntilTimeout)
            {
                IEnumerable<IBrowserElement> elements = await _elementLocator.FindMatchingElements(_actWebSmartSync.ElementLocateBy, _actWebSmartSync.ElementLocateValue);
                IBrowserElement? firstElement = elements.FirstOrDefault();

                if (firstElement != null)
                {   
                    return;
                }
                await Task.Delay(100);
            }
            act.Error = "Element does not exist within the given time.";
        }

        /// <summary>
        /// Handles the operation to check if an alert is present within the given timeout.
        /// </summary>
        private async Task HandleAlertIsPresentAsync(Act act, float waitUntilTimeout)
        {
            if (await _browserTab.WaitForAlertAsync(waitUntilTimeout))
            {
                return;
            }
            act.Error = "Alert is not present within the given time.";
        }

        /// <summary>
        /// Handles the operation to check if an element is selected within the given timeout.
        /// </summary>
        private async Task HandleElementIsSelectedAsync(Act act, float waitUntilTimeout)
        {
            IBrowserElement selectedElement = await GetFirstMatchingElementAsync();
            if (await selectedElement.ElementIsSelectedAsync(waitUntilTimeout))
            {
                return;
            }
            act.Error = "Element is not selected within the given time.";
        }

        /// <summary>
        /// Handles the operation to check if the page has been loaded within the given timeout.
        /// </summary>
        private async Task HandlePageHasBeenLoadedAsync(Act act, float waitUntilTimeout)
        {
            if (await _browserTab.WaitTillLoadedAsync(waitUntilTimeout))
            {
                return;
            }
            act.Error = "Page has not been loaded within the given time.";

        }

        /// <summary>
        /// Handles the operation to check if an element is clickable within the given timeout.
        /// </summary>
        private async Task HandleElementToBeClickableAsync(Act act, float waitUntilTimeout)
        {
            IBrowserElement clickableElement = await GetFirstMatchingElementAsync();
            if (await clickableElement.ElementToBeClickableAsync(waitUntilTimeout))
            {
                return;
            }
            act.Error = "Element is not clickable within the given time.";
        }

        /// <summary>
        /// Handles the operation to check if the text matches within the given timeout.
        /// </summary>
        private async Task HandleTextMatchesAsync(Act act, float waitUntilTimeout)
        {
            string expectedText = _actWebSmartSync.GetInputParamCalculatedValue(nameof(ActWebSmartSync.TxtMatchInput));
            if (string.IsNullOrEmpty(expectedText))
            {
                throw new InvalidDataException("For TextMatches operation, the input value is missing or invalid.");
            }
            IBrowserElement textElement = await GetFirstMatchingElementAsync();
            if (await textElement.TextMatchesAsync(expectedText, waitUntilTimeout))
            {
                return;
            }
            act.Error = "Text does not match within the given time.";
        }

        /// <summary>
        /// Handles the operation to check if the attribute matches within the given timeout.
        /// </summary>
        private async Task HandleAttributeMatchesAsync(Act act, float waitUntilTimeout)
        {

            string attributeName = _actWebSmartSync.GetInputParamCalculatedValue(nameof(ActWebSmartSync.AttributeName));
            string attributeValue = _actWebSmartSync.GetInputParamCalculatedValue(nameof(ActWebSmartSync.AttributeValue));
            if (string.IsNullOrEmpty(attributeName))
            {
                throw new InvalidDataException("For AttributeMatches operation, the Attribute Name value is missing or invalid.");
            }
            if (string.IsNullOrEmpty(attributeValue))
            {
                throw new InvalidDataException("For AttributeMatches operation, the Attribute Value value is missing or invalid.");
            }
            IBrowserElement attributeElement = await GetFirstMatchingElementAsync();
            if (await attributeElement.AttributeMatchesAsync(attributeName, attributeValue, waitUntilTimeout))
            {
                return;
            }
            act.Error = "Attribute does not match within the given time.";
        }

        /// <summary>
        /// Handles the operation to check if all elements located by are enabled within the given timeout.
        /// </summary>
        private async Task HandleEnabilityOfAllElementsLocatedByAsync(Act act, float waitUntilTimeout)
        {
            try
            {
                await GetLocateByandValue();
                var elementsEnabled = await _browserTab.WaitForElementsEnabledAsync(eLocateBy, eLocateValue, waitUntilTimeout);
                if (!elementsEnabled)
                {
                    act.Error = "Not all elements are enabled within the given time.";
                }
            }
            catch (Exception ex)
            {
                act.Error = $"An error occurred: {ex.Message}";
            }
        }

   

        eLocateBy eLocateBy;
        string eLocateValue;
        /// <summary>
        /// Retrieves the locate by and value for the element.
        /// </summary>
        private async Task GetLocateByandValue()
        {
            eLocateBy = _actWebSmartSync.ElementLocateBy;
            eLocateValue = _actWebSmartSync.ElementLocateValue;
            if (eLocateBy == eLocateBy.POMElement)
            {
                var locators = await GetPOMElementLocator();
                eLocateBy = locators.Item1;
                eLocateValue = locators.Item2;
            }
        }



        /// <summary>
        /// Handles the operation to check if the frame is available and switches to it within the given timeout.
        /// </summary>
        private async Task HandleFrameToBeAvailableAndSwitchToItAsync(Act act, float waitUntilTimeout)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed.TotalSeconds < waitUntilTimeout)
            {
                bool wasSwitched = await _browserTab.SwitchFrameAsync(_actWebSmartSync.ElementLocateBy, _actWebSmartSync.ElementLocateValue);
                if (wasSwitched)
                {
                    return;
                }
                await Task.Delay(100);
            }
            act.Error = "Frame is not available within the given time.";
        }

        /// <summary>
        /// Handles the operation to check if all elements located by are invisible within the given timeout.
        /// </summary>
        private async Task HandleInvisibilityOfAllElementsLocatedByAsync(Act act, float waitUntilTimeout)
        {
            try
            {
                await GetLocateByandValue();
                var elementsInvisible = await _browserTab.WaitForElementsInvisibleAsync(eLocateBy, eLocateValue, waitUntilTimeout);
                if (!elementsInvisible)
                {
                    act.Error = "Not all elements are invisible within the given time.";
                }
            }
            catch (Exception ex)
            {
                act.Error = $"An error occurred: {ex.Message}";
            }
        }

        /// <summary>
        /// Handles the operation to check if an element is invisible within the given timeout.
        /// </summary>
        private async Task HandleInvisibilityOfElementLocatedAsync(Act act, float waitUntilTimeout)
        {
            IBrowserElement invisibleElement = await GetFirstMatchingElementAsync();
            if (await invisibleElement.ToBeNotVisibleAsync(waitUntilTimeout))
            {
                return;
            }
            act.Error = "Element is not visible within the given time.";
        }

        /// <summary>
        /// Handles the operation to check if all elements located by are present within the given timeout.
        /// </summary>
        private async Task HandlePresenceOfAllElementsLocatedByAsync(Act act, float waitUntilTimeout)
        {
            try
            {
                await GetLocateByandValue();

                var elementsPresent = await _browserTab.WaitForElementsPresenceAsync(eLocateBy, eLocateValue, waitUntilTimeout);
                if (!elementsPresent)
                {
                    act.Error = "Elements are not present within the given time.";
                }
            }
            catch (Exception ex)
            {
                act.Error = $"An error occurred: {ex.Message}";
            }
        }

        /// <summary>
        /// Handles the operation to check if all elements located by are selected within the given timeout.
        /// </summary>
        private async Task HandleSelectedOfAllElementsLocatedByAsync(Act act, float waitUntilTimeout)
        {
            try
            {
                await GetLocateByandValue();

                var elementsLocated = await _browserTab.WaitForElementsCheckedAsync(eLocateBy, eLocateValue, waitUntilTimeout);
                if (!elementsLocated)
                {
                    act.Error = "Not all elements are selected within the given time.";
                }
            }
            catch (Exception ex)
            {
                act.Error = $"An error occurred: {ex.Message}";
            }
        }



        /// <summary>
        /// Handles the operation to check if the URL matches within the given timeout.
        /// </summary>
        private async Task HandleUrlMatchesAsync(Act act, float waitUntilTimeout)
        {
            string expectedUrl = _actWebSmartSync.GetInputParamCalculatedValue(nameof(ActWebSmartSync.UrlMatches));
            if (string.IsNullOrEmpty(expectedUrl))
            {
                throw new InvalidDataException("For URLMatches operation, the input value is missing or invalid.");
            }
            if (await _browserTab.WaitForUrlMatchAsync(expectedUrl, waitUntilTimeout))
            {
                return;
            }
            act.Error = "URL does not match within the given time.";
        }

        /// <summary>
        /// Handles the operation to check if all elements located by are visible within the given timeout.
        /// </summary>
        private async Task HandleVisibilityOfAllElementsLocatedByAsync(Act act, float waitUntilTimeout)
        {
            try
            {
               await GetLocateByandValue();

                var elementsVisible = await _browserTab.WaitForElementsVisibleAsync(eLocateBy, eLocateValue, waitUntilTimeout);
                if (!elementsVisible)
                {
                    act.Error = "Not all elements are visible within the given time.";
                }
            }
            catch (Exception ex)
            {
                act.Error = $"An error occurred: {ex.Message}";
            }
        }

        /// <summary>
        /// Retrieves the first matching element based on the locator and value.
        /// </summary>
        private async Task<IBrowserElement> GetFirstMatchingElementAsync()
        {
            IEnumerable<IBrowserElement> elements = await _elementLocator.FindMatchingElements(_actWebSmartSync.ElementLocateBy, _actWebSmartSync.ElementLocateValue);
            IBrowserElement? firstElement = elements.FirstOrDefault();
            if (firstElement == null)
            {
                throw new InvalidDataException($"Element not found by:{_actWebSmartSync.ElementLocateBy} {_actWebSmartSync.ElementLocateValue}");
            }
            return firstElement;
        }

        /// <summary>
        /// Retrieves the element locator for the first matching element.
        /// </summary>
        async Task<string> GetElementLocator()
        {
            var element = await GetFirstMatchingElementAsync();
            return await element.GetElementLocator();
        }
        /// <summary>
        /// Retrieves the CSS selector value for the first matching element.
        /// </summary>
        private async Task<(eLocateBy, string)> GetPOMElementLocator()
        {
            string locator = await GetElementLocator();
            string cssValue = null;
            eLocateBy locateBy = eLocateBy.Unknown; // Default value
            if (locator != null)
            {

                switch (locator?.Split('@')[1].Split('=')[0])
                {
                    case "css":
                        locateBy = eLocateBy.ByCSSSelector;
                        break;
                    case "xpath":
                        locateBy = eLocateBy.ByXPath;
                        break;
                    case "id":
                        locateBy = eLocateBy.ByID;
                        break;
                    case "name":
                        locateBy = eLocateBy.ByName;
                        break;
                    case "class":
                        locateBy = eLocateBy.ByClassName;
                        break;
                    case "linkText":
                        locateBy = eLocateBy.ByLinkText;
                        break;
                    case "tagName":
                        locateBy = eLocateBy.ByTagName;
                        break;
                    case "relXPath":
                        locateBy = eLocateBy.ByRelXPath;
                        break;
                    default:
                        throw new InvalidDataException("Locators not found.");
                        
                }

                cssValue = locator?.Split('=')[1].Split(' ')[0];
                cssValue = cssValue?.Trim();
            }
            return (locateBy, cssValue);
        }
    }
}
