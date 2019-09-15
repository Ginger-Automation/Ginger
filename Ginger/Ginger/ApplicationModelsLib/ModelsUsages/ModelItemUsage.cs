#region License
/*
Copyright © 2014-2019 European Support Limited

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
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using System;
using System.ComponentModel;

namespace Ginger.ApplicationsModels.ModelsUsages
{
    class ModelItemUsage : INotifyPropertyChanged
    {
        public enum eStatus
        {
            NotUpdated,
            Pending,
            Updated,
            UpdateFailed,
            UpdatedAndSaved,
            SaveFailed,
        }

        public static class Fields
        {
            public static string Selected = "Selected";
            public static string HostBizFlowPath = "HostBizFlowPath";
            public static string HostActivityName = "HostActivityName";
            public static string UsageItemName = "UsageItemName";
            public static string UsageExtraDetails = "UsageExtraDetails";
            public static string Status = "Status";
            public static string ItemParts = "ItemParts";
            public static string SelectedItemPart = "SelectedItemPart";
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
                mSelected = value;
                OnPropertyChanged(Fields.Selected);
            }
        }

        public BusinessFlow HostBusinessFlow { get; set; }
        public string HostBizFlowPath { get; set; }

        public Activity HostActivity { get; set; }

        public Act Action { get; set; } 
        public string HostActivityName { get; set; }

        public RepositoryItemBase UsageItem { get; set; }
        public string UsageItemName { get; set; }

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
