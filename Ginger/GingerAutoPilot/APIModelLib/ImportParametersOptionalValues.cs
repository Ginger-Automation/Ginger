#region License
/*
Copyright © 2014-2023 European Support Limited

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

using Amdocs.Ginger.Common.Repository.ApplicationModelLib;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using Amdocs.Ginger.Common.GeneralLib;
using Newtonsoft.Json.Linq;

namespace GingerAutoPilot.APIModelLib
{
    public class ImportParametersOptionalValues
    {
        #region XML 
        public void GetXMLAllOptionalValuesFromExamplesFile(TemplateFile XMLTemplateFile, Dictionary<Tuple<string, string>, List<string>> OptionalValuesPerParameterDict)
        {
            XmlDocument XmlDocument = new XmlDocument();
            string FileContent = File.ReadAllText(XMLTemplateFile.FilePath);
            XmlDocument.LoadXml(FileContent);
            XMLDocExtended XDE = new XMLDocExtended(XmlDocument);
            IEnumerable<XMLDocExtended> NodeList = XDE.GetEndingNodes(false);
            foreach (XMLDocExtended XDN in NodeList)
            {
                AddXMLValueToOptionalValuesPerParameterDict(OptionalValuesPerParameterDict, XDN);
            }
        }

        public void GetXMLAllOptionalValuesFromExamplesFile(string fileContent, Dictionary<Tuple<string, string>, List<string>> OptionalValuesPerParameterDict)
        {
            XmlDocument XmlDocument = new XmlDocument();
            XmlDocument.LoadXml(fileContent);
            XMLDocExtended XDE = new XMLDocExtended(XmlDocument);
            IEnumerable<XMLDocExtended> NodeList = XDE.GetEndingNodes(false);
            foreach (XMLDocExtended XDN in NodeList)
            {
                AddXMLValueToOptionalValuesPerParameterDict(OptionalValuesPerParameterDict, XDN);
            }
        }

        private void AddXMLValueToOptionalValuesPerParameterDict(Dictionary<Tuple<string, string>, List<string>> OptionalValuesPerParameterDict, XMLDocExtended XDN)
        {
            Tuple<string, string> tuple = new Tuple<string, string>(XDN.LocalName, XDN.XPath);
            string Value = XDN.Value;

            if (OptionalValuesPerParameterDict.ContainsKey(tuple))
            {
                OptionalValuesPerParameterDict[tuple].Add(Value);
            }
            else
            {
                OptionalValuesPerParameterDict.Add(tuple, new List<string>() { Value });
            }

            foreach (XmlAttribute attribute in XDN.Attributes)
            {
                Tuple<string, string> attributetuple = new Tuple<string, string>(attribute.LocalName, XDN.XPath);
                string attributeValue = attribute.Value;

                if (OptionalValuesPerParameterDict.ContainsKey(attributetuple))
                {
                    OptionalValuesPerParameterDict[attributetuple].Add(attributeValue);
                }
                else
                {
                    OptionalValuesPerParameterDict.Add(attributetuple, new List<string> { attributeValue });
                }
            }
        }

        /// <summary>
        /// Update all parameters optional values according to xml file
        /// </summary>
        /// <param name="AAM"></param>
        /// <param name="OptionalValuesPerParameterDict"></param>
        public void PopulateXMLOptionalValuesForAPIParameters(ApplicationModelBase AAM, Dictionary<Tuple<string, string>, List<string>> OptionalValuesPerParameterDict)
        {
            foreach (AppModelParameter AMP in AAM.AppModelParameters)
            {
                string result = Regex.Match(AMP.Path, @"(.)*soapenv:Body\[1\]\/([a-zA-Z]|\d)*:").Value;
                if (string.IsNullOrEmpty(result))
                {
                    result = Regex.Match(AMP.Path, @"(.)*soapenv:Body\[1\]\/").Value;
                }

                string VAXBXPath = string.Empty;
                if (!string.IsNullOrEmpty(result))
                { VAXBXPath = AMP.Path.Replace(result, "//*[name()='vaxb:VAXB']/vaxb:"); }

                Tuple<string, string> tuple = new Tuple<string, string>(AMP.TagName, AMP.Path);
                Tuple<string, string> relativePathTuple = new Tuple<string, string>(AMP.TagName, VAXBXPath);
                if (OptionalValuesPerParameterDict.ContainsKey(tuple))
                {
                    PopulateOptionalValuesByTuple(AMP, OptionalValuesPerParameterDict, tuple);
                }
                if (OptionalValuesPerParameterDict.ContainsKey(relativePathTuple))
                {
                    PopulateOptionalValuesByTuple(AMP, OptionalValuesPerParameterDict, relativePathTuple);
                }
            }
        }

        /// <summary>
        /// Update optional values only for selected parameters  according to xml file
        /// </summary>
        /// <param name="AAM"></param>
        /// <param name="OptionalValuesPerParameterDict"></param>
        /// <param name="SelectedParametersGridList"></param>
        public void PopulateXMLOptionalValuesForAPIParameters(ApplicationModelBase AAM, Dictionary<Tuple<string, string>, List<string>> OptionalValuesPerParameterDict, List<AppModelParameter> SelectedParametersGridList)
        {
            foreach (var tuple in SelectedParametersGridList.Zip(AAM.AppModelParameters, (x, y) => (x, y)))
            {
                if (tuple.x.RequiredAsInput)//selected
                {
                    string result = Regex.Match(tuple.y.Path, @"(.)*soapenv:Body\[1\]\/([a-zA-Z]|\d)*:").Value;
                    if (string.IsNullOrEmpty(result))
                    {
                        result = Regex.Match(tuple.y.Path, @"(.)*soapenv:Body\[1\]\/").Value;
                    }

                    string VAXBXPath = string.Empty;
                    if (!string.IsNullOrEmpty(result))
                    { VAXBXPath = tuple.y.Path.Replace(result, "//*[name()='vaxb:VAXB']/vaxb:"); }

                    Tuple<string, string> tupleKey = new Tuple<string, string>(tuple.y.TagName, tuple.y.Path);
                    Tuple<string, string> relativePathTuple = new Tuple<string, string>(tuple.y.TagName, VAXBXPath);
                    if (OptionalValuesPerParameterDict.ContainsKey(tupleKey))
                    {
                        PopulateOptionalValuesByTuple(tuple.y, OptionalValuesPerParameterDict, tupleKey);
                    }
                    if (OptionalValuesPerParameterDict.ContainsKey(relativePathTuple))
                    {
                        PopulateOptionalValuesByTuple(tuple.y, OptionalValuesPerParameterDict, relativePathTuple);
                    }
                }
            }
        }
        #endregion

        #region JSON
        public void GetJSONAllOptionalValuesFromExamplesFile(TemplateFile xMLTemplateFile, Dictionary<Tuple<string, string>, List<string>> optionalValuesPerParameterDict)
        {
            string FileContent = File.ReadAllText(xMLTemplateFile.FilePath);
            JsonExtended JE = new JsonExtended(FileContent);
            foreach (JsonExtended JTN in JE.GetEndingNodes())
            {
                try
                {
                    AddJSONValueToOptionalValuesPerParameterDict(optionalValuesPerParameterDict, JTN.GetToken());
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, ex.StackTrace);
                }
            }
        }

        public void GetJSONAllOptionalValuesFromExamplesFile(string fileContent, Dictionary<Tuple<string, string>, List<string>> optionalValuesPerParameterDict)
        {
            JsonExtended JE = new JsonExtended(fileContent);
            foreach (JsonExtended JTN in JE.GetEndingNodes())
            {
                try
                {
                    AddJSONValueToOptionalValuesPerParameterDict(optionalValuesPerParameterDict, JTN.GetToken());
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, ex.StackTrace);
                }
            }
        }

        private void AddJSONValueToOptionalValuesPerParameterDict(Dictionary<Tuple<string, string>, List<string>> optionalValuesPerParameterDict, JToken xDN)
        {
            Tuple<string, string> tuple = new Tuple<string, string>(xDN.Path.Split('.').LastOrDefault(), xDN.Path);
            string Value = "";
            try
            {
                Value = ((JProperty)xDN).Value.ToString();
            }
            catch
            {
                Value = xDN.ToString();
            }
            if (optionalValuesPerParameterDict.ContainsKey(tuple))
            {

                optionalValuesPerParameterDict[tuple].Add(Value);
            }
            else
            {
                optionalValuesPerParameterDict.Add(tuple, new List<string>() { Value });
            }
        }

        /// <summary>
        /// Update all parameters optional values according to json file
        /// </summary>
        /// <param name="AAM"></param>
        /// <param name="OptionalValuesPerParameterDict"></param>
        public void PopulateJSONOptionalValuesForAPIParameters(ApplicationModelBase AAM, Dictionary<Tuple<string, string>, List<string>> OptionalValuesPerParameterDict)
        {
            int UpdatedParametersCounter = 0;
            foreach (AppModelParameter AMP in AAM.AppModelParameters)
            {
                string result = AMP.Path;
                Tuple<string, string> tuple = new Tuple<string, string>(AMP.TagName, AMP.Path);
                if (OptionalValuesPerParameterDict.ContainsKey(tuple))
                {
                    PopulateOptionalValuesByTuple(AMP, OptionalValuesPerParameterDict, tuple);
                    if (APIConfigurationsDocumentParserBase.ParameterValuesUpdated)
                    {
                        UpdatedParametersCounter++;
                        APIConfigurationsDocumentParserBase.ParameterValuesUpdated = false;
                    }
                }
            }
        }

        /// <summary>
        /// Update optional values only for selected parameter according to json file
        /// </summary>
        /// <param name="AAM"></param>
        /// <param name="OptionalValuesPerParameterDict"></param>
        /// <param name="SelectedParametersGridList"></param>
        public void PopulateJSONOptionalValuesForAPIParameters(ApplicationModelBase AAM, Dictionary<Tuple<string, string>, List<string>> OptionalValuesPerParameterDict, List<AppModelParameter> SelectedParametersGridList)
        {
            int UpdatedParametersCounter = 0;
            foreach (var tuple in SelectedParametersGridList.Zip(AAM.AppModelParameters, (x, y) => (x, y)))
            {
                if (tuple.x.RequiredAsInput)//selected
                {
                    Tuple<string, string> tupleKey = new Tuple<string, string>(tuple.y.TagName, tuple.y.Path);
                    if (OptionalValuesPerParameterDict.ContainsKey(tupleKey))
                    {
                        PopulateOptionalValuesByTuple(tuple.y, OptionalValuesPerParameterDict, tupleKey);
                        if (APIConfigurationsDocumentParserBase.ParameterValuesUpdated)
                        {
                            UpdatedParametersCounter++;
                            APIConfigurationsDocumentParserBase.ParameterValuesUpdated = false;
                        }
                    }
                }
            }
        }
        #endregion

        private void PopulateOptionalValuesByTuple(AppModelParameter AMP, Dictionary<Tuple<string, string>, List<string>> OptionalValuesPerParameterDict, Tuple<string, string> tuple)
        {
            foreach (string Value in OptionalValuesPerParameterDict[tuple])
            {
                OptionalValue OptionalValueExist = AMP.OptionalValuesList.Where(x => x.Value == Value).FirstOrDefault();
                if (OptionalValueExist == null)
                {
                    OptionalValue OptionalValue = new OptionalValue() { Value = Value };
                    if (!string.IsNullOrEmpty(Value))
                    { OptionalValue.IsDefault = true; }

                    AMP.OptionalValuesList.Add(OptionalValue);
                }
            }
        }
    }
}
