#region License
/*
Copyright © 2014-2019 European Support Limited

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
            ApplicationAPIModel AAM = new ApplicationAPIModel();
            AAM.Name = Path.GetFileNameWithoutExtension(FileName);

            string JSOnText = System.IO.File.ReadAllText(FileName);
            //JObject jo = JObject.Parse(JSOnText);
            //IList<string> keys = jo.Properties().Select(p => p.Path).ToList();

            if (avoidDuplicatesNodes)
            {
                JsonExtended fullJSOnObjectExtended = new JsonExtended(JSOnText);
                fullJSOnObjectExtended.RemoveDuplicatesNodes();
                JSOnText = fullJSOnObjectExtended.JsonString;
            }

            object[] BodyandModelParameters = GenerateBodyANdModelParameters(JSOnText);

            AAM.RequestBody = (string)BodyandModelParameters[0];
            AAM.AppModelParameters = (ObservableList<AppModelParameter>)BodyandModelParameters[1];
            AAMSList.Add(AAM);

            return AAMSList;
        }

        /// <summary>
        /// This method will return the AppmodelParameters from Json string
        /// </summary>
        /// <param name="JsonText"></param>
        /// <param name="avoidDuplicatesNodes"></param>
        /// <returns></returns>
        public ObservableList<AppModelParameter> GetAppModelParametersFromJson(string JsonText)
        {
            ObservableList<AppModelParameter> appList = new ObservableList<AppModelParameter>();
            dynamic JE = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonText);
            RecursivelyGetAllAppParameters(JE, ref appList);

            return appList;
        }

        /// <summary>
        /// This method is used to get app parameters recursively
        /// </summary>
        /// <param name="JE"></param>
        private void RecursivelyGetAllAppParameters(dynamic JE, ref ObservableList<AppModelParameter> appList)
        {
            foreach (var item in JE)
            {
                if (Convert.ToString(item.Value).StartsWith("{"))
                {
                    RecursivelyGetAllAppParameters(item.Value, ref appList);
                }
                else
                {
                    string paramName = string.Empty;
                    ObservableList<OptionalValue> vals = new ObservableList<OptionalValue>();
                    int count = 0;
                    var property = item.GetType().GetProperty("Name");
                    if (property != null)
                    {
                        count = GetAppParametersCountInList(appList, item.Name.ToUpper());
                        paramName = string.Format("<{0}[{1}]>", item.Name.ToUpper(), count);
                    }
                    else
                    {
                        count = GetAppParametersCountInList(appList, item.Key.ToUpper());
                        paramName = string.Format("<{0}[{1}]>", item.Key.ToUpper(), count);
                    }

                    if (Convert.ToString(item.Value).StartsWith("["))
                    {
                        dynamic child = item.Value;
                        foreach (var val in child)
                        {
                            OptionalValue optionalValue = new OptionalValue() { Value = Convert.ToString(val.Value) };
                            vals.Add(optionalValue);
                        }
                    }
                    else
                    {
                        if (property != null)
                        {
                            OptionalValue optionalValue = new OptionalValue() { Value = Convert.ToString(((Newtonsoft.Json.Linq.JValue)item.Value).Value) };
                            vals.Add(optionalValue);
                        }
                        else
                        {
                            OptionalValue optionalValue = new OptionalValue() { Value = Convert.ToString(item.Value) };
                            vals.Add(optionalValue);
                        }
                    }
                    appList.Add(new AppModelParameter() { ItemName = paramName, OptionalValuesList = vals });
                }
            }
        }

        /// <summary>
        /// This method will get the parameter count from the list of parameters
        /// </summary>
        /// <param name="appList"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        private int GetAppParametersCountInList(ObservableList<AppModelParameter> appList, string paramName)
        {
            int count = 0;
            foreach (var appModel in appList)
            {
                if(appModel.ItemName.Contains(paramName))
                {
                    count++;
                }
            }
            return count;
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
