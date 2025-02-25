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

using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Repository;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Amdocs.Ginger.Common.APIModelLib
{
    public class JSONTemplateParser : APIConfigurationsDocumentParserBase
    {
        public override ObservableList<ApplicationAPIModel> ParseDocument(string FileName, ObservableList<ApplicationAPIModel> AAMSList, bool avoidDuplicatesNodes = false)
        {
            string jsOnText = System.IO.File.ReadAllText(FileName);
            string fileName = Path.GetFileNameWithoutExtension(FileName);
            ObservableList<ApplicationAPIModel> parameters = GetParameters(jsOnText, AAMSList, avoidDuplicatesNodes, fileName);
            return parameters;
        }

        public ObservableList<ApplicationAPIModel> ParseDocumentWithJsonContent(string fileContent, ObservableList<ApplicationAPIModel> AAMSList, bool avoidDuplicatesNodes = false)
        {
            return GetParameters(fileContent, AAMSList, avoidDuplicatesNodes, string.Empty);
        }

        private static ObservableList<ApplicationAPIModel> GetParameters(string jsonText, ObservableList<ApplicationAPIModel> AAMSList, bool avoidDuplicatesNodes, string fileName)
        {
            ApplicationAPIModel AAM = new ApplicationAPIModel
            {
                Name = Path.GetFileNameWithoutExtension(fileName)
            };

            //JObject jo = JObject.Parse(JSOnText);
            //IList<string> keys = jo.Properties().Select(p => p.Path).ToList();

            if (avoidDuplicatesNodes)
            {
                JsonExtended fullJSOnObjectExtended = new JsonExtended(jsonText);
                fullJSOnObjectExtended.RemoveDuplicatesNodes();
                jsonText = fullJSOnObjectExtended.JsonString;
            }

            object[] BodyandModelParameters = GenerateBodyANdModelParameters(jsonText);

            AAM.RequestBody = (string)BodyandModelParameters[0];
            AAM.AppModelParameters = (ObservableList<AppModelParameter>)BodyandModelParameters[1];
            AAMSList.Add(AAM);

            return AAMSList;
        }

        public static ObservableList<ActReturnValue> ParseJSONResponseSampleIntoReturnValues(string JSOnText)
        {
            ObservableList<ActReturnValue> ReturnValues = new ObservableList<ActReturnValue>();

            JToken jt = JToken.Parse(JSOnText);

            JsonExtended JE = new JsonExtended(JSOnText);
            foreach (var Jn in JE.GetEndingNodes())
            {
                string tagName = Jn.Path.Split('.').LastOrDefault();

                ReturnValues.Add(new ActReturnValue() { Param = tagName, Path = Jn.Path, Active = true, DoNotConsiderAsTemp = true });
            }

            return ReturnValues;
        }

        public static object[] GenerateBodyANdModelParameters(string JSOnText)
        {
            object[] BodyParamArray = new object[2];

            JsonExtended JE = new JsonExtended(JSOnText);

            ObservableList<AppModelParameter> AppModelParameters = new ObservableList<AppModelParameter>();

            JToken jt = JToken.Parse(JSOnText);
            Dictionary<string, string> ParamPath = new Dictionary<string, string>();
            List<string> consts = new List<string>();


            // till here value comes as false but after JE.GetEndingNodes(), it gets converted to False. made some changes to GetEndingNodes also but did not work.
            IEnumerable<JsonExtended> EndingNodesList = JE.GetEndingNodes();
            foreach (var Jn in EndingNodesList)
            {
                if (Jn == null)
                {
                    continue;
                }
                JToken jt2 = jt.SelectToken(Jn.Path);

                // here jt2 value is False, trying to convert it to false
                if (jt2.Type == JTokenType.Boolean)
                {
                    bool boolValue = jt2.Value<bool>(); // Get actual boolean
                    jt2.Replace(new JValue(boolValue));
                    jt2 = JToken.FromObject(boolValue); // Recreate token correctly
                }

                if (IsValidJson(Jn.JsonString))
                {
                    JsonExtended je4Jn = new JsonExtended(Jn.JsonString);
                    IEnumerable<JsonExtended> EEEL = je4Jn.GetEndingNodes();

                    if (EEEL.FirstOrDefault() != null)
                    {
                        object[] BodyParamArrayTemp = GenerateBodyANdModelParameters(Jn.JsonString);

                        foreach (var item in (ObservableList<AppModelParameter>)BodyParamArrayTemp[1])
                        {
                            AppModelParameters.Add(item);
                        }

                        ((JValue)jt2).Value = (string)BodyParamArrayTemp[0];
                        continue;
                    }
                }

                string tagName = Jn.Path.Split('.').LastOrDefault();
                string paramname = tagName.ToUpper();
                int i = 0;
                string param = "<" + paramname + ">";
                while (ParamPath.ContainsKey(param))
                {
                    i++;
                    param = "<" + paramname + i + ">";
                }

                ParamPath.Add(param, Jn.Path);

                try
                {
                    if (jt2.Type != JTokenType.String && jt2.Type != JTokenType.Object && jt2.Type != JTokenType.Array)
                    {
                        consts.Add(param);
                    }
                    if (jt2.Type != JTokenType.Array && param == "<ID2>")
                    {
                        consts.Add(param);
                    }
                    //handling empty aaray like 'NewCar':[ ]
                    if (jt2.Type == JTokenType.Array && jt2.Children().Count() == 0)
                    {
                        string jsonarraystring = "[ ]";
                        jt2 = JArray.Parse(jsonarraystring);
                    }
                    else
                    {
                        ((JValue)jt2).Value = param;

                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine("GenerateBodyANdModelParameters error - " + ex.Message);

                    // Why do we do work in exception !!!!!!!!!!!!!
                    if (jt2.Type != JTokenType.String && jt2.Type != JTokenType.Array && jt2.Type != JTokenType.Object)
                    {
                        consts.Add(param);
                    }
                    jt2.Replace(param);
                }
                try
                {
                    JToken jt2token = jt.SelectToken(Jn.Path);
                    if (jt2token.Type == JTokenType.Boolean)
                    {
                        bool boolValue = jt2token.Value<bool>();
                        AppModelParameters.Add(new AppModelParameter(param, "", tagName, Jn.Path, new ObservableList<OptionalValue> { new OptionalValue() { Value = !string.IsNullOrEmpty(Jn.JsonString) ? Jn.JsonString.ToLower(): "false", IsDefault = true } }));
                    }
                    else
                    {
                        AppModelParameters.Add(new AppModelParameter(param, "", tagName, Jn.Path, new ObservableList<OptionalValue> { new OptionalValue() { Value = Jn.JsonString.Replace("\"", ""), IsDefault = true } }));
                    }
                }
                catch (Exception ex)
                {
                    AppModelParameters.Add(new AppModelParameter(param, "", tagName, Jn.Path, new ObservableList<OptionalValue> { new OptionalValue() { Value = Jn.JsonString.Replace("\"", ""), IsDefault = true } }));
                }
            }
            string body = jt.ToString();
            foreach (var item in consts)
            {
                body = body.Replace("\"" + item + "\"", item);
            }
            BodyParamArray[0] = body;
            BodyParamArray[1] = AppModelParameters;
            return BodyParamArray;
        }

        private static bool IsValidJson(string json)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(json) || !json.Contains('{'))
                {
                    return false;
                }

                JToken.Parse(json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void SetOptionalValuesList(IEnumerable<JsonExtended> EndingNodesOFEN, ObservableList<AppModelParameter> AppModelParameters)
        {
            foreach (var optionalValueItem in EndingNodesOFEN)
            {
                if (optionalValueItem == null)
                {
                    continue;
                }
                else
                {

                    string insidetagName = optionalValueItem.Path.Split('.').LastOrDefault();
                    string insideparamname = insidetagName.ToUpper();

                    insideparamname = "<" + insideparamname + ">";


                    AppModelParameters.Add(new AppModelParameter(insideparamname, "", insidetagName, optionalValueItem.Path, new ObservableList<OptionalValue> { new OptionalValue() { Value = optionalValueItem.JsonString } }));

                }
            }
        }
    }
}
