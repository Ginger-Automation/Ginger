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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using GingerCoreNET.SourceControl;
using Medallion.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
namespace Amdocs.Ginger.CoreNET.SourceControl
{

    /// <summary>
    /// Implements SVN SourceControl Operations using shell commands. Useful only for checkout and getlatest 
    /// </summary>
    public class SVNSourceControlShellWrapper : SourceControlBase
    {
        private string SourceCpntrolURL;
        public SVNSourceControlShellWrapper()
        {
            try
            {
                var result = Command.Run("svn").Result;

            }
            catch (Exception e)

            {
                throw new PlatformNotSupportedException("SVN Commandline tools not found");
            }
        }

        public override string Name { get { return "SVN"; } }

        public override bool IsSupportingLocks { get { return true; } }

        public override bool IsSupportingGetLatestForIndividualFiles { get { return true; } }

        public override eSourceControlType GetSourceControlType { get { return eSourceControlType.SVN; } }

        public override List<string> GetSourceControlmConflict => throw new NotImplementedException();

        public override bool AddFile(string Path, ref string error)
        {
            throw new NotImplementedException();
        }

        public override void CleanUp(string path)
        {
            throw new NotImplementedException();
        }

        public override bool CommitChanges(string Comments, ref string error)
        {
            throw new NotImplementedException();
        }

        public override bool CommitAndCheckinChanges(ICollection<string> Paths, string Comments, ref string error, ref List<string> conflictsPaths, bool includLockedFiles = false)
        {
            throw new NotImplementedException("CommitAndCheckinChanges not implemented");
        }

        public override bool CreateConfigFile(ref string error)
        {
            return true;
        }

        public override bool DeleteFile(string Path, ref string error)
        {
            throw new NotImplementedException();
        }

        public override void Disconnect()
        {
            ;
        }

        public override List<string> GetConflictPaths()
        {
            throw new NotImplementedException();
        }

        public override List<string> GetBranches()
        {
            throw new NotImplementedException();
        }

        public override string GetCurrentWorkingBranch()
        {
            throw new NotImplementedException();
        }

        public override SourceControlFileInfo.eRepositoryItemStatus GetFileStatus(string Path, bool ShowIndicationkForLockedItems, ref string error)
        {
            throw new NotImplementedException();
        }

        public override SourceControlItemInfoDetails GetInfo(string path, ref string error)
        {
            throw new NotImplementedException();
        }

        public override bool GetLatest(string path, ref string error, ref List<string> conflictsPaths, ProgressNotifier progressNotifier = null)
        {
            Console.WriteLine("Reverting and Get Latest");
            RunSVNCommand(new object[] { "revert", "-R", ".", "--username", Username, "--password", Password }, path);
            return RunSVNCommand(new object[] { "up", "--username", Username, "--password", Password }, path);
        }

        public override string GetLockOwner(string path, ref string error)
        {
            throw new NotImplementedException();
        }

        public override ObservableList<SourceControlFileInfo> GetPathFilesStatus(string Path, ref string error, bool includLockedFiles = false)
        {
            throw new NotImplementedException();
        }

        public override bool GetProject(string Path, string URI, ref string error)
        {
            Console.WriteLine("Check Out");

            URL = URI;

            return RunSVNCommand(new object[] { "checkout", URI, Path, "--username", Username, "--password", Password }, Path);
        }


        public override ObservableList<SolutionInfo> GetProjectsList()
        {
            throw new NotImplementedException();
        }

        public override SourceControlItemInfoDetails GetRepositoryInfo(ref string error)
        {
            throw new NotImplementedException();
        }

        public override string GetRepositoryURL(ref string error)
        {
            return URL;
        }

        public override void Init()
        {

        }

        public override bool Lock(string path, string lockComment, ref string error)
        {
            throw new NotImplementedException();
        }

        public override bool ResolveConflicts(string path, eResolveConflictsSide side, ref string error)
        {
            throw new NotImplementedException();
        }

        public override bool Revert(string path, ref string error)
        {
            Console.WriteLine("Reverting ");
            RunSVNCommand(new object[] { "revert", "-R", "." }, path);
            return true;
        }

        public override bool TestConnection(ref string error)
        {
            throw new NotImplementedException();
        }

        public override bool UnLock(string path, ref string error)
        {
            throw new NotImplementedException();
        }

        public override bool UpdateFile(string Path, ref string error)
        {
            throw new NotImplementedException();
        }

        private static bool RunSVNCommand(object[] args, string Path)
        {
            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }
            var result = Command.Run("svn",
                args,
             options: o => o.WorkingDirectory(Path)).Result;

            if (result.Success)
            {
                // Reporter.ToLog(eLogLevel.INFO,result.StandardOutput);
                return true;
            }
            else
            {
                Console.WriteLine(result.StandardError);
                // Reporter.ToLog(eLogLevel.ERROR, result.StandardError);
                return false;
            }
        }

        public override bool InitializeRepository(string remoteURL)
        {
            throw new NotImplementedException();
        }

        public override bool IsRepositoryPublic()
        {
            return false;
        }

        public override string GetLocalContentForConflict(string conflictedFilePath)
        {
            throw new NotImplementedException();
        }

        public override string GetRemoteContentForConflict(string conflictedFilePath)
        {
            throw new NotImplementedException();
        }

        public override bool ResolveConflictWithContent(string path, string content, ref string error)
        {
            throw new NotImplementedException();
        }

        public override ObservableList<SourceControlChangesetDetails> GetUnpushedLocalCommits()
        {
            throw new NotImplementedException("GetUnpushedLocalCommits not Implemented");
        }
        public override bool UndoUncommitedChanges(List<SourceControlFileInfo> selectedFiles)
        {
            throw new NotImplementedException("UndoUncommitedChanges not Implemented");
        }

        public override bool GetProjectWithProgress(string Path, string URI, ref string error, ProgressNotifier progressNotifier = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override bool CreateBranch( string newBranchName, ref string error  )
        {
            throw new NotImplementedException();
        }

        public override List<string> GetLocalBranches()
        {
            throw new NotImplementedException();
        }

    }
}
