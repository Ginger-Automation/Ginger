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
using GingerCore;
using GingerCore.SourceControl;
using GingerCoreNET.SourceControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

namespace Ginger.SourceControl
{
    class SourceControlIntegration
    {
        public static bool BusyInProcessWhileDownloading = false;
        public static bool conflictFlag =false;
        public static bool TestConnection(SourceControlBase SourceControl, SourceControlConnDetailsPage.eSourceControlContext context,  bool ignoreSuccessMessage)
        {
            string error = string.Empty;
            bool res = false;

            res = SourceControl.TestConnection(ref error);
            if (res)
            {
                if (!ignoreSuccessMessage)
                    Reporter.ToUser(eUserMsgKey.SourceControlConnSucss);
                return true;
            }
            else
            {
                if (error.Contains("remote has never connected"))
                    Reporter.ToUser(eUserMsgKey.SourceControlRemoteCannotBeAccessed, error);
                else
                    Reporter.ToUser(eUserMsgKey.SourceControlConnFaild, error);
                return false;
            }
        }


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

        public static bool CommitChanges(SourceControlBase SourceControl, ICollection<string> pathsToCommit, string Comments, bool includeLocks, ref bool conflictHandled)
        {
            string error = string.Empty;
            bool result = true;
            bool conflict = conflictHandled;
            List<string> conflictsPaths = new List<string>();
            if (!SourceControl.CommitChanges(pathsToCommit, Comments, ref error, ref conflictsPaths, includeLocks))
            {
                App.MainWindow.Dispatcher.Invoke(() => {
                    foreach (string cPath in conflictsPaths)
                    {
                        ResolveConflictPage resConfPage = new ResolveConflictPage(cPath);
                        if( WorkSpace.Instance.RunningInExecutionMode == true)
                            SourceControlIntegration.ResolveConflicts( WorkSpace.Instance.Solution.SourceControl, cPath, eResolveConflictsSide.Server);
                        else
                            resConfPage.ShowAsWindow();
                        result = resConfPage.IsResolved;
                        conflict = true;
                        conflictFlag= conflict;
                    }
                    if (SourceControl.GetSourceControlmConflict != null)
                        SourceControl.GetSourceControlmConflict.Clear();
                });
                if (!conflict)
                {
                    if (error.Contains("too many redirects or authentication replays"))
                        error = "Commit failed because of wrong credentials error, please enter valid Username and Password and try again";
                    if (error.Contains("is locked in another working copy"))
                        error = "This file has been locked by other user. Please remove lock and then try to Check in.";
                    App.MainWindow.Dispatcher.Invoke(() => {
                        Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, error);
                    });
                    return false;
                }
            }
            return result;
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
            bool result = true;
            bool conflictHandled = false;
            if (!SourceControl.GetLatest(path, ref error, ref conflictsPaths))
            {

                foreach (string cPath in conflictsPaths)
                {
                    ResolveConflictPage resConfPage = new ResolveConflictPage(cPath);
                    if( WorkSpace.Instance.RunningInExecutionMode == true)
                        SourceControlIntegration.ResolveConflicts(SourceControl, cPath, eResolveConflictsSide.Server);
                    else
                        resConfPage.ShowAsWindow();
                    result = resConfPage.IsResolved;

                    if (!result)
                    {
                        Reporter.ToUser(eUserMsgKey.SourceControlGetLatestConflictHandledFailed);
                        return false;
                    }
                    conflictHandled = true;
                }
                if (!conflictHandled)
                {
                    Reporter.ToUser(eUserMsgKey.SourceControlUpdateFailed, error);
                    return false;
                }
            }
            return true;
        }

        internal static string GetLockOwner(SourceControlBase SourceControl, string path)
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

        internal static void Lock(SourceControlBase SourceControl, string path, string lockComment)
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

        internal static void UnLock(SourceControlBase SourceControl, string path)
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

        internal static SourceControlItemInfoDetails GetRepositoryInfo(SourceControlBase sourceControl)
        {
            string error = string.Empty;
            return sourceControl.GetRepositoryInfo(ref error);
        }

        internal static BitmapImage GetItemSourceControlImage(string FileName,ref SourceControlFileInfo.eRepositoryItemStatus ItemSourceControlStatus)
        {

            if ( WorkSpace.Instance.Solution.SourceControl == null || FileName == null)
            {
                return null;
            }

            SourceControlFileInfo.eRepositoryItemStatus RIS = SourceControlIntegration.GetFileStatus( WorkSpace.Instance.Solution.SourceControl, FileName,  WorkSpace.Instance.Solution.ShowIndicationkForLockedItems);
            ItemSourceControlStatus = RIS;
            BitmapImage img = null;
            switch (RIS)
            {
                case SourceControlFileInfo.eRepositoryItemStatus.New:
                    img = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@SourceControlItemAdded_10x10.png"));
                    break;
                case SourceControlFileInfo.eRepositoryItemStatus.Modified:
                    img = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@SourceControlItemChange_10x10.png"));
                    break;
                case SourceControlFileInfo.eRepositoryItemStatus.Deleted:
                    img = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@SourceControlItemDeleted_10x10.png"));
                    break;
                case SourceControlFileInfo.eRepositoryItemStatus.Equel:
                    img = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@SourceControlItemUnchanged_10x10.png"));
                    break;
                case SourceControlFileInfo.eRepositoryItemStatus.LockedByAnotherUser:
                    img = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@Lock_Red_10x10.png"));
                    break;
                case SourceControlFileInfo.eRepositoryItemStatus.LockedByMe:
                    img = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@Lock_Yellow_10x10.png"));
                    break;
            }
            return img;
        }


        public static SourceControlBase.eSourceControlType CheckForSolutionSourceControlType(string SolutionFolder, ref string ReposiytoryRootFolder )
        {
            string SourceControlRootFolder = SolutionFolder;
            while (SourceControlRootFolder != Path.GetPathRoot(SolutionFolder))
            {
                //Path.get
                if (Directory.Exists(SourceControlRootFolder + @"\" + ".git"))
                {
                    FileAttributes attributes = File.GetAttributes(SourceControlRootFolder + @"\" + ".git");
                    if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    {
                        ReposiytoryRootFolder = Path.GetFullPath(SourceControlRootFolder);
                        return SourceControlBase.eSourceControlType.GIT;
                    }
                }
                if (Directory.Exists(SourceControlRootFolder + @"\" + ".svn"))
                {
                    FileAttributes attributes = File.GetAttributes(SourceControlRootFolder+ @"\" + ".svn");
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

        internal static eImageType GetFileImage(string path)
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
    }
}
