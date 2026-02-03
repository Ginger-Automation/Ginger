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

using System;
using System.ComponentModel;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.WorkSpaceLib;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.Variables;

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

        public static ObservableList<UploadItemSelection> mSelectedItems = [];

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

        ObservableList<string> mPartToUpload = [];
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

        public IContext Context { get; set; }

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

        string mTargetFolderFullPath = string.Empty;
        public string TargetFolderFullPath
        {
            get { return mTargetFolderFullPath; }
            set
            {
                if (mTargetFolderFullPath != value)
                {
                    mTargetFolderFullPath = value ?? string.Empty;
                    OnPropertyChanged(nameof(TargetFolderFullPath));
                    OnPropertyChanged(nameof(TargetFolderDisplay));
                }
            }
        }

        public string TargetFolderDisplay
        {
            get
            {
                // Determine the shared root folder and friendly name based on item type
                string rootPath = string.Empty;
                string rootName = "Shared Repository";
                if (UsageItem is ActivitiesGroup)
                {
                    var root = GingerCoreCommonWorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ActivitiesGroup>();
                    rootPath = root?.FolderFullPath ?? string.Empty;
                    rootName = "(root) Shared Activities Groups";
                }
                else if (UsageItem is Activity)
                {
                    var root = GingerCoreCommonWorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Activity>();
                    rootPath = root?.FolderFullPath ?? string.Empty;
                    rootName = "(root) Shared Activities";
                }
                else if (UsageItem is Act)
                {
                    var root = GingerCoreCommonWorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Act>();
                    rootPath = root?.FolderFullPath ?? string.Empty;
                    rootName = "(root) Shared Actions";
                }
                else if (UsageItem is VariableBase)
                {
                    var root = GingerCoreCommonWorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<VariableBase>();
                    rootPath = root?.FolderFullPath ?? string.Empty;
                    rootName = "(root) Shared Variables";
                }

                // No selection yet: show the root folder name
                if (string.IsNullOrEmpty(TargetFolderFullPath))
                {
                    return rootName;
                }

                // Normalize paths for consistent comparison and slicing
                var normRoot = Normalize(rootPath);
                var normTarget = Normalize(TargetFolderFullPath);

                // If selection equals root, show just root
                if (!string.IsNullOrEmpty(normRoot) &&
                    string.Equals(normRoot, normTarget, StringComparison.OrdinalIgnoreCase))
                {
                    return rootName;
                }

                // Otherwise show relative path from the root, prefixed with root name
                if (!string.IsNullOrEmpty(normRoot) &&
                    normTarget.StartsWith(normRoot, StringComparison.OrdinalIgnoreCase))
                {
                    var rel = normTarget.Length > normRoot.Length
                        ? normTarget[normRoot.Length..].Trim('\\', '/')
                        : string.Empty;

                    return string.IsNullOrEmpty(rel) ? rootName : $"{rootName}/{rel}";
                }

                // Fallback: if not under the expected root, show the last segment
                return System.IO.Path.GetFileName(TargetFolderFullPath);
            }
        }

        // helper to normalize paths for comparison
        private static string Normalize(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            return path.Replace('/', '\\').TrimEnd('\\');
        }

        string mTargetFolderFullPath = string.Empty;
        public string TargetFolderFullPath
        {
            get { return mTargetFolderFullPath; }
            set
            {
                if (mTargetFolderFullPath != value)
                {
                    mTargetFolderFullPath = value ?? string.Empty;
                    OnPropertyChanged(nameof(TargetFolderFullPath));
                    OnPropertyChanged(nameof(TargetFolderDisplay));
                }
            }
        }

        public string TargetFolderDisplay
        {
            get
            {
                if (string.IsNullOrEmpty(TargetFolderFullPath))
                {
                    return "Select...";
                }
                return TargetFolderFullPath;
            }
        }
    }
}
