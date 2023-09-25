using NUglify;
using OctaneStdSDK.Entities.Tasks;
using OpenQA.Selenium.DevTools.V113.CSS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

#nullable enable
namespace Amdocs.Ginger.CoreNET.BPMN
{
    public sealed class BPMNXMLSerializer
    {
        private const string BPMN_XML_PREFIX = "bpmn";
        private const string BPMN_XML_URI = "http://www.omg.org/spec/BPMN/20100524/MODEL";
        private const string IG_XML_PREFIX = "ig";
        private const string IG_XML_URI = "http://insightguard.com/schema/1.0/bpmn";

        public string Serialize(Collaboration collaboration)
        {
            XmlDocument xmlDocument = new();

            XmlElement definitionsElement = CreateDefinitionsElement(xmlDocument);
            xmlDocument.AppendChild(definitionsElement);

            XmlElement collaborationElement = CreateCollaborationElement(xmlDocument, collaboration);
            definitionsElement.AppendChild(collaborationElement);

            IEnumerable<Process> definitionProcesses = collaboration.Participants.Select(participant => participant.Process);
            foreach (Process process in definitionProcesses)
            {
                XmlElement processElement = CreateProcessElement(xmlDocument, process);
                definitionsElement.AppendChild(processElement);
            }

            return XmlDocumentToString(xmlDocument);
        }

        private string XmlDocumentToString(XmlDocument xmlDocument)
        {
            XmlWriterSettings xmlWriterSettings = new()
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n"
            };
            using MemoryStream memoryStream = new(1024);
            using StreamWriter streamWriter = new(memoryStream);
            using XmlWriter xmlTextWriter = XmlWriter.Create(streamWriter, xmlWriterSettings);
            xmlDocument.WriteTo(xmlTextWriter);
            xmlTextWriter.Flush();
            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        private XmlElement CreateDefinitionsElement(XmlDocument xmlDocument)
        {
            XmlElement definitionsElement = xmlDocument.CreateElement(BPMN_XML_PREFIX, "definitions", BPMN_XML_URI);
            definitionsElement.SetAttribute($"xmlns:{BPMN_XML_PREFIX}", BPMN_XML_URI);
            definitionsElement.SetAttribute($"xmlns:{IG_XML_PREFIX}", IG_XML_URI);
            definitionsElement.SetAttribute("targetNamespace", "http://bpmn.io/schema/bpmn");

            return definitionsElement;
        }

        private XmlElement CreateCollaborationElement(XmlDocument xmlDocument, Collaboration collaboration)
        {
            XmlElement collaborationElement = xmlDocument.CreateElement(BPMN_XML_PREFIX, "collaboration", BPMN_XML_URI);

            collaborationElement.SetAttribute("id", collaboration.Id);
            collaborationElement.SetAttribute("isClosed", "false"); //static value
            collaborationElement.SetAttribute("name", collaboration.Name);
            //collaborationElement.AppendChild(CreateDocumentationElement(xmlDocument));
            //TODO: BPMN - Why a different documentation element for Collaboration
            XmlElement documentationElement = xmlDocument.CreateElement("documentation", BPMN_XML_URI);
            documentationElement.SetAttribute("id", $"doc_{collaboration.Id}");
            documentationElement.SetAttribute("textFormat", "text/plain");
            collaborationElement.AppendChild(documentationElement);

            XmlElement extensionElements = CreateExtensionElements(xmlDocument, collaboration);
            collaborationElement.AppendChild(extensionElements);

            foreach(Participant participant in collaboration.Participants) 
            {
                XmlElement participantElement = CreateParticipantElement(xmlDocument, participant);
                collaborationElement.AppendChild(participantElement);
            }

            foreach(MessageFlow messageFlow in collaboration.GetMessageFlows())
            {
                XmlElement messageFlowElement = CreateMessageFlowElement(xmlDocument, messageFlow);
                collaborationElement.AppendChild(messageFlowElement);
            }

            return collaborationElement;
        }

        private XmlElement CreateExtensionElements(XmlDocument xmlDocument, Collaboration collaboration)
        {
            XmlElement extensionElements = xmlDocument.CreateElement(BPMN_XML_PREFIX, "extensionElements", BPMN_XML_URI);
            extensionElements.SetAttribute("xmlns", BPMN_XML_URI);

            XmlElement bpmnMetadataElement = CreateBPMNMetadataElement(xmlDocument, collaboration);
            extensionElements.AppendChild(bpmnMetadataElement);

            return extensionElements;
        }

        private XmlElement CreateBPMNMetadataElement(XmlDocument xmlDocument, Collaboration collaboration)
        {
            //TODO: MetadataElement details shouldn't be comming from Collaboration, since Collaboration.Guid is not necessarily the Metadata.uuid. Rethink this
            XmlElement bpmnMetaDataElement = xmlDocument.CreateElement(IG_XML_PREFIX, "bpmnMetadata", IG_XML_URI);

            XmlElement uuidElement = xmlDocument.CreateElement(IG_XML_PREFIX, "uuid", IG_XML_URI);
            XmlText uuidTextNode = xmlDocument.CreateTextNode(collaboration.Guid);
            uuidElement.AppendChild(uuidTextNode);
            bpmnMetaDataElement.AppendChild(uuidElement);

            XmlElement bpmnNameElement = xmlDocument.CreateElement(IG_XML_PREFIX, "bpmnName", IG_XML_URI);
            XmlText bpmnNameTextNode = xmlDocument.CreateTextNode(collaboration.Name);
            bpmnNameElement.AppendChild(bpmnNameTextNode);
            bpmnMetaDataElement.AppendChild(bpmnNameElement);

            XmlElement descriptionElement = xmlDocument.CreateElement(IG_XML_PREFIX, "description", IG_XML_URI);
            XmlText descriptionTextNode = xmlDocument.CreateTextNode(collaboration.Description);
            descriptionElement.AppendChild(descriptionTextNode);
            bpmnMetaDataElement.AppendChild(descriptionElement);

            //TODO: BPMN - Validate definitions->collaboration->extensionElements->bpmnMetadata->systemRef
            XmlElement systemRefElement = xmlDocument.CreateElement(IG_XML_PREFIX, "systemRef", IG_XML_URI);
            XmlText systemRefTextNode = xmlDocument.CreateTextNode(collaboration.SystemRef);
            systemRefElement.AppendChild(systemRefTextNode);
            bpmnMetaDataElement.AppendChild(systemRefElement);

            XmlElement domainRefElement = xmlDocument.CreateElement(IG_XML_PREFIX, "domainRef", IG_XML_URI);
            XmlText domainRefTextNode = xmlDocument.CreateTextNode("32"); //static value
            domainRefElement.AppendChild(domainRefTextNode);
            bpmnMetaDataElement.AppendChild(domainRefElement);

            XmlElement collaborationTypeElement = xmlDocument.CreateElement(IG_XML_PREFIX, "collaborationType", IG_XML_URI);
            XmlText collaborationTypeTextNode = xmlDocument.CreateTextNode(collaboration.CollaborationType.ToString().ToUpper());
            collaborationTypeElement.AppendChild(collaborationTypeTextNode);
            bpmnMetaDataElement.AppendChild(collaborationTypeElement);

            return bpmnMetaDataElement;
        }

        private XmlElement CreateParticipantElement(XmlDocument xmlDocument, Participant participant)
        {
            XmlElement participantElement = xmlDocument.CreateElement(BPMN_XML_PREFIX, "participant", BPMN_XML_URI);

            participantElement.SetAttribute("id", participant.Id);
            //TODO: BPMN - Validate definitions->collaboration->participant@systemRef attribute
            participantElement.SetAttribute("systemRef", IG_XML_URI, participant.SystemRef);
            participantElement.SetAttribute("name", participant.Name);
            participantElement.SetAttribute("processRef", participant.Process.Id);
            participantElement.AppendChild(CreateDocumentationElement(xmlDocument));

            return participantElement;
        }

        private XmlElement CreateMessageFlowElement(XmlDocument xmlDocument, MessageFlow messageFlow)
        {
            XmlElement messageFlowElement = xmlDocument.CreateElement(BPMN_XML_PREFIX, "messageFlow", BPMN_XML_URI);

            messageFlowElement.SetAttribute("id", messageFlow.Id);
            //TODO: BPMN - Validate definitions->collaboration->messageFlow@messageRef attribute
            messageFlowElement.SetAttribute("messageRef", IG_XML_URI, messageFlow.MessageRef);
            messageFlowElement.SetAttribute("name", messageFlow.Name);
            messageFlowElement.SetAttribute("sourceRef", messageFlow.Source.Id);
            messageFlowElement.SetAttribute("targetRef", messageFlow.Target.Id);
            messageFlowElement.AppendChild(CreateDocumentationElement(xmlDocument));

            return messageFlowElement;
        }

        private XmlElement CreateProcessElement(XmlDocument xmlDocument, Process process)
        {
            XmlElement processElement = xmlDocument.CreateElement(BPMN_XML_PREFIX, "process", BPMN_XML_URI);

            processElement.SetAttribute("id", process.Id);
            processElement.SetAttribute("isClosed", "false"); //static value
            processElement.SetAttribute("processType", "None"); //static value
            
            foreach(Task task in process.Tasks)
            {
                XmlElement taskElement;
                if(task is UserTask userTask)
                {
                    taskElement = CreateUserTaskElement(xmlDocument, userTask);
                }
                else if (task is ReceiveTask receiveTask)
                {
                    taskElement = CreateReceiveTaskElement(xmlDocument, receiveTask);
                }
                else if (task is ScriptTask scriptTask)
                {
                    taskElement = CreateScriptTaskElement(xmlDocument, scriptTask);
                }
                else if (task is SendTask sendTask)
                {
                    taskElement = CreateSendTaskElement(xmlDocument, sendTask);
                }
                else if (task is ServiceTask serviceTask)
                {
                    taskElement = CreateServiceTaskElement(xmlDocument, serviceTask);
                }
                else if (task is ManualTask manualTask)
                {
                    taskElement = CreateManualTaskElement(xmlDocument, manualTask);
                }
                else
                {
                    taskElement = CreateTaskElement(xmlDocument, task);
                }

                processElement.AppendChild(taskElement);
            }

            if (process.StartEvent != null)
            {
                XmlElement startEventElement = CreateStartEventElement(xmlDocument, process.StartEvent);
                processElement.AppendChild(startEventElement);
            }

            if (process.EndEvent != null)
            {
                XmlElement endEventElement = CreateEndEventElement(xmlDocument, process.EndEvent);
                processElement.AppendChild(endEventElement);
            }

            foreach(SequenceFlow sequenceFlow in process.GetSequenceFlows())
            {
                XmlElement sequenceFlowElement = CreateSequenceFlowElement(xmlDocument, sequenceFlow);
                processElement.AppendChild(sequenceFlowElement);
            }

            return processElement;
        }

        private XmlElement CreateUserTaskElement(XmlDocument xmlDocument, UserTask userTask)
        {
            XmlElement userTaskElement = xmlDocument.CreateElement(BPMN_XML_PREFIX, "userTask", BPMN_XML_URI);

            userTaskElement.SetAttribute("completionQuantity", "1"); //static value
            userTaskElement.SetAttribute("id", userTask.Id);
            userTaskElement.SetAttribute("messageRef", IG_XML_URI, userTask.MessageRef);
            userTaskElement.SetAttribute("implementation", "##unspecified"); //static value
            userTaskElement.SetAttribute("isForCompensation", "false"); //static value
            userTaskElement.SetAttribute("name", userTask.Name);
            userTaskElement.SetAttribute("startQuantity", "1"); //static value
            userTaskElement.AppendChild(CreateDocumentationElement(xmlDocument));

            foreach (Flow flow in userTask.IncomingFlows)
            {
                XmlElement incomingFlowElement = CreateIncomingFlowElement(xmlDocument, flow);
                userTaskElement.AppendChild(incomingFlowElement);
            }

            foreach (Flow flow in userTask.OutgoingFlows)
            {
                XmlElement outgoingFlowElement = CreateOutgoingFlowElement(xmlDocument, flow);
                userTaskElement.AppendChild(outgoingFlowElement);
            }

            return userTaskElement;
        }

        private XmlElement CreateReceiveTaskElement(XmlDocument xmlDocument, ReceiveTask receiveTask)
        {
            XmlElement receiveTaskElement = CreateGenericTaskElement(xmlDocument, taskTypeName: "receiveTask", receiveTask);
            receiveTaskElement.SetAttribute("implementation", "##WebService"); //static value
            //TODO: BPMN - For ReceiveTask is instantiate always false
            receiveTaskElement.SetAttribute("instantiate", "false"); //static value

            return receiveTaskElement;
        }

        private XmlElement CreateScriptTaskElement(XmlDocument xmlDocument, ScriptTask scriptTask)
        {
            XmlElement scriptTaskElement = CreateGenericTaskElement(xmlDocument, taskTypeName: "scriptTask", scriptTask);

            return scriptTaskElement;
        }

        private XmlElement CreateSendTaskElement(XmlDocument xmlDocument, SendTask sendTask)
        {
            XmlElement sendTaskElement = CreateGenericTaskElement(xmlDocument, taskTypeName: "sendTask", sendTask);
            sendTaskElement.SetAttribute("implementation", "##WebService"); //static value

            return sendTaskElement;
        }

        private XmlElement CreateServiceTaskElement(XmlDocument xmlDocument, ServiceTask serviceTask)
        {
            XmlElement serviceTaskElement = CreateGenericTaskElement(xmlDocument, taskTypeName: "serviceTask", serviceTask);
            serviceTaskElement.SetAttribute("implementation", "##WebService"); //static value

            return serviceTaskElement;
        }

        private XmlElement CreateManualTaskElement(XmlDocument xmlDocument, ManualTask manualTask)
        {
            XmlElement manualTaskElement = CreateGenericTaskElement(xmlDocument, taskTypeName: "manualTask", manualTask);

            return manualTaskElement;
        }

        private XmlElement CreateTaskElement(XmlDocument xmlDocument, Task task)
        {
            XmlElement taskElement = xmlDocument.CreateElement(BPMN_XML_PREFIX, "task", BPMN_XML_URI);

            taskElement.SetAttribute("completionQuantity", "1"); //static value
            taskElement.SetAttribute("id", task.Id);
            taskElement.SetAttribute("isForCompensation", "false"); //static value
            taskElement.SetAttribute("name", task.Name);
            taskElement.SetAttribute("startQuantity", "1"); //static value
            taskElement.AppendChild(CreateDocumentationElement(xmlDocument));

            foreach(Flow flow in task.IncomingFlows)
            {
                XmlElement incomingFlowElement = CreateIncomingFlowElement(xmlDocument, flow);
                taskElement.AppendChild(incomingFlowElement);
            }

            foreach(Flow flow in task.OutgoingFlows)
            {
                XmlElement outgoingFlowElement = CreateOutgoingFlowElement(xmlDocument, flow);
                taskElement.AppendChild(outgoingFlowElement);
            }

            return taskElement;
        }

        private XmlElement CreateGenericTaskElement(XmlDocument xmlDocument, string taskTypeName, Task task)
        {
            XmlElement taskElement = xmlDocument.CreateElement(BPMN_XML_PREFIX, taskTypeName, BPMN_XML_URI);

            taskElement.SetAttribute("completionQuantity", "1"); //static value
            taskElement.SetAttribute("id", task.Id);
            taskElement.SetAttribute("isForCompensation", "false"); //static value
            taskElement.SetAttribute("name", task.Name);
            taskElement.SetAttribute("startQuantity", "1"); //static value
            taskElement.AppendChild(CreateDocumentationElement(xmlDocument));

            foreach (Flow flow in task.IncomingFlows)
            {
                XmlElement incomingFlowElement = CreateIncomingFlowElement(xmlDocument, flow);
                taskElement.AppendChild(incomingFlowElement);
            }

            foreach (Flow flow in task.OutgoingFlows)
            {
                XmlElement outgoingFlowElement = CreateOutgoingFlowElement(xmlDocument, flow);
                taskElement.AppendChild(outgoingFlowElement);
            }

            return taskElement;
        }

        private XmlElement CreateIncomingFlowElement(XmlDocument xmlDocument, Flow incomingFlow)
        {
            XmlElement incomingFlowElement = xmlDocument.CreateElement(BPMN_XML_PREFIX, "incoming", BPMN_XML_URI);

            XmlText idText = xmlDocument.CreateTextNode(incomingFlow.Id);
            incomingFlowElement.AppendChild(idText);

            return incomingFlowElement;
        }

        private XmlElement CreateOutgoingFlowElement(XmlDocument xmlDocument, Flow outgoingFlow)
        {
            XmlElement outgoingFlowElement = xmlDocument.CreateElement(BPMN_XML_PREFIX, "outgoing", BPMN_XML_URI);

            XmlText idText = xmlDocument.CreateTextNode(outgoingFlow.Id);
            outgoingFlowElement.AppendChild(idText);

            return outgoingFlowElement;
        }

        private XmlElement CreateStartEventElement(XmlDocument xmlDocument, StartEvent startEvent)
        {
            XmlElement startEventElement = xmlDocument.CreateElement(BPMN_XML_PREFIX, "startEvent", BPMN_XML_URI);

            startEventElement.SetAttribute("id", startEvent.Id);
            startEventElement.SetAttribute("isInterrupting", "true"); //static value
            startEventElement.SetAttribute("name", startEvent.Name);
            startEventElement.SetAttribute("parallelMultiple", "false"); //static value
            startEventElement.AppendChild(CreateDocumentationElement(xmlDocument));

            foreach(Flow outgoingFlow in startEvent.OutgoingFlows)
            {
                XmlElement outgoingFlowElement = CreateOutgoingFlowElement(xmlDocument, outgoingFlow);
                startEventElement.AppendChild(outgoingFlowElement);
            }

            return startEventElement;
        }

        private XmlElement CreateEndEventElement(XmlDocument xmlDocument, EndEvent endEvent)
        {
            XmlElement endEventElement = xmlDocument.CreateElement(BPMN_XML_PREFIX, "endEvent", BPMN_XML_URI);

            endEventElement.SetAttribute("id", endEvent.Id);
            endEventElement.SetAttribute("name", endEvent.Name);
            endEventElement.AppendChild(CreateDocumentationElement(xmlDocument));

            foreach (Flow incomingFlow in endEvent.IncomingFlows)
            {
                XmlElement incomingFlowElement = CreateIncomingFlowElement(xmlDocument, incomingFlow);
                endEventElement.AppendChild(incomingFlowElement);
            }

            if (endEvent.EndEventType == EndEventType.Error)
            {
                XmlElement errorEventDefinitionElement = xmlDocument.CreateElement(BPMN_XML_PREFIX, "errorEventDefinition", BPMN_XML_URI);
                endEventElement.AppendChild(errorEventDefinitionElement);
            }
            else if(endEvent.EndEventType == EndEventType.MessageSend)
            {
                XmlElement messageEventDefinitionElement = xmlDocument.CreateElement(BPMN_XML_PREFIX, "messageEventDefinition", BPMN_XML_URI);
                endEventElement.AppendChild(messageEventDefinitionElement);
            }
            else if(endEvent.EndEventType == EndEventType.Termination)
            {
                XmlElement terminateEventDefinitionElement = xmlDocument.CreateElement(BPMN_XML_PREFIX, "terminateEventDefinition", BPMN_XML_URI);
                endEventElement.AppendChild(terminateEventDefinitionElement);
            }

            return endEventElement;
        }

        private XmlElement CreateSequenceFlowElement(XmlDocument xmlDocument, SequenceFlow sequenceFlow)
        {
            XmlElement sequenceFlowElement = xmlDocument.CreateElement(BPMN_XML_PREFIX, "sequenceFlow", BPMN_XML_URI);

            sequenceFlowElement.SetAttribute("id", sequenceFlow.Id);
            sequenceFlowElement.SetAttribute("name", sequenceFlow.Name);
            sequenceFlowElement.SetAttribute("sourceRef", sequenceFlow.Source.Id);
            sequenceFlowElement.SetAttribute("targetRef", sequenceFlow.Target.Id);
            sequenceFlowElement.AppendChild(CreateDocumentationElement(xmlDocument));

            return sequenceFlowElement;
        }

        private XmlElement CreateDocumentationElement(XmlDocument xmlDocument)
        {
            XmlElement documentationElement = xmlDocument.CreateElement(BPMN_XML_PREFIX, "documentation", BPMN_XML_URI);

            documentationElement.SetAttribute("textFormat", "text/plain");

            return documentationElement;
        }
    }
}
