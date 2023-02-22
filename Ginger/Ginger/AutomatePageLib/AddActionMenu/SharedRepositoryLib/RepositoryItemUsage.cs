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

using Amdocs.Ginger.Common;
using System;
using System.ComponentModel;
using GingerCore;
using Amdocs.Ginger.Repository;
using System.Collections.Generic;
using System.Collections;
using GingerCore.GeneralLib;

namespace Ginger.Repository
{
    public class RepositoryItemUsage : INotifyPropertyChanged
    {
        public enum eStatus
        {
            NotUpdated,
            Pending,            
            Updated, 
            UpdateFailed,
            UpdatedAndSaved,
            SaveFailed,
            NA
        }

        public enum eRepositoryItemPublishType
        {
            [EnumValueDescription("Regular Instance")]
            RegularInstance = 1,
            [EnumValueDescription("Link Instance")]
            LinkInstance = 0
        }

        public enum ePublishStatus
        {
            [EnumValueDescription("Not Published")]
            NotPublished,
            Published,
            [EnumValueDescription("Failed To Publish")]
            FailedToPublish
        }

        public enum eInsertRepositoryInsatncePosition
        {
            [EnumValueDescription("At End")]
            AtEnd,
            [EnumValueDescription("Beginning")]
            Beginning,
            [EnumValueDescription("After Specific Activity")]
            AfterSpecificActivity
        }

        public enum eUsageTypes
        {
            Original,
            [EnumValueDescription("Regular Instance")]
            RegularInstance,
            None,
            [EnumValueDescription("Link Instance")]
            LinkInstance
        }

        public bool IsDisabled
        {
            get
            {
                return !UsageItem.IsLinkedItem;
            }
        }

        public static class Fields
        {
            public static string Selected = "Selected";
            public static string HostBizFlowPath = "HostBizFlowPath";
            public static string HostActivityName = "HostActivityName";
            public static string UsageItemName = "UsageItemName";
            public static string UsageItemType = "UsageItemType";
            public static string UsageExtraDetails = "UsageExtraDetails";
            public static string Status = "Status";
            public static string ItemParts = "ItemParts";
            public static string SelectedItemPart = "SelectedItemPart";

            public static string RepositoryItemPublishType = "RepositoryItemPublishType";
            public static string InsertRepositoryInsatncePosition = "InsertRepositoryInsatncePosition";
            public static string PublishStatus = "PublishStatus";
            public static string IndexActivityName = "IndexActivityName";
        }

        bool mSelected;
        public bool Selected 
        { 
            get
            {
                return mSelected;
            }
            set
            {
                mSelected=value;
                OnPropertyChanged(Fields.Selected);
            }
        }

        public BusinessFlow HostBusinessFlow { get; set; }
        public string HostBizFlowPath { get; set; }

        public Activity HostActivity { get; set; }
        public string HostActivityName { get; set; }

        public RepositoryItemBase UsageItem { get; set; }
        public string UsageItemName { get; set; }

        public eUsageTypes UsageItemType { get; set; }
        public string UsageExtraDetails { get; set; }
        
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

        private eRepositoryItemPublishType mRepositoryItemPublishType;
        public eRepositoryItemPublishType RepositoryItemPublishType
        {
            get { return mRepositoryItemPublishType; }
            set
            {
                if (mRepositoryItemPublishType != value)
                {
                    mRepositoryItemPublishType = value;
                    OnPropertyChanged(Fields.RepositoryItemPublishType);
                }
            }
        }

        private string mIndexActivityName;
        public string IndexActivityName
        {
            get { return mIndexActivityName; }
            set
            {
                if (mIndexActivityName != value)
                {
                    mIndexActivityName = value;
                    OnPropertyChanged(Fields.IndexActivityName);
                }
            }
        }

        private  ObservableList<string> mActivityList = new ObservableList<string>();
       public  ObservableList<string> ActivityNameList
        {
            get
            {
                return mActivityList;
            }
            set
            {
                mActivityList = value;
                OnPropertyChanged(nameof(ActivityNameList));
            }
        }


        private eInsertRepositoryInsatncePosition mInsertRepositoryInsatncePosition;
        public eInsertRepositoryInsatncePosition InsertRepositoryInsatncePosition
        {
            get { return mInsertRepositoryInsatncePosition; }
            set
            {
                if (mInsertRepositoryInsatncePosition != value)
                {
                    mInsertRepositoryInsatncePosition = value;

                    OnPropertyChanged(Fields.InsertRepositoryInsatncePosition);
                }
            }
        }

        private ePublishStatus mPublishStatus;
        public ePublishStatus PublishStatus
        {
            get { return mPublishStatus; }
            set
            {
                if (mPublishStatus != value)
                {
                    mPublishStatus = value;

                    OnPropertyChanged(Fields.PublishStatus);
                }
            }
        }


        ObservableList<string> mItemParts = new ObservableList<string>();
        public ObservableList<string> ItemParts
        {
            get
            {
                return mItemParts;
            }
            set
            {
                mItemParts = value;
                OnPropertyChanged(Fields.ItemParts);
            }
        }

        string mSelectedItemPart;
        public string SelectedItemPart
        {
            get
            {
                return mSelectedItemPart;
            }
            set
            {
                mSelectedItemPart = value;
                OnPropertyChanged(Fields.SelectedItemPart);
            }
        }

        public void SetItemPartesFromEnum(Type enumType)
        {
            ItemParts.Clear();
            if (enumType != null)
            {
                // Get all possible enum vals
                foreach (object item in Enum.GetValues(enumType))
                {
                    ItemParts.Add(item.ToString());
                }
            }
            if (ItemParts.Count > 0)
                SelectedItemPart = ItemParts[0];
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
