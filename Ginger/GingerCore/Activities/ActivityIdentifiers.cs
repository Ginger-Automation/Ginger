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

using Amdocs.Ginger.Repository;
using System;

namespace GingerCore.Activities
{
    public class ActivityIdentifiers : RepositoryItemBase
    {
        public new static class Fields
        {
            public static string ActivityName = "ActivityName";
            public static string ActivityDescription = "ActivityDescription";
            public static string ActivityGuid = "ActivityGuid";
            public static string ActivityExternalID = "ActivityExternalID";
            public static string ActivityAutomationStatus = "ActivityAutomationStatus";
            public static string ExistInRepository = "ExistInRepository";
        }

        public ActivityIdentifiers()
        {
        }

        [IsSerializedForLocalRepository]
        public string ActivityName { get; set; }
        [IsSerializedForLocalRepository]
        public string ActivityDescription { get; set; }
        [IsSerializedForLocalRepository]
        public Guid ActivityGuid { get; set; }
        [IsSerializedForLocalRepository]
        public string ActivityExternalID { get; set; }
        [IsSerializedForLocalRepository]
        public Activity.eActivityAutomationStatus? ActivityAutomationStatus { get; set; }
        
        public bool ExistInRepository { get; set; }

        private Activity mIdentifiedActivity;
        public Activity IdentifiedActivity
        {
            get
            {
                return mIdentifiedActivity;
            }
            set
            {
                if (mIdentifiedActivity != null) mIdentifiedActivity.PropertyChanged -= Activity_PropertyChanged;
                mIdentifiedActivity= value;
                if (mIdentifiedActivity != null)
                {
                    RefreshActivityIdentifiers();
                    mIdentifiedActivity.PropertyChanged += Activity_PropertyChanged;
                }
            }
        }

        public void RefreshActivityIdentifiers()
        {
            if (mIdentifiedActivity != null)
            {
                ActivityName = mIdentifiedActivity.ActivityName;
                OnPropertyChanged(Fields.ActivityName);
                ActivityDescription = mIdentifiedActivity.Description;
                OnPropertyChanged(Fields.ActivityDescription);
                ActivityGuid = mIdentifiedActivity.Guid;
                OnPropertyChanged(Fields.ActivityGuid);
                ActivityExternalID = mIdentifiedActivity.ExternalID;
                OnPropertyChanged(Fields.ActivityExternalID);
                ActivityAutomationStatus = mIdentifiedActivity.AutomationStatus;
                OnPropertyChanged(Fields.ActivityAutomationStatus);
            }
        }

        private void Activity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Activity.Fields.ActivityName
                    || e.PropertyName == Activity.Fields.Description
                        || e.PropertyName == Activity.Fields.ExternalID
                            || e.PropertyName == Activity.Fields.AutomationStatus)
            {
                RefreshActivityIdentifiers();
            }
        }

        public override string ItemName
        {
            get
            {
                return string.Empty;
            }
            set
            {
                return;
            }
        }
    }
}
