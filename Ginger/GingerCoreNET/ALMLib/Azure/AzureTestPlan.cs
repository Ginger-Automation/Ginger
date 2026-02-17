#region License
/*
Copyright © 2014-2026 European Support Limited
 
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

namespace Amdocs.Ginger.CoreNET.ALMLib.Azure
{
    public class AzureTestPlan
    {
        public static partial class Fields
        {
            public static readonly string Name = "Name";
            public static readonly string AzureID = "AzureID";
            public static readonly string State = "State";
            public static readonly string Project = "Project";
        }
        public AzureTestPlan()
        {
            this.TestCases = [];
        }
        public string Name { get; set; }
        public string AzureID { get; set; }
        public string Project { get; set; }
        public string State { get; set; }
        public string TestLink { get; set; }
        public string Description { get; set; }
        public List<AzureTestCases> TestCases { get; set; }
    }
}
