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

using Amdocs.Ginger.Common;
using System;

namespace Amdocs.Ginger.Repository
{
    public class SolutionRepositoryItemInfo<T> : SolutionRepositoryItemInfoBase
    {
        public ObservableList<T> AllItemsCache = null;
        

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

        public ObservableList<T> GetAllItemsCache()
        {
            string mLock = string.Empty;
            lock (mLock)
            {
                if (AllItemsCache == null)
                {
                    AllItemsCache = new ObservableList<T>();

                    // drill down for each sub folder and get items - combine to one list and cache
                    AllItemsCache = new ObservableList<T>(ItemRootReposiotryfolder.GetFolderItemsRecursive());
                }
                mLock = "Released";
                return AllItemsCache;
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

            //ObservableList<T> Allitems = GetAllItems();
            foreach(T x in GetAllItemsCache())
            {
                RepositoryItemBase RI = (dynamic)x;
                if (RI.Guid == guid)
                {
                    return (dynamic)RI;
                }
            }
            dynamic d = null;
            return d;           
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
    }
}
