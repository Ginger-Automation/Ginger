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
using System.Collections.Generic;
using System.Xml.Linq;

namespace Amdocs.Ginger.CoreNET.ALMLib.DataContract
{
    public class ALMTestCase
    {
        public string Id { get; set; }
        public string TestId { get; set; }
        public string TestSetId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string PlanName { set; get; }

        public Dictionary<string, object> ElementsField { set; get; }

        public ALMTestCase()
        {
            ElementsField = [];
        }

        public ALMTestCase FromXML(IEnumerable<XElement> fields)
        {
            foreach (var field in fields)
            {
                string name = field.Attribute("Name").Value;
                if (name.Equals("id"))
                {
                    this.Id = field.Element("Value").Value;
                }
                else if (name.Equals("test-id"))
                {
                    this.TestId = field.Element("Value").Value;
                }
                else if (name.Equals("cycle-id"))
                {
                    this.TestSetId = field.Element("Value").Value;
                }
                else if (name.Equals("status"))
                {
                    this.Status = field.Element("Value").Value;
                }
                else if (name.Equals("name"))
                {
                    this.Name = field.Element("Value").Value;
                }
                else
                {
                    var curName = field.Attribute("Name").Value;
                    var curValue = field.Element("Value");

                    if (curValue == null)
                    {
                        continue;
                    }
                    var value = curValue.Value;
                    this.ElementsField.Add(curName, value);
                }
            }
            return this;
        }
    }

    public class QCTestCaseColl : List<ALMTestCase> { }
}
