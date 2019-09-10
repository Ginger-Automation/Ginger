#region License
/*
Copyright © 2014-2019 European Support Limited

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
using Amdocs.Ginger.IO;
using GingerCore;
using GingerCore.SourceControl;
using GingerCoreNET.SourceControl;
using System;
using System.Collections.Generic;
using System.IO;


namespace Ginger.SourceControl
{
    public class SourceControlIntegration
    {
        public static bool BusyInProcessWhileDownloading = false;


        public static bool conflictFlag =false;
 


        public static bool AddFile( SourceControlBase SourceControl, string path)
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

            if (!SourceControl.UpdateFile(path, ref error))
            {
                Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, error);
                return false;
            }
            return true;
        }



        public static bool CleanUp(SourceControlBase SourceControl, string folder)
        {
            SourceControl.CleanUp(folder);
            return true;
        }

        public static ObservableList<SourceControlFileInfo> GetPathFilesStatus(SourceControlBase SourceControl, string Path)
        {
            string error = string.Empty;
            ObservableList<SourceControlFileInfo> OL =  SourceControl.GetPathFilesStatus(Path, ref error,  WorkSpace.Instance.Solution.ShowIndicationkForLockedItems);
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
            return  WorkSpace.Instance.Solution.SourceControl.GetRepositoryURL(ref error);
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

     
                if (conflictsPaths.Count>0)
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

            if (!SourceControl.ResolveConflicts(path, side, ref error))
            {
                Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, error);
                return false;
            }

            return true;
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
            return SourceControl.GetInfo(path, ref  error);
        }

        public static SourceControlItemInfoDetails GetRepositoryInfo(SourceControlBase sourceControl)
        {
            string error = string.Empty;
            return sourceControl.GetRepositoryInfo(ref error);
        }





        public static SourceControlBase.eSourceControlType CheckForSolutionSourceControlType(string SolutionFolder, ref string ReposiytoryRootFolder )
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
                    FileAttributes attributes = File.GetAttributes(SourceControlRootFolder+ Path.DirectorySeparatorChar + ".svn");
                    if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    {
                        ReposiytoryRootFolder = Path.GetFullPath(SourceControlRootFolder);
                        return SourceControlBase.eSourceControlType.SVN;
                    }
                }
                SourceControlRootFolder = System.IO.Directory.GetParent(SourceControlRootFolder).FullName ;
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


        public static void DownloadSolution(string SolutionFolder)
        {
            try {
                SourceControlBase mSourceControl;
                if (WorkSpace.Instance.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.GIT)
                {

                    mSourceControl = new GITSourceControl();
                }
                else if (WorkSpace.Instance.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.SVN)
                {
                    mSourceControl = RepositoryItemHelper.RepositoryItemFactory.GetNewSVnRepo();
                }
                else
                {
                    mSourceControl = RepositoryItemHelper.RepositoryItemFactory.GetNewSVnRepo();
                }

                if (mSourceControl != null)
                {
                    WorkSpace.Instance.UserProfile.SourceControlType = mSourceControl.GetSourceControlType;
                    mSourceControl.SourceControlURL = WorkSpace.Instance.UserProfile.SourceControlURL;
                    mSourceControl.SourceControlUser = WorkSpace.Instance.UserProfile.SourceControlUser;
                    mSourceControl.SourceControlPass = WorkSpace.Instance.UserProfile.SourceControlPass;
                    mSourceControl.SourceControlLocalFolder = WorkSpace.Instance.UserProfile.SourceControlLocalFolder;
                    mSourceControl.SolutionFolder = SolutionFolder;

                    mSourceControl.SourceControlConfigureProxy = WorkSpace.Instance.UserProfile.SolutionSourceControlConfigureProxy;
                    mSourceControl.SourceControlProxyAddress = WorkSpace.Instance.UserProfile.SolutionSourceControlProxyAddress;
                    mSourceControl.SourceControlProxyPort = WorkSpace.Instance.UserProfile.SolutionSourceControlProxyPort;
                    mSourceControl.SourceControlTimeout = WorkSpace.Instance.UserProfile.SolutionSourceControlTimeout;
                    mSourceControl.supressMessage = true;
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
                if (WorkSpace.Instance.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.SVN && Directory.Exists(PathHelper.GetLongPath(sol.LocalFolder)))
                {
                    sol.ExistInLocaly = true;
                }
                else if (WorkSpace.Instance.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.GIT && Directory.Exists(PathHelper.GetLongPath(sol.LocalFolder +Path.DirectorySeparatorChar + @".git")))
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
                    return;
                }

                string ProjectURI = string.Empty;
                if (WorkSpace.Instance.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.SVN)
                {
                    ProjectURI = WorkSpace.Instance.UserProfile.SourceControlURL.StartsWith("SVN", StringComparison.CurrentCultureIgnoreCase) ?
                    sol.SourceControlLocation : WorkSpace.Instance.UserProfile.SourceControlURL + sol.SourceControlLocation;
                }
                else
                {
                    ProjectURI = WorkSpace.Instance.UserProfile.SourceControlURL;
                }
                bool getProjectResult = true;
                getProjectResult = SourceControlIntegration.CreateConfigFile(mSourceControl);
                if (getProjectResult != true)
                {
                    return;
                }

                if (sol.ExistInLocaly == true)
                {
                    mSourceControl.RepositoryRootFolder = sol.LocalFolder;


                    RepositoryItemHelper.RepositoryItemFactory.GetLatest(sol.LocalFolder, mSourceControl);

                }
                else
                {
                    getProjectResult = SourceControlIntegration.GetProject(mSourceControl, sol.LocalFolder, ProjectURI);
                }
            }
            catch (Exception e)
            {
                Reporter.ToConsole(eLogLevel.INFO, "Error Downloading solution ");
                Reporter.ToConsole(eLogLevel.INFO, e.Message);
                Reporter.ToConsole(eLogLevel.INFO, e.Source);
   

            }
            }

    }
}
