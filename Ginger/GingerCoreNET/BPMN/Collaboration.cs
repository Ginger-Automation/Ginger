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

        public string Name { get; }

        public string Description { get; }

        public CollaborationType CollaborationType { get; }

        private readonly ICollection<Participant> _participants;
        public IEnumerable<Participant> Participants => _participants;

        public Collaboration(Guid guid, CollaborationType collaborationType, string name) :
            this(guid, collaborationType, name, description: string.Empty) { }

        public Collaboration(Guid guid, CollaborationType collaborationType, string name, string description) :
            this(guid.ToString(), collaborationType, name, description) { }

        public Collaboration(string guid, CollaborationType collaborationType, string name) :
            this(guid, collaborationType, name, description: string.Empty) { }

        public Collaboration(string guid, CollaborationType collaborationType, string name, string description)
        {
            Guid = guid;
            Id = $"Collaboration_{Guid}";
            Name = name;
            Description = description;
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
                .Select(flow => (MessageFlow)flow);

            return messageFlows;
        }
    }
}
