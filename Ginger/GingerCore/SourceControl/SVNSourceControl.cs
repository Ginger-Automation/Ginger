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
using GingerCoreNET.SourceControl;
using SharpSvn;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.CompilerServices;

namespace GingerCore.SourceControl
{
    public class SVNSourceControl : SourceControlBase
    {
        SvnClient client;
        public List<string> mConflictsPaths = new List<string>();

        public override string Name { get { return "SVN"; } }

        public override bool IsSupportingLocks { get { return true; } }

        public override bool IsSupportingGetLatestForIndividualFiles { get { return true; } }

        public override eSourceControlType GetSourceControlType { get { return eSourceControlType.SVN; } }

        public override List<string> GetSourceControlmConflict { get { return mConflictsPaths; } }

        public override bool AddFile(string Path, ref string error)
        {
            try
            {
                client.Add(Path);
            }
            catch (Exception e)
            {
                error = e.Message + Environment.NewLine + e.InnerException;
                return false;
            }
            return true;
        }

        public override bool UpdateFile(string Path, ref string error)
        {
            try
            {
                client.Update(Path);
            }
            catch (Exception e)
            {
                error = e.Message + Environment.NewLine + e.InnerException;
                return false;
            }
            return true;
        }

        public override bool DeleteFile(string Path, ref string error)
        {
            try
            {
                client.Delete(Path);
            }
            catch (Exception e)
            {
                error = e.Message + Environment.NewLine + e.InnerException;
                return false;
            }
            return true;
        }


        // Get Source Control one file info
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override SourceControlFileInfo.eRepositoryItemStatus GetFileStatus(string Path, bool ShowIndicationkForLockedItems, ref string error)
        {
            if (Path == null || Path.Length == 0) return SourceControlFileInfo.eRepositoryItemStatus.Unknown;

            if (client == null) Init();

            System.Collections.ObjectModel.Collection<SvnStatusEventArgs> statuses;

            Uri targetUri = null;

            try
            {
                int lastDotIndex = Path.LastIndexOf(".");
                if (lastDotIndex != -1 && Path.Substring(lastDotIndex).ToUpper() == ".XML" && ShowIndicationkForLockedItems)
                {

                    Collection<SvnListEventArgs> ListEventArgs;
                    targetUri = GetRemoteUriFromPath(Path, out ListEventArgs);


                    if (ListEventArgs != null && ListEventArgs[0].Lock != null && ListEventArgs[0].Lock.Owner == SourceControlUser)
                        return SourceControlFileInfo.eRepositoryItemStatus.LockedByMe;
                    else if (ListEventArgs != null && ListEventArgs[0].Lock != null && ListEventArgs[0].Lock.Owner != SourceControlUser)
                        return SourceControlFileInfo.eRepositoryItemStatus.LockedByAnotherUser;
                }

                client.GetStatus(Path, out statuses);


                foreach (SvnStatusEventArgs arg in statuses)
                {
                    if (arg.LocalContentStatus == SvnStatus.Modified)
                    {
                        return SourceControlFileInfo.eRepositoryItemStatus.Modified;
                    }
                    if (arg.LocalContentStatus == SvnStatus.NotVersioned)
                    {
                        return SourceControlFileInfo.eRepositoryItemStatus.New;
                    }
                    if (arg.LocalContentStatus == SvnStatus.Missing)
                    {
                        return SourceControlFileInfo.eRepositoryItemStatus.Deleted;
                    }
                }

                return SourceControlFileInfo.eRepositoryItemStatus.Equel;
            }
            catch (Exception ex)
            {
                error = ex.Message + Environment.NewLine + ex.InnerException;
                return SourceControlFileInfo.eRepositoryItemStatus.New;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override ObservableList<SourceControlFileInfo> GetPathFilesStatus(string Path, ref string error, List<string> PathsToIgnore = null, bool includLockedFiles = false)
        {
            if (client == null) Init();
            ObservableList<SourceControlFileInfo> files = new ObservableList<SourceControlFileInfo>();

            System.Collections.ObjectModel.Collection<SvnStatusEventArgs> statuses;
            try
            {
                client.GetStatus(Path, out statuses);

                foreach (SvnStatusEventArgs arg in statuses)
                {
                    if (PathsToIgnore != null)
                    {
                        bool pathToIgnoreFound = false;
                        foreach (string pathToIgnore in PathsToIgnore)
                            if (System.IO.Path.GetFullPath(arg.FullPath).Contains(System.IO.Path.GetFullPath(pathToIgnore)) || arg.FullPath.Contains(pathToIgnore))
                            {
                                pathToIgnoreFound = true;
                                break;
                            }
                        if (pathToIgnoreFound) continue;
                    }

                    if (System.IO.Path.GetExtension(arg.FullPath) == ".ldb" || System.IO.Path.GetExtension(arg.FullPath) == ".ignore")
                        continue;

                    SourceControlFileInfo SCFI = new SourceControlFileInfo();
                    SCFI.Path = arg.FullPath;
                    SCFI.SolutionPath = arg.FullPath.Replace(SolutionFolder, @"~\");
                    SCFI.Status = SourceControlFileInfo.eRepositoryItemStatus.Unknown;
                    SCFI.Selected = true;
                    SCFI.Diff = "";
                    if (arg.LocalContentStatus == SvnStatus.Modified)
                    {
                        SCFI.Status = SourceControlFileInfo.eRepositoryItemStatus.Modified;
                        Task.Run(() =>
                        SCFI.Diff = Diff(arg.FullPath, arg.Uri));
                    }
                    if (arg.LocalContentStatus == SvnStatus.NotVersioned)
                    {
                        SCFI.Status = SourceControlFileInfo.eRepositoryItemStatus.New;
                    }
                    if (arg.LocalContentStatus == SvnStatus.Missing)
                    {
                        SCFI.Status = SourceControlFileInfo.eRepositoryItemStatus.Deleted;
                    }

                    if (includLockedFiles)
                    {
                        Collection<SvnListEventArgs> ListEventArgs;
                        Task.Run(() =>
                        {
                            GetRemoteUriFromPath(arg.FullPath, out ListEventArgs);

                            if (ListEventArgs != null && ListEventArgs[0].Lock != null)
                            {
                                SCFI.Locked = true;
                                SCFI.LockedOwner = ListEventArgs[0].Lock.Owner;
                                SCFI.LockComment = ListEventArgs[0].Lock.Comment;
                            }
                            else
                            {
                                SCFI.Locked = false;
                            }
                        });
                    }

                    if (SCFI.Status != SourceControlFileInfo.eRepositoryItemStatus.Unknown)
                        files.Add(SCFI);
                }
            }
            catch (Exception ex)
            {
                error = ex.Message + Environment.NewLine + ex.InnerException;
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to communicate with SVN server through path " + Path + " got error " + ex.Message);
                return files;
            }

            return files;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private string Diff(string pSourcePath, Uri u)
        {
            if (client == null)
            {
                Init();
            }
            try
            {
                MemoryStream objMemoryStream = new MemoryStream();
                SvnDiffArgs da = new SvnDiffArgs();
                da.IgnoreAncestry = true;
                da.DiffArguments.Add("-b");
                da.DiffArguments.Add("-w");

                bool b = client.Diff(SvnTarget.FromString(pSourcePath), new SvnUriTarget(u, SvnRevision.Head), da, objMemoryStream);
                objMemoryStream.Position = 0;
                StreamReader strReader = new StreamReader(objMemoryStream);
                string str = strReader.ReadToEnd();
                return str;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        // Get all files in path recursive 
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override bool GetLatest(string path, ref string error, ref List<string> conflictsPaths)
        {
            if (client == null) Init();
            SvnUpdateResult result;
            try
            {
                mConflictsPaths.Clear();
                client.Update(path, out result);

                if (mConflictsPaths.Count > 0)
                {
                    conflictsPaths = mConflictsPaths;
                    return false;
                }

                if (result.Revision != -1)
                {
                    if (supressMessage == true)
                        Reporter.ToLog(eAppReporterLogLevel.INFO, "The solution was updated successfully to revision:  " + result.Revision);
                    else
                        Reporter.ToUser(eUserMsgKeys.UpdateToRevision, result.Revision);
                }
                else
                {
                    if (supressMessage == true)
                        Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to update the solution from source control.Error Details: 'The files are not connected to source control'");
                    else
                        Reporter.ToUser(eUserMsgKeys.SourceControlUpdateFailed, "The files are not connected to source control");
                }

            }
            catch (Exception ex)
            {
                client = null;
                error = ex.Message + Environment.NewLine + ex.InnerException;
                return false;
            }
            return true;
        }

        public override bool GetProject(string Path, string URI, ref string error)
        {
            if (client == null) Init();
            SvnUpdateResult result;

            try
            {
                client.CheckOut(new Uri(URI), Path, out result);
            }
            catch (Exception e)
            {
                client = null;
                error = e.Message + Environment.NewLine + e.InnerException;
                return false;

            }
            return true;
        }

        public override bool CommitChanges(string Comments, ref string error)
        {
            //Commit Changes
            SvnCommitArgs ca = new SvnCommitArgs();
            ca.LogMessage = Comments;
            try
            {
                client.Commit(SolutionFolder);
            }
            catch (Exception e)
            {
                error = e.Message + Environment.NewLine + e.InnerException;
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override bool CommitChanges(ICollection<string> Paths, string Comments, ref string error, ref List<string> conflictsPaths, bool includLockedFiles = false)
        {
            //Commit Changes
            SvnCommitArgs ca = new SvnCommitArgs();
            ca.LogMessage = Comments;
            SvnCommitResult result;
            try
            {
                if (includLockedFiles)
                {
                    bool unlockResult = true;
                    foreach (string file in Paths)
                    {
                        Collection<SvnListEventArgs> ListEventArgs;
                        Uri targetUri = GetRemoteUriFromPath(file, out ListEventArgs);
                        if (ListEventArgs != null && ListEventArgs[0].Lock != null)
                        {
                            SvnUnlockArgs args = new SvnUnlockArgs();
                            args.BreakLock = true;
                            unlockResult = client.RemoteUnlock(targetUri, args);
                            if (unlockResult == false && Reporter.ToUser(eUserMsgKeys.SourceControlUnlockFaild, file, targetUri.ToString()) == MessageBoxResult.No)
                            {
                                error = "Check in aborted";
                                return false;
                            }
                        }
                    }
                }

                client.Commit(Paths, ca, out result);
                if (result != null)
                    if (result.Revision != -1)
                    {
                        Reporter.ToUser(eUserMsgKeys.CommitedToRevision, result.Revision);
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKeys.SourceControlCommitFailed, "The files are not connected to source control");
                    }
            }
            catch (Exception e)
            {
                if (e.InnerException != null && e.InnerException.ToString().Contains("conflict"))
                {
                    if (mConflictsPaths.Count > 0)
                    {
                        conflictsPaths = mConflictsPaths;
                    }
                }
                error = e.Message + Environment.NewLine + e.InnerException;
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void CleanUp(string Path)
        {
            if (client == null) Init();
            try
            {
                client.CleanUp(Path);
            }
            catch (Exception e)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private Uri GetRemoteUriFromPath(string Path, out Collection<SvnListEventArgs> ListEventArgs)
        {
            try
            {
                string relativePath = System.IO.Path.GetFullPath(Path);
                relativePath = relativePath.Replace(SolutionFolder, "");
                Uri targetUri = new Uri(SourceControlURL + relativePath);
                SvnTarget target = SvnTarget.FromUri(targetUri);
                SvnListArgs args = new SvnListArgs();
                args.RetrieveLocks = true;
                client.GetList(target, args, out ListEventArgs);

                return targetUri;
            }
            catch (Exception ex)
            {
                ListEventArgs = null;
                Reporter.ToLog(eAppReporterLogLevel.WARN, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return null;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override bool Lock(string Path, string lockComment, ref string error)
        {
            if (client == null)
            {
                Init();
            }
            bool result = false;
            try
            {
                string relativePath = System.IO.Path.GetFullPath(Path);
                relativePath = relativePath.Replace(SolutionFolder, "");

                Uri targetUri = new Uri(SourceControlURL + relativePath);
                result = client.RemoteLock(targetUri, lockComment);
            }
            catch (Exception e)
            {
                error = e.Message + Environment.NewLine + e.InnerException;
                return false;
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override bool UnLock(string Path, ref string error)
        {
            if (client == null) Init();
            bool result = false;
            try
            {
                Collection<SvnListEventArgs> ListEventArgs;
                Uri targetUri = GetRemoteUriFromPath(Path, out ListEventArgs);

                if (ListEventArgs != null && ListEventArgs[0].Lock != null && ListEventArgs[0].Lock.Owner != SourceControlUser)
                {
                    if ((Reporter.ToUser(eUserMsgKeys.SourceControlFileLockedByAnotherUser, Path, ListEventArgs[0].Lock.Owner, ListEventArgs[0].Lock.Comment) == MessageBoxResult.Yes))
                    {
                        SvnUnlockArgs args = new SvnUnlockArgs();
                        args.BreakLock = true;
                        result = client.RemoteUnlock(targetUri, args);
                    }
                    else
                    {
                        return false;
                    }

                }
                else if (ListEventArgs != null && ListEventArgs[0].Lock != null && ListEventArgs[0].Lock.Owner == SourceControlUser)
                {
                    result = client.RemoteUnlock(targetUri);
                }
                else
                {
                    error = "Cannot Unlock Unlocked File";
                    return false;
                }
            }
            catch (Exception e)
            {
                error = e.Message + Environment.NewLine + e.InnerException;
                return false;
            }
            return result;
        }

        private void Client_Conflict(object sender, SvnConflictEventArgs e)
        {
            //conflict as raised- need to solve it
            mConflictsPaths.Add(e.MergedFile);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override bool ResolveConflicts(string Path, eResolveConflictsSide side, ref string error)
        {
            if (client == null) Init();
            try
            {
                if (System.IO.Path.GetExtension(Path) != string.Empty && !System.IO.File.Exists(System.IO.Path.GetFullPath(Path.Replace(".xml", ".ignore"))))
                {
                    System.IO.File.Copy(Path, (System.IO.Path.GetFullPath(Path.Replace(".xml", ".ignore"))));
                }
                CleanUp(Path);
                switch (side)
                {
                    case eResolveConflictsSide.Local:
                        client.Resolve(Path, SvnAccept.Mine, new SvnResolveArgs { Depth = SvnDepth.Infinity });//keep local changes for all conflicts with server
                        client.Resolved(Path);
                        break;
                    case eResolveConflictsSide.Server:
                        client.Resolve(Path, SvnAccept.Theirs, new SvnResolveArgs { Depth = SvnDepth.Infinity });//keep local changes for all conflicts with server
                        client.Resolved(Path);
                        break;
                }

                string fileContent = File.ReadAllText(Path);
                if (fileContent.Contains("<<<<<<<"))
                    Reporter.ToUser(eUserMsgKeys.SourceControlConflictResolveFailed, Path);
            }
            catch (Exception e)
            {
                error = e.Message + Environment.NewLine + e.InnerException;
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override bool Revert(string Path, ref string error)
        {
            if (client == null) Init();
            try
            {
                client.Revert(Path, new SvnRevertArgs { Depth = SvnDepth.Infinity });//throw all local changes and return to base copy
            }
            catch (Exception e)
            {
                error = e.Message + Environment.NewLine + e.InnerException;
                return false;
            }
            return true;
        }

        public override ObservableList<SolutionInfo> GetProjectsList()
        {
            ObservableList<SolutionInfo> SourceControlSolutions = new ObservableList<SolutionInfo>();
            if (SourceControlURL.ToUpper().Trim().StartsWith("HTTP"))
            {
                WebRequest request = WebRequest.Create(SourceControlURL);
                request.Timeout = 60000;
                request.Credentials = new System.Net.NetworkCredential(SourceControlUser, SourceControlPass);
                SourceControlSolutions.Clear();
                Stream objStream;

                objStream = request.GetResponse().GetResponseStream();
                StreamReader objReader = new StreamReader(objStream);

                //quick and dirty way to get the folder instead of messing with XML
                string sLine = "";
                while (!objReader.EndOfStream)
                {
                    sLine = objReader.ReadLine();
                    if (sLine.Contains("dir name"))
                    {
                        int i1 = sLine.IndexOf("dir name=");
                        int len = sLine.IndexOf(" href") - i1 - 11;
                        string SolutionName = sLine.Substring(i1 + 10, len);

                        AddSolution(SourceControlSolutions, SourceControlLocalFolder + @"\" + SolutionName, SolutionName);
                    }
                    else if (sLine.StartsWith("  <li><a href=\""))
                    {
                        int start = sLine.IndexOf("\"");
                        sLine = sLine.Substring(start + 1);
                        int end = sLine.IndexOf("\"");
                        string SolutionName = sLine.Substring(0, end - 1);

                        if (!SolutionName.Contains("."))
                            AddSolution(SourceControlSolutions, SourceControlLocalFolder + @"\" + SolutionName, SolutionName);
                    }
                }
            }
            else if (SourceControlURL.ToUpper().Trim().StartsWith("SVN"))
            {
                using (SvnClient sc = new SvnClient())
                {
                    sc.Authentication.DefaultCredentials = new System.Net.NetworkCredential(SourceControlUser, SourceControlPass);
                    Uri targetUri = new Uri(SourceControlURL);
                    var target = SvnTarget.FromUri(targetUri);
                    Collection<SvnInfoEventArgs> info;
                    bool result = sc.GetInfo(target, new SvnInfoArgs { ThrowOnError = false }, out info);
                    SourceControlSolutions.Clear();
                    if (result)
                    {
                        foreach (var f in info)
                        {
                            AddSolution(SourceControlSolutions, SourceControlLocalFolder + @"\" + f.Path, f.Uri.OriginalString);
                        }
                    }
                }
            }
            return SourceControlSolutions;
        }

        private void AddSolution(ObservableList<SolutionInfo> SourceControlSolutions, string LocalFolder, string SourceControlLocation)
        {
            SolutionInfo sol = new SolutionInfo();
            sol.LocalFolder = LocalFolder;
            if (Directory.Exists(sol.LocalFolder))
                sol.ExistInLocaly = true;
            else
                sol.ExistInLocaly = false;
            sol.SourceControlLocation = SourceControlLocation;
            SourceControlSolutions.Add(sol);
        }

        public override void Init()
        {
            try
            {
                client = new SvnClient();
                client.Authentication.Clear();
                client.Authentication.DefaultCredentials = new System.Net.NetworkCredential(SourceControlUser, SourceControlPass);
                client.Conflict += Client_Conflict;
                client.Authentication.SslServerTrustHandlers += new EventHandler<SharpSvn.Security.SvnSslServerTrustEventArgs>(Authentication_SslServerTrustHandlers);
            }
            catch (Exception ex)
            {
                client = null;
                throw ex;
            }
        }

        public void Authentication_SslServerTrustHandlers(object sender, SharpSvn.Security.SvnSslServerTrustEventArgs e)
        {
            e.AcceptedFailures = e.Failures;
            e.Save = true;
        }

        public override bool TestConnection(ref string error)
        {
            WebResponse response = null;
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(
                   delegate
                   {
                       return true;
                   }
                   );

                bool result = false;
                if (SourceControlURL.ToUpper().Trim().StartsWith("HTTP"))
                {
                    if (!SourceControlURL.EndsWith("/"))
                    {
                        SourceControlURL = SourceControlURL + "/";
                    }
                    WebRequest request = WebRequest.Create(SourceControlURL);

                    request.Timeout = SourceControlTimeout * 1000;
                    request.Credentials = new System.Net.NetworkCredential(SourceControlUser, SourceControlPass);
                    response = (WebResponse)request.GetResponse();
                }
                else if (SourceControlURL.ToUpper().Trim().StartsWith("SVN"))
                {
                    using (SvnClient sc = new SvnClient())
                    {
                        sc.Authentication.DefaultCredentials = new System.Net.NetworkCredential(SourceControlUser, SourceControlPass);
                        Uri targetUri = new Uri(SourceControlURL);
                        var target = SvnTarget.FromUri(targetUri);
                        Collection<SvnInfoEventArgs> info;
                        result = sc.GetInfo(target, new SvnInfoArgs { ThrowOnError = false }, out info);
                    }
                }

                if (response != null || result)
                {
                    return true;
                }
                else
                {
                    error = "No Details";
                    return false;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                if (response != null)
                {
                    response.Close();
                }
                return false;
            }
            finally
            {
                //Close Response
                if (response != null)
                {
                    response.Close();
                }
            }
        }

        public override void Disconnect()
        {
            if (client != null)
            {
                client.Dispose();
                client = null;
            }

        }

        public override string GetRepositoryURL(ref string error)
        {
            if (client == null) Init();


            SvnInfoEventArgs info;
            client.GetInfo(SolutionFolder, out info);
            string RemoteURL = info.Uri.ToString();

            return RemoteURL;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override SourceControlItemInfoDetails GetInfo(string path, ref string error)
        {

            try
            {
                if (client == null) Init();
                SvnInfoEventArgs info;
                client.GetInfo(path, out info);
                System.Collections.ObjectModel.Collection<SvnStatusEventArgs> statuses;
                client.GetStatus(path, out statuses);
                Collection<SvnListEventArgs> ListEventArgs;
                Uri targetUri = GetRemoteUriFromPath(path, out ListEventArgs);

                SourceControlItemInfoDetails SCIID = new SourceControlItemInfoDetails();

                SCIID.ShowFileInfo = true;
                SCIID.FilePath = info.Path;
                SCIID.FileWorkingDirectory = info.WorkingCopyRoot;
                SCIID.ShowChangeInfo = true;
                SCIID.LastChangeAuthor = info.LastChangeAuthor;
                SCIID.LastChangeRevision = " " + info.LastChangeRevision;
                SCIID.LastChangeTime = " " + info.LastChangeTime.ToLocalTime();

                if (ListEventArgs != null && ListEventArgs[0].Lock != null)
                {
                    SCIID.ShowLock = true;
                    SCIID.LockOwner = ListEventArgs[0].Lock.Owner;
                    SCIID.LockCreationTime = " " + ListEventArgs[0].Lock.CreationTime.ToLocalTime(); ;
                    SCIID.LockComment = " " + ListEventArgs[0].Lock.Comment;
                }
                return SCIID;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override string GetLockOwner(string path, ref string error)
        {
            if (client == null) Init();

            Collection<SvnListEventArgs> ListEventArgs;
            Uri targetUri = GetRemoteUriFromPath(path, out ListEventArgs);

            if (ListEventArgs != null && ListEventArgs[0].Lock != null && ListEventArgs[0].Lock.Owner != null)
            {
                return ListEventArgs[0].Lock.Owner;
            }
            else
            {
                return null;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override SourceControlItemInfoDetails GetRepositoryInfo(ref string error)
        {
            if (client == null) Init();
            try
            {
                SvnInfoEventArgs info;
                client.GetInfo(SolutionFolder, out info);

                SourceControlItemInfoDetails SCIID = new SourceControlItemInfoDetails();
                SCIID.ShowRepositoryInfo = true;
                SCIID.RepositoryRoot = " " + info.RepositoryRoot;
                SCIID.RepositoryPath = info.FullPath;
                SCIID.RepositoryId = " " + info.RepositoryId;
                SCIID.WorkingCopyRoot = " " + info.WorkingCopyRoot;
                SCIID.WorkingCopySize = " " + info.WorkingCopySize;
                SCIID.CopyFromRevision = "  " + info.CopyFromRevision;
                SCIID.Revision = " " + info.Revision;
                return SCIID;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        public override bool CreateConfigFile(ref string error)
        {
            return true;
        }
    }
}
