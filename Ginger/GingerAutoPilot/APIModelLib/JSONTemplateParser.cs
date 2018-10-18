#region License
/*
Copyright © 2014-2018 European Support Limited

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
        public override ObservableList<ApplicationAPIModel> ParseDocument(string FileName, bool avoidDuplicatesNodes = false)
        {
            ObservableList<ApplicationAPIModel> AAMSList = new ObservableList<ApplicationAPIModel>();

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
            foreach (var Jn in JE.GetEndingNodes())
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
                    ((JValue)jt2).Value = param;
                
                }

                catch(Exception)
                {
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
