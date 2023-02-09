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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amdocs.Ginger.Common.GeneralLib
{
   public class JsonExtended
    {

        private JToken JT;
        JsonExtended ParentNode;

        #region Properties
        public string JsonString
        {
            get
            {
               return JT.ToString();
            }
        }
        public string Path
        {
            get
            {
                return JT.Path;
            }
        }
        public string Name
        {
            get
            {
                return JT.Path.Split('.').Last();
            }
        }
        public string XPathWithoutNamspaces;
      /*  {
            get
            {
                string tempPath = "";
                string[] parts = XPath.Split('/');

                string part1 = parts[2];

                if (part1.StartsWith(@"*[name()='"))
                {
                    string a = part1.Replace(@"//*[name()='", "");
                    string b = a.Replace("']", "");
                    part1 = "//*[local-name()='" + b.Split(':').ElementAt(1) + "']";


                }

                for (int i = 3; i < parts.Length; i++)
                {

                    string[] currentParts = parts[i].Split(':');
                    if (currentParts.Length > 1)
                    {
                        tempPath += "/" + currentParts[1];
                    }
                    else
                    {
                        tempPath += "/" + parts[i];
                    }

                }

                return part1 + tempPath;
            }*/



    

  
        #endregion

        #region Constructors
        private JsonExtended(JsonExtended Parent, JToken Node)
        {
            ParentNode = Parent;
            
            JT = Node;

            JEnumerable<JToken> childList = JT.Children();

            BuildThisNode(childList);

        }
        public JsonExtended(string json)
        {
            JT = JToken.Parse(json);
            ParentNode = null;
            //   XPath = Node.Name;
            JEnumerable<JToken> childList = JT.Children();
            BuildThisNode(childList);
        }

 
        #endregion

        #region PublicFunctions
 
   
        public List<JsonExtended> GetChildNodes()
        {
            return ChildNodes;
        }





      

        public JToken GetToken()
        {
            return JT;
        }

        /// <summary>
        /// Selects a Newtonsoft.Json.Linq.JToken using a JPath expression. Selects the token that matches the object path.
        /// </summary>
        /// <param name="path">A System.String that contains a JPath expression.</param>
        /// <returns>A Newtonsoft.Json.Linq.JToken, or null.</returns>
        public JToken SelectToken(string path)
        {
            return JT.SelectToken(path);
        }


        public List<JsonExtended> GetAllNodes(List<JsonExtended> XDL = null)
        {

            if (XDL == null)
            {
                XDL = new List<JsonExtended>();
            }


            List < JsonExtended > ChildNodeList = this.GetChildNodes();

            foreach (JsonExtended XDN in ChildNodeList)
            {
                XDN.GetAllNodes(XDL);
                int contOfChiledWithSameTokenAsParent = XDL.Where(x => x.GetToken().Parent == XDN.GetToken()).Count();
                if (contOfChiledWithSameTokenAsParent == 0)
                    XDL.Add(XDN);
                
            }

            return XDL;
        }

        public IEnumerable<JsonExtended> GetEndingNodes(bool IncludeSelfClosingTags = true)
        {
            List<JsonExtended> mEndingnodes = new List<JsonExtended>();

            var Jpropertytype = typeof(JProperty);
            //  mEndingnodes=   this.GetAllNodes().Where(x=> x.GetToken().GetType()== Jpropertytype & (x.GetToken().Children().Count()>0)).ToList();
            /* if (IncludeSelfClosingTags)
             {
                 mEndingnodes = this.GetAllNodes().Where(x => x.ChildNodes.Count == 0 && x.GetXmlNode().NodeType != XmlNodeType.EndElement && x.GetXmlNode().NodeType != XmlNodeType.Comment).ToList();
             }
             else
             {
                 mEndingnodes = this.GetAllNodes().Where(x => x.ChildNodes.Count == 0 && !x.XMLString.EndsWith("/>") && x.GetXmlNode().NodeType != XmlNodeType.EndElement && x.GetXmlNode().NodeType != XmlNodeType.Comment).ToList();

             }*/

            List<JsonExtended> allNodesList = this.GetAllNodes();

            foreach (var nodes in allNodesList)
            {
                if (nodes.GetToken().GetType() == Jpropertytype)
                {

                    mEndingnodes.Add(nodes);
                }
            }
            return allNodesList.Where(x => x.GetToken().Children().Count() == 0);
        }


        #endregion

        #region Private
  

        private List<JsonExtended> ChildNodes = new List<JsonExtended>();

       

 



        private void BuildThisNode(JEnumerable<JToken> JTl)
        {
            foreach (JToken XN in JTl)
            {
              JsonExtended childJsonExtended = new JsonExtended(this, XN);
              ChildNodes.Add(childJsonExtended);
            }
        }

        public void RemoveDuplicatesNodes()
        {
            List<JsonExtended> childNodesList = this.GetChildNodes();
            if (childNodesList.Count() == 0)
                return;

            if (childNodesList.Count() > 1)
            {
                for (int i = 0; i < childNodesList.Count(); i++)
                {
                    List<JsonExtended> duplicateChildren;
                    duplicateChildren = childNodesList.Where(x => x.GetToken().First.Path.Split('.').LastOrDefault() == childNodesList[i].GetToken().First.Path.Split('.').LastOrDefault() && x.GetToken().First.Path != childNodesList[i].GetToken().First.Path).ToList();
                    foreach (JsonExtended child in duplicateChildren)
                    {
                        ChildNodes.Remove(child);
                        child.JT.Remove();
                    }
                }
            }

            foreach (JsonExtended child in childNodesList)
                child.RemoveDuplicatesNodes();
        }

        public List<JToken> FindTokens(string name)
        {
            List<JToken> matches = new List<JToken>();
            FindTokens(JT, name, matches);
            return matches;
        }

        private void FindTokens(JToken containerToken, string name, List<JToken> matches)
        {
            if (containerToken.Type == JTokenType.Object)
            {
                foreach (JProperty child in containerToken.Children<JProperty>())
                {
                    if (child.Value.ToString() == name)
                    {
                        matches.Add(child.Value);
                    }
                    FindTokens(child.Value, name, matches);
                }
            }
            else if (containerToken.Type == JTokenType.Array)
            {
                foreach (JToken child in containerToken.Children())
                {
                    FindTokens(child, name, matches);
                }
            }
        }

        #endregion

    }
}
