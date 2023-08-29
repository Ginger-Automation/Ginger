using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.BPMN
{
    public sealed class FlowCollection : IEnumerable<Flow>
    {
        private readonly List<Flow> _items;

        public FlowCollection()
        {
            _items = new();
        }

        public bool Add(Flow flow)
        {
            if (_items.Any(item => string.Equals(item.Id, flow.Id)))
                return false;

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
