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
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace GingerATS
{
    public class GingerATSXmlReader
    {
        public static XmlReader GetXMLReaderFromFile(string xmlFilePath)
        {
            try
            {
                using (FileStream fileSteam = File.OpenRead(xmlFilePath))
                {
                    XmlReaderSettings settings;

                    settings = new XmlReaderSettings();
                    settings.IgnoreWhitespace = true;
                    XmlReader reader = XmlReader.Create(fileSteam, settings);
                    return reader;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static XmlReader GetXMLReaderFromString(string xmlString)
        {
            try
            {
                XmlReaderSettings settings;
                settings = new XmlReaderSettings();
                settings.IgnoreWhitespace = true;
                XmlReader reader = XmlReader.Create(new StringReader(xmlString), settings);
                return reader;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool MoveXmlReaderToSpecificNode(XmlReader xmlReader, string nodeName)
        {
            try
            {                
                while (xmlReader.Read())
                {
                    if (xmlReader.Name.ToUpper().Contains(nodeName.ToUpper()))
                        return true;
                }
                return false;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public static bool MoveXmlReaderToSpecificNode(XmlReader xmlReader, string nodeName, string attributeName, string attributeValue)
        {
            try
            {
                while (xmlReader.Read())
                {
                    if (xmlReader.Name.ToUpper().Contains(nodeName.ToUpper()))
                    {
                        if (xmlReader.GetAttribute(attributeName) == attributeValue)
                            return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string GetNodeAttributeValue(string xmlFilePath, string nodeName, string attributeName)
        {
            XmlReader xmlReader=null;
            try
            {
                xmlReader = GingerATSXmlReader.GetXMLReaderFromFile(xmlFilePath);
                if (xmlReader != null)
                {
                    if (GingerATSXmlReader.MoveXmlReaderToSpecificNode(xmlReader, nodeName))
                    {
                        string attributeValue= xmlReader.GetAttribute(attributeName);
                        xmlReader.Close();
                        return attributeValue;
                    }
                }

                xmlReader.Close();
                return null;
            }
            catch (Exception)
            {
                if (xmlReader != null)
                    xmlReader.Close();
                return null;
            }
        }

        public static bool ValidateNodeAttributsValue(string xmlPathOrString, string nodeName, List<NodeAttributeValidation> attributesToValidate, bool getReaderFromString=false)
        {
            XmlReader xmlReader = null;
            try
            {
                if (!getReaderFromString)
                    xmlReader = GingerATSXmlReader.GetXMLReaderFromFile(xmlPathOrString);
                else
                    xmlReader = GingerATSXmlReader.GetXMLReaderFromString(xmlPathOrString);

                while (GingerATSXmlReader.MoveXmlReaderToSpecificNode(xmlReader, nodeName))
                {
                    if (attributesToValidate != null && attributesToValidate.Count > 0)
                    {
                        int matchingAttributesNum = 0;
                        foreach (NodeAttributeValidation attributToValidate in attributesToValidate)
                        {
                            string attributeValue = xmlReader.GetAttribute(attributToValidate.Name);
                            if (attributeValue != null && attributToValidate.ValidateInUpperCase)
                            {
                                attributeValue = attributeValue.ToUpper();
                                attributToValidate.Value = attributToValidate.Value.ToUpper();
                            }
                            switch (attributToValidate.ValidationType)
                            {
                                case NodeAttributeValidation.eNodeAttributeValidationType.Equal:
                                    if (attributeValue == attributToValidate.Value)
                                        matchingAttributesNum++;
                                    break;
                                case NodeAttributeValidation.eNodeAttributeValidationType.Contains:
                                    if (attributeValue != null && attributeValue.Contains(attributToValidate.Value))
                                        matchingAttributesNum++;
                                    break;
                                case NodeAttributeValidation.eNodeAttributeValidationType.CommaSeperatedListValue:
                                    if (attributeValue != null)
                                    {
                                        string[] listValues = attributeValue.Split(';');
                                        if (listValues.Where(x => x == attributToValidate.Value).FirstOrDefault() != null)
                                            matchingAttributesNum++;
                                    }
                                    break;
                                case NodeAttributeValidation.eNodeAttributeValidationType.NewLineSeperatedListValue:
                                    if (attributeValue != null)
                                    {
                                        string[] listValues = attributeValue.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                                        if (listValues.Where(x => x == attributToValidate.Value).FirstOrDefault() != null)
                                            matchingAttributesNum++;
                                    }
                                    break;
                            }
                        }
                        if (matchingAttributesNum == attributesToValidate.Count)
                        {
                            xmlReader.Close();
                            return true;
                        }
                    }
                }

                xmlReader.Close();
                return false;
            }
            catch (Exception)
            {
                if (xmlReader != null)
                    xmlReader.Close();
                return false;
            }
        }

        public static string GetXmlFileNodeContent(string xmlFilePath, string nodeName)
        {
            XDocument doc;
            try
            {                
                doc = XDocument.Load(xmlFilePath);
                string nodeXml = doc.Root.Element(nodeName).ToString();
                doc = null;
                return nodeXml;                
            }
            catch (Exception)
            {
                doc= null;
                return string.Empty;
            }
        }
    }

    public class NodeAttributeValidation
    {
        public enum eNodeAttributeValidationType { Equal, Contains, CommaSeperatedListValue, NewLineSeperatedListValue }

        public string Name;
        public string Value;
        public eNodeAttributeValidationType ValidationType;
        public bool ValidateInUpperCase;

        public NodeAttributeValidation(string name, string value,
                                         eNodeAttributeValidationType validationType = eNodeAttributeValidationType.Equal,
                                         bool validateInUpperCase = false)
        {
            this.Name = name;
            this.Value = value;
            this.ValidationType = validationType;
            this.ValidateInUpperCase = validateInUpperCase;
        }        
    }
}
