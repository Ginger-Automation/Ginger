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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger.Reports;
using Ginger.SolutionWindows;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Environments;
using GingerCore.SourceControl;
using GingerCoreNET.SourceControl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Variables;
using Ginger.Run;
using GingerCore.Activities;

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
        bool mCheckInWasDone=false;
        private List<string> parentFolders = null;

        public CheckInPage(string path)
        {
            InitializeComponent();
            SourceControlIntegration.BusyInProcessWhileDownloading = false;
            mPath = path;
            lblAnalyzedPath.Content = mPath;

            Init();

            SetCheckinGridView();
            CheckInFilesGrid.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGrid));
            CheckInFilesGrid.AddToolbarTool("@CheckAllColumn_16x16.png", "Select All", new RoutedEventHandler(SelectAll));
            CheckInFilesGrid.AddToolbarTool("@UnCheckAllColumn_16x16.png", "Un-Select All", new RoutedEventHandler(UnSelectAll));
            CheckInFilesGrid.RowDoubleClick += CheckInFilesGrid_grdMain_MouseDoubleClick;
            CommentsTextBox.Focus();
        }

        private void SetCheckinGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            view.GridColsView = viewCols;

            // TODO: use fields
            viewCols.Add(new GridColView() { Field = nameof(SourceControlFileInfo.Selected), Header = "Selected", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.CheckBox, AllowSorting = true });
            viewCols.Add(new GridColView() { Field = nameof(SourceControlFileInfo.Status), Header = "Status", WidthWeight = 10, ReadOnly=true, AllowSorting = true });
            viewCols.Add(new GridColView() { Field = nameof(SourceControlFileInfo.Name), Header = "Item Name", WidthWeight = 20, AllowSorting = true });
            viewCols.Add(new GridColView() { Field = nameof(SourceControlFileInfo.FileType), Header = "Item Type", WidthWeight = 20, AllowSorting = true });
            viewCols.Add(new GridColView() { Field = nameof(SourceControlFileInfo.SolutionPath), Header="Item Path", WidthWeight = 40, ReadOnly=true, AllowSorting = true });
            if (App.UserProfile.Solution.ShowIndicationkForLockedItems)
            {
                viewCols.Add(new GridColView() { Field = nameof(SourceControlFileInfo.Locked), Header = "Locked", WidthWeight = 10,  StyleType=GridColView.eGridColStyleType.Text });
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
                    Reporter.ToUser(eUserMsgKeys.StaticInfoMessage, "Please wait for current process to end.");
                    return;
                }
                SourceControlIntegration.BusyInProcessWhileDownloading = true;
               
                await Task.Run(() =>
                {
                    
                //set paths to ignore:
                List<string> pathsToIgnore = new List<string>();
                    pathsToIgnore.Add("PrevVersions");
                    pathsToIgnore.Add("RecentlyUsed.dat");
                    pathsToIgnore.Add("AutoSave");
                    pathsToIgnore.Add("Recover");
                    if (App.UserProfile.Solution != null && App.UserProfile.Solution.ExecutionLoggerConfigurationSetList != null && App.UserProfile.Solution.ExecutionLoggerConfigurationSetList.Count > 0)
                        pathsToIgnore.Add(Ginger.Run.ExecutionLogger.GetLoggerDirectory(App.UserProfile.Solution.ExecutionLoggerConfigurationSetList[0].ExecutionLoggerConfigurationExecResultsFolder));
                HTMLReportsConfiguration reportConfig = App.UserProfile.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
                    if (reportConfig != null)
                        pathsToIgnore.Add(Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetReportDirectory(reportConfig.HTMLReportsFolder));

                    mFiles = SourceControlIntegration.GetPathFilesStatus(App.UserProfile.Solution.SourceControl, mPath, pathsToIgnore);
                //set items name and type
                Parallel.ForEach(mFiles, SCFI =>
                     {
                         try
                         {
                             if (SCFI.Path.ToUpper().Contains(".GINGER.") && SCFI.Path.ToUpper().Contains(".XML"))
                             {
                             //try to unsearlize
                             object item = RepositoryItem.LoadFromFile(SCFI.Path);
                                 SCFI.Name = ((RepositoryItem)item).GetNameForFileName();
                             }
                             else
                                 SCFI.Name = SCFI.Path.Substring(SCFI.Path.LastIndexOf('\\') + 1);
                         }
                         catch (Exception ex)
                         {
                             if (SCFI.Path.Contains('\\') && (SCFI.Path.LastIndexOf('\\') + 1 < SCFI.Path.Length - 1))
                                 SCFI.Name = SCFI.Path.Substring(SCFI.Path.LastIndexOf('\\') + 1);
                             Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                         }
                    
                         if (string.IsNullOrEmpty(SCFI.Path)) SCFI.FileType = "";
                         else if (SCFI.Path.ToUpper().Contains("AGENTS")) SCFI.FileType = "Agent";
                         else if (SCFI.Path.ToUpper().Contains("BUSINESSFLOWS")) SCFI.FileType = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
                         else if (SCFI.Path.ToUpper().Contains("DOCUMENTS")) SCFI.FileType = "Document";
                         else if (SCFI.Path.ToUpper().Contains("ENVIRONMENTS")) SCFI.FileType = "Environment";
                         else if (SCFI.Path.ToUpper().Contains("EXECUTIONRESULTS")) SCFI.FileType = "Execution Result";
                         else if (SCFI.Path.ToUpper().Contains("RUNSET")) SCFI.FileType = GingerDicser.GetTermResValue(eTermResKey.RunSet);
                         else if (SCFI.Path.ToUpper().Contains("ACTIONS")) SCFI.FileType = "Action";
                         else if (SCFI.Path.ToUpper().Contains("ACTIVITIESGROUPS")) SCFI.FileType = GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup);
                         else if (SCFI.Path.ToUpper().Contains("ACTIVITIES")) SCFI.FileType = GingerDicser.GetTermResValue(eTermResKey.Activity);
                         else if (SCFI.Path.ToUpper().Contains("VARIABLES")) SCFI.FileType = GingerDicser.GetTermResValue(eTermResKey.Variable);
                         else if (SCFI.Path.ToUpper().Contains("REPORTTEMPLATE")) SCFI.FileType = "Report Template";
                         else if (SCFI.Path.Contains("ApplicationAPIModel")) SCFI.FileType = "Application API Model";
                         else if (SCFI.Path.Contains("GlobalAppModelParameter")) SCFI.FileType = "Global Applocations Model Parameter";
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
            if (mFiles == null) return;
            foreach (SourceControlFileInfo SCFI in mFiles)
            {
                SCFI.Selected = true;
            }
            CheckInFilesGrid.DataSourceList = mFiles;
        }

        private void UnSelectAll(object sender, RoutedEventArgs e)
        {
            if (mFiles == null) return;
            foreach (SourceControlFileInfo SCFI in mFiles)
            {
                SCFI.Selected = false;
            }
            CheckInFilesGrid.DataSourceList = mFiles;
        }
      
        private async void CheckInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                xProcessingIcon.Visibility = Visibility.Visible;
                if (SourceControlIntegration.BusyInProcessWhileDownloading)
                {
                    Reporter.ToUser(eUserMsgKeys.StaticInfoMessage, "Please wait for current process to end.");
                    return;
                }
                SourceControlIntegration.BusyInProcessWhileDownloading = true;
                List<SourceControlFileInfo> SelectedFiles = mFiles.Where(x => x.Selected == true).ToList();
                if (SelectedFiles == null || SelectedFiles.Count == 0)
                {
                    Reporter.ToUser(eUserMsgKeys.SourceControlMissingSelectionToCheckIn);
                    return;
                }
                if (CommentsTextBox.Text.Length == 0)
                {
                    Reporter.ToUser(eUserMsgKeys.AskToAddCheckInComment);
                    return;
                }
                if (Reporter.ToUser(eUserMsgKeys.SourceControlChkInConfirmtion, SelectedFiles.Count) == MessageBoxResult.No)
                    return;
                string Comments = CommentsTextBox.Text.ToString();
                // performing on the another thread 
                await Task.Run(() =>
                {
                    App.MainWindow.Dispatcher.Invoke(() =>
                    {
                        SaveAllDirtyFiles(SelectedFiles);
                    });
                //performing cleanup for the solution folder to clean old locks left by faild check ins
                SourceControlIntegration.CleanUp(App.UserProfile.Solution.SourceControl, App.UserProfile.Solution.Folder);
                    List<string> pathsToCommit = new List<string>();
                    foreach (SourceControlFileInfo fi in SelectedFiles)
                    {

                    switch (fi.Status)
                        {
                            case SourceControlFileInfo.eRepositoryItemStatus.New:
                                SourceControlIntegration.AddFile(App.UserProfile.Solution.SourceControl, fi.Path);
                                pathsToCommit.Add(fi.Path);
                                break;
                            case SourceControlFileInfo.eRepositoryItemStatus.Modified:
                                if (fi.Locked && fi.LockedOwner != App.UserProfile.Solution.SourceControl.SourceControlUser && Reporter.ToUser(eUserMsgKeys.SourceControlCheckInLockedByAnotherUser, fi.Path, fi.LockedOwner, fi.LockComment) == MessageBoxResult.Yes)
                                {
                                    SourceControlIntegration.UpdateFile(App.UserProfile.Solution.SourceControl, fi.Path);
                                    pathsToCommit.Add(fi.Path);
                                }
                                else if (fi.Locked && fi.LockedOwner == App.UserProfile.Solution.SourceControl.SourceControlUser && Reporter.ToUser(eUserMsgKeys.SourceControlCheckInLockedByMe, fi.Path, fi.LockedOwner, fi.LockComment) == MessageBoxResult.Yes)
                                {
                                    SourceControlIntegration.UpdateFile(App.UserProfile.Solution.SourceControl, fi.Path);
                                    pathsToCommit.Add(fi.Path);
                                }
                                else if (!fi.Locked)
                                {
                                    SourceControlIntegration.UpdateFile(App.UserProfile.Solution.SourceControl, fi.Path);
                                    pathsToCommit.Add(fi.Path);
                                }
                                break;
                            case SourceControlFileInfo.eRepositoryItemStatus.ModifiedAndResolved:
                                pathsToCommit.Add(fi.Path);
                                SourceControlIntegration.UpdateFile(App.UserProfile.Solution.SourceControl, fi.Path);
                                break;
                            case SourceControlFileInfo.eRepositoryItemStatus.Deleted:
                                SourceControlIntegration.DeleteFile( App.UserProfile.Solution.SourceControl, fi.Path);
                                pathsToCommit.Add(fi.Path);
                                break;
                            default:
                                throw new Exception("Unknown file status to check-in - " + fi.Name);
                        }
                    }

                    bool conflictHandled = false;
                    bool CommitSuccess = false;

                    CommitSuccess = SourceControlIntegration.CommitChanges(App.UserProfile.Solution.SourceControl, pathsToCommit, Comments, App.UserProfile.Solution.ShowIndicationkForLockedItems, ref conflictHandled);

                    AfterCommitProcess(CommitSuccess, conflictHandled);

                
                    TriggerSourceControlIconChanged(SelectedFiles);
                    
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

        private void TriggerSourceControlIconChanged(List<SourceControlFileInfo> selectedFiles)
        {
            parentFolders = new List<string>();
            foreach (SourceControlFileInfo fi in selectedFiles)
            {
                FileAttributes attr;
                if (fi.Status != SourceControlFileInfo.eRepositoryItemStatus.Deleted)
                {
                    attr = File.GetAttributes(fi.Path);

                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        RepositoryFolderBase repoFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryFolderByPath(fi.Path);
                        repoFolder.RefreshFolderAndChildElementsSourceControlStatus();

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
                if (parentFolders.Where(x => x.Contains(standardPath)).FirstOrDefault() == null)
                {
                    parentFolders.Add(standardPath);
                }
            }
        }

        private void AfterCommitProcess(bool CommitSuccess, bool conflictHandled)
        {
            this.Dispatcher.BeginInvoke(
            System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                    delegate ()
                    {
                        if (CommitSuccess && conflictHandled && Reporter.ToUser(eUserMsgKeys.SourceControlChkInConflictHandled) == MessageBoxResult.Yes)
                        {
                            Init();
                            CommentsTextBox.Text = string.Empty;
                            mCheckInWasDone = true;
                        }
                        else if (CommitSuccess && Reporter.ToUser(eUserMsgKeys.SourceControlChkInSucss) == MessageBoxResult.Yes)
                        {
                            Init();
                            CommentsTextBox.Text = string.Empty;
                            mCheckInWasDone = true;
                        }
                        else if (!CommitSuccess)
                        {
                            Reporter.ToUser(eUserMsgKeys.SourceControlChkInConflictHandledFailed);
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
                Object obj = null;
                
                if (SCFI.FileType == "Agent")
                {
                    obj = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>().Where(x => Path.GetFullPath(x.FileName) == Path.GetFullPath(SCFI.Path)).FirstOrDefault();
                }
                else if (SCFI.FileType == "Business Flow")
                {
                    ObservableList<BusinessFlow> businessFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
                    obj = businessFlows.Where(x => Path.GetFullPath(x.FileName) == Path.GetFullPath(SCFI.Path)).FirstOrDefault();
                }
                else if (SCFI.FileType == "Environment")
                {
                    obj = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().Where(x => Path.GetFullPath(x.FileName) == Path.GetFullPath(SCFI.Path)).FirstOrDefault();
                }
                else if (SCFI.FileType == "Execution Result")
                {
                    throw new NotImplementedException();
                    //FIXME                    
                }
                else if (SCFI.FileType == "Run Set")
                {
                    ObservableList<RunSetConfig> RunSets = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>();
                    obj = RunSets.Where(x => Path.GetFullPath(x.FileName) == Path.GetFullPath(SCFI.Path)).FirstOrDefault();
                }
                else if (SCFI.FileType == "Action")
                {
                    ObservableList<Act> SharedActions = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Act>();
                    obj = SharedActions.Where(x => Path.GetFullPath(x.FileName) == Path.GetFullPath(SCFI.Path)).FirstOrDefault();
                }
                else if (SCFI.FileType == "Activities Group")
                {
                    ObservableList<ActivitiesGroup> activitiesGroup = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
                    obj = activitiesGroup.Where(x => Path.GetFullPath(x.FileName) == Path.GetFullPath(SCFI.Path)).FirstOrDefault();
                }
                else if (SCFI.FileType == "Activity")
                {
                    ObservableList<Activity> activities = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
                    obj = activities.Where(x => Path.GetFullPath(x.FileName) == Path.GetFullPath(SCFI.Path)).FirstOrDefault();
                }
                else if (SCFI.FileType == "Variable")
                {
                    ObservableList<VariableBase> variables = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<VariableBase>();
                    obj = variables.Where(x => Path.GetFullPath(x.FileName) == Path.GetFullPath(SCFI.Path)).FirstOrDefault();
                }
                else if (SCFI.FileType == "Report Template")
                {
                    ObservableList<ReportTemplate>  reports = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ReportTemplate>();
                    obj = reports.Where(x => Path.GetFullPath(x.FileName) == Path.GetFullPath(SCFI.Path)).FirstOrDefault();
                }                

                if (obj != null && ((RepositoryItemBase)obj).DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified)
                {
                    if (Reporter.ToUser(eUserMsgKeys.SourceControlCheckInUnsavedFileChecked, SCFI.Name) == MessageBoxResult.Yes)
                    {
                        Reporter.ToGingerHelper(eGingerHelperMsgKey.SaveItem, null, App.UserProfile.Solution.GetNameForFileName(), "item");                        
                        WorkSpace.Instance.SolutionRepository.SaveRepositoryItem((RepositoryItemBase)obj);                        
                        Reporter.CloseGingerHelper();
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, "Check in Aborted");
                        return;
                    }
                }
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Free)
        {
            Button CheckIn = new Button();
            CheckIn.Content = "Check-In Selected Changes";
            CheckIn.Click += CheckInButton_Click;

            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, windowStyle, this.Title, this, new ObservableList<Button> { CheckIn }, true, "Close", CloseWindow);
        }

        private void CloseWindow(object sender, EventArgs e)
        {
            CloseWindow();
        }

        private void CloseWindow()
        {   
            //TODO: remove sol refresh afetr all RIs moved to new repo and using new tree item if mCheckInWasDone true            
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
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo = new ProcessStartInfo("TortoiseUDiff.exe");
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;

            try
            {
                Process udiff = Process.Start(p.StartInfo);
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
                Reporter.ToLog(eAppReporterLogLevel.ERROR, e.Message);
            }
        }
    }
}
