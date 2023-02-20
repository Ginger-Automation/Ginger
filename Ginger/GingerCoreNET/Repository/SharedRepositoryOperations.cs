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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using Ginger.Repository.AddItemToRepositoryWizard;
using Ginger.Repository.ItemToRepositoryWizard;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.Platforms;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;

namespace Ginger.Repository
{
    public class SharedRepositoryOperations : ISharedRepositoryOperations
    {
        //public void AddItemsToRepository(Context context, List<RepositoryItemBase> listSelectedRepoItems)
        //{
        //    if (listSelectedRepoItems != null && listSelectedRepoItems.Count>0)
        //    {
        //        WizardWindow.ShowWizard(new UploadItemToRepositoryWizard(context, listSelectedRepoItems));
        //    }
        //    else
        //    {
        //        Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
        //    }
        //}

        //public void AddItemToRepository(Context context, RepositoryItemBase item)
        //{
        //    List<RepositoryItemBase> itemList = new List<RepositoryItemBase>();
        //    itemList.Add(item);
        //    AddItemsToRepository(context, itemList);
        //}

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



                bool blockingIssuesHandled = HandleItemValidationIssues(context, itemToUpload, itemCopy, ref isOverwrite);

                if (blockingIssuesHandled == false)
                {
                    itemToUpload.ItemUploadStatus = UploadItemSelection.eItemUploadStatus.FailedToUpload;
                    return false;
                }

                if (itemCopy is Activity)
                {
                    ((Activity)itemCopy).Type = eSharedItemType.Regular;
                    foreach (Act act in ((Activity)itemCopy).Acts)
                    {
                        foreach (ActInputValue inputValue in act.InputValues)
                        {
                            inputValue.StartDirtyTracking();
                            inputValue.OnDirtyStatusChanged += act.RaiseDirtyChanged;
                        }
                    }
                }

                if (isOverwrite)
                {
                    WorkSpace.Instance.SolutionRepository.MoveSharedRepositoryItemToPrevVersion(itemToUpload.ExistingItem);

                    RepositoryFolderBase repositoryFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryFolderByPath(itemToUpload.ExistingItem.ContainingFolderFullPath);
                    if (repositoryFolder != null)
                    {
                        itemCopy.ItemName = itemToUpload.ExistingItem.ItemName;
                        repositoryFolder.AddRepositoryItem(itemCopy);
                    }
                }
                else
                {
                    WorkSpace.Instance.SolutionRepository.AddRepositoryItem(itemCopy);
                }



                if (itemToUpload.ExistingItemType == UploadItemSelection.eExistingItemType.ExistingItemIsParent && itemToUpload.ItemUploadType == UploadItemSelection.eItemUploadType.New)
                {
                    itemToUpload.UsageItem.ParentGuid = Guid.Empty;
                }
                if (itemToUpload.ReplaceType == UploadItemSelection.eActivityInstanceType.LinkInstance && !itemToUpload.UsageItem.IsLinkedItem)
                {
                    context.BusinessFlow.MarkActivityAsLink(itemToUpload.ItemGUID, itemCopy.Guid);
                }
                else if (itemToUpload.ReplaceType == UploadItemSelection.eActivityInstanceType.RegularInstance && itemToUpload.UsageItem.IsLinkedItem)
                {
                    context.BusinessFlow.UnMarkActivityAsLink(itemToUpload.ItemGUID, itemCopy.Guid);
                }
                itemToUpload.UsageItem.IsSharedRepositoryInstance = true;
                itemToUpload.ItemUploadStatus = UploadItemSelection.eItemUploadStatus.Uploaded;
                return true;
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "failed to upload the repository item", e);
                itemToUpload.ItemUploadStatus = UploadItemSelection.eItemUploadStatus.FailedToUpload;
                return false;
            }
        }

        private static RepositoryItemBase GetItemToOverrite(UploadItemSelection itemToUpload)
        {
            RepositoryItemBase itemCopy = itemToUpload.UsageItem.GetUpdatedRepoItem(itemToUpload.UsageItem, itemToUpload.ExistingItem, itemToUpload.SelectedItemPart);

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
                                selectedItem.Comment = "Uploaded with new newm" + issue.ItemNewName;
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
                        //if(linkIsByExternalID==false && linkIsByParentID==false)
                        //{
                        //    item.ParentGuid = item.Guid;
                        //}
                    }
                    else
                    {
                        item.IsSharedRepositoryInstance = false;
                    }
                }

            }
        }

        public static bool IsSharedRepositoryItem(RepositoryItemBase repositoryItem)
        {
            bool linkIsByExternalID = false;
            bool linkIsByParentID = false;

            var item = GetMatchingRepoItem(repositoryItem, null, ref linkIsByExternalID, ref linkIsByParentID);
            if (item == null)
            {
                return false;
            }
            else
            {
                return true;
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

            foreach (RepositoryItemBase existingRepoItem in existingRepoItems)
            {
                if (IsMatchingRepoItem(item, existingRepoItem, ref linkIsByExternalID, ref linkIsByParentID))
                {
                    return existingRepoItem;
                }
            }
            return null;
        }

        public static bool IsMatchingRepoItem(RepositoryItemBase item, RepositoryItemBase existingRepoItem, ref bool linkIsByExternalID, ref bool linkIsByParentID)
        {
            bool isMatch = false;
            //check if item with the same GUID already exist in repository
            if (existingRepoItem.Guid == item.Guid)
            {
                isMatch = true;
            }
            else
            {
                //check if there is already item in repo which map to a specific ExternalID
                if (!string.IsNullOrEmpty(item.ExternalID) && item.ExternalID != "0")
                {
                    if (existingRepoItem.ExternalID == item.ExternalID)
                    {
                        linkIsByExternalID = true;
                        isMatch = true;
                    }
                }
                //check if there is already item in repo which map to a specific ParentGuid
                if (item.ParentGuid != Guid.Empty)
                {
                    if (existingRepoItem.Guid == item.ParentGuid)
                    {
                        linkIsByParentID = true;
                        isMatch = true;
                    }
                }
            }
            return isMatch;
        }
        public static bool CheckIfSureDoingChange(RepositoryItemBase item, string changeType)
        {
            //RepositoryItemUsagePage usagePage = null;
            //usagePage = new RepositoryItemUsagePage(item, true);
            //if (usagePage.RepoItemUsages.Count > 0)//TODO: check if only one instance exist for showing the pop up for better performance
            //{
            //if (Reporter.ToUser(eUserMsgKey.AskIfWantsToChangeeRepoItem, item.GetNameForFileName(), usagePage.RepoItemUsages.Count, changeType) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            if (Reporter.ToUser(eUserMsgKey.AskIfWantsToChangeLinkedRepoItem, item.GetNameForFileName(), changeType) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                return true;
            }
            else
            {
                return false;
            }
            // }

            //return true;
        }

        public static void Validate(UploadItemSelection selectedItem)
        {
            if (selectedItem.ExistingItem != null)
            {
                return;
            }

            bool isDuplicateFound = CheckForItemWithDuplicateName(selectedItem);
            if (isDuplicateFound)
            {
                ItemValidationBase VA = ItemValidationBase.CreateNewIssue((RepositoryItemBase)selectedItem.UsageItem);
                VA.IssueDescription = "Item with same name already exists";
                VA.mIssueType = ItemValidationBase.eIssueType.DuplicateName;
                VA.ItemNewName = GetUniqueItemName(selectedItem);
                VA.IssueResolution = "Item will be uploaded with new name: " + VA.ItemNewName;
                VA.Selected = true;
                ItemValidationBase.mIssuesList.Add(VA);
            }
            switch (selectedItem.UsageItem.GetType().Name)
            {
                case "Activity":
                    ValidateActivity.Validate((Activity)selectedItem.UsageItem);
                    break;
            }
        }

        public static bool CheckForItemWithDuplicateName(UploadItemSelection selectedItem)
        {
            List<RepositoryItemBase> existingRepoItems = new List<RepositoryItemBase>();
            ObservableList<Activity> activities = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
            ObservableList<Act> SharedActions = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Act>();
            ObservableList<VariableBase> variables = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<VariableBase>();
            ObservableList<ActivitiesGroup> activitiesGroup = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
            if (selectedItem.UsageItem is ActivitiesGroup)
            {
                existingRepoItems = activitiesGroup.Cast<RepositoryItemBase>().ToList();
            }
            else if (selectedItem.UsageItem is Activity)
            {
                existingRepoItems = activities.Cast<RepositoryItemBase>().ToList();
            }
            else if (selectedItem.UsageItem is Act)
            {
                existingRepoItems = SharedActions.Cast<RepositoryItemBase>().ToList();
            }
            else if (selectedItem.UsageItem is VariableBase)
            {
                existingRepoItems = variables.Cast<RepositoryItemBase>().ToList();
            }

            if (selectedItem.ItemUploadType == UploadItemSelection.eItemUploadType.Overwrite)
            {
                existingRepoItems.Remove(selectedItem.ExistingItem);
            }

            foreach (object o in existingRepoItems)
            {
                if (((RepositoryItemBase)o).GetNameForFileName() == selectedItem.ItemName)
                {
                    return true;
                }
            }
            return false;
        }

        public static string GetUniqueItemName(UploadItemSelection duplicateItem)
        {
            List<string> existingRepoItems = new List<string>();
            ObservableList<Activity> activities = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
            ObservableList<Act> actions = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Act>();
            ObservableList<VariableBase> variables = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<VariableBase>();

            ObservableList<ActivitiesGroup> activitiesGroup = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
            if (duplicateItem.UsageItem is ActivitiesGroup)
            {
                existingRepoItems = activitiesGroup.Select(x => x.ItemName).ToList();
            }
            else if (duplicateItem.UsageItem is Activity)
            {
                existingRepoItems = activities.Select(x => x.ItemName).ToList();
            }
            else if (duplicateItem.UsageItem is Act)
            {
                existingRepoItems = actions.Select(x => x.ItemName).ToList();
            }
            else if (duplicateItem.UsageItem is VariableBase)
            {
                existingRepoItems = variables.Select(x => x.ItemName).ToList();
            }

            string newItemName = duplicateItem.ItemName + "_copy";
            int copyCountIndex = 0;
            while (true)
            {
                string itemNameToCheck;
                if (copyCountIndex > 0)
                {
                    itemNameToCheck = newItemName + copyCountIndex;
                }
                else
                {
                    itemNameToCheck = newItemName;
                }

                if (!existingRepoItems.Contains(itemNameToCheck))
                {
                    return itemNameToCheck;
                }
                copyCountIndex++;
            }
            //TODO - find better way to get unique name
        }


        //TODO: Not a good solution. Each time we make changes to Linked Activity, we traverse all the flows in the solution
        private static readonly object saveLock = new object();
        public static async Task UpdateLinkedInstances(Activity mActivity, string ExcludeBusinessFlowGuid = null)
        {
            try
            {
                Reporter.ToStatus(eStatusMsgKey.StaticStatusProcess, null, "Updating and Saving Linked Activity instanced in Businessflows...");
                await Task.Run(() =>
                {
                    ObservableList<BusinessFlow> BizFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
                    Parallel.ForEach(BizFlows, BF =>
                    {
                        try
                        {
                            if (!BF.ActivitiesLazyLoad && BF.Guid.ToString() != ExcludeBusinessFlowGuid && BF.Activities.Any(f => f.IsLinkedItem && f.ParentGuid == mActivity.Guid))
                            {
                                for (int i = 0; i < BF.Activities.Count(); i++)
                                {
                                    if (BF.Activities[i].IsLinkedItem && BF.Activities[i].ParentGuid == mActivity.Guid)
                                    {
                                        mActivity.UpdateInstance(BF.Activities[i], eItemParts.All.ToString(), BF);
                                    }
                                }
                                lock (saveLock)
                                {
                                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(BF);
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Failed to update the Activity in businessFlow " + BF.Name, ex);
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to update the Activity in businessFlow", ex);
            }
            finally
            {
                Reporter.HideStatusMessage();
            }
        }

        /// <summary>
        /// Save a Linked activity
        /// </summary>
        /// <param name="LinkedActivity">Linked activity used in the flow to save</param>
        /// <returns></returns>
        public static async Task SaveLinkedActivity(Activity LinkedActivity, string ExcludeBusinessFlowGuid)
        {
            Activity sharedActivity = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<Activity>(LinkedActivity.ParentGuid);
            if (sharedActivity != null)
            {
                WorkSpace.Instance.SolutionRepository.MoveSharedRepositoryItemToPrevVersion(sharedActivity);
                sharedActivity = (Activity)LinkedActivity.CreateInstance(true);
                sharedActivity.Guid = LinkedActivity.ParentGuid;
                sharedActivity.Type = eSharedItemType.Regular;
                WorkSpace.Instance.SolutionRepository.AddRepositoryItem(sharedActivity);
                LinkedActivity.EnableEdit = false;
                await UpdateLinkedInstances(sharedActivity, ExcludeBusinessFlowGuid);
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, "Activity not found in shared repository.");
            }
        }
        public async Task UpdateSharedRepositoryLinkedInstances(Activity activity)
        {
            await UpdateLinkedInstances(activity);
        }

        public async Task SaveLinkedActivityAndUpdateInstances(Activity LinkedActivity)
        {
            await SaveLinkedActivity(LinkedActivity, String.Empty);
        }
    }
}
