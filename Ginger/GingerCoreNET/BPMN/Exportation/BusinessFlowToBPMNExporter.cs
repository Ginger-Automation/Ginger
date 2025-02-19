#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.CoreNET.BPMN.Exceptions;
using Amdocs.Ginger.CoreNET.BPMN.Models;
using Amdocs.Ginger.CoreNET.BPMN.Serialization;
using GingerCore;
using GingerCore.Activities;
using GingerUtils;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

#nullable enable
namespace Amdocs.Ginger.CoreNET.BPMN.Exportation
{
    public sealed class BusinessFlowToBPMNExporter
    {
        public sealed class Options
        {
            public CollaborationFromActivityGroupCreator.Options GroupConversionOptions { get; init; } = new();

            public bool IgnoreGroupWithNoValidActivity { get; init; } = false;

            public required string ExportPath { get; init; }
        }

        private readonly BusinessFlow _businessFlow;
        private readonly Options _options;

        public BusinessFlowToBPMNExporter(BusinessFlow businessFlow, Options options)
        {
            _businessFlow = businessFlow;
            _options = options;
        }

        public string Export()
        {
            List<BPMNFileData> bpmnFiles = [];

            BPMNFileData businessFlowBPMNFileData = CreateBusinessFlowBPMNFileData(_businessFlow);
            bpmnFiles.Add(businessFlowBPMNFileData);

            foreach (ActivitiesGroup activityGroup in _businessFlow.ActivitiesGroups)
            {
                BPMNFileData? activityGroupBPMNFileData = CreateActivityGroupBPMNFileData(activityGroup);
                if (activityGroupBPMNFileData == null)
                {
                    continue;
                }
                bpmnFiles.Add(activityGroupBPMNFileData);
            }

            string zipFilePath = CreateReleaseZIP(bpmnFiles);
            return zipFilePath;
        }

        private BPMNFileData CreateBusinessFlowBPMNFileData(BusinessFlow businessFlow)
        {
            Collaboration businessFlowCollaboration = CreateCollaborationFromBusinessFlow(businessFlow);
            string businessFlowCollaborationXML = SerializeCollaborationToXML(businessFlowCollaboration);
            string businessFlowCollaborationBPMNFileName = $"usecase-{businessFlow.Name}.bpmn";
            return new BPMNFileData(businessFlowCollaborationBPMNFileName, businessFlowCollaborationXML);
        }

        private BPMNFileData? CreateActivityGroupBPMNFileData(ActivitiesGroup activityGroup)
        {
            Collaboration? activityGroupCollaboration = CreateCollaborationFromActivityGroup(activityGroup);
            if (activityGroupCollaboration == null)
            {
                return null;
            }

            string activityGroupCollaborationXML = SerializeCollaborationToXML(activityGroupCollaboration);
            string activityGroupCollaborationBPMNFileName = $"subprocess-{activityGroup.Name}.bpmn";
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

        private Collaboration? CreateCollaborationFromActivityGroup(ActivitiesGroup activityGroup)
        {
            bool isEmpty = activityGroup.ActivitiesIdentifiers.Count == 0;
            if (isEmpty)
            {
                return null;
            }

            CollaborationFromActivityGroupCreator collaborationFromActivityGroupCreator = new(activityGroup, _options.GroupConversionOptions);
            try
            {
                return collaborationFromActivityGroupCreator.Create();
            }
            catch (NoValidActivityFoundInGroupException)
            {
                if (_options.IgnoreGroupWithNoValidActivity)
                {
                    return null;
                }
                throw;
            }
        }

        private string CreateReleaseZIP(IEnumerable<BPMNFileData> bpmnFiles)
        {
            string zipDirectoryPath = CreateReleaseZIPDirectory(bpmnFiles);
            string zipFilePath = $"{zipDirectoryPath}.zip";
            zipFilePath = FileUtils.GetUniqueFilePath(zipFilePath);
            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }
            ZipFile.CreateFromDirectory(zipDirectoryPath, zipFilePath);
            Directory.Delete(zipDirectoryPath, recursive: true);
            return zipFilePath;
        }

        private string CreateReleaseZIPDirectory(IEnumerable<BPMNFileData> bpmnFiles)
        {
            string zipDirectoryRootPath = Path.Combine(_options.ExportPath, _businessFlow.Name);

            zipDirectoryRootPath = FileUtils.GetUniqueDirectoryPath(zipDirectoryRootPath);

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
