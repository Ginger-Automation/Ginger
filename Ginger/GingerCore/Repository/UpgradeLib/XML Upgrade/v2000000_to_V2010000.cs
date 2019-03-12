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

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace GingerCore.XMLConverters
{
    class v2000000_to_V2010000 : XMLConverterBase
    {
        public override void Convert()
        {
            string outputxml = "";
            UpdateXMLVersion("2.1.0.0");
            switch (RepoType)
            {
                #region case GingerFileType.BusinessFlow:
                case eGingerFileType.BusinessFlow:
                    // ### Set all Activity's to active ###
                    IEnumerable<XElement> qActivities = (from el in xmlDoc.Descendants("GingerCore.Activity")
                                                         select el);

                    foreach (XElement el in qActivities)
                    {
                        var qActConsoleCommandFunctions = (from e in el.Descendants("GingerCore.Actions.ActConsoleCommand") select e);
                        foreach (XElement el1 in qActConsoleCommandFunctions)
                        {
                            if (el1.Attribute("ConsoleCommand").Value == "ParametrizedCommand")
                            {
                                var qActInputValue = (from e in el1.Descendants("GingerCore.Actions.ActInputValue") select e);


                                foreach (XElement el2 in qActInputValue)
                                {
                                    if (el2.Attribute("Param").Value == "Free Command")
                                    {
                                        el2.Attribute("Param").Value = "Value";
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
            #endregion
            outputxml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + xmlDoc.ToString();
            UpdatedXML = outputxml;
        }
    }
}
