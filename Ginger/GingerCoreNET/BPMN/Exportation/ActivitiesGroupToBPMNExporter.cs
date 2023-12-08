using Amdocs.Ginger.CoreNET.BPMN.Conversion;
using Amdocs.Ginger.CoreNET.BPMN.Models;
using Amdocs.Ginger.CoreNET.BPMN.Serialization;
using GingerCore.Activities;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Amdocs.Ginger.CoreNET.BPMN.Exportation
{
    public sealed class ActivitiesGroupToBPMNExporter
    {
        private readonly ActivitiesGroup _activityGroup;
        private readonly string _exportPath;

        public ActivitiesGroupToBPMNExporter(ActivitiesGroup activityGroup, string exportPath)
        {
            _activityGroup = activityGroup;
            _exportPath = exportPath;
        }

        public string Export()
        {
            BPMNFileData activityGroupBPMNFileData = CreateActivityGroupBPMNFileData(_activityGroup);
            string filePath = CreateBPMNFile(activityGroupBPMNFileData);
            return filePath;
        }

        private BPMNFileData CreateActivityGroupBPMNFileData(ActivitiesGroup activityGroup)
        {
            Collaboration activityGroupCollaboration = CreateCollaborationFromActivityGroup(activityGroup);
            string activityGroupCollaborationXML = SerializeCollaborationToXML(activityGroupCollaboration);
            string activityGroupCollaborationBPMNFileName = $"subprocess-{activityGroup.Guid}.bpmn";
            return new BPMNFileData(activityGroupCollaborationBPMNFileName, activityGroupCollaborationXML);
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

        private string CreateBPMNFile(BPMNFileData bpmnFile)
        {
            if (!Directory.Exists(_exportPath))
            {
                Directory.CreateDirectory(_exportPath);
            }
            string filePath = Path.Combine(_exportPath, bpmnFile.Name);
            File.WriteAllText(filePath, bpmnFile.Content);
            return filePath;
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
