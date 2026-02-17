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
using System.Linq;
using System.Xml;

namespace Ginger.Imports.UFT
{
    public static class ObjectRepositoryConverter
    {
        //Create List Objects for Property and Repo Item
        public static List<ObjectRepositoryItem> Objectlist = [];

        public static List<ObjectRepositoryItem> ProcessXML(string sXMLPath)
        {
            // force cleaning the list 
            Objectlist.Clear();

            //Parse the XML
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(sXMLPath);
            XmlNodeList nodeList = xmlDoc.GetElementsByTagName("qtpRep:Object");

            //Fetch all the properties associated with the object from XML
            foreach (XmlNode node in nodeList)
            {
                ObjectRepositoryItem Object_ori = new ObjectRepositoryItem();

                //fetch the Object Class and Name as it appears in OR
                var sXMLObjectName = node.Attributes["Name"].Value;
                var sXMLObjectClass = node.Attributes["Class"].Value;

                Object_ori.Name = sXMLObjectName.ToString();
                Object_ori.Class = sXMLObjectClass.ToString();

                XmlNodeList PropertiesList = node.ChildNodes[0].ChildNodes;

                //Loop through all the properties of the Object paresed in XML 
                for (int i = 0; i < PropertiesList.Count; i++)
                {
                    //fetch the List of Properties
                    XmlElement eChild = (XmlElement)PropertiesList[i];

                    //Access each property in the Property list
                    var sXMLObjectProperty = eChild.Attributes["Name"].Value;

                    //Storing value in ObjectRepositoryItemProperty object
                    if (eChild.FirstChild.FirstChild.Value != "")
                    {
                        ObjectRepositortyItemProperty Orip = new ObjectRepositortyItemProperty
                        {
                            Name = sXMLObjectProperty.ToString(),
                            Value = eChild.FirstChild.FirstChild.Value
                        };
                        Object_ori.Properties.Add(Orip);
                        Orip = null;
                    }
                }

                //Create a List of Object Repository Items
                Objectlist.Add(Object_ori);
                Object_ori = null;
            }

            return Objectlist;
        }

        public static string GetObjectProperty(string ObjectName, string PropertyName)
        {
            ObjectRepositoryItem Item = (from x in Objectlist where x.Name == ObjectName select x).FirstOrDefault();
            if (Item != null)
            {
                return (from y in Item.Properties where y.Name == PropertyName select y.Value).FirstOrDefault();
            }

            return null;
        }
    }
}
