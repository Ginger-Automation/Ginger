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
using System.ComponentModel;

namespace Amdocs.Ginger.Repository
{
    public abstract class RepositoryFolderBase : INotifyPropertyChanged
    {
        //TODO: delete
        // public static IRepositorySerializer mRepositorySerializer; // need init = new NewRepositorySerializer();

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public Type ItemType;

        public bool IsRootFolder { get; set; }

        public string FolderRelativePath { get; set; }

        public string FolderFullPath { get { return SolutionRepository.GetFolderFullPath(FolderRelativePath); } }

        public string ItemFilePattern { get; set; }

        public SolutionRepository SolutionRepository { get; set; }

        /// <summary>
        /// if this folders potentially contains Repository Items then will be marked with true
        /// Repo items which need to be upgraded - the XML contains version and we compare with current running version
        /// if Current Ginger is in higher versin we notify the user for upgrade his solution files
        /// This flag is used to drill down fast only in folder which have these type of files
        /// Like for the following: BFs, Agents etc...
        /// Folders to mark with false are: folders
        /// </summary>
        public bool ContainsRepositoryItems; // { get { return true; } }


        /// <summary>
        /// Save the Repository Item to folder and add it to cache
        /// </summary>
        /// <param name="repositoryItem"></param>
        public abstract void AddRepositoryItem(RepositoryItemBase repositoryItem);

        /// <summary>
        /// Delete the Repository Item from folder and cache
        /// </summary>
        /// <param name="repositoryItem"></param>
        public abstract void DeleteRepositoryItem(RepositoryItemBase repositoryItem);

        public abstract void SaveRepositoryItem(string fileName, string txt);

        /// <summary>
        /// Add sub folder with provided name
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public abstract RepositoryFolderBase AddSubFolder(string folderName);

        /// <summary>
        /// Rename the folder and it items related fields on file system and cache (recursive)
        /// </summary>
        /// <param name="newFolderName"></param>
        public abstract void RenameFolder(string newFolderName);

        /// <summary>
        /// Clear folder items & sub folders cache recursive
        /// </summary>
        /// <param name="newFolderName"></param>
        // public abstract void ClearFolderCache();


        /// <summary>
        /// Clear folder items & sub folders cache recursive and then reload the folder items to cache recursive
        /// </summary>
        /// <param name="newFolderName"></param>
        // public abstract void RefreshFolderCache();

        public abstract void ReloadItems();


        public virtual void StartFileWatcher()
        {
            // handled in Repository Folder
        }

        public virtual void StopFileWatcherRecursive()
        {
            // handled in Repository Folder
        }

        public virtual void PauseFileWatcher()
        {
            // handled in Repository Folder
        }

        public virtual void ResumeFileWatcher()
        {
            // handled in Repository Folder
        }


        /// <summary>
        /// Get Items located in current folder as RepositoryItemBase list
        /// Better to use GetFolderItems() if possible to have binding to exact folder items list and not duplication of it
        /// </summary>
        /// <returns></returns>
        public abstract ObservableList<RepositoryItemBase> GetFolderRepositoryItems();

        public abstract RepositoryFolderBase GetSubFolderByName(string name, bool recursive = false);

        /// <summary>
        /// Move Repository item from current folder to another folder
        /// </summary>
        /// <param name="repositoryFolder"></param>
        public abstract void MoveItem(RepositoryItemBase repositoryItem, RepositoryFolderBase repositoryFolder);
    }
}
