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
using System.Linq;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.Repository.Serialization;
using Amdocs.Ginger.Repository;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GingerCore.Activities
{
    public class ActivityIdentifiers : RepositoryItemBase
    {
        public ActivityIdentifiers() { }

        public ActivityIdentifiers(DeserializedSnapshot snapshot) : base(snapshot) { }

        protected override SerializedSnapshot.Builder WriteSnapshotProperties(SerializedSnapshot.Builder snapshotBuilder)
        {
            return base.WriteSnapshotProperties(snapshotBuilder)
                .WithValue(nameof(ActivityAutomationStatus), ActivityAutomationStatus.ToString())
                .WithValue(nameof(ActivityGuid), ActivityGuid.ToString())
                .WithValue(nameof(ActivityName), ActivityName)
                .WithValue(nameof(ActivityParentGuid), ActivityParentGuid.ToString());
        }

        protected override void ReadSnapshotProperties(DeserializedSnapshot.Property property)
        {
            base.ReadSnapshotProperties(property);
            if (property.HasName(nameof(ActivityAutomationStatus)))
                ActivityAutomationStatus = property.GetValueAsEnum<eActivityAutomationStatus>();
            else if (property.HasName(nameof(ActivityGuid)))
                ActivityGuid = property.GetValueAsGuid();
            else if (property.HasName(nameof(ActivityName)))
                ActivityName = property.GetValue();
            else if (property.HasName(nameof(ActivityParentGuid)))
                ActivityParentGuid = property.GetValueAsGuid();
        }

        [IsSerializedForLocalRepository]
        public string ActivityName { get; set; }
        [IsSerializedForLocalRepository]
        public string ActivityDescription { get; set; }
        [IsSerializedForLocalRepository]
        public Guid ActivityGuid { get; set; }
        [IsSerializedForLocalRepository]
        public Guid ActivityParentGuid { get; set; }
        [IsSerializedForLocalRepository]
        public string ActivityExternalID { get; set; }
        [IsSerializedForLocalRepository]
        public eActivityAutomationStatus? ActivityAutomationStatus { get; set; }

        public bool ExistInRepository { get; set; }

        public bool AddDynamicly { get; set; }

        private Activity mIdentifiedActivity;
        public Activity IdentifiedActivity
        {
            get
            {
                return mIdentifiedActivity;
            }
            set
            {
                if (mIdentifiedActivity != null)
                {
                    ((Activity)mIdentifiedActivity).PropertyChanged -= Activity_PropertyChanged;
                }
                mIdentifiedActivity = value;
                if (mIdentifiedActivity != null)
                {
                    RefreshActivityIdentifiers();
                    ((Activity)mIdentifiedActivity).PropertyChanged += Activity_PropertyChanged;
                }
            }
        }

        public void RefreshActivityIdentifiers()
        {
            if (mIdentifiedActivity != null)
            {
                if (ActivityName != mIdentifiedActivity.ActivityName)
                {
                    ActivityName = mIdentifiedActivity.ActivityName;
                    OnPropertyChanged(nameof(ActivityName));
                }
                if (ActivityDescription != mIdentifiedActivity.Description)
                {
                    ActivityDescription = mIdentifiedActivity.Description;
                    OnPropertyChanged(nameof(ActivityDescription));
                }
                if (ActivityGuid != mIdentifiedActivity.Guid)
                {
                    ActivityGuid = mIdentifiedActivity.Guid;
                    OnPropertyChanged(nameof(ActivityGuid));
                }
                if (ActivityParentGuid != mIdentifiedActivity.ParentGuid)
                {
                    ActivityParentGuid = mIdentifiedActivity.ParentGuid;
                    OnPropertyChanged(nameof(ActivityParentGuid));
                }
                if (ActivityExternalID != mIdentifiedActivity.ExternalID)
                {
                    ActivityExternalID = mIdentifiedActivity.ExternalID;
                    OnPropertyChanged(nameof(ActivityExternalID));
                }
                if (ActivityAutomationStatus != mIdentifiedActivity.AutomationStatus)
                {
                    ActivityAutomationStatus = mIdentifiedActivity.AutomationStatus;
                    OnPropertyChanged(nameof(ActivityAutomationStatus));
                }
            }
        }

        private void Activity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Activity.ActivityName)
                    || e.PropertyName == nameof(Activity.Description)
                        || e.PropertyName == nameof(Activity.ExternalID)
                            || e.PropertyName == nameof(Activity.AutomationStatus))
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
        public override string GetItemType()
        {
            return "ActivityIdentifiers";
        }
        public override string ItemNameField
        {
            get
            {
                return ItemName;
            }
        }
    }
}
