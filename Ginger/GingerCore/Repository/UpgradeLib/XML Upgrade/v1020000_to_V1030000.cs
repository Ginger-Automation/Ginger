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
    class v1020000_to_V1030000 : XMLConverterBase
    {
        /// <summary>
        /// Update v2 files as necessary 
        /// </summary>
        public override void Convert()
        {
            string outputxml = "";
            UpdateXMLVersion("1.3.0.0");
            #region parse using Linq
            switch (RepoType)
            {
                #region case GingerFileType.BusinessFlow:
                case eGingerFileType.BusinessFlow:
                    // ### Set all Activity's to active ###
                    IEnumerable<XElement> qInactiveActivities = (from el in xmlDoc.Descendants("GingerCore.Activity") 
                                                           select el);
                    foreach (XElement el in qInactiveActivities) 
                    {
                        if(el.Attribute("Active") == null)
                        {
                            el.Add(new XAttribute("Active", "True"));
                        }
                        else 
                        {
                            if (el.Attribute("Active").Value == "False")
                            {
                                el.Attribute("Active").Value = "True";
                            }
                        }                        
                    }
                    break;
                #endregion

                #region case GingerFileType.Activity
                case eGingerFileType.Activity:
                    var qActivity = (from e in xmlDoc.Descendants("GingerCore.Activity")  select e);
                    XElement el_Activity = (XElement)qActivity.First();
                    if (el_Activity.Attribute("Active") == null)
                    {
                        el_Activity.Add(new XAttribute("Active", "True"));
                    }
                    else
                    {
                        if (el_Activity.Attribute("Active").Value == "False")
                        {
                            el_Activity.Attribute("Active").Value = "True";
                        }
                    }    
                    break;
                #endregion
            }
            #endregion

            outputxml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + xmlDoc.ToString();
            UpdatedXML = outputxml;
        }
    }
}
