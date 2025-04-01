#region License
/*
Copyright © 2014-2025 European Support Limited

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

using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Plugin.Core;
using System;

namespace GingerCoreNETUnitTest.RecordingLibTest
{
    public class TestDriver : IRecord
    {
        private bool LearnAdditionalDetails { get; set; }
        public event RecordingEventHandler RecordingEvent;

        void Amdocs.Ginger.Plugin.Core.IRecord.ResetRecordingEventHandler()
        {
            RecordingEvent = null;
        }

        public void StartRecording(bool learnAdditionalChanges = false)
        {
            LearnAdditionalDetails = learnAdditionalChanges;
            for (int i = 0; i < 2; i++)
            {
                DoRecording(i);
            }
        }

        private void DoRecording(int i)
        {
            string name = "Name_" + Convert.ToString(i);

            ElementInfo eInfo = new ElementInfo
            {
                ElementName = name
            };

            ElementActionCongifuration eleArgs = new ElementActionCongifuration();
            if (i is 1 or 3)
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

            PageChangedEventArgs pageArgs = new PageChangedEventArgs();
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

            OnRecordingEvent(new RecordingEventArgs() { EventType = eRecordingEvent.PageChanged, EventArgs = pageArgs });
            OnRecordingEvent(new RecordingEventArgs() { EventType = eRecordingEvent.ElementRecorded, EventArgs = eleArgs });
        }

        public void StopRecording()
        {
        }

        protected void OnRecordingEvent(RecordingEventArgs e)
        {
            RecordingEvent?.Invoke(this, e);
        }
    }
}
