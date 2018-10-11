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

namespace GingerCore.SourceControl
{
    [IsSerializedForLocalRepository]
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
            //Commit Changes
            bool result = true;
            try
            {
                Commit(Comments);
            }
            catch (Exception e)
            {
                error = e.Message + Environment.NewLine + e.InnerException;

            }
            finally
            {
                try
                {
                    Push();
                    Pull();
                    using (var repo = new LibGit2Sharp.Repository(RepositoryRootFolder))
                    {
                        Reporter.ToUser(eUserMsgKeys.CommitedToRevision, repo.Head.Tip.Sha);
                    }
                }
                catch (Exception e)
                {

                    error = error + Environment.NewLine + e.Message + Environment.NewLine + e.InnerException;
                    try
                    {
                        Pull();
                    }
                    catch { }

                    conflictsPaths = GetConflictsPaths();
                    Reporter.ToUser(eUserMsgKeys.SourceControlCommitFailed, "The files are not connected to source control");
                    result = false;
                }
            }
            return result;
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
                Pull();
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                try
                {
                    conflictPaths = GetConflictsPaths();
                }
                catch
                { }
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
                        if (item.FilePath.StartsWith(localFilePath) && item.FilePath != localFilePath)
                            return SourceControlFileInfo.eRepositoryItemStatus.Modified;

                        if (localFilePath == item.FilePath)
                        {
                            if (item.State == FileStatus.ModifiedInWorkdir || item.State == FileStatus.ModifiedInIndex)
                                return SourceControlFileInfo.eRepositoryItemStatus.Modified;
                            if (item.State == FileStatus.DeletedFromWorkdir)
                                return SourceControlFileInfo.eRepositoryItemStatus.Deleted;
                            if (item.State == FileStatus.Unaltered)
                                return SourceControlFileInfo.eRepositoryItemStatus.Equel;
                            if (item.State == FileStatus.NewInWorkdir)
                                return SourceControlFileInfo.eRepositoryItemStatus.New;
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
            try
            {
                MergeResult result;
                result = Pull();

                if (result.Status != MergeStatus.Conflicts)
                {
                    using (var repo = new LibGit2Sharp.Repository(RepositoryRootFolder))
                    {
                        if (supressMessage == true)

                            Reporter.ToLog(eAppReporterLogLevel.INFO, "The solution was updated successfully, Update status: " + result.Status + ", to Revision :"  + repo.Head.Tip.Sha);

                        else
                            Reporter.ToUser(eUserMsgKeys.GitUpdateState, result.Status, repo.Head.Tip.Sha);
                    }
                }
                else
                {
                    if (supressMessage == true)
                        Reporter.ToLog(eAppReporterLogLevel.INFO, "Failed to update the solution from source control.Error Details: 'The files are not connected to source control'");
                    else
                        Reporter.ToUser(eUserMsgKeys.SourceControlUpdateFailed, "The files are not connected to source control");
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

        public override ObservableList<SourceControlFileInfo> GetPathFilesStatus(string Path, ref string error, List<string> PathsToIgnore = null, bool includLockedFiles = false)
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

                        if (PathsToIgnore != null)
                        {
                            bool pathToIgnoreFound = false;
                            foreach (string pathToIgnore in PathsToIgnore)
                                if (System.IO.Path.GetFullPath(RepositoryRootFolder + @"\" + item.FilePath).Contains(System.IO.Path.GetFullPath(pathToIgnore)) ||
                                    item.FilePath.Contains(pathToIgnore))
                                {
                                    pathToIgnoreFound = true;
                                    break;
                                }
                            if (pathToIgnoreFound) continue;
                        }

                        if (System.IO.Path.GetExtension(item.FilePath) == ".ldb" || System.IO.Path.GetExtension(item.FilePath) == ".ignore")
                            continue;


                        if (relativePath == string.Empty || item.FilePath.StartsWith(relativePath))
                        {
                            SourceControlFileInfo SCFI = new SourceControlFileInfo();
                            SCFI.Path = RepositoryRootFolder + @"\" + item.FilePath;
                            SCFI.SolutionPath = @"~\" + item.FilePath;
                            SCFI.Status = SourceControlFileInfo.eRepositoryItemStatus.Unknown;
                            SCFI.Selected = true;
                            SCFI.Diff = "";
                            if (item.State == FileStatus.ModifiedInWorkdir)
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
                co.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = SourceControlUser, Password = SourceControlPass };
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
                AddSolution(SourceControlSolutions, SourceControlLocalFolder + @"\" + repositoryName, repositoryName);
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
                    System.IO.File.Copy(path, path.Replace(".xml",".conflictBackup"));
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
                    string committishOrBranchSpec = "master";
                    CheckoutOptions checkoutOptions = new CheckoutOptions();
                    checkoutOptions.CheckoutModifiers = CheckoutModifiers.Force;
                    checkoutOptions.CheckoutNotifyFlags = CheckoutNotifyFlags.Ignored;
                    repo.CheckoutPaths(committishOrBranchSpec, new[] { path }, checkoutOptions);
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
                var co = new CloneOptions();
                if (SourceControlUser.Length != 0)
                {
                    co.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = SourceControlUser, Password = SourceControlPass };
                    IEnumerable<LibGit2Sharp.Reference> References = LibGit2Sharp.Repository.ListRemoteReferences(SourceControlURL, co.CredentialsProvider);
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
            return true;
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

                        if (relativePath == item.FilePath)
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
            using (var repo = new LibGit2Sharp.Repository(RepositoryRootFolder))
            {
                PullOptions PullOptions = new PullOptions();
                PullOptions.FetchOptions = new FetchOptions();
                PullOptions.FetchOptions.CredentialsProvider = new CredentialsHandler(
                    (url, usernameFromUrl, types) => new UsernamePasswordCredentials() { Username = SourceControlUser, Password = SourceControlPass });
                //return repo.Network.Pull(new Signature(SourceControlUser, SourceControlUser, new DateTimeOffset(DateTime.Now)), PullOptions);
                MergeResult mergeResult = Commands.Pull(repo, new Signature(SourceControlUser, SourceControlUser, new DateTimeOffset(DateTime.Now)), PullOptions);
                return mergeResult;
            }
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
                Remote remote = repo.Network.Remotes["origin"];
                PushOptions options = new PushOptions();
                options.OnPushStatusError += ErrorOnppush;
                options.CredentialsProvider = (_url, _user, _cred) =>
                    new UsernamePasswordCredentials { Username = SourceControlUser, Password = SourceControlPass };

                repo.Network.Push(remote, @"refs/heads/master", options);
            }
        }

        private void ErrorOnppush(PushStatusError pushStatusErrors)
        {
            throw new Exception("Error Occured in push" + pushStatusErrors.Message);
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
                        ConflictPaths.Add(RepositoryRootFolder + @"\" + item.FilePath);
                    }
                }
            }
            return ConflictPaths;
        }

        public override bool CreateConfigFile(ref string error)
        {
            try
            {
                string UserFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)/*.Replace("AppData\\Local","")*/;
                string ConfigFileContent = string.Empty;
                string ConfigFilePath = UserFolder.Replace("AppData\\Local", "") + ".gitconfig";

                if (SourceControlConfigureProxy)
                {
                    if (File.Exists(ConfigFilePath))
                    {
                        ConfigFileContent = File.ReadAllText(ConfigFilePath);
                        if (!ConfigFileContent.Contains(SourceControlProxyAddress + ":" + SourceControlProxyPort))
                        {
                            ConfigFileContent += Environment.NewLine + "[http]" + Environment.NewLine + "proxy =" + '\u0022' + SourceControlProxyAddress + ":" + SourceControlProxyPort + '\u0022';
                            File.WriteAllText(ConfigFilePath, ConfigFileContent);
                        }
                    }
                    else
                    {
                        ConfigFileContent = "[http]" + Environment.NewLine + "proxy =" + '\u0022' + SourceControlProxyAddress + ":" + SourceControlProxyPort + '\u0022';
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
    }
}
