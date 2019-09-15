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

using System;
using System.Collections.Generic;
using System.Linq;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.Repository.ItemToRepositoryWizard;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.Variables;
using GingerWPF.WizardLib;

namespace Ginger.Repository.AddItemToRepositoryWizard
{
    public class UploadItemToRepositoryWizard : WizardBase
    {
        public override string Title { get { return "Add Items to Shared Repository"; } }
        public Context Context;
        public UploadItemToRepositoryWizard(Context context, IEnumerable<object> items)
        {
            UploadItemSelection.mSelectedItems.Clear();
            Context = context;
            foreach (object i in items)
            {
                UploadItemSelection.mSelectedItems.Add(CreateUploadItem((RepositoryItemBase)i));
            }
            AddPage(Name: "Items Selection", Title: "Item/s Selection", SubTitle: "Selected items to be added to Shared Repository", Page: new UploadItemsSelectionPage(UploadItemSelection.mSelectedItems));
            AddPage(Name: "Items Validation", Title: "Item/s Validation", SubTitle: "Validate the items to be added to Shared Repository", Page: new UploadItemsValidationPage());
            AddPage(Name: "Items Status", Title: "Item/s Status", SubTitle: "Upload Item Status", Page: new UploadStatusPage());
        }

        private UploadItemSelection CreateUploadItem(RepositoryItemBase item)
        {
            string strComment = "";
            UploadItemSelection uploadItem = new UploadItemSelection();
            uploadItem.Selected = true;
          
            UploadItemSelection.eExistingItemType existingItemType = UploadItemSelection.eExistingItemType.NA;
            RepositoryItemBase existingItem = ExistingItemCheck(item, ref strComment, ref existingItemType);
            if (existingItem != null)
            {
                uploadItem.ItemUploadType = UploadItemSelection.eItemUploadType.Overwrite;
                uploadItem.ExistingItem = existingItem;
                uploadItem.ExistingItemType = existingItemType;
                uploadItem.Comment = strComment;
            }
            else
                uploadItem.ItemUploadType = UploadItemSelection.eItemUploadType.New;

            if(item is Activity)
            {
                Activity activity = (Activity)item;

                if (activity.ActivitiesGroupID != null && activity.ActivitiesGroupID != string.Empty)
                {
                    ActivitiesGroup group =(ActivitiesGroup)Context.BusinessFlow.ActivitiesGroups.Where(x => x.Name == activity.ActivitiesGroupID).FirstOrDefault();
                    if (group != null)
                    {
                        ObservableList<ActivitiesGroup> repoGroups = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
                        ActivitiesGroup repoGroup = repoGroups.Where(x => (x.Guid == group.Guid) || (x.Guid == group.ParentGuid) || (group.ExternalID != null &&
                        group.ExternalID != string.Empty && x.ExternalID == group.ExternalID)).FirstOrDefault();
                        if (repoGroup == null)
                        {
                            uploadItem.Comment = "It is recommended to also add parent activity group: " + group.ItemName + " to repository";
                        }
                    }
                }
            }

            uploadItem.ItemName = item.ItemName;
            uploadItem.ItemGUID = item.Guid;
            uploadItem.SetItemPartesFromEnum(GetTypeOfItemParts(item));
            uploadItem.UsageItem = item;

            return uploadItem;
        }

        public virtual Type GetTypeOfItemParts(RepositoryItemBase item)
        {
            if (item.GetType() == typeof(Activity))
            {
                return typeof(eItemParts);   
            }
            else if (item.GetType() == typeof(Act))
            {
                return typeof(Act.eItemParts);
            }
            else if (item.GetType() == typeof(ActivitiesGroup))
            { 
                return typeof(ActivitiesGroup.eItemParts);
            }
            else if (item.GetType() == typeof(VariableBase))
            { 
                return typeof(VariableBase.eItemParts);
            }
            else
            {
                return null;
            }
                
        }

        public RepositoryItemBase ExistingItemCheck(object item, ref string strComment, ref UploadItemSelection.eExistingItemType existingItemType)
        {
            IEnumerable<object> existingRepoItems = new ObservableList<RepositoryItem>();
            bool existingItemIsExternalID = false;
            bool existingItemIsParent = false;
            string existingItemFileName = string.Empty;

            ObservableList<ActivitiesGroup> activitiesGroup = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
            if (item is ActivitiesGroup)
            {
                existingRepoItems = (IEnumerable<object>)activitiesGroup;
            }
            else if (item is Activity)
            {
                existingRepoItems = (IEnumerable<object>)WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
            }
            else if (item is Act)
            {
                existingRepoItems = (IEnumerable<object>)WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Act>();
            }
            else if (item is VariableBase)
            {
                existingRepoItems = (IEnumerable<object>)WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<VariableBase>();
            }

            RepositoryItemBase exsitingItem = SharedRepositoryOperations.GetMatchingRepoItem((RepositoryItemBase)item, existingRepoItems, ref existingItemIsExternalID, ref existingItemIsParent);
          
            if (exsitingItem != null)
            {
                existingItemFileName = exsitingItem.FileName;

                if (existingItemIsExternalID)
                {
                    strComment = "Item with Same External Id Exist. Back up of existing item will be saved in PrevVersion folder.Change the item upload type if you want to upload it as new item";
                    existingItemType= UploadItemSelection.eExistingItemType.ExistingItemIsExternalID;
                }
                else if (existingItemIsParent)
                {
                    strComment = "Parent item exist in repository. Back up of existing item will be saved in PrevVersion folder.Change the item upload type if you want to upload it as new item";
                    existingItemType = UploadItemSelection.eExistingItemType.ExistingItemIsParent;
                }
                else
                {
                    strComment = "Item already exist in repository. Back up of existing item will be saved in PrevVersion folder.Change the item upload type if you want to upload it as new item";
                    existingItemType = UploadItemSelection.eExistingItemType.ExistingItemIsDuplicate;
                }
            }
            return exsitingItem;
        }

        public override void Finish()
        {
            mWizardWindow.Close();
        }
    }
}
