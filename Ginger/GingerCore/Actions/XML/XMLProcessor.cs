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

using Amdocs.Ginger.Repository;
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
            XmlReader rdr1 = XmlReader.Create(new System.IO.StringReader(xmlDoc.InnerXml));
            XmlReader rdr = XmlReader.Create(new System.IO.StringReader(xmlDoc.InnerXml));
            XmlReader subrdr = null;
            string Elm = "";
            
            ArrayList ls = new ArrayList();
            List<string> DeParams = new List<string>();
            foreach (ActReturnValue r in act.ReturnValues)
            {
                //TODO: How the user will know? add check box?
                if (r.Param.IndexOf("AllDescOf") == 0)
                    DeParams.Add(r.Param.Trim().Substring(9).Trim());
            }

            while (rdr.Read())
            {
                if (rdr.NodeType == XmlNodeType.Element)
                {
                    Elm = rdr.Name;
                    if (ls.Count <= rdr.Depth)
                        ls.Add(Elm);
                    else
                        ls[rdr.Depth] = Elm;
                    foreach (var p in DeParams)
                    {
                        if (p == rdr.Name)
                        {
                            subrdr = rdr.ReadSubtree();
                            subrdr.ReadToFollowing(p);
                            act.AddOrUpdateReturnParamActualWithPath("AllDescOf" + p, subrdr.ReadInnerXml(), "/" + string.Join("/", ls.ToArray().Take(rdr.Depth)));
                            subrdr = null;
                        }
                    }
                }

                if (rdr.NodeType == XmlNodeType.Text)
                {
                    // soup req contains sub xml, so parse them 
                    if (rdr.Value.StartsWith("<?xml"))
                    {
                        XmlDocument xmlnDoc = new XmlDocument();
                        xmlnDoc.LoadXml(rdr.Value);
                        SetOutput(xmlnDoc, act);
                    }
                    else
                    {

                        act.AddOrUpdateReturnParamActualWithPath(Elm, rdr.Value, "/" + string.Join("/", ls.ToArray().Take(rdr.Depth)));
                    }
                }
            }
        }
    }
}
