using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.BPMN
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

        public Collaboration(Guid guid, CollaborationType collaborationType) :
            this(guid.ToString(), collaborationType) { }

        public Collaboration(string guid, CollaborationType collaborationType)
        {
            Guid = guid;
            Id = $"Collaboration_{Guid}";
            Name = string.Empty;
            SystemRef = string.Empty;
            Description = string.Empty;
            CollaborationType = collaborationType;
            _participants = new List<Participant>();
        }

        public void AddParticipant(Participant participant)
        {
            _participants.Add(participant);
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
