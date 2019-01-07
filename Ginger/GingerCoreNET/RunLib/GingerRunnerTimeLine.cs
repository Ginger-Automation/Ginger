using System;
using System.Collections.Generic;
using System.Text;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCoreNET.RunLib;
using GingerUtils.TimeLine;

namespace Amdocs.Ginger.CoreNET.RunLib
{
    public class GingerRunnerTimeLine
    {


        public TimeLineEvents timeLineEvents = new TimeLineEvents();
        TimeLineEvent BFTimeLineEvent;

        TimeLineEvent ActionTimeLineEvent = null;

        internal void Start(GingerRunner gingerRunner)
        {
            //if (WorkSpace.Instance.LogTimeLineEvents)
                gingerRunner.GingerRunnerEvent += HandleEvent;
            
            TimeLineEvents.Stopwatch.Start();

        }

        private void HandleEvent(GingerRunnerEventArgs EventArgs)
        {

            switch(EventArgs.EventType)
            {
                case GingerRunnerEventArgs.eEventType.BusinessFlowStart:
                    BusinessFlowStart((BusinessFlow)EventArgs.Object);
                    break;
                case GingerRunnerEventArgs.eEventType.BusinessFlowEnd:
                    BusinessFlowEnd((BusinessFlow)EventArgs.Object);
                    break;                
                case GingerRunnerEventArgs.eEventType.ActivityStart:
                    ActivityStart((Activity)EventArgs.Object);
                    break;
                case GingerRunnerEventArgs.eEventType.ActivityEnd:
                    ActivityEnd((Activity)EventArgs.Object);
                    break;
                case GingerRunnerEventArgs.eEventType.ActionStart:
                    ActionStart((Act)EventArgs.Object);
                    break;
                case GingerRunnerEventArgs.eEventType.ActionEnd:
                    ActionEnd((Act)EventArgs.Object);
                    break;
                

            }
        }


        private void BusinessFlowStart(BusinessFlow businessFlow)
        {                        
            BFTimeLineEvent = TimeLineEvent.StartNew("BusinessFlow", businessFlow.Name);
            timeLineEvents.AddEvent(BFTimeLineEvent);
        }

        private void BusinessFlowEnd(BusinessFlow businessFlow)
        {
            BFTimeLineEvent.Stop();

            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            timeLineEvents.SaveTofile(@"c:\temp\Ginger\BFTimeLine.txt");
        }

        #region PrepAction

        TimeLineEvent PrepActionTimeLineEvent;
        private void PrepActionStart(Act act)
        {
            PrepActionTimeLineEvent = TimeLineEvent.StartNew("Prep Action", act.Description);
            ActionTimeLineEvent.AddSubEvent(PrepActionTimeLineEvent);            
        }

        #endregion PrepAction

        private void PrepActionEnd(Act act)
        {
            PrepActionTimeLineEvent.Stop();
        }

        TimeLineEvent ActivityTimeLineEvent;
        private void ActivityStart(Activity activity)
        {
            ActivityTimeLineEvent = TimeLineEvent.StartNew("Activity", activity.ActivityName);
            BFTimeLineEvent.AddSubEvent(ActivityTimeLineEvent);
        }

        private void ActivityEnd(Activity activity)
        {
            //TODO: verify the same activity
            ActivityTimeLineEvent.Stop();
        }

        #region Action
        private void ActionStart(Act act)
        {
            ActionTimeLineEvent = TimeLineEvent.StartNew("Action", act.Description);
            if (ActivityTimeLineEvent != null)
            {
                ActivityTimeLineEvent.AddSubEvent(ActionTimeLineEvent);
            }                        
        }
        

        private void ActionEnd(Act @object)
        {            
            ActionTimeLineEvent.Stop();            
        }

        #endregion Action


    }
}
