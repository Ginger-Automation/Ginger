using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using Org.BouncyCastle.Asn1.Cms;
using RepositorySerializerBenchmarks.Enhancements.LiteXML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RepositorySerializerBenchmarks.Enhancements
{
    public sealed class ActivityXMLSerializer
    {
        public ActivityXMLSerializer() { }

        public XmlElement Serialize(Activity activity, XmlDocument xmlDocument)
        {
            XmlElement activityElement = xmlDocument.CreateElement(nameof(Activity));

            if(activity.ActionRunOption != null)
                activityElement.SetAttribute(nameof(Activity.ActionRunOption), activity.ActionRunOption.ToString());

            activityElement.SetAttribute(nameof(Activity.Active), activity.Active.ToString());

            if(!string.IsNullOrEmpty(activity.ActivitiesGroupID))
                activityElement.SetAttribute(nameof(Activity.ActivitiesGroupID), activity.ActivitiesGroupID);

            if(!string.IsNullOrEmpty(activity.ActivityName))
                activityElement.SetAttribute(nameof(Activity.ActivityName), activity.ActivityName);
            
            if(activity.AutomationStatus != null)
                activityElement.SetAttribute(nameof(Activity.AutomationStatus), activity.AutomationStatus.ToString());
            
            if(!string.IsNullOrEmpty(activity.Description))
                activityElement.SetAttribute(nameof(Activity.Description), activity.Description);
            
            activityElement.SetAttribute(nameof(Activity.ErrorHandlerMappingType), activity.ErrorHandlerMappingType.ToString());
            
            activityElement.SetAttribute(nameof(Activity.Guid), activity.Guid.ToString());
            
            activityElement.SetAttribute(nameof(Activity.ParentGuid), activity.ParentGuid.ToString());
            
            if(!string.IsNullOrEmpty(activity.PercentAutomated))
                activityElement.SetAttribute(nameof(Activity.PercentAutomated), activity.PercentAutomated);
            
            activityElement.SetAttribute(nameof(Activity.POMMetaDataId), activity.POMMetaDataId.ToString());
            
            if(!string.IsNullOrEmpty(activity.TargetApplication))
                activityElement.SetAttribute(nameof(Activity.TargetApplication), activity.TargetApplication);
            
            activityElement.SetAttribute(nameof(Activity.Type), activity.Type.ToString());

            if (activity.Acts.Any())
            {
                XmlElement actsElement = xmlDocument.CreateElement(nameof(Activity.Acts));
                ActXMLSerializer actXMLSerializer = new();
                foreach (Act act in activity.Acts)
                {
                    XmlElement actElement = actXMLSerializer.Serialize(act, xmlDocument);
                    actsElement.AppendChild(actElement);
                }
                activityElement.AppendChild(actsElement);
            }

            return activityElement;
        }

        public Activity Deserialize(XmlElement activityElement)
        {
            Activity activity = new();

            foreach(XmlAttribute attribute in activityElement.Attributes)
                SetActivityPropertyFromAttribute(activity, attribute.Name, attribute.Value);

            foreach(XmlElement childElement in activityElement.ChildNodes.Cast<XmlElement>())
            {
                if(string.Equals(childElement.Name, nameof(Activity.Acts)))
                {
                    ActXMLSerializer actXMLSerializer = new();
                    List<Act> acts = new();
                    foreach(XmlElement actElement in childElement.ChildNodes.Cast<XmlElement>())
                    {
                        Act act = actXMLSerializer.Deserialize(actElement);
                        acts.Add(act);
                    }
                    activity.Acts = new(acts);
                }
            }

            return activity;
        }

        private void SetActivityPropertyFromAttribute(Activity activity, string attributeName, string attributeValue)
        {
            if (string.Equals(attributeName, nameof(Activity.ActionRunOption)))
                activity.ActionRunOption = Enum.Parse<eActionRunOption>(attributeValue);
            else if (string.Equals(attributeName, nameof(Activity.Active)))
                activity.Active = bool.Parse(attributeValue);
            else if (string.Equals(attributeName, nameof(Activity.ActivitiesGroupID)))
                activity.ActivitiesGroupID = attributeValue;
            else if (string.Equals(attributeName, nameof(Activity.ActivityName)))
                activity.ActivityName = attributeValue;
            else if (string.Equals(attributeName, nameof(Activity.AutomationStatus)))
                activity.AutomationStatus = Enum.Parse<eActivityAutomationStatus>(attributeValue);
            else if (string.Equals(attributeName, nameof(Activity.ErrorHandlerMappingType)))
                activity.ErrorHandlerMappingType = Enum.Parse<eHandlerMappingType>(attributeValue);
            else if (string.Equals(attributeName, nameof(Activity.Guid)))
                activity.Guid = Guid.Parse(attributeValue);
            else if (string.Equals(attributeName, nameof(Activity.ParentGuid)))
                activity.ParentGuid = Guid.Parse(attributeValue);
            else if (string.Equals(attributeName, nameof(Activity.PercentAutomated)))
                activity.PercentAutomated = attributeValue;
            else if (string.Equals(attributeName, nameof(Activity.POMMetaDataId)))
                activity.POMMetaDataId = Guid.Parse(attributeValue);
            else if (string.Equals(attributeName, nameof(Activity.TargetApplication)))
                activity.TargetApplication = attributeValue;
            else if (string.Equals(attributeName, nameof(Activity.Type)))
                activity.Type = Enum.Parse<eSharedItemType>(attributeValue);
        }

        public Activity Deserialize(XmlReader xmlReader)
        {
            //return new Activity(new RIBXmlReader(xmlReader));
            if (xmlReader.NodeType != XmlNodeType.Element)
                throw new Exception($"Expected a element node type but found {xmlReader.NodeType}.");
            if (!string.Equals(xmlReader.Name, nameof(Activity)))
                throw new Exception($"Expected element {nameof(Activity)} but found {xmlReader.Name}.");

            Activity activity = new();

            for (int attrIndex = 0; attrIndex < xmlReader.AttributeCount; attrIndex++)
            {
                xmlReader.MoveToAttribute(attrIndex);
                SetActivityPropertyFromAttribute(activity, attributeName: xmlReader.Name, attributeValue: xmlReader.Value);
            }
            xmlReader.MoveToElement();

            if (!xmlReader.IsEmptyElement)
            {
                int startDepth = xmlReader.Depth;
                while (xmlReader.Read())
                {
                    xmlReader.MoveToContent();

                    bool reachedEndOfElement = xmlReader.Depth == startDepth && xmlReader.NodeType == XmlNodeType.EndElement;
                    if (reachedEndOfElement)
                        break;

                    if (!xmlReader.IsStartElement())
                        continue;

                    bool isGrandChild = xmlReader.Depth > startDepth + 1;
                    if (isGrandChild)
                        continue;

                    if (string.Equals(xmlReader.Name, nameof(Activity.Acts)))
                        activity.Acts = new(DeserializeActsElement(xmlReader));
                }
            }
            
            return activity;
        }

        private IEnumerable<Act> DeserializeActsElement(XmlReader xmlReader)
        {
            if (xmlReader.NodeType != XmlNodeType.Element)
                throw new Exception($"Expected a element node type but found {xmlReader.NodeType}.");
            if (!string.Equals(xmlReader.Name, nameof(Activity.Acts)))
                throw new Exception($"Expected element {nameof(Activity.Acts)} but found {xmlReader.Name}.");

            if (xmlReader.IsEmptyElement)
                return Array.Empty<Act>();

            List<Act> acts = new();
            ActXMLSerializer actXMLSerializer = new();

            int startDepth = xmlReader.Depth;
            while (xmlReader.Read())
            {
                xmlReader.MoveToContent();

                bool reachedEndOfElement = xmlReader.Depth == startDepth && xmlReader.NodeType == XmlNodeType.EndElement;
                if (reachedEndOfElement)
                    break;

                if (!xmlReader.IsStartElement())
                    continue;

                bool isGrandChild = xmlReader.Depth > startDepth + 1;
                if (isGrandChild)
                    continue;

                Act act = actXMLSerializer.Deserialize(xmlReader);
                acts.Add(act);
            }

            return acts;
        }

        public Activity Deserialize(LiteXMLElement activityElement)
        {
            Activity activity = new();

            foreach (LiteXMLAttribute attribute in activityElement.Attributes)
                SetActivityPropertyFromAttribute(activity, attribute.Name, attribute.Value);

            foreach(LiteXMLElement childElement in activityElement.ChildElements)
            {
                if (string.Equals(childElement.Name, nameof(Activity.Acts)))
                {
                    List<Act> acts = new();
                    ActXMLSerializer actXMLSerializer = new();
                    foreach (LiteXMLElement actElement in childElement.ChildElements)
                    {
                        Act act = actXMLSerializer.Deserialize(actElement);
                        acts.Add(act);
                    }
                    activity.Acts = new(acts);
                }
            }

            return activity;
        }
    }
}
