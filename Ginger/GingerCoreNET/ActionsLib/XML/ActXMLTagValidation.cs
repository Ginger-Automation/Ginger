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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace GingerCore.Actions.XML
{
    public class ActXMLTagValidation : ActWithoutDriver
    {

        public enum eDocumentType
        {
            XML, JSON
        }
        public new static partial class Fields
        {
            public static string InputFile = "InputFile";
            public static string ReqisFromFile = "ReqisFromFile";
            public static string DocumentType = "DocumentType";
        }


        public override string ActionDescription { get { return "XML/JSON Tag Validation Action"; } }
        public override string ActionUserDescription { get { return "XML/JSON Tag Validation Action"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Validate tags in XML/JSON documents by path");
            TBH.AddLineBreak();

        }

        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

        public override string ActionEditPage { get { return "XML.ActXMLValidateTagsEditPage"; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    AddAllPlatforms();
                }
                return mPlatforms;
            }
        }


        public ActInputValue InputFile
        {
            get
            {
                return GetOrCreateInputParam(Fields.InputFile);

            }
        }

        [IsSerializedForLocalRepository(true)]
        public bool ReqisFromFile
        {
            get; set;
        }
        private eDocumentType mDocumentType;
        [IsSerializedForLocalRepository]
        public eDocumentType DocumentType
        {
            get
            {
                return mDocumentType;
            }
            set
            {
                mDocumentType = value;
            }
        }



        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> DynamicElements = [];


        public override List<ObservableList<ActInputValue>> GetInputValueListForVEProcessing()
        {
            List<ObservableList<ActInputValue>> list = [DynamicElements];
            return list;
        }

        public override String ActionType
        {
            get
            {
                return "XML Tag Validation";
            }
        }
        [IsSerializedForLocalRepository]
        public bool ReadJustXMLAttributeValues
        {
            get; set;
        }
        public override eImageType Image { get { return eImageType.CodeFile; } }



        public override void Execute()
        {
            string docTxt = String.Empty;
            String FilePath = InputFile.ValueForDriver.ToString();

            if (ReqisFromFile == true)
            {
                if (FilePath == null || FilePath == String.Empty)
                {
                    this.Error = "Please provide a valid file name";
                }

                if (FilePath.Contains("~"))
                {
                    FilePath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(FilePath);
                }

                if (FilePath.EndsWith(".XML", StringComparison.OrdinalIgnoreCase) || FilePath.EndsWith(".JSON", StringComparison.OrdinalIgnoreCase))
                {
                    docTxt = System.IO.File.ReadAllText(FilePath);
                }
                else
                {
                    this.Error = "Please provide a valid file path";
                    return;
                }
            }
            else
            {
                if (!FilePath.EndsWith(".XML", StringComparison.OrdinalIgnoreCase) && !FilePath.EndsWith(".JSON", StringComparison.OrdinalIgnoreCase))
                {
                    docTxt = InputFile.ValueForDriver.ToString();
                }
                else
                {
                    this.Error = "Please provide a valid file content";
                    return;
                }
            }

            if (DocumentType == eDocumentType.XML)
            {
                XMLValidation(docTxt);
            }
            else if (DocumentType == eDocumentType.JSON)
            {
                JSONValidation(docTxt);
            }
        }
        private void JSONValidation(string json)
        {
            JToken jo = null;
            try
            {
                jo = JObject.Parse(json);
            }
            catch
            {
                jo = JArray.Parse(json);
            }

            foreach (ActInputValue aiv in DynamicElements)
            {
                ValueExpression VE = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow, DSList)
                {
                    Value = @aiv.Param
                };
                if (string.IsNullOrEmpty(VE.ValueCalculated))
                {
                    continue;
                }
                JToken Tokenfound = jo.SelectToken(VE.ValueCalculated);
                if (Tokenfound != null)
                {
                    AddOrUpdateReturnParamActualWithPath("InnerText", Tokenfound.ToString(), VE.ValueCalculated);
                    if (Tokenfound.Children().Any())
                    {
                        JsonExtended JE = new JsonExtended(Tokenfound.ToString());
                        foreach (JsonExtended item in JE.GetEndingNodes())
                        {
                            AddOrUpdateReturnParamActualWithPath(item.Name, item.JsonString, item.Path);
                        }
                    }
                }
            }
        }

        private void XMLValidation(string xml)
        {
            XmlDocument xmlReqDoc = new XmlDocument();
            xmlReqDoc.LoadXml(xml);

            // Steps
            // 1. Read Source File and replace Place Holders                
            // 2. Copy To target file
            // 3. Wait for processed file to exist
            // 4. read target file into Act.ReturnValues 
            try
            {
                foreach (ActInputValue aiv in DynamicElements)
                {
                    ValueExpression VE = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow, DSList)
                    {
                        Value = @aiv.Param
                    };
                    // var.Value = VE.ValueCalculated;

                    XmlNode node = ReadNodeFromXmlDoc(xmlReqDoc, VE.ValueCalculated);
                    if (!this.ReadJustXMLAttributeValues)
                    {
                        if (node.InnerText != null)
                        {
                            AddOrUpdateReturnParamActualWithPath("InnerText", node.InnerText.ToString(), VE.ValueCalculated);
                        }
                    }

                    if (aiv.Value == null || aiv.Value == String.Empty)
                    {
                        foreach (XmlAttribute XA in node.Attributes)
                        {
                            ActReturnValue rv = ReturnValues.FirstOrDefault(x => x.Path == XA.Name);
                            if (rv == null)
                            {
                                AddOrUpdateReturnParamActualWithPath(aiv.Param, XA.Value.ToString(), XA.Name);
                            }
                            else
                            {
                                rv.Actual = XA.Value.ToString();
                            }
                        }
                    }
                    else
                    {
                        if (node.Attributes != null)
                        {
                            var nameAttribute = node.Attributes[@aiv.Value];
                            ActReturnValue rv = ReturnValues.FirstOrDefault(x => x.Path == aiv.Value && x.FilePath == aiv.FilePath);
                            if (rv == null)
                            {
                                AddOrUpdateReturnParamActualWithPath(aiv.Param, nameAttribute.Value.ToString(), aiv.Value);
                            }
                            else
                            {
                                rv.Actual = nameAttribute.Value.ToString();
                            }
                        }
                    }
                }
            }

            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e);
                throw new System.ArgumentException("Node not found at provided path");
            }
        }

        private XmlNode ReadNodeFromXmlDoc(XmlDocument xmlReqDoc, string valueCalculated)
        {
            string namespaceName = "ns";
            XmlNamespaceManager nameSpaceManager = null;

            XmlAttribute xmlns = GetXmlAttribute(xmlReqDoc);
            string valueCalculatedBackup = valueCalculated;
            if (xmlns != null)
            {
                nameSpaceManager = new XmlNamespaceManager(xmlReqDoc.NameTable);
                nameSpaceManager.AddNamespace(namespaceName, xmlns.Value);
                string namespacePrefix = namespaceName + ":";
                valueCalculated = GetWithPrefix(valueCalculated, namespacePrefix);
            }

            try
            {
                return xmlReqDoc.SelectSingleNode(valueCalculated, nameSpaceManager)
                    ?? xmlReqDoc.SelectSingleNode(valueCalculatedBackup, nameSpaceManager);
            }
            catch (Exception)
            {
                return xmlReqDoc.SelectSingleNode(valueCalculatedBackup, nameSpaceManager);
            }
        }

        private static XmlAttribute GetXmlAttribute(XmlDocument xmlDocument)
        {
            foreach (XmlNode node in xmlDocument.ChildNodes)
            {
                if (node.Attributes != null)
                {
                    foreach (XmlAttribute attribute in node.Attributes)
                    {
                        if (attribute.Name != null && attribute.Name.StartsWith("xmlns"))
                        {
                            return attribute;
                        }
                    }
                }
            }

            return null;
        }

        private string GetWithPrefix(string valueCalculated, string namespacePrefix)
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();

            foreach (string value in valueCalculated.Split('/'))
            {
                if (!string.IsNullOrEmpty(value))
                {
                    stringBuilder.Append(string.Format("{0}{1}/", namespacePrefix, value));
                }
            }

            return stringBuilder.Remove(stringBuilder.Length - 1, 1).ToString();
        }
    }
}
