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

using GingerCore.Helpers;
using GingerCoreNET.SourceControl;
using System.Windows.Controls;

namespace Ginger.SourceControl
{
    /// <summary>
    /// Interaction logic for SourceControlItemInfoPage.xaml
    /// </summary>
    public partial class SourceControlItemInfoPage : Page
    {
        SourceControlItemInfoDetails mSCIInfoDetails;
        GenericWindow _GenWin;

        public SourceControlItemInfoPage(SourceControlItemInfoDetails SCIInfoDetails)
        {
            InitializeComponent();
            TextBlockHelper TBH = new TextBlockHelper(ItemInfoTextBlock);
            mSCIInfoDetails = SCIInfoDetails;
            Init(TBH);
        }

        private void Init(TextBlockHelper TBH)
        {
            if (mSCIInfoDetails.ShowRepositoryInfo)
            {
                TBH.AddBoldText("Solution Repository Info:");
                TBH.AddLineBreak();
                if (!string.IsNullOrEmpty(mSCIInfoDetails.RepositoryRoot))
                {
                    TBH.AddText("RepositoryRoot: " + mSCIInfoDetails.RepositoryRoot);
                    TBH.AddLineBreak();
                }
                if (!string.IsNullOrEmpty(mSCIInfoDetails.RepositoryPath))
                {
                    TBH.AddText("RepositoryPath: " + mSCIInfoDetails.RepositoryPath);
                    TBH.AddLineBreak();
                }
                if (!string.IsNullOrEmpty(mSCIInfoDetails.RepositoryId))
                {
                    TBH.AddText("RepositoryId: " + mSCIInfoDetails.RepositoryId);
                    TBH.AddLineBreak();
                }
                if (!string.IsNullOrEmpty(mSCIInfoDetails.WorkingCopyRoot))
                {
                    TBH.AddText("WorkingCopyRoot: " + mSCIInfoDetails.WorkingCopyRoot);
                    TBH.AddLineBreak();
                }
                if (!string.IsNullOrEmpty(mSCIInfoDetails.WorkingCopySize))
                {
                    TBH.AddText("WorkingCopySize: " + mSCIInfoDetails.WorkingCopySize);
                    TBH.AddLineBreak();
                }
                if (!string.IsNullOrEmpty(mSCIInfoDetails.CopyFromRevision))
                {
                    TBH.AddText("CopyFromRevision: " + mSCIInfoDetails.CopyFromRevision);
                    TBH.AddLineBreak();
                }
                if (!string.IsNullOrEmpty(mSCIInfoDetails.Revision))
                {
                    TBH.AddText("Revision: " + mSCIInfoDetails.Revision);
                    TBH.AddLineBreak();
                }
                if (!string.IsNullOrEmpty(mSCIInfoDetails.TrackedBranchName))
                {
                    TBH.AddText("TrackedBranchName: " + mSCIInfoDetails.TrackedBranchName);
                    TBH.AddLineBreak();
                }
                if (!string.IsNullOrEmpty(mSCIInfoDetails.BranchName))
                {
                    TBH.AddText("BranchName: " + mSCIInfoDetails.BranchName);
                    TBH.AddLineBreak();
                }
                if (!string.IsNullOrEmpty(mSCIInfoDetails.CanonicalName))
                {
                    TBH.AddText("CanonicalName: " + mSCIInfoDetails.CanonicalName);
                    TBH.AddLineBreak();
                }
                if (!string.IsNullOrEmpty(mSCIInfoDetails.FriendlyName))
                {
                    TBH.AddText("FriendlyName: " + mSCIInfoDetails.FriendlyName);
                    TBH.AddLineBreak();
                }
            }
            if (mSCIInfoDetails.ShowFileInfo)
            {

                TBH.AddBoldText("File Info:");
                TBH.AddLineBreak();
                if (!string.IsNullOrEmpty(mSCIInfoDetails.FilePath))
                {
                    TBH.AddText("FilePath: " + mSCIInfoDetails.FilePath);
                    TBH.AddLineBreak();
                }
                if (!string.IsNullOrEmpty(mSCIInfoDetails.FileWorkingDirectory))
                {
                    TBH.AddText("FileWorkingDirectory: " + mSCIInfoDetails.FileWorkingDirectory);
                    TBH.AddLineBreak();
                }
                if (!string.IsNullOrEmpty(mSCIInfoDetails.FileState))
                {
                    TBH.AddText("FileState: " + mSCIInfoDetails.FileState);
                    TBH.AddLineBreak();
                }
                if (!string.IsNullOrEmpty(mSCIInfoDetails.HasUnpushedCommits))
                {
                    TBH.AddText("HasUnpushedCommits: " + mSCIInfoDetails.HasUnpushedCommits);
                    TBH.AddLineBreak();
                }
                if (!string.IsNullOrEmpty(mSCIInfoDetails.HasUncommittedChanges))
                {
                    TBH.AddText("HasUncommittedChanges: " + mSCIInfoDetails.HasUncommittedChanges);
                    TBH.AddLineBreak();
                }
            }

            if (mSCIInfoDetails.ShowChangeInfo)
            {

                TBH.AddBoldText("Change Info:");
                TBH.AddLineBreak();
                if (!string.IsNullOrEmpty(mSCIInfoDetails.LastChangeAuthor))
                {
                    TBH.AddText("LastChangeAuthor: " + mSCIInfoDetails.LastChangeAuthor);
                    TBH.AddLineBreak();
                }
                if (!string.IsNullOrEmpty(mSCIInfoDetails.LastChangeCommiter))
                {
                    TBH.AddText("LastChangeCommiter: " + mSCIInfoDetails.LastChangeCommiter);
                    TBH.AddLineBreak();
                }
                if (!string.IsNullOrEmpty(mSCIInfoDetails.LastChangeRevision))
                {
                    TBH.AddText("LastChangeRevision: " + mSCIInfoDetails.LastChangeRevision);
                    TBH.AddLineBreak();
                }
                if (!string.IsNullOrEmpty(mSCIInfoDetails.LastChangeTime))
                {
                    TBH.AddText("LastChangeTime: " + mSCIInfoDetails.LastChangeTime);
                    TBH.AddLineBreak();
                }
                if (!string.IsNullOrEmpty(mSCIInfoDetails.LastChangeMessage))
                {
                    TBH.AddText("LastChangeTime: " + mSCIInfoDetails.LastChangeMessage);
                    TBH.AddLineBreak();
                }
            }

            if (mSCIInfoDetails.ShowLock)
            {

                TBH.AddBoldText("Lock Info:");
                TBH.AddLineBreak();
                if (!string.IsNullOrEmpty(mSCIInfoDetails.LockOwner))
                {
                    TBH.AddText("LockOwner: " + mSCIInfoDetails.LockOwner);
                    TBH.AddLineBreak();
                }
                if (!string.IsNullOrEmpty(mSCIInfoDetails.LockCreationTime))
                {
                    TBH.AddText("LockCreationTime: " + mSCIInfoDetails.LockCreationTime);
                    TBH.AddLineBreak();
                }
                if (!string.IsNullOrEmpty(mSCIInfoDetails.LockComment))
                {
                    TBH.AddText("LockComment: " + mSCIInfoDetails.LockComment);
                    TBH.AddLineBreak();
                }
            }
        }
        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Free)
        {
            string Title = string.Empty;
            //Todo create different header for file/repository
            if (mSCIInfoDetails.ShowRepositoryInfo)
                Title = "Source Control Repository Info";
            else
                Title = "Source Control File Info";

            GingerCore.General.LoadGenericWindow(ref _GenWin, null, windowStyle, Title, this);
        }
    }
}
