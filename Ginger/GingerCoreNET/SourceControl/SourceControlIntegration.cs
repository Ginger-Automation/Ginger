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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Core;
using Amdocs.Ginger.CoreNET.SourceControl;
using Amdocs.Ginger.IO;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.SourceControl;
using GingerCoreNET.SourceControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

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

        public static string GetSourceControlType(SourceControlBase SourceControl)
        {
            return SourceControl.GetSourceControlType.ToString();
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
            return SourceControl.GetProjectsList();
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

        public static bool GetProject(SourceControlBase SourceControl, string Path, string URI)
        {
            string error = string.Empty;
            if (!SourceControl.GetProject(Path, URI, ref error))
            {
                Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, error);
                return false;
            }
            return true;
        }



        public static bool GetLatest(string path, SourceControlBase SourceControl)
        {
            string error = string.Empty;
            List<string> conflictsPaths = new List<string>();
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

            List<string> conflictsPaths = new List<string>();
            return mSourceControl.CommitChanges(paths, "check-in self healing changes.", ref error, ref conflictsPaths, false);

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
                    Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, error);
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


        public static bool DownloadSolution(string SolutionFolder, bool undoSolutionLocalChanges = false)
        {
            try
            {
                SourceControlBase mSourceControl;
                if (WorkSpace.Instance.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.GIT)
                {

                    if (WorkSpace.Instance != null && WorkSpace.Instance.UserProfile != null && WorkSpace.Instance.UserProfile.SourceControlUseShellClient)
                    {
                        mSourceControl = new GitSourceControlShellWrapper();
                    }
                    else
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            mSourceControl = new GITSourceControl();
                        }
                        else
                        {
                            mSourceControl = new GitSourceControlShellWrapper();
                        }
                    }



                }
                else if (WorkSpace.Instance.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.SVN)
                {

                    if (WorkSpace.Instance != null && WorkSpace.Instance.UserProfile != null && WorkSpace.Instance.UserProfile.SourceControlUseShellClient)
                    {
                        mSourceControl = new SVNSourceControlShellWrapper();
                    }
                    else
                    {
                        mSourceControl = TargetFrameworkHelper.Helper.GetNewSVnRepo();
                    }

                }
                else
                {
                    mSourceControl = TargetFrameworkHelper.Helper.GetNewSVnRepo();
                }

                if (mSourceControl != null)
                {
                    WorkSpace.Instance.UserProfile.SourceControlType = mSourceControl.GetSourceControlType;
                    mSourceControl.SourceControlURL = WorkSpace.Instance.UserProfile.SourceControlURL;
                    mSourceControl.SourceControlUser = WorkSpace.Instance.UserProfile.SourceControlUser;
                    mSourceControl.SourceControlPass = WorkSpace.Instance.UserProfile.SourceControlPass;
                    mSourceControl.SourceControlLocalFolder = WorkSpace.Instance.UserProfile.SourceControlLocalFolder;
                    mSourceControl.IgnoreCertificate = WorkSpace.Instance.UserProfile.SourceControlIgnoreCertificate;

                    mSourceControl.SolutionFolder = SolutionFolder;

                    mSourceControl.SourceControlConfigureProxy = WorkSpace.Instance.UserProfile.SolutionSourceControlConfigureProxy;
                    mSourceControl.SourceControlProxyAddress = WorkSpace.Instance.UserProfile.SolutionSourceControlProxyAddress;
                    mSourceControl.SourceControlProxyPort = WorkSpace.Instance.UserProfile.SolutionSourceControlProxyPort;
                    mSourceControl.SourceControlTimeout = WorkSpace.Instance.UserProfile.SolutionSourceControlTimeout;
                    mSourceControl.supressMessage = true;

                    mSourceControl.SourceControlBranch = WorkSpace.Instance.UserProfile.SolutionSourceControlBranch;
                }

                if (WorkSpace.Instance.UserProfile.SourceControlLocalFolder == string.Empty)
                {
                    Reporter.ToUser(eUserMsgKey.SourceControlConnMissingLocalFolderInput);
                }
                if (SolutionFolder.EndsWith("\\"))
                {
                    SolutionFolder = SolutionFolder.Substring(0, SolutionFolder.Length - 1);
                }

                SolutionInfo sol = new SolutionInfo();
                sol.LocalFolder = SolutionFolder;
                if (WorkSpace.Instance.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.SVN && Directory.Exists(PathHelper.GetLongPath(sol.LocalFolder + Path.DirectorySeparatorChar + @".svn")))
                {
                    sol.ExistInLocaly = true;
                }
                else if (WorkSpace.Instance.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.GIT && Directory.Exists(PathHelper.GetLongPath(sol.LocalFolder + Path.DirectorySeparatorChar + @".git")))
                {
                    sol.ExistInLocaly = true;
                }
                else
                {
                    sol.ExistInLocaly = false;
                }

                sol.SourceControlLocation = SolutionFolder.Substring(SolutionFolder.LastIndexOf(Path.DirectorySeparatorChar) + 1);

                if (sol == null)
                {
                    Reporter.ToUser(eUserMsgKey.AskToSelectSolution);
                    return false;
                }

                string ProjectURI = string.Empty;
                if (WorkSpace.Instance.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.SVN && !(mSourceControl is SVNSourceControlShellWrapper))
                {

                    if (WorkSpace.Instance.UserProfile.SourceControlURL.StartsWith("SVN", StringComparison.CurrentCultureIgnoreCase))
                    {
                        ProjectURI = sol.SourceControlLocation;
                    }
                    else
                    {
                        if (WorkSpace.Instance.UserProfile.SourceControlURL.ToUpper().Contains(sol.SourceControlLocation.ToUpper()))
                        {
                            ProjectURI = WorkSpace.Instance.UserProfile.SourceControlURL;
                        }
                        else
                        {
                            ProjectURI = WorkSpace.Instance.UserProfile.SourceControlURL;
                            if (!ProjectURI.ToUpper().Contains("/SVN") && !ProjectURI.ToUpper().Contains("/SVN/"))
                            {
                                if (!ProjectURI.ToUpper().EndsWith("/"))
                                {
                                    ProjectURI += "/";
                                }
                                ProjectURI += "svn/";
                            }
                            if (!ProjectURI.ToUpper().EndsWith("/"))
                            {
                                ProjectURI += "/";
                            }
                            ProjectURI += sol.SourceControlLocation;
                        }


                        //if(WorkSpace.Instance.GingerCLIMode==Amdocs.Ginger.CoreNET.RunLib.CLILib.eGingerCLIMode.run)
                        //{
                        //    ProjectURI = WorkSpace.Instance.UserProfile.SourceControlURL;
                        //}
                        //else
                        //{
                        //    ProjectURI= WorkSpace.Instance.UserProfile.SourceControlURL + sol.SourceControlLocation;
                        //}
                    }

                }
                else
                {
                    ProjectURI = WorkSpace.Instance.UserProfile.SourceControlURL;
                }
                bool getProjectResult = true;
                getProjectResult = SourceControlIntegration.CreateConfigFile(mSourceControl);
                if (getProjectResult != true)
                {
                    return false;
                }

                if (sol.ExistInLocaly == true)
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
                    return TargetFrameworkHelper.Helper.GetLatest(sol.LocalFolder, mSourceControl);
                }
                else
                {
                    return getProjectResult = SourceControlIntegration.GetProject(mSourceControl, sol.LocalFolder, ProjectURI);
                }
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while Downloading/Updating Solution from source control", e);
                return false;
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
                return SourceControl.GetCurrentBranchForSolution();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred during Fetching Branches..", ex);
                return null;
            }
        }

    }
}
