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
                    itemCopy = (RepositoryItemBase)item.CreateCopy(false);
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
                    
                    MovePrevVersion(itemToUpload.ExistingItem, itemToUpload.ExistingItem.FileName);
                    //To be removed from here. And and need to handle from solution repository
                    itemCopy.ContainingFolder = itemToUpload.ExistingItem.ContainingFolder;
                    itemCopy.ContainingFolderFullPath = itemToUpload.ExistingItem.ContainingFolderFullPath;
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(itemCopy);
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
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "failed to upload the repository item", e);
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


        public static void MarkSharedRepositoryItems(IEnumerable<object> items, IEnumerable<object> existingRepoItems = null)
        {
            bool linkIsByExternalID = false;
            bool linkIsByParentID = false;
            if (items != null && items.Count() > 0)
            {
                foreach (RepositoryItemBase item in items)
                {
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

        public static void MovePrevVersion(RepositoryItemBase obj, string FileName)
        {
            if (File.Exists(FileName))
            {
                string repoItemTypeFolder = GetSharedRepoItemTypeFolder(obj.GetType());
                string PrevFolder = Path.Combine(App.UserProfile.Solution.Folder, repoItemTypeFolder, "PrevVersions");
                if (!Directory.Exists(PrevFolder))
                {
                    Directory.CreateDirectory(PrevFolder);
                }
                //TODO: change to usae locale or yyyymmdd...
                string dts = DateTime.Now.ToString("MM_dd_yyyy_H_mm_ss");
                string repoName = string.Empty;
                if (obj.FileName != null && File.Exists(obj.FileName))
                {
                    repoName = obj.FileName;
                }
                string PrevFileName = repoName.Replace(repoItemTypeFolder, repoItemTypeFolder + @"\PrevVersions") + "." + dts + "." + obj.ObjFileExt;

                if (PrevFileName.Length > 255)
                {
                    PrevFileName = PrevFileName.Substring(0, 250) + new Random().Next(1000).ToString();
                }
                try
                {
                    if (File.Exists(PrevFileName))
                    {
                        File.Delete(PrevFileName);
                    }                        
                    File.Move(FileName, PrevFileName);
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eAppReporterLogLevel.ERROR, "Save Previous File got error " + ex.Message);
                }
            }
        }

        public static string GetSharedRepoItemTypeFolder(Type T)
        {
            string folder = @"SharedRepository\";

            if (T.Equals(typeof(ActivitiesGroup)))
            {
                folder += "ActivitiesGroups";
            }
            else if (T.Equals(typeof(Activity)))
            {
                folder += "Activities";
            }
            else if (T.IsSubclassOf(typeof(Act)))
            {
                folder += "Actions";
            }
            else if (T.IsSubclassOf(typeof(VariableBase)))
            {
                folder += "Variables";
            }

            if (folder == @"SharedRepository\")
            {
                throw new System.InvalidOperationException("Shared Repository Item folder path creation is wrong");
            }                

            return folder;
        }


        public static bool CheckIfSureDoingChange(RepositoryItemBase item, string changeType)
        {
            RepositoryItemUsagePage usagePage = null;
            usagePage = new RepositoryItemUsagePage(item, true);
            if (usagePage.RepoItemUsages.Count > 0)//TODO: check if only one instance exist for showing the pop up for better performance
            {
                if (Reporter.ToUser(eUserMsgKeys.AskIfWantsToChangeeRepoItem, item.GetNameForFileName(), usagePage.RepoItemUsages.Count, changeType) == MessageBoxResult.Yes)
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
