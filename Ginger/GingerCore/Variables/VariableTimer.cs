#region License
/*
Copyright © 2014-2018 European Support Limited

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

using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Threading;

namespace GingerCore.Variables
{
    public class VariableTimer : VariableBase
    {

        public Stopwatch RunWatch = new Stopwatch();
        public DispatcherTimer DispatcherTimerElapsed = new System.Windows.Threading.DispatcherTimer();


        public override string VariableUIType
        {
            get { return GingerDicser.GetTermResValue(eTermResKey.Variable) + " Timer"; }
        }


        public enum eTimerUnit
        {
            Seconds,
            MilliSeconds,
            Minutes,
            Hours
        }

        
        [IsSerializedForLocalRepository]
        public eTimerUnit TimerUnit { get; set; }

        public override string VariableEditPage { get { return  "VariableTimerPage"; } }

        public Boolean IsStopped { get; set; }

        public override string GetFormula()
        {
            return "Timer unit="+ TimerUnit.ToString();
        }

        public void StartTimer(bool isContinue=false)
        {
           
            if(isContinue==false)
                RunWatch.Reset();

            if (TimerUnit == eTimerUnit.MilliSeconds)
                DispatcherTimerElapsed.Interval = new TimeSpan(0, 0, 0, 0, 1);
            if (TimerUnit == eTimerUnit.Seconds)
                DispatcherTimerElapsed.Interval = new TimeSpan(0, 0, 0, 1, 0);
            if (TimerUnit == eTimerUnit.Minutes)
                DispatcherTimerElapsed.Interval = new TimeSpan(0, 0, 1, 0, 0);
            if (TimerUnit == eTimerUnit.Hours)
                DispatcherTimerElapsed.Interval = new TimeSpan(0, 1, 0, 0, 0);
            
            RunWatch.Start();
            DispatcherTimerElapsed.Start();
            DispatcherTimerElapsed.Tick += dispatcherTimerElapsedTick;          
        }
              

        public void StopTimer()
        {
            if (RunWatch.IsRunning)
            {
                RunWatch.Stop();
                DispatcherTimerElapsed.Stop();
                DispatcherTimerElapsed.Tick -= dispatcherTimerElapsedTick;
            }

        }

        public void ContinueTimer()
        {
            StartTimer(true);
        }


        public override void ResetValue()
        {
            if (RunWatch.IsRunning)
            {                
                RunWatch.Reset();
                DispatcherTimerElapsed.Stop();
                DispatcherTimerElapsed.Tick -= dispatcherTimerElapsedTick;
                UpdateTimervalue();
            }
            else
            {
               Value = "0";
            }
        }

        private void dispatcherTimerElapsedTick(object sender, System.EventArgs e)
        {
            if (RunWatch.IsRunning)
            {
                UpdateTimervalue();
            }
        }

        private void UpdateTimervalue()
        {

            switch (TimerUnit)
            {
                case eTimerUnit.MilliSeconds:
                    Value = "" + Math.Round(RunWatch.Elapsed.TotalMilliseconds, 2);
                    break;

                case eTimerUnit.Seconds:
                    Value = "" + Math.Round(RunWatch.Elapsed.TotalSeconds, 2);
                    break;

                case eTimerUnit.Minutes:
                    Value = "" + Math.Round(RunWatch.Elapsed.TotalMinutes, 2);
                    break;

                case eTimerUnit.Hours:
                    Value = "" + Math.Round(RunWatch.Elapsed.TotalHours, 2);
                    break;
            }

        }

        public override void GenerateAutoValue()
        {
            //NA
        }

        public override eImageType Image { get { return eImageType.Timer; } }
        public override string VariableType() { return "Timer"; }

        public override List<ActSetVariableValue.eSetValueOptions> GetSupportedOperations()
        {
            List<ActSetVariableValue.eSetValueOptions> supportedOperations = new List<ActSetVariableValue.eSetValueOptions>();
            supportedOperations.Add(ActSetVariableValue.eSetValueOptions.StartTimer);
            supportedOperations.Add(ActSetVariableValue.eSetValueOptions.StopTimer);
            supportedOperations.Add(ActSetVariableValue.eSetValueOptions.ContinueTimer);
            supportedOperations.Add(ActSetVariableValue.eSetValueOptions.ResetValue);
            return supportedOperations;
        }
        public override bool SupportSetValue { get { return false; } }

    }
}
