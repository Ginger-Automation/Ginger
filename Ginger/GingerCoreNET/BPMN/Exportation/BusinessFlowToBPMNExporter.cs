using Amdocs.Ginger.CoreNET.BPMN.Conversion;
using Amdocs.Ginger.CoreNET.BPMN.Models;
using Amdocs.Ginger.CoreNET.BPMN.Serialization;
using GingerCore;
using GingerCore.Activities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.BPMN.Exportation
{
    public sealed class BusinessFlowToBPMNExporter
    {
        private readonly BusinessFlow _businessFlow;
        private readonly string _exportPath;

        public BusinessFlowToBPMNExporter(BusinessFlow businessFlow, string exportPath)
        {
            _businessFlow = businessFlow;
            _exportPath = exportPath;
        }

        public string Export()
        {
            List<BPMNFileData> bpmnFiles = new();

            BPMNFileData businessFlowBPMNFileData = CreateBusinessFlowBPMNFileData(_businessFlow);
            bpmnFiles.Add(businessFlowBPMNFileData);

            foreach (ActivitiesGroup activityGroup in _businessFlow.ActivitiesGroups)
            {
                BPMNFileData activityGroupBPMNFileData = CreateActivityGroupBPMNFileData(activityGroup);
                bpmnFiles.Add(activityGroupBPMNFileData);
            }

            string zipFilePath = CreateReleaseZIP(bpmnFiles);
            return zipFilePath;
        }

        private BPMNFileData CreateBusinessFlowBPMNFileData(BusinessFlow businessFlow)
        {
            Collaboration businessFlowCollaboration = CreateCollaborationFromBusinessFlow(businessFlow);
            string businessFlowCollaborationXML = SerializeCollaborationToXML(businessFlowCollaboration);
            string businessFlowCollaborationBPMNFileName = $"usecase-{businessFlow.Guid}.bpmn";
            return new BPMNFileData(businessFlowCollaborationBPMNFileName, businessFlowCollaborationXML);
        }

        private BPMNFileData CreateActivityGroupBPMNFileData(ActivitiesGroup activityGroup)
        {
            Collaboration activityGroupCollaboration = CreateCollaborationFromActivityGroup(activityGroup);
            string activityGroupCollaborationXML = SerializeCollaborationToXML(activityGroupCollaboration);
            string activityGroupCollaborationBPMNFileName = $"subprocess-{activityGroup.Guid}.bpmn";
            return new BPMNFileData(activityGroupCollaborationBPMNFileName, activityGroupCollaborationXML);
        }

        private Collaboration CreateCollaborationFromBusinessFlow(BusinessFlow businessFlow)
        {
            CollaborationFromBusinessFlowCreator collaborationFromBusinessFlowCreator = new(businessFlow);
            Collaboration businessFlowCollaboration = collaborationFromBusinessFlowCreator.Create();
            return businessFlowCollaboration;
        }

        private string SerializeCollaborationToXML(Collaboration collaboration)
        {
            BPMNXMLSerializer serializer = new();
            string xml = serializer.Serialize(collaboration);
            return xml;
        }

        private Collaboration CreateCollaborationFromActivityGroup(ActivitiesGroup activityGroup)
        {
            CollaborationFromActivityGroupCreator collaborationFromActivityGroupCreator = new(activityGroup);
            Collaboration activityGroupCollaboration = collaborationFromActivityGroupCreator.Create();
            return activityGroupCollaboration;
        }

        private string CreateReleaseZIP(IEnumerable<BPMNFileData> bpmnFiles)
        {
            string zipDirectoryPath = CreateReleaseZIPDirectory(bpmnFiles);
            string zipFilePath = $"{zipDirectoryPath}.zip";
            if(File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }
            ZipFile.CreateFromDirectory(zipDirectoryPath, zipFilePath);
            Directory.Delete(zipDirectoryPath, recursive: true);
            return zipFilePath;
        }

        private string CreateReleaseZIPDirectory(IEnumerable<BPMNFileData> bpmnFiles)
        {
            string zipDirectoryRootPath = Path.Combine(_exportPath, _businessFlow.Name);
            string zipDirectoryPath = Path.Combine(zipDirectoryRootPath, "requirements-library");
            if (Directory.Exists(zipDirectoryPath))
            {
                Directory.Delete(zipDirectoryPath, recursive: true);
            }
            Directory.CreateDirectory(zipDirectoryPath);

            foreach (BPMNFileData bpmnFile in bpmnFiles)
            {
                string filePath = Path.Combine(zipDirectoryPath, bpmnFile.Name);
                File.WriteAllText(filePath, bpmnFile.Content);
            }
            return zipDirectoryRootPath;
        }

        private sealed class BPMNFileData
        {
            internal string Name { get; }

            internal string Content { get; }

            internal BPMNFileData(string name, string content)
            {
                Name = name;
                Content = content;
            }
        }
    }
}
