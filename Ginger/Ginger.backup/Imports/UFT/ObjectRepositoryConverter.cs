using System.Collections.Generic;
using System.Xml;
using System.Linq;

namespace Ginger.Imports.UFT
{
    public static class ObjectRepositoryConverter
    {
       //Create List Objects for Property and Repo Item
        public static List<ObjectRepositoryItem> Objectlist = new List<ObjectRepositoryItem>();

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
                        ObjectRepositortyItemProperty Orip = new ObjectRepositortyItemProperty();
                        Orip.Name = sXMLObjectProperty.ToString();
                        Orip.Value = eChild.FirstChild.FirstChild.Value;
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