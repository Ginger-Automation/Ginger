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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.ConflictResolve;
using Ginger.Run;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.Environments;
using GingerCore.Variables;
using GingerCoreNET.SourceControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SourceControl
{
    /// <summary>
    /// Interaction logic for CheckInWindow.xaml
    /// </summary>
    public partial class CheckInPage : Page
    {
        //TODO: put it in App
        string mPath;
        ObservableList<SourceControlFileInfo> mFiles;
        GenericWindow genWin = null;
        bool mCheckInWasDone = false;
        private List<string> parentFolders = null;

        public CheckInPage(string path)
        {
            InitializeComponent();
            SourceControlIntegration.BusyInProcessWhileDownloading = false;
            mPath = path;
            lblAnalyzedPath.Content = mPath;
            if (WorkSpace.Instance.Solution.SourceControl.GetSourceControlType == SourceControlBase.eSourceControlType.GIT)
            {
                xSourceControlBranchPanel.Visibility = Visibility.Visible;
                xSourceControlBranchLabel.Content = WorkSpace.Instance.Solution.SourceControl.GetCurrentWorkingBranch();
            }
            else
            {
                xSourceControlBranchPanel.Visibility = Visibility.Collapsed;
            }
            Init();

            SetCheckinGridView();
            CheckInFilesGrid.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGrid));
            CheckInFilesGrid.AddToolbarTool("@CheckAllColumn_16x16.png", "Select All", new RoutedEventHandler(SelectAll));
            CheckInFilesGrid.AddToolbarTool("@UnCheckAllColumn_16x16.png", "Un-Select All", new RoutedEventHandler(UnSelectAll));
            CheckInFilesGrid.RowDoubleClick += CheckInFilesGrid_grdMain_MouseDoubleClick;

            if (WorkSpace.Instance.Solution.SourceControl.GetSourceControlType == SourceControlBase.eSourceControlType.GIT)
            {
                SetLocalCommitGridView();
                LocalCommitedFilesGrid.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshLocalCommitsGrid));
                InitLocalCommitGrid();
            }

            CommentsTextBox.Focus();
        }

        private void SetCheckinGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = [];
            view.GridColsView = viewCols;

            // TODO: use fields
            viewCols.Add(new GridColView() { Field = nameof(SourceControlFileInfo.Selected), Header = "Selected", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.CheckBox, AllowSorting = true });
            viewCols.Add(new GridColView() { Field = nameof(SourceControlFileInfo.Status), Header = "Status", WidthWeight = 10, ReadOnly = true, AllowSorting = true });
            viewCols.Add(new GridColView() { Field = nameof(SourceControlFileInfo.Name), Header = "Item Name", WidthWeight = 20, AllowSorting = true });
            viewCols.Add(new GridColView() { Field = nameof(SourceControlFileInfo.FileType), Header = "Item Type", WidthWeight = 20, AllowSorting = true });
            viewCols.Add(new GridColView() { Field = nameof(SourceControlFileInfo.SolutionPath), Header = "Item Path", WidthWeight = 40, ReadOnly = true, AllowSorting = true });
            if (WorkSpace.Instance.Solution.ShowIndicationkForLockedItems)
            {
                viewCols.Add(new GridColView() { Field = nameof(SourceControlFileInfo.Locked), Header = "Locked", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.Text });
            }
            CheckInFilesGrid.SetAllColumnsDefaultView(view);
            CheckInFilesGrid.InitViewItems();
        }

        private async void Init()
        {
            try
            {
                xProcessingIcon.Visibility = Visibility.Visible;
                if (SourceControlIntegration.BusyInProcessWhileDownloading)
                {
                    Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Please wait for current process to end.");
                    return;
                }
                SourceControlIntegration.BusyInProcessWhileDownloading = true;

                await Task.Run(() =>
                {
                    mFiles = SourceControlIntegration.GetPathFilesStatus(WorkSpace.Instance.Solution.SourceControl, mPath);
                    //set items name and type
                    Parallel.ForEach(mFiles, SCFI =>
                     {
                         try
                         {
                             if (SCFI.Path.ToUpper().Contains(".GINGER.") && SCFI.Path.ToUpper().Contains(".XML") && SCFI.Status != SourceControlFileInfo.eRepositoryItemStatus.Deleted)
                             {
                                 NewRepositorySerializer newRepositorySerializer = new NewRepositorySerializer();
                                 //unserialize the item
                                 RepositoryItemBase item = newRepositorySerializer.DeserializeFromFile(SCFI.Path);
                                 SCFI.Name = item.ItemName;
                             }
                             else
                             {
                                 SCFI.Name = SCFI.Path[(SCFI.Path.LastIndexOf('\\') + 1)..];
                             }
                         }
                         catch (Exception ex)
                         {

                             //TODO: fix the path changes 
                             if (SCFI.Path.Contains('\\') && (SCFI.Path.LastIndexOf('\\') + 1 < SCFI.Path.Length - 1))
                             {
                                 SCFI.Name = SCFI.Path[(SCFI.Path.LastIndexOf('\\') + 1)..];
                             }

                             Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                         }

                         if (string.IsNullOrEmpty(SCFI.Path))
                         {
                             SCFI.FileType = "";
                         }
                         else if (SCFI.Path.ToUpper().Contains("AGENTS"))
                         {
                             SCFI.FileType = "Agent";
                         }
                         else if (SCFI.Path.ToUpper().Contains("BUSINESSFLOWS"))
                         {
                             SCFI.FileType = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
                         }
                         else if (SCFI.Path.ToUpper().Contains("DOCUMENTS"))
                         {
                             SCFI.FileType = "Document";
                         }
                         else if (SCFI.Path.ToUpper().Contains("ENVIRONMENTS"))
                         {
                             SCFI.FileType = "Environment";
                         }
                         else if (SCFI.Path.ToUpper().Contains("EXECUTIONRESULTS"))
                         {
                             SCFI.FileType = "Execution Result";
                         }
                         else if (SCFI.Path.ToUpper().Contains("RUNSET"))
                         {
                             SCFI.FileType = GingerDicser.GetTermResValue(eTermResKey.RunSet);
                         }
                         else if (SCFI.Path.ToUpper().Contains("ACTIONS"))
                         {
                             SCFI.FileType = "Action";
                         }
                         else if (SCFI.Path.ToUpper().Contains("ACTIVITIESGROUPS"))
                         {
                             SCFI.FileType = GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup);
                         }
                         else if (SCFI.Path.ToUpper().Contains("ACTIVITIES"))
                         {
                             SCFI.FileType = GingerDicser.GetTermResValue(eTermResKey.Activity);
                         }
                         else if (SCFI.Path.ToUpper().Contains("VARIABLES"))
                         {
                             SCFI.FileType = GingerDicser.GetTermResValue(eTermResKey.Variable);
                         }
                         else if (SCFI.Path.Contains("ApplicationAPIModel"))
                         {
                             SCFI.FileType = "Application API Model";
                         }
                         else if (SCFI.Path.Contains("GlobalAppModelParameter"))
                         {
                             SCFI.FileType = "Global Applications Model Parameter";
                         }
                     });
                });

                CheckInFilesGrid.DataSourceList = mFiles;
            }
            finally
            {
                xProcessingIcon.Visibility = Visibility.Collapsed;
                SourceControlIntegration.BusyInProcessWhileDownloading = false;
            }

        }
        private void SelectAll(object sender, RoutedEventArgs e)
        {
            if (mFiles == null)
            {
                return;
            }

            foreach (SourceControlFileInfo SCFI in mFiles)
            {
                SCFI.Selected = true;
            }
            CheckInFilesGrid.DataSourceList = mFiles;
        }

        private void UnSelectAll(object sender, RoutedEventArgs e)
        {
            if (mFiles == null)
            {
                return;
            }

            foreach (SourceControlFileInfo SCFI in mFiles)
            {
                SCFI.Selected = false;
            }
            CheckInFilesGrid.DataSourceList = mFiles;
        }

        private async void CommitAndCheckinButton_Click(object sender, RoutedEventArgs e)
        {
            if (WorkSpace.Instance.Solution.SourceControl.Name == SourceControlBase.eSourceControlType.GIT.ToString())
            {
                if (String.IsNullOrEmpty(WorkSpace.Instance.Solution.SourceControl.AuthorName) || String.IsNullOrEmpty(WorkSpace.Instance.Solution.SourceControl.AuthorEmail))
                {
                    Reporter.ToUser(eUserMsgKey.SourceControlCommitFailed, "Please provide Author Name and Email in source control connection details page.");
                    return;
                }
            }
            try
            {
                xProcessingIcon.Visibility = Visibility.Visible;
                if (SourceControlIntegration.BusyInProcessWhileDownloading)
                {
                    Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Please wait for current process to end.");
                    return;
                }
                SourceControlIntegration.BusyInProcessWhileDownloading = true;
                List<SourceControlFileInfo> SelectedFiles = mFiles.Where(x => x.Selected == true).ToList();

                int unpushedLocalCommitsCount = 0;
                if (WorkSpace.Instance.Solution.SourceControl.GetSourceControlType == SourceControlBase.eSourceControlType.GIT)
                {
                    ObservableList<SourceControlChangesetDetails> unpushedLocalCommits = WorkSpace.Instance.Solution.SourceControl.GetUnpushedLocalCommits();
                    unpushedLocalCommitsCount = unpushedLocalCommits.Count;
                }

                if ((SelectedFiles == null || SelectedFiles.Count == 0) && (unpushedLocalCommitsCount == 0))
                {
                    Reporter.ToUser(eUserMsgKey.SourceControlMissingSelectionToCheckIn);
                    return;
                }
                if (CommentsTextBox.Text.Length == 0)
                {
                    Reporter.ToUser(eUserMsgKey.AskToAddCheckInComment);
                    return;
                }
                if (SelectedFiles != null && SelectedFiles.Count > 0 && unpushedLocalCommitsCount > 0)
                {
                    if (Reporter.ToUser(eUserMsgKey.SourceControlChkInConfirmtionForLocalCommitAndFiles, SelectedFiles.Count, unpushedLocalCommitsCount) == eUserMsgSelection.No)
                    {
                        return;
                    }
                }
                else if ((SelectedFiles == null || SelectedFiles.Count == 0) && unpushedLocalCommitsCount > 0)
                {
                    if (Reporter.ToUser(eUserMsgKey.SourceControlChkInConfirmtionForLocalCommit, unpushedLocalCommitsCount) == eUserMsgSelection.No)
                    {
                        return;
                    }
                }
                else if (Reporter.ToUser(eUserMsgKey.SourceControlChkInConfirmtionForLocalFiles, SelectedFiles.Count) == eUserMsgSelection.No)
                {
                    return;
                }

                string Comments = CommentsTextBox.Text.ToString();
                // performing on the another thread 
                await Task.Run(() =>
                {
                    try
                    {
                        App.MainWindow.Dispatcher.Invoke(() =>
                        {
                            SaveAllDirtyFiles(SelectedFiles);
                        });
                        //performing cleanup for the solution folder to clean old locks left by faild check ins
                        SourceControlIntegration.CleanUp(WorkSpace.Instance.Solution.SourceControl, WorkSpace.Instance.Solution.Folder);
                        List<string> pathsToCommit = StageTheFilesToCommit(SelectedFiles);


                        bool conflictHandled = false;
                        bool CommitSuccess = false;
                        CommitSuccess = CommitAndCheckinChanges(WorkSpace.Instance.Solution.SourceControl, pathsToCommit, Comments, WorkSpace.Instance.Solution.ShowIndicationkForLockedItems, ref conflictHandled);


                        AfterCommitAndCheckinProcess(CommitSuccess, conflictHandled);

                        if (CommitSuccess && conflictHandled)
                        {
                            TriggerSourceControlIconChanged(SelectedFiles);
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to Check in", ex);
                    }
                });
                xProcessingIcon.Visibility = Visibility.Collapsed;
                if (SourceControlIntegration.conflictFlag)
                {
                    SourceControlIntegration.conflictFlag = false;
                }
            }
            finally
            {
                xProcessingIcon.Visibility = Visibility.Collapsed;
                SourceControlIntegration.BusyInProcessWhileDownloading = false;
            }
        }
        private static bool CommitAndCheckinChanges(SourceControlBase SourceControl, ICollection<string> pathsToCommit, string Comments, bool includeLocks, ref bool conflictHandled)
        {
            string error = string.Empty;
            bool result = true;
            bool conflict = conflictHandled;
            List<string> conflictsPaths = [];
            if (!SourceControl.CommitAndCheckinChanges(pathsToCommit, Comments, ref error, ref conflictsPaths, includeLocks))
            {
                if (conflictsPaths.Count != 0)
                {
                    App.MainWindow.Dispatcher.Invoke(() =>
                    {
                        ResolveConflictWindow resConfPage = new ResolveConflictWindow(conflictsPaths);
                        if (WorkSpace.Instance.RunningInExecutionMode == true)
                        {
                            conflictsPaths.ForEach(cPath => SourceControlIntegration.ResolveConflicts(WorkSpace.Instance.Solution.SourceControl, cPath, eResolveConflictsSide.Server));
                        }
                        else
                        {
                            resConfPage.ShowAsWindow();
                        }

                        result = resConfPage.IsResolved;
                        conflict = true;
                        SourceControlIntegration.conflictFlag = conflict;
                        if (SourceControl.GetSourceControlmConflict != null)
                        {
                            SourceControl.GetSourceControlmConflict.Clear();
                        }
                    });
                    if (!conflict)
                    {
                        if (error.Contains("too many redirects or authentication replays"))
                        {
                            error = "Commit failed because of wrong credentials error, please enter valid Username and Password and try again";
                        }

                        if (error.Contains("is locked in another working copy"))
                        {
                            error = "This file has been locked by other user. Please remove lock and then try to Check in.";
                        }

                        App.MainWindow.Dispatcher.Invoke(() =>
                        {
                            Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, error);
                        });
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    //no conflicts to handle
                    conflictHandled = true;
                }
                return false;
            }
            return result;
        }

        private void TriggerSourceControlIconChanged(List<SourceControlFileInfo> selectedFiles)
        {
            parentFolders = [];
            foreach (SourceControlFileInfo fi in selectedFiles)
            {
                FileAttributes attr;
                if (fi.Status != SourceControlFileInfo.eRepositoryItemStatus.Deleted)
                {
                    attr = File.GetAttributes(fi.Path);

                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        RepositoryFolderBase repoFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryFolderByPath(fi.Path);
                        if (repoFolder != null)
                        {
                            repoFolder.RefreshFolderAndChildElementsSourceControlStatus();
                        }

                        AddToParentFoldersToRefresh(Directory.GetParent(fi.Path).FullName);
                    }
                    else
                    {
                        RepositoryItemBase repoItem = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByPath(fi.Path);
                        if (repoItem != null)
                        {
                            repoItem.RefreshSourceControlStatus();
                        }

                        AddToParentFoldersToRefresh(Path.GetDirectoryName(fi.Path));
                    }
                }
                else
                {
                    AddToParentFoldersToRefresh(Directory.GetParent(fi.Path).FullName);
                }
            }
            //refresh parent folders
            foreach (string folder in parentFolders)
            {
                WorkSpace.Instance.SolutionRepository.RefreshParentFoldersSoucerControlStatus(folder);
            }
        }

        /// <summary>
        /// Generate unique list of parent Repository folders 
        /// </summary>
        /// <param name="parentFolderPath"></param>
        public void AddToParentFoldersToRefresh(string parentFolderPath)
        {
            string standardPath = Path.GetFullPath(parentFolderPath);
            if (!parentFolders.Contains(standardPath))
            {
                //check if not already covered with exsting folder path(
                if (parentFolders.FirstOrDefault(x => x.Contains(standardPath)) == null)
                {
                    parentFolders.Add(standardPath);
                }
            }
        }

        private void AfterCommitAndCheckinProcess(bool CommitSuccess, bool conflictHandled)
        {
            this.Dispatcher.BeginInvoke(
            System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                    delegate ()
                    {
                        if (CommitSuccess && conflictHandled)
                        {
                            Init();
                            CommentsTextBox.Text = string.Empty;
                            mCheckInWasDone = true;
                        }
                        else if (!CommitSuccess && !conflictHandled)
                        {
                            Reporter.ToUser(eUserMsgKey.SourceControlChkInConflictHandledFailed);
                            CloseWindow();
                        }
                        else
                        {
                            CloseWindow();
                        }
                    }));
        }

        private void SaveAllDirtyFiles(List<SourceControlFileInfo> SelectedFiles)
        {
            foreach (SourceControlFileInfo SCFI in SelectedFiles)
            {
                Object? obj = null;

                if (SCFI.FileType == "Agent")
                {
                    obj = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>().FirstOrDefault(x => Path.GetFullPath(x.FileName) == Path.GetFullPath(SCFI.Path));
                }
                else if (SCFI.FileType == "Business Flow")
                {
                    ObservableList<BusinessFlow> businessFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
                    obj = businessFlows.FirstOrDefault(x => Path.GetFullPath(x.FileName) == Path.GetFullPath(SCFI.Path));
                }
                else if (SCFI.FileType == "Environment")
                {
                    obj = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().FirstOrDefault(x => Path.GetFullPath(x.FileName) == Path.GetFullPath(SCFI.Path));
                }
                else if (SCFI.FileType == "Run Set")
                {
                    ObservableList<RunSetConfig> RunSets = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>();
                    obj = RunSets.FirstOrDefault(x => Path.GetFullPath(x.FileName) == Path.GetFullPath(SCFI.Path));
                }
                else if (SCFI.FileType == "Action")
                {
                    ObservableList<Act> SharedActions = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Act>();
                    obj = SharedActions.FirstOrDefault(x => Path.GetFullPath(x.FileName) == Path.GetFullPath(SCFI.Path));
                }
                else if (SCFI.FileType == "Activities Group")
                {
                    ObservableList<ActivitiesGroup> activitiesGroup = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
                    obj = activitiesGroup.FirstOrDefault(x => Path.GetFullPath(x.FileName) == Path.GetFullPath(SCFI.Path));
                }
                else if (SCFI.FileType == "Activity")
                {
                    ObservableList<Activity> activities = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
                    obj = activities.FirstOrDefault(x => Path.GetFullPath(x.FileName) == Path.GetFullPath(SCFI.Path));
                }
                else if (SCFI.FileType == "Variable")
                {
                    ObservableList<VariableBase> variables = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<VariableBase>();
                    obj = variables.FirstOrDefault(x => Path.GetFullPath(x.FileName) == Path.GetFullPath(SCFI.Path));
                }

                if (obj != null && ((RepositoryItemBase)obj).DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified)
                {
                    if (Reporter.ToUser(eUserMsgKey.SourceControlCheckInUnsavedFileChecked, SCFI.Name) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                    {
                        Reporter.ToStatus(eStatusMsgKey.SaveItem, null, WorkSpace.Instance.Solution.GetNameForFileName(), "item");
                        WorkSpace.Instance.SolutionRepository.SaveRepositoryItem((RepositoryItemBase)obj);
                        Reporter.HideStatusMessage();
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, "Check in Aborted");
                        return;
                    }
                }
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Free)
        {
            ObservableList<Button> windowBtnsList = [];

            Button CommitAndCheckin = new Button
            {
                Content = "Commit and Push"
            };
            CommitAndCheckin.Click += CommitAndCheckinButton_Click;

            windowBtnsList.Add(CommitAndCheckin);


            if (WorkSpace.Instance.Solution.SourceControl.GetSourceControlType == SourceControlBase.eSourceControlType.GIT)
            {
                Button LocalCommit = new Button
                {
                    Content = "Commit Locally"
                };
                LocalCommit.Click += LocalCommitButton_Click;
                windowBtnsList.Add(LocalCommit);

                Button UndoLocalChanges = new Button
                {
                    Content = "Undo Changes"
                };
                UndoLocalChanges.Click += LocalUndoChanges_Click;
                windowBtnsList.Add(UndoLocalChanges);
            }

            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, this.Title, this, windowBtnsList, true, "Close", CloseWindow);
        }

        private void CloseWindow(object sender, EventArgs e)
        {
            CloseWindow();
        }

        private void CloseWindow()
        {
            //TODO: remove sol refresh after all RIs moved to new repo and using new tree item if mCheckInWasDone true            
            genWin.Close();
        }

        private void RefreshGrid(object sender, RoutedEventArgs e)
        {
            Init();
        }
        private void CheckInFilesGrid_grdMain_MouseDoubleClick(object sender, EventArgs e)
        {
            SourceControlFileInfo a = (SourceControlFileInfo)CheckInFilesGrid.CurrentItem;
            openDiff(a.Diff);
        }

        public void openDiff(string diff)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo("TortoiseUDiff.exe")
                {
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            try
            {
                System.Diagnostics.Process udiff = System.Diagnostics.Process.Start(p.StartInfo);
                StreamWriter myWriter = udiff.StandardInput;
                myWriter.AutoFlush = true;
                myWriter.Write(diff);
                myWriter.Close();

                p.WaitForExit();
                while (!p.HasExited)
                {
                    System.Threading.Thread.Sleep(100);
                }
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, e.Message);
            }
        }

        private async void LocalCommitButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(WorkSpace.Instance.Solution.SourceControl.AuthorName) || String.IsNullOrEmpty(WorkSpace.Instance.Solution.SourceControl.AuthorEmail))
            {
                Reporter.ToUser(eUserMsgKey.SourceControlCommitFailed, "Please provide Author Name and Email in source control connection details page.");
                return;
            }

            try
            {
                xProcessingIcon.Visibility = Visibility.Visible;
                if (SourceControlIntegration.BusyInProcessWhileDownloading)
                {
                    Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Please wait for current process to end.");
                    return;
                }
                SourceControlIntegration.BusyInProcessWhileDownloading = true;
                List<SourceControlFileInfo> SelectedFiles = mFiles.Where(x => x.Selected == true).ToList();
                if (SelectedFiles == null || SelectedFiles.Count == 0)
                {
                    Reporter.ToUser(eUserMsgKey.SourceControlMissingSelectionToLocalCommit);
                    return;
                }
                if (CommentsTextBox.Text.Length == 0)
                {
                    Reporter.ToUser(eUserMsgKey.AskToAddLocalCommitComment);
                    return;
                }

                string Comments = CommentsTextBox.Text.ToString();

                await Task.Run(() =>
                {
                    try
                    {
                        App.MainWindow.Dispatcher.Invoke(() =>
                        {
                            SaveAllDirtyFiles(SelectedFiles);
                        });
                        //performing cleanup for the solution folder to clean old locks left by faild check ins
                        SourceControlIntegration.CleanUp(WorkSpace.Instance.Solution.SourceControl, WorkSpace.Instance.Solution.Folder);
                        List<string> pathsToCommit = StageTheFilesToCommit(SelectedFiles);

                        bool CommitSuccess = CommitChanges(WorkSpace.Instance.Solution.SourceControl, Comments);


                        Dispatcher.BeginInvoke(() =>
                        {
                            if (!CommitSuccess)
                            {
                                Reporter.ToUser(eUserMsgKey.SourceControlChkInConflictHandledFailed);
                            }

                            CloseWindow();

                        });

                        if (CommitSuccess)
                        {
                            TriggerSourceControlIconChanged(SelectedFiles);
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to Check in", ex);
                    }
                });

                xProcessingIcon.Visibility = Visibility.Collapsed;
            }
            finally
            {
                xProcessingIcon.Visibility = Visibility.Collapsed;
                SourceControlIntegration.BusyInProcessWhileDownloading = false;
            }
        }

        private static List<string> StageTheFilesToCommit(List<SourceControlFileInfo> SelectedFiles)
        {
            List<string> pathsToCommit = [];
            foreach (SourceControlFileInfo fi in SelectedFiles)
            {
                switch (fi.Status)
                {
                    case SourceControlFileInfo.eRepositoryItemStatus.New:
                        SourceControlIntegration.AddFile(WorkSpace.Instance.Solution.SourceControl, fi.Path);
                        pathsToCommit.Add(fi.Path);
                        break;

                    case SourceControlFileInfo.eRepositoryItemStatus.Modified:
                        if (fi.Locked && fi.LockedOwner != WorkSpace.Instance.Solution.SourceControl.Username && Reporter.ToUser(eUserMsgKey.SourceControlCheckInLockedByAnotherUser, fi.Path, fi.LockedOwner, fi.LockComment) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                        {
                            SourceControlIntegration.UpdateFile(WorkSpace.Instance.Solution.SourceControl, fi.Path);
                            pathsToCommit.Add(fi.Path);
                        }
                        else if (fi.Locked && fi.LockedOwner == WorkSpace.Instance.Solution.SourceControl.Username && Reporter.ToUser(eUserMsgKey.SourceControlCheckInLockedByMe, fi.Path, fi.LockedOwner, fi.LockComment) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                        {
                            SourceControlIntegration.UpdateFile(WorkSpace.Instance.Solution.SourceControl, fi.Path);
                            pathsToCommit.Add(fi.Path);
                        }
                        else if (!fi.Locked)
                        {
                            SourceControlIntegration.UpdateFile(WorkSpace.Instance.Solution.SourceControl, fi.Path);
                            pathsToCommit.Add(fi.Path);
                        }
                        break;

                    case SourceControlFileInfo.eRepositoryItemStatus.ModifiedAndResolved:
                        pathsToCommit.Add(fi.Path);
                        SourceControlIntegration.UpdateFile(WorkSpace.Instance.Solution.SourceControl, fi.Path);
                        break;
                    case SourceControlFileInfo.eRepositoryItemStatus.Deleted:
                        SourceControlIntegration.DeleteFile(WorkSpace.Instance.Solution.SourceControl, fi.Path);
                        pathsToCommit.Add(fi.Path);
                        break;

                    default:
                        throw new Exception("Unknown file status to check-in - " + fi.Name);
                }
            }

            return pathsToCommit;
        }

        private static bool CommitChanges(SourceControlBase SourceControl, string Comments)
        {
            string error = string.Empty;

            bool commitSuccess = SourceControl.CommitChanges(Comments, ref error);

            if (!string.IsNullOrEmpty(error))
            {
                App.MainWindow.Dispatcher.Invoke(() =>
                {
                    Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, error);
                });
            }

            return commitSuccess;
        }

        private void RefreshLocalCommitsGrid(object sender, RoutedEventArgs e)
        {
            InitLocalCommitGrid();
        }

        private void InitLocalCommitGrid()
        {
            LocalCommitedFilesGrid.DataSourceList = WorkSpace.Instance.Solution.SourceControl.GetUnpushedLocalCommits();
            LocalCommitedFilesGrid.Title = $"Pending Local Commits for Check-In ({LocalCommitedFilesGrid.DataSourceList.Count})";
        }

        private void SetLocalCommitGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = [];
            view.GridColsView = viewCols;

            viewCols.Add(new GridColView() { Field = nameof(SourceControlChangesetDetails.ID), Header = "ID", WidthWeight = 20, AllowSorting = true });
            viewCols.Add(new GridColView() { Field = nameof(SourceControlChangesetDetails.Author), Header = "Author", WidthWeight = 15, ReadOnly = true, AllowSorting = true });
            viewCols.Add(new GridColView() { Field = nameof(SourceControlChangesetDetails.Date), Header = "Date", WidthWeight = 20, AllowSorting = true });
            viewCols.Add(new GridColView() { Field = nameof(SourceControlChangesetDetails.Message), Header = "Message", WidthWeight = 45, AllowSorting = false });

            LocalCommitedFilesGrid.SetAllColumnsDefaultView(view);
            LocalCommitedFilesGrid.InitViewItems();
        }

        private async void LocalUndoChanges_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(WorkSpace.Instance.Solution.SourceControl.AuthorName) || String.IsNullOrEmpty(WorkSpace.Instance.Solution.SourceControl.AuthorEmail))
            {
                Reporter.ToUser(eUserMsgKey.SourceControlCommitFailed, "Please provide Author Name and Email in source control connection details page.");
                return;
            }

            try
            {
                xProcessingIcon.Visibility = Visibility.Visible;
                if (SourceControlIntegration.BusyInProcessWhileDownloading)
                {
                    Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Please wait for current process to end.");
                    return;
                }
                SourceControlIntegration.BusyInProcessWhileDownloading = true;
                List<SourceControlFileInfo> SelectedFiles = mFiles.Where(x => x.Selected == true).ToList();
                if (SelectedFiles == null || SelectedFiles.Count == 0)
                {
                    Reporter.ToUser(eUserMsgKey.SourceControlMissingSelectionToLocalCommit);
                    return;
                }
                if (Reporter.ToUser(eUserMsgKey.SourceControlUndoLocalChanges, SelectedFiles.Count) == eUserMsgSelection.No)
                {

                    return;
                }

                // Performing on another thread 
                await Task.Run(() =>
                {
                    try
                    {
                        App.MainWindow.Dispatcher.Invoke(() =>
                        {
                            SaveAllDirtyFiles(SelectedFiles);
                        });

                        //performing cleanup for the solution folder to clean old locks left by faild check ins
                        SourceControlIntegration.CleanUp(WorkSpace.Instance.Solution.SourceControl, WorkSpace.Instance.Solution.Folder);
                        // Undo changes for selected filest
                        WorkSpace.Instance.Solution.SourceControl.UndoUncommitedChanges(SelectedFiles);
                        Dispatcher.BeginInvoke(() =>
                        {
                            CloseWindow();
                        });
                        //Reload of Solution
                        ReloadSolution();

                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to Undo Changes", ex);
                    }
                });

                xProcessingIcon.Visibility = Visibility.Collapsed;
                if (SourceControlIntegration.conflictFlag)
                {
                    SourceControlIntegration.conflictFlag = false;
                }
            }
            finally
            {
                xProcessingIcon.Visibility = Visibility.Collapsed;
                SourceControlIntegration.BusyInProcessWhileDownloading = false;
            }
        }
        private void ReloadSolution()
        {
            string path = WorkSpace.Instance.Solution.ContainingFolderFullPath;
            CloseSolution();
            OpenSolution(path);
        }
        private void CloseSolution()
        {
            App.MainWindow.Dispatcher.Invoke(WorkSpace.Instance.CloseSolution);
        }

        private void OpenSolution(string folder)
        {
            App.MainWindow.Dispatcher.Invoke(() =>
            {
                // TODO: do it like user with open solution page
                WorkSpace.Instance.OpenSolution(folder);
            });
        }
    }
}
