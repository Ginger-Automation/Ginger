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

using System;
using System.Collections.Generic;
using System.IO;
using Amdocs.Ginger.Repository;
using Ginger.Repository.AddItemToRepositoryWizard;
using Ginger.Repository.ItemToRepositoryWizard;
using GingerCore;
using GingerCore.Variables;
using GingerWPF.WizardLib;

namespace Ginger.Repository
{
    public class SharedRepositoryOperations
    {
        public static void AddItemsToRepository(List<RepositoryItemBase> listSelectedRepoItems)
        {
            if (listSelectedRepoItems != null && listSelectedRepoItems.Count>0)
            {
                WizardWindow.ShowWizard(new UploadItemToRepositoryWizard(listSelectedRepoItems), 1200);                
            }
            else
            {
                Reporter.ToUser(eUserMsgKeys.NoItemWasSelected);
            }
        }

        public static void AddItemToRepository(RepositoryItemBase item)
        {
            List<RepositoryItemBase> itemList = new List<RepositoryItemBase>();
            itemList.Add(item);
            AddItemsToRepository(itemList);
        }

        public static Boolean UploadItemToRepository(UploadItemSelection itemToUpload)
        {
            try
            {
                RepositoryItemBase item = itemToUpload.UsageItem;
                string itemFileName = string.Empty;
                RepositoryItemBase itemCopy = null;
                bool isOverwrite = false;
                if (itemToUpload.ItemUploadType == UploadItemSelection.eItemUploadType.Overwrite)
                {
                    isOverwrite = true;
                    itemCopy = GetItemToOverrite(itemToUpload);
                }
                else
                {
                    itemCopy = (RepositoryItem)item.CreateCopy(false);
                }

                itemCopy.UpdateItemFieldForReposiotryUse();

                bool blockingIssuesHandled= HandleItemValidationIssues(itemToUpload, itemCopy, ref isOverwrite);

                if(blockingIssuesHandled==false)
                {
                    itemToUpload.ItemUploadStatus = UploadItemSelection.eItemUploadStatus.FailedToUpload;
                    return false;
                }

                if (isOverwrite)
                {
                    App.LocalRepository.MovePrevVersion(itemToUpload.ExistingItem, itemToUpload.ExistingItem.FileName);
                    App.LocalRepository.RemoveItemFromCache(itemToUpload.ExistingItem);
                    itemFileName = itemToUpload.ExistingItem.FileName;
                }
                else
                    itemFileName = LocalRepository.GetRepoItemFileName(itemCopy, Path.Combine(App.UserProfile.Solution.Folder, LocalRepository.GetSharedRepoItemTypeFolder(itemCopy.GetType())));

                itemCopy.SaveToFile(itemFileName);

                App.LocalRepository.AddRepoItemToCache(itemCopy);


                itemToUpload.UsageItem.IsSharedRepositoryInstance = true;

                if (itemToUpload.ExistingItemType == UploadItemSelection.eExistingItemType.ExistingItemIsParent && itemToUpload.ItemUploadType == UploadItemSelection.eItemUploadType.New)
                {
                    itemToUpload.UsageItem.ParentGuid = Guid.Empty;
                }

                itemToUpload.ItemUploadStatus = UploadItemSelection.eItemUploadStatus.Uploaded;
                return true;
            }
            catch(Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "failed to upload the repository item", e);
                itemToUpload.ItemUploadStatus = UploadItemSelection.eItemUploadStatus.FailedToUpload;
                return false;
            }
        }
      
        private static RepositoryItemBase GetItemToOverrite(UploadItemSelection itemToUpload)
        {
           RepositoryItemBase itemCopy = itemToUpload.UsageItem.GetUpdatedRepoItem(itemToUpload.UsageItem, itemToUpload.ExistingItem,itemToUpload.SelectedItemPart);

            switch (itemToUpload.ExistingItemType)
            {
                case UploadItemSelection.eExistingItemType.ExistingItemIsParent:
                    itemCopy.Guid = itemToUpload.ExistingItem.Guid;
                    itemCopy.ParentGuid = itemToUpload.ExistingItem.ParentGuid;
                    itemCopy.ExternalID = itemToUpload.ExistingItem.ExternalID;

                    break;

                case UploadItemSelection.eExistingItemType.ExistingItemIsExternalID:

                    break;

                case UploadItemSelection.eExistingItemType.ExistingItemIsDuplicate:

                    break;
            }

            return itemCopy;
        }

        private static bool HandleItemValidationIssues(UploadItemSelection selectedItem, RepositoryItemBase itemCopy, ref bool isOverwrite)
        {
            bool blockingIssuesHandled = true;
            List<ItemValidationBase> itemIssues = ItemValidationBase.GetAllIssuesForItem(selectedItem);
            if (itemIssues != null && itemIssues.Count > 0)
            {
                foreach (ItemValidationBase issue in itemIssues)
                {
                    switch (issue.mIssueType)
                    {
                        case ItemValidationBase.eIssueType.MissingVariables:
                            if (issue.Selected)
                            {
                                foreach (string missingVariableName in issue.missingVariablesList)
                                {
                                    VariableBase missingVariable = App.BusinessFlow.GetHierarchyVariableByName(missingVariableName);

                                    if (missingVariable != null)
                                    {
                                        ((Activity)itemCopy).Variables.Add(missingVariable);
                                    }

                                }

                                selectedItem.Comment = "Missing variables added to activity.";
                            }
                            else
                            {
                                selectedItem.Comment = "Uploaded without adding missing variables";
                            }
                            break;

                        case ItemValidationBase.eIssueType.DuplicateName:
                            if (issue.Selected)
                            {
                                isOverwrite = false;
                                itemCopy.ItemName = issue.ItemNewName;
                                selectedItem.Comment = "Uploaded with new newm"+ issue.ItemNewName;
                            }
                            else
                            {
                                selectedItem.Comment = "Can not upload the item with same name";
                                blockingIssuesHandled = false;// if user do not accept new name, upload can not proceed for the item
                            }
                            break;
                    }
                }
            }
            return blockingIssuesHandled;
        }
    }
}
