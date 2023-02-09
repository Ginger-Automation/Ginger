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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;

namespace GingerCore.Variables
{
    public class VariableTimer : VariableBase
    {
        public Stopwatch RunWatch = new Stopwatch();
        public Timer timer;

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

        private eTimerUnit mTimerUnit;
        
        [IsSerializedForLocalRepository]
        public eTimerUnit TimerUnit
        {
            get
            {
                return mTimerUnit;
            }
            set
            {
                mTimerUnit = value;
                OnPropertyChanged(nameof(this.TimerUnit));
            }
        }

        public override string VariableEditPage { get { return  "VariableTimerPage"; } }

        public Boolean IsStopped { get; set; }

        public override string GetFormula()
        {
            return "Timer unit="+ TimerUnit.ToString();
        }

        private string mValue;

        public override string Value
        {
            get
            {

                if (RunWatch.IsRunning)
                {
                  mValue= GetElapsedValue();
                }

                return mValue;

            }
            set
            {
                mValue = value;
                OnPropertyChanged("Value");
            }
        }


        private string GetElapsedValue()
        {
            string elapsed = "";
            switch (TimerUnit)
            {
                case eTimerUnit.MilliSeconds:
                    elapsed = Math.Round(RunWatch.Elapsed.TotalMilliseconds, 2).ToString();
                    break;

                case eTimerUnit.Seconds:
                    elapsed = Math.Round(RunWatch.Elapsed.TotalSeconds, 2).ToString();
                    break;

                case eTimerUnit.Minutes:
                    elapsed = Math.Round(RunWatch.Elapsed.TotalMinutes, 2).ToString();
                    break;

                case eTimerUnit.Hours:
                    elapsed = Math.Round(RunWatch.Elapsed.TotalHours, 2).ToString();
                    break;
            }
            return elapsed;
        }

        public void StartTimer(bool isContinue=false)
        { 
            if (timer == null)
            {
                timer = new Timer();
            }

            if(isContinue==false)
                RunWatch.Reset();

            if (TimerUnit == eTimerUnit.MilliSeconds)
                timer.Interval = new TimeSpan(0, 0, 0, 0, 1).TotalMilliseconds;
            if (TimerUnit == eTimerUnit.Seconds)
                timer.Interval = new TimeSpan(0, 0, 0, 1, 0).TotalSeconds;
            if (TimerUnit == eTimerUnit.Minutes)
                timer.Interval = new TimeSpan(0, 0, 1, 0, 0).TotalMinutes;
            if (TimerUnit == eTimerUnit.Hours)
                timer.Interval = new TimeSpan(0, 1, 0, 0, 0).TotalHours;
            
            RunWatch.Start();
            timer.Start();
            timer.Elapsed += dispatcherTimerElapsedTick;
        }
              

        public void StopTimer()
        {
            if (RunWatch.IsRunning)
            {
                Value = GetElapsedValue();
                RunWatch.Stop();
                timer.Stop();
                timer.Elapsed -= dispatcherTimerElapsedTick;
            }
        }

        public void ContinueTimer()
        {
            StartTimer(true);
        }


        public override void ResetValue()
        {
            StopTimer();
            RunWatch.Reset();
            Value = GetElapsedValue();
        }

        private void dispatcherTimerElapsedTick(object sender, System.EventArgs e)
        {
            if (RunWatch.IsRunning)
            {
                OnPropertyChanged(nameof(Value));
            }
        }

       
        public override bool GenerateAutoValue(ref string errorMsg)
        {
            //NA
            errorMsg = "Generate Auto Value is not supported";
            return false;
        }

        public override eImageType Image { get { return eImageType.Timer; } }
        public override bool SupportSetValue { get { return false; } }
        public override string VariableType
        {
            get { return "Timer"; }
        }

        public override List<VariableBase.eSetValueOptions> GetSupportedOperations()
        {
            List<VariableBase.eSetValueOptions> supportedOperations = new List<VariableBase.eSetValueOptions>();
            supportedOperations.Add(VariableBase.eSetValueOptions.StartTimer);
            supportedOperations.Add(VariableBase.eSetValueOptions.StopTimer);
            supportedOperations.Add(VariableBase.eSetValueOptions.ContinueTimer);
            supportedOperations.Add(VariableBase.eSetValueOptions.ResetValue);
            return supportedOperations;
        }

        public override bool SupportResetValue { get { return true; } }

        public override bool SupportAutoValue { get { return false; } }
    }
}
