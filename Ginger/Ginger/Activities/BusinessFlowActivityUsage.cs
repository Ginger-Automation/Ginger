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
using System.ComponentModel;
using GingerCore;

namespace Ginger.Activities
{
    public class BusinessFlowActivityUsage : INotifyPropertyChanged
    {
        public enum eStatus
        {
            Pending,
            NotSelected,
            Updated
        }

        public static class Fields
        {
            public static string Selected = "Selected";
            public static string BizFlowName = "BizFlowName";
            public static string ActivityName = "ActivityName";
            public static string ActivityGUID = "ActivityGUID";
            public static string ActivityParentGUID = "ActivityParentGUID";
            public static string ActionsCount = "ActionsCount";
            public static string Status = "Status";
        }

        public bool mSelected;
        public bool Selected
        {
            get { return mSelected; }
            set
            {
                mSelected = value;
                if (mSelected)
                {
                    Status = eStatus.Pending;
                }
                else
                {
                    Status = eStatus.NotSelected;
                }
            }
        }


        public string BizFlowName { get; set; }
        public BusinessFlow BusinessFlow { get; set; }
        public string ActivityName { get; set; }
        public Guid ActivityGUID { get; set; }
        public Guid ActivityParentGUID { get; set; }
        public int ActionsCount { get; set; }
        public Activity Activity { get; set; }

        private eStatus mStatus;
        public eStatus Status
        {
            get { return mStatus; }
            set
            {
                if (mStatus != value)
                {
                    mStatus = value;
                    OnPropertyChanged(Fields.Status);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}