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

using GingerCore.Actions;
using System;

namespace GingerCore.Drivers.Common
{
    public enum POMAction
    {
        StartRecording,
        EndRecording,
    }

    public delegate void POMEventHandler(object sender, POMEventArgs e);

    public class POMEventArgs : EventArgs
    {
        string mWindowTitle;
        Act mAct;        

        public POMEventArgs(string WindowTitle, Act act)
        {
            mAct = act;
            mWindowTitle = WindowTitle;            
        }

        
        public string WindowTitle { get { return mWindowTitle; } }
        public Act Act { get { return mAct; } }
    }

    public interface IPOM
    {
        void ActionRecordedCallback(POMEventHandler ActionRecorded);
    }
}
