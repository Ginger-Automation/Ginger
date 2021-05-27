using Amdocs.Ginger.Common;
using GingerCoreNET.SourceControl;
using System;
using System.Collections.Generic;
using System.Text;
using Medallion.Shell;
namespace Amdocs.Ginger.CoreNET.SourceControl
{
    public class GitSourceControlShellWrapper : SourceControlBase
    {


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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public override string GetCurrentBranchForSolution()
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




            return RunGITCommand(new object[] { "clone", GetCloneUrlString() }, Path);



            string GetCloneUrlString()
            {
                if(string.IsNullOrEmpty(SourceControlUser))
                {
                    return URI;
                }
                Uri url = new Uri(URI);
                string scheme = url.Scheme;

                return url.Scheme + SourceControlUser + ":" + SourceControlPass + "@" + url.Host + url.AbsolutePath;
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
            throw new NotImplementedException();
        }

        public override void Init()
        {
            throw new NotImplementedException();
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
            var result = Command.Run("GIT",
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
    }
}
