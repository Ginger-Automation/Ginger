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

using System.Collections.Generic;
using System.Linq;
using Amdocs.Ginger.Common;
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
        public override string Title { get { return "Add Items to Repository"; } }
                         
        public UploadItemToRepositoryWizard(IEnumerable<object> items)
        {
            UploadItemSelection.mSelectedItems.Clear();
        
            foreach (object i in items)
            {
                UploadItemSelection.mSelectedItems.Add(CreateUploadItem((RepositoryItem)i));
            }
            AddPage(Name: "Items Selection", Title: "Upload Item/s Selection", SubTitle: "Selected items to be added to Shared Repository", Page: new UploadItemsSelectionPage(UploadItemSelection.mSelectedItems));
            AddPage(Name: "Items Validation", Title: "Upload Item/s Validation", SubTitle: "Validate the items to be added to Shared Repository", Page: new UploadItemsValidationPage());
            AddPage(Name: "Items Upload Status", Title: "Upload Item/s Status", SubTitle: "Upload Item Status", Page: new UploadStatusPage());
        }

        private UploadItemSelection CreateUploadItem(RepositoryItem item)
        {
            string strComment = "";
            UploadItemSelection uploadItem = new UploadItemSelection();
            uploadItem.Selected = true;
          
            UploadItemSelection.eExistingItemType existingItemType = UploadItemSelection.eExistingItemType.NA;
            RepositoryItem existingItem = ExistingItemCheck(item, ref strComment, ref existingItemType);
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
                    ActivitiesGroup group = App.BusinessFlow.ActivitiesGroups.Where(x => x.Name == activity.ActivitiesGroupID).FirstOrDefault();
                    if (group != null)
                    {
                        ObservableList<ActivitiesGroup> repoGroups = App.LocalRepository.GetSolutionRepoActivitiesGroups();
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
            uploadItem.SetItemPartesFromEnum(item.GetTypeOfItemParts());
            uploadItem.UsageItem = item;

            return uploadItem;
        }

        public RepositoryItem ExistingItemCheck(object item, ref string strComment, ref UploadItemSelection.eExistingItemType existingItemType)
        {
            IEnumerable<object> existingRepoItems = new ObservableList<RepositoryItem>();
            bool existingItemIsExternalID = false;
            bool existingItemIsParent = false;
            string existingItemFileName = string.Empty;

            if (item is ActivitiesGroup) existingRepoItems = (IEnumerable<object>)App.LocalRepository.GetSolutionRepoActivitiesGroups();
            else if (item is Activity) existingRepoItems = (IEnumerable<object>)App.LocalRepository.GetSolutionRepoActivities();
            else if (item is Act) existingRepoItems = (IEnumerable<object>)App.LocalRepository.GetSolutionRepoActions();
            else if (item is VariableBase) existingRepoItems = (IEnumerable<object>)App.LocalRepository.GetSolutionRepoVariables();

            RepositoryItem exsitingItem = App.LocalRepository.GetMatchingRepoItem((RepositoryItem)item, existingRepoItems, ref existingItemIsExternalID, ref existingItemIsParent);
          
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
