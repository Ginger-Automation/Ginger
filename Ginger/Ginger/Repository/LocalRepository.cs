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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using Ginger.Run;
using Ginger.SourceControl;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.Platforms;
using GingerCore.Repository;
using GingerCore.Variables;
using GingerCoreNET.SourceControl;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Ginger.Repository
{

    // Moving all to use new Solutionrepository

    public class LocalRepository
    {
        //This class will  be used to read and write data to repository
        // Can be Files folder local or on shared drive, can be real DB like Oracle or SQLServer.
        

        private static ObservableList<ActivitiesGroup> mRepoActivitiesGroupsCache = null;
        // private static ObservableList<Activity> mRepoActivitiesCache = null;
        // private static ObservableList<Act> mRepoActionsCache = null;        
        private static ObservableList<RunSetConfig> mRunSetsCache = null;
        private static ObservableList<BusinessFlowExecutionSummary> mExectionResultsCache = null;        

        public bool UpdateAppProgressBar = true;

        #region General Items Cache Handling
        private ObservableList<T> GetItemTypeObjects<T>(Type itemType, ObservableList<object> cacheList, bool UseCache = true, string specificFolderPath = "", bool includeSubFolders = false, ObservableList<Guid> Tags = null)
        {
            ObservableList<object> itemsListToReturn = new ObservableList<object>();
            if (UseCache)
            {
                if (string.IsNullOrEmpty(specificFolderPath) == false)
                    if (includeSubFolders == false)
                        itemsListToReturn =
                            new ObservableList<object>(cacheList.Where(x => SetFolderPath(((RepositoryItem)x).ContainingFolderFullPath) == SetFolderPath(specificFolderPath)).ToList());
                    else
                        itemsListToReturn =
                            new ObservableList<object>(cacheList.Where(x => (SetFolderPath(((RepositoryItem)x).ContainingFolderFullPath)).Contains(SetFolderPath(specificFolderPath))).ToList());
                else
                    itemsListToReturn = cacheList;
            }
            else //not using Cache
            {
                if (string.IsNullOrEmpty(specificFolderPath) == false)
                    if (includeSubFolders == false)
                        LoadObjectsToList(itemsListToReturn, itemType, specificFolderPath);
                    else
                        LoadObjectsToListIncludingSubFolders(itemsListToReturn, itemType, specificFolderPath);
                else
                    LoadObjectsToListIncludingSubFolders(itemsListToReturn, itemType);
            }

            ObservableList<T> actualTypelist = new ObservableList<T>();
            foreach (object itemObj in itemsListToReturn)
            {
                if (Tags != null && Tags.Count != 0 && itemObj.GetType().GetField("Tags") != null)
                {
                    foreach (Guid guid in Tags)
                    {
                        ObservableList<Guid> objGuids = (ObservableList<Guid>)itemObj.GetType().GetField("Tags").GetValue(itemObj);
                        foreach (Guid oGuid in objGuids)
                        {
                            if (oGuid == guid)
                            {
                                actualTypelist.Add((T)itemObj);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    actualTypelist.Add((T)itemObj);
                }

            }

            return actualTypelist;
        }
        
        public void AddItemToCache(RepositoryItemBase itemToAdd)
        {
            //TODO: find a better way than if elses...

            
            if (itemToAdd is ActivitiesGroup)
            {
                if (mRepoActivitiesGroupsCache.Where(x => x.Guid == itemToAdd.Guid).FirstOrDefault() == null)
                    mRepoActivitiesGroupsCache.Add((ActivitiesGroup)itemToAdd);
            }                        
            
            else if (itemToAdd is RunSetConfig)
            {
                if (mRunSetsCache.Where(x => x.Guid == itemToAdd.Guid).FirstOrDefault() == null)
                    mRunSetsCache.Add((RunSetConfig)itemToAdd);
            }
            else if (itemToAdd is BusinessFlowExecutionSummary)
            {
                if (mExectionResultsCache.Where(x => x.Guid == itemToAdd.Guid).FirstOrDefault() == null)
                    mExectionResultsCache.Add((BusinessFlowExecutionSummary)itemToAdd);
            }
          
            
            

        }

        public void RemoveItemFromCache(RepositoryItemBase itemToRemove)
        {
            if (itemToRemove is ActivitiesGroup)
                mRepoActivitiesGroupsCache.Remove((ActivitiesGroup)itemToRemove);                                    
            else if (itemToRemove is RunSetConfig)
                mRunSetsCache.Remove((RunSetConfig)itemToRemove);
            else if (itemToRemove is BusinessFlowExecutionSummary)
                mExectionResultsCache.Remove((BusinessFlowExecutionSummary)itemToRemove);                        
            
            
        }

        public T GetItemByFileName<T>(Type ItemType, string fileName)
        {
            object item = null;

                   
            if (ItemType == typeof(ActivitiesGroup))
                item = mRepoActivitiesGroupsCache.Where(x => x.FileName == fileName).FirstOrDefault();                        
            
            

            if (item == null)
            {
                if (File.Exists(fileName))
                {
                    //not in cache so load it
                    item = RepositoryItem.LoadFromFile(ItemType, fileName);

                    //check if exist already by GUID
                    T existingItem = GetItemByGuid<T>(ItemType, ((RepositoryItem)item).Guid);
                    if (existingItem == null)
                    {
                        // not exist already so add it
                        AddItemToCache((RepositoryItem)item);
                    }
                    else
                    {
                        //exist already
                        return existingItem;
                    }
                }
            }

            return (T)item;
        }

        public T GetItemByGuid<T>(Type ItemType, Guid ItemGuid)
        {
            object item = null;

          
            if (ItemType == typeof(ActivitiesGroup))
                item = mRepoActivitiesGroupsCache.Where(x => x.Guid == ItemGuid).FirstOrDefault();                        
            
            

            return (T)item;
        }

        private void RefreshItemTypeCache(Type itemType, ObservableList<object> cacheList, string specificFolderPath = "")
        {
            ObservableList<object> updatedListOfItems = new ObservableList<object>();

            Dictionary<int, object> itemsToReplace = new Dictionary<int, object>();
            ObservableList<object> itemsToAdd = new ObservableList<object>();
            ObservableList<object> itemsToRemove = new ObservableList<object>();
            ObservableList<object> duplicatedItems = new ObservableList<object>();

            //get latest objects list from file system
            if (string.IsNullOrEmpty(specificFolderPath) == false)
                LoadObjectsToListIncludingSubFolders(updatedListOfItems, itemType, specificFolderPath);
            else
                LoadObjectsToListIncludingSubFolders(updatedListOfItems, itemType);

            //Update items / find missing items
            foreach (object latestItem in updatedListOfItems)
            {
                object item = cacheList.Where(x => ((RepositoryItem)x).Guid == ((RepositoryItem)latestItem).Guid).FirstOrDefault();

                if (item != null)
                {
                    //update the existing item
                    int indx = cacheList.IndexOf(item);
                    if (itemsToReplace.ContainsKey(indx) == false)
                        itemsToReplace.Add(indx, latestItem);
                    else
                        duplicatedItems.Add(latestItem);//duplicated key item
                }
                else
                {
                    itemsToAdd.Add(latestItem); //missing item
                }
            }
        

            //find items to remove
            if (string.IsNullOrEmpty(specificFolderPath) == false)//refer only to the folder items
                cacheList =
                    new ObservableList<object>(cacheList.Where(x => SetFolderPath(((RepositoryItem)x).ContainingFolderFullPath) == SetFolderPath(specificFolderPath)).ToList());
            foreach (object currentItem in cacheList)
            {
                if (updatedListOfItems.Where(x => ((RepositoryItem)x).Guid == ((RepositoryItem)currentItem).Guid).FirstOrDefault() == null)
                    itemsToRemove.Add(currentItem);
            }

            //replace existing items
            foreach (KeyValuePair<int, object> itemToReplce in itemsToReplace)
            {
                       
                if (itemType == typeof(ActivitiesGroup))
                {
                    mRepoActivitiesGroupsCache.RemoveAt(itemToReplce.Key);
                    mRepoActivitiesGroupsCache.Insert(itemToReplce.Key, (ActivitiesGroup)itemToReplce.Value);
                }                                
                
                else if (itemType == typeof(RunSetConfig))
                {
                    mRunSetsCache.RemoveAt(itemToReplce.Key);
                    mRunSetsCache.Insert(itemToReplce.Key, (RunSetConfig)itemToReplce.Value);
                }
                else if (itemType == typeof(BusinessFlowExecutionSummary))
                {
                    mExectionResultsCache.RemoveAt(itemToReplce.Key);
                    mExectionResultsCache.Insert(itemToReplce.Key, (BusinessFlowExecutionSummary)itemToReplce.Value);
                }               
                
            }

            //add missing items to cache
            foreach (object itemToAdd in itemsToAdd)
            {
              if (itemType == typeof(ActivitiesGroup))
                    mRepoActivitiesGroupsCache.Add((ActivitiesGroup)itemToAdd);                                                
                else if (itemType == typeof(RunSetConfig))
                    mRunSetsCache.Add((RunSetConfig)itemToAdd);
                else if (itemType == typeof(BusinessFlowExecutionSummary))
                    mExectionResultsCache.Add((BusinessFlowExecutionSummary)itemToAdd);                              
            }

            //delete old items from cache
            foreach (object itemToRemove in itemsToRemove)
            {
                if (itemType == typeof(ActivitiesGroup))
                    mRepoActivitiesGroupsCache.Remove((ActivitiesGroup)itemToRemove);                                                
                else if (itemType == typeof(RunSetConfig))
                    mRunSetsCache.Remove((RunSetConfig)itemToRemove);
                else if (itemType == typeof(BusinessFlowExecutionSummary))
                    mExectionResultsCache.Remove((BusinessFlowExecutionSummary)itemToRemove);               
                
            }

            //notify on Duplicated items
            if (duplicatedItems.Count > 0)
            {
                string msg =
                    duplicatedItems.Count + " items with duplicated key were found and there for were not loaded." + Environment.NewLine +
                    "Duplicate key may be resulte of manual items Copy-Paste on file system, please avoid that." + Environment.NewLine + Environment.NewLine +
                    "The duplicated items files are:" + Environment.NewLine;
                int count = 0;
                foreach (RepositoryItem duplicatedItem in duplicatedItems)
                {
                    msg += ++count + ". " + duplicatedItem.GetType().Name + ": " + duplicatedItem.FileName + Environment.NewLine + Environment.NewLine;
                }
                Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, msg);
            }
        }

        public void RefreshAllCache()
        {
            
            RefreshSolutionRepoActivitiesGroups();                       
            
            RefreshSolutionRunSetsCache();
            RefreshSolutionExectionResultsCache();          
            //RefreshSolutionPluginsCache();
        }

        public void RefreshCacheByItemType(Type itemType, string specificFolderPath = "")
        {
            
            if (itemType == typeof(ActivitiesGroup))
                RefreshSolutionRepoActivitiesGroups(specificFolderPath);
                        
            
            else if (itemType == typeof(SharedRepositorySummaryPage))
           {
                RefreshSolutionRepoActivitiesGroups();                        
            
            }
            else if (itemType == typeof(RunSetConfig))
                RefreshSolutionRunSetsCache(specificFolderPath);
            else if (itemType == typeof(BusinessFlowExecutionSummary))
                RefreshSolutionExectionResultsCache(specificFolderPath);          
        }

      
        
      

        public void ClearItemCache(Type ItemTypeToClear)
        {            
           if (ItemTypeToClear == typeof(ActivitiesGroup))
                mRepoActivitiesGroupsCache = null;                        
            
        }

        public void ClearAllCache()
        {                        
            
            mRepoActivitiesGroupsCache = null;                        

        }
        
        #endregion General Items Cache Handling



        #region Business Flows
        public ObservableList<BusinessFlow> GetSolutionBusinessFlows(bool UseCache = true, string specificFolderPath = "", bool includeSubFolders = false)
        {
            ObservableList<BusinessFlow> BFs;            
                // need to do by folder!!!!!!!!!!!!!!!!!! is it used??? not for RepositoryFolder
             BFs = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();                                     
            return BFs;
        }

       
        
        #endregion Business Flows

       
      

        #region Shared Repository Items
        public ObservableList<ActivitiesGroup> GetSolutionRepoActivitiesGroups(bool UseCache = true, string specificFolderPath = "", bool includeSubFolders = false)
        {
            if (mRepoActivitiesGroupsCache == null)
            {
                mRepoActivitiesGroupsCache = new ObservableList<ActivitiesGroup>();
                LoadObjectsToListIncludingSubFolders(mRepoActivitiesGroupsCache, typeof(ActivitiesGroup));//load cache for first time
            }
            ObservableList<object> cacheList = new ObservableList<object>(mRepoActivitiesGroupsCache.ToList());
            return GetItemTypeObjects<ActivitiesGroup>(typeof(ActivitiesGroup), cacheList, UseCache, specificFolderPath, includeSubFolders);
        }
        public void RefreshSolutionRepoActivitiesGroups(string specificFolderPath = "")
        {
            if (mRepoActivitiesGroupsCache == null) return;
            ObservableList<object> cacheList = new ObservableList<object>(mRepoActivitiesGroupsCache.ToList());
            RefreshItemTypeCache(typeof(ActivitiesGroup), cacheList, specificFolderPath);
        }
        public void AttachHandlerToSolutionRepoActivitiesGroupsChange(System.Collections.Specialized.NotifyCollectionChangedEventHandler handler)
        {
            mRepoActivitiesGroupsCache.CollectionChanged += handler;
        }

        
        

        //public void AddItemToRepositoryWithPreChecks(RepositoryItem item, BusinessFlow containingBusinessFlow = null)
        //{
        //    if (item is Activity)
        //    {
        //        Activity activity = (Activity)item;
        //        //check if the Activity is part of a group which not exist in ActivitiesGroups repository
        //        if (activity.ActivitiesGroupID != null && activity.ActivitiesGroupID != string.Empty)
        //        {
        //            ActivitiesGroup group = containingBusinessFlow.ActivitiesGroups.Where(x => x.Name == activity.ActivitiesGroupID).FirstOrDefault();
        //            if (group != null)
        //            {
        //                ObservableList<ActivitiesGroup> repoGroups = GetSolutionRepoActivitiesGroups();
        //                ActivitiesGroup repoGroup = repoGroups.Where(x => (x.Guid == group.Guid) || (x.Guid == group.ParentGuid) || (group.ExternalID != null && group.ExternalID != string.Empty && x.ExternalID == group.ExternalID)).FirstOrDefault();
        //                if (repoGroup == null)
        //                {
        //                    //offer to add also the group
        //                    if (Reporter.ToUser(eUserMsgKeys.OfferToUploadAlsoTheActivityGroupToRepository, activity.ActivityName, group.Name) == MessageBoxResult.Yes)
        //                        AddItemToRepository(group, false);
        //                }
        //            }
        //        }
        //        //check that all used variables exist in the Activity (and not taken from the BF)
        //        if (activity.WarnFromMissingVariablesUse(App.UserProfile.Solution.Variables, containingBusinessFlow.Variables, false) == false)
        //            AddItemToRepository(activity, false); 
        //    }
        //    else if (item is ActivitiesGroup)
        //    {
        //        if (AddItemToRepository(item, false) == true)
        //        {
        //            //add the Group Activities to repository
        //            foreach (ActivityIdentifiers actIdent in ((ActivitiesGroup)item).ActivitiesIdentifiers)
        //            {
        //                AddItemToRepositoryWithPreChecks(actIdent.IdentifiedActivity, containingBusinessFlow);
        //            }
        //        }
        //    }
        //    else if (item is VariableBase || item is Act)
        //    {
        //       AddItemToRepository(item, false);
        //    }
        //}

        //public void AddItemsToRepository(IEnumerable<object> items, bool inSilentMode = true)
        //{
        //    if (items != null && items.Count() > 0)
        //    {
        //        foreach (RepositoryItem item in items)
        //            if (item is RepositoryItem)
        //                AddItemToRepository(item, inSilentMode);
        //    }
        //    else
        //        if (!inSilentMode)
        //        Reporter.ToUser(eUserMsgKeys.NoItemWasSelected);
        //}

        //public bool AddItemToRepository(RepositoryItem item, bool inSilentMode = true, bool overwriteExisting = false, bool overwriteExistingParent = false)
        //{
        //    bool toOverwriteExistingRepoItem = false;
        //    bool existingItemIsExternalID = false;
        //    bool existingItemIsParent = false;
        //    string existingItemFileName = string.Empty;
        //    bool needToClearParent = false;
           
        //    try
        //    {
        //        if (item == null) return false;

        //        IEnumerable<object> existingRepoItems = new ObservableList<RepositoryItem>();
        //        if (item is ActivitiesGroup) existingRepoItems = (IEnumerable<object>)GetSolutionRepoActivitiesGroups();
        //        else if (item is Activity) existingRepoItems = (IEnumerable<object>)GetSolutionRepoActivities();
        //        else if (item is Act) existingRepoItems = (IEnumerable<object>)GetSolutionRepoActions();
        //        else if (item is VariableBase) existingRepoItems = (IEnumerable<object>)GetSolutionRepoVariables();

        //        ////check if user wants to overwrite existing item in case exist 
        //        //////check if item with the same GUID already exist in repository
                
        //        RepositoryItem exsitingItem = GetMatchingRepoItem(item, existingRepoItems, ref existingItemIsExternalID, ref existingItemIsParent);
        //        if (exsitingItem != null)
        //        {
        //            existingItemFileName = exsitingItem.FileName;
        //            if (!inSilentMode)
        //            {
        //                if (existingItemIsExternalID)
        //                {
        //                    if (Reporter.ToUser(eUserMsgKeys.ItemExternalExistsInRepository, item.GetNameForFileName(), exsitingItem.GetNameForFileName()) == MessageBoxResult.No)
        //                        return false;
        //                    else
        //                        toOverwriteExistingRepoItem = true;
        //                }
        //                else if (existingItemIsParent)
        //                {
        //                    MessageBoxResult res = Reporter.ToUser(eUserMsgKeys.ItemParentExistsInRepository, item.GetNameForFileName(), exsitingItem.GetNameForFileName());
        //                    if (res == MessageBoxResult.Yes)//overwrite parent
        //                        toOverwriteExistingRepoItem = true;
        //                    else if (res == MessageBoxResult.No)//upload as new
        //                        needToClearParent = true;
        //                    else if (res == MessageBoxResult.Cancel)
        //                        return false;
        //                }
        //                else
        //                {
        //                    if (Reporter.ToUser(eUserMsgKeys.ItemExistsInRepository, item.GetNameForFileName(), exsitingItem.GetNameForFileName()) == MessageBoxResult.No)
        //                        return false;
        //                    else
        //                        toOverwriteExistingRepoItem = true;
        //                }
        //            }
        //            else
        //            {
        //                if (existingItemIsParent == false && overwriteExisting == false)
        //                    return false;
        //                else if (existingItemIsParent == true && overwriteExistingParent == false)
        //                    return false;
        //                toOverwriteExistingRepoItem = true;
        //            }

        //            if (toOverwriteExistingRepoItem)
        //            {
        //                MovePrevVersion(exsitingItem, existingItemFileName);
        //                RemoveItemFromCache(exsitingItem);
        //                if (item is ActivitiesGroup) existingRepoItems = (IEnumerable<object>)GetSolutionRepoActivitiesGroups();
        //                else if (item is Activity) existingRepoItems = (IEnumerable<object>)GetSolutionRepoActivities();
        //                else if (item is Act) existingRepoItems = (IEnumerable<object>)GetSolutionRepoActions();
        //                else if (item is VariableBase) existingRepoItems = (IEnumerable<object>)GetSolutionRepoVariables();
        //            }
        //        }

        //        //check if same item name already exist in repository- if yes, don't allow to continue
        //        if ((existingRepoItems.Where(x => ((RepositoryItem)x).GetNameForFileName() == item.GetNameForFileName()).FirstOrDefault()) != null)
        //        {
        //            if (!inSilentMode)
        //                Reporter.ToUser(eUserMsgKeys.ItemNameExistsInRepository, item.GetNameForFileName());
        //            return false;
        //        }

        //        //Show helper
        //        Reporter.ToGingerHelper(eGingerHelperMsgKey.AddingToSharedRepository, null, item.GetNameForFileName());

        //        //create copy and update fields                
        //        RepositoryItem itemCopy = (RepositoryItem)item.CreateCopy(false);
        //        if (toOverwriteExistingRepoItem && existingItemIsParent)
        //        {
        //            //to keep the usages track
        //            itemCopy.Guid = exsitingItem.Guid;
        //            itemCopy.ParentGuid = exsitingItem.ParentGuid;
        //            itemCopy.ExternalID = exsitingItem.ExternalID;
        //        }
                
        //        itemCopy.UpdateItemFieldForReposiotryUse();

        //        //make sure the file name is unique
        //        string itemFileName = string.Empty;
        //        if (!toOverwriteExistingRepoItem)
        //            itemFileName = GetRepoItemFileName(itemCopy, Path.Combine(App.UserProfile.Solution.Folder, GetSharedRepoItemTypeFolder(itemCopy.GetType())));
        //        else
        //            itemFileName = existingItemFileName;//using the same name of the overwrited item so it will consider "Modify" and not "New" change in SVN 

        //        //Save as XML
        //        itemCopy.SaveToFile(itemFileName);

        //        //add to Cache
        //        if (itemCopy is ActivitiesGroup && mRepoActivitiesGroupsCache != null) mRepoActivitiesGroupsCache.Add((ActivitiesGroup)itemCopy);
        //        else if (itemCopy is Activity && mRepoActivitiesCache != null) mRepoActivitiesCache.Add((Activity)itemCopy);
        //        else if (itemCopy is Act && mRepoActionsCache != null) mRepoActionsCache.Add((Act)itemCopy);
        //        else if (itemCopy is VariableBase && mRepoVariablesCache != null) mRepoVariablesCache.Add((VariableBase)itemCopy);

        //        //update the uploaded source item
        //        item.IsSharedRepositoryInstance = true;
        //        if (needToClearParent)
        //            item.ParentGuid = Guid.Empty;
        //        Reporter.CloseGingerHelper();

        //        //In case existing repo item was modified (replaced) then offer to update all of it instances as well
        //        //disabling for now to simplify the upload to S.R flow
        //        if (toOverwriteExistingRepoItem && inSilentMode == false)
        //        {
        //            RepositoryItemUsagePage usagePage = null;
        //            usagePage = new RepositoryItemUsagePage(itemCopy, false, item);
        //            if (usagePage.RepoItemUsages.Count > 1)//TODO: check if only one instance exist for showing the pop up for better performance
        //            {
        //                if (Reporter.ToUser(eUserMsgKeys.AskIfWantsToUpdateRepoItemInstances, item.GetNameForFileName(), usagePage.RepoItemUsages.Count) == MessageBoxResult.Yes)
        //                {
        //                    usagePage.ShowAsWindow();
        //                }
        //            }
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Reporter.ToLog(eLogLevel.ERROR, "Failed to add the item '" + item.GetNameForFileName() + "' to Shared Repository.", ex);
        //        if (inSilentMode == false)
        //            Reporter.ToUser(eUserMsgKeys.FailedToAddItemToSharedRepository, item.GetNameForFileName(), ex.Message);
        //        return false;
        //    }
        //}

        public static bool CheckIfSureDoingChange(RepositoryItemBase item, string changeType)
        {
            RepositoryItemUsagePage usagePage = null;
            usagePage = new RepositoryItemUsagePage(item, true);
            if (usagePage.RepoItemUsages.Count > 0)//TODO: check if only one instance exist for showing the pop up for better performance
            {
                if (Reporter.ToUser(eUserMsgKeys.AskIfWantsToChangeeRepoItem, item.GetNameForFileName(), usagePage.RepoItemUsages.Count, changeType) == MessageBoxResult.Yes)
                    return true;
                else
                    return false;
            }

            return true;
        }

        #endregion Shared Repository Items

        #region Run Sets
        public ObservableList<RunSetConfig> GetSolutionRunSets(bool UseCache = true, string specificFolderPath = "", bool includeSubFolders = false)
        {
            if (mRunSetsCache == null)
            {
                mRunSetsCache = new ObservableList<RunSetConfig>();
                LoadObjectsToListIncludingSubFolders(mRunSetsCache, typeof(RunSetConfig));//load cache for first time
            }
            ObservableList<object> cacheList = new ObservableList<object>(mRunSetsCache.ToList());
            return GetItemTypeObjects<RunSetConfig>(typeof(RunSetConfig), cacheList, UseCache, specificFolderPath, includeSubFolders);
        }

        public void RefreshSolutionRunSetsCache(string specificFolderPath = "")
        {
            if (mRunSetsCache == null) return;
            ObservableList<object> cacheList = new ObservableList<object>(mRunSetsCache.ToList());
            RefreshItemTypeCache(typeof(RunSetConfig), cacheList, specificFolderPath);

            //dirty solution to keep Run tab RunSet synced with cache items after refresh
            if (App.RunsetExecutor.RunSetConfig != null)//if not null so that's mean the Run tab is loaded
            {
                RunSetConfig lastRunset = App.RunsetExecutor.RunSetConfig;               
                App.RunsetExecutor.RunSetConfig = mRunSetsCache.Where(x => x.Guid == lastRunset.Guid).FirstOrDefault();
            }
        }
        
        #endregion Run Sets

        #region Execution Results
        public ObservableList<BusinessFlowExecutionSummary> GetSolutionExectionResults(bool UseCache = true, string specificFolderPath = "", bool includeSubFolders = false)
        {
            if (mExectionResultsCache == null)
            {
                mExectionResultsCache = new ObservableList<BusinessFlowExecutionSummary>();
                LoadObjectsToListIncludingSubFolders(mExectionResultsCache, typeof(BusinessFlowExecutionSummary));//load cache for first time
            }
            ObservableList<object> cacheList = new ObservableList<object>(mExectionResultsCache.ToList());
            return GetItemTypeObjects<BusinessFlowExecutionSummary>(typeof(BusinessFlowExecutionSummary), cacheList, UseCache, specificFolderPath, includeSubFolders);
        }

        public void RefreshSolutionExectionResultsCache(string specificFolderPath = "")
        {
            if (mExectionResultsCache == null) return;
            ObservableList<object> cacheList = new ObservableList<object>(mExectionResultsCache.ToList());
            RefreshItemTypeCache(typeof(BusinessFlowExecutionSummary), cacheList, specificFolderPath);
        }
        
        #endregion Execution Results

        #region XML's Work
        public void SaveNewItem(RepositoryItemBase itemToSave, string path = "")
        {
            string FileName = GetRepoItemFileName(itemToSave, path);
            SaveObjectToFile(itemToSave, FileName);
        }

        public void DeleteItem(RepositoryItemBase itemToDelete)
        {
            File.Delete(itemToDelete.FileName);
            RemoveItemFromCache(itemToDelete);
        }

        //Generic method to load object from file
        private Object LoadObjectFromFile(Type type, string FileName, out string Error)
        {
            try
            {
                Object obj = RepositoryItem.LoadFromFile(type, FileName);
                Error = "";
                return obj;
            }

            catch (Exception e)
            {
                //checking for conflict files
                string failedFileFullPath = Path.GetFullPath(FileName);
                string fileContent = File.ReadAllText(failedFileFullPath);
                int startIndex = fileContent.IndexOf("<<<<<<<");
                if (startIndex != -1)
                {
                    //Resolving Conflict
                    ResolveConflictPage resConfPage = new ResolveConflictPage(failedFileFullPath);
                    if(App.RunningFromConfigFile == true)
                        SourceControlIntegration.ResolveConflicts(App.UserProfile.Solution.SourceControl, failedFileFullPath, eResolveConflictsSide.Server);
                    else
                        resConfPage.ShowAsWindow();
                    if (resConfPage.IsResolved)
                    {
                        return LoadObjectFromFile(type, FileName, out Error);
                    }
                    else
                    {
                        
                        Error = null; //For nor showing extra popup.
                        return null;
                    }
                }

                Error = "Error when trying to load object from file - " + Environment.NewLine;
                Error += "Type=" + type.ToString() + Environment.NewLine;
                Error += "FileName=" + FileName + Environment.NewLine;
                Error += e.Message;
                return null;
            }
        }

        //Generic method to save object to file
        // BaseType is used to override the type when we want to save the Base like in Act for later generic load //TODO: is used!?
        private void SaveObjectToFile(RepositoryItemBase obj, string FileName, Type BaseType = null)
        {
            //TODO: find a way to skip save prev version if there was no change
            // rename old file if exist with +tmp
            // do the save
            // comapre the files and decide
            // another option is to load from file to biz flow, and compare objects - not sure if it is efficent.
            MovePrevVersion(obj, FileName);
            ((RepositoryItem)obj).UpdateControlFields();

            //Save as XML
            ((RepositoryItem)obj).SaveToFile(FileName);

        }

        public void MovePrevVersion(RepositoryItemBase obj, string FileName)
        {
            if (File.Exists(FileName))
            {
                //string PrevFolder = App.UserProfile.Solution.Folder + @"\" + obj.ObjFolderName + @"\PrevVersions\";

                string repoItemTypeFolder = GetSharedRepoItemTypeFolder(obj.GetType());
                string PrevFolder=Path.Combine(App.UserProfile.Solution.Folder, repoItemTypeFolder, "PrevVersions");
                if (!Directory.Exists(PrevFolder))
                {
                    Directory.CreateDirectory(PrevFolder);
                }
                string dts = DateTime.Now.ToString("MM_dd_yyyy_H_mm_ss");
                string repoName = string.Empty;
                if (obj.FileName != null && File.Exists(obj.FileName))
                    repoName = obj.FileName;
                else
                    repoName = GetRepoItemFileName(obj);
                string PrevFileName = repoName.Replace(repoItemTypeFolder, repoItemTypeFolder + @"\PrevVersions") + "." + dts + "." + obj.ObjFileExt;

                if (PrevFileName.Length > 255)
                {
                    PrevFileName = PrevFileName.Substring(0, 250) + new Random().Next(1000).ToString();
                }
                try
                {
                    if (File.Exists(PrevFileName))
                        File.Delete(PrevFileName);
                    File.Move(FileName, PrevFileName);
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Save Previous File got error " + ex.Message);
                }
            }
        }

        public static string GetRepoItemFileName(RepositoryItemBase ri, string Folder = null, string nameAddition = null)
        {
            string riDir;
            if (string.IsNullOrEmpty(Folder))
            {
                riDir = App.UserProfile.Solution.Folder;
            }
            else
            {
                riDir = Folder;
            }

            if (!riDir.EndsWith(@"\"))
            {
                riDir = riDir + @"\";
            }

            //Used only when in root
            if (string.IsNullOrEmpty(Folder))
            {
                riDir = riDir + ri.ObjFolderName + @"\";
            }

            if (!Directory.Exists(riDir))
            {
                Directory.CreateDirectory(riDir);
            }
            string ObjName = ri.GetNameForFileName();
            if (nameAddition != null)
                ObjName += nameAddition;

            //TODO: check when it happens 
            if (ObjName == null) ObjName = "NoName";

            //Removing all possible invalid path chars and checking the file name length is legal (<= 255)                      
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                ObjName = ObjName.Replace(invalidChar.ToString(), "");
            }
            ObjName = ObjName.Replace(@".", "");

            int staticFileNameLength = (riDir + "." + ri.ObjFileExt + ".xml").Length;
            if ((ObjName.Length + staticFileNameLength) > 255)
            {
                //Trim the object name as needed to fit 255 charchters
                ObjName = ObjName.Substring(0, (ObjName.Length - ((ObjName.Length + staticFileNameLength) - 255)));
            }

            string FileName = riDir + ObjName + "." + ri.ObjFileExt + ".xml";
            //make sure the file name is unique to avoid overiding exitiing item
            int counter = 2;
            while (File.Exists(FileName))
            {
                FileName = riDir + ObjName + counter.ToString() + "." + ri.ObjFileExt + ".xml";
                counter++;
            }

            return FileName;
        }


        public void AddRepoItemToCache(RepositoryItemBase itemCopy)
        {
            if (itemCopy is ActivitiesGroup && mRepoActivitiesGroupsCache != null) mRepoActivitiesGroupsCache.Add((ActivitiesGroup)itemCopy);                        
            
        }

        private string[] GetRepoItemFiles(Type T, string Folder = null)
        {
            if (Folder == null)
            {
                string SubFolder = RepositoryItem.FolderName(T);
                Folder = App.UserProfile.Solution.Folder + @"\" + SubFolder + @"\";
            }

            //Local repo is only for old RI items save with old RS
            string ext = RepositoryItemBase.GetOldRepositoryItemFileExt(T);

            if (!Directory.Exists(Folder)) Directory.CreateDirectory(Folder);

            // Read all Ginger files in the folder
            string[] fileEntries = Directory.GetFiles(Folder, "*." + ext + ".xml");
            return fileEntries;
        }
       
        private void LoadObjectsToList(IObservableList List, Type T, string folder = null, bool UpdateProgressBar = true)
        {            

            if (RepositorySerializer.FastLoad)
            {
                 FastLoadObjectsToListAsync(List, T, folder, UpdateProgressBar);
            }
            else
            {
                string[] fileEntries = GetRepoItemFiles(T, folder);

                if (UpdateProgressBar & UpdateAppProgressBar)
                {
                    App.AppProgressBar.Init("Loading " + T.ToString(), fileEntries.Count());
                }
                foreach (string FileName in fileEntries)
                {
                    string Error = null;
                    RepositoryItem o = (RepositoryItem)LoadObjectFromFile(T, FileName, out Error);
                    if (o != null)
                    {
                        if (UpdateProgressBar & UpdateAppProgressBar)
                        {
                            App.AppProgressBar.NextItem("Loading - " + o.ToString());
                        }                        
                        List.Add(o);
                    }
                    else
                    {
                        if (Error != null)
                            Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, "Failed to load from XML - " + Error);
                    }
                }
            }

            if (UpdateProgressBar & UpdateAppProgressBar)
            {
                App.AppProgressBar.Completed();
            }
        }

        private void FastLoadObjectsToListAsync(IObservableList List, Type T, string folder, bool UpdateProgressBar)
        {
            string[] fileEntries = GetRepoItemFiles(T, folder);

            ConcurrentBag<RepositoryItem> CB = new ConcurrentBag<RepositoryItem>();
            
            Parallel.ForEach(fileEntries, FileName =>
            {
                string Error = null;
                RepositoryItem o = (RepositoryItem)LoadObjectFromFile(T, FileName, out Error);

                if (o != null)
                {
                    CB.Add(o);
                    string s = CB.ElementAt(CB.Count - 1).FileName;
                }
                else
                {
                    if (Error != null)
                        Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, "Failed to load from XML - " + Error);
                }
            });

            foreach (RepositoryItem RI in CB)
            {
                List.Add(RI);
            }
        }
        

        //TODO: run parallel !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        private void LoadObjectsToListIncludingSubFolders(IObservableList List, Type T, string rootFolderPath = "")
        {
            Console.WriteLine("LoadObjectsToListIncludingSubFolders start for type: " + T.ToString());
            Stopwatch st = Stopwatch.StartNew();

            string mainFolder;
            if (String.IsNullOrEmpty(rootFolderPath))
                mainFolder = App.UserProfile.Solution.Folder + @"\" + RepositoryItem.FolderName(T) + @"\";
            else
                mainFolder = rootFolderPath;
            LoadObjectsToList(List, T, mainFolder);
            //TODO: need to list only valid folders which can contain the item, below will go to bak folder and any junk exist in solution folder
            // We need to show in Solution all folders under - so users will see the junk
            string[] subFolders = Directory.GetDirectories(mainFolder, "*", SearchOption.AllDirectories);
            foreach (string subFolder in subFolders)
            {
                if (!subFolder.Contains("PrevVersions"))
                    LoadObjectsToList(List, T, subFolder);
            }

            st.Stop();
            Console.WriteLine("LocalRepository LoadObjectsToListIncludingSubFolders elapsed ms: " + st.ElapsedMilliseconds);
        }

        // Used in Unit Test to check perf of Repo Seri
        public void LoadObjectsToListIncludingSubFoldersForSpeedTest(string Folder, IObservableList List, Type T)
        {
            LoadObjectsToList(List, T, Folder, false);
            string[] subFolders = Directory.GetDirectories(Folder);
            foreach (string subFolder in subFolders)
            {
                LoadObjectsToList(List, T, subFolder, false);
            }
        }

        private string SetFolderPath(string Folder)
        {
            //Make sure folder is upper and adding \ if needed
            string sFolder = Folder.ToUpper();
            if (!sFolder.EndsWith(@"\")) sFolder += @"\";
            sFolder = sFolder.Replace(@"\\", @"\");
            sFolder = sFolder.Replace(@"/", @"\");
            sFolder = sFolder.Replace(@"//", @"\");
            return sFolder;
        }
        #endregion XML's Work

        #region Saveall
        public void SaveAllSolutionDirtyItems(bool refreshSolutionTree = false)
        {
            bool refreshNeeded = false;
            if (App.ItemstoSave.Count == 0)
            {
                Reporter.ToGingerHelper(eGingerHelperMsgKey.NoDirtyItem);
                Reporter.CloseGingerHelper();
                return;
            }
            try
            {
                foreach (RepositoryItem RepoItem in App.ItemstoSave)
                {
                    
                    if (RepoItem is DataSourceBase)
                    {
                        refreshNeeded = true;
                    }
                    if (System.IO.File.Exists(RepoItem.FileName))//make sure item was not already deleted
                    {
                        Reporter.ToGingerHelper(eGingerHelperMsgKey.SaveItem, null, RepoItem.GetNameForFileName(), "item");
                        RepoItem.Save();
                        Reporter.CloseGingerHelper();
                    }                           
                }
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to do Save All", e);
                Reporter.ToUser(eUserMsgKeys.Failedtosaveitems);
            }
            finally
            {
                App.ItemstoSave.Clear();
            }
            
            App.AddItemToSaveAll();

            if (refreshNeeded)
            {
                //Refresh the solution tree
                App.MainWindow.RefreshSolutionPage();
            }
            Reporter.ToUser(eUserMsgKeys.AllItemsSaved);
        }
        
        #endregion Saveall

        #region ReportTemplates
        public ObservableList<ReportTemplate> GetSolutionReportTemplates(bool UseCache = true, string specificFolderPath = "", bool includeSubFolders = false)
        {
            return WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ReportTemplate>();
        }
        #endregion ReportTemplates

        #region HTMLReportTemplates
        public ObservableList<HTMLReportTemplate> GetSolutionHTMLReportTemplates(bool UseCache = true, string specificFolderPath = "", bool includeSubFolders = false)
        {
            // TODO: use the params in !!!!!!!!!!!!!!!!
            return WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportTemplate>();
        }
        #endregion ReportTemplates

        #region HTMLReportConfigurations
        public ObservableList<HTMLReportConfiguration> GetSolutionHTMLReportConfigurations(bool UseCache = true, string specificFolderPath = "", bool includeSubFolders = false)
        {
            var list = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>();
            list.ToList().ForEach(y => y = Ginger.Reports.HTMLReportTemplatePage.EnchancingLoadedFieldsWithDataAndValidating(y));
            return list;
        }
        #endregion HTMLReportConfigurations

        #region Data Sources
        public ObservableList<DataSourceBase> GetSolutionDataSources(bool UseCache = true, string specificFolderPath = "", bool includeSubFolders = false)
        {
            return WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
        //    if (mDataSourcesCache == null)
        //    {
        //        mDataSourcesCache = new ObservableList<DataSourceBase>();
        //        LoadObjectsToListIncludingSubFolders(mDataSourcesCache, typeof(DataSourceBase));//load cache for first time
        //    }
        //    ObservableList<object> cacheList = new ObservableList<object>(mDataSourcesCache.ToList());
        //    return GetItemTypeObjects<DataSourceBase>(typeof(DataSourceBase), cacheList, UseCache, specificFolderPath, includeSubFolders);
        }
        
        #endregion Data Sources

       

        public static RunSetConfig CreateNewRunset(string runSetName, string runSetFolderPath=null)
        {        
            RunSetConfig rsc = new RunSetConfig();
            rsc.Name = runSetName;
            rsc.GingerRunners.Add(new GingerRunner() { Name = "Runner 1" });
            if (string.IsNullOrEmpty(runSetFolderPath))
                rsc.FileName = LocalRepository.GetRepoItemFileName(rsc);
            else
                rsc.FileName = LocalRepository.GetRepoItemFileName(rsc, runSetFolderPath);

            App.LocalRepository.SaveNewItem(rsc, runSetFolderPath);
            App.LocalRepository.AddItemToCache(rsc);
            return rsc;
        }

        public static BusinessFlow CreateNewBizFlow(string Name)
        {

            BusinessFlow biz = new BusinessFlow();
            biz.Name = Name;
            biz.Activities = new ObservableList<Activity>();
            biz.Variables = new ObservableList<VariableBase>();
            // Set the new BF to be same like main app
            if (App.UserProfile.Solution.MainApplication != null)
            {
                biz.TargetApplications.Add(new TargetApplication() { AppName = App.UserProfile.Solution.MainApplication });
            }

            Activity a = new Activity() { Active = true };
            a.ActivityName = GingerDicser.GetTermResValue(eTermResKey.Activity) + " 1";
            a.Acts = new ObservableList<Act>();
            if (biz.TargetApplications.Count > 0)
                a.TargetApplication = biz.TargetApplications[0].AppName;
            biz.Activities.Add(a);

            biz.Activities.CurrentItem = a;
            biz.CurrentActivity = a;
            return biz;
        }

      

        // FIXME if needed and used outside LR
        public RepositoryItemBase GetMatchingRepoItem(RepositoryItemBase item, IEnumerable<object> existingRepoItems, ref bool linkIsByExternalID, ref bool linkIsByParentID)
        {
            if (existingRepoItems == null)
            {
                if (item is ActivitiesGroup) existingRepoItems = (IEnumerable<object>)GetSolutionRepoActivitiesGroups();                                
                
            }

            linkIsByExternalID = false;
            linkIsByParentID = false;

            //check if item with the same GUID already exist in repository
            RepositoryItem repoItem = (RepositoryItem)existingRepoItems.Where(x => ((RepositoryItem)x).Guid == item.Guid).FirstOrDefault();
            //check if there is already item in repo which map to a specific ExternalID
            if (repoItem == null && item.ExternalID != null && item.ExternalID != string.Empty && item.ExternalID != "0")
            {
                repoItem = (RepositoryItem)existingRepoItems.Where(x => ((RepositoryItem)x).ExternalID == item.ExternalID).FirstOrDefault();
                if (repoItem != null) linkIsByExternalID = true;
            }
            if (repoItem == null && item.ParentGuid != Guid.Empty)
            {
                repoItem = (RepositoryItem)existingRepoItems.Where(x => ((RepositoryItem)x).Guid == item.ParentGuid).FirstOrDefault();
                if (repoItem != null) linkIsByParentID = true;
            }

            return repoItem;
        }

        public void MarkSharedRepositoryItems(IEnumerable<object> items, IEnumerable<object> existingRepoItems = null)
        {
            bool linkIsByExternalID = false;
            bool linkIsByParentID = false;
            if (items != null && items.Count() > 0)
            {
                foreach (RepositoryItemBase item in items)
                    if (GetMatchingRepoItem(item, existingRepoItems, ref linkIsByExternalID, ref linkIsByParentID) != null)
                        item.IsSharedRepositoryInstance = true;
                    else
                        item.IsSharedRepositoryInstance = false;
            }
        }
        
        public ObservableList<RepositoryItemBase> GetSolutionVEsupportedItems()
        {
            ObservableList<RepositoryItemBase> supportedItems = new ObservableList<RepositoryItemBase>();
            foreach (BusinessFlow bf in GetSolutionBusinessFlows())
                supportedItems.Add(bf);            
            foreach (Agent agent in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>())
                supportedItems.Add(agent);
            foreach (ProjEnvironment env in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>())
                supportedItems.Add(env);
            foreach (RunSetConfig set in GetSolutionRunSets())
                supportedItems.Add(set);

            return supportedItems;
        }

        public static string GetSharedRepoItemTypeFolder(Type T)
        {
            string folder = @"SharedRepository\";

            if (T.Equals(typeof(ActivitiesGroup)))
                folder += "ActivitiesGroups";
            else if (T.Equals(typeof(Activity)))
                folder += "Activities";
            else if (T.IsSubclassOf(typeof(Act)))
                folder += "Actions";
            else if (T.IsSubclassOf(typeof(VariableBase)))
                folder += "Variables";
            
            if (folder == @"SharedRepository\")
                throw new System.InvalidOperationException("Shared Repository Item folder path creation is wrong");

            return folder;
        }
    }
}

