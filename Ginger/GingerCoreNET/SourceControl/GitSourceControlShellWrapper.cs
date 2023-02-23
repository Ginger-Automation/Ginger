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

using Amdocs.Ginger.Common;
using GingerCoreNET.SourceControl;
using System;
using System.Collections.Generic;
using System.Text;
using Medallion.Shell;
using System.IO;

namespace Amdocs.Ginger.CoreNET.SourceControl
{
    public class GitSourceControlShellWrapper : SourceControlBase
    {
        private string RepositoryUrl;

        public GitSourceControlShellWrapper()
        {
            try
            {
                var result = Command.Run("git").Result;

            }
            catch (Exception e)

            {
                throw new PlatformNotSupportedException("GIT Commandline tools not found");
            }
        }

        public override string Name { get { return "GIT"; } }

        public override bool IsSupportingLocks { get { return true; } }

        public override bool IsSupportingGetLatestForIndividualFiles { get { return true; } }

        public override eSourceControlType GetSourceControlType { get { return eSourceControlType.GIT; } }

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

        public override bool CommitChanges(ICollection<string> Paths, string Comments, ref string error, ref List<string> conflictsPaths, bool includLockedFiles = false)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public override List<string> GetBranches()
        {
            return new List<string>();
        }

        public override string GetCurrentBranchForSolution()
        {
            return "master";
        }

        public override SourceControlFileInfo.eRepositoryItemStatus GetFileStatus(string Path, bool ShowIndicationkForLockedItems, ref string error)
        {
            throw new NotImplementedException();
        }

        public override SourceControlItemInfoDetails GetInfo(string path, ref string error)
        {
            throw new NotImplementedException();
        }

        public override bool GetLatest(string path, ref string error, ref List<string> conflictsPaths)
        {
            RunGITCommand(new object[] { "reset", "--hard", "HEAD" }, path);
            RunGITCommand(new object[] { "fetch"}, path);
            return RunGITCommand(new object[] { "pull" }, path);
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
            RepositoryUrl = URI;



            return RunGITCommand(new object[] { "clone",  GetCloneUrlString(),"." }, Path);



            string GetCloneUrlString()
            {
                if(string.IsNullOrEmpty(SourceControlUser))
                {
                    return URI;
                }
                Uri url = new Uri(URI);
                string scheme = url.Scheme;

                return url.Scheme+@"://" + SourceControlUser + ":" + SourceControlPass + "@" + url.Host + url.AbsolutePath;
            }
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
            return RepositoryUrl;
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
            throw new NotImplementedException();
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

        private static bool RunGITCommand(object[] args, string Path)
        {
            if(!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }
            var command = "git";

  
            var result = Command.Run("git",
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
    }
}
