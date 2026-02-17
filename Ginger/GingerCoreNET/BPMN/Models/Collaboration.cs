#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace Amdocs.Ginger.CoreNET.BPMN.Models
{
    public sealed class Collaboration
    {
        public string Guid { get; }

        public string Id { get; }

        public string Name { get; set; }

        public string SystemRef { get; set; }

        public string Description { get; set; }

        public CollaborationType CollaborationType { get; }

        private readonly ICollection<Participant> _participants;
        public IEnumerable<Participant> Participants => _participants;

        private Collaboration(string guid, CollaborationType collaborationType)
        {
            Guid = guid;
            Id = $"Collaboration_{Guid}";
            Name = string.Empty;
            SystemRef = string.Empty;
            Description = string.Empty;
            CollaborationType = collaborationType;
            _participants = [];
        }

        public static Collaboration CreateForSubProcess(Guid guid, string systemRef)
        {
            return CreateForSubProcess(guid.ToString(), systemRef);
        }

        public static Collaboration CreateForSubProcess(string guid, string systemRef)
        {
            return new Collaboration(guid, CollaborationType.SubProcess)
            {
                SystemRef = systemRef
            };
        }

        public static Collaboration CreateForUseCase(Guid guid)
        {
            return CreateForUseCase(guid.ToString());
        }

        public static Collaboration CreateForUseCase(string guid)
        {
            return new Collaboration(guid, CollaborationType.UseCase);
        }

        public void AddParticipant(Participant participant)
        {
            bool isUnique = _participants.All(p => p.Guid != participant.Guid);
            if (isUnique)
            {
                _participants.Add(participant);
            }
        }

        public Participant? GetParticipantByGuid(Guid participantGuid)
        {
            Participant? participant = Participants
                .FirstOrDefault(participant => participant.Guid == participantGuid);
            return participant;
        }

        public IEnumerable<MessageFlow> GetMessageFlows()
        {
            IEnumerable<MessageFlow> messageFlows = Participants
                .SelectMany(participant => participant.Process.GetFlows())
                .Where(flow => flow is MessageFlow)
                .Select(flow => (MessageFlow)flow)
                .Distinct();

            return messageFlows;
        }
    }
}
