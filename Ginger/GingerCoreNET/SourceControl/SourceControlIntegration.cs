#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.SourceControlLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.SourceControl;
using Amdocs.Ginger.IO;
using Amdocs.Ginger.Repository;
using GingerCore.SourceControl;
using GingerCoreNET.SourceControl;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using static GingerCoreNET.SourceControl.SourceControlBase;

namespace Ginger.SourceControl
{
    public class SourceControlIntegration
    {
        public static bool BusyInProcessWhileDownloading = false;


        public static bool conflictFlag = false;



        public static bool AddFile(SourceControlBase SourceControl, string path)
        {
            string error = string.Empty;

            if (!SourceControl.AddFile(path, ref error))
            {
                Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, error);
                return false;
            }
            return true;
        }

        public static bool DeleteFile(SourceControlBase SourceControl, string path)
        {
            string error = string.Empty;

            if (!SourceControl.DeleteFile(path, ref error))
            {
                Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, error);
                return false;
            }
            return true;
        }

        public static bool UpdateFile(SourceControlBase SourceControl, string path)
        {
            string error = string.Empty;
            bool IsFileUpdated = true;
            RepositoryFolderBase repositoryFolderBase = WorkSpace.Instance.SolutionRepository.GetRepositoryFolderByPath(Path.GetDirectoryName(path));
            repositoryFolderBase?.PauseFileWatcher();
            if (!SourceControl.UpdateFile(path, ref error))
            {
                IsFileUpdated = false;
                Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, error);
                return IsFileUpdated;
            }
            repositoryFolderBase?.ResumeFileWatcher();
            return IsFileUpdated;
        }



        public static bool CleanUp(SourceControlBase SourceControl, string folder)
        {
            SourceControl.CleanUp(folder);
            return true;
        }

        public static ObservableList<SourceControlFileInfo> GetPathFilesStatus(SourceControlBase SourceControl, string Path)
        {
            string error = string.Empty;
            ObservableList<SourceControlFileInfo> OL = SourceControl.GetPathFilesStatus(Path, ref error, WorkSpace.Instance.Solution.ShowIndicationkForLockedItems);
            if (error != string.Empty)
            {
                Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, error);
            }
            return OL;
        }


        public static string GetRepositoryURL(SourceControlBase SourceControl)
        {
            string error = string.Empty;
            return WorkSpace.Instance.Solution.SourceControl.GetRepositoryURL(ref error);
        }


        public static bool Init(SourceControlBase SourceControl)
        {
            SourceControl.Init();
            return true;
        }

        public static bool Disconnect(SourceControlBase SourceControl)
        {
            SourceControl.Disconnect();
            return true;
        }

        public static ObservableList<SolutionInfo> GetProjectsList(SourceControlBase SourceControl)
        {
            try
            {
                return SourceControl.GetProjectsList();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to Get Project List", ex);
                return [];
            }
        }

        public static bool CreateConfigFile(SourceControlBase SourceControl)
        {
            string error = string.Empty;
            if (!SourceControl.CreateConfigFile(ref error))
            {
                Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, error);
                return false;
            }
            return true;
        }

        public static bool GetProject(SourceControlBase sourceControl, string path, string uri, ProgressNotifier progressNotifier = null, CancellationToken cancellationToken = default)
        {
            try
            {
                string error = string.Empty;
                bool isLinuxOrSVN = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || sourceControl.GetSourceControlType == eSourceControlType.SVN;

                bool success = isLinuxOrSVN ? sourceControl.GetProject(path, uri, ref error) : sourceControl.GetProjectWithProgress(path, uri, ref error, progressNotifier, cancellationToken);

                if (!success && !string.IsNullOrEmpty(error))
                {
                    Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, error);
                }

                return success;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to Get Project", ex);
                return false;
            }
        }

        public static bool GetLatest(string path, SourceControlBase SourceControl)
        {
            string error = string.Empty;
            List<string> conflictsPaths = [];
            if (!SourceControl.GetLatest(path, ref error, ref conflictsPaths))
            {
                if (conflictsPaths.Count > 0)
                {
                    Reporter.ToUser(eUserMsgKey.SourceControlUpdateFailed, error);
                    return false;
                }
            }
            return true;
        }

        public static string GetLockOwner(SourceControlBase SourceControl, string path)
        {
            string error = string.Empty;
            return SourceControl.GetLockOwner(path, ref error);
        }

        public static RepositoryItemBase GetLocalItemFromConflict(SourceControlBase sourceControl, string path)
        {
            if (path.Contains("@/"))
            {
                path = path.Replace("@/", "\\");
            }

            string localContent = sourceControl.GetLocalContentForConflict(path);
            RepositoryItemBase localItem = null;
            bool isXMLFile = path.EndsWith(".xml");
            if (isXMLFile && !string.IsNullOrEmpty(localContent))
            {
                localItem = NewRepositorySerializer.DeserializeFromText(localContent);
            }

            return localItem;
        }

        public static RepositoryItemBase GetRemoteItemFromConflict(SourceControlBase sourceControl, string path)
        {
            if (path.Contains("@/"))
            {
                path = path.Replace("@/", "\\");
            }

            string remoteContent = sourceControl.GetRemoteContentForConflict(path);

            RepositoryItemBase remoteItem = null;
            bool isXMLFile = path.EndsWith(".xml");
            if (isXMLFile && !string.IsNullOrEmpty(remoteContent))
            {
                remoteItem = NewRepositorySerializer.DeserializeFromText(remoteContent);
            }

            return remoteItem;
        }

        public static Comparison CompareConflictedItems(RepositoryItemBase localItem, RepositoryItemBase remoteItem)
        {
            ICollection<Comparison> childComparisons = RepositoryItemBaseComparer.Compare("[0]", localItem, remoteItem);
            Comparison.StateType state = childComparisons.All(c => c.State == Comparison.StateType.Unmodified) ? Comparison.StateType.Unmodified : Comparison.StateType.Modified;
            Type dataType = typeof(RepositoryItemBase);
            if (localItem != null)
            {
                dataType = localItem.GetType();
            }
            else if (remoteItem != null)
            {
                dataType = remoteItem.GetType();
            }
            return new Comparison("ROOT", state, childComparisons: childComparisons, dataType: dataType);
        }

        public static Comparison GetComparisonForConflicted(SourceControlBase sourceControl, string path)
        {
            if (path.Contains("@/"))
            {
                path = path.Replace("@/", "\\");
            }

            string localContent = sourceControl.GetLocalContentForConflict(path);
            RepositoryItemBase localItem = NewRepositorySerializer.DeserializeFromText(localContent);
            string remoteContent = sourceControl.GetRemoteContentForConflict(path);
            RepositoryItemBase remoteItem = NewRepositorySerializer.DeserializeFromText(remoteContent);
            ICollection<Comparison> childComparisons = RepositoryItemBaseComparer.Compare("[0]", localItem, remoteItem);
            Comparison.StateType state = childComparisons.All(c => c.State == Comparison.StateType.Unmodified) ? Comparison.StateType.Unmodified : Comparison.StateType.Modified;
            return new Comparison("ROOT", state, childComparisons: childComparisons, dataType: localItem.GetType());
        }

        public static RepositoryItemBase CreateMergedItemFromComparison(Comparison comparison)
        {
            RepositoryItemBase mergedRIB = RepositoryItemBaseMerger.Merge(comparison.DataType, comparison.ChildComparisons);
            return mergedRIB;
        }

        /// <summary>
        /// Resolve merge conflict with content that contains the resolved data of the file.
        /// </summary>
        /// <param name="sourceControl">Source control implementation to use</param>
        /// <param name="path">Path of the conflicted file</param>
        /// <param name="content">Content containing the resolved data.</param>
        /// <returns><see langword="true"/> if the conflict was resolved successfully, <see langword="false"/> otherwise.</returns>
        public static bool ResolveConflictWithContent(SourceControlBase sourceControl, string path, string content)
        {
            try
            {
                if (path == null)
                {
                    return false;
                }
                if (path.Contains("@/"))
                {
                    path = path.Replace("@/", "\\");
                }

                string error = string.Empty;
                bool isConflictResolved = sourceControl.ResolveConflictWithContent(path, content, ref error);

                if (!isConflictResolved)
                {
                    Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, error);
                }

                return isConflictResolved;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred during resolving conflicts..", ex);
                return false;
            }
        }

        public static List<string> GetConflictPaths(SourceControlBase sourceControl)
        {
            return sourceControl.GetConflictPaths();
        }

        public static bool ResolveConflicts(SourceControlBase SourceControl, string path, eResolveConflictsSide side)
        {
            string error = string.Empty;
            bool IsConflictResolved = true;
            try
            {
                if (path == null)
                {
                    return false;
                }
                if (path.Contains("@/"))
                {
                    path = path.Replace("@/", "\\");
                }
                RepositoryFolderBase repositoryFolderBase = null;
                if (path.EndsWith("xml"))
                {
                    if (path != SourceControl.SolutionFolder)
                    {
                        repositoryFolderBase = WorkSpace.Instance.SolutionRepository.GetRepositoryFolderByPath(Path.GetDirectoryName(path));
                        if (repositoryFolderBase != null)
                        {
                            repositoryFolderBase.PauseFileWatcher();
                        }
                    }
                }

                if (!SourceControl.ResolveConflicts(path, side, ref error))
                {
                    IsConflictResolved = false;
                    Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, error);
                    return IsConflictResolved;
                }
                if (repositoryFolderBase != null)
                {
                    repositoryFolderBase.ResumeFileWatcher();
                    repositoryFolderBase.ReloadUpdatedXML(path);
                }

                return IsConflictResolved;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred during resolving conflicts..", ex);
                return false;
            }


        }

        public static bool CommitSelfHealingChanges(string solutionPath)
        {
            SourceControlBase mSourceControl = WorkSpace.Instance.SourceControl;

            string error = string.Empty;
            var sourceControlFileInfos = mSourceControl.GetPathFilesStatus(solutionPath, ref error);

            var paths = new List<string>();
            foreach (var item in sourceControlFileInfos)
            {
                if (item.Path.Contains("DOCUMENTS") || item.Path.Contains("EXECUTIONRESULTS"))
                {
                    continue;
                }
                if (item.Status == SourceControlFileInfo.eRepositoryItemStatus.Modified)
                {
                    paths.Add(item.Path);
                }
            }

            if (paths.Count == 0)
            {
                return false;
            }

            List<string> conflictsPaths = [];
            string err = string.Empty;
            bool isSuccess = mSourceControl.AddFile("*", ref err);
            if (!isSuccess)
            {
                Reporter.ToLog(eLogLevel.ERROR, err);
                return false;
            }
            return mSourceControl.CommitAndCheckinChanges(paths, "check-in self healing changes.", ref error, ref conflictsPaths, false);

        }

        public static void Lock(SourceControlBase SourceControl, string path, string lockComment)
        {
            string error = string.Empty;
            if (!SourceControl.Lock(path, lockComment, ref error))
            {
                Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, error);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.SourceControlLockSucss);
            }
        }

        public static void UnLock(SourceControlBase SourceControl, string path)
        {
            string error = string.Empty;
            if (!SourceControl.UnLock(path, ref error))
            {
                if (error != string.Empty)
                {
                    Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, error);
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.SourceControlUnlockSucss);
            }
        }

        public static SourceControlFileInfo.eRepositoryItemStatus GetFileStatus(SourceControlBase SourceControl, string Path, bool ShowIndicationkForLockedItems)
        {
            string error = string.Empty;
            return SourceControl.GetFileStatus(Path, ShowIndicationkForLockedItems, ref error);
        }

        public static bool Revert(SourceControlBase SourceControl, string path)
        {
            string error = string.Empty;

            if (!SourceControl.Revert(path, ref error))
            {
                Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, error);
                return false;
            }
            return true;
        }

        public static SourceControlItemInfoDetails GetInfo(SourceControlBase SourceControl, string path)
        {
            string error = string.Empty;
            return SourceControl.GetInfo(path, ref error);
        }

        public static SourceControlItemInfoDetails GetRepositoryInfo(SourceControlBase sourceControl)
        {
            string error = string.Empty;
            return sourceControl.GetRepositoryInfo(ref error);
        }





        public static SourceControlBase.eSourceControlType CheckForSolutionSourceControlType(string SolutionFolder, ref string ReposiytoryRootFolder)
        {
            string SourceControlRootFolder = SolutionFolder;
            while (SourceControlRootFolder != Path.GetPathRoot(SolutionFolder))
            {
                //Path.get
                if (Directory.Exists(SourceControlRootFolder + Path.DirectorySeparatorChar + ".git"))
                {
                    FileAttributes attributes = File.GetAttributes(SourceControlRootFolder + Path.DirectorySeparatorChar + ".git");
                    if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    {
                        ReposiytoryRootFolder = Path.GetFullPath(SourceControlRootFolder);
                        return SourceControlBase.eSourceControlType.GIT;
                    }
                }
                if (Directory.Exists(SourceControlRootFolder + Path.DirectorySeparatorChar + ".svn"))
                {
                    FileAttributes attributes = File.GetAttributes(SourceControlRootFolder + Path.DirectorySeparatorChar + ".svn");
                    if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    {
                        ReposiytoryRootFolder = Path.GetFullPath(SourceControlRootFolder);
                        return SourceControlBase.eSourceControlType.SVN;
                    }
                }
                SourceControlRootFolder = System.IO.Directory.GetParent(SourceControlRootFolder).FullName;
            }
            return SourceControlBase.eSourceControlType.None;
        }

        public static eImageType GetFileImage(string path)
        {
            eImageType SCImage = eImageType.Null;
            if (WorkSpace.Instance.SourceControl == null)
            {
                return SCImage;
            }

            SourceControlFileInfo.eRepositoryItemStatus RIS = SourceControlIntegration.GetFileStatus(WorkSpace.Instance.SourceControl, path, true);

            switch (RIS)
            {
                case SourceControlFileInfo.eRepositoryItemStatus.New:
                    SCImage = eImageType.SourceControlNew;
                    break;
                case SourceControlFileInfo.eRepositoryItemStatus.Modified:
                    SCImage = eImageType.SourceControlModified;
                    break;
                case SourceControlFileInfo.eRepositoryItemStatus.Deleted:
                    SCImage = eImageType.SourceControlDeleted;
                    break;
                case SourceControlFileInfo.eRepositoryItemStatus.Equel:
                    SCImage = eImageType.SourceControlEquel;
                    break;
                case SourceControlFileInfo.eRepositoryItemStatus.LockedByAnotherUser:
                    SCImage = eImageType.SourceControlLockedByAnotherUser;
                    break;
                case SourceControlFileInfo.eRepositoryItemStatus.LockedByMe:
                    SCImage = eImageType.SourceControlLockedByMe;
                    break;
            }
            return SCImage;
        }

        private static SourceControlBase CreateSourceControl()
        {
            if (WorkSpace.Instance.UserProfile.Type == SourceControlBase.eSourceControlType.GIT)
            {

                if (WorkSpace.Instance != null && WorkSpace.Instance.UserProfile != null && WorkSpace.Instance.UserProfile.UserProfileOperations.SourceControlUseShellClient)
                {
                    return new GitSourceControlShellWrapper();
                }
                else
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        return new GITSourceControl();
                    }
                    else
                    {
                        return new GitSourceControlShellWrapper();
                    }
                }
            }
            else if (WorkSpace.Instance.UserProfile.Type == SourceControlBase.eSourceControlType.SVN)
            {

                if (WorkSpace.Instance != null && WorkSpace.Instance.UserProfile != null && WorkSpace.Instance.UserProfile.UserProfileOperations.SourceControlUseShellClient)
                {
                    return new SVNSourceControlShellWrapper();
                }
                else
                {
                    return TargetFrameworkHelper.Helper.GetNewSVnRepo();
                }
            }
            else
            {
                return TargetFrameworkHelper.Helper.GetNewSVnRepo();
            }
        }

        private static void ConfigureSourceControl(SourceControlBase sourceControl, string solutionFolder)
        {
            sourceControl.URL = WorkSpace.Instance.UserProfile.URL;
            sourceControl.Username = WorkSpace.Instance.UserProfile.Username;
            sourceControl.Password = WorkSpace.Instance.UserProfile.Password;
            sourceControl.LocalFolder = WorkSpace.Instance.UserProfile.LocalFolderPath;
            sourceControl.IgnoreCertificate = WorkSpace.Instance.UserProfile.UserProfileOperations.SourceControlIgnoreCertificate;

            sourceControl.SolutionFolder = solutionFolder;

            sourceControl.IsProxyConfigured = WorkSpace.Instance.UserProfile.IsProxyConfigured;
            sourceControl.ProxyAddress = WorkSpace.Instance.UserProfile.ProxyAddress;
            sourceControl.ProxyPort = WorkSpace.Instance.UserProfile.ProxyPort;
            sourceControl.Timeout = WorkSpace.Instance.UserProfile.Timeout;
            sourceControl.supressMessage = true;

            sourceControl.Branch = WorkSpace.Instance.UserProfile.Branch;
        }

        private static SolutionInfo GetSolutionInfo(string solutionFolder)
        {
            var sol = new SolutionInfo { LocalFolder = solutionFolder };
            if (WorkSpace.Instance.UserProfile.Type == SourceControlBase.eSourceControlType.SVN &&
                Directory.Exists(PathHelper.GetLongPath(sol.LocalFolder + Path.DirectorySeparatorChar + @".svn")))
            {
                sol.ExistInLocaly = true;
            }
            else if (WorkSpace.Instance.UserProfile.Type == SourceControlBase.eSourceControlType.GIT &&
                Directory.Exists(PathHelper.GetLongPath(sol.LocalFolder + Path.DirectorySeparatorChar + @".git")))
            {
                sol.ExistInLocaly = true;
            }
            else
            {
                sol.ExistInLocaly = false;
            }
            sol.SourceControlLocation = solutionFolder[(solutionFolder.LastIndexOf(Path.DirectorySeparatorChar) + 1)..];
            return sol;
        }

        private static string GetProjectURI(SolutionInfo sol, SourceControlBase sourceControl)
        {
            if (WorkSpace.Instance.UserProfile.Type == SourceControlBase.eSourceControlType.SVN && sourceControl is not SVNSourceControlShellWrapper)
            {
                if (WorkSpace.Instance.UserProfile.URL.StartsWith("SVN", StringComparison.CurrentCultureIgnoreCase))
                {
                    return sol.SourceControlLocation;
                }
                else
                {
                    string projectURI = WorkSpace.Instance.UserProfile.URL;
                    if (!projectURI.ToUpper().Contains(sol.SourceControlLocation.ToUpper()))
                    {
                        if (!projectURI.ToUpper().Contains("/SVN") && !projectURI.ToUpper().Contains("/SVN/"))
                        {
                            if (!projectURI.ToUpper().EndsWith("/"))
                            {
                                projectURI += "/";
                            }
                            projectURI += "svn/";
                        }
                        if (!projectURI.ToUpper().EndsWith("/"))
                        {
                            projectURI += "/";
                        }
                        projectURI += sol.SourceControlLocation;
                    }
                    return projectURI;
                }
            }
            else
            {
                return WorkSpace.Instance.UserProfile.URL;
            }
        }

        public static bool DownloadSolution(string SolutionFolder, bool undoSolutionLocalChanges = false, ProgressNotifier progressNotifier = null)
        {
            try
            {
                if (WorkSpace.Instance.UserProfile.LocalFolderPath == string.Empty)
                {
                    Reporter.ToUser(eUserMsgKey.SourceControlConnMissingLocalFolderInput);
                }
                if (SolutionFolder.EndsWith("\\"))
                {
                    SolutionFolder = SolutionFolder[..^1];
                }

                SourceControlBase mSourceControl = CreateSourceControl();
                if (mSourceControl != null)
                {
                    ConfigureSourceControl(mSourceControl, SolutionFolder);
                }

                SolutionInfo sol = GetSolutionInfo(SolutionFolder);
                if (sol == null)
                {
                    Reporter.ToUser(eUserMsgKey.AskToSelectSolution);
                    return false;
                }

                string projectURI = GetProjectURI(sol, mSourceControl);
                bool getProjectResult = SourceControlIntegration.CreateConfigFile(mSourceControl);
                if (!getProjectResult)
                {
                    return false;
                }

                if (!sol.ExistInLocaly && !string.IsNullOrWhiteSpace(SolutionFolder) && WorkSpace.Instance.RunningInExecutionMode
                     && SolutionFolder.StartsWith(General.DefaultGingerReposFolder, StringComparison.OrdinalIgnoreCase) &&
                     Directory.Exists(SolutionFolder) && Directory.GetFileSystemEntries(SolutionFolder).Length > 0)
                {
                    CleanSolutionFolder(SolutionFolder, mSourceControl);
                }

                if (sol.ExistInLocaly)
                {
                    mSourceControl.RepositoryRootFolder = sol.LocalFolder;
                    if (undoSolutionLocalChanges)
                    {
                        Reporter.ToLog(eLogLevel.INFO, "Reverting local Solution changes");
                        try
                        {
                            TargetFrameworkHelper.Helper.Revert(sol.LocalFolder, mSourceControl);
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Failed to revert local Solution changes, error: " + ex.Message);
                        }
                    }
                    try
                    {
                        return TargetFrameworkHelper.Helper.GetLatest(sol.LocalFolder, mSourceControl, progressNotifier);
                    }
                    catch (Exception ex) when (WorkSpace.Instance.RunningInExecutionMode && ex.Message.Contains("doesn't point at a valid Git repository or workdir", StringComparison.InvariantCultureIgnoreCase)
                            && SolutionFolder.StartsWith(General.DefaultGingerReposFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        Reporter.ToLog(eLogLevel.WARN, $"The local repository {SolutionFolder} is corrupted. Attempting to clean the folder and retry pulling the repository.");

                        CleanSolutionFolder(SolutionFolder, mSourceControl);

                        return SourceControlIntegration.GetProject(mSourceControl, sol.LocalFolder, projectURI, progressNotifier);
                    }
                }
                else
                {
                    return SourceControlIntegration.GetProject(mSourceControl, sol.LocalFolder, projectURI, progressNotifier);
                }
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while Downloading/Updating Solution from source control", e);
                return false;
            }
        }

        private static void CleanSolutionFolder(string SolutionFolder, SourceControlBase mSourceControl)
        {
            //clean folder and retry                        
            if (Directory.Exists(SolutionFolder))
            {
                try
                {
                    // Ensure any libgit2 resources released
                    mSourceControl?.Disconnect();

                    int attempts = 0;
                    while (true)
                    {
                        try
                        {
                            General.ClearDirectoryContent(SolutionFolder);
                            break;
                        }
                        catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) && attempts < 2)
                        {
                            attempts++;
                            Reporter.ToLog(eLogLevel.WARN, $"Retry {attempts} clearing corrupted repository folder due to {ex.GetType().Name}: {ex.Message}");
                            System.Threading.Thread.Sleep(300 * attempts);
                        }
                    }
                }
                catch (Exception cleanEx)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Failed to clean corrupted repository folder {SolutionFolder}", cleanEx);
                }
            }
        }

        public static List<string> GetBranches(SourceControlBase SourceControl)
        {
            return SourceControl.GetBranches();
        }

        public static string GetCurrentBranchForSolution(SourceControlBase SourceControl)
        {
            try
            {
                return SourceControl.GetCurrentWorkingBranch();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred during Fetching Branches..", ex);
                return null;
            }
        }

    }
}
