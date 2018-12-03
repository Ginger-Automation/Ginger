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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static GingerCoreNET.SourceControl.SourceControlFileInfo;
using System.Threading.Tasks;

namespace GingerCoreNET.SourceControl
{
    // Base class for Source Control types can be SVN, GIT, TFS, XtraC etc...
    public enum eResolveConflictsSide { Local, Server }

    public abstract class SourceControlBase : INotifyPropertyChanged, ISourceControl
    {
        public static class Fields
        {
            public static string SourceControlURL = "SourceControlURL";
            public static string SourceControlUser = "SourceControlUser";
            public static string SourceControlPass = "SourceControlPass";
            public static string SourceControlLocalFolder = "SourceControlLocalFolder";
            public static string SourceControlConfigureProxy = "SourceControlConfigureProxy";
            public static string SourceControlProxyAddress = "SourceControlProxyAddress";
            public static string SourceControlProxyPort = "SourceControlProxyPort";
            public static string SolutionSourceControlAuthorName = "SolutionSourceControlAuthorName";
            public static string SolutionSourceControlAuthorEmail = "SolutionSourceControlAuthorEmail";
        }

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

        string mSourceControlURL;
        public string SourceControlURL { get { return mSourceControlURL; } set { mSourceControlURL = value; OnPropertyChanged(Fields.SourceControlURL); } }

        string mSourceControlUser;
        public string SourceControlUser { get { return mSourceControlUser; } set { mSourceControlUser = value; OnPropertyChanged(Fields.SourceControlUser); } }

        string mSourceControlPass;
        public string SourceControlPass { get { return mSourceControlPass; } set { mSourceControlPass = value; OnPropertyChanged(Fields.SourceControlPass); } }

        string mSourceControlLocalFolder;
        public string SourceControlLocalFolder { get { return mSourceControlLocalFolder; } set { mSourceControlLocalFolder = value; OnPropertyChanged(Fields.SourceControlLocalFolder); } }

        string mSourceControlProxyAddress;
        public string SourceControlProxyAddress { get { return mSourceControlProxyAddress; } set { mSourceControlProxyAddress = value; OnPropertyChanged(Fields.SourceControlProxyAddress); } }

        string mSourceControlProxyPort;
        public string SourceControlProxyPort { get { return mSourceControlProxyPort; } set { mSourceControlProxyPort = value; OnPropertyChanged(Fields.SourceControlProxyPort); } }


        int mSourceControlTimeout=80;
        public int SourceControlTimeout { get { return mSourceControlTimeout; } set { mSourceControlTimeout = value; OnPropertyChanged(nameof(SourceControlTimeout)); } }

        bool mSourceControlConfigureProxy;
        public bool SourceControlConfigureProxy { get { return mSourceControlConfigureProxy; } set { mSourceControlConfigureProxy = value; OnPropertyChanged(Fields.SourceControlConfigureProxy); } }


        string mSolutionSourceControlAuthorName;
        public string SolutionSourceControlAuthorName { get { return mSolutionSourceControlAuthorName; } set { mSolutionSourceControlAuthorName = value; OnPropertyChanged(Fields.SourceControlConfigureProxy); } }

        string mSolutionSourceControlAuthorEmail;
        public string SolutionSourceControlAuthorEmail { get { return mSolutionSourceControlAuthorEmail; } set { mSolutionSourceControlAuthorEmail = value; OnPropertyChanged(Fields.SourceControlConfigureProxy); } }

        public string SolutionFolder { get; set; }

        public string RepositoryRootFolder { get; set; }

        public bool supressMessage { get; set; }


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
        public abstract ObservableList<SourceControlFileInfo> GetPathFilesStatus(string Path, ref string error, List<string> PathsToIgnore = null, bool includLockedFiles = false);

        public abstract bool GetLatest(string path, ref string error, ref List<string> conflictsPaths);

        public abstract bool GetProject(string Path, string URI, ref string error);

        public abstract void Init();

        public abstract bool TestConnection(ref string error);

        public abstract void Disconnect();

        public abstract bool CommitChanges(string Comments, ref string error);

        public abstract bool CommitChanges(ICollection<string> Paths, string Comments, ref string error, ref List<string> conflictsPaths, bool includLockedFiles = false);

        //clear locks
        public abstract void CleanUp(string path);

        //prevent other users from commiting changes to this item
        public abstract bool Lock(string path, string lockComment, ref string error);
        //allowing other users to commit changes to this item
        public abstract bool UnLock(string path, ref string error);

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

        public async Task<eImageType> GetFileStatusForRepositoryItemPath(string FullPath)
        {
            string err = null;
            return await Task.Run(() =>
            {
                eRepositoryItemStatus ss = GetFileStatus(FullPath, true, ref err);
                switch (ss)
                {
                    case eRepositoryItemStatus.New:
                        return eImageType.SourceControlNew;

                    case eRepositoryItemStatus.Modified:
                        return eImageType.SourceControlModified;

                    case eRepositoryItemStatus.Equel:
                        return eImageType.SourceControlEquel;

                    case eRepositoryItemStatus.LockedByMe:
                        return eImageType.SourceControlLockedByMe;

                    case eRepositoryItemStatus.LockedByAnotherUser:
                        return eImageType.SourceControlLockedByAnotherUser;

                    case eRepositoryItemStatus.Unknown:
                        return eImageType.SourceControlError;

                    default:
                        return eImageType.SourceControlDeleted;
                }
            }).ConfigureAwait(true);
        }
    }
}