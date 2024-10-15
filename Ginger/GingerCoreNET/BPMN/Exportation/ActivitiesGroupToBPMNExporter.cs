#region License
/*
Copyright © 2014-2024 European Support Limited

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

using Amdocs.Ginger.CoreNET.BPMN.Conversion;
using Amdocs.Ginger.CoreNET.BPMN.Models;
using Amdocs.Ginger.CoreNET.BPMN.Serialization;
using GingerCore.Activities;
using GingerUtils;
using System.IO;

#nullable enable
namespace Amdocs.Ginger.CoreNET.BPMN.Exportation
{
    public sealed class ActivitiesGroupToBPMNExporter
    {
        private readonly ActivitiesGroup _activityGroup;
        private readonly CollaborationFromActivityGroupCreator.Options? _activityGroupCollaborationOptions;
        private readonly string _exportPath;

        public ActivitiesGroupToBPMNExporter(ActivitiesGroup activityGroup, string exportPath)
        {
            _activityGroup = activityGroup;
            _activityGroupCollaborationOptions = null;
            _exportPath = exportPath;
        }

        public ActivitiesGroupToBPMNExporter(ActivitiesGroup activityGroup, CollaborationFromActivityGroupCreator.Options activityGroupCollaborationOptions, string exportPath)
        {
            _activityGroup = activityGroup;
            _activityGroupCollaborationOptions = activityGroupCollaborationOptions;
            _exportPath = exportPath;
        }

        public string Export()
        {
            BPMNFileData activityGroupBPMNFileData = CreateActivityGroupBPMNFileData();
            string filePath = CreateBPMNFile(activityGroupBPMNFileData);
            return filePath;
        }

        private BPMNFileData CreateActivityGroupBPMNFileData()
        {
            Collaboration activityGroupCollaboration = CreateCollaborationFromActivityGroup();
            string activityGroupCollaborationXML = SerializeCollaborationToXML(activityGroupCollaboration);
            string activityGroupCollaborationBPMNFileName = $"subprocess-{_activityGroup.Name}.bpmn";
            return new BPMNFileData(activityGroupCollaborationBPMNFileName, activityGroupCollaborationXML);
        }

        private string SerializeCollaborationToXML(Collaboration collaboration)
        {
            BPMNXMLSerializer serializer = new();
            string xml = serializer.Serialize(collaboration);
            return xml;
        }

        private Collaboration CreateCollaborationFromActivityGroup()
        {
            CollaborationFromActivityGroupCreator collaborationFromActivityGroupCreator;
            if (_activityGroupCollaborationOptions != null)
            {
                collaborationFromActivityGroupCreator = new(_activityGroup, _activityGroupCollaborationOptions);
            }
            else
            {
                collaborationFromActivityGroupCreator = new(_activityGroup);
            }
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

            filePath = FileUtils.GetUniqueFilePath(filePath);

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
