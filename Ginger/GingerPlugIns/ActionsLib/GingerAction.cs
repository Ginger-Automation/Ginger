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

using System.Collections.Generic;
using System.Linq;

namespace GingerPlugIns.ActionsLib
{
    public class GingerAction
    {
        public string ID { get; set; }

        public List<ActionParam> ParamsIn = new List<ActionParam>();

        public List<ActionOutput> ParamsOut = new List<ActionOutput>();

        public string ExInfo { get; set; }

        public string Error { get; set; }

        public string SolutionFolder { get; set; }

        public ActionParam GetOrCreateParam(string Name, string defualtValue="")
        {
            ActionParam AP = (from x in ParamsIn where x.Name == Name select x).FirstOrDefault();

            if (AP == null)
            {
                AP = new ActionParam() { Name = Name };
                if (string.IsNullOrEmpty(defualtValue) == false)
                    AP.Value = defualtValue;
                ParamsIn.Add(AP);
            }

            return AP;            
        }

        public void AddOutput(string key, string value, string path = null)
        {
            ParamsOut.Add(new ActionOutput() { Param = key, Value = value, Path = path });
        }
    }
}
