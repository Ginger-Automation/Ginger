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

using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System.IO;
using GingerCoreNET.SourceControl;
using amdocs.ginger.GingerCoreNET;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GingerCore.SourceControl
{

    public class GITSourceControl : SourceControlBase
    {
        public override string Name { get { return "GIT"; } }

        public override bool IsSupportingLocks { get { return false; } }

        public override bool IsSupportingGetLatestForIndividualFiles { get { return false; } }

        public override eSourceControlType GetSourceControlType { get { return eSourceControlType.GIT; } }

        public override List<string> GetSourceControlmConflict { get { return null; } }

        private string CheckinComment { get; set; }

        public override bool AddFile(string Path, ref string error)
        {
            try
            {
                Stage(Path);
            }
            catch (Exception e)
            {
                error = e.Message + Environment.NewLine + e.InnerException;
                return false;
            }
            return true;
        }

        public override void CleanUp(string path)
        {
        }

        public override bool CommitChanges(string Comments, ref string error)
        {
            return false;
        }

        public override bool CommitChanges(ICollection<string> Paths, string Comments, ref string error, ref List<string> conflictsPaths, bool includLockedFiles = false)
        {
            if (TestConnection(ref error))
            {
                //Commit Changes
                bool result = true;
                try
                {
                    Commit(Comments);
                }
                catch (Exception e)
                {
                    error = e.Message + Environment.NewLine + e.InnerException;
                    Reporter.ToLog(eLogLevel.ERROR, error, e);

                }
                finally
                {
                    try
                    {
                        Push();
                        Pull();
                        using (var repo = new LibGit2Sharp.Repository(RepositoryRootFolder))
                        {
                            Reporter.ToUser(eUserMsgKey.CommitedToRevision, repo.Head.Tip.Sha);
                        }
                    }
                    catch (Exception e)
                    {

                        error = error + Environment.NewLine + e.Message + Environment.NewLine + e.InnerException;
                        if (e.Message.Contains("403"))
                        {
                            Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Check-In Failed, please check if user has Write Permission for this repository");
                        }
                        else
                        {
                            Reporter.ToUser(eUserMsgKey.SourceControlCommitFailed, e.Message);
                        }
                        Reporter.ToLog(eLogLevel.ERROR, error, e);
                        try
                        {
                            Pull();
                        }
                        catch { }

                        conflictsPaths = GetConflictsPaths();
                        result = false;
                    }
                }
                return result;
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Unable to connect to repository");
                return false;
            }
        }

        private List<string> GetConflictsPathsforGetLatestConflict(string path)
        {
            List<string> conflictPaths = new List<string>();
            try
            {
                Stage("*");
                Commit(CheckinComment + "-Ginger_CheckIn");
                Pull();
            }
            catch (Exception e)
            {
                try
                {
                    Pull();
                    Console.WriteLine(e.StackTrace);
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.WARN, ex.Message, ex);
                }
            }
            finally
            {
                try
                {
                    conflictPaths = GetConflictsPaths();
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.WARN, ex.Message, ex);
                }
            }
            return conflictPaths;
        }

        public override bool DeleteFile(string Path, ref string error)
        {
            try
            {
                using (var repo = new LibGit2Sharp.Repository(RepositoryRootFolder))
                {
                    Commands.Remove(repo, Path);
                }
            }
            catch (Exception e)
            {
                error = e.Message + Environment.NewLine + e.InnerException;
                return false;
            }
            return true;
        }

        public override SourceControlFileInfo.eRepositoryItemStatus GetFileStatus(string Path, bool ShowIndicationkForLockedItems, ref string error)
        {
            try
            {
                Console.WriteLine("GITHub - GetFileStatus");
                Path = System.IO.Path.GetFullPath(Path);
                int RepositoryFolderCount = RepositoryRootFolder.Count();
                string localFilePath = string.Empty;
                if (Path.Count() > (RepositoryFolderCount))
                    localFilePath = Path.Substring(RepositoryFolderCount);

                if (localFilePath.StartsWith("\\"))
                    localFilePath = localFilePath.Substring(1);

                using (var repo = new LibGit2Sharp.Repository(RepositoryRootFolder))
                {
                    if (Path == SolutionFolder && repo.RetrieveStatus().Count() != 0)
                        return SourceControlFileInfo.eRepositoryItemStatus.Modified;

                    foreach (var item in repo.RetrieveStatus())
                    {
                        if (NormalizePath(item.FilePath).StartsWith(NormalizePath(localFilePath)) && item.State != FileStatus.NewInWorkdir)
                            return SourceControlFileInfo.eRepositoryItemStatus.Modified;

                        if (NormalizePath(localFilePath) == NormalizePath(item.FilePath))
                        {
                            return GetItemStatus(item.State);
                        }
                        else if (NormalizePath(item.FilePath).Contains(NormalizePath(localFilePath)) && !localFilePath.EndsWith(".xml"))
                        {
                            return GetItemStatus(item.State);
                        }
                    }
                }
                return SourceControlFileInfo.eRepositoryItemStatus.Equel;
            }
            catch (Exception ex)
            {
                error = ex.Message + Environment.NewLine + ex.InnerException;
                return SourceControlFileInfo.eRepositoryItemStatus.Unknown;
            }
        }

        private SourceControlFileInfo.eRepositoryItemStatus GetItemStatus(FileStatus state)
        {
            if (state == FileStatus.ModifiedInWorkdir || state == FileStatus.ModifiedInIndex)
            {
                return SourceControlFileInfo.eRepositoryItemStatus.Modified;
            }
            else if (state == FileStatus.DeletedFromWorkdir)
            {
                return SourceControlFileInfo.eRepositoryItemStatus.Deleted;
            }
            else if (state == FileStatus.Unaltered)
            {
                return SourceControlFileInfo.eRepositoryItemStatus.Equel;
            }
            else
            {
                //if state == FileStatus.NewInWorkdir
                return SourceControlFileInfo.eRepositoryItemStatus.New;
            }
        }

        private static string NormalizePath(string path)
        {
            return Path.GetFullPath(path)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .ToUpperInvariant();
        }
        public override string GetRepositoryURL(ref string error)
        {
            string remoteURL = string.Empty;
            try
            {
                using (var repo = new LibGit2Sharp.Repository(RepositoryRootFolder))
                {
                    remoteURL = repo.Config.Get<string>("remote", "origin", "url").Value;
                }
            }
            catch (Exception e)
            {
                error = error + Environment.NewLine + e.Message + Environment.NewLine + e.InnerException;
            }
            return remoteURL;
        }

        public override bool GetLatest(string path, ref string error, ref List<string> conflictsPaths)
        {
            Console.WriteLine("GITHub - GetLatest");
            if (TestConnection(ref error))
            {
                try
                {
                    MergeResult result;
                    result = Pull();

                    if (result.Status != MergeStatus.Conflicts)
                    {
                        using (var repo = new LibGit2Sharp.Repository(RepositoryRootFolder))
                        {
                            if (supressMessage == true)

                                Reporter.ToLog(eLogLevel.INFO, "The solution was updated successfully, Update status: " + result.Status + ", to Revision :" + repo.Head.Tip.Sha);

                            else
                                Reporter.ToUser(eUserMsgKey.GitUpdateState, result.Status, repo.Head.Tip.Sha);
                        }
                    }
                    else
                    {
                        if (supressMessage == true)
                            Reporter.ToLog(eLogLevel.ERROR, "Failed to update the solution from source control. Error Details: 'The files are not connected to source control'");
                        else
                            Reporter.ToUser(eUserMsgKey.SourceControlUpdateFailed, "The files are not connected to source control");
                    }

                }
                catch (Exception ex)
                {
                    conflictsPaths = GetConflictsPathsforGetLatestConflict(path);
                    error = ex.Message + Environment.NewLine + ex.InnerException;
                    return false;
                }
                return true;
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Unable to connect to repository");
                return false;
            }
        }

        public override ObservableList<SourceControlFileInfo> GetPathFilesStatus(string Path, ref string error, bool includLockedFiles = false)
        {
            Console.WriteLine("GITHub - GetPathFilesStatus");
            ObservableList<SourceControlFileInfo> list = new ObservableList<SourceControlFileInfo>();

            try
            {
                string relativePath = System.IO.Path.GetFullPath(Path);
                relativePath = relativePath.Substring(RepositoryRootFolder.Count());

                if (relativePath.StartsWith(@"\"))
                    relativePath = relativePath.Substring(1);

                using (var repo = new LibGit2Sharp.Repository(RepositoryRootFolder))
                {
                    foreach (var item in repo.RetrieveStatus())
                    {
                        if (WorkSpace.Instance.SolutionRepository.IsSolutionPathToAvoid(System.IO.Path.Combine(RepositoryRootFolder, item.FilePath)))
                        {
                            continue;
                        }

                        if (System.IO.Path.GetExtension(item.FilePath) == ".ldb" || System.IO.Path.GetExtension(item.FilePath) == ".ignore" || System.IO.Path.GetExtension(item.FilePath) == ".db")
                            continue;


                        //sometimes remote file path uses / otherwise \  our code should be path independent 
                        if (relativePath == string.Empty || System.IO.Path.GetFullPath(System.IO.Path.Combine(RepositoryRootFolder, item.FilePath)).StartsWith(System.IO.Path.GetFullPath(Path)))
                        {
                            SourceControlFileInfo SCFI = new SourceControlFileInfo();
                            SCFI.Path = RepositoryRootFolder + @"\" + item.FilePath;
                            SCFI.SolutionPath = @"~\" + item.FilePath;
                            SCFI.Status = SourceControlFileInfo.eRepositoryItemStatus.Unknown;
                            SCFI.Selected = true;
                            SCFI.Diff = "";
                            if (item.State.ToString().Contains(FileStatus.ModifiedInWorkdir.ToString()))
                            {
                                SCFI.Status = SourceControlFileInfo.eRepositoryItemStatus.Modified;
                            }
                            if (item.State == FileStatus.ModifiedInIndex)
                            {
                                SCFI.Status = SourceControlFileInfo.eRepositoryItemStatus.ModifiedAndResolved;
                            }
                            if (item.State == FileStatus.NewInWorkdir)
                            {
                                SCFI.Status = SourceControlFileInfo.eRepositoryItemStatus.New;
                            }
                            if (item.State == FileStatus.DeletedFromWorkdir)
                            {
                                SCFI.Status = SourceControlFileInfo.eRepositoryItemStatus.Deleted;
                            }


                            if (SCFI.Status != SourceControlFileInfo.eRepositoryItemStatus.Unknown)
                            {
                                list.Add(SCFI);
                            }
                        }
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                error = ex.Message + Environment.NewLine + ex.InnerException;
                return list;
            }
        }

        public override bool GetProject(string Path, string URI, ref string error)
        {
            if (!System.IO.Directory.Exists(Path))
                System.IO.Directory.CreateDirectory(Path);
            try
            {
                var co = new CloneOptions();
                co.BranchName = string.IsNullOrEmpty(SourceControlBranch) ? "master" : SourceControlBranch;
                co.CredentialsProvider = GetSourceCredentialsHandler();
                RepositoryRootFolder = LibGit2Sharp.Repository.Clone(URI, Path, co);
            }
            catch (Exception ex)
            {
                error = ex.Message + Environment.NewLine + ex.InnerException;
                return false;
            }

            return true;
        }

        private void AddSolution(ObservableList<SolutionInfo> SourceControlSolutions, string LocalFolder, string SourceControlLocation)
        {
            SolutionInfo sol = new SolutionInfo();

            sol.LocalFolder = LocalFolder;
            if (System.IO.Directory.Exists(sol.LocalFolder))
                sol.ExistInLocaly = true;
            else
                sol.ExistInLocaly = false;
            sol.SourceControlLocation = SourceControlLocation;
            SourceControlSolutions.Add(sol);
        }

        public override ObservableList<SolutionInfo> GetProjectsList()
        {
            ObservableList<SolutionInfo> SourceControlSolutions = new ObservableList<SolutionInfo>();
            try
            {
                string repositoryName = SourceControlURL.Substring(SourceControlURL.LastIndexOf("/") + 1);
                if (repositoryName == string.Empty)
                {
                    string SourceControlURLExcludeSlash = SourceControlURL.Substring(0, SourceControlURL.LastIndexOf("/"));
                    repositoryName = SourceControlURLExcludeSlash.Substring(SourceControlURLExcludeSlash.LastIndexOf("/") + 1);
                }
                //check which path to show to download
                string localPath = SourceControlLocalFolder;
                if (IsImportSolution)
                {
                    localPath = SourceControlLocalFolderForGlobalSolution;
                }
                AddSolution(SourceControlSolutions, localPath + @"\" + repositoryName, repositoryName);
            }
            catch (Exception ex)
            { Console.WriteLine(ex.StackTrace); }
            return SourceControlSolutions;
        }

        public override void Init()
        {
            Console.WriteLine("GITHub - Init");
        }

        public override bool Lock(string path, string lockComment, ref string error)
        {
            throw new NotImplementedException();
        }

        private bool ResolveConflict(string path, eResolveConflictsSide side, ref string error)
        {
            try
            {
                if (System.IO.Path.GetExtension(path) != string.Empty && !System.IO.File.Exists(path.Replace(".xml", ".ignore")))
                {
                    if (!System.IO.File.Exists(path.Replace(".xml", ".conflictBackup")))
                    {
                        System.IO.File.Copy(path, path.Replace(".xml", ".conflictBackup"));
                    }
                }

                string firstCommonResultText = string.Empty;
                string middleResultText = string.Empty;
                string lastCommonResultText = string.Empty;
                string fileContent = string.Empty;
                if (side == eResolveConflictsSide.Local)
                {
                    fileContent = File.ReadAllText(path);

                    int startIndex = fileContent.IndexOf("<<<<<<< HEAD");
                    if (startIndex != 0)
                        firstCommonResultText = fileContent.Substring(0, startIndex);
                    int endIndex = fileContent.IndexOf("=======");
                    int RequestLeanth = (endIndex - startIndex);
                    middleResultText = fileContent.Substring(startIndex + 14, RequestLeanth - 14);

                    startIndex = fileContent.IndexOf(">>>>>>>");
                    lastCommonResultText = fileContent.Substring(startIndex);
                    startIndex = lastCommonResultText.IndexOf("\r\n");
                    lastCommonResultText = lastCommonResultText.Substring(startIndex + 2);

                    File.WriteAllText(path, firstCommonResultText + middleResultText + lastCommonResultText);

                }
                else
                {
                    fileContent = File.ReadAllText(path);
                    int startIndex = fileContent.IndexOf("<<<<<<< HEAD");
                    if (startIndex != 0)
                        firstCommonResultText = fileContent.Substring(0, startIndex);

                    startIndex = fileContent.IndexOf("=======");
                    int endIndex = fileContent.IndexOf(">>>>>>>");
                    int RequestLeanth = (endIndex - startIndex);
                    middleResultText = fileContent.Substring(startIndex + 9, RequestLeanth - 9);

                    startIndex = fileContent.IndexOf(">>>>>>>");
                    lastCommonResultText = fileContent.Substring(startIndex);
                    startIndex = lastCommonResultText.IndexOf("\r\n");
                    lastCommonResultText = lastCommonResultText.Substring(startIndex + 2);

                    File.WriteAllText(path, firstCommonResultText + middleResultText + lastCommonResultText);
                }

                if (File.ReadAllText(path).Contains("<<<<<<< HEAD"))
                    return ResolveConflict(path, side, ref error);
            }
            catch (Exception ex)
            {
                error = ex.Message + Environment.NewLine + ex.InnerException;
                return false;
            }
            return true;
        }

        public override bool ResolveConflicts(string path, eResolveConflictsSide side, ref string error)
        {
            try
            {
                string ConflictsPathsError = string.Empty;
                string ResolveConflictError = string.Empty;
                bool result = true;
                if (path == SolutionFolder)
                {
                    List<string> conflictPaths = GetConflictsPaths();
                    foreach (string cp in conflictPaths)
                    {
                        result = ResolveConflict(cp, side, ref ResolveConflictError);
                        if (!result)
                            error = error + ConflictsPathsError;
                        Stage(cp);
                    }
                }
                else
                {
                    result = ResolveConflict(path, side, ref ResolveConflictError);
                    if (!result)
                        error = error + ConflictsPathsError;
                    Stage(path);
                }
            }
            catch (Exception ex)
            {
                error = ex.Message + Environment.NewLine + ex.InnerException;
                return false;
            }
            return true;
        }

        public override bool Revert(string path, ref string error)
        {
            try
            {
                using (var repo = new LibGit2Sharp.Repository(RepositoryRootFolder))
                {
                    if (Path.GetFullPath(new Uri(RepositoryRootFolder).LocalPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToUpperInvariant() ==
                            Path.GetFullPath(new Uri(path).LocalPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToUpperInvariant())
                    {
                        //undo all changes
                        repo.Reset(ResetMode.Hard);
                    }
                    else
                    {
                        //undo specific changes
                        string committishOrBranchSpec = SourceControlBranch;
                        CheckoutOptions checkoutOptions = new CheckoutOptions();
                        checkoutOptions.CheckoutModifiers = CheckoutModifiers.Force;
                        checkoutOptions.CheckoutNotifyFlags = CheckoutNotifyFlags.Ignored;
                        repo.CheckoutPaths(committishOrBranchSpec, new[] { path }, checkoutOptions);
                    }
                }
            }
            catch (Exception e)
            {
                error = e.Message + Environment.NewLine + e.InnerException;
                return false;
            }
            return true;
        }

        public override bool TestConnection(ref string error)
        {
            Console.WriteLine("GITHub - TestConnection");
            try
            {
                if (IsRepositoryPublic())
                {
                    IEnumerable<LibGit2Sharp.Reference> References = LibGit2Sharp.Repository.ListRemoteReferences(SourceControlURL);
                }
                else
                {
                    if (SourceControlUser.Length != 0)
                    {
                        IEnumerable<LibGit2Sharp.Reference> References = LibGit2Sharp.Repository.ListRemoteReferences(SourceControlURL, GetSourceCredentialsHandler());
                    }
                    else
                    {
                        error = "Username cannot be empty";
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
            return true;
        }

        public override bool InitializeRepository(string remoteURL)
        {
            try
            {
                RepositoryRootFolder = WorkSpace.Instance.Solution.Folder;
                LibGit2Sharp.Repository.Init(RepositoryRootFolder);
                UploadSolutionToSourceControl(remoteURL);
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to Upload Solution to Source Control", ex);
                Reporter.ToUser(eUserMsgKey.UploadSolutionFailed, ex.Message);
                return false;
            }
        }

        private bool isRemoteBranchExist()
        {
            var remoteBranches = GetBranches();
            foreach (var branch in remoteBranches)
            {
                if (branch.Equals(SourceControlBranch))
                {
                    return true;
                }
            }
            return false;
        }

        private void UploadSolutionToSourceControl(string remoteURL)
        {
            using (var repo = new LibGit2Sharp.Repository(RepositoryRootFolder))
            {
                // Stage all items in working directory
                Commands.Stage(repo, "*");

                // Create the committer's signature and commit
                Signature author = new LibGit2Sharp.Signature(SourceControlUser, SourceControlUser, DateTime.Now);
                Signature committer = author;

                //Commit the staged items 
                repo.Commit("First commit using Ginger", author, committer);

                // Add the origin remote
                LibGit2Sharp.Remote remote = repo.Network.Remotes.Add("origin", remoteURL);

                PushOptions options = new PushOptions();
                options.CredentialsProvider = GetSourceCredentialsHandler();

                if (!String.IsNullOrEmpty(SourceControlBranch) && isRemoteBranchExist())
                {
                    PullOptions pullOptions = new PullOptions();

                    pullOptions.MergeOptions = new MergeOptions();
                    pullOptions.MergeOptions.FailOnConflict = true;

                    pullOptions.FetchOptions = new FetchOptions();
                    pullOptions.FetchOptions.CredentialsProvider = GetSourceCredentialsHandler();

                    Signature merger = author;

                    Branch localBranch = repo.Head;

                    if (!repo.Head.FriendlyName.Equals(SourceControlBranch))
                    {
                        repo.CreateBranch(SourceControlBranch);
                        localBranch = repo.Branches[SourceControlBranch];

                        repo.Branches.Update(localBranch, b => b.TrackedBranch = localBranch.CanonicalName);
                        Commands.Checkout(repo, SourceControlBranch);

                        repo.Branches.Update(localBranch,
                            b => b.Remote = remote.Name,
                            b => b.UpstreamBranch = localBranch.CanonicalName);
                    }
                    else
                    {
                        repo.Branches.Update(localBranch,
                            b => b.Remote = remote.Name,
                            b => b.UpstreamBranch = localBranch.CanonicalName);
                    }

                    Commands.Pull(repo, merger, pullOptions);
                }
                else
                {
                    repo.CreateBranch(SourceControlBranch);
                    Branch localBranch = repo.Branches[SourceControlBranch];

                    repo.Branches.Update(localBranch, b => b.TrackedBranch = localBranch.CanonicalName);
                    Commands.Checkout(repo, SourceControlBranch);

                    repo.Branches.Update(localBranch,
                        b => b.Remote = remote.Name,
                        b => b.UpstreamBranch = localBranch.CanonicalName);
                }
                repo.Network.Push(remote, @"refs/heads/" + SourceControlBranch, options);
            }
        }

        public override bool UnLock(string path, ref string error)
        {
            throw new NotImplementedException();
        }

        public override bool UpdateFile(string Path, ref string error)
        {
            return AddFile(Path, ref error);
        }

        public override void Disconnect()
        {
        }

        public override SourceControlItemInfoDetails GetInfo(string path, ref string error)
        {
            string relativePath = string.Empty;
            try
            {
                relativePath = System.IO.Path.GetFullPath(path);
                relativePath = relativePath.Substring(SolutionFolder.Count());

                SourceControlItemInfoDetails SCIID = new SourceControlItemInfoDetails();
                using (var repo = new LibGit2Sharp.Repository(RepositoryRootFolder))
                {
                    SCIID.ShowFileInfo = true;
                    SCIID.FilePath = System.IO.Path.GetFullPath(path);
                    SCIID.FileWorkingDirectory = repo.Info.WorkingDirectory;
                    SCIID.HasUnpushedCommits = " " + (repo.Head.TrackingDetails.AheadBy > 1).ToString();
                    SCIID.HasUncommittedChanges = "False";
                    SCIID.FileState = " " + FileStatus.Ignored.ToString();
                    foreach (var item in repo.RetrieveStatus())
                    {

                        if (Path.GetFullPath(path) == Path.GetFullPath(Path.Combine(RepositoryRootFolder, item.FilePath)))
                        {
                            if (item.State != FileStatus.Ignored)
                                SCIID.HasUncommittedChanges = "true";
                            SCIID.FileState = " " + item.State;
                        }
                    }

                    SCIID.ShowChangeInfo = true;
                    SCIID.LastChangeAuthor = " " + repo.Head.Tip.Author;
                    SCIID.LastChangeCommiter = " " + repo.Head.Tip.Committer;
                    //SCIID.LastChangeMessage = " " + repo.Head.Tip.Message;
                    //SCIID.LastChangeRevision = " " + repo.Refs.Head
                    SCIID.LastChangeRevision = " " + repo.Head.Tip.Sha;

                }
                return SCIID;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        public override string GetLockOwner(string path, ref string error)
        {
            return null;
        }

        public override SourceControlItemInfoDetails GetRepositoryInfo(ref string error)
        {
            try
            {
                SourceControlItemInfoDetails SCIID = new SourceControlItemInfoDetails();
                using (LibGit2Sharp.Repository repo = new LibGit2Sharp.Repository(RepositoryRootFolder))
                {
                    SCIID.ShowRepositoryInfo = true;
                    SCIID.RepositoryRoot = repo.Info.Path;
                    SCIID.RepositoryPath = repo.Info.WorkingDirectory;
                    SCIID.TrackedBranchName = repo.Head.TrackedBranch.ToString();
                    SCIID.BranchName = repo.Head.RemoteName;
                    SCIID.CanonicalName = repo.Head.CanonicalName;
                    SCIID.FriendlyName = repo.Head.FriendlyName;
                    repo.Commits.AsEnumerable();
                }
                return SCIID;
            }
            catch (Exception ex)
            {
                error = Environment.NewLine + ex.Message + Environment.NewLine + ex.InnerException;
                return null;
            }
        }

        private MergeResult Pull()
        {
            MergeResult mergeResult = null;
            //Pull = Fetch + Merge
            Task.Run(() =>
             {
                 using (var repo = new LibGit2Sharp.Repository(RepositoryRootFolder))
                 {

                     PullOptions PullOptions = new PullOptions();
                     PullOptions.FetchOptions = new FetchOptions();
                     PullOptions.FetchOptions.CredentialsProvider = GetSourceCredentialsHandler();
                     if (!IsRepositoryPublic())
                     {
                         mergeResult = Commands.Pull(repo, new Signature(SourceControlUser, SourceControlUser, new DateTimeOffset(DateTime.Now)), PullOptions);
                     }
                     else
                     {
                         mergeResult = Commands.Pull(repo, new Signature("dummy", "dummy", new DateTimeOffset(DateTime.Now)), PullOptions);
                     }
                 }
             }).Wait();
            return mergeResult;
        }

        private Commit Commit(string Comments)
        {
            CheckinComment = Comments;
            using (var repo = new LibGit2Sharp.Repository(RepositoryRootFolder))
            {
                // Create the committer's signature and commit
                Signature author = new LibGit2Sharp.Signature(SolutionSourceControlAuthorName, SolutionSourceControlAuthorEmail, DateTime.Now);
                Signature committer = author;
                // Commit to the repository
                return repo.Commit(Comments, author, committer);
            }
        }

        private void Stage(string Path)
        {
            using (var repo = new LibGit2Sharp.Repository(RepositoryRootFolder))
            {
                Commands.Stage(repo, Path);
            }
        }

        private void Push()
        {
            using (var repo = new LibGit2Sharp.Repository(RepositoryRootFolder))
            {
                PushOptions options = new PushOptions();
                options.OnPushStatusError += ErrorOnppush;
                options.CredentialsProvider = GetSourceCredentialsHandler();

                Branch currentBranch = repo.Branches.Where(x => x.FriendlyName == SourceControlBranch).FirstOrDefault();
                repo.Network.Push(currentBranch, options);
            }
        }

        private void ErrorOnppush(PushStatusError pushStatusErrors)
        {
            throw new Exception("Error Occurred in push" + pushStatusErrors.Message);
        }

        private List<string> GetConflictsPaths()
        {
            List<string> ConflictPaths = new List<string>();

            using (var repo = new LibGit2Sharp.Repository(RepositoryRootFolder))
            {
                foreach (var item in repo.RetrieveStatus())
                {
                    if (item.State == FileStatus.Conflicted)
                    {
                        string filePath = item.FilePath;
                        if (item.FilePath.Contains(@"/"))
                        {
                            filePath = filePath.Replace(@"/", @"\");
                        }
                        string fullPath = Path.Combine(RepositoryRootFolder, filePath);
                        ConflictPaths.Add(fullPath);
                    }
                }
            }
            return ConflictPaths;
        }

        public override bool CreateConfigFile(ref string error)
        {
            try
            {
                string UserFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)/*.Replace("AppData\\Local","")*/;
                string ConfigFileContent = string.Empty;
                string ConfigFilePath = Path.Combine(UserFolder, ".gitconfig");

                if (SourceControlConfigureProxy || IgnoreCertificate)
                {
                    if (File.Exists(ConfigFilePath))
                    {
                        ConfigFileContent = File.ReadAllText(ConfigFilePath);
                        if (!ConfigFileContent.Contains(SourceControlProxyAddress + ":" + SourceControlProxyPort))
                        {
                            ConfigFileContent += Environment.NewLine + "[http]";

                            if (SourceControlConfigureProxy)
                            {
                                ConfigFileContent += Environment.NewLine + "proxy =" + '\u0022' + SourceControlProxyAddress + ":" + SourceControlProxyPort + '\u0022';

                            }

                        }
                        if (IgnoreCertificate)
                        {
                            ConfigFileContent += Environment.NewLine + "sslVerify = false;";

                        }


                        File.WriteAllText(ConfigFilePath, ConfigFileContent);
                    }
                    else
                    {
                        ConfigFileContent = "[http]";

                        if (SourceControlConfigureProxy)
                        {
                            ConfigFileContent += Environment.NewLine + "proxy =" + '\u0022' + SourceControlProxyAddress + ":" + SourceControlProxyPort + '\u0022';

                        }
                        if (IgnoreCertificate)
                        {
                            ConfigFileContent += Environment.NewLine + "sslVerify = false;";

                        }
                        File.WriteAllText(ConfigFilePath, ConfigFileContent);
                    }
                }
                else
                {
                    if (File.Exists(ConfigFilePath))
                    {
                        ConfigFileContent = File.ReadAllText(ConfigFilePath);
                        string ConfigFilePortContent = "[http]" + Environment.NewLine + "proxy =" + '\u0022' + SourceControlProxyAddress + ":" + SourceControlProxyPort + '\u0022';
                        if (ConfigFileContent.Contains(ConfigFilePortContent))
                        {
                            ConfigFileContent = ConfigFileContent.Remove(ConfigFileContent.IndexOf(ConfigFilePortContent), ConfigFilePortContent.Length);
                        }
                        File.WriteAllText(ConfigFilePath, ConfigFileContent);
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex.Message + Environment.NewLine + ex.InnerException;
            }
            return true;
        }

        public override List<string> GetBranches()
        {
            try
            {
                return Repository.ListRemoteReferences(SourceControlURL, GetSourceCredentialsHandler())
                    .Where(elem => elem.CanonicalName.Contains("refs/heads/"))
                    .Select(elem => elem.CanonicalName.Replace("refs/heads/", ""))
                    .ToList();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("remote has never connected"))
                {
                    Reporter.ToUser(eUserMsgKey.SourceControlRemoteCannotBeAccessed, ex.Message);
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.SourceControlConnFaild, ex.Message);
                }
                return null;
            }
        }

        public override bool IsRepositoryPublic()
        {
            try
            {
                var branchesList = Repository.ListRemoteReferences(SourceControlURL, GetEmptySourceCredentialsHandler())
                    .Where(elem => elem.CanonicalName.Contains("refs/heads/"))
                    .Select(elem => elem.CanonicalName.Replace("refs/heads/", ""))
                    .ToList();
                if (branchesList == null || branchesList.Count == 0)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public override string GetCurrentBranchForSolution()
        {
            using (var repo = new LibGit2Sharp.Repository(RepositoryRootFolder))
            {
                return repo.Head.FriendlyName;
            }
        }

        private CredentialsHandler GetEmptySourceCredentialsHandler()
        {
            var credentials = new UsernamePasswordCredentials()
            {
                Username = String.Empty,
                Password = String.Empty,

            };
            CredentialsHandler credentialHandler = (_url, _user, _cred) => credentials;

            return credentialHandler;
        }

        private CredentialsHandler GetSourceCredentialsHandler()
        {
            var credentials = new UsernamePasswordCredentials()
            {
                Username = SourceControlUser,
                Password = SourceControlPass,

            };
            CredentialsHandler credentialHandler = (_url, _user, _cred) => credentials;

            return credentialHandler;
        }

    }
}
