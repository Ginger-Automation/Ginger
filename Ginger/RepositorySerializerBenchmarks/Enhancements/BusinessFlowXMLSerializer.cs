using Amdocs.Ginger.Repository;
using BenchmarkDotNet.Toolchains.CsProj;
using GingerCore;
using GingerCore.Activities;
using GingerCore.Platforms;
using GingerCore.Variables;
using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RepositorySerializerBenchmarks.Enhancements
{
    public sealed class BusinessFlowXMLSerializer
    {
        public BusinessFlowXMLSerializer() { }

        public XmlElement Serialize(BusinessFlow businessFlow, XmlDocument xmlDocument)
        {
            XmlElement businessFlowElement = xmlDocument.CreateElement(nameof(BusinessFlow));

            businessFlowElement.SetAttribute(nameof(BusinessFlow.Guid), businessFlow.Guid.ToString());
            
            if(!string.IsNullOrEmpty(businessFlow.Name))
                businessFlowElement.SetAttribute(nameof(BusinessFlow.Name), businessFlow.Name);
            
            businessFlowElement.SetAttribute(nameof(BusinessFlow.ParentGuid), businessFlow.ParentGuid.ToString());
            
            businessFlowElement.SetAttribute(nameof(BusinessFlow.Source), businessFlow.Source.ToString());
            
            businessFlowElement.SetAttribute(nameof(BusinessFlow.Status), businessFlow.Status.ToString());

            if (businessFlow.Activities.Any())
            {
                XmlElement activitiesElement = xmlDocument.CreateElement(nameof(BusinessFlow.Activities));
                ActivityXMLSerializer activityXMLSerializer = new();
                foreach (Activity activity in businessFlow.Activities)
                {
                    XmlElement activityElement = activityXMLSerializer.Serialize(activity, xmlDocument);
                    activitiesElement.AppendChild(activityElement);
                }
                businessFlowElement.AppendChild(activitiesElement);
            }

            if (businessFlow.ActivitiesGroups.Any())
            {
                XmlElement activitiesGroupsElement = xmlDocument.CreateElement(nameof(BusinessFlow.ActivitiesGroups));
                ActivitiesGroupXMLSerializer activitiesGroupXMLSerializer = new();
                foreach (ActivitiesGroup activitiesGroup in businessFlow.ActivitiesGroups)
                {
                    XmlElement activitiesGroupElement = activitiesGroupXMLSerializer.Serialize(activitiesGroup, xmlDocument);
                    activitiesGroupsElement.AppendChild(activitiesGroupElement);
                }
                businessFlowElement.AppendChild(activitiesGroupsElement);
            }

            if (businessFlow.TargetApplications.Any())
            {
                XmlElement targetApplicationsElement = xmlDocument.CreateElement(nameof(BusinessFlow.TargetApplications));
                TargetApplicationXMLSerializer targetApplicationXMLSerializer = new();
                foreach (TargetApplication targetApplication in businessFlow.TargetApplications)
                {
                    XmlElement targetApplicationElement = targetApplicationXMLSerializer.Serialize(targetApplication, xmlDocument);
                    targetApplicationsElement.AppendChild(targetApplicationElement);
                }
                businessFlowElement.AppendChild(targetApplicationsElement);
            }

            if (businessFlow.Variables.Any())
            {
                XmlElement variablesElement = xmlDocument.CreateElement(nameof(BusinessFlow.Variables));
                VariableXMLSerializer variableXMLSerializer = new();
                foreach (VariableBase variable in businessFlow.Variables)
                {
                    XmlElement variableElement = variableXMLSerializer.Serialize(variable, xmlDocument);
                    variablesElement.AppendChild(variableElement);
                }
                businessFlowElement.AppendChild(variablesElement);
            }

            return businessFlowElement;
        }

        public BusinessFlow Deserialize(XmlElement businessFlowElement, bool lazyLoad)
        {
            BusinessFlow businessFlow = new();

            foreach (XmlAttribute attribute in businessFlowElement.Attributes)
                SetBusinessFlowPropertyFromAttribute(businessFlow, attribute.Name, attribute.Value);

            foreach (XmlElement childElement in businessFlowElement.ChildNodes.Cast<XmlElement>())
            {
                if (string.Equals(childElement.Name, nameof(BusinessFlow.Activities)) && !lazyLoad)
                {
                    ActivityXMLSerializer activityXMLSerializer = new();
                    List<Activity> activities = new();
                    foreach (XmlElement activityElement in childElement.ChildNodes.Cast<XmlElement>())
                    {
                        Activity activity = activityXMLSerializer.Deserialize(activityElement);
                        activities.Add(activity);
                    }
                    businessFlow.Activities = new(activities);
                }
                else if (string.Equals(childElement.Name, nameof(BusinessFlow.ActivitiesGroups)) && !lazyLoad)
                {
                    ActivitiesGroupXMLSerializer activitiesGroupXMLSerializer = new();
                    List<ActivitiesGroup> activitiesGroups = new();
                    foreach (XmlElement activitiesGroupElement in childElement.ChildNodes.Cast<XmlElement>())
                    {
                        ActivitiesGroup activitiesGroup = activitiesGroupXMLSerializer.Deserialize(activitiesGroupElement);
                        activitiesGroups.Add(activitiesGroup);
                    }
                    businessFlow.ActivitiesGroups = new(activitiesGroups);
                }
                else if (string.Equals(childElement.Name, nameof(BusinessFlow.TargetApplications)) && !lazyLoad)
                {
                    TargetApplicationXMLSerializer targetApplicationXMLSerializer = new();
                    List<TargetApplication> targetApplications = new();
                    foreach (XmlElement targetApplicationElement in childElement.ChildNodes.Cast<XmlElement>())
                    {
                        TargetApplication targetApplication = targetApplicationXMLSerializer.Deserialize(targetApplicationElement);
                        targetApplications.Add(targetApplication);
                    }
                    businessFlow.TargetApplications = new(targetApplications);
                }
                else if (string.Equals(childElement.Name, nameof(BusinessFlow.Variables)) && !lazyLoad)
                {
                    VariableXMLSerializer variableXMLSerializer = new();
                    List<VariableBase> variables = new();
                    foreach (XmlElement variableElement in childElement.ChildNodes.Cast<XmlElement>())
                    {
                        VariableBase variable = variableXMLSerializer.Deserialize(variableElement);
                        variables.Add(variable);
                    }
                    businessFlow.Variables = new(variables);
                }
            }

            return businessFlow;
        }

        private void SetBusinessFlowPropertyFromAttribute(BusinessFlow businessFlow, string attributeName, string attributeValue)
        {
            if (string.Equals(attributeName, nameof(BusinessFlow.Guid)))
                businessFlow.Guid = Guid.Parse(attributeValue);
            else if (string.Equals(attributeName, nameof(BusinessFlow.Name)))
                businessFlow.Name = attributeValue;
            else if (string.Equals(attributeName, nameof(BusinessFlow.ParentGuid)))
                businessFlow.ParentGuid = Guid.Parse(attributeValue);
            else if (string.Equals(attributeName, nameof(BusinessFlow.Source)))
                businessFlow.Source = Enum.Parse<BusinessFlow.eSource>(attributeValue);
            else if (string.Equals(attributeName, nameof(BusinessFlow.Status)))
                businessFlow.Status = Enum.Parse<BusinessFlow.eBusinessFlowStatus>(attributeValue);
        }

        public BusinessFlow Deserialize(XmlReader xmlReader, bool lazyLoad)
        {
            if (xmlReader.NodeType != XmlNodeType.Element)
                throw new Exception($"Expected a element node type but found {xmlReader.NodeType}.");
            if(!string.Equals(xmlReader.Name, nameof(BusinessFlow)))
                throw new Exception($"Expected element {nameof(BusinessFlow)} but found {xmlReader.Name}.");

            BusinessFlow businessFlow = new();

            for (int attrIndex = 0; attrIndex < xmlReader.AttributeCount; attrIndex++)
            {
                xmlReader.MoveToAttribute(attrIndex);
                SetBusinessFlowPropertyFromAttribute(businessFlow, attributeName: xmlReader.Name, attributeValue: xmlReader.Value);
            }
            xmlReader.MoveToElement();

            if (lazyLoad)
                return businessFlow;

            int startDepth = xmlReader.Depth;
            while (xmlReader.Read())
            {
                bool reachedEndOfFile = xmlReader.EOF;
                bool reachedSibling = xmlReader.Depth == startDepth && !string.Equals(xmlReader.Name, nameof(BusinessFlow));
                bool reachedParent = xmlReader.Depth < startDepth;
                if (reachedEndOfFile || reachedSibling || reachedParent)
                    break;

                if (string.Equals(xmlReader.Name, nameof(BusinessFlow.Activities)))
                    businessFlow.Activities = new(DeserializeActivitiesElement(xmlReader));

                else if (string.Equals(xmlReader.Name, nameof(BusinessFlow.ActivitiesGroups)))
                    businessFlow.ActivitiesGroups = new(DeserializeActivitiesGroupsElement(xmlReader));

                else if (string.Equals(xmlReader.Name, nameof(BusinessFlow.TargetApplications)))
                    businessFlow.TargetApplications = new(DeserializeTargetApplicationsElement(xmlReader));

                else if (string.Equals(xmlReader.Name, nameof(BusinessFlow.Variables)))
                    businessFlow.Variables = new(DeserializeVariablesElement(xmlReader));
            }

            return businessFlow;
        }

        private IEnumerable<Activity> DeserializeActivitiesElement(XmlReader xmlReader)
        {
            if (xmlReader.NodeType != XmlNodeType.Element)
                throw new Exception($"Expected a element node type but found {xmlReader.NodeType}.");
            if (!string.Equals(xmlReader.Name, nameof(BusinessFlow.Activities)))
                throw new Exception($"Expected element {nameof(BusinessFlow.Activities)} but found {xmlReader.Name}.");

            List<Activity> activities = new();
            ActivityXMLSerializer activityXMLSerializer = new();

            int startDepth = xmlReader.Depth;
            while (xmlReader.Read())
            {
                bool reachedEndOfFile = xmlReader.EOF;
                bool reachedSibling = xmlReader.Depth == startDepth && !string.Equals(xmlReader.Name, nameof(BusinessFlow.Activities));
                bool reachedParent = xmlReader.Depth < startDepth;
                if (reachedEndOfFile || reachedSibling || reachedParent)
                    break;

                if (xmlReader.NodeType != XmlNodeType.Element)
                    continue;

                if (xmlReader.Depth != startDepth + 1)
                    continue;

                if (string.Equals(xmlReader.Name, nameof(Activity)))
                {
                    Activity activity = activityXMLSerializer.Deserialize(xmlReader);
                    activities.Add(activity);
                }
            }

            return activities;
        }

        private IEnumerable<ActivitiesGroup> DeserializeActivitiesGroupsElement(XmlReader xmlReader)
        {
            if (xmlReader.NodeType != XmlNodeType.Element)
                throw new Exception($"Expected a element node type but found {xmlReader.NodeType}.");
            if (!string.Equals(xmlReader.Name, nameof(BusinessFlow.ActivitiesGroups)))
                throw new Exception($"Expected element {nameof(BusinessFlow.ActivitiesGroups)} but found {xmlReader.Name}.");

            List<ActivitiesGroup> activitiesGroups = new();
            ActivitiesGroupXMLSerializer activitiesGroupXMLSerializer = new();

            int startDepth = xmlReader.Depth;
            while (xmlReader.Read())
            {
                bool reachedEndOfFile = xmlReader.EOF;
                bool reachedSibling = xmlReader.Depth == startDepth && !string.Equals(xmlReader.Name, nameof(BusinessFlow.ActivitiesGroups));
                bool reachedParent = xmlReader.Depth < startDepth;
                if (reachedEndOfFile || reachedSibling || reachedParent)
                    break;

                if (xmlReader.NodeType != XmlNodeType.Element)
                    continue;

                if (xmlReader.Depth != startDepth + 1)
                    continue;

                if (string.Equals(xmlReader.Name, nameof(ActivitiesGroup)))
                {
                    ActivitiesGroup activitiesGroup = activitiesGroupXMLSerializer.Deserialize(xmlReader);
                    activitiesGroups.Add(activitiesGroup);
                }
            }

            return activitiesGroups;
        }

        private IEnumerable<TargetApplication> DeserializeTargetApplicationsElement(XmlReader xmlReader)
        {
            if (xmlReader.NodeType != XmlNodeType.Element)
                throw new Exception($"Expected a element node type but found {xmlReader.NodeType}.");
            if (!string.Equals(xmlReader.Name, nameof(BusinessFlow.TargetApplications)))
                throw new Exception($"Expected element {nameof(BusinessFlow.TargetApplications)} but found {xmlReader.Name}.");

            List<TargetApplication> targetApplications = new();
            TargetApplicationXMLSerializer targetApplicationXMLSerializer = new();

            int startDepth = xmlReader.Depth;
            while (xmlReader.Read())
            {
                bool reachedEndOfFile = xmlReader.EOF;
                bool reachedSibling = xmlReader.Depth == startDepth && !string.Equals(xmlReader.Name, nameof(BusinessFlow.TargetApplications));
                bool reachedParent = xmlReader.Depth < startDepth;
                if (reachedEndOfFile || reachedSibling || reachedParent)
                    break;

                if (xmlReader.NodeType != XmlNodeType.Element)
                    continue;

                if (xmlReader.Depth != startDepth + 1)
                    continue;

                if (string.Equals(xmlReader.Name, nameof(TargetApplication)))
                {
                    TargetApplication targetApplication = targetApplicationXMLSerializer.Deserialize(xmlReader);
                    targetApplications.Add(targetApplication);
                }
            }

            return targetApplications;
        }

        private IEnumerable<VariableBase> DeserializeVariablesElement(XmlReader xmlReader)
        {
            if (xmlReader.NodeType != XmlNodeType.Element)
                throw new Exception($"Expected a element node type but found {xmlReader.NodeType}.");
            if (!string.Equals(xmlReader.Name, nameof(BusinessFlow.Variables)))
                throw new Exception($"Expected element {nameof(BusinessFlow.Variables)} but found {xmlReader.Name}.");

            List<VariableBase> variables = new();
            VariableXMLSerializer variableXMLSerializer = new();

            int startDepth = xmlReader.Depth;
            while (xmlReader.Read())
            {
                bool reachedEndOfFile = xmlReader.EOF;
                bool reachedSibling = xmlReader.Depth == startDepth && !string.Equals(xmlReader.Name, nameof(BusinessFlow.Variables));
                bool reachedParent = xmlReader.Depth < startDepth;
                if (reachedEndOfFile || reachedSibling || reachedParent)
                    break;

                if (xmlReader.NodeType != XmlNodeType.Element)
                    continue;

                if (xmlReader.Depth != startDepth + 1)
                    continue;

                VariableBase variable = variableXMLSerializer.Deserialize(xmlReader);
                variables.Add(variable);
            }

            return variables;
        }

    }
}
