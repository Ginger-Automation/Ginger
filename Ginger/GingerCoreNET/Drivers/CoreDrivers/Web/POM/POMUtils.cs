#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Mobile;
using Amdocs.Ginger.CoreNET.GeneralLib;
using Amdocs.Ginger.Repository;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.POM
{
    public sealed class POMUtils
    {

        private static readonly HashSet<string> FilterProperties = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "name", "Platform Element Type", "Element Type", "TagName", "Text", "Value",
            "AutomationID", "Title", "AriaLabel", "DataTestId", "Placeholder", "ID"
        };

        ConcurrentQueue<ElementInfo> processingQueue = new ConcurrentQueue<ElementInfo>();
        private readonly ConcurrentDictionary<Guid, byte> _enqueuedIds = new();

        private Dictionary<string, int> nameCount = new Dictionary<string, int>();

        private readonly object lockObj = new object();

        public event EventHandler<bool> ProcessingStatusChanged;

        private volatile bool _isProcessing;
        public bool IsProcessing => _isProcessing;
        private void SetProcessing(bool value)
        {
            bool changed = false;
            lock (lockObj)
            {
                if (_isProcessing != value)
                {
                    _isProcessing = value;
                    changed = true;
                }
            }
            if (changed) { ProcessingStatusChanged?.Invoke(this, _isProcessing); }
        }

        public ElementWrapperInfo GenerateJsonToSendAIRequestByList(PomSetting pomSetting, ObservableList<ElementInfo> foundElementList)
        {
            ElementWrapperInfo elementWrapperInfo = new ElementWrapperInfo();
            elementWrapperInfo.elements = new List<ElementWrapper>();
            if (pomSetting.LearnPOMByAI)
            {
                for (int i = 0; i < foundElementList.Count; i++)
                {

                    ElementWrapper element = new ElementWrapper();

                    element.elementinfo = new Element();

                    ElementInfo elementInfo = foundElementList[i];

                    element.elementinfo.elementGuid = elementInfo.Guid;

                    ElementwrapperProperties props = new ElementwrapperProperties();

                    // Filter and assign properties
                    foreach (var prop in elementInfo.Properties)
                    {
                        // Check if property name is in the filter list AND value is not null or whitespace
                        if (FilterProperties.Contains(prop.Name) &&
                            !string.IsNullOrWhiteSpace(prop.Value?.ToString()))
                        {
                            var propertyInfo = typeof(ElementwrapperProperties)
                                .GetProperty(prop.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                            if (propertyInfo != null && propertyInfo.CanWrite)
                            {
                                propertyInfo.SetValue(props, prop.Value);
                            }
                        }
                    }


                    element.elementinfo.Properties = props;

                    ElementwrapperLocators Locators = new ElementwrapperLocators();
                    foreach (var elementLocator in elementInfo.Locators)
                    {

                        if (!string.IsNullOrWhiteSpace(elementLocator.LocateValue))
                        {

                            typeof(ElementwrapperLocators)
                            .GetProperty(elementLocator.LocateBy.ToString(), System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                            ?.SetValue(Locators, elementLocator.LocateValue);
                        }
                    }
                    element.elementinfo.locators = Locators;
                    elementWrapperInfo.elements.Add(element);
                }
            }
            return elementWrapperInfo;
        }
        public async Task<string> SendInBatchesList(ElementWrapperInfo elementWrapperInfo, ObservableList<ElementInfo> list, string url, ePomElementCategory? PomCategory, eDevicePlatformType? DevicePlatformType = null, int batchSize = 2000)
        {
            var responses = new List<string>();
            var errors = new List<string>();
            var currentBatch = new List<string>();
            int currentSize = 2; // For opening and closing brackets of JSON array
            batchSize = !string.IsNullOrEmpty(GingerCoreNET.GeneralLib.General.GetAIBatchsize()) ? Convert.ToInt32(GingerCoreNET.GeneralLib.General.GetAIBatchsize()) : batchSize;

            foreach (var element in elementWrapperInfo.elements)
            {
                string serializedElement = JsonConvert.SerializeObject(element, Formatting.None);
                int elementSize = Encoding.UTF8.GetByteCount(serializedElement) + 1; // +1 for comma

                if (currentSize + elementSize > batchSize && currentBatch.Count > 0)
                {
                    // Send current batch
                    string batchPayload = "[" + string.Join(",", currentBatch) + "]";
                    try
                    {
                        await GetResponseFromGenAI(list, url, batchPayload, PomCategory, DevicePlatformType);
                        responses.Add($"Batch processed successfully with {currentBatch.Count} elements");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Batch processing failed: {ex.Message}");
                        Reporter.ToLog(eLogLevel.ERROR, $"Failed to process batch", ex);
                    }

                    // Reset batch
                    currentBatch.Clear();
                    currentSize = 2;
                }

                currentBatch.Add(serializedElement);
                currentSize += elementSize;
            }

            // Send remaining batch
            if (currentBatch.Count > 0)
            {
                string batchPayload = "[" + string.Join(",", currentBatch) + "]";
                try
                {
                    await GetResponseFromGenAI(list, url, batchPayload, PomCategory, DevicePlatformType);
                    responses.Add($"Final batch processed successfully with {currentBatch.Count} elements");
                }
                catch (Exception ex)
                {
                    errors.Add($"Final batch processing failed: {ex.Message}");
                    Reporter.ToLog(eLogLevel.ERROR, $"Failed to process final batch", ex);
                }
            }

            if (errors.Count > 0)
            {
                responses.Add($"Errors encountered: {string.Join("; ", errors)}");
            }

            return string.Join("\n---\n", responses);
        }

        public async Task GetResponseFromGenAI(ObservableList<ElementInfo> list, string url, string batchPayload, ePomElementCategory? PomCategory, eDevicePlatformType? DevicePlatformType = null)
        {
            string response = await GingerCoreNET.GeneralLib.General.GetResponseForprocess_extracted_elementsByOpenAI(batchPayload, DevicePlatformType, url);
            ProcessGenAIResponseAndUpdatePOM(list, response, PomCategory);
        }
        public void ProcessGenAIResponseAndUpdatePOM(ObservableList<ElementInfo> list, string response, ePomElementCategory? PomCategory)
        {
            string cleanedResponse = CleanAIResponse(response);
            Reporter.ToLog(eLogLevel.DEBUG, $"cleanedResponse : {cleanedResponse}");
            if (IsErrorResponse(cleanedResponse))
            {
                Reporter.ToLog(eLogLevel.INFO, "Failed to connect to OpenAI API. Please check your internet connection or firewall settings");

            }
            else if (cleanedResponse.Contains("Error:"))
            {
                Reporter.ToLog(eLogLevel.INFO, "Failed to connect to OpenAI API. Please check your internet connection or firewall settings");
            }
            else
            {
                // LLMs sometimes return JSON wrapped as a JSON string or fenced. Add a fallback parse before failing the whole batch

                try
                {
                    List<ElementWrapper> responseElements = null;

                    try
                    {
                        var jObject = JObject.Parse(cleanedResponse);
                        var genaiResultToken = jObject["data"]?["genai_result"].ToString();
                        if (genaiResultToken != null)
                        {
                            responseElements = JsonConvert.DeserializeObject<List<ElementWrapper>>((string)genaiResultToken);
                        }
                        else
                        {
                            responseElements = JsonConvert.DeserializeObject<List<ElementWrapper>>(cleanedResponse);
                        }
                    }   
                    catch (JsonException ex)
                    {
                        Reporter.ToLog(eLogLevel.WARN, "AI response could not be parsed into ElementWrapper list.",ex);
                        // Fallback: response is a JSON string containing the payload
                        var inner = JsonConvert.DeserializeObject<string>(cleanedResponse);
                        if (!string.IsNullOrWhiteSpace(inner))
                        {
                            responseElements = JsonConvert.DeserializeObject<List<ElementWrapper>>(inner);
                        }
                    }
                    if (responseElements == null)
                    {
                        Reporter.ToLog(eLogLevel.WARN, "AI response could not be parsed into ElementWrapper list.");
                        return;
                    }

                    foreach (var ele in responseElements)
                    {
                        var existingElement = list.FirstOrDefault(x => x.Guid == ele.elementinfo.elementGuid);
                        if (existingElement != null)
                        {
                            var enhancedName = ele.elementinfo.Properties.EnhanceName;
                            var enhancedDescription = ele.elementinfo.Properties.EnhanceDescription;

                            existingElement.ElementName = enhancedName ?? existingElement.ElementName;
                            existingElement.Description = enhancedDescription ?? existingElement.Description;
                            existingElement.IsProcessed = true;

                            var baseName = existingElement.ElementName;
                            // Strip any existing suffix to get true base name
                            var suffixMatch = System.Text.RegularExpressions.Regex.Match(baseName, @"^(.+)_(\d+)$");
                            if (suffixMatch.Success)
                            {
                                baseName = suffixMatch.Groups[1].Value;
                            }

                            if (nameCount.ContainsKey(baseName))
                            {
                                nameCount[baseName]++;
                                existingElement.ElementName = $"{baseName}_{nameCount[baseName]}";
                            }
                            else
                            {
                                nameCount[baseName] = 0; // First occurrence, no suffix
                                // Check if base name (without suffix) is already taken
                                if (list.Any(e => e != existingElement && e.ElementName == baseName))
                                {
                                    nameCount[baseName]++;
                                    existingElement.ElementName = $"{baseName}_{nameCount[baseName]}";
                                }
                            }
                            if (ele.elementinfo.locators.EnhanceLocatorsByAI != null)
                            {
                                // Deserialize the EnhanceLocatorsByAI property

                                string enhanceLocatorsJson = ele.elementinfo.locators.EnhanceLocatorsByAI?.ToString();

                                if (string.IsNullOrWhiteSpace(enhanceLocatorsJson))
                                {
                                    continue;
                                }
                                Dictionary<string, string> enhanceLocators = JsonConvert.DeserializeObject<Dictionary<string, string>>(enhanceLocatorsJson);

                                if (enhanceLocators != null)
                                {
                                    foreach (var kvp in enhanceLocators)
                                    {
                                        eLocateBy locateBy;

                                        // Try to parse the key to the enum, fallback to Unknown
                                        if (!Enum.TryParse(kvp.Key, true, out locateBy))
                                        {
                                            Reporter.ToLog(eLogLevel.DEBUG, $"Unknown locator type '{kvp.Key}', defaulting to ByRelXPath");
                                            locateBy = eLocateBy.ByRelXPath;
                                        }

                                        if (!string.IsNullOrEmpty(kvp.Value))
                                        {
                                            var locator = new ElementLocator
                                            {
                                                LocateBy = locateBy,
                                                LocateValue = kvp.Value,
                                                IsAutoLearned = true,
                                                Category = PomCategory,
                                                IsAIGenerated = true,
                                                Active = true
                                            };
                                            existingElement.Locators.Add(locator);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error for deserialization of response", ex);
                }

            }
        }

        private bool IsErrorResponse(string response)
        {
            return string.IsNullOrWhiteSpace(response) ||
                   response.Contains("unauthorized", StringComparison.OrdinalIgnoreCase) || response.Contains("Error:", StringComparison.OrdinalIgnoreCase);
        }

        private string CleanAIResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
            {
                return string.Empty;
            }


            return response
                .Replace("```json", "", StringComparison.OrdinalIgnoreCase)
                .Replace("```", "")
            .Trim();
        }

        public void TriggerFineTuneWithAI(PomSetting pomSetting, ElementInfo foundElementInfo, ePomElementCategory? PomCategory, eDevicePlatformType? DevicePlatformType = null)
        {
            if (pomSetting.LearnPOMByAI)
            {
                // Only enqueue if not already processed
                if (!foundElementInfo.IsProcessed && _enqueuedIds.TryAdd(foundElementInfo.Guid, 0))
                {
                    processingQueue.Enqueue(foundElementInfo);
                }

                // Trigger batch processing
                TriggerBatchProcessing(pomSetting, PomCategory, DevicePlatformType);
            }
        }

        public void TriggerDelayProcessingfinetuneWithAI(PomSetting pomSetting, ePomElementCategory? PomCategory, eDevicePlatformType? DevicePlatformType = null)
        {
            if (pomSetting.LearnPOMByAI)
            {
                lock (lockObj)
                {
                    if (processingQueue.Count < 10 && !IsProcessing)
                    {
                        SetProcessing(true);
                        _ = Task.Run(() => TriggerDelayedProcessing(pomSetting, PomCategory, DevicePlatformType));
                    }
                }
            }
        }
        private void TriggerBatchProcessing(PomSetting pomSetting, ePomElementCategory? PomCategory, eDevicePlatformType? DevicePlatformType = null)
        {
            lock (lockObj)
            {
                if (processingQueue.Count >= 10 && !IsProcessing)
                {
                    SetProcessing(true);
                    _ = Task.Run(() => ProcessBatchAsync(pomSetting, PomCategory, DevicePlatformType));
                }
            }
        }

        private async Task ProcessBatchAsync(PomSetting pomSetting, ePomElementCategory? PomCategory, eDevicePlatformType? DevicePlatformType = null)
        {
            try
            {
                while (true)
                {
                    var batch = new ObservableList<ElementInfo>();
                    while (batch.Count < 10 && processingQueue.TryDequeue(out var item))
                    {
                        if (!item.IsProcessed)
                        {
                            batch.Add(item);
                        }
                        _enqueuedIds.TryRemove(item.Guid, out _);
                    }

                    if (batch.Count == 0)
                    { break; }
                    await UpdateAndMarkElementsAsync(pomSetting, batch, PomCategory, DevicePlatformType);
                }
                await FlushRemainingAsync(pomSetting, PomCategory, DevicePlatformType);
            }
            finally
            {
                SetProcessing(false);
            }
        }

        private async Task FlushRemainingAsync(PomSetting pomSetting, ePomElementCategory? PomCategory, eDevicePlatformType? DevicePlatformType = null)
        {
            ObservableList<ElementInfo> remaining = new ObservableList<ElementInfo>();
            while (processingQueue.TryDequeue(out var item))
            {
                if (!item.IsProcessed)
                {
                    remaining.Add(item);
                }
                _enqueuedIds.TryRemove(item.Guid, out _);
            }

            if (remaining.Count > 0)
            {
                await UpdateAndMarkElementsAsync(pomSetting, remaining, PomCategory, DevicePlatformType);
            }
        }

        private async Task TriggerDelayedProcessing(PomSetting pomSetting, ePomElementCategory? PomCategory, eDevicePlatformType? DevicePlatformType = null)
        {
            await Task.Delay(750);
            await ProcessBatchAsync(pomSetting, PomCategory, DevicePlatformType);
        }


        private async Task UpdateAndMarkElementsAsync(PomSetting pomSetting, ObservableList<ElementInfo> foundElementList, ePomElementCategory? PomCategory, eDevicePlatformType? DevicePlatformType = null)
        {
            ElementWrapperInfo elementWrapperInfo = GenerateJsonToSendAIRequestByList(pomSetting, foundElementList);
            await SendInBatchesList(elementWrapperInfo, foundElementList, string.Empty, PomCategory, DevicePlatformType);
        }
    }
}
