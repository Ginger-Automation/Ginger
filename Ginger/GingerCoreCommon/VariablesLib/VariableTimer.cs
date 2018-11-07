using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Threading;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;

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
        public override bool SupportSetValue { get { return false; } }
        public override string VariableType() { return "Timer"; }

        public override List<VariableBase.eSetValueOptions> GetSupportedOperations()
        {
            List<VariableBase.eSetValueOptions> supportedOperations = new List<VariableBase.eSetValueOptions>();
            supportedOperations.Add(VariableBase.eSetValueOptions.StartTimer);
            supportedOperations.Add(VariableBase.eSetValueOptions.StopTimer);
            supportedOperations.Add(VariableBase.eSetValueOptions.ContinueTimer);
            supportedOperations.Add(VariableBase.eSetValueOptions.ResetValue);
            return supportedOperations;
        }

    }
}
