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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GingerCore;
using SharpSvn;
using System.IO;
using System.Net;
using System.Collections.ObjectModel;
using System.Net.Security;

namespace Ginger.Team
{
    public class SVNSourceControl : SourceControlBase
    {
        SvnClient client;
        private List<string> mConflictsPaths = new List<string>();

        public override void Init()
        {
            try
            {
                client = new SvnClient();
                client.Authentication.DefaultCredentials = new System.Net.NetworkCredential(App.UserProfile.SourceControlUser, App.UserProfile.SourceControlPass);
                client.Conflict += Client_Conflict;
            }
            catch (Exception ex)
            {
                client = null;
                throw ex;
            }
        }

        public override bool TestConnection(ref string error)
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(
                   delegate
                   {
                       return true;
                   }
                   );
                WebResponse response = null;
                bool result = false;
                if (App.UserProfile.SourceControlURL.ToUpper().Trim().StartsWith("HTTP"))
                {
                    WebRequest request = WebRequest.Create(App.UserProfile.SourceControlURL);
                    request.Timeout = 15000;
                    request.Credentials = new System.Net.NetworkCredential(App.UserProfile.SourceControlUser, App.UserProfile.SourceControlPass);
                    response = (WebResponse)request.GetResponse();
               }
                else if (App.UserProfile.SourceControlURL.ToUpper().Trim().StartsWith("SVN"))
                {
                    using (SvnClient sc = new SvnClient())
                    {
                        sc.Authentication.DefaultCredentials = new System.Net.NetworkCredential(App.UserProfile.SourceControlUser, App.UserProfile.SourceControlPass);
                        Uri targetUri = new Uri(App.UserProfile.SourceControlURL);
                        var target = SvnTarget.FromUri(targetUri);
                        Collection<SvnInfoEventArgs> info;
                        result = sc.GetInfo(target, new SvnInfoArgs { ThrowOnError = false }, out info);
                    }
                }

                if (response != null || result)
                    return true;
                else
                {
                    error = "No Details";
                    return false;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
        
        
        public override void AddFile(string Path)
        {
            try
            {
                client.Add(Path);
            }
            catch (Exception e)
            {
                Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, e.Message + Environment.NewLine + e.InnerException);
            }

        }

        public override void UpdateFile(string Path)
        {
            try
            {
                mConflictsPaths.Clear();
                client.Update(Path);
                AskUserIfToResolveConflicts();
            }
            catch (Exception e)
            {
                Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, e.Message + Environment.NewLine + e.InnerException);
            }
        }

        public override void DeleteFile(string Path)
        {
            try
            {
                client.Delete(Path);
            }
            catch (Exception e)
            {
                Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, e.Message + Environment.NewLine + e.InnerException);             
            }
        }

        // Get Source Control one file info
        public override SourceControlFileInfo.eRepositoryItemStatus GetFileStatus(string Path)
        {
            if (Path == null ||  Path.Length == 0) return SourceControlFileInfo.eRepositoryItemStatus.Unknown;

            if (client == null) Init();
            System.Collections.ObjectModel.Collection<SvnStatusEventArgs> statuses;
            
            try
            {
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
            catch (Exception )
            {
                return SourceControlFileInfo.eRepositoryItemStatus.Unknown;
            }
        }


        public override ObservableList<SourceControlFileInfo> GetPathFilesStatus(string Path)
        {
            if (client == null) Init();
            ObservableList<SourceControlFileInfo> files = new ObservableList<SourceControlFileInfo>();

            System.Collections.ObjectModel.Collection<SvnStatusEventArgs> statuses;
            try
            {
                client.GetStatus(Path, out statuses);
                
                foreach (SvnStatusEventArgs arg in statuses)
                {
                    //TODO: removes saving veriosn to folder under Source control, meahnwhile ignore files with Prev
                    if (arg.FullPath.Contains("PrevVersions")) continue;
                    if (arg.FullPath.Contains("RecentlyUsed.dat")) continue;

                    SourceControlFileInfo SCFI = new SourceControlFileInfo();
                    SCFI.Path = arg.FullPath;
                    SCFI.SolutionPath = arg.FullPath.Replace(App.UserProfile.Solution.Folder, @"~\");
                    SCFI.Status = SourceControlFileInfo.eRepositoryItemStatus.Unknown;
                    SCFI.Selected = true;
                    SCFI.Diff = "";
                    if (arg.LocalContentStatus == SvnStatus.Modified)
                    {
                        SCFI.Status = SourceControlFileInfo.eRepositoryItemStatus.Modified;
                        SCFI.Diff = Diff(arg.FullPath, arg.Uri);                      
                    }
                    if (arg.LocalContentStatus == SvnStatus.NotVersioned)
                    {
                        SCFI.Status = SourceControlFileInfo.eRepositoryItemStatus.New;
                    }
                    if (arg.LocalContentStatus == SvnStatus.Missing)
                    {
                        SCFI.Status = SourceControlFileInfo.eRepositoryItemStatus.Deleted;
                    }
                    

                    if (SCFI.Status != SourceControlFileInfo.eRepositoryItemStatus.Unknown)
                    {
                        files.Add(SCFI);
                    }
                }                
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to comunicate with SVN server through path "+Path+" got error "+ex.Message);
                return files;
            }

            return files;
        }

        private string Diff(string pSourcePath, Uri u)
        {
            if (client == null) Init();
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
                MessageBox.Show(e.Message);
            }
            return "";
        }

        // Get all files in path recursive 
        public override void GetLatest(string path)
        {
            if (client == null) Init();
            SvnUpdateResult result;
            try
            {
                if (path != null)
                {
                    mConflictsPaths.Clear();
                    client.Update(path, out result);
                    AskUserIfToResolveConflicts();                   

                    if (result.Revision != -1)
                    {
                        Reporter.ToUser(eUserMsgKeys.UpdateToRevision, result.Revision);
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKeys.SourceControlUpdateFailed, "The files are not connected to source control");
                    }
                }
                else
                {
                    Reporter.ToUser(eUserMsgKeys.SourceControlUpdateFailed, "Invalid Path provided");
                }
            }
            catch(Exception ex)
            {
                client = null;
                Reporter.ToUser(eUserMsgKeys.SourceControlUpdateFailed, ex.Message + Environment.NewLine + ex.InnerException);
            }            
        }

        public override void GetProject(string Path, string URI)
        {
            if (client == null) Init();
            SvnUpdateResult result;

            try
            {
                client.CheckOut(new Uri(URI), Path, out result);
                Reporter.ToUser(eUserMsgKeys.DownloadedSolutionFromSourceControl, Path);                
            }
            catch (Exception e)
            {
                client = null;
                Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, e.Message + Environment.NewLine + e.InnerException);
            }            
        }        

        public override void CommitChanges(string Comments)
        {
            //Commit Changes
            SvnCommitArgs ca = new SvnCommitArgs();
            ca.LogMessage = Comments;
            try
            {
                client.Commit(App.UserProfile.Solution.Folder);
            }
            catch (Exception e)
            {
                Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, e.Message + Environment.NewLine + e.InnerException);
            }
        }

        public override void CommitChanges(ICollection<string> Paths, string Comments)
        {
            //Commit Changes
            SvnCommitArgs ca = new SvnCommitArgs();
            ca.LogMessage = Comments;
            try
            {
                client.Commit(Paths, ca);
            }
            catch (Exception e)
            {
                Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, e.Message + Environment.NewLine + e.InnerException);
            }
        }

        public override void CleanUp(string Path)
        {
            if (client == null) Init();
            try
            {
                client.CleanUp(Path);
            }
            catch (Exception e)
            {
            }
        }

        public override void Lock(string Path)
        {
            if (client == null) Init();
            try
            {
                client.Lock(Path, "Locking the Item");
            }
            catch (Exception e)
            {
                Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, e.Message + Environment.NewLine + e.InnerException);
            }
        }

        public override void UnLock(string Path)
        {
            if (client == null) Init();
            try
            {
                client.Unlock(Path);
            }
            catch (Exception e)
            {
                Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, e.Message + Environment.NewLine + e.InnerException);
            }
        }

        private void Client_Conflict(object sender, SvnConflictEventArgs e)
        {
            //conflict as raised- need to solve it
            mConflictsPaths.Add(e.MergedFile);
        }

        private void AskUserIfToResolveConflicts()
        {
            if (mConflictsPaths.Count > 0)
                foreach (string conflictPath in mConflictsPaths)
                {
                    ResolveConflictPage resConfPage = new ResolveConflictPage(conflictPath);
                    resConfPage.ShowAsWindow();
                }
        }

        public override void ResolveConflicts(string Path, eResolveConflictsSide side)
        {
            if (client == null) Init();
            try
            {
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
            }
            catch (Exception e)
            {
            }
        }

        public override void Revert(string Path)
        {
            if (client == null) Init();
            try
            {
                client.Revert(Path, new SvnRevertArgs {Depth = SvnDepth.Infinity });//throw all local changes and return to base copy
            }
            catch (Exception e)
            {
                Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, e.Message + Environment.NewLine + e.InnerException);
            }
        }

    }
}
