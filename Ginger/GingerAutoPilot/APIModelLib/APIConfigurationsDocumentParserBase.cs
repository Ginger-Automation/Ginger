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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.APIModelLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Amdocs.Ginger.Repository
{
    public abstract class APIConfigurationsDocumentParserBase
    {
        public abstract ObservableList<ApplicationAPIModel> ParseDocument(string FileName, ObservableList<ApplicationAPIModel> AAMSList, bool avoidDuplicatesNodes = false);
        public static bool ParameterValuesUpdated { get; set; }

        public static ObservableList<ActReturnValue> ParseResponseSampleIntoReturnValuesPerFileType(string FilePath)
        {
            string fileContent = System.IO.File.ReadAllText(FilePath);
            if (IsValidJson(fileContent))
            {
                return JSONTemplateParser.ParseJSONResponseSampleIntoReturnValues(fileContent); 
            }
            else if (IsValidXML(fileContent))
            {
                return XMLTemplateParser.ParseXMLResponseSampleIntoReturnValues(fileContent);
            }
            return null;
        }


        public void PopulateOptionalValuesByTuple(AppModelParameter AMP, Dictionary<Tuple<string, string>, List<string>> OptionalValuesPerParameterDict, Tuple<string, string> tuple)
        {
            foreach (string Value in OptionalValuesPerParameterDict[tuple])
            {
                OptionalValue OptionalValueExist = AMP.OptionalValuesList.Where(x => x.Value == Value).FirstOrDefault();
                if (OptionalValueExist == null)
                {
                    OptionalValue OptionalValue = new OptionalValue() { Value = Value };
                    if (!string.IsNullOrEmpty(Value))
                        OptionalValue.IsDefault = true; 

                    AMP.OptionalValuesList.Add(OptionalValue);
                    ParameterValuesUpdated = true;
                }
            }
        }

        public static string ParserTypeByContent(string FilePath)
        {
            string file = System.IO.File.ReadAllText(FilePath);
            if (IsValidJson(file))
            {
                return ".json";
            }
            else if (IsValidXML(file))
            {
                return ".xml";
            }
            return string.Empty;
        }

        public static bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = Newtonsoft.Json.Linq.JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException)
                {
                    return false;
                }
                catch (Exception) 
                {
                    return false;
                }
            }
            return false;
        }
        public static bool IsValidXML(string strInput)
        {
            try
            {
                XmlDocument XmlDocument = new XmlDocument();
                XmlDocument.LoadXml(strInput);
                return true;
            }
            catch (XmlException)
            {
                return false;
            }
            catch(Exception)
            {
                return false;
            }            
        }
    }
}
