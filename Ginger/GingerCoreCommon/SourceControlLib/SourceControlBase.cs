#region License
/*
Copyright © 2014-2025 European Support Limited

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
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.UIElement;
using static GingerCoreNET.SourceControl.SourceControlFileInfo;

namespace GingerCoreNET.SourceControl
{
    // Base class for Source Control types can be SVN, GIT, TFS, XtraC etc...
    public enum eResolveConflictsSide { Local, Server, Manual }

    public abstract class SourceControlBase : INotifyPropertyChanged, ISourceControl
    {
        public enum eSourceControlType
        {
            [EnumValueDescription("")]
            None,
            [EnumValueDescription("GIT")]
            GIT,
            [EnumValueDescription("SVN")]
            SVN
        }

        public static bool Active { get; internal set; }

        string mURL;
        public string URL { get { return mURL; } set { mURL = value; OnPropertyChanged(nameof(URL)); } }

        string mUsername;
        public string Username { get { return mUsername; } set { mUsername = value; OnPropertyChanged(nameof(Username)); } }

        string mBranch;
        public string Branch { get { return mBranch; } set { mBranch = value; OnPropertyChanged(nameof(Branch)); } }

        string mPassword;
        public string Password { get { return mPassword; } set { mPassword = value; OnPropertyChanged(nameof(Password)); } }

        string mLocalFolder;
        public string LocalFolder { get { return mLocalFolder; } set { mLocalFolder = value; OnPropertyChanged(nameof(LocalFolder)); } }

        string mSourceControlLocalFolderForGlobalSolution;
        public string SourceControlLocalFolderForGlobalSolution { get { return mSourceControlLocalFolderForGlobalSolution; } set { mSourceControlLocalFolderForGlobalSolution = value; OnPropertyChanged(nameof(SourceControlLocalFolderForGlobalSolution)); } }

        string mProxyAddress;
        public string ProxyAddress { get { return mProxyAddress; } set { mProxyAddress = value; OnPropertyChanged(nameof(ProxyAddress)); } }

        string mProxyPort;
        public string ProxyPort { get { return mProxyPort; } set { mProxyPort = value; OnPropertyChanged(nameof(ProxyPort)); } }


        int mTimeout = 80;
        public int Timeout { get { return mTimeout; } set { mTimeout = value; OnPropertyChanged(nameof(Timeout)); } }

        bool mIsProxyConfigured;
        public bool IsProxyConfigured { get { return mIsProxyConfigured; } set { mIsProxyConfigured = value; OnPropertyChanged(nameof(IsProxyConfigured)); } }

        bool mIgnoreCertificate;
        public bool IgnoreCertificate { get { return mIgnoreCertificate; } set { mIgnoreCertificate = value; } }

        string mAuthorName;
        public string AuthorName { get { return mAuthorName; } set { mAuthorName = value; OnPropertyChanged(nameof(AuthorName)); } }

        string mAuthorEmail;
        public string AuthorEmail { get { return mAuthorEmail; } set { mAuthorEmail = value; OnPropertyChanged(nameof(AuthorEmail)); } }

        bool mIsPublicRepo;
        public bool IsPublicRepo { get { return mIsPublicRepo; } set { mIsPublicRepo = value; OnPropertyChanged(nameof(IsPublicRepo)); } }

        public string SolutionFolder { get; set; }

        public string RepositoryRootFolder { get; set; }

        public bool supressMessage { get; set; }
        public bool IsImportSolution { get; set; }

        public abstract eSourceControlType GetSourceControlType { get; }

        public abstract List<string> GetSourceControlmConflict { get; }

        public abstract string GetRepositoryURL(ref string error);

        //  Name of the Source Control handler: SVN, GIT, TFS...   
        public abstract string Name { get; }

        public abstract bool IsSupportingLocks { get; }

        public abstract bool IsSupportingGetLatestForIndividualFiles { get; }

        public abstract bool CreateConfigFile(ref string error);

        //Add new file to source control
        public abstract bool AddFile(string Path, ref string error);

        public abstract bool UpdateFile(string Path, ref string error);

        public abstract bool DeleteFile(string Path, ref string error);

        // get one file status
        public abstract SourceControlFileInfo.eRepositoryItemStatus GetFileStatus(string Path, bool ShowIndicationkForLockedItems, ref string error);

        // get list of files changed in path recursively - modified, add, deleted
        public abstract ObservableList<SourceControlFileInfo> GetPathFilesStatus(string Path, ref string error, bool includLockedFiles = false);

        public abstract bool GetLatest(string path, ref string error, ref List<string> conflictsPaths, ProgressNotifier progressNotifier = null);

        public abstract bool GetProject(string Path, string URI, ref string error);

        public abstract bool GetProjectWithProgress(string Path, string URI, ref string error, Amdocs.Ginger.Common.UIElement.ProgressNotifier progressNotifier = null, CancellationToken cancellationToken = default);

        public abstract void Init();

        public abstract bool InitializeRepository(string remoteURL);

        public abstract bool TestConnection(ref string error);

        public abstract List<string> GetBranches();
        public abstract bool IsRepositoryPublic();

        public abstract bool CreateBranch( string newBranchName, ref string error);

        public abstract List<string> GetLocalBranches();
        public abstract string GetCurrentWorkingBranch();

        public abstract List<string> GetConflictPaths();

        public abstract void Disconnect();

        public abstract bool CommitChanges(string Comments, ref string error);

        public abstract bool CommitAndCheckinChanges(ICollection<string> Paths, string Comments, ref string error, ref List<string> conflictsPaths, bool includLockedFiles = false);

        public abstract ObservableList<SourceControlChangesetDetails> GetUnpushedLocalCommits();

        public abstract bool UndoUncommitedChanges(List<SourceControlFileInfo> selectedFiles);

        //clear locks
        public abstract void CleanUp(string path);

        //prevent other users from commiting changes to this item
        public abstract bool Lock(string path, string lockComment, ref string error);
        //allowing other users to commit changes to this item
        public abstract bool UnLock(string path, ref string error);

        /// <summary>
        /// Get local version content for the conflicted file.
        /// </summary>
        /// <param name="conflictFilePath">Conflicted file path.</param>
        /// <returns>Local version content.</returns>
        public abstract string GetLocalContentForConflict(string conflictFilePath);

        /// <summary>
        /// Get remote version content for the conflicted file.
        /// </summary>
        /// <param name="conflictFilePath">Conflicted file path.</param>
        /// <returns>Remote version content.</returns>
        public abstract string GetRemoteContentForConflict(string conflictFilePath);

        /// <summary>
        /// Resolve merge conflict with content that contains the resolved data of the file.
        /// </summary>
        /// <param name="path">Path of the conflicted file</param>
        /// <param name="content">Content containing the resolved data.</param>
        /// <param name="error">Error details if any.</param>
        /// <returns><see langword="true"/> if the conflict was resolved successfully, <see langword="false"/> otherwise.</returns>
        public abstract bool ResolveConflictWithContent(string path, string content, ref string error);

        //resolve conflicts automatically when getting latest updates
        public abstract bool ResolveConflicts(string path, eResolveConflictsSide side, ref string error);

        //throw all local changes 
        public abstract bool Revert(string path, ref string error);

        public abstract SourceControlItemInfoDetails GetInfo(string path, ref string error);

        public abstract ObservableList<SolutionInfo> GetProjectsList();

        public class ConflictEventArgs : EventArgs
        {
            public List<string> Paths { get; set; }
        }

        // If during check-in or get latest there is confilct we will get the list here, ginger app will register handler to popup a window to ask the user what to do
        public delegate void ConflictEventHandler(ConflictEventArgs e);

        public event ConflictEventHandler Conflict;

        // impl need to raise when during Checkin or get latest conflict is found
        public void OnConflict(ConflictEventArgs e)
        {
            if (Conflict != null)
            {
                Conflict(e);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public abstract string GetLockOwner(string path, ref string error);

        public abstract SourceControlItemInfoDetails GetRepositoryInfo(ref string error);

        public Task<eImageType> GetFileStatusForRepositoryItemPath(string FullPath)
        {
            return Task.Run(() =>
            {
                string err = null;
                eRepositoryItemStatus ss = GetFileStatus(FullPath, true, ref err);
                return ss switch
                {
                    eRepositoryItemStatus.New => eImageType.SourceControlNew,
                    eRepositoryItemStatus.Modified => eImageType.SourceControlModified,
                    eRepositoryItemStatus.Equel => eImageType.SourceControlEquel,
                    eRepositoryItemStatus.LockedByMe => eImageType.SourceControlLockedByMe,
                    eRepositoryItemStatus.LockedByAnotherUser => eImageType.SourceControlLockedByAnotherUser,
                    eRepositoryItemStatus.Unknown => eImageType.SourceControlError,
                    _ => eImageType.SourceControlDeleted,
                };
            });
        }

    }

}
