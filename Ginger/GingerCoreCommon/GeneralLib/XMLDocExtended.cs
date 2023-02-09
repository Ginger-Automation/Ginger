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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Amdocs.Ginger.Common
{
    public class XMLDocExtended
    {

        private XmlNode XN;
        XMLDocExtended ParentNode;

        #region Properties
        public string XMLString
        {
            get
            {
                return PrettyXml(XN.OuterXml);
            }
        }
        public string XPath
        {
            get; set;
        }
        public string XPathWithoutNamspaces
        {
            get
            {
                string tempPath = "";
                string[] parts = XPath.Split('/');

                    string part1 = parts[2];

             if(part1.StartsWith(@"*[name()='"))
                {
                    string a= part1.Replace(@"//*[name()='", "");
                    string b = a.Replace("']","");
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

                return part1+ tempPath;
            }
        }

        public XmlAttributeCollection Attributes
        {

            get
            {
                return XN.Attributes;

            }

        }

        public string Name
        {

            get
            {
                return XN.Name.ToString().Replace("#", "");
            }

        }

        public string LocalName
        {

            get
            {
                return XN.LocalName;
            }

        }


        public string Value
        {

            get
            {
                return XN.InnerText;
            }

            set
            {
                XN.InnerText = value;
            }

        }
        #endregion

        #region Constructors
        private XMLDocExtended(XMLDocExtended Parent, XmlNode Node)
        {
            ParentNode = Parent;

            XN = Node;
            XPath = SetXpath();
            BuildThisNode(XN.ChildNodes);

        }
        public XMLDocExtended(XmlDocument XD)
        {
            ParentNode = null;
            //   XPath = Node.Name;
            XN = XD.DocumentElement;
            XPath = SetXpath();

            BuildThisNode(XN.ChildNodes);
        }
        #endregion

        #region PublicFunctions
        public static XmlNode GetNodeByXPath(string xml, string xpath)
        {
            XmlDocument XD = new XmlDocument();
            XD.LoadXml(xml);

            return GetNodeByXpath(XD, xpath);
        }
        public static XmlNode GetNodeByXpath(XmlDocument XD, string xpath)
        {
            XmlNamespaceManager ns = GetAllNamespaces(XD);
            XmlNode root = XD.DocumentElement;
            XmlNode node = root.SelectSingleNode(xpath, ns);

            return node;
        }

        public static XmlNamespaceManager GetAllNamespaces(XmlDocument xDoc)
        {
            XmlNamespaceManager result = new XmlNamespaceManager(xDoc.NameTable);

            IDictionary<string, string> localNamespaces = null;
            XPathNavigator xNav = xDoc.CreateNavigator();
            while (xNav.MoveToFollowing(XPathNodeType.Element))
            {
                localNamespaces = xNav.GetNamespacesInScope(XmlNamespaceScope.Local);
                foreach (var localNamespace in localNamespaces)
                {
                    string prefix = localNamespace.Key;
                    if (string.IsNullOrEmpty(prefix))
                    {
                        prefix = "DEFAULT";
                    }

                    result.AddNamespace(prefix, localNamespace.Value);
                }
            }

            return result;
        }

        public List<XMLDocExtended> GetChildNodes()
        {
            return ChildNodes;
        }





        public static string PrettyXml(string xml,bool newLineOnAttribue = false )
        {
            try
            {
                var stringBuilder = new StringBuilder();

                XmlDocument element = new XmlDocument();
                element.LoadXml(xml);

                var settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;
                settings.Indent = true;
                settings.NewLineOnAttributes = newLineOnAttribue;

                using (var xmlWriter = XmlWriter.Create(stringBuilder, settings))
                {
                    element.Save(xmlWriter);
                }

                return stringBuilder.ToString();
            }

            catch(Exception ee)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Failed to Prettify XML", ee);
                return xml;
            }
        }

        public XmlNode GetXmlNode()
        {
            return XN;
        }


        public List<XMLDocExtended> GetAllNodes(List<XMLDocExtended> XDL = null)
        {

            if (XDL == null)
            {
                XDL = new List<XMLDocExtended>();
            }


            foreach (XMLDocExtended XDN in this.GetChildNodes())
            {
                XDN.GetAllNodes(XDL);
                XDL.Add(XDN);
            }

            return XDL;
        }

        public IEnumerable<XMLDocExtended> GetEndingNodes(bool IncludeSelfClosingTags = true)
        {
            IEnumerable<XMLDocExtended> mEndingnodes;
            if (IncludeSelfClosingTags)
            {
                mEndingnodes = this.GetAllNodes().Where(x => x.ChildNodes.Count == 0 && x.GetXmlNode().NodeType != XmlNodeType.EndElement && x.GetXmlNode().NodeType != XmlNodeType.Comment).ToList();
            }
            else
            {
                mEndingnodes = this.GetAllNodes().Where(x => x.ChildNodes.Count == 0 && !x.XMLString.EndsWith("/>") && x.GetXmlNode().NodeType != XmlNodeType.EndElement && x.GetXmlNode().NodeType != XmlNodeType.Comment).ToList();

            }
            return mEndingnodes;
        }


        #endregion

        #region Private
        private static string UpdateXML(string xml, string xpath, string newVal)
        {
            XmlDocument XD = new XmlDocument();
            XD.LoadXml(xml);
            XmlNode mXN = XD.SelectSingleNode(xpath);
            mXN.InnerXml = newVal;
            return XD.InnerXml;
        }
        
        private List<XMLDocExtended> ChildNodes = new List<XMLDocExtended>();

        private string SetXpath()
        {

            string basePath = (ParentNode == null || ParentNode.GetXmlNode().NodeType == XmlNodeType.Document)
                ? "/"
                : ParentNode.XPath + "/";
            string TempXpath = basePath + GetRelativeXpath();

            TempXpath = ProcessXpath(TempXpath);
            return TempXpath;
        }

        private string ProcessXpath(string tempXpath)
        {
            string xpath = tempXpath;



             const string regex = @"^\/([a-zA-Z])*:([a-zA-Z])*(\[(\d)*\]|\/)";
            if (GetXmlNode().NodeType.ToString().ToUpper() == "TEXT")
            {
                string removeme = Regex.Match(xpath, @"text\[(\d|\d\d|\d\d\d)\]$").Value;
                xpath = xpath.Remove(xpath.Length - removeme.Length - 1);

            }


            string Starting = Regex.Match(xpath, regex).Value;



            string XpathNameSpace = Regex.Match(Starting.Replace("/", ""), "([a-zA-Z])*:([a-zA-Z])*").Value;

            if (!String.IsNullOrEmpty(XpathNameSpace))
            {
                XpathNameSpace = @"//*[name()='" + XpathNameSpace + "']";
                
                xpath = xpath.Replace(Starting, "");
                if (!xpath.StartsWith("/"))
                {
                    xpath = XpathNameSpace + @"/" + xpath;
                }
            }

            return xpath;
        }



        private void BuildThisNode(XmlNodeList XNL)
        {
            foreach (XmlNode XN in XNL)
            {
                if(XN.OuterXml.ElementAt(0).Equals('<') && XN.OuterXml.ElementAt(XN.OuterXml.Length - 1).Equals('>'))
                {
                    ChildNodes.Add(new XMLDocExtended(this, XN));
                }
            }
        }

        
        private string GetRelativeXpath()
        {
            if (ParentNode == null)
            {
                return XN.Name;
            }
            if (XN.NodeType == XmlNodeType.Document)
                return string.Empty;

            int i = 1;
            foreach (XmlNode node in ParentNode.GetXmlNode().ChildNodes)
            {
                if (node.Name != XN.Name)
                {
                    continue;
                }

                if (node == this.XN)
                {
                    break;
                }
                i++;
            }

            return XN.Name + "[" + i + "]";
        }

        public void RemoveDuplicatesNodes()
        {
            List<XMLDocExtended> childNodesList = this.GetChildNodes();
            if (childNodesList.Count == 0)
            {
                return;
            }
            if (childNodesList.Count > 1)
            {
                for (int i = 0; i < childNodesList.Count; i++)
                {
                    if (!childNodesList[i].Name.Equals("comment"))
                    {
                        List<XMLDocExtended> duplicateChild = null;
                        duplicateChild = childNodesList.Where(x => x.Name == childNodesList[i].Name && x.XPath != childNodesList[i].XPath).ToList();
                        foreach (XMLDocExtended child in duplicateChild)
                        {
                            childNodesList[i].XN.ParentNode.RemoveChild(child.XN);
                            childNodesList.Remove(child);
                        }
                    }
                }
            }

            foreach (XMLDocExtended child in childNodesList)
            {
                child.RemoveDuplicatesNodes();
            }
        }

        #endregion

    }
}
