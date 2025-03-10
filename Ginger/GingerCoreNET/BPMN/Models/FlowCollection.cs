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

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Amdocs.Ginger.CoreNET.BPMN.Models
{
    public sealed class FlowCollection : IEnumerable<Flow>
    {
        private readonly List<Flow> _items;

        public FlowCollection()
        {
            _items = [];
        }

        public bool Add(Flow flow)
        {
            if (_items.Any(item => string.Equals(item.Id, flow.Id)))
            {
                return false;
            }
            _items.Add(flow);
            return true;
        }

        public IEnumerator<Flow> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
