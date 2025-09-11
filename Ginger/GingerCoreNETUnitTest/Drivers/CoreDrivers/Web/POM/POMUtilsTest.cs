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

using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.POM;
using Newtonsoft.Json;
using System.Reflection;

namespace GingerCoreNETUnitTest.Drivers.CoreDrivers.Web.POM
{
    [TestClass]
    public class POMUtilsTest
    {
        private POMUtils _utils;

        [TestInitialize]
        public void Init()
        {
            _utils = (POMUtils)Activator.CreateInstance(typeof(POMUtils), nonPublic: true);
        }

        private ElementInfo CreateElement(Guid? guid = null, string title = "MyTitle", string initialName = "OrigName", string initialDesc = "OrigDesc")
        {
            var ei = new ElementInfo();
            typeof(ElementInfo).GetProperty("Guid").SetValue(ei, guid ?? Guid.NewGuid());

            ei.ElementName = initialName;
            ei.Description = initialDesc;

            // Properties collection
            ei.Properties = new ObservableList<ControlProperty>
            {
                new ControlProperty(){ Name="Title", Value=title},
                new ControlProperty(){ Name="RandomPropX", Value="ShouldBeFilteredOut"},
                new ControlProperty(){ Name="ID", Value="elem-id-1"}
            };

            // Locators collection
            ei.Locators = new ObservableList<ElementLocator>
            {
                new ElementLocator(){ LocateBy = eLocateBy.ByID, LocateValue="elem-id-1", Active=true},
                new ElementLocator(){ LocateBy = eLocateBy.ByName, LocateValue="" } // should be ignored in JSON
            };
            return ei;
        }

        private string BuildAIResponse(Guid guid, string enhanceName, string enhanceDesc, Dictionary<string,string> locatorsDict, bool wrapInData = true, bool asStringPayload = false, bool emptyLocatorsJson = false)
        {
            var wrapper = new
            {
                elementinfo = new
                {
                    elementGuid = guid,
                    Properties = new
                    {
                        EnhanceName = enhanceName,
                        EnhanceDescription = enhanceDesc
                    },
                    locators = new
                    {
                        EnhanceLocatorsByAI = emptyLocatorsJson ? "" : JsonConvert.SerializeObject(locatorsDict)
                    }
                }
            };

            var arrJson = JsonConvert.SerializeObject(new[] { wrapper });

            if (asStringPayload)
            {
                // Response itself is a JSON string containing the array
                return JsonConvert.SerializeObject(arrJson);
            }

            if (wrapInData)
            {
                return JsonConvert.SerializeObject(new
                {
                    data = new
                    {
                        genai_result = arrJson
                    }
                });
            }

            return arrJson;
        }

        [TestMethod]
        public void GenerateJsonToSendAIRequestByList_FiltersPropertiesAndLocators()
        {
            var ps = new PomSetting() { LearnPOMByAI = true };
            var list = new ObservableList<ElementInfo>();
            var e1 = CreateElement();
            list.Add(e1);

            var result = _utils.GenerateJsonToSendAIRequestByList(ps, list);

            Assert.AreEqual(1, result.elements.Count, "One element expected");
            var elementObj = result.elements[0].elementinfo;
            // Use reflection to get Properties object
            var propsObj = elementObj.Properties;
            // Title should be copied
            var titleProp = propsObj.GetType().GetProperty("ByTitle", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(titleProp);
            Assert.AreEqual("MyTitle", titleProp.GetValue(propsObj));
            // Random property should not exist (returns null)
            var randomProp = propsObj.GetType().GetProperty("ByRandomPropX", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNull(randomProp);
            // Locators: only ID non-empty should be mapped
            var locatorsObj = elementObj.locators;
            var idProp = locatorsObj.GetType().GetProperty("ByID");
            Assert.IsNotNull(idProp);
            Assert.AreEqual("elem-id-1", idProp.GetValue(locatorsObj));
            var nameProp = locatorsObj.GetType().GetProperty("ByName");
            if (nameProp != null)
            {
                Assert.IsTrue(string.IsNullOrEmpty(Convert.ToString(nameProp.GetValue(locatorsObj))), "Empty locator should not be assigned a value");
            }
        }

        [TestMethod]
        public void GenerateJsonToSendAIRequestByList_LearnDisabled_ReturnsEmpty()
        {
            var ps = new PomSetting() { LearnPOMByAI = false };
            var list = new ObservableList<ElementInfo> { CreateElement() };
            var result = _utils.GenerateJsonToSendAIRequestByList(ps, list);
            Assert.AreEqual(0, result.elements.Count);
        }

        [TestMethod]
        public void ProcessGenAIResponseAndUpdatePOM_SuccessWithDataWrapper()
        {
            var list = new ObservableList<ElementInfo>();
            var guid = Guid.NewGuid();
            var ei = CreateElement(guid);
            list.Add(ei);

            var locators = new Dictionary<string, string>
            {
                { "ID", "new-id-val" },
                { "CustomUnknown", "//div[@x='y']" }
            };

            var resp = BuildAIResponse(guid, "EnhancedName", "EnhancedDesc", locators, wrapInData: true);
            _utils.ProcessGenAIResponseAndUpdatePOM(list, resp, ePomElementCategory.Web);

            Assert.AreEqual("EnhancedName", ei.ElementName);
            Assert.AreEqual("EnhancedDesc", ei.Description);
            Assert.IsTrue(ei.IsProcessed);

            var addedLocators = ei.Locators.Where(l => l.IsAIGenerated).ToList();
            Assert.AreEqual(2, addedLocators.Count);

            var idLoc = addedLocators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath);
            Assert.IsNotNull(idLoc);
            Assert.AreEqual("new-id-val", idLoc.LocateValue);
            Assert.AreEqual(ePomElementCategory.Web, idLoc.Category);

            var unknownLoc = addedLocators.FirstOrDefault(l => l.LocateValue == "//div[@x='y']");
            Assert.IsNotNull(unknownLoc);
            Assert.AreEqual(eLocateBy.ByRelXPath, unknownLoc.LocateBy);
        }

        [TestMethod]
        public void ProcessGenAIResponseAndUpdatePOM_FallbackJsonStringParsing()
        {
            var list = new ObservableList<ElementInfo>();
            var guid = Guid.NewGuid();
            var ei = CreateElement(guid);
            list.Add(ei);

            var locators = new Dictionary<string, string>
            {
                { "XPath", "//*[@" + "id='x']" }
            };

            var resp = BuildAIResponse(guid, "Name2", "Desc2", locators, wrapInData: false, asStringPayload: true);
            _utils.ProcessGenAIResponseAndUpdatePOM(list, resp, ePomElementCategory.Web);

            Assert.AreEqual("Name2", ei.ElementName);
            Assert.AreEqual("Desc2", ei.Description);
            Assert.IsTrue(ei.IsProcessed);
            Assert.IsTrue(ei.Locators.Any(l => l.IsAIGenerated && l.LocateValue.Contains("@id='x'")));
        }

        [TestMethod]
        public void ProcessGenAIResponseAndUpdatePOM_ErrorUnauthorized_NoChanges()
        {
            var list = new ObservableList<ElementInfo>();
            var guid = Guid.NewGuid();
            var ei = CreateElement(guid, initialName: "OrigName", initialDesc: "OrigDesc");
            list.Add(ei);

            _utils.ProcessGenAIResponseAndUpdatePOM(list, "unauthorized user", ePomElementCategory.Web);

            Assert.AreEqual("OrigName", ei.ElementName);
            Assert.AreEqual("OrigDesc", ei.Description);
            Assert.IsFalse(ei.IsProcessed);
            Assert.IsFalse(ei.Locators.Any(l => l.IsAIGenerated));
        }

        [TestMethod]
        public void ProcessGenAIResponseAndUpdatePOM_CodeFenceCleaned()
        {
            var list = new ObservableList<ElementInfo>();
            var guid = Guid.NewGuid();
            var ei = CreateElement(guid);
            list.Add(ei);

            var locators = new Dictionary<string, string> { { "ID", "after-fence" } };
            var inner = BuildAIResponse(guid, "FenceName", "FenceDesc", locators, wrapInData: false);
            var fenced = $"```json\n{{\"data\":{{genai_result:{inner}}}}}\n```";
            _utils.ProcessGenAIResponseAndUpdatePOM(list, fenced, ePomElementCategory.Web);

            Assert.AreEqual("FenceName", ei.ElementName);
            Assert.AreEqual("FenceDesc", ei.Description);
            Assert.IsTrue(ei.Locators.Any(l => l.IsAIGenerated && l.LocateValue == "after-fence"));
        }

        [TestMethod]
        public void ProcessGenAIResponseAndUpdatePOM_EmptyEnhanceLocators_NoLocatorAdded()
        {
            var list = new ObservableList<ElementInfo>();
            var guid = Guid.NewGuid();
            var ei = CreateElement(guid);
            var originalLocatorCount = ei.Locators.Count;
            list.Add(ei);

            var resp = BuildAIResponse(guid, "NoLocName", "NoLocDesc", new Dictionary<string, string>(), wrapInData: true, emptyLocatorsJson: true);
            _utils.ProcessGenAIResponseAndUpdatePOM(list, resp, ePomElementCategory.Web);

            Assert.AreEqual("NoLocName", ei.ElementName);
            Assert.AreEqual("NoLocDesc", ei.Description);
            Assert.IsTrue(ei.IsProcessed);
            Assert.AreEqual(originalLocatorCount, ei.Locators.Count, "No new locators should be added");
        }

        [TestMethod]
        public void ProcessGenAIResponseAndUpdatePOM_IgnoresElementsNotInList()
        {
            var list = new ObservableList<ElementInfo>();
            var guidInList = Guid.NewGuid();
            var guidMissing = Guid.NewGuid();
            var ei = CreateElement(guidInList);
            list.Add(ei);

            var locators = new Dictionary<string, string> { { "ID", "present" } };
            var payloadForExisting = BuildAIResponse(guidInList, "NewName", "NewDesc", locators, wrapInData: false);
            var payloadForMissing = BuildAIResponse(guidMissing, "IgnoreName", "IgnoreDesc", locators, wrapInData: false);
            var combinedArray = $"```json\n{{\"data\":{{genai_result:[{payloadForExisting.Trim('[', ']')},{payloadForMissing.Trim('[', ']')}]}}}}\n```";
            _utils.ProcessGenAIResponseAndUpdatePOM(list, combinedArray, ePomElementCategory.Web);

            Assert.AreEqual("NewName", ei.ElementName);
            Assert.AreEqual("NewDesc", ei.Description);
            Assert.IsFalse(list.Any(e => e.Guid == guidMissing));
        }
    }
}