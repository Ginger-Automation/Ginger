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
using Amdocs.Ginger.Common;

namespace Amdocs.Ginger.Repository
{
    public class SolutionRepositoryItemInfo<T> : SolutionRepositoryItemInfoBase
    {
        private readonly object mAllItemsCacheLock = new object();
        private bool mDoneAllCache = false;

        private ObservableList<T> mAllItemsCache = null;
        private ObservableList<T> AllItemsCache
        {
            get
            {
                if (!mDoneAllCache)                
                {
                    // We use lock since several threads can request AllItems at the same time when it was not initialized yet
                    // if one thread start getting all items we want other threads to wait for it to complete 
                    // so they don't get partial list while work is in progress                                        
                    lock (mAllItemsCacheLock)
                    {
                        if (!mDoneAllCache)// make sure all thread which were waiting just return back, only the first enry will do the work
                        {
                            // drill down for each sub folder and get items - combine to one list and cache                        
                            mAllItemsCache = new ObservableList<T>(ItemRootReposiotryfolder.GetFolderItemsRecursive());
                            mDoneAllCache = true;                            
                        }                     
                    }                    
                }
                return mAllItemsCache;
            }
        }

        public void AddItemToCache(T newItem)
        {
            lock (AllItemsCache)
            {
                AllItemsCache.Add(newItem);
            }
        }

        public SolutionRepositoryItemInfo()
        {
            base.ItemType = typeof(T);  
        }

        public RepositoryFolder<T> ItemRootReposiotryfolder { get; internal set; }

        public override RepositoryFolderBase ItemRootRepositoryFolder
        {
            get
            {
                return ItemRootReposiotryfolder;
            }
        }

        
        

        /// <summary>
        /// Delete the Repository Item folder and it sub folders from file system and cache
        /// </summary>
        /// <param name="repositoryFolder"></param>
        public override void DeleteRepositoryItemFolder(RepositoryFolderBase repositoryFolder)
        {
            RepositoryFolder<T> itemRepositoryParentFolder = GetRepositoryFolderParent(repositoryFolder);
            itemRepositoryParentFolder.DeleteSubFolder(repositoryFolder);
        }
        

        /// <summary>
        /// Get Item By GUID- to be used by SolutionRepository class ONLY
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        internal T GetItemByGuid(Guid guid)
        {
            //TODO: first look in items cache instead of getting all items
            foreach(T x in AllItemsCache)
            {
                RepositoryItemBase RI = (RepositoryItemBase)(object)x;
                if (RI.Guid == guid)
                {
                    return (T)(object)RI;
                }
            }
            object nullObject  = null;
            return (T)nullObject;           
        }


        /// <summary>
        /// Get the parent folder of the Repository Item
        /// </summary>
        /// <param name="repositoryItem"></param>
        /// <returns></returns>
        public override RepositoryFolderBase GetItemRepositoryFolder(RepositoryItemBase repositoryItem)
        {
            return GetRepositoryFolder(repositoryItem, null);
        }

        /// <summary>
        /// Recursive function to find the repositry item folder
        /// </summary>
        /// <param name="repositoryItem"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        RepositoryFolder<T> GetRepositoryFolder(RepositoryItemBase repositoryItem, RepositoryFolder<T> folder = null)
        {
            if (folder == null)
            {
                folder = ItemRootReposiotryfolder;
            }

            if (folder.FolderFullPath.Trim().ToLower() == repositoryItem.ContainingFolderFullPath.Trim().ToLower())
            {
                return folder;
            }
            else
            {
                foreach (RepositoryFolder<T>  subfolder in folder.GetSubFolders())
                {
                    if (subfolder.FolderRelativePath == repositoryItem.ContainingFolder)
                    {
                        return subfolder;
                    }
                    else
                    {
                        RepositoryFolder<T> sf = GetRepositoryFolder(repositoryItem, subfolder);
                        if (sf != null) return sf;
                    }
                }                
            }
            
            return null;
        }

        internal bool AllItemsContains(T item)
        {
            if (AllItemsCache.Contains(item))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Recursive function to find the repositry item folder
        /// </summary>
        /// <param name="repositoryFolder"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        RepositoryFolder<T> GetRepositoryFolderParent(RepositoryFolderBase repositoryFolder, RepositoryFolder<T> folder = null)
        {
            if (folder == null)
            {
                folder = ItemRootReposiotryfolder;
            }

            foreach (RepositoryFolder<T> subfolder in folder.GetSubFolders())
            {
                if (subfolder.FolderFullPath.Trim().ToLower() == repositoryFolder.FolderFullPath.Trim().ToLower())
                {
                    return folder;
                }
                else
                {
                    RepositoryFolder<T> sf = GetRepositoryFolderParent(repositoryFolder, subfolder);
                    if (sf != null) return sf;
                }
            }

            return null;
        }

        internal void AllItemsCacheRemove(T item)
        {
            // Lock in case 2 remove will happen from seperate threads
            lock (mAllItemsCacheLock)
            {
                AllItemsCache.Remove(item);
            }
        }

        internal bool AllItemsCacheIsNull()
        {
            if (mAllItemsCache is null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal ObservableList<T> GetAllItemsCache()
        {
            return AllItemsCache;
        }
    }
}
