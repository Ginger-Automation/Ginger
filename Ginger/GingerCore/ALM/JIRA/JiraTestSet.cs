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
namespace GingerCore.ALM.JIRA
{
    public class JiraTestSet
    {
        public static partial class Fields
        {
            public static string Seq = "Seq";
            public static string Name = "Name";
            public static string JiraID = "Key";
            public static string CreatedBy = "CreatedBy";
            public static string CreationDate = "DateCreated";
            public static string URLPath = "URLPath";
        }
        public JiraTestSet()
        {
            this.Tests = new List<JiraTest>();
        }
        public int Seq { get; set; }
        public string Name { get; set; }
        public string URLPath { get; set; }
        public string ID { get; set; }
        public string Version { get; set; }
        public string Project { get; set; }
        public string CreatedBy { get; set; }
        public string DateCreated { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
        public List<JiraTest> Tests { get; set; }

    }
}
