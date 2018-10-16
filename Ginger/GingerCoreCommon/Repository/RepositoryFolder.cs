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
using Amdocs.Ginger.IO;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Repository
{
    public class RepositoryFolder<T> : RepositoryFolderBase
    {
        SolutionRepositoryItemInfo<T> mSolutionRepositoryItemInfo = null;

        ObservableList<T> mFolderItemsList = null;        

        RepositoryCache mFolderItemsCache = new RepositoryCache(typeof(T));

        ObservableList<RepositoryFolder<T>> mSubFoldersCache = null;

        // Watch for file and folder changes on this folder
        private FileSystemWatcher mFileWatcher = null;

        public RepositoryFolder(SolutionRepository SolutionRepository, SolutionRepositoryItemInfo<T> solutionRepositoryItemInfo, string pattern, string FolderRelativePath, bool ContainsRepositoryItems = true, string DisplayName = null, bool isRootFolder = false)
        {
            this.SolutionRepository = SolutionRepository;
            mSolutionRepositoryItemInfo = solutionRepositoryItemInfo;
            this.FolderRelativePath = FolderRelativePath;
            //this.FolderFullPath = SolutionRepository.GetFolderFullPath(FolderRelativePath);
            this.DisplayName = DisplayName;
            this.ContainsRepositoryItems = ContainsRepositoryItems;
            this.ItemFilePattern = pattern;
            base.ItemType = typeof(T);
            IsRootFolder = isRootFolder;            
        }

        ~RepositoryFolder()
        {
            if (mFileWatcher != null)
            {
                mFileWatcher.EnableRaisingEvents = false;
                mFileWatcher.Dispose();
            }
        }

        string mDisplayName = string.Empty;
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(mDisplayName))
                    return FolderName;
                return mDisplayName;
            }
            set
            {
                if (value != mDisplayName)
                {
                    mDisplayName = value;
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        public string FolderName
        {
            get
            {
                return Path.GetFileName(PathHelper.GetLongPath(FolderFullPath));
            }
        }

        //private string GetItemTypeForFileName(Type t)
        //{
        //    string s = RepositoryItemBase.GetClassShortName(t);
        //    return s;
        //}

        /// <summary>
        /// Get list of all current folder sub folders
        /// </summary>
        /// <returns></returns>
        public virtual ObservableList<RepositoryFolder<T>> GetSubFolders()
        {
            if (mSubFoldersCache == null)
            {
                mSubFoldersCache = GetDirectorySubFolders(this, this.ContainsRepositoryItems);
            }
            return mSubFoldersCache;
        }

        private ObservableList<RepositoryFolder<T>> GetDirectorySubFolders(RepositoryFolder<T> Folder, bool ContainsRepositoryItems)
        {
            ObservableList<RepositoryFolder<T>> list = new ObservableList<RepositoryFolder<T>>();
            string FullPath = SolutionRepository.GetFolderFullPath(Folder.FolderRelativePath);
            string[] folders = FileSystem.GetDirectorySubFolders(FullPath);
            foreach (string subFolder in folders)
            {
                //string DisplayName = Path.GetFileName(subFolder);
                string relativePath = Path.Combine(FolderRelativePath, Path.GetFileName(PathHelper.GetLongPath(subFolder)));
                RepositoryFolder<T> sf = new RepositoryFolder<T>(SolutionRepository, mSolutionRepositoryItemInfo, Folder.ItemFilePattern, relativePath, ContainsRepositoryItems); // Each sub folder is like it's parent type                                
                sf.StartFileWatcher();                                                    
                //sf.FolderFullPath = Path.Combine(FullPath, subFolder);
                list.Add(sf);
            }
            return list;
        }

        /// <summary>
        /// Get Items located in current folder
        /// </summary>
        /// <returns></returns>
        public ObservableList<T> GetFolderItems()
        {            
            if (mFolderItemsList == null)
            {
                ObservableList<T> list = LoadFolderFiles<T>(FolderFullPath);
                mFolderItemsList = list;

                //add it to general item cache if needed
                if (mSolutionRepositoryItemInfo.AllItemsCache != null)
                {
                    foreach (T item in list)
                        if (mSolutionRepositoryItemInfo.AllItemsCache.Contains(item) == false)
                            mSolutionRepositoryItemInfo.AllItemsCache.Add(item);
                }                
            }

            return mFolderItemsList;
        }

        /// <summary>
        /// Get Items located in current folder as RepositoryItemBase list
        /// Better to use GetFolderItems() if possible to have binding to exact folder items list and not duplication of it
        /// </summary>
        /// <returns></returns>
        public override ObservableList<RepositoryItemBase> GetFolderRepositoryItems()
        {
            GetFolderItems();

            ObservableList<RepositoryItemBase> folderItems = new ObservableList<RepositoryItemBase>();
            foreach (object item in mFolderItemsList)
                folderItems.Add((RepositoryItemBase)item);

            return folderItems;
        }

        /// <summary>
        /// Return the list of Sub Folders
        /// </summary>
        /// <returns></returns>
        public override ObservableList<RepositoryFolderBase> GetSubFoldersAsFolderBase()
        {
            ObservableList<RepositoryFolderBase> subFolders = new ObservableList<RepositoryFolderBase>();
            foreach (object item in GetSubFolders())
            {
                subFolders.Add((RepositoryFolderBase)item);
            }

            return subFolders;
        }

        /// <summary>
        /// Get current folder and all of it sub folders items
        /// </summary>        
        /// <param name="list"></param>
        public ConcurrentBag<T> GetFolderItemsRecursive(ConcurrentBag<T> list = null)
        {
            if (list == null)
                list = new ConcurrentBag<T>();

            foreach (T item in GetFolderItems())
            {
                list.Add(item);
            }

            ObservableList<RepositoryFolder<T>> subfolders = GetSubFolders();

            Parallel.ForEach(subfolders, sf =>
            {
                sf.GetFolderItemsRecursive(list);
            });

            return list;
        }

        /// <summary>
        /// Delete current folder and all of it sub folders items
        /// </summary>        
        /// <param name="list"></param>
        private void DeleteFolderCacheItemsRecursive()
        {
            ObservableList<T> cacheItems = new ObservableList<T>();
            foreach (T item in mFolderItemsCache.Items<T>())
                cacheItems.Add(item);//creating list to iterate over
            foreach (T cacheItem in cacheItems)
                DeleteRepositoryItem((dynamic)cacheItem);

            if (mSubFoldersCache != null)
            {
                foreach (RepositoryFolder<T> sf in mSubFoldersCache)
                {
                    sf.DeleteFolderCacheItemsRecursive();
                }
            }
        }




        // Generic handling for any RI type
        // This is recursive function which run in parallel for extreme speed, be carefull! 
        private ObservableList<T> LoadFolderFiles<T>(string Folder = null)
        {
            // for each file we check if in cache return from cache else load from file system and cache the item             

            string FullPath = SolutionRepository.GetFolderFullPath(Folder);

            if (FullPath == null || !Directory.Exists(PathHelper.GetLongPath(FullPath)))
            {
                AppReporter.ToLog(eAppReporterLogLevel.ERROR, "RepositoryFolder/LoadFolderFiles- Invalid folder: " + Folder);
                return null;                
            }

            // TODO: move from here to better place                
            string ContainingFolder = Folder.Replace(SolutionRepository.SolutionFolder, SolutionRepository.cSolutionRootFolderSign); 

            ConcurrentBag<T> list = new ConcurrentBag<T>(); // Thread safe list

            string[] fileEntries = FileSystem.GetDirectoryFiles(FullPath, mSolutionRepositoryItemInfo.Pattern);

            Parallel.ForEach(fileEntries, FileName =>
            {
                try
                {
                    // Check if item exist in cache if yes use it, no need to load from file, yay!
                    T item = (T)mFolderItemsCache[FileName];
                    if (item == null)
                    {
                        item = LoadItemfromFile<T>(FileName, ContainingFolder);
                        AddItemtoCache(FileName, item);
                    }
                    list.Add(item);
                }
                catch(Exception ex)
                {
                    AppReporter.ToLog(eAppReporterLogLevel.ERROR, string.Format("RepositoryFolder/LoadFolderFiles- Failed to load the Repository Item XML which in file: '{0}'.", FileName), ex);
                }
            });

            return new ObservableList<T>(list); //TODO: order by name .OrderBy(x => ((RepositoryItem)x).FilePath)); ??
        }


        T LoadItemfromFile<T>(string fileName, string containingFolder)
        {
            T item = (T)SolutionRepository.RepositorySerializer.DeserializeFromFileObj(typeof(T), fileName);
            SetRepositoryItemInfo(item, fileName, containingFolder);
            return item;
        }

        void AddItemtoCache(string fileName, object item)
        {
            mFolderItemsCache[fileName] = item;
        }

        void SetRepositoryItemInfo(object item, string fileName, string containingFolder)
        {
            RepositoryItemBase rbb = (RepositoryItemBase)item;

            // why do we need both?
            rbb.FileName = fileName;
            rbb.FilePath = fileName;
            // rbb.UseSolutionRepository = true;
            rbb.ContainingFolder = containingFolder;
        }
        

        public override void StartFileWatcher()
        {
            try
            {
                mFileWatcher = new FileSystemWatcher();
                mFileWatcher.Path = base.FolderFullPath;
                
                //TODO: for documents or other need to have all !!!! or get from SRII the extension to watch not all...
                // for now we do all xml
                // mFileWatcher.Filter = "*.xml";

                mFileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                mFileWatcher.IncludeSubdirectories = false;

                mFileWatcher.Changed += new FileSystemEventHandler(FileWatcher_Changed);
                mFileWatcher.Deleted += new FileSystemEventHandler(FileWatcher_Changed);
                mFileWatcher.Created += new FileSystemEventHandler(FileWatcher_Changed);
                mFileWatcher.Renamed += new RenamedEventHandler(FileWatcher_Renamed);
               
                mFileWatcher.EnableRaisingEvents = true;
            }
            catch(Exception ex)
            {
                throw new Exception("StartFileWatcher failed for " + base.FolderFullPath + " " + ex.Message);
            }
        }

        public override void StopFileWatcherRecursive()
        {
            if (mFileWatcher != null)
            {
                mFileWatcher.EnableRaisingEvents = false;
                mFileWatcher = null;
                foreach (RepositoryFolderBase RF in GetSubFolders())
                {
                    RF.StopFileWatcherRecursive();
                }
            }
        }

        public override void PauseFileWatcher()
        {
            if (mFileWatcher == null) return;
            if (mFileWatcher.EnableRaisingEvents != false)
            {
                mFileWatcher.EnableRaisingEvents = false;
            }
            else
            {
                throw new Exception("RepositoryFolder.PauseFileWatcher is already EnableRaisingEvents = false");
            }           
        }

        public override void ResumeFileWatcher()
        {
            if (mFileWatcher == null) return;
            if (mFileWatcher.EnableRaisingEvents != true)
            {
                mFileWatcher.EnableRaisingEvents = true;
            }
            else
            {
                throw new Exception("RepositoryFolder.PauseFileWatcher is already EnableRaisingEvents = true");
            }            
        }

        private void FileWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            try
            {
                // check if it is directory
                if (Directory.Exists(PathHelper.GetLongPath(e.FullPath)))
                {
                    string fn = Path.GetFileName(PathHelper.GetLongPath(e.OldFullPath));
                    RepositoryFolder<T> sf = GetSubFolder(fn);
                    sf.DisplayName = e.Name;
                    sf.FolderRelativePath = ReplaceLastOccurrence(sf.FolderRelativePath, fn, e.Name);
                    return;
                }

                // this is single repository item
                RepositoryItemBase item = GetItemFromCacheByFileName(e.OldFullPath);
                item.FileName = e.FullPath;
                item.FilePath = e.FullPath;

                // set Folder item cache as it depends ont he file name, so remove the old name and add with new name
                mFolderItemsCache.DeleteItem(e.OldFullPath);
                mFolderItemsCache[e.FullPath] = item;

                RepositoryItemBase item2 = GetItemFromCacheByFileName(e.FullPath);
            }
            catch(Exception ex)
            {
                AppReporter.ToLog(eAppReporterLogLevel.ERROR, "Exception thrown from ReposiotryFolder/FileWatcher", ex, true);
            }

        }

        //TODO: move to global functions or create string extension
        public static string ReplaceLastOccurrence(string Source, string Find, string Replace)
        {
            int place = Source.LastIndexOf(Find);

            if (place == -1)
                return Source;

            string result = Source.Remove(place, Find.Length).Insert(place, Replace);
            return result;
        }

        // using a mutext to queue the changes, in case we get duplicates we process one by one
        Mutex m = new Mutex();
        private void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            AppReporter.ToConsole("FileWatcher change detected: " + e.FullPath + " , " + e.ChangeType);
            try
            {                
                m.WaitOne();
                {
                    if (e.ChangeType == WatcherChangeTypes.Deleted)
                    {
                        if((from x in mSubFoldersCache where x.FolderName == e.Name select x).SingleOrDefault() != null)
                        {
                            HandleDirecortyChange(e);
                        }
                        else
                        {
                            HandleFileChange(e);
                        }
                    }
                    else
                    {
                        if (Directory.Exists(PathHelper.GetLongPath(e.FullPath)))
                        {
                            HandleDirecortyChange(e);
                        }
                        else
                        {
                            HandleFileChange(e);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                AppReporter.ToLog(eAppReporterLogLevel.ERROR, "Exception thrown from ReposiotryFolder/FileWatcher", ex, true);
            }
                        
            finally
            {
                m.ReleaseMutex();
            }
            AppReporter.ToConsole("FileWatcher change handled: " + e.FullPath + " , " + e.ChangeType);
        }

        private void HandleFileChange(FileSystemEventArgs e)
        {            
            RepositoryItemBase item = null;

            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Changed:
                    WaitforFileIsReadable(e.FullPath);
                    // reLoad the object to mem updating fields
                    item = GetItemFromCacheByFileName(e.FullPath);                    
                    NewRepositorySerializer.ReloadObjectFromFile(item);
                    item.RefreshSourceControlStatus();
                    SolutionRepository.RefreshParentFoldersSoucerControlStatus(Path.GetDirectoryName(e.FullPath));
                    break;
                case WatcherChangeTypes.Deleted:
                    //remove from cache and list
                    item = GetItemFromCacheByFileName(e.FullPath);
                    RemoveItemFromLists(item);
                    SolutionRepository.RefreshParentFoldersSoucerControlStatus(Path.GetDirectoryName(e.FullPath));                    
                    break;
                case WatcherChangeTypes.Created:
                    WaitforFileIsReadable(e.FullPath);
                    // add item to cache and list
                    T newItem = LoadItemfromFile<T>(e.FullPath, Path.GetDirectoryName(e.FullPath));
                    AddItemtoCache(e.FullPath, newItem);
                    mFolderItemsList.Add(newItem);
                    SolutionRepository.RefreshParentFoldersSoucerControlStatus(Path.GetDirectoryName(e.FullPath));
                    break;
            }
        }

        private void HandleDirecortyChange(FileSystemEventArgs e)
        {            
            string fn = Path.GetFileName(PathHelper.GetLongPath(e.FullPath));
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    string relativeFolder = FolderRelativePath + Path.DirectorySeparatorChar + e.Name; 
                    RepositoryFolder<T> subfolder = new RepositoryFolder<T>(SolutionRepository, mSolutionRepositoryItemInfo, ItemFilePattern, relativeFolder, ContainsRepositoryItems, null);
                    GetSubFolders().Add(subfolder);
                    break;
                case WatcherChangeTypes.Changed:
                    // change happen when new file is added, for now we can ignore, as we will get also file added event
                    break;
                case WatcherChangeTypes.Deleted:
                    RepositoryFolder<T> sf2 = GetSubFolder(fn);
                    sf2.DeleteFolderCacheItemsRecursive();

                    //delete the folder from folders cache  
                    if (mSubFoldersCache != null)
                    {
                        mSubFoldersCache.Remove(sf2);
                    }

                    break;
            }
            return;
        }

        void WaitforFileIsReadable(string fileName)
        {
            //retry max 10 times
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    FileStream fs = File.OpenRead(fileName);
                    fs.Close();
                    fs.Dispose();
                    return;
                }
                catch (IOException)
                {
                    // enable some time in case the file is in the middle of the save, so we get IOException used by other process
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return;
        }

        RepositoryItemBase GetItemFromCacheByFileName(string fileName)
        {
            foreach (RepositoryItemBase item in mFolderItemsCache.Items<RepositoryItemBase>())
            {
                if (item != null && item.FilePath == fileName)
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// Get sub folder by it name
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public RepositoryFolder<T> GetSubFolder(string folderName)
        {
            foreach (RepositoryFolder<T> subFolder in GetSubFolders())
            {
                if (subFolder.FolderName.Trim() == folderName.Trim())
                    return subFolder;
            }

            return null;
        }

        /// <summary>
        /// Add sub folder with provided name
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public override RepositoryFolderBase AddSubFolder(string folderName)
        {            
            string FullPath = Path.Combine(FolderFullPath, folderName);

            //add to folders cache
            string relativeFolder = FolderRelativePath + Path.DirectorySeparatorChar + folderName;    
            RepositoryFolder<T> subfolder = new RepositoryFolder<T>(SolutionRepository, mSolutionRepositoryItemInfo, ItemFilePattern, relativeFolder, ContainsRepositoryItems, null);
                        
            PauseFileWatcher();
            //add to file system
            try
            {
                Directory.CreateDirectory(PathHelper.GetLongPath(FullPath));
                GetSubFolders().Add(subfolder);
                subfolder.StartFileWatcher();
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                ResumeFileWatcher();
            }
            
            return subfolder;
        }

        /// <summary>
        /// Save the Repository Item to folder and add it to cache
        /// </summary>
        /// <param name="repositoryItem"></param>
        public override void AddRepositoryItem(RepositoryItemBase repositoryItem)
        {            
            //save it
            repositoryItem.ContainingFolder = FolderRelativePath;
            repositoryItem.ContainingFolderFullPath = FolderFullPath;            
            SolutionRepository.SaveNewRepositoryItem(repositoryItem);

            //add it to folder cache
            mFolderItemsCache[repositoryItem.FilePath] = repositoryItem;
            if (mFolderItemsList != null)
                mFolderItemsList.Add((dynamic)repositoryItem);

            //add it to general item cache
            if (mSolutionRepositoryItemInfo.AllItemsCache != null)
                mSolutionRepositoryItemInfo.AllItemsCache.Add((dynamic)repositoryItem);
        }

        /// <summary>
        /// Delete the Repository Item from folder and cache
        /// </summary>
        /// <param name="repositoryItem"></param>
        public override void DeleteRepositoryItem(RepositoryItemBase repositoryItem)
        {
            //delete from file system
            if (File.Exists(repositoryItem.FilePath))
            {
                if (mFileWatcher != null)
                {
                    mFileWatcher.EnableRaisingEvents = false;
                }
                File.Delete(repositoryItem.FilePath);
                if (mFileWatcher != null)
                {
                    mFileWatcher.EnableRaisingEvents = true;
                }
            }
            else
            {
                throw new Exception("Failed to delete RI File not found - " + repositoryItem.FilePath);
            }

            RemoveItemFromLists(repositoryItem);
        }


        public override void SaveRepositoryItem(string fileName, string txt)
        {                        
            PauseFileWatcher();            
            File.WriteAllText(fileName, txt);
            ResumeFileWatcher();            
        }

        void RemoveItemFromLists(RepositoryItemBase repositoryItem)
        {
            //Delete from folder cache
            mFolderItemsCache.DeleteItem(repositoryItem.FilePath);
            if (mFolderItemsList != null)
                mFolderItemsList.Remove((dynamic)repositoryItem);


            //Delete it from general item cache
            if (mSolutionRepositoryItemInfo.AllItemsCache != null)
                mSolutionRepositoryItemInfo.AllItemsCache.Remove((dynamic)repositoryItem);

        }

        /// <summary>
        /// Delete provided sub folder (recursive) and all of it items from file system and cache
        /// </summary>
        /// <param name="repositoryFolder"></param>
        /// <returns></returns>
        public bool DeleteSubFolder(RepositoryFolderBase repositoryFolder)
        {
            RepositoryFolder<T> subfolder = (RepositoryFolder<T>)repositoryFolder;
            if (subfolder != null)
            {
                //delete all sub folder inner items from file system and cache
                subfolder.DeleteFolderCacheItemsRecursive();

                //delete the folder from file system                
                if (Directory.Exists(PathHelper.GetLongPath(subfolder.FolderFullPath)))
                {
                    PauseFileWatcher();
                    Directory.Delete(PathHelper.GetLongPath(subfolder.FolderFullPath), true);
                    ResumeFileWatcher();
                }
                else
                {
                    throw new Exception("Failed to delete the Folder- Path not found - " + subfolder.FolderFullPath);
                }

                //delete the folder from folders cache  
                if (mSubFoldersCache != null)
                {
                    //GetSubFolders().Remove(subfolder);
                    mSubFoldersCache.Remove(subfolder);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Rename the folder and it items related fields on file system and cache (recursive)
        /// </summary>
        /// <param name="newFolderName"></param>
        public override void RenameFolder(string newFolderName)
        {
            //update the folder name in file system
            string newFullPath = Path.Combine(Path.GetDirectoryName(PathHelper.GetLongPath(FolderFullPath)), newFolderName);
            if (FolderName.ToUpper() == newFolderName.ToUpper())//user just changed the name letters case
            {
                //move to temp folder
                string tempTargetPath = Path.Combine(Path.GetTempPath(), FolderName);
                Directory.Move(PathHelper.GetLongPath(FolderFullPath), PathHelper.GetLongPath(tempTargetPath));
                Directory.Move(PathHelper.GetLongPath(tempTargetPath), PathHelper.GetLongPath(newFullPath));
            }
            else
            {                
                Directory.Move(PathHelper.GetLongPath(FolderFullPath), PathHelper.GetLongPath(newFullPath));             
            }
            //Enable file watcher to catch the change first, so it will be visible in UI
            Thread.Sleep(100);
            
            //update folder fields            
            FolderRelativePath = Path.Combine(FolderRelativePath.Substring(0, FolderRelativePath.LastIndexOf(FolderName)), newFolderName); //parentFolderRelativePath + "/" + FolderName;
            OnPropertyChanged(nameof(FolderRelativePath));
            OnPropertyChanged(nameof(FolderFullPath));
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(FolderName));

            //updating the folder items cache with correct File Path
            UpdateFolderItemsCacheFilePath();

            //update the sub folders and their items in cache
            if (mSubFoldersCache != null)
            {
                foreach (RepositoryFolder<T> sf in mSubFoldersCache)
                {
                    sf.UpdateSubFolderFieldsAndItemsPath(FolderRelativePath);
                }
            }
        }

        private void UpdateFolderItemsCacheFilePath()
        {
            ObservableList<T> cacheItems = new ObservableList<T>();
            foreach (T item in mFolderItemsCache.Items<T>())
                cacheItems.Add(item);
            foreach (T cacheitem in cacheItems)
            {
                RepositoryItemBase item = (dynamic)cacheitem;
                mFolderItemsCache.DeleteItem(item.FilePath);
                item.FilePath = Path.Combine(FolderFullPath, Path.GetFileName(PathHelper.GetLongPath(((RepositoryItemBase)item).FilePath)));
                item.ContainingFolder = FolderRelativePath;
                item.ContainingFolderFullPath = FolderFullPath;
                mFolderItemsCache[item.FilePath] = item;
                item.OnPropertyChanged(nameof(RepositoryItemBase.FilePath));
                item.OnPropertyChanged(nameof(RepositoryItemBase.ContainingFolder));
                item.OnPropertyChanged(nameof(RepositoryItemBase.ContainingFolderFullPath));
                item.OnPropertyChanged(nameof(RepositoryItemBase.RelativeFilePath));
            }
        }

        private void UpdateSubFolderFieldsAndItemsPath(string parentFolderRelativePath)
        {
            //update Folder fields
            FolderRelativePath = Path.Combine(parentFolderRelativePath, FolderName); //parentFolderRelativePath + "/" + FolderName;
            OnPropertyChanged(nameof(FolderRelativePath));
            OnPropertyChanged(nameof(FolderFullPath));
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(FolderName));

            //update folder items
            UpdateFolderItemsCacheFilePath();

            if (mSubFoldersCache != null)
            {
                foreach (RepositoryFolder<T> sf in mSubFoldersCache)
                {
                    sf.UpdateSubFolderFieldsAndItemsPath(FolderRelativePath);
                }
            }
        }

        /// <summary>
        /// Clear folder items & sub folders cache recursive
        /// </summary>
        /// <param name="newFolderName"></param>
        //public override void ClearFolderCache()
        //{
        //    //Clear sub folders cache
        //    if (mSubFoldersCache != null)
        //    {
        //        foreach (RepositoryFolder<T> sf in mSubFoldersCache)
        //            sf.ClearFolderCache();

        //        mSubFoldersCache = null;
        //    }

        //    //clear folder items list
        //    if (mFolderItemsList != null)
        //    {
        //        //clear items from local & general items list
        //        while (mFolderItemsList.Count > 0)
        //        {
        //            if (mSolutionRepositoryItemInfo.AllItemsCache != null)
        //                mSolutionRepositoryItemInfo.AllItemsCache.Remove(mFolderItemsList[0]);

        //            mFolderItemsList.RemoveAt(0);
        //        }

        //        //mFolderItemsList.Clear();
        //        //mFolderItemsList = null;//making it null so list will be reloaded when GetITems will be called
        //    }

        //    //clear folder items cache
        //    mFolderItemsCache.Clear();
        //}

        /// <summary>
        /// Clear folder items & sub folders cache recursive and then reload the folder items to cache recursive
        /// </summary>
        /// <param name="newFolderName"></param>
        //public override void RefreshFolderCache()
        //{
        //    //clear cache
        //    ClearFolderCache();

        //    //reload items
        //    GetFolderItemsRecursive();
        //}


            //TO be used in rare cases were file watcher didn't catch a change!?
        public override void ReloadItems()
        {
            foreach (T item in mFolderItemsCache.Items<T>())
            {
                RepositoryItemBase ri = (RepositoryItemBase)(object)item;
                NewRepositorySerializer.ReloadObjectFromFile(ri);                
            }
                
            //clear cache
            //ClearFolderCache();

            //reload items
            //GetFolderItemsRecursive();
        }

        public override RepositoryFolderBase GetSubFolderByName(string name, bool recursive = false)
        {
            
            foreach (RepositoryFolder<T> RF in GetSubFolders())
            {
                if (RF.FolderRelativePath == name)
                {
                    return RF;
                }
                // Recursive scan down the tree
                if (recursive)
                {
                    RepositoryFolderBase subRF = RF.GetSubFolderByName(name, true);
                    if (subRF != null)
                    {
                        return subRF;
                    }
                }
            }
            return null;
        }

        public override void MoveItem(RepositoryItemBase repositoryItem, RepositoryFolderBase targetRepositoryFolder)
        {
            // remove from current RF cache lists
            RemoveItemFromLists(repositoryItem);

            repositoryItem.RelativeFilePath = targetRepositoryFolder.FolderRelativePath;


            //move the item to target folder
            // Directory.Move(((RepositoryItemBase)nodeItemToCut).FileName, targetFileName);
            //set target folder path + file name
            //string targetFileName = CreateRepositoryItemFileName((repositoryItem), targetFolder);
            //repositoryItem.FileName = targetFileName;
            //repositoryItem.FilePath = targetFileName;

            repositoryItem.FilePath = null;
            targetRepositoryFolder.AddRepositoryItem(repositoryItem);
                // move the file in the file system
                // move the item to target cache

            
        }
    }
}
