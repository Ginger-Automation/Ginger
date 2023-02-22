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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.IO;

namespace Amdocs.Ginger.Repository
{
    public class SolutionRepository
    {
        // This class will be the only one to read/write Repository items to disk
        // It will know the solution structure and will be able to load items, search and cache
        // It will cache each item loaded and will make sure only one item exist in memory so all list and places have reference to the same object 
        // And will use cache when items are requested first, if not found will load from disk
        // when item is deleted it will be removed from cache and disk
        // It will also cache items list, so if same list is requested again it will be from cache
        // items Added or removed, the corresponding folder will be updated including the folder cache
        // Item can be referenced more than once - as stand alone and in Folder cache

        public const string cSolutionRootFolderSign = @"~\"; // + Path.DirectorySeparatorChar;

        /// <summary>
        /// List of files and folders to exclude from solution load and Source Control
        /// </summary>
        private static List<string> mSolutionPathsToAvoid = new List<string>()
        {
             "AutoSave",
             "Recover",
             "RecentlyUsed.dat",
             "Backups",
             "ExecutionResults",
             "HTMLReports",

             @"SharedRepository\Activities\PrevVersions",
             @"SharedRepository\Actions\PrevVersions",
             @"SharedRepository\Variables\PrevVersions",
             @"SharedRepository\ActivitiesGroup\PrevVersions",

             @"SharedRepository\Activities\PrevVerions",
             @"SharedRepository\Actions\PrevVerions",
             @"SharedRepository\Variables\PrevVerions",
             @"SharedRepository\ActivitiesGroup\PrevVerions"
        };

        private List<string> mCalculatedSolutionPathsToAvoid = null;



        private ISolution mSolution = null;
        public ISolution Solution
        {
            get
            {
                return mSolution;
            }
        }
        NewRepositorySerializer mRepositorySerializer = new NewRepositorySerializer();
        public NewRepositorySerializer RepositorySerializer { get { return mRepositorySerializer; } }

        private string mSolutionFolderPath;
        public string SolutionFolder { get { return mSolutionFolderPath; } }
        private List<RepositoryFolderBase> mSolutionRootFolders = new List<RepositoryFolderBase>();
        public List<RepositoryFolderBase> SolutionRootFolders
        {
            get { return mSolutionRootFolders; }
        }

        public ObservableList<RepositoryItemBase> ModifiedFiles = new ObservableList<RepositoryItemBase>();

        Dictionary<Type, SolutionRepositoryItemInfoBase> mSolutionRepositoryItemInfoDictionary = new Dictionary<Type, SolutionRepositoryItemInfoBase>();
        public bool IsItemTypeHandled(RepositoryItemBase repositoryItem)
        {
            SolutionRepositoryItemInfoBase SRII = null;
            mSolutionRepositoryItemInfoDictionary.TryGetValue(repositoryItem.GetType(), out SRII);
            if (SRII != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #region Public Functions         
        // ------------------------------------------------------------------------------------------------
        // All Public function to use across
        // ------------------------------------------------------------------------------------------------

        public void Open(string solutionFolderPath)
        {
            //TODO: check if folder exist else exit
            // Verify folder is Ginger solution else exit            

            // Verify we have Solution.xml file in and load
            // DO basic verifications

            mSolutionFolderPath = solutionFolderPath;

            if (mSolutionRepositoryItemInfoDictionary.Count > 0)
            {
                VerifyOrCreateSolutionFolders(solutionFolderPath);
            }

        }




        /// <summary>
        /// Save the Repository Item to the Root folder and add it to cache
        /// </summary>
        /// <param name="repositoryItem"></param>
        public void AddRepositoryItem(RepositoryItemBase repositoryItem, bool doNotSave = false)
        {
            SolutionRepositoryItemInfoBase SRII = GetSolutionRepositoryItemInfo(repositoryItem.GetType());
            SRII.ItemRootRepositoryFolder.AddRepositoryItem(repositoryItem, doNotSave);
        }

        public async Task SaveRepositoryItemAsync(RepositoryItemBase repositoryItem)
        {
            await Task.Run(() => SaveRepositoryItem(repositoryItem));
        }

        /// <summary>
        /// Save changes of exsiting Repository Item to file system
        /// </summary>
        /// <param name="repositoryItem"></param>
        public void SaveRepositoryItem(RepositoryItemBase repositoryItem)
        {
            if (String.IsNullOrEmpty(repositoryItem.ContainingFolder))
            {
                throw new Exception("Cannot save item, there is no containing folder defined - " + repositoryItem.GetType().FullName + ", " + repositoryItem.GetNameForFileName());
            }
            if(repositoryItem.PreSaveHandler())
            {
                return;
            }

            repositoryItem.UpdateBeforeSave();

            string txt = RepositorySerializer.SerializeToString(repositoryItem);

            string filePath = CreateRepositoryItemFileName(repositoryItem);
            RepositoryFolderBase rf = GetItemRepositoryFolder(repositoryItem);
            rf.SaveRepositoryItem(filePath, txt);
            repositoryItem.FileName = filePath;
            repositoryItem.FilePath = filePath;
            repositoryItem.RefreshSourceControlStatus();
            RefreshParentFoldersSoucerControlStatus(Path.GetDirectoryName(repositoryItem.FilePath));

            if (repositoryItem.DirtyStatus != Common.Enums.eDirtyStatus.NoTracked)
            {
                repositoryItem.SetDirtyStatusToNoChange();
            }

            repositoryItem.CreateBackup();
            if (ModifiedFiles.Contains(repositoryItem))
            {
                ModifiedFiles.Remove(repositoryItem);
            }
            repositoryItem.PostSaveHandler();
        }

        public void Close()
        {
            StopAllRepositoryFolderWatchers();

            mRepositorySerializer = null;
            mSolutionFolderPath = null;
            mSolutionRootFolders = null;
            mSolutionRepositoryItemInfoDictionary = null;
        }

        public void StopAllRepositoryFolderWatchers()
        {
            foreach (RepositoryFolderBase RF in SolutionRootFolders)
            {
                RF.StopFileWatcherRecursive();
            }
        }

        /// <summary>
        /// Get the Repository Root Folder of the Repository Item
        /// </summary>
        /// <param name="repositoryItem"></param>
        public RepositoryFolderBase GetItemRepositoryRootFolder(RepositoryItemBase repositoryItem)
        {
            SolutionRepositoryItemInfoBase SRII = GetSolutionRepositoryItemInfo(repositoryItem.GetType());
            return SRII.ItemRootRepositoryFolder;
        }

        /// <summary>
        /// Get RepositoryItem by its path
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public RepositoryItemBase GetRepositoryItemByPath(string filePath)
        {
            RepositoryItemBase repoItem = null;
            ObservableList<RepositoryItemBase> repoItemList = new ObservableList<RepositoryItemBase>();
            RepositoryFolderBase repositoryFolderBase = GetRepositoryFolderByPath(Path.GetDirectoryName(filePath));
            if(repositoryFolderBase != null)
            {
                repoItemList = repositoryFolderBase.GetFolderRepositoryItems();
            }
            repoItem = repoItemList.Where(x => Path.GetFullPath(x.FileName) == Path.GetFullPath(filePath)).FirstOrDefault();
            return repoItem;
        }

        /// <summary>
        /// Get the Repository Folder by its path
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public RepositoryFolderBase GetRepositoryFolderByPath(string folderPath)
        {
            RepositoryFolderBase repoFolder = null;
            var inputURI = new Uri(folderPath+"\\"); // path must end with slash for isBaseOf to work correctly
            Parallel.ForEach(mSolutionRootFolders, folder =>
            {
                if (repoFolder == null)
                {
                    if (Path.GetFullPath(folderPath) == Path.GetFullPath(folder.FolderFullPath))
                    {
                        repoFolder = folder;
                    }
                    else if (new Uri(folder.FolderFullPath+"\\").IsBaseOf(inputURI))
                    {
                        string relPath = "~" + folderPath.Replace(SolutionFolder, "");
                        repoFolder = folder.GetSubFolderByName(relPath, true);
                    }
                }
            });
            return repoFolder;
        }


        /// <summary>
        /// Refresh source control status of all parent folders
        /// </summary>
        /// <param name="folderPath"></param>
        public void RefreshParentFoldersSoucerControlStatus(string folderPath)
        {
            RepositoryFolderBase repoFolder = GetRepositoryFolderByPath(folderPath);
            if (repoFolder != null)
            {
                repoFolder.RefreshFolderSourceControlStatus();
                RefreshParentFoldersSoucerControlStatus(Directory.GetParent(folderPath)?.FullName);
            }
        }



        /// <summary>
        /// Delete the Repository Item from file system and removing it from cache
        /// </summary>
        /// <param name="repositoryItem"></param>
        public void DeleteRepositoryItem(RepositoryItemBase repositoryItem)
        {
            RepositoryFolderBase itemFolder = GetItemRepositoryFolder(repositoryItem);
            if (itemFolder != null)
            {
                itemFolder.DeleteRepositoryItem(repositoryItem);
                if (ModifiedFiles.Contains(repositoryItem))
                {
                    ModifiedFiles.Remove(repositoryItem);
                }
            }
        }

        /// <summary>
        /// Get the Repository Folder of the Repository Item
        /// </summary>
        /// <param name="repositoryItem"></param>
        public RepositoryFolderBase GetItemRepositoryFolder(RepositoryItemBase repositoryItem)
        {
            SolutionRepositoryItemInfoBase SRII = GetSolutionRepositoryItemInfo(repositoryItem.GetType());
            return SRII.GetItemRepositoryFolder(repositoryItem);
        }

        /// <summary>
        /// Delete the Repository Item folder and it sub folders from file system and cache
        /// </summary>
        /// <param name="repositoryItemType"></param>
        /// <param name="folderPath"></param>
        /// <param name="recursive"></param>
        public void DeleteRepositoryItemFolder(RepositoryFolderBase repositoryFolder)
        {
            SolutionRepositoryItemInfoBase SRII = GetSolutionRepositoryItemInfo(repositoryFolder.ItemType);
            SRII.DeleteRepositoryItemFolder(repositoryFolder);
        }

        /// <summary>
        /// Get Repository Item by it GUID
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="repositoryItemGuid"></param>
        /// <returns></returns>
        public T GetRepositoryItemByGuid<T>(Guid repositoryItemGuid)
        {
            SolutionRepositoryItemInfo<T> SRII = GetSolutionRepositoryItemInfo<T>();
            T RI = SRII.GetItemByGuid(repositoryItemGuid);
            return RI;
        }

        /// <summary>
        /// Get the root RepositoryFolder of the provided Repository Item Type
        /// </summary>
        /// <typeparam name="T">Repository Item Type</typeparam>
        /// <returns></returns>
        public RepositoryFolder<T> GetRepositoryItemRootFolder<T>()
        {
            SolutionRepositoryItemInfo<T> SRII = GetSolutionRepositoryItemInfo<T>();
            return SRII.ItemRootReposiotryfolder;
        }

        /// <summary>
        /// Get list of All cached Repository Items from provided Repository Item Type
        /// </summary>
        /// <typeparam name="T">Repository Item Type</typeparam>
        /// <returns></returns>
        public ObservableList<T> GetAllRepositoryItems<T>()
        {
            SolutionRepositoryItemInfo<T> SRII = GetSolutionRepositoryItemInfo<T>();
            return SRII.GetAllItemsCache();
        }

        /// <summary>
        /// Get first cached Repository Item from provided Repository Item Type (if list is empty then null will be returned)
        /// </summary>
        /// <typeparam name="T">Repository Item Type</typeparam>
        /// <returns></returns>
        public dynamic GetFirstRepositoryItem<T>()
        {
            SolutionRepositoryItemInfo<T> SRII = GetSolutionRepositoryItemInfo<T>();
            if (SRII.GetAllItemsCache().Count > 0)
            {
                return (SRII.GetAllItemsCache()[0]);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Convert relative path to full path - for example: '~/BusinessFlow' will convert to 'C:\abc\sol1\BusinessFlow'
        /// </summary>
        /// <param name="Folder"></param>
        /// <returns></returns>
        public string GetFolderFullPath(string Folder)
        {
            string FullPath;

            if (Folder == null)
            {
                FullPath = mSolutionFolderPath;
            }
            else
            {
                if (Folder.StartsWith(cSolutionRootFolderSign))
                {
                    string subfolder = Folder.Replace(cSolutionRootFolderSign, string.Empty);
                    FullPath = Path.Combine(mSolutionFolderPath, subfolder);
                }
                else
                {
                    FullPath = Folder;
                }
            }
            return FullPath;
        }

        
        /// <summary>
        /// Converts path of file inside the Solution to be relative
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public string ConvertFullPathToBeRelative(string fullPath)
        {
            string relative = fullPath;
            if (fullPath != null && fullPath.ToUpper().Contains(SolutionFolder.ToUpper()))
            {
                relative = cSolutionRootFolderSign + fullPath.Remove(0, SolutionFolder.Length);
            }
            return relative;
        }

        /// <summary>
        ///  Return enumerator of all valid files in solution, only repo items no junk
        /// </summary>
        /// <param name="solutionFolder"></param>
        /// <returns></returns>
        public IEnumerable<RepositoryFile> GetAllSolutionRepositoryFiles()
        {
            //TODO: cache but be carefull
            // super fast way to get files list in parllel

            //List only need directories which have repo items // But we say all rename to get all repo or...
            //Do not add documents, ExecutionResults, HTMLReports
            ConcurrentBag<RepositoryFile> fileEntries = new ConcurrentBag<RepositoryFile>();

            Parallel.ForEach(mSolutionRootFolders, folder =>
            {
                // We drill down only if there is chance to find repo item for xml upgrade
                if (folder.ContainsRepositoryItems)
                {
                    // Get each main folder sub folder all levels                    
                    string MainFolderFullPath = GetFolderFullPath(folder.FolderRelativePath);

                    if (Directory.Exists(MainFolderFullPath))
                    {
                        // Add main folder files
                        AddFolderFiles(fileEntries, MainFolderFullPath, folder.ItemFilePattern);

                        //Now drill down to ALL sub folders
                        string[] SubFolders = Directory.GetDirectories(MainFolderFullPath, "*", SearchOption.AllDirectories);

                        if (SubFolders.Length > 0)
                        {
                            Parallel.ForEach(SubFolders, sf =>
                            {
                                // Add all files of sub folder
                                if (!IsSolutionPathToAvoid(sf))
                                {
                                    AddFolderFiles(fileEntries, sf, folder.ItemFilePattern);
                                }
                            });
                        }
                    }
                }
            });

            // TODO: return sorted list
            return fileEntries.ToArray();
        }

        public static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       .ToUpperInvariant();
        }


        public bool IsSolutionPathToAvoid(string pathToCheck)
        {
            if (mCalculatedSolutionPathsToAvoid == null)
            {
                mCalculatedSolutionPathsToAvoid = new List<string>();
                foreach (string path in mSolutionPathsToAvoid)
                {
                    mCalculatedSolutionPathsToAvoid.Add(Path.GetFullPath(Path.Combine(SolutionFolder, path)));
                }
            }

            return mCalculatedSolutionPathsToAvoid.Any(Path.GetFullPath(pathToCheck).Contains);
        }

        #endregion Public Functions

        #region Private Functions        
        // ------------------------------------------------------------------------------------------------
        //  private functions
        // ------------------------------------------------------------------------------------------------

        public void AddItemInfo<T>(string pattern, string rootFolder, bool containRepositoryItems, string displayName, string PropertyNameForFileName)
        {
            SolutionRepositoryItemInfo<T> SRII = new SolutionRepositoryItemInfo<T>();
            SRII.ItemFileSystemRootFolder = rootFolder;
            SRII.PropertyForFileName = PropertyNameForFileName;
            SRII.Pattern = pattern;
            SRII.DisplayName = displayName;
            SRII.ItemRootReposiotryfolder = new RepositoryFolder<T>(this, SRII, pattern, rootFolder, containRepositoryItems, displayName, true);

            mSolutionRepositoryItemInfoDictionary.Add(typeof(T), SRII);
            mSolutionRootFolders.Add((RepositoryFolderBase)SRII.ItemRootRepositoryFolder);
        }

        public SolutionRepositoryItemInfoBase GetSolutionRepositoryItemInfo(Type type)
        {
            SolutionRepositoryItemInfoBase SRII;
            mSolutionRepositoryItemInfoDictionary.TryGetValue(type, out SRII);

            if (SRII == null)
            {
                // for subclass (like APIModel REST/SOAP which subclass APIModelBase) we will not find so now we search each one which is IsAssignableFrom
                SRII = (from x in mSolutionRepositoryItemInfoDictionary where x.Value.ItemType.IsAssignableFrom(type) select x.Value).FirstOrDefault();

                if (SRII == null)
                {
                    throw new Exception("GetSolutionRepositoryItemInfo failed cannot find SRII for type - " + type.FullName);
                }
            }

            return SRII;
        }



        private SolutionRepositoryItemInfo<T> GetSolutionRepositoryItemInfo<T>()
        {
            SolutionRepositoryItemInfoBase SRIIBase = GetSolutionRepositoryItemInfo(typeof(T));
            SolutionRepositoryItemInfo<T> SRII = (SolutionRepositoryItemInfo<T>)SRIIBase;
            return SRII;
        }

        private void VerifyOrCreateSolutionFolders(string folder)
        {
            foreach(RepositoryFolderBase RF in mSolutionRootFolders)
            {
                string dir = Path.Combine(mSolutionFolderPath, RF.FolderRelativePath.Replace(cSolutionRootFolderSign, ""));
                if (!Directory.Exists(PathHelper.GetLongPath(dir)))
                {
                    Directory.CreateDirectory(PathHelper.GetLongPath(dir));
                }
                RF.StartFileWatcher();
            }

            //After we made sure we have root folders, we verify all again including sub folders
            var SRIIs = from x in mSolutionRepositoryItemInfoDictionary select x.Value;
            foreach (SolutionRepositoryItemInfoBase SRII in SRIIs)
            {
                string dir = Path.Combine(mSolutionFolderPath, SRII.ItemFileSystemRootFolder.Replace(cSolutionRootFolderSign, ""));
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
        }

        internal void SaveNewRepositoryItem(RepositoryItemBase repositoryItem)
        {
            //check if file already exist
            string filePath = CreateRepositoryItemFileName(repositoryItem);
            if (System.IO.File.Exists(filePath))
            {
                throw new Exception("Repository file already exist - " + filePath);
            }
            SaveRepositoryItem(repositoryItem);
        }

        //TODO: fix this method name or cretae or !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public string CreateRepositoryItemFileName(RepositoryItemBase repositoryItemBase, string containingFolder = "")
        {
            if (repositoryItemBase.FilePath != null && File.Exists(repositoryItemBase.FilePath) && string.IsNullOrEmpty(containingFolder))
            {
                return repositoryItemBase.FilePath;
            }
            else
            {
                var repositoryItemInfoBaseType = GetSolutionRepositoryItemInfo(repositoryItemBase.GetType());

                string name = repositoryItemBase.ItemName; // (string)RI.GetType().GetProperty(v.PropertyForFileName).GetValue(RI);

                //probably new item so create new path for it

                //FOLDER             
                string fileFolderPath = string.Empty;
                if (string.IsNullOrEmpty(containingFolder))
                {
                    fileFolderPath = repositoryItemBase.ContainingFolder;
                }
                else
                {
                    fileFolderPath = containingFolder;
                }

                if (!fileFolderPath.StartsWith(cSolutionRootFolderSign) || !fileFolderPath.StartsWith(mSolutionFolderPath))
                {
                    string solutionPath = mSolutionFolderPath;
                    string filefolder = fileFolderPath.Replace(cSolutionRootFolderSign, Path.DirectorySeparatorChar.ToString()).TrimStart(Path.DirectorySeparatorChar).TrimEnd(Path.DirectorySeparatorChar);
                    if (!solutionPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    {
                        solutionPath += Path.DirectorySeparatorChar;
                    }
                    fileFolderPath = Path.Combine(solutionPath, filefolder);
                    if (fileFolderPath.Length > 260)
                    {
                        ThrowErrorIfPathLimitExeceeded();
                    }
                }

                string fileName = Amdocs.Ginger.Common.GeneralLib.General.RemoveInvalidFileNameChars(name);

                string fullName = repositoryItemInfoBaseType.Pattern.Replace("*", fileName);


                string filefullPath = Path.Combine(fileFolderPath, fullName);

                //TODO: remove max 255 once we switch all to work with .Net core 2.0 no limit
                //Validate Path length - MAX_PATH is 260
                if (filefullPath.Length > 260)
                {
                    var newFileName = string.Empty;
                    var noOfCharToEscape = 0;
                    if (fileName.Length > 255)
                    {
                        noOfCharToEscape = filefullPath.Length + 2 - 255;
                        newFileName = fileName.Substring(0, fileName.Length - noOfCharToEscape);   //TODO: validate that works as expected using unit test !!!!!!!!!! file extension must remain or give err

                        newFileName = newFileName + "~1";
                        newFileName = repositoryItemInfoBaseType.Pattern.Replace("*", newFileName);
                        filefullPath = Path.Combine(fileFolderPath, newFileName);
                    }

                    if (filefullPath.Length > 260 && fileName.Length > 3 )
                    {
                        noOfCharToEscape = filefullPath.Length - 257;
                        newFileName = fileName.Substring(0, fileName.Length - noOfCharToEscape);

                        if (newFileName.Length < 3)
                        {
                            ThrowErrorIfPathLimitExeceeded();
                        }
                        newFileName = newFileName + "~1";
                        newFileName = repositoryItemInfoBaseType.Pattern.Replace("*", newFileName);
                        filefullPath = Path.Combine(fileFolderPath, newFileName);
                    }
                    else if(filefullPath.Length > 260 && fileName.Length < 3)
                    {
                        ThrowErrorIfPathLimitExeceeded();
                    }
                }



                //validate no other file with same name

                //find first file which doesn't exist, add ~counter until found
                int counter = 0;
                string Nameex = "";
                string ext = repositoryItemInfoBaseType.Pattern.Replace("*", "");
                while (File.Exists(filefullPath))
                {
                    if (Nameex != "")
                    {
                        filefullPath = filefullPath.Replace(Nameex, "");
                    }
                    counter++;
                    Nameex = "~" + counter;
                    filefullPath = filefullPath.Replace(ext, Nameex + ext);

                    if (counter > 100)
                    {
                        throw new Exception("cannot find unique file after 100 tries");
                    }
                }

                return filefullPath;
            }
        }

        private static void ThrowErrorIfPathLimitExeceeded()
        {
            throw new Exception("File path length limit execeeded");
        }

        static void AddFolderFiles(ConcurrentBag<RepositoryFile> CB, string folder, string pattern)
        {
            string[] patterns = pattern.Split('|');

            foreach (string p in patterns)
            {
                IEnumerable<string> files = Directory.EnumerateFiles(folder, p, SearchOption.TopDirectoryOnly);
                foreach (string file in files)
                {
                    CB.Add(new RepositoryFile() { FilePath = file });
                }
            }
        }
        #endregion Private Functions




        /// <summary>
        /// Create new solution folders 
        /// path must be to empty folder
        /// </summary>
        /// <param name="path"></param>
        public void CreateRepository(string path)
        {
            if (System.IO.Directory.Exists(path))
            {
                throw new Exception("Cannot create new solution, directory already exist - " + path);
            }

            mSolutionFolderPath = path;
            System.IO.Directory.CreateDirectory(path);



            foreach (SolutionRepositoryItemInfoBase SRII in mSolutionRepositoryItemInfoDictionary.Values)
            {
                string fullPath = GetFolderFullPath(SRII.ItemFileSystemRootFolder);
                Directory.CreateDirectory(fullPath);
            }
        }


        public void MoveItem(RepositoryItemBase repositoryItem, string targetFolder)
        {
            RepositoryFolderBase RF = GetItemRepositoryFolder(repositoryItem);
            RepositoryFolderBase targetRF= GetRepositoryFolderByPath(targetFolder);

            if (RF != null && targetRF != null)
            {
                RF.DeleteRepositoryItem(repositoryItem);
                targetRF.AddRepositoryItem(repositoryItem);
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to Move repository item because source or target folders failed to be identified for item '{0}' and target folder '{1}'.", repositoryItem.FilePath, targetFolder));
            }
        }


        /// <summary>
        /// Move existing shared repository item to PrevVersion folder. And remove it from cache
        /// </summary>
        /// <param name="repositoryItem"></param>
        public void MoveSharedRepositoryItemToPrevVersion(RepositoryItemBase repositoryItem)
        {
            if (repositoryItem.FileName != null && File.Exists(repositoryItem.FileName))
            {
                RepositoryFolderBase repostitoryRootFolder = GetItemRepositoryRootFolder(repositoryItem);

                string targetPath=Path.Combine(repostitoryRootFolder.FolderFullPath, "PrevVersions");
                if (!Directory.Exists(targetPath))
                {
                    repostitoryRootFolder.PauseFileWatcher();
                    //We do not want to file watcher track PrevVersions Folder. So creating it explicity using Create directory
                    Directory.CreateDirectory(targetPath);
                    repostitoryRootFolder.ResumeFileWatcher();
                }

                string dts = DateTime.Now.ToString("yyyyMMddHHmm");

                string targetFileName = repositoryItem.ItemName +"." + dts + "." + repositoryItem.ObjFileExt+".xml";

                targetFileName = General.RemoveInvalidCharsCombinePath(targetPath, targetFileName);

                if (targetFileName.Length > 255)
                {
                    targetFileName = targetFileName.Substring(0, 250) + new Random().Next(1000).ToString();
                }

                try
                {
                    if (File.Exists(targetFileName))
                    {
                        File.Delete(targetFileName);
                    }
                    //We want to delete the item and remove it from cache. So first we copy it to destination and then delete using Repository Folder.
                    File.Copy(repositoryItem.FileName, targetFileName);
                    RepositoryFolderBase repositoryFolder = GetItemRepositoryFolder(repositoryItem);
                    repositoryFolder.DeleteRepositoryItem(repositoryItem);

                }
                catch (IOException ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Shared Repository moving item to PrevVersion", ex);
                }

            }
        }

    }
}
