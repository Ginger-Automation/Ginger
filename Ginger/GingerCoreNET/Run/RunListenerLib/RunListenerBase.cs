using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.Environments;
using System;
using System.Diagnostics;

namespace Amdocs.Ginger.Run
{
    public class RunListenerBase
    {
        // Stopwatch for all listeners to have the same start point reference for event time 
        static readonly Stopwatch mStopwatch = new Stopwatch();
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
        public virtual void RunnerRunStart(uint eventTime)
        {

        }

        public virtual void RunnerRunEnd(uint eventTime)
        {

        }

        #endregion Runner

        #region BusinessFlow
        public virtual void BusinessFlowStart(uint eventTime, BusinessFlow businessFlow)
        {

        }
        public virtual void BusinessFlowEnd(uint eventTime, BusinessFlow businessFlow)
        {

        }

        public virtual void BusinessflowWasReset(uint eventTime, BusinessFlow businessFlow)
        {

        }
        #endregion BusinessFlow


        #region Activity
        public virtual void ActivityStart(uint eventTime, Activity activity)
        {

        }
        public virtual void ActivityEnd(uint eventTime, Activity activity)
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
        public virtual void ActionEnd(uint eventTime, Act action)
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

        public virtual void ActivityGroupEnd(uint eventTime, ActivitiesGroup activityGroup)
        {

        }
        #endregion Action



    }
}
