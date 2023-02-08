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

using Amdocs.Ginger.Repository;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace GingerCore.Actions.XML
{
    public class XMLProcessor
    {
        //TODO: write Unit test

        public void ParseToReturnValues(string XML, Act act)
        {
            XmlDocument xmlReqDoc = new XmlDocument();
            xmlReqDoc.LoadXml(XML);            
            SetOutput(xmlReqDoc, act);
        }

        // Read the XML for each elem generate output with Path
        private void SetOutput(XmlDocument xmlDoc, Act act)
        {
            XmlReader xmlReader = XmlReader.Create(new System.IO.StringReader(xmlDoc.InnerXml));
            XmlReader subXmlReader = null;
            string xmlElement = "";
            
            ArrayList elementArrayList = new ArrayList();
            List<string> DeParams = new List<string>();
            foreach (ActReturnValue actReturnValue in act.ReturnValues)
            {
                //TODO: How the user will know? add check box?
                if (actReturnValue.Param.IndexOf("AllDescOf") == 0)
                {
                    DeParams.Add(actReturnValue.Param.Trim().Substring(9).Trim());
                }
            }

            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    xmlElement = xmlReader.Name;
                    if (elementArrayList.Count <= xmlReader.Depth)
                    {
                        elementArrayList.Add(xmlElement);
                    }
                    else
                    {
                        elementArrayList[xmlReader.Depth] = xmlElement;
                    }

                    foreach (var p in DeParams)
                    {
                        if (p == xmlReader.Name)
                        {
                            subXmlReader = xmlReader.ReadSubtree();
                            subXmlReader.ReadToFollowing(p);
                            act.AddOrUpdateReturnParamActualWithPath("AllDescOf" + p, subXmlReader.ReadInnerXml(), "/" + string.Join("/", elementArrayList.ToArray().Take(xmlReader.Depth)));
                        }
                    }
                }

                if (xmlReader.NodeType == XmlNodeType.Text)
                {
                    // soup req contains sub xml, so parse them 
                    if (xmlReader.Value.StartsWith("<?xml"))
                    {
                        XmlDocument xmlnDoc = new XmlDocument();
                        xmlnDoc.LoadXml(xmlReader.Value);
                        SetOutput(xmlnDoc, act);
                    }
                    else
                    {
                        act.AddOrUpdateReturnParamActualWithPath(xmlElement, xmlReader.Value, "/" + string.Join("/", elementArrayList.ToArray().Take(xmlReader.Depth)));
                    }
                }
            }
        }
    }
}
