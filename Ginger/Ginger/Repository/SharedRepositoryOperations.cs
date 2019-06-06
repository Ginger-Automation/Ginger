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
using System.IO;
using System.Linq;
using System.Windows;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.Repository.AddItemToRepositoryWizard;
using Ginger.Repository.ItemToRepositoryWizard;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.Variables;
using GingerWPF.WizardLib;

namespace Ginger.Repository
{
    public class SharedRepositoryOperations
    {        
        public void AddItemsToRepository(Context context, List<RepositoryItemBase> listSelectedRepoItems)
        {
            if (listSelectedRepoItems != null && listSelectedRepoItems.Count>0)
            {
                WizardWindow.ShowWizard(new UploadItemToRepositoryWizard(context, listSelectedRepoItems), 1200);                
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
        }

        public void AddItemToRepository(Context context, RepositoryItemBase item)
        {
            List<RepositoryItemBase> itemList = new List<RepositoryItemBase>();
            itemList.Add(item);
            AddItemsToRepository(context, itemList);
        }

        public Boolean UploadItemToRepository(Context context, UploadItemSelection itemToUpload)
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
                    itemCopy = (RepositoryItemBase)item.CreateCopy(false);
                }

                itemCopy.UpdateItemFieldForReposiotryUse();

               

                bool blockingIssuesHandled= HandleItemValidationIssues(context, itemToUpload, itemCopy, ref isOverwrite);

                if(blockingIssuesHandled==false)
                {
                    itemToUpload.ItemUploadStatus = UploadItemSelection.eItemUploadStatus.FailedToUpload;
                    return false;
                }

                if (isOverwrite)
                {
                    WorkSpace.Instance.SolutionRepository.MoveSharedRepositoryItemToPrevVersion(itemToUpload.ExistingItem);
                
                    RepositoryFolderBase repositoryFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryFolderByPath(itemToUpload.ExistingItem.ContainingFolderFullPath);
                    if(repositoryFolder !=null)
                    {
                       repositoryFolder.AddRepositoryItem(itemCopy);
                    }                    
                }
                else
                {
                    WorkSpace.Instance.SolutionRepository.AddRepositoryItem(itemCopy);
                }     

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

        private bool HandleItemValidationIssues(Context context, UploadItemSelection selectedItem, RepositoryItemBase itemCopy, ref bool isOverwrite)
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
                                    VariableBase missingVariable = context.BusinessFlow.GetHierarchyVariableByName(missingVariableName);

                                    if (missingVariable != null)
                                    {
                                        ((Activity)itemCopy).Variables.Add(missingVariable);
                                    }

                                }

                                selectedItem.Comment = "Missing " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " added to " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                            }
                            else
                            {
                                selectedItem.Comment = "Uploaded without adding missing " + GingerDicser.GetTermResValue(eTermResKey.Variables);
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


        public static void MarkSharedRepositoryItems(IEnumerable<object> items, IEnumerable<object> existingRepoItems = null)
        {
            bool linkIsByExternalID = false;
            bool linkIsByParentID = false;
            if (items != null && items.Count() > 0)
            {
                foreach (RepositoryItemBase item in items)
                {
                    if (item == null)
                    {
                        continue;
                    }
                    if (GetMatchingRepoItem(item, existingRepoItems, ref linkIsByExternalID, ref linkIsByParentID) != null)
                    {
                        item.IsSharedRepositoryInstance = true;
                    }                        
                    else
                    {
                        item.IsSharedRepositoryInstance = false;
                    }                        
                }
                    
            }
        }


        
        public static RepositoryItemBase GetMatchingRepoItem(RepositoryItemBase item, IEnumerable<object> existingRepoItems, ref bool linkIsByExternalID, ref bool linkIsByParentID)
        {            
            if (existingRepoItems == null)
            {
                if (item is ActivitiesGroup)
                {
                    existingRepoItems = (IEnumerable<object>)WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
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
                else
                {
                    return null;
                }
            }

            linkIsByExternalID = false;
            linkIsByParentID = false;

            //check if item with the same GUID already exist in repository
            RepositoryItemBase repoItem = (RepositoryItemBase)existingRepoItems.Where(x => ((RepositoryItemBase)x).Guid == item.Guid).FirstOrDefault();
            //check if there is already item in repo which map to a specific ExternalID
            if (repoItem == null && item.ExternalID != null && item.ExternalID != string.Empty && item.ExternalID != "0")
            {
                repoItem = (RepositoryItemBase)existingRepoItems.Where(x => ((RepositoryItemBase)x).ExternalID == item.ExternalID).FirstOrDefault();
                if (repoItem != null)
                {
                    linkIsByExternalID = true;
                }
            }
            if (repoItem == null && item.ParentGuid != Guid.Empty)
            {
                repoItem = (RepositoryItemBase)existingRepoItems.Where(x => ((RepositoryItemBase)x).Guid == item.ParentGuid).FirstOrDefault();
                if (repoItem != null)
                {
                    linkIsByParentID = true;
                }
            }

            return repoItem;
        }

        public static bool CheckIfSureDoingChange(RepositoryItemBase item, string changeType)
        {
            RepositoryItemUsagePage usagePage = null;
            usagePage = new RepositoryItemUsagePage(item, true);
            if (usagePage.RepoItemUsages.Count > 0)//TODO: check if only one instance exist for showing the pop up for better performance
            {
                if (Reporter.ToUser(eUserMsgKey.AskIfWantsToChangeeRepoItem, item.GetNameForFileName(), usagePage.RepoItemUsages.Count, changeType) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                {
                    return true;
                }                    
                else
                {
                    return false;
                }                    
            }

            return true;
        }

    }
}
