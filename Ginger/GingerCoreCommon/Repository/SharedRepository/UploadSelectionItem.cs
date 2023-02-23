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

namespace Ginger.Repository.ItemToRepositoryWizard
{
    public class UploadItemSelection : RepositoryItemBase, INotifyPropertyChanged
    {

        public enum eItemUploadStatus
        {
            PendingUpload,
            Uploaded,
            FailedToUpload,
            Skipped
        }

        public enum eActivityInstanceType
        {
            [EnumValueDescription("Link Instance")]
            LinkInstance = 1,
            [EnumValueDescription("Regular Instance")]
            RegularInstance = 0
        }

        public eItemUploadStatus ItemUploadStatus { get; set; }

        public static ObservableList<UploadItemSelection> mSelectedItems = new ObservableList<UploadItemSelection>();

        public enum eItemUploadType
        {
            New,
            Overwrite
        }
        public enum eItemReplace
        {
            NormalInstance,
            AsLink
        }
        private eItemUploadType mItemUploadType;
        public eItemUploadType ItemUploadType
        {
            get { return mItemUploadType; }
            set
            {
                if (mItemUploadType != value)
                {
                    mItemUploadType = value;
                    OnPropertyChanged(nameof(ItemUploadType));
                    OnPropertyChanged(nameof(IsOverrite));
                }
            }
        }
        public bool IsOverrite
        {
            get
            {
                if (ItemUploadType == eItemUploadType.Overwrite)
                    return true;
                else
                    return false;
            }
        }

        public bool IsExistingItemParent
        {
            get
            {
                if (ExistingItemType == eExistingItemType.ExistingItemIsParent)
                    return true;
                else
                    return false;
            }
        }

        public RepositoryItemBase UsageItem { get; set; }
        public bool IsActivity { get => UsageItem is Activity; }
        public RepositoryItemBase ExistingItem { get; set; }

        public enum eExistingItemType
        {
            NA,
            ExistingItemIsParent,
            ExistingItemIsExternalID,
            ExistingItemIsDuplicate,
        }

        public eExistingItemType ExistingItemType { get; set; }

        bool mSelected;
        public bool Selected
        {
            get
            {
                return mSelected;
            }
            set
            {
                if (mSelected != value)
                {
                    mSelected = value;
                    OnPropertyChanged(nameof(Selected));
                }
            }
        }

        public string Comment { get; set; }

        string mSelectedItemPart;
        public string SelectedItemPart
        {
            get
            {
                return mSelectedItemPart;
            }
            set
            {
                if (mSelectedItemPart != value)
                {
                    mSelectedItemPart = value;
                    OnPropertyChanged(nameof(SelectedItemPart));
                }
            }
        }

        Array mUploadTypeList { get; set; }

        public Array UploadTypeList
        {
            get
            {
                return Enum.GetValues(typeof(eItemUploadType));
            }
            set
            {
                mUploadTypeList = value;
                OnPropertyChanged(nameof(UploadTypeList));
            }
        }

        ObservableList<string> mPartToUpload = new ObservableList<string>();
        public ObservableList<string> PartToUpload
        {
            get
            {
                return mPartToUpload;
            }
            set
            {
                mPartToUpload = value;
                OnPropertyChanged(nameof(PartToUpload));
            }
        }

        public override string ItemName { get; set; }

        public string ExistingItemName
        {
            get
            {
                if (ExistingItem != null)
                    return ExistingItem.ItemName;
                else
                    return "NA";
            }
            set
            {
                ExistingItemName = value;
            }
        }

        public Guid ItemGUID { get; set; }


        public void SetItemPartesFromEnum(Type enumType)
        {
            PartToUpload.Clear();
            if (enumType != null)
            {
                // Get all possible enum vals
                foreach (object item in Enum.GetValues(enumType))
                {
                    PartToUpload.Add(item.ToString());
                }
            }
            if (PartToUpload.Count > 0)
                SelectedItemPart = PartToUpload[0];
        }

        eActivityInstanceType mReplaceType;
        public eActivityInstanceType ReplaceType
        {
            get
            {
                return mReplaceType;
            }
            set
            {
                if (mReplaceType != value)
                {
                    mReplaceType = value;
                    OnPropertyChanged(nameof(ReplaceType));
                }
            }
        }
    }
}
