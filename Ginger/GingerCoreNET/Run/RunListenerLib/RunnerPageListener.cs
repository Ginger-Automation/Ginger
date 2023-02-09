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

using Amdocs.Ginger.Run;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using System;

namespace Amdocs.Ginger.CoreNET.Run.RunListenerLib
{
    public class RunnerPageListener : RunListenerBase
    {
        public EventHandler UpdateStat;

        public EventHandler UpdateBusinessflowActivities;

        public override void GiveUserFeedback(uint eventTime)
        {          
            UpdateStat.Invoke(this, null);
        }
        public override void ActionStart(uint eventTime, Act action)
        {
            UpdateStat.Invoke(this, null);
        }
        public override void ActionEnd(uint eventTime, Act action, bool offlineMode = false)
        {
            UpdateStat.Invoke(this, null);
        }

        public override void BusinessFlowStart(uint eventTime, BusinessFlow businessFlow, bool ContinueRun = false)
        {
            UpdateStat.Invoke(this, null);
        }

        public override void BusinessFlowEnd(uint eventTime, BusinessFlow businessFlow, bool offlineMode = false)
        {
            UpdateStat.Invoke(this, null);
        }

        public override void BusinessflowWasReset(uint eventTime, BusinessFlow businessFlow)
        {
            UpdateBusinessflowActivities.Invoke(businessFlow, null);
        }

        public override void BusinessFlowSkipped(uint eventTime, BusinessFlow businessFlow, bool offlineMode = false)
        {
            UpdateStat.Invoke(this, null);
        }

        public override void ActivityStart(uint eventTime, Activity activity, bool continuerun=false)
        {
            UpdateStat.Invoke(this, null);
        }

        public override void ActivityEnd(uint eventTime, Activity activity, bool offlineMode= false)
        {
            UpdateStat.Invoke(this, null);
        }

        public override void RunnerRunStart(uint eventTime, GingerRunner gingerRunner, bool offlineMode = false)
        {
            UpdateStat.Invoke(this, null);
        }

        public override void RunnerRunEnd(uint eventTime, GingerRunner gingerRunner, string filename = null, int runnerCount = 0, bool offlineMode = false)
        {
            UpdateStat.Invoke(this, null);
        }

        public override void DynamicActivityWasAddedToBusinessflow(uint eventTime, BusinessFlow businessFlow)
        {            
            UpdateBusinessflowActivities.Invoke(businessFlow, null);
        }
       
    }
}
