#region License
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

using GingerCore;
using GingerCore.Actions;
using GingerUtils.TimeLine;

namespace Amdocs.Ginger.Run
{
    public class GingerRunnerTimeLine : RunListenerBase
    {
        public TimeLineEvents timeLineEvents = new TimeLineEvents();
        TimeLineEvent BusinessFlowTimeLineEvent;
        TimeLineEvent ActivityTimeLineEvent;
        TimeLineEvent ActionTimeLineEvent = null;
        TimeLineEvent PrepActionTimeLineEvent;

        public void Reset()
        {
            timeLineEvents.Clear();
            ActivityTimeLineEvent = null;
            ActionTimeLineEvent = null;
            BusinessFlowTimeLineEvent = null;
            PrepActionTimeLineEvent = null;
        }


        public override void BusinessFlowStart(uint eventTime, BusinessFlow businessFlow, bool ContinueRun = false) 
        {
            BusinessFlowTimeLineEvent = new TimeLineEvent("BusinessFlow", businessFlow.Name, eventTime);
            timeLineEvents.AddEvent(BusinessFlowTimeLineEvent);
        }

        public override void BusinessFlowEnd(uint eventTime, BusinessFlow businessFlow, bool offlineMode = false)
        {
            BusinessFlowTimeLineEvent.End = eventTime;
        }

        
        public override void ActivityStart(uint eventTime, Activity activity,  bool continuerun = false)
        {
            ActivityTimeLineEvent = new TimeLineEvent("Activity", activity.ActivityName, eventTime);
            if (BusinessFlowTimeLineEvent != null)
            {
                BusinessFlowTimeLineEvent.AddSubEvent(ActivityTimeLineEvent);
            }
        }

        public override void ActivityEnd(uint eventTime, Activity activity, bool offlineMode= false)
        {
            //TODO: verify the same activity
            ActivityTimeLineEvent.End = eventTime;
        }

        #region Action
       

        
        public override void PrepActionStart(uint eventTime, Act action)
        {       
            PrepActionTimeLineEvent = new TimeLineEvent("Prep Action", action.Description, eventTime);
            ActivityTimeLineEvent.AddSubEvent(PrepActionTimeLineEvent);
        }

        public override void PrepActionEnd(uint eventTime, Act action)
        {
            PrepActionTimeLineEvent.End = eventTime;
        }


        public override void ActionStart(uint eventTime, Act action)
        {
            ActionTimeLineEvent = new TimeLineEvent("Action", action.Description, eventTime);
            if (ActivityTimeLineEvent != null)
            {
                ActivityTimeLineEvent.AddSubEvent(ActionTimeLineEvent);
            }
        }


        public override void ActionEnd(uint eventTime, Act action, bool offlineMode=false)
        {
            ActionTimeLineEvent.End = eventTime;
        }
        #endregion Action


    }
}
