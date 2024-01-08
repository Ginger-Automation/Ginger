using GingerCore;
using GingerCore.Activities;
using GingerCore.Platforms;
using GingerCore.Variables;
using Microsoft.Azure.Cosmos.Linq;
using OpenQA.Selenium.DevTools.V115.DOM;
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
        public static bool LazyLoad { get; set; } = false;

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

        public BusinessFlow Deserialize(XmlElement businessFlowElement)
        {
            BusinessFlow businessFlow = new();

            foreach(XmlAttribute attribute in businessFlowElement.Attributes)
            {
                if (string.Equals(attribute.Name, nameof(BusinessFlow.Guid)))
                    businessFlow.Guid = Guid.Parse(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(BusinessFlow.Name)))
                    businessFlow.Name = attribute.Value;
                else if (string.Equals(attribute.Name, nameof(BusinessFlow.ParentGuid)))
                    businessFlow.ParentGuid = Guid.Parse(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(BusinessFlow.Source)))
                    businessFlow.Source = Enum.Parse<BusinessFlow.eSource>(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(BusinessFlow.Status)))
                    businessFlow.Status = Enum.Parse<BusinessFlow.eBusinessFlowStatus>(attribute.Value);
            }

            foreach(XmlElement childElement in businessFlowElement.ChildNodes.Cast<XmlElement>())
            {
                if(string.Equals(childElement.Name, nameof(BusinessFlow.Activities)) && LazyLoad)
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
                else if(string.Equals(childElement.Name, nameof(BusinessFlow.ActivitiesGroups)) && LazyLoad)
                {
                    ActivitiesGroupXMLSerializer activitiesGroupXMLSerializer = new();
                    List<ActivitiesGroup> activitiesGroups = new();
                    foreach(XmlElement activitiesGroupElement in childElement.ChildNodes.Cast<XmlElement>())
                    {
                        ActivitiesGroup activitiesGroup = activitiesGroupXMLSerializer.Deserialize(activitiesGroupElement);
                        activitiesGroups.Add(activitiesGroup);
                    }
                    businessFlow.ActivitiesGroups = new(activitiesGroups);
                }
                else if(string.Equals(childElement.Name, nameof(BusinessFlow.TargetApplications)) && LazyLoad)
                {
                    TargetApplicationXMLSerializer targetApplicationXMLSerializer = new();
                    List<TargetApplication> targetApplications = new();
                    foreach(XmlElement targetApplicationElement in childElement.ChildNodes.Cast<XmlElement>())
                    {
                        TargetApplication targetApplication = targetApplicationXMLSerializer.Deserialize(targetApplicationElement);
                        targetApplications.Add(targetApplication);
                    }
                    businessFlow.TargetApplications = new(targetApplications);
                }
                else if(string.Equals(childElement.Name, nameof(BusinessFlow.Variables)) && LazyLoad)
                {
                    VariableXMLSerializer variableXMLSerializer = new();
                    List<VariableBase> variables = new();
                    foreach(XmlElement variableElement in childElement.ChildNodes.Cast<XmlElement>())
                    {
                        VariableBase variable = variableXMLSerializer.Deserialize(variableElement);
                        variables.Add(variable);
                    }
                    businessFlow.Variables = new(variables);
                }
            }

            return businessFlow;
        }
    }
}
