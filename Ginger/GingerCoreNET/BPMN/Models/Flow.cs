﻿#region License
/*
Copyright © 2014-2023 European Support Limited

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

namespace Amdocs.Ginger.CoreNET.BPMN.Models
{
    public abstract class Flow
    {
        public Guid Guid { get; }

        public string Id { get; }

        public string Name { get; set; }

        public IFlowSource Source { get; }

        public IFlowTarget Target { get; }

        protected Flow(string name, IFlowSource source, IFlowTarget target) :
            this(Guid.NewGuid(), name, source, target)
        { }

        protected Flow(Guid guid, string name, IFlowSource source, IFlowTarget target)
        {
            Guid = guid;
            Id = $"flow_{Guid}";
            Name = name;
            Source = source;
            Target = target;
        }

        public static Flow Create(string name, IFlowSource source, IFlowTarget target)
        {
            return Create(Guid.NewGuid(), name, source, target);
        }

        public static Flow Create(Guid guid, string name, IFlowSource source, IFlowTarget target)
        {
            Flow flow;
            if (string.Equals(source.ProcessId, target.ProcessId))
            {
                flow = new SequenceFlow(guid, name, source, target);
            }
            else
            {
                flow = new MessageFlow(guid, name, source, target);
            }
            source.OutgoingFlows.Add(flow);
            target.IncomingFlows.Add(flow);

            return flow;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj is not Flow)
            {
                return false;
            }

            Flow flow = (Flow)obj;

            return string.Equals(Id, flow.Id) && string.Equals(Name, flow.Name);
        }

        public override int GetHashCode()
        {
            HashCode hashCode = new();
            hashCode.Add(Id);
            hashCode.Add(Name);
            return hashCode.ToHashCode();
        }
    }
}