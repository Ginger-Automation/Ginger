using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Plugin.Core;
using System;
using System.Timers;

namespace GingerCoreNETUnitTest.RecordingLibTest
{
    public class TestDriver : IRecord
    {
        private bool LearnAdditionalDetails { get; set; }
        public event RecordingEventHandler RecordingEvent;

        public Timer mGetRecordingTimer;
        int i = 0;

        public void StartRecording(bool learnAdditionalChanges = false)
        {
            i = 0;
            LearnAdditionalDetails = learnAdditionalChanges;
            mGetRecordingTimer = new Timer(1000);
            mGetRecordingTimer.Elapsed += MGetRecordingTimer_Elapsed;
            mGetRecordingTimer.Start();
            //DoRecording();
        }

        private void MGetRecordingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DoRecording();
        }

        private void DoRecording()
        {
            string name = "Name_" + Convert.ToString(i);

            ElementInfo eInfo = new ElementInfo();
            eInfo.ElementName = name;

            ElementActionCongifuration eleArgs = new ElementActionCongifuration();
            if (i == 1 || i == 3)
            {
                eleArgs.LocateBy = eLocateBy.ByName.ToString();
                eleArgs.LocateValue = name;
                eleArgs.ElementValue = "aaa";
                eleArgs.Operation = "SetText";
                eleArgs.Type = "TextBox";
                eleArgs.Description = "input Text " + name;
                eInfo.ElementTypeEnum = eElementType.TextBox;
            }
            else
            {
                eleArgs.LocateBy = eLocateBy.ByID.ToString();
                eleArgs.LocateValue = name;
                eleArgs.ElementValue = "cc";
                eleArgs.Operation = "Click";
                eleArgs.Type = "Button";
                eleArgs.Description = "input button " + name;
                eInfo.ElementTypeEnum = eElementType.Button;
            }

            if (LearnAdditionalDetails)
            {
                eleArgs.LearnedElementInfo = eInfo;
            }

            RecordingEventArgs pageArgs = new RecordingEventArgs();
            if (i != 2)
            {
                pageArgs.PageURL = "www.google.com";
                pageArgs.PageTitle = "Google";                
            }
            else
            {
                pageArgs.PageURL = "www.new.com";
                pageArgs.PageTitle = "New";
            }

            OnRecordingEvent(new ElementActionCongifuration() { EventType = eRecordingEvent.PageChanged, EventArgs = pageArgs });
            OnRecordingEvent(eleArgs);
            i++;
        }

        public void StopRecording()
        {
            if (mGetRecordingTimer != null)
            {
                mGetRecordingTimer.Stop();
                mGetRecordingTimer.Dispose();
            }
        }

        protected void OnRecordingEvent(ElementActionCongifuration e)
        {
            RecordingEvent?.Invoke(this, e);
        }
    }
}
