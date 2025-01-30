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
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.ActionHandlers
{
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
                Reporter.ToLog(eLogLevel.DEBUG,$"Error in operation {_actWebSmartSync.SyncOperations}: {ex.Message} {ex.InnerException?.Message}");
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
                var elements = await _browserTab.GetElementsAsync(_actWebSmartSync.ElementLocateBy, _actWebSmartSync.ElementLocateValue);
                if (elements.Any())
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
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed.TotalSeconds < waitUntilTimeout)
            {
                var state = await _browserTab.ExecuteJavascriptAsync("document.readyState");
                if (state.Equals("complete", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                await Task.Delay(100);
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
            IBrowserElement attributeElement = await GetFirstMatchingElementAsync();
            string attributeName = _actWebSmartSync.GetInputParamCalculatedValue(nameof(ActWebSmartSync.AttributeName));
            string attributeValue = _actWebSmartSync.GetInputParamCalculatedValue(nameof(ActWebSmartSync.AttributeValue));
            if (string.IsNullOrEmpty(attributeName) || string.IsNullOrEmpty(attributeValue))
            {
                throw new InvalidDataException("For AttributeMatches operation, the input value is missing or invalid.");
            }
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
            if (await _browserTab.WaitForElementsEnabledAsync(_actWebSmartSync.ElementLocateBy, _actWebSmartSync.ElementLocateValue, waitUntilTimeout))
            {
                return;
            }
            act.Error = "Not all elements are enabled within the given time.";
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
            if (await _browserTab.WaitForElementsInvisibleAsync(_actWebSmartSync.ElementLocateBy, _actWebSmartSync.ElementLocateValue, waitUntilTimeout))
            {
                return;
            }
            act.Error = "Not all elements are invisible within the given time.";
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
            if (await _browserTab.WaitForElementsPresenceAsync(_actWebSmartSync.ElementLocateBy, _actWebSmartSync.ElementLocateValue, waitUntilTimeout))
            {
                return;
            }
            act.Error = "Elements are not present within the given time.";
        }

        /// <summary>
        /// Handles the operation to check if all elements located by are selected within the given timeout.
        /// </summary>
        private async Task HandleSelectedOfAllElementsLocatedByAsync(Act act, float waitUntilTimeout)
        {
            if (await _browserTab.WaitForElementsCheckedAsync(_actWebSmartSync.ElementLocateBy, _actWebSmartSync.ElementLocateValue, waitUntilTimeout))
            {
                return;
            }
            act.Error = "Not all elements are selected within the given time.";
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
            if (await _browserTab.WaitForElementsVisibleAsync(_actWebSmartSync.ElementLocateBy, _actWebSmartSync.ElementLocateValue, waitUntilTimeout))
            {
                return;
            }
            act.Error = "Not all elements are visible within the given time.";
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

    }
}
