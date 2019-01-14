//using System;
//using System.Collections.Generic;
//using System.Text;
//
//using Ginger.Reports;
//using Ginger.Run;
//using GingerCore;
//using GingerCore.Activities;

//namespace Amdocs.Ginger.Common.InterfacesLib
//{
//    public interface IExecutionLogger
//    {
//        string ExecutionLogfolder { get; set; }
//        ParentGingerData GingerData { get; set; }
//        DateTime CurrentExecutionDateTime { get; set; }
//       ExecutionLoggerConfiguration Configuration { get; set; }
        
//        void GingerStart();
//        void ActivityGroupStart(ActivitiesGroup currentActivityGroup, BusinessFlow currentBusinessFlow);
//        void ActivityStart(BusinessFlow currentBusinessFlow,Activity activity);
//        void ActivityEnd(BusinessFlow BusinessFlow, Activity Activity, bool offlineMode = false);
//        void ActionEnd(Activity Activity, IAct act, bool offlineMode = false);
//        void ActivityGroupEnd(ActivitiesGroup currentActivityGroup, BusinessFlow currentBusinessFlow, bool offlineMode);
//        void BusinessFlowEnd(BusinessFlow currentBusinessFlow, bool offlineMode = false);
//        void ActionStart(BusinessFlow currentBusinessFlow,Activity currentActivity, IAct act);
//        void GingerEnd(GingerRunner GR = null, string filename = null, int runnerCount = 0);
//    }
//}
