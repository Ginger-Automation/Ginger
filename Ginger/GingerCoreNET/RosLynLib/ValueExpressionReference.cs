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

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Amdocs.Ginger.CoreNET.RosLynLib.Refrences
{
    public class ValueExpressionReference
    {

        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string Name { get; set; }
        public string Expression { get; set; }
        public string UseCase { get; set; }
        public List<string> Samples { get; set; }
        public string ReturnType { get; set; }
        public string ExpressionResult { get; set; }
        public string IconImageName { get; set; }
        public bool RequiresSpecificFlowDetails { get; set; }
        public bool RequiresRunsetOperation { get; set; }

        public ValueExpressionReference()
        {
            Samples = new List<string>();

        }
    }

    public class VEReferenceList
    {
        public List<ValueExpressionReference> Refrences = new List<ValueExpressionReference>();

        public static VEReferenceList LoadFromJson(string JsonFilePath)
        {
            return JsonConvert.DeserializeObject<VEReferenceList>(System.IO.File.ReadAllText(JsonFilePath));
        }
        public void SavetoJson(string JsonFilePath)
        {
            string output = JsonConvert.SerializeObject(this);
            System.IO.File.WriteAllText(JsonFilePath, output);
        }
    }
}
