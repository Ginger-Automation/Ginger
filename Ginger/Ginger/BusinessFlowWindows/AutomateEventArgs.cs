using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.BusinessFlowWindows
{
    public class AutomateEventArgs
    {
        public enum eEventType
        {
            Automate,
            SetupRunnerForExecution,
            RunCurrentAction,
            RunCurrentActivity,
            ContinueActionRun,
            ContinueActivityRun,
            StopRun,
            GenerateLastExecutedItemReport,
        }

        public eEventType EventType;
        public Object Object;

        public AutomateEventArgs(eEventType EventType, object Object)
        {
            this.EventType = EventType;
            this.Object = Object;
        }
    }
}
