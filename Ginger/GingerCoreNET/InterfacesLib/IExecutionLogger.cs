using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.CoreNET.InterfacesLib;
using Ginger.Reports;
using Ginger.Run;

namespace Amdocs.Ginger.Common.InterfacesLib
{
    public interface IExecutionLogger
    {
        string ExecutionLogfolder { get; set; }
        ParentGingerData GingerData { get; set; }
        DateTime CurrentExecutionDateTime { get; set; }
       ExecutionLoggerConfiguration Configuration { get; set; }
        
        void GingerStart();
        void ActivityGroupStart(IActivitiesGroup currentActivityGroup, IBusinessFlow currentBusinessFlow);
        void ActivityStart(IBusinessFlow currentBusinessFlow,IActivity activity);
        void ActivityEnd(IBusinessFlow BusinessFlow, IActivity Activity, bool offlineMode = false);
        void ActionEnd(IActivity Activity, IAct act, bool offlineMode = false);
        void ActivityGroupEnd(IActivitiesGroup currentActivityGroup, IBusinessFlow currentBusinessFlow, bool offlineMode);
        void BusinessFlowEnd(IBusinessFlow currentBusinessFlow, bool offlineMode = false);
        void ActionStart(IBusinessFlow currentBusinessFlow,IActivity currentActivity, IAct act);
        void GingerEnd(IGingerRunner GR = null, string filename = null, int runnerCount = 0);
    }
}
