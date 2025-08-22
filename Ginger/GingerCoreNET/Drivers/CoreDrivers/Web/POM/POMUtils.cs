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

using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.GeneralLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.POM
{
    internal sealed class POMUtils
    {

        private static readonly List<string> FilterProperties = new List<string> { "name", "Platform Element Type", "Element Type", "TagName", "Text", "Value", "AutomationID", "Title", "AriaLabel", "DataTestId", "Placeholder", "ID" };
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

                        if (FilterProperties.Contains(prop.Name, StringComparer.InvariantCultureIgnoreCase) &&
                                !string.IsNullOrWhiteSpace(prop.Value?.ToString()))

                        {
                            typeof(ElementwrapperProperties)
                                .GetProperty(prop.Name, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                ?.SetValue(props, prop.Value);
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
                    elementInfo.IsProcessed = true;
                }
            }
            return elementWrapperInfo;
        }
        public async Task<string> SendInBatchesList(ElementWrapperInfo elementWrapperInfo, ObservableList<ElementInfo> list, string url, ePomElementCategory? PomCategory, int batchSize = 2000)
        {
            var responses = new List<string>();
            var currentBatch = new List<string>();
            int currentSize = 2; // For opening and closing brackets of JSON array

            foreach (var element in elementWrapperInfo.elements)
            {
                string serializedElement = JsonConvert.SerializeObject(element, Formatting.None);
                int elementSize = Encoding.UTF8.GetByteCount(serializedElement) + 1; // +1 for comma

                if (currentSize + elementSize > batchSize && currentBatch.Count > 0)
                {
                    // Send current batch
                    string batchPayload = "[" + string.Join(",", currentBatch) + "]";
                    await GetResponseFromGenAI(list, url, batchPayload,PomCategory);

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
                await GetResponseFromGenAI(list, url, batchPayload,PomCategory);
            }

            return string.Join("\n---\n", responses);
        }

        public async Task GetResponseFromGenAI(ObservableList<ElementInfo> list, string url, string batchPayload, ePomElementCategory? PomCategory)
        {
            string response = GingerCoreNET.GeneralLib.General.GetResponseForprocess_extracted_elementsByOpenAI(batchPayload).GetAwaiter().GetResult();
            ProcessGenAIResponseAndUpdatePOM(list, response,PomCategory);
        }
        public void ProcessGenAIResponseAndUpdatePOM(ObservableList<ElementInfo> list, string response , ePomElementCategory? PomCategory)
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
                // Parse the response and update the locators

                try
                {
                    List<ElementWrapper> responseElements = JsonConvert.DeserializeObject<List<ElementWrapper>>(cleanedResponse);

                    foreach (var ele in responseElements)
                    {
                        var existingElement = list.FirstOrDefault(x => x.Guid.ToString() == ele.elementinfo.elementGuid.ToString());
                        if (existingElement != null)
                        {
                            var enhancedName = ele.elementinfo.Properties.EnhanceName;
                            var enhancedDescription = ele.elementinfo.Properties.EnhanceDescription;

                            existingElement.ElementName = enhancedName ?? existingElement.ElementName;
                            existingElement.Description = enhancedDescription ?? existingElement.Description;
                            if (ele.elementinfo.locators.EnhanceLocatorsByAI != null)
                            {
                                // Deserialize the EnhanceLocatorsByAI property
                                
                                Dictionary<string, string> enhanceLocators = JsonConvert.DeserializeObject<Dictionary<string, string>>(ele.elementinfo.locators.EnhanceLocatorsByAI.ToString());

                                if (enhanceLocators != null)
                                {
                                    foreach (var kvp in enhanceLocators)
                                    {
                                        eLocateBy locateBy;

                                        // Try to parse the key to the enum, fallback to Unknown
                                        if (!Enum.TryParse(kvp.Key, true, out locateBy))
                                        {
                                            locateBy = eLocateBy.ByRelXPath;
                                        }

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
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error for deserialization of response", ex);
                }

            }
        }

        private bool IsErrorResponse(string response)
        {
            return string.IsNullOrWhiteSpace(response) ||
                   response.Contains("unauthorized", StringComparison.OrdinalIgnoreCase);
        }

        private string CleanAIResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response)) return string.Empty;

            return response
                .Replace("```json", "", StringComparison.OrdinalIgnoreCase)
                .Replace("```", "")
                .Replace("\\\"", "'")
                .Replace("\\n", "")
                .Replace("\\r", "")
                .Replace("\r", "")
                .Replace("\n", "")
                .Trim();
        }

        
    }
}
