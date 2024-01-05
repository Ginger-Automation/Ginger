using GingerCore;
using GingerCore.Activities;
using GingerCore.Platforms;
using GingerCore.Variables;
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
        private readonly XmlDocument _xmlDocument;

        public BusinessFlowXMLSerializer(XmlDocument xmlDocument)
        {
            _xmlDocument = xmlDocument;
        }

        public XmlElement Serialize(BusinessFlow businessFlow)
        {
            XmlElement businessFlowElement = _xmlDocument.CreateElement(nameof(BusinessFlow));

            businessFlowElement.SetAttribute(nameof(BusinessFlow.Guid), businessFlow.Guid.ToString());
            
            if(!string.IsNullOrEmpty(businessFlow.Name))
                businessFlowElement.SetAttribute(nameof(BusinessFlow.Name), businessFlow.Name);
            
            businessFlowElement.SetAttribute(nameof(BusinessFlow.ParentGuid), businessFlow.ParentGuid.ToString());
            
            businessFlowElement.SetAttribute(nameof(BusinessFlow.Source), businessFlow.Source.ToString());
            
            businessFlowElement.SetAttribute(nameof(BusinessFlow.Status), businessFlow.Status.ToString());

            if (businessFlow.Activities.Any())
            {
                XmlElement activitiesElement = _xmlDocument.CreateElement(nameof(BusinessFlow.Activities));
                ActivityXMLSerializer activityXMLSerializer = new(_xmlDocument);
                foreach (Activity activity in businessFlow.Activities)
                {
                    XmlElement activityElement = activityXMLSerializer.Serialize(activity);
                    activitiesElement.AppendChild(activityElement);
                }
                businessFlowElement.AppendChild(activitiesElement);
            }

            if (businessFlow.ActivitiesGroups.Any())
            {
                XmlElement activitiesGroupsElement = _xmlDocument.CreateElement(nameof(BusinessFlow.ActivitiesGroups));
                ActivitiesGroupXMLSerializer activitiesGroupXMLSerializer = new(_xmlDocument);
                foreach (ActivitiesGroup activitiesGroup in businessFlow.ActivitiesGroups)
                {
                    XmlElement activitiesGroupElement = activitiesGroupXMLSerializer.Serialize(activitiesGroup);
                    activitiesGroupsElement.AppendChild(activitiesGroupElement);
                }
                businessFlowElement.AppendChild(activitiesGroupsElement);
            }

            if (businessFlow.TargetApplications.Any())
            {
                XmlElement targetApplicationsElement = _xmlDocument.CreateElement(nameof(BusinessFlow.TargetApplications));
                TargetApplicationXMLSerializer targetApplicationXMLSerializer = new(_xmlDocument);
                foreach (TargetApplication targetApplication in businessFlow.TargetApplications)
                {
                    XmlElement targetApplicationElement = targetApplicationXMLSerializer.Serialize(targetApplication);
                    targetApplicationsElement.AppendChild(targetApplicationElement);
                }
                businessFlowElement.AppendChild(targetApplicationsElement);
            }

            if (businessFlow.Variables.Any())
            {
                XmlElement variablesElement = _xmlDocument.CreateElement(nameof(BusinessFlow.Variables));
                VariableXMLSerializer variableXMLSerializer = new(_xmlDocument);
                foreach (VariableBase variable in businessFlow.Variables)
                {
                    XmlElement variableElement = variableXMLSerializer.Serialize(variable);
                    variablesElement.AppendChild(variableElement);
                }
                businessFlowElement.AppendChild(variablesElement);
            }

            return businessFlowElement;
        }
    }
}
