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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Repository
{
    public class SolutionRepository
    {
        // This class will be the only one to read/write Repo items to disk
        // It will know the solution structure and will be able to load items, search and cache
        // It will cache each item loaded and will make sure only one item exist in mememory so all list and places have refenerence to the same object 
        // And will use cache when items are reuquested first, if not found will load from disk
        // when item is deleted it will be removed from cache and disk
        // It will also cache items list, so if same list is requested again it will be from cache
        // items Added or removed, the corresponsing folder will be updated including the folder cache
        // Item can be refenced more than once - as stand alone and in Folder cache

        //TODO; need init
        // public static IRepositorySerializer mRepositorySerializer;  //  = new RepositorySerializer2();   // We create one instance

        public const string cSolutionRootFolderSign = @"~\"; // + Path.DirectorySeparatorChar;

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
        private List<RepositoryFolderBase> mSolutionRootFolders = null;
        public List<RepositoryFolderBase> SolutionRootFolders
        {
            get { return mSolutionRootFolders; }
        }

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
        public void AddRepositoryItem(RepositoryItemBase repositoryItem)
        {
            SolutionRepositoryItemInfoBase SRII = GetSolutionRepositoryItemInfo(repositoryItem.GetType());
            SRII.ItemRootRepositoryFolder.AddRepositoryItem(repositoryItem);
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
                repositoryItem.SetDirtyStatusToNoChange();
        }      

        public void Close()
        {
            StopAllRepositoryFolderWatchers(SolutionRootFolders);

            mRepositorySerializer = null;
            mSolutionFolderPath = null;
            mSolutionRootFolders = null;
            mSolutionRepositoryItemInfoDictionary = null;
        }

        private void StopAllRepositoryFolderWatchers(List<RepositoryFolderBase> folders)
        {
            foreach (RepositoryFolderBase RF in folders)
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
            ObservableList<RepositoryItemBase> repoItemList = GetRepositoryFolderByPath(Path.GetDirectoryName(filePath)).GetFolderRepositoryItems();
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
            Parallel.ForEach(mSolutionRootFolders, folder =>
            {
                if (repoFolder == null)
                {
                    if (Path.GetFullPath(folderPath) == Path.GetFullPath(folder.FolderFullPath))
                    {
                        repoFolder = folder;                        
                    }
                    else if (folderPath.ToLower().Contains(folder.FolderFullPath.ToLower()))
                    {
                        Uri fullPath = new Uri(folderPath, UriKind.Absolute);
                        Uri relRoot = new Uri(folder.FolderFullPath, UriKind.Absolute);
                        string relPath = relRoot.MakeRelativeUri(fullPath).ToString().Replace("/", "\\");
                        repoFolder = folder.GetSubFolderByName("~\\" + Uri.UnescapeDataString(relPath), true);
                    }
                }
            });
            return repoFolder;
        }


        /// <summary>
        /// Refresh source control status of all parent folders
        /// </summary>
        /// <param name="folderPath"></param>
        public void RefreshParentFoldersSoucerControlStatus(string folderPath, bool pullParentFolder = false)
        {
            if (pullParentFolder)
            {
                FileAttributes attr;
                attr = File.GetAttributes(folderPath);

                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    folderPath = Directory.GetParent(folderPath).FullName;
                }
                else
                {
                    folderPath = Path.GetDirectoryName(folderPath);
                }
            }

            RepositoryFolderBase repoFolder = GetRepositoryFolderByPath(folderPath);
            if (repoFolder != null)
            {
                repoFolder.RefreshFolderSourceControlStatus();
                RefreshParentFoldersSoucerControlStatus(Directory.GetParent(folderPath).FullName);
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
                itemFolder.DeleteRepositoryItem(repositoryItem);
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
                                if (sf != "PrevVersions")  //TODO: use const
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
        #endregion Public Functions

        #region Private Functions        
        // ------------------------------------------------------------------------------------------------
        //  private functions
        // ------------------------------------------------------------------------------------------------

        public void AddItemInfo<T>(string pattern, string rootFolder, bool containRepositoryItems, string displayName, bool addToRootFolders, string PropertyNameForFileName)
        {
            SolutionRepositoryItemInfo<T> SRII = new SolutionRepositoryItemInfo<T>();
            SRII.ItemFileSystemRootFolder = rootFolder;
            SRII.PropertyForFileName = PropertyNameForFileName;
            SRII.Pattern = pattern;
            SRII.ItemRootReposiotryfolder = new RepositoryFolder<T>(this, SRII, pattern, rootFolder, containRepositoryItems, displayName, true);
            mSolutionRepositoryItemInfoDictionary.Add(typeof(T), SRII);

            if (addToRootFolders)
            {
                if (mSolutionRootFolders == null)
                {
                    mSolutionRootFolders = new List<RepositoryFolderBase>();
                }
                mSolutionRootFolders.Add((RepositoryFolderBase)SRII.ItemRootRepositoryFolder);
            }
        }

        private SolutionRepositoryItemInfoBase GetSolutionRepositoryItemInfo(Type type)
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
        public string CreateRepositoryItemFileName(RepositoryItemBase RI, string containingFolder = "")
        {
            var v = GetSolutionRepositoryItemInfo(RI.GetType());

            string name = RI.ItemName; // (string)RI.GetType().GetProperty(v.PropertyForFileName).GetValue(RI);

            if (RI.FilePath != null && File.Exists(RI.FilePath) && string.IsNullOrEmpty(containingFolder))
                return RI.FilePath;
            else
            {
                //probably new item so create new path for it

                //FOLDER             
                string fileFolderPath = string.Empty; 
                if (string.IsNullOrEmpty(containingFolder))
                    fileFolderPath = RI.ContainingFolder;
                else
                    fileFolderPath = containingFolder;
                if (!fileFolderPath.StartsWith(cSolutionRootFolderSign) || !fileFolderPath.StartsWith(mSolutionFolderPath))
                {
                    // Fix me for Linux !!!	
                    string A = mSolutionFolderPath; //.TrimEnd(Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);	
                    string B = fileFolderPath.Replace(cSolutionRootFolderSign, Path.DirectorySeparatorChar.ToString()).TrimStart(Path.DirectorySeparatorChar).TrimEnd(Path.DirectorySeparatorChar);
                    if (!A.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    {
                        A += Path.DirectorySeparatorChar;
                    }
                    fileFolderPath = Path.Combine(A, B);
                }

                //FILE
                string fileName = name;
                //Removing all possible invalid path chars and checking the file name length is legal (<= 255)                      
                foreach (char invalidChar in Path.GetInvalidFileNameChars())
                {
                    fileName = fileName.Replace(invalidChar.ToString(), "");
                }
                fileName = fileName.Replace(@".", "");
                
                string fullName = v.Pattern.Replace("*", fileName);


                string filefullPath = Path.Combine(fileFolderPath, fullName);

                //TODO: remove max 255 once we swithc all to work with .Net core 2.0 no limit
                //Validate Path length - MAX_PATH is 260
                //if (filefullPath.Length > 255)
                if (fileName.Length > 255)
                {
                    //FIXME !!!!!!!!!!!!!!

                    int gap = filefullPath.Length + 2 - 255;
                    string NewName = fileName.Substring(0, fileName.Length - gap);   //TODO: validate that works as expected using unit test !!!!!!!!!! file extension must remain or give err

                    //tODO: throw exception if file name too short...

                    NewName = NewName + "~1";
                    NewName = v.Pattern.Replace("*", NewName);
                    filefullPath = Path.Combine(fileFolderPath, NewName);
                }

                //validate no other file with same name

                //find first file which doesn't exist, add ~counter until found
                int counter = 0;
                string Nameex = "";
                string ext = v.Pattern.Replace("*", "");
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
                        throw new Exception("cannot find uniqe file after 100 tries");
                    }
                }

                return filefullPath;
            }
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

            RF.DeleteRepositoryItem(repositoryItem);
            targetRF.AddRepositoryItem(repositoryItem);
        }
    }
}
