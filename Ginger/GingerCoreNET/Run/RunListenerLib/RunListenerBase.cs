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

using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.Environments;
using System;
using static Ginger.Reports.ExecutionLoggerConfiguration;
using Activity = GingerCore.Activity;

namespace Amdocs.Ginger.Run
{
    public class RunListenerBase : IRunListenerBase
    {
        // Stopwatch for all listeners to have the same start point reference for event time 
        static readonly System.Diagnostics.Stopwatch mStopwatch = new System.Diagnostics.Stopwatch();
        static DateTime mStartDateTime = new DateTime();

        public static void Start()
        {
            mStopwatch.Reset();
            mStopwatch.Start();
            mStartDateTime = DateTime.Now;
        }

        public static DateTime GetDateTime(uint eventTime)
        {
            return mStartDateTime.AddMilliseconds(eventTime);
        }

        internal static uint GetEventTime()
        {
            return (uint)mStopwatch.ElapsedMilliseconds;
        }


        #region General
        public virtual void GiveUserFeedback(uint eventTime)
        {

        }
        #endregion General

        #region Runner
        public virtual void RunnerRunStart(uint eventTime, GingerRunner gingerRunner, bool offlineMode = false)
        {

        }

        public virtual void RunnerRunEnd(uint eventTime, GingerRunner gingerRunner, string filename = null, int runnerCount = 0, bool offlineMode = false)
        {

        }

        #endregion Runner

        #region BusinessFlow
        public virtual void BusinessFlowStart(uint eventTime, BusinessFlow businessFlow, bool ContinueRun = false)
        {

        }
        public virtual void BusinessFlowEnd(uint eventTime, BusinessFlow businessFlow, bool offlineMode = false)
        {

        }

        public virtual void BusinessflowWasReset(uint eventTime, BusinessFlow businessFlow)
        {

        }
        #endregion BusinessFlow


        #region Activity
        public virtual void ActivityStart(uint eventTime, Activity activity, bool continuerun = false)
        {

        }
        public virtual void ActivityEnd(uint eventTime, Activity activity, bool offlineMode = false)
        {

        }

        public virtual void DynamicActivityWasAddedToBusinessflow(uint eventTime, BusinessFlow businessFlow)
        {

        }
        #endregion Activity


        #region Action
        public virtual void ActionStart(uint eventTime, Act action)
        {

        }
        public virtual void ActionEnd(uint eventTime, Act action, bool offlineMode = false)
        {

        }

        public virtual void PrepActionStart(uint eventTime, Act action)
        {

        }

        public virtual void PrepActionEnd(uint eventTime, Act action)
        {

        }


        // If user put some seconds to wait before runnign the action
        public virtual void ActionWaitStart(uint eventTime, Act action)
        {

        }
        public virtual void ActionWaitEnd(uint eventTime, Act action)
        {

        }

        public virtual void ActionUpdatedStart(uint eventTime, Act action)
        {

        }

        public virtual void ActionUpdatedEnd(uint eventTime, Act action)
        {

        }


        public virtual void EnvironmentChanged(uint eventTime, ProjEnvironment mProjEnvironment)
        {

        }

        public virtual void ActivityGroupStart(uint eventTime, ActivitiesGroup activityGroup)
        {

        }

        public virtual void ActivityGroupEnd(uint eventTime, ActivitiesGroup activityGroup, bool offlineMode = false)
        {

        }

        public virtual void ActivityGroupSkipped(uint eventTime, ActivitiesGroup activityGroup, bool offlineMode = false)
        {

        }

        public virtual void ActivitySkipped(uint eventTime, Activity activity, bool offlineMode = false)
        {

        }

        public virtual void BusinessFlowSkipped(uint eventTime, BusinessFlow businessFlow, bool offlineMode = false)
        {

        }


        /// <summary>
        /// Notify the context of execution: Business Flow, Activity, Action
        /// </summary>
        /// <param name="eventTime"></param>
        /// <param name="automationTabContext"></param>
        public virtual void ExecutionContext(uint eventTime, AutomationTabContext automationTabContext, BusinessFlow businessFlow)
        {

        }
        #endregion Action


        //enum aa
        //{
        //    UserClickRunFlow
        //        BreakPoint
        //        User clicked Contrine
        //}

        //public virtual void FlowInterrruptiont(uint eventTime, enum)
        //{

        //}


    }
}
