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

using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Repository;
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
            return GetParameters(jsOnText, AAMSList, avoidDuplicatesNodes, fileName);
        }

        public ObservableList<ApplicationAPIModel> ParseDocumentWithJsonContent(string fileContent, ObservableList<ApplicationAPIModel> AAMSList, bool avoidDuplicatesNodes = false)
        {
            return GetParameters(fileContent, AAMSList, avoidDuplicatesNodes, string.Empty);
        }

        private static ObservableList<ApplicationAPIModel> GetParameters(string jsonText, ObservableList<ApplicationAPIModel> AAMSList, bool avoidDuplicatesNodes, string fileName)
        {
            ApplicationAPIModel AAM = new ApplicationAPIModel();
            AAM.Name = Path.GetFileNameWithoutExtension(fileName);

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

                ReturnValues.Add(new ActReturnValue() { Param = tagName, Path = Jn.Path, Active = true, DoNotConsiderAsTemp=true });
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

            IEnumerable<JsonExtended> EndingNodesList = JE.GetEndingNodes();
            foreach (var Jn in EndingNodesList)
            {
                if (Jn == null)
                    continue;
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
                JToken jt2 = jt.SelectToken(Jn.Path);
                try
                {
                    if (jt2.Type != JTokenType.String&& jt2.Type != JTokenType.Array)
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

                catch(Exception ex)
                {
                    Console.WriteLine("GenerateBodyANdModelParameters error - " + ex.Message);

                    // Why do we do work in exception !!!!!!!!!!!!!
                    if (jt2.Type != JTokenType.String && jt2.Type != JTokenType.Array)
                    {
                        consts.Add(param);
                    }
                    jt2.Replace( param);
                }
                AppModelParameters.Add(new AppModelParameter(param, "", tagName, Jn.Path, new ObservableList<OptionalValue>()));

            }
       string body= jt.ToString();
            foreach (var item in consts)
            {
                body = body.Replace("\"" + item + "\"", item);
            }
            BodyParamArray[0] = body;
            BodyParamArray[1] = AppModelParameters;
            return BodyParamArray;
        }
    }
}
