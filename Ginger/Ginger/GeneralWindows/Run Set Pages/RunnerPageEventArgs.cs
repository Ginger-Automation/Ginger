using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.Run
{
    public class RunnerPageEventArgs
    {
        public enum eEventType
        {
            RemoveRunner,
            DuplicateRunner,
            ResetRunnerStatus
        }

        public eEventType EventType;
        public Object Object;

        public RunnerPageEventArgs(eEventType EventType, object Object)
        {
            this.EventType = EventType;
            this.Object = Object;
        }
    }
}
