#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Amdocs.Ginger.Common.Telemetry;
using Amdocs.Ginger.Common.UIElement;
using GingerCoreNET.SourceControl;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GingerCore.SourceControl
{

    public class GITSourceControl : SourceControlBase
    {
        private const string XMLFileExtension = ".xml";
        private const string IgnoreFileExtension = ".ignore";
        private const string ConflictBackupFileExtension = ".conflictBackup";

        private DateTime _lastGitIgnoreCheckTimeUtc;

        public override string Name { get { return "GIT"; } }

        public override bool IsSupportingLocks { get { return false; } }

        public override bool IsSupportingGetLatestForIndividualFiles { get { return false; } }

        public override eSourceControlType GetSourceControlType { get { return eSourceControlType.GIT; } }

        public override List<string> GetSourceControlmConflict { get { return null; } }

        private string CheckinComment { get; set; }

        private string GitIgnoreFilePath => Path.Combine(RepositoryRootFolder, ".gitignore");

        public override bool AddFile(string Path, ref string error)
        {
            try
            {
                Stage(Path);
            }
            catch (Exception e)
            {
                error = HandleGitExceptions(e);
                return false;
            }
            return true;
        }

        public override void CleanUp(string path)
        {
        }

        public override bool CommitChanges(string Comments, ref string error)
        {
            bool result = false;
            try
            {
                result = Commit(Comments) != null;
            }
            catch (Exception e)
            {
                error = HandleGitExceptions(e);
            }
            return result;
        }

        public override bool CommitAndCheckinChanges(ICollection<string> Paths, string Comments, ref string error, ref List<string> conflictsPaths, bool includLockedFiles = false)
        {
            if (TestConnection(ref error))
            {
                //Commit Changes
                bool result = true;
                try
                {
                    if (Paths != null && Paths.Count > 0 && AnyLocalChangesPendingtoCommit())
                    {
                        Commit(Comments);
                    }
                }
                catch (Exception e)
                {
                    error = HandleGitExceptions(e);
                }
                finally
                {
                    try
                    {
                        Push();
                        Pull();
                        using var repo = new Repository(RepositoryRootFolder);
                        Reporter.ToUser(eUserMsgKey.SourceControlChkInSucss);
                    }
                    catch (Exception e)
                    {
                        error = HandleGitExceptions(e);

                        if (e.Message.Contains("403"))
                        {
                            Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Check-In Failed, please check if user has Write Permission for this repository");
                        }
                        else
                        {
                            Reporter.ToUser(eUserMsgKey.SourceControlCommitFailed, e.Message);
                        }
                        try
                        {
                            Pull();
                        }
                        catch { }

                        conflictsPaths = GetConflictPaths();
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

        public bool AnyLocalChangesPendingtoCommit()
        {
            using var repo = new Repository(RepositoryRootFolder);
            RepositoryStatus status = repo.RetrieveStatus();

            return status.IsDirty;
        }

        private List<string> GetConflictsPathsforGetLatestConflict(string path)
        {
            List<string> conflictPaths = [];
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
                    conflictPaths = GetConflictPaths();
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.WARN, ex.Message, ex);
                }
            }
            return conflictPaths;
        }

        public override bool DeleteFile(string path, ref string error)
        {
            try
            {
                using Repository repo = new(RepositoryRootFolder);
                path = Path.GetRelativePath(RepositoryRootFolder, path).Replace(@"\", @"/");
                Conflict conflict = repo.Index.Conflicts[path];
                if (conflict != null)
                {
                    Stage(path);
                }
                Commands.Remove(repo, path);
            }
            catch (Exception e)
            {
                error = HandleGitExceptions(e);
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
                int RepositoryFolderCount = RepositoryRootFolder.Length;
                string localFilePath = string.Empty;
                if (Path.Length > (RepositoryFolderCount))
                {
                    localFilePath = Path[RepositoryFolderCount..];
                }

                if (localFilePath.StartsWith("\\"))
                {
                    localFilePath = localFilePath[1..];
                }

                using (var repo = new LibGit2Sharp.Repository(RepositoryRootFolder))
                {
                    if (Path == SolutionFolder && repo.RetrieveStatus().Any())
                    {
                        return SourceControlFileInfo.eRepositoryItemStatus.Modified;
                    }

                    foreach (var item in repo.RetrieveStatus())
                    {
                        if (NormalizePath(item.FilePath).StartsWith(NormalizePath(localFilePath)) && item.State != FileStatus.NewInWorkdir)
                        {
                            return SourceControlFileInfo.eRepositoryItemStatus.Modified;
                        }

                        if (NormalizePath(localFilePath) == NormalizePath(item.FilePath))
                        {
                            return GetItemStatus(item.State);
                        }
                        else if (NormalizePath(item.FilePath).Contains(NormalizePath(localFilePath)) && !localFilePath.EndsWith(XMLFileExtension))
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
            if (state is FileStatus.ModifiedInWorkdir or FileStatus.ModifiedInIndex)
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
                error = HandleGitExceptions(e);
            }
            return remoteURL;
        }

        public override bool GetLatest(string path, ref string error, ref List<string> conflictsPaths, ProgressNotifier progressNotifier = null)
        {
            if (!TestConnection(ref error))
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Unable to connect to repository.");
                return false;
            }

            try
            {
                MergeResult result;
                result = Pull(progressNotifier);

                if (result.Status != MergeStatus.Conflicts)
                {
                    using var repo = new Repository(RepositoryRootFolder);
                    if (supressMessage == true)
                    {
                        Reporter.ToLog(eLogLevel.INFO, $"The solution was updated successfully, Update status: {result.Status}, to Revision :{repo.Head.Tip.Sha}");
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKey.GitUpdateState, result.Status, repo.Head.Tip.Sha);
                    }
                    return true;
                }
                else
                {
                    conflictsPaths = GetConflictPaths();

                    if (supressMessage == true)
                    {
                        Reporter.ToLog(eLogLevel.INFO, "Merge Conflict occurred while getting latest changes.");
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKey.SourceControlUpdateFailed, "Merge Conflict occurred while getting latest changes.");
                    }
                    return false;
                }

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while getting latest changes.", ex);
                if (ex is AggregateException && ex.InnerException is CheckoutConflictException)
                {
                    error = Reporter.UserMsgsPool[eUserMsgKey.UncommitedChangesPreventCheckout].Message;
                    return false;
                }
                conflictsPaths = GetConflictPaths();
                error = $"{ex.Message} {Environment.NewLine} {ex.InnerException}";
                return false;
            }
        }

        public override ObservableList<SourceControlFileInfo> GetPathFilesStatus(string Path, ref string error, bool includLockedFiles = false)
        {
            Console.WriteLine("GITHub - GetPathFilesStatus");
            ObservableList<SourceControlFileInfo> list = [];

            try
            {
                string relativePath = System.IO.Path.GetFullPath(Path);
                relativePath = relativePath[RepositoryRootFolder.Length..];

                if (relativePath.StartsWith(@"\"))
                {
                    relativePath = relativePath[1..];
                }

                if (File.Exists(GitIgnoreFilePath))
                {
                    UpdateGitIgnoreFile();
                }
                else
                {
                    CreateGitIgnoreFile();
                }

                using (var repo = new LibGit2Sharp.Repository(RepositoryRootFolder))
                {
                    foreach (var item in repo.RetrieveStatus())
                    {
                        if (WorkSpace.Instance.SolutionRepository.IsSolutionPathToAvoid(System.IO.Path.Combine(RepositoryRootFolder, item.FilePath)))
                        {
                            continue;
                        }

                        //if (System.IO.Path.GetExtension(item.FilePath) == ".ldb" || System.IO.Path.GetExtension(item.FilePath) == ".ignore" || System.IO.Path.GetExtension(item.FilePath) == ".db")
                        //{
                        //    continue;
                        //}


                        //sometimes remote file path uses / otherwise \  our code should be path independent 
                        if (relativePath == string.Empty || System.IO.Path.GetFullPath(System.IO.Path.Combine(RepositoryRootFolder, item.FilePath)).StartsWith(System.IO.Path.GetFullPath(Path)))
                        {
                            SourceControlFileInfo SCFI = new SourceControlFileInfo
                            {
                                Path = RepositoryRootFolder + @"\" + item.FilePath,
                                SolutionPath = @"~\" + item.FilePath,
                                Status = SourceControlFileInfo.eRepositoryItemStatus.Unknown,
                                Selected = true,
                                Diff = ""
                            };
                            if (item.State.ToString().Contains(FileStatus.ModifiedInWorkdir.ToString()))
                            {
                                SCFI.Status = SourceControlFileInfo.eRepositoryItemStatus.Modified;
                            }
                            if (item.State == FileStatus.ModifiedInIndex)
                            {
                                SCFI.Status = SourceControlFileInfo.eRepositoryItemStatus.ModifiedAndResolved;
                            }
                            if (item.State is FileStatus.NewInWorkdir or FileStatus.NewInIndex)
                            {
                                SCFI.Status = SourceControlFileInfo.eRepositoryItemStatus.New;
                            }
                            if (item.State is FileStatus.DeletedFromWorkdir or FileStatus.DeletedFromIndex)
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
            {
                System.IO.Directory.CreateDirectory(Path);
            }

            try

            {                
                var co = new CloneOptions(GetFetchOptions())
                {
                    BranchName = string.IsNullOrEmpty(SourceControlBranch) ? "master" : SourceControlBranch,
                };
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
            SolutionInfo sol = new SolutionInfo
            {
                LocalFolder = LocalFolder
            };
            if (System.IO.Directory.Exists(sol.LocalFolder))
            {
                sol.ExistInLocaly = true;
            }
            else
            {
                sol.ExistInLocaly = false;
            }

            sol.SourceControlLocation = SourceControlLocation;
            SourceControlSolutions.Add(sol);
        }

        public override ObservableList<SolutionInfo> GetProjectsList()
        {
            ObservableList<SolutionInfo> SourceControlSolutions = [];
            try
            {
                string repositoryName = SourceControlURL[(SourceControlURL.LastIndexOf("/") + 1)..];
                if (repositoryName == string.Empty)
                {
                    string SourceControlURLExcludeSlash = SourceControlURL[..SourceControlURL.LastIndexOf("/")];
                    repositoryName = SourceControlURLExcludeSlash[(SourceControlURLExcludeSlash.LastIndexOf("/") + 1)..];
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

        public void CreateGitIgnoreFile()
        {
            try
            {
                if (File.Exists(GitIgnoreFilePath))
                {
                    File.Delete(GitIgnoreFilePath);
                }

                string gitIgnoreFileContent = WorkSpace.Instance.SolutionRepository
                    .GetRelativePathsToAvoidFromSourceControl()
                    .Select(path => path.Replace(oldValue: @"\", newValue: @"/"))
                    .Aggregate((aggContent, path) => $"{aggContent}\n{path}");
                File.WriteAllText(GitIgnoreFilePath, gitIgnoreFileContent);
                string errorWhileAddingFile = string.Empty;
                AddFile(GitIgnoreFilePath, ref errorWhileAddingFile);
                if (!string.IsNullOrEmpty(errorWhileAddingFile))
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Error occurred while adding .gitignore file for source control tracking.\n{errorWhileAddingFile}");
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while creating .gitignore file.", ex);
            }
        }

        public void UpdateGitIgnoreFile()
        {
            if (!File.Exists(GitIgnoreFilePath))
            {
                throw new InvalidOperationException($"No git ignore file found at path '{GitIgnoreFilePath}'.");
            }

            DateTime lastWriteTime = File.GetLastWriteTimeUtc(GitIgnoreFilePath);
            if (lastWriteTime <= _lastGitIgnoreCheckTimeUtc)
            {
                //gitignore file wasn't modified until last checked
                return;
            }

            string[] gitIgnorePaths = File.ReadAllText(GitIgnoreFilePath)
                .Split('\n')
                .ToArray();

            string[] solutionIgnoredPaths = WorkSpace.Instance.SolutionRepository
                    .GetRelativePathsToAvoidFromSourceControl()
                    .Select(path => path.Replace(oldValue: @"\", newValue: @"/"))
                    .ToArray();

            List<string> missingPaths = [];
            foreach (string solutionIgnoredPath in solutionIgnoredPaths)
            {
                if (!gitIgnorePaths.Contains(solutionIgnoredPath))
                {
                    missingPaths.Add(solutionIgnoredPath);
                }
            }

            if (missingPaths.Count == 0)
            {
                _lastGitIgnoreCheckTimeUtc = DateTime.UtcNow;
                return;
            }

            string gitIgnoreFileContent = gitIgnorePaths
                    .Concat(missingPaths)
                    .Select(path => path.Replace(oldValue: @"\", newValue: @"/"))
                    .Aggregate((aggContent, path) => $"{aggContent}\n{path}");

            try
            {
                File.WriteAllText(GitIgnoreFilePath, gitIgnoreFileContent);
                _lastGitIgnoreCheckTimeUtc = File.GetLastWriteTimeUtc(GitIgnoreFilePath);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error occurred while updating git ignore file at path '{GitIgnoreFilePath}'.", ex);
            }
        }

        public override bool Lock(string path, string lockComment, ref string error)
        {
            throw new NotImplementedException();
        }

        private bool ResolveConflict(string path, eResolveConflictsSide side, ref string error)
        {
            try
            {
                string extension = Path.GetExtension(path);

                //if it is not an XML file then, take the local/remote BLOB file and save it.
                //if it is an XML file then, read the file content and take the local/remote content within the conflict markers.
                bool isNotXMLFile = !string.Equals(extension, XMLFileExtension);
                if (isNotXMLFile)
                {
                    try
                    {
                        using Repository repo = new(RepositoryRootFolder);
                        using FileStream fileStream = new(path, FileMode.Create);
                        if (side == eResolveConflictsSide.Local)
                        {
                            Blob oursBlob = GetLocalBlobForConflict(repo, path);
                            oursBlob.GetContentStream().CopyTo(fileStream);
                        }
                        else
                        {
                            Blob theirsBlob = GetRemoteBlobForConflict(repo, path);
                            theirsBlob.GetContentStream().CopyTo(fileStream);
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message + Environment.NewLine + ex.InnerException;
                        return false;
                    }
                }

                if (!string.IsNullOrEmpty(extension) && !File.Exists(path.Replace(XMLFileExtension, IgnoreFileExtension)))
                {
                    if (!File.Exists(path.Replace(XMLFileExtension, ConflictBackupFileExtension)))
                    {
                        File.Copy(path, path.Replace(XMLFileExtension, ConflictBackupFileExtension));
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
                    {
                        firstCommonResultText = fileContent[..startIndex];
                    }

                    int endIndex = fileContent.IndexOf("=======");
                    int RequestLeanth = (endIndex - startIndex);
                    middleResultText = fileContent.Substring(startIndex + 14, RequestLeanth - 14);

                    startIndex = fileContent.IndexOf(">>>>>>>");
                    lastCommonResultText = fileContent[startIndex..];
                    startIndex = lastCommonResultText.IndexOf("\r\n");
                    lastCommonResultText = lastCommonResultText[(startIndex + 2)..];

                    File.WriteAllText(path, firstCommonResultText + middleResultText + lastCommonResultText);

                }
                else
                {
                    fileContent = File.ReadAllText(path);
                    int startIndex = fileContent.IndexOf("<<<<<<< HEAD");
                    if (startIndex != 0)
                    {
                        firstCommonResultText = fileContent[..startIndex];
                    }

                    startIndex = fileContent.IndexOf("=======");
                    int endIndex = fileContent.IndexOf(">>>>>>>");
                    int RequestLeanth = (endIndex - startIndex);
                    middleResultText = fileContent.Substring(startIndex + 9, RequestLeanth - 9);

                    startIndex = fileContent.IndexOf(">>>>>>>");
                    lastCommonResultText = fileContent[startIndex..];
                    startIndex = lastCommonResultText.IndexOf("\r\n");
                    lastCommonResultText = lastCommonResultText[(startIndex + 2)..];

                    File.WriteAllText(path, firstCommonResultText + middleResultText + lastCommonResultText);
                }

                if (File.ReadAllText(path).Contains("<<<<<<< HEAD"))
                {
                    return ResolveConflict(path, side, ref error);
                }
            }
            catch (Exception ex)
            {
                error = ex.Message + Environment.NewLine + ex.InnerException;
                return false;
            }
            return true;
        }

        public override string GetLocalContentForConflict(string conflictFilePath)
        {
            using Repository repo = new(RepositoryRootFolder);
            Blob oursBlob = GetLocalBlobForConflict(repo, conflictFilePath);
            if (oursBlob == null)
            {
                return string.Empty;
            }
            return oursBlob.GetContentText();
        }

        private Blob GetLocalBlobForConflict(Repository repo, string conflictFilePath)
        {
            conflictFilePath = Path.GetRelativePath(RepositoryRootFolder, conflictFilePath);
            conflictFilePath = conflictFilePath.Replace(@"\", @"/");
            Conflict conflict = repo.Index.Conflicts[conflictFilePath]; //does this return null if no conflict for given path?
            if (conflict.Ours == null)
            {
                return null;
            }

            Blob oursBlob = (Blob)repo.Lookup(conflict.Ours.Id);
            return oursBlob;
        }

        public override string GetRemoteContentForConflict(string conflictFilePath)
        {
            using Repository repo = new(RepositoryRootFolder);
            Blob theirsBlob = GetRemoteBlobForConflict(repo, conflictFilePath);
            if (theirsBlob == null)
            {
                return string.Empty;
            }
            return theirsBlob.GetContentText();
        }

        private Blob GetRemoteBlobForConflict(Repository repo, string conflictFilePath)
        {
            conflictFilePath = Path.GetRelativePath(RepositoryRootFolder, conflictFilePath);
            conflictFilePath = conflictFilePath.Replace(@"\", @"/");
            Conflict conflict = repo.Index.Conflicts[conflictFilePath]; //does this return null if no conflict for given path?
            if (conflict.Theirs == null)
            {
                return null;
            }

            Blob theirsBlob = (Blob)repo.Lookup(conflict.Theirs.Id);
            return theirsBlob;
        }

        private const string ConflictStartMarker = "<<<<<<<";
        private const string ConflictPartitionMarker = "=======";
        private const string ConflictEndMarker = ">>>>>>>";
        private const string CR_LF = "\r\n";

        private string GetLocalContentFromConflictedContent(string conflictedContent)
        {
            if (!conflictedContent.Contains(ConflictStartMarker))
            {
                return conflictedContent;
            }

            string leadingContent = GetLeadingContentFromConflicted(conflictedContent);
            string headContent = GetHeadContentFromConflicted(conflictedContent);
            string trailingContent = GetTrailingContentFromConflicted(conflictedContent);

            string localContent = leadingContent + headContent + trailingContent;
            localContent = GetLocalContentFromConflictedContent(localContent);

            return localContent;
        }

        private string GetRemoteContentFromConflictedContent(string conflictedContent)
        {
            if (!conflictedContent.Contains(ConflictStartMarker))
            {
                return conflictedContent;
            }

            string leadingContent = GetLeadingContentFromConflicted(conflictedContent);
            string branchContent = GetBranchContentFromConflicted(conflictedContent);
            string trailingContent = GetTrailingContentFromConflicted(conflictedContent);

            string remoteContent = leadingContent + branchContent + trailingContent;
            remoteContent = GetRemoteContentFromConflictedContent(remoteContent);

            return remoteContent;
        }

        /// <summary>
        /// Get the content leading the first <see cref="ConflictStartMarker"/>. 
        /// <example>
        /// <code>
        /// //For example, for below text,
        /// 
        /// If you have questions, please
        /// &lt;&lt;&lt;&lt;&lt;&lt;&lt; HEAD
        /// open an issue
        /// =======
        /// ask your question in IRC
        /// &gt;&gt;&gt;&gt;&gt;&gt;&gt; branch-a
        /// thank you.
        /// 
        /// //it will return,
        /// 
        /// If you have questions, please
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="conflictedContent">Content with conflicting data.</param>
        /// <returns>Content leading the first <see cref="ConflictStartMarker"/>.</returns>
        internal string GetLeadingContentFromConflicted(string conflictedContent)
        {
            string leadingContent = string.Empty;
            int startMarkerIndex = conflictedContent.IndexOf(ConflictStartMarker);
            int leadingContentLength = startMarkerIndex;
            if (leadingContentLength >= 0)
            {
                leadingContent = conflictedContent[..leadingContentLength];
            }

            return leadingContent;
        }

        /// <summary>
        /// Get the content between the first <see cref="ConflictStartMarker"/> and <see cref="ConflictPartitionMarker"/>.
        /// <example>
        /// <code>
        /// //For example, for below text,
        /// 
        /// If you have questions, please
        /// &lt;&lt;&lt;&lt;&lt;&lt;&lt; HEAD
        /// open an issue
        /// =======
        /// ask your question in IRC
        /// &gt;&gt;&gt;&gt;&gt;&gt;&gt; branch-a
        /// thank you.
        /// 
        /// //it will return,
        /// 
        /// open an issue
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="conflictedContent">Content with conflicting data.</param>
        /// <returns>Content between the first <see cref="ConflictStartMarker"/> and <see cref="ConflictPartitionMarker"/>.</returns>
        internal string GetHeadContentFromConflicted(string conflictedContent)
        {
            int startMarkerIndex = conflictedContent.IndexOf(ConflictStartMarker);
            int startMarkerCRLFIndex = conflictedContent.IndexOf(CR_LF, startMarkerIndex);
            int partitionMarkerIndex = conflictedContent.IndexOf(ConflictPartitionMarker);
            int headContentLength = partitionMarkerIndex - (startMarkerCRLFIndex + CR_LF.Length);
            int headContentStartIndex = startMarkerCRLFIndex + CR_LF.Length;
            string headContent = conflictedContent.Substring(headContentStartIndex, headContentLength);
            return headContent;
        }

        /// <summary>
        /// Get the content between the first <see cref="ConflictPartitionMarker"/> and <see cref="ConflictEndMarker"/>.
        /// <example>
        /// <code>
        /// //For example, for below text,
        /// 
        /// If you have questions, please
        /// &lt;&lt;&lt;&lt;&lt;&lt;&lt; HEAD
        /// open an issue
        /// =======
        /// ask your question in IRC
        /// &gt;&gt;&gt;&gt;&gt;&gt;&gt; branch-a
        /// thank you.
        /// 
        /// //it will return,
        /// 
        /// ask your question in IRC
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="conflictedContent">Content with conflicting data.</param>
        /// <returns>Content between the first <see cref="ConflictPartitionMarker"/> and <see cref="ConflictEndMarker"/>.</returns>
        internal string GetBranchContentFromConflicted(string conflictedContent)
        {
            int partitionMarkerIndex = conflictedContent.IndexOf(ConflictPartitionMarker);
            int endMarkerIndex = conflictedContent.IndexOf(ConflictEndMarker);
            int branchContentLength = endMarkerIndex - (partitionMarkerIndex + ConflictPartitionMarker.Length + CR_LF.Length);
            int branchContentStartIndex = partitionMarkerIndex + ConflictPartitionMarker.Length + CR_LF.Length;
            string branchContent = conflictedContent.Substring(branchContentStartIndex, branchContentLength);
            return branchContent;
        }

        /// <summary>
        /// Get the content after the first <see cref="ConflictEndMarker"/>. 
        /// <example>
        /// <code>
        /// //For example, for below text,
        /// 
        /// If you have questions, please
        /// &lt;&lt;&lt;&lt;&lt;&lt;&lt; HEAD
        /// open an issue
        /// =======
        /// ask your question in IRC
        /// &gt;&gt;&gt;&gt;&gt;&gt;&gt; branch-a
        /// thank you.
        /// 
        /// //it will return,
        /// 
        /// thank you.
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="conflictedContent">Content with conflicting data.</param>
        /// <returns>Content after the first <see cref="ConflictEndMarker"/>.</returns>
        internal string GetTrailingContentFromConflicted(string conflictedContent)
        {
            int endMarkerIndex = conflictedContent.IndexOf(ConflictEndMarker);
            int endMarkerCRLFIndex = conflictedContent.IndexOf(CR_LF, endMarkerIndex);
            int trailingContentStartIndex = endMarkerCRLFIndex + CR_LF.Length;
            string trailingContent = conflictedContent[trailingContentStartIndex..];
            return trailingContent;
        }

        public override bool ResolveConflictWithContent(string path, string resolvedContent, ref string error)
        {
            bool wasConflictResolved;
            bool wasBackupCreated = false;
            try
            {
                error = string.Empty;

                if (NeedToCreateBackup(path))
                {
                    wasBackupCreated = true;
                    CreateBackup(path);
                }
                File.WriteAllText(path, resolvedContent);
                Stage(path);
                wasConflictResolved = true;
            }
            catch (Exception ex)
            {
                error = ex.ToString();
                wasConflictResolved = false;
            }
            if (wasConflictResolved && wasBackupCreated)
            {
                RemoveBackup(path);
            }
            return wasConflictResolved;
        }

        private string GetBackupFilePath(string path)
        {
            return path.Replace(XMLFileExtension, ConflictBackupFileExtension);
        }

        private string GetIgnoreFilePath(string path)
        {
            return path.Replace(XMLFileExtension, IgnoreFileExtension);
        }

        private void CreateBackup(string path)
        {
            string backupPath = GetBackupFilePath(path);
            File.Copy(path, backupPath);
        }

        private void RemoveBackup(string path)
        {
            string backupPath = GetBackupFilePath(path);
            File.Delete(backupPath);
        }

        private bool NeedToCreateBackup(string path)
        {
            bool hasExtension = Path.GetExtension(path) != string.Empty;

            string ignoreFilePath = GetIgnoreFilePath(path);
            bool ignoreFileExists = File.Exists(ignoreFilePath);

            string backupFilePath = GetBackupFilePath(path);
            bool backupFileExists = File.Exists(backupFilePath);

            return hasExtension && !ignoreFileExists && !backupFileExists;
        }

        public bool ResolveConflictsForSolution(eResolveConflictsSide side, ref string error)
        {
            try
            {
                error = string.Empty;
                string ConflictsPathsError = string.Empty;
                string ResolveConflictError = string.Empty;
                bool result = true;

                List<string> conflictPaths = GetConflictPaths();
                foreach (string conflictPath in conflictPaths)
                {
                    result = ResolveConflict(conflictPath, side, ref ResolveConflictError);
                    if (!result)
                    {
                        error = error + ConflictsPathsError;
                    }

                    Stage(conflictPath);
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message + Environment.NewLine + ex.InnerException;
                return false;
            }
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
                    List<string> conflictPaths = GetConflictPaths();
                    foreach (string cp in conflictPaths)
                    {
                        result = ResolveConflict(cp, side, ref ResolveConflictError);
                        if (!result)
                        {
                            error = error + ConflictsPathsError;
                        }

                        Stage(cp);
                    }
                }
                else
                {
                    result = ResolveConflict(path, side, ref ResolveConflictError);
                    if (!result)
                    {
                        error = error + ConflictsPathsError;
                    }

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
                        CheckoutOptions checkoutOptions = new CheckoutOptions
                        {
                            CheckoutModifiers = CheckoutModifiers.Force,
                            CheckoutNotifyFlags = CheckoutNotifyFlags.Ignored
                        };
                        repo.CheckoutPaths(committishOrBranchSpec, new[] { path }, checkoutOptions);
                    }
                }
            }
            catch (Exception e)
            {
                error = HandleGitExceptions(e);
                return false;
            }
            return true;
        }

        private static string HandleGitExceptions(Exception e)
        {
            string error = e.Message + Environment.NewLine + e.InnerException;
            Reporter.ToLog(eLogLevel.ERROR, error, e);
            return error;
        }

        public override bool TestConnection(ref string error)
        {
            bool wasConnectedSuccessfully = true;
            try
            {
                if (IsRepositoryPublic())
                {
                    IEnumerable<LibGit2Sharp.Reference> References = LibGit2Sharp.Repository.ListRemoteReferences(SourceControlURL);
                }
                else
                {
                    if (SourceControlUser != null && SourceControlUser.Length != 0)
                    {
                        CredentialsHandler credentialsHandler = GetSourceCredentialsHandler();
                        IEnumerable<LibGit2Sharp.Reference> References = LibGit2Sharp.Repository.ListRemoteReferences(SourceControlURL, credentialsHandler);
                    }
                    else
                    {
                        error = "Username cannot be empty";
                        wasConnectedSuccessfully = false;
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                wasConnectedSuccessfully = false;
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while testing connection.", ex);
            }

            return wasConnectedSuccessfully;
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

                PushOptions options = new PushOptions
                {
                    CredentialsProvider = GetSourceCredentialsHandler()
                };

                if (!String.IsNullOrEmpty(SourceControlBranch) && isRemoteBranchExist())
                {
                    var pullOptions = GetPullOptions();

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
        /// <summary>
        /// Gets the options for pulling changes from the remote repository.
        /// </summary>
        /// <returns>A PullOptions object configured with merge and fetch options.</returns>
        private PullOptions GetPullOptions()
        {
            return new PullOptions
            {
                MergeOptions = new MergeOptions
                {
                    FailOnConflict = true,
                },

                FetchOptions = GetFetchOptions()
            };
        }

        /// <summary>
        /// Gets the options for fetching changes from the remote repository.
        /// </summary>
        /// <returns>A FetchOptions object configured with credentials, depth, and certificate check.</returns>
        private FetchOptions GetFetchOptions()
        {
            return new FetchOptions
            {
                CredentialsProvider = GetSourceCredentialsHandler(),
                Depth = 1,
                CertificateCheck = (certificate, valid, host) => true,
            };
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
                relativePath = relativePath[SolutionFolder.Length..];

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
                            {
                                SCIID.HasUncommittedChanges = "true";
                            }

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

        /// <summary>
        /// Clones a Git repository to the specified path with progress notifications.
        /// </summary>
        /// <param name="path">The local path where the repository will be cloned.</param>
        /// <param name="uri">The URI of the remote repository.</param>
        /// <param name="error">Output parameter to capture any error messages.</param>
        /// <param name="progressNotifier">Optional progress notifier for reporting progress.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>True if the operation succeeds, otherwise false.</returns>
        public override bool GetProjectWithProgress(string path, string uri, ref string error, ProgressNotifier progressNotifier = null, CancellationToken cancellationToken = default)
        {
            try
            {
                EnsureDirectoryExists(path);
                var fetchOptions = GetFetchOptionsWithProgress(progressNotifier, cancellationToken);

                var cloneOptions = new CloneOptions(fetchOptions)
                {
                    BranchName = string.IsNullOrEmpty(SourceControlBranch) ? "master" : SourceControlBranch,
                    OnCheckoutProgress = (path, completedSteps, totalSteps) =>
                    {
                        progressNotifier?.NotifyProgressDetailText($"Checkout solution status: {completedSteps}/{totalSteps}");
                        progressNotifier?.NotifyProgressUpdated("Checkout solution status: ", completedSteps, totalSteps);
                    }
                };

                RepositoryRootFolder = LibGit2Sharp.Repository.Clone(uri, path, cloneOptions);
            }
            catch (Exception ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    error = $"{ex.Message}{Environment.NewLine}{ex.InnerException}";
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// Ensures that the specified directory exists, creating it if necessary.
        /// </summary>
        /// <param name="path">The path of the directory to check or create.</param>
        private void EnsureDirectoryExists(string path)
        {
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Gets fetch options with progress notifications and cancellation support.
        /// </summary>
        /// <param name="progressNotifier">Optional progress notifier for reporting progress.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>Fetch options configured with progress notifications and cancellation support.</returns>
        private FetchOptions GetFetchOptionsWithProgress(ProgressNotifier progressNotifier, CancellationToken cancellationToken)
        {
            var fetchOptions = GetFetchOptions();
            if (progressNotifier != null)
            {
                fetchOptions.OnProgress = progress =>
                {
                    progressNotifier.NotifyProgressDetailText($"{progress}");
                    return !cancellationToken.IsCancellationRequested;
                };

                fetchOptions.OnTransferProgress = progress =>
                {
                    if (progress.TotalObjects == 0)
                    {
                        progressNotifier.NotifyProgressDetailText("Initializing...");
                        return true;
                    }
                    double percentage = (double)progress.ReceivedObjects / progress.TotalObjects * 100;
                    progressNotifier.NotifyProgressDetailText($"{percentage:F2}% {progress.ReceivedObjects}/{progress.TotalObjects} files downloaded.");
                    progressNotifier.NotifyProgressUpdated("Download solution status: ", progress.ReceivedObjects, progress.TotalObjects);
                    return !cancellationToken.IsCancellationRequested;
                };
            }
            return fetchOptions;
        }

        /// <summary>
        /// Pulls the latest changes from the remote repository with progress notifications and cancellation support.
        /// </summary>
        /// <param name="progressNotifier">Optional progress notifier for reporting progress.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>The result of the merge operation.</returns>
        private MergeResult Pull(ProgressNotifier progressNotifier = null, CancellationToken cancellationToken = default)
        {
            MergeResult mergeResult = null;
            ReportFeatureUsage(operation: "Pull");

            Task.Run(() =>
            {
                using (var repo = new LibGit2Sharp.Repository(RepositoryRootFolder))
                {
                    var pullOptions = new PullOptions
                    {
                        FetchOptions = GetFetchOptionsWithProgress(progressNotifier, cancellationToken)
                    };

                    var signature = new Signature(
                        IsRepositoryPublic() ? "dummy" : SourceControlUser,
                        IsRepositoryPublic() ? "dummy" : SourceControlUser,
                        DateTimeOffset.Now
                    );

                    mergeResult = Commands.Pull(repo, signature, pullOptions);
                }
            }).Wait();
            return mergeResult;
        }

        private Commit Commit(string Comments)
        {
            ReportFeatureUsage(operation: "Commit");
            CheckinComment = Comments;
            using (var repo = new LibGit2Sharp.Repository(RepositoryRootFolder))
            {
                Signature author = new LibGit2Sharp.Signature(SolutionSourceControlAuthorName, SolutionSourceControlAuthorEmail, DateTime.Now);
                Signature committer = author;
                return repo.Commit(Comments, author, committer);
            }
        }

        public override ObservableList<SourceControlChangesetDetails> GetUnpushedLocalCommits()
        {
            try
            {
                using (var repo = new Repository(RepositoryRootFolder))
                {
                    var localBranch = repo.Branches[SourceControlBranch];
                    var trackingBranch = localBranch.TrackedBranch;

                    var filter = new CommitFilter
                    {
                        IncludeReachableFrom = localBranch.Tip,
                        ExcludeReachableFrom = trackingBranch.Tip
                    };

                    var unpushedCommits = repo.Commits.QueryBy(filter).ToList();
                    ObservableList<SourceControlChangesetDetails> commits = [];
                    foreach (var commit in unpushedCommits)
                    {
                        commits.Add(new SourceControlChangesetDetails() { Author = commit.Committer.Name, Date = commit.Committer.When, ID = commit.Id.Sha, Message = commit.MessageShort });
                    }

                    return commits;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to unpushed local commit", ex);
                return []; ;
            }

        }


        public override bool UndoUncommitedChanges(List<SourceControlFileInfo> selectedFiles)
        {
            try
            {
                using (var repo = new Repository(RepositoryRootFolder))
                {
                    List<string> filesPathsToUndo = [];
                    foreach (var file in selectedFiles)
                    {
                        if (file.Status == SourceControlFileInfo.eRepositoryItemStatus.New)
                        {
                            if (File.Exists(file.Path))
                            {
                                File.Delete(file.Path);
                            }
                        }
                        else if (file.Status is SourceControlFileInfo.eRepositoryItemStatus.Modified or
                                 SourceControlFileInfo.eRepositoryItemStatus.ModifiedAndResolved or
                                 SourceControlFileInfo.eRepositoryItemStatus.Deleted)
                        {
                            filesPathsToUndo.Add(file.Path);
                        }
                    }
                    if (filesPathsToUndo.Count > 0)
                    {
                        CheckoutOptions options = new CheckoutOptions() { CheckoutModifiers = CheckoutModifiers.Force };
                        repo.Checkout(repo.Head.Tip.Tree, filesPathsToUndo, options);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to Undo Changes", ex);
                return false;
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
            ReportFeatureUsage(operation: "Push");
            using (var repo = new LibGit2Sharp.Repository(RepositoryRootFolder))
            {
                PushOptions options = new PushOptions();
                options.OnPushStatusError += ErrorOnppush;
                options.CredentialsProvider = GetSourceCredentialsHandler();

                Branch currentBranch = repo.Branches.FirstOrDefault(x => x.FriendlyName == SourceControlBranch);
                repo.Network.Push(currentBranch, options);
            }
        }

        private void ErrorOnppush(PushStatusError pushStatusErrors)
        {
            throw new Exception("Error Occurred in push" + pushStatusErrors.Message);
        }

        public override List<string> GetConflictPaths()
        {
            List<string> ConflictPaths = [];

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
            catch (Exception ex)
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

        private static void ReportFeatureUsage(string operation)
        {
            Reporter.AddFeatureUsage(FeatureId.SourceControl, new TelemetryMetadata()
            {
                { "VersionControlType", "GIT" },
                { "Operation", operation }
            });
        }

    }
}
