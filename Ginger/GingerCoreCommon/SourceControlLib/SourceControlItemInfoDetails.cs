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

namespace GingerCoreNET.SourceControl
{
    public class SourceControlItemInfoDetails
    {
        public bool ShowRepositoryInfo = false;
        public string RepositoryRoot;
        public string RepositoryPath;
        public string RepositoryId;
        public string WorkingCopyRoot;
        public string WorkingCopySize;
        public string CopyFromRevision;
        public string Revision;
        public string TrackedBranchName;
        public string BranchName;
        public string CanonicalName;
        public string FriendlyName;
        public bool ShowFileInfo = false;
        public string FilePath;
        public string FileWorkingDirectory;
        public string FileState;
        public string HasUnpushedCommits;
        public string HasUncommittedChanges;
        public bool ShowChangeInfo = false;
        public string LastChangeAuthor;
        public string LastChangeCommiter;
        public string LastChangeMessage;
        public string LastChangeRevision;
        public string LastChangeTime;
        public bool ShowLock = false;
        public string LockOwner;
        public string LockCreationTime;
        public string LockComment;
    }
}
