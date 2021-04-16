#region License
/*
Copyright © 2014-2021 European Support Limited

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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.CoreNET.TelemetryLib;
using Amdocs.Ginger.IO;
using Amdocs.Ginger.UserControls;
using Ginger.ALM;
using Ginger.AnalyzerLib;
using Ginger.BusinessFlowWindows;
using Ginger.ConfigurationsLib;
using Ginger.Functionalities;
using Ginger.GeneralLib;
using Ginger.GeneralWindows;
using Ginger.MenusLib;
using Ginger.SolutionGeneral;
using Ginger.SolutionWindows;
using Ginger.SourceControl;
using Ginger.User;
using GingerCore;
using GingerCoreNET.SolutionRepositoryLib.UpgradeLib;
using GingerCoreNET.SourceControl;
using GingerWPF;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ginger
{
    public partial class MainWindow : Window
    {
        public enum eSolutionTabType { None, BusinessFlows, Run, Configurations, Resources };
        public eSolutionTabType SelectedSolutionTab;
        
        private bool mAskUserIfToClose = true;

        ObservableList<HelpLayoutArgs> mHelpLayoutList = new ObservableList<HelpLayoutArgs>();

        public MainWindow()
        {
            InitializeComponent();            
            lblAppVersion.Content = "Version " + Amdocs.Ginger.Common.GeneralLib.ApplicationInfo.ApplicationVersion;
            xVersionAndNewsIcon.Visibility = Visibility.Collapsed;

            mHelpLayoutList.CollectionChanged += MHelpLayoutList_CollectionChanged;

            Telemetry.eventHandler += TelemetryEventHandler;
            GingerCore.General.DoEvents();            
        }

        private void TelemetryEventHandler(object sender, Telemetry.TelemetryEventArgs e)
        {
            this.Dispatcher.Invoke(() => 
            {
                xVersionAndNewsIcon.ToolTip = Telemetry.VersionAndNewsInfo + ", click for details";
                xVersionAndNewsIcon.Visibility = Visibility.Visible;
            });
            
            
        }

        private void XVersionAndNewsIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            xVersionAndNewsIcon.Visibility = Visibility.Collapsed;
            VersionAndNewsPage versionAndNewsPage = new VersionAndNewsPage();
            versionAndNewsPage.ShowAsWindow();
        }

        public void Init()
        {
            bool autoLoadSolDone = false;

            try
            {                
                //App
                App.AutomateBusinessFlowEvent += App_AutomateBusinessFlowEvent;

                //User Profile
                WorkSpace.Instance.PropertyChanged += Workspace_PropertyChanged;
                WorkSpace.Instance.PropertyChanged += WorkSpacePropertyChanged;
                if ( WorkSpace.Instance.UserProfile.GingerStatus == eGingerStatus.Active)
                {
                    Reporter.ToStatus(eStatusMsgKey.ExitMode);
                }
                 WorkSpace.Instance.UserProfile.GingerStatus = eGingerStatus.Active;
                 WorkSpace.Instance.UserProfile.SaveUserProfile();
                 WorkSpace.Instance.UserProfile.RecentSolutionsAsObjects.CollectionChanged += RecentSolutionsObjects_CollectionChanged;

                //Main Menu                            
                xGingerIconImg.ToolTip = Amdocs.Ginger.Common.GeneralLib.ApplicationInfo.ApplicationName + Environment.NewLine + "Version " + Amdocs.Ginger.Common.GeneralLib.ApplicationInfo.ApplicationVersionWithInfo;
                SetSolutionDependedUIElements();
                UpdateUserDetails();
                if ( WorkSpace.Instance.UserProfile.RecentSolutionsAsObjects.Count > 0)
                {
                    xRecentSolutionsMenuItem.Visibility = Visibility.Visible;
                }
                xBusinessFlowsListItemText.Text = GingerDicser.GetTermResValue(eTermResKey.BusinessFlows).ToUpper();
                xRunListItemText.Text = GingerDicser.GetTermResValue(eTermResKey.RunSets).ToUpper();

                //Status Bar            
                xLogErrorsPnl.Visibility = Visibility.Collapsed;
                xProcessMsgPnl.Visibility = Visibility.Collapsed;                
                WorkSpace.Instance.BetaFeatures.PropertyChanged += BetaFeatures_PropertyChanged;
                SetBetaFlagIconVisibility();
                lblVersion.Content = "Version " + Amdocs.Ginger.Common.GeneralLib.ApplicationInfo.ApplicationVersionWithInfo;

                //Solution                  
                if ( WorkSpace.Instance.UserProfile.AutoLoadLastSolution &&  WorkSpace.Instance.RunningInExecutionMode == false && WorkSpace.Instance.RunningFromUnitTest == false)
                {
                    AutoLoadLastSolution();
                    autoLoadSolDone = true;
                }

                //Messages
                if ( WorkSpace.Instance.UserProfile.NewHelpLibraryMessgeShown == false)
                {
                    Reporter.ToStatus(eStatusMsgKey.GingerHelpLibrary);
                     WorkSpace.Instance.UserProfile.NewHelpLibraryMessgeShown = true;
                }


                Reporter.ReporterData.PropertyChanged += ReporterDataChanged;                

            }
            catch (Exception ex)
            {                
                Reporter.ToUser(eUserMsgKey.ApplicationInitError, ex.Message);
                Reporter.ToLog(eLogLevel.ERROR, "Error in Init Main Window", ex);                
            }
            finally
            {
                HideSplash();
                if (autoLoadSolDone == false && WorkSpace.Instance.Solution == null)
                {
                    AddHelpLayoutToShow("MainWindow_AddSolutionHelp", xSolutionSelectionMainMenuItem, "Click here to create new Solution or to open / download an existing one");
                }
            }
        }

      private void ReporterDataChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ReporterData.ErrorCounter))
                {
                this.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                 delegate ()
                 {
                     if (Reporter.ReporterData.ErrorCounter == 0)
                     {
                         xLogErrorsPnl.Visibility = Visibility.Collapsed;

                     }
                     else
                     {
                         xLogErrorsPnl.Visibility = Visibility.Visible;
                         xLogErrorsLbl.Content = "[" + Reporter.ReporterData.ErrorCounter + "]";
                         xLogErrorsPnl.ToolTip = Reporter.ReporterData.ErrorCounter + " Errors were logged to Ginger log, click to view log file";
                     }
                 }
              ));               
            }
        }
      
     
        private void BetaFeatures_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WorkSpace.Instance.BetaFeatures.IsUsingBetaFeatures))
            {
                SetBetaFlagIconVisibility();
            }
        }

        private void SetBetaFlagIconVisibility()
        {
            if (WorkSpace.Instance.BetaFeatures.IsUsingBetaFeatures)
            {
                xBetaFeaturesIcon.Visibility = Visibility.Visible;
            }
            else
            {
                xBetaFeaturesIcon.Visibility = Visibility.Collapsed;
            }
        }

        private void Workspace_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WorkSpace.LoadingSolution))
            {
                if (WorkSpace.Instance.LoadingSolution)
                {
                    xNoLoadedSolutionImg.Visibility = Visibility.Collapsed;
                    xMainWindowFrame.Content = new LoadingPage("Loading Solution...");
                    xMainWindowFrame.Visibility = Visibility.Visible;                    
                    GingerCore.General.DoEvents();
                }
                //else if (xMainWindowFrame.Content is LoadingPage && SelectedSolutionTab == eSolutionTabType.None)
                else if (WorkSpace.Instance.Solution == null)
                {
                    xMainWindowFrame.Visibility = Visibility.Collapsed;
                    xNoLoadedSolutionImg.Visibility = Visibility.Visible;
                }
            }
        }

        private void SetRecentSolutionsAsMenuItems()
        {
            //delete all shown Recent Solutions menu items
            for (int i = 0; i < xSolutionSelectionMainMenuItem.Items.Count; i++)
            {
                if (((MenuItem)xSolutionSelectionMainMenuItem.Items[i]).Tag is Solution)
                {
                    xSolutionSelectionMainMenuItem.Items.RemoveAt(i);
                    i--;
                }
            }

            if (xRecentSolutionsMenuItem.Tag != null)
            {
                //Insert
                int insertIndex = xSolutionSelectionMainMenuItem.Items.IndexOf(xRecentSolutionsMenuItem) + 1;
                if ( WorkSpace.Instance.UserProfile.RecentSolutionsAsObjects.Count > 0)
                {
                    xRecentSolutionsMenuItem.Visibility = Visibility.Visible;

                    foreach (Solution sol in  WorkSpace.Instance.UserProfile.RecentSolutionsAsObjects)
                    {
                        AddSubMenuItem(xSolutionSelectionMainMenuItem, sol.Name, sol, RecentSolutionSelection_Click, insertIndex++, sol.Folder, eImageType.Solution);
                    }
                }
                else
                {
                    xRecentSolutionsMenuItem.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void RecentSolutionsObjects_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (xRecentSolutionsMenuItem.Tag != null) //means it is expanded
            {
                SetRecentSolutionsAsMenuItems();
            }
        }

        private void xRecentSolutionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (xRecentSolutionsMenuItem.Tag == null)
            {
                xRecentSolutionsMenuItem.Tag = true;//expanded
            }
            else
            {
                xRecentSolutionsMenuItem.Tag = null;
            }

            SetRecentSolutionsAsMenuItems();
        }


        // New method to set staus bar text and icon
        internal void ShowStatus(eStatusMsgType messageType, string statusText)
        {
            this.Dispatcher.Invoke(() => {
                if (!string.IsNullOrEmpty(statusText))
                {
                    xProcessMsgPnl.Visibility = Visibility.Visible;
                    xProcessMsgTxtBlock.Text = statusText;
                    xProcessMsgTxtBlock.ToolTip = statusText;

                    switch(messageType)
                    {
                        case eStatusMsgType.PROCESS:
                            xProcessMsgIcon.ImageType = eImageType.Processing;
                            break;

                        case eStatusMsgType.INFO:
                            xProcessMsgIcon.ImageType = eImageType.Info;
                            break;                                                
                    }
                    
                    GingerCore.General.DoEvents();
                }
                else
                {
                    xProcessMsgPnl.Visibility = Visibility.Collapsed;
                }
            });
            
        }


        internal void AutoLoadLastSolution()
        {
            try
            {
                if ( WorkSpace.Instance.UserProfile.RecentSolutionsAsObjects.Count > 0)
                {
                    WorkSpace.Instance.OpenSolution( WorkSpace.Instance.UserProfile.RecentSolutionsAsObjects[0].Folder);
                    xSolutionTabsListView.SelectedItem = null;
                    xSolutionTabsListView.SelectedItem = xBusinessFlowsListItem;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.SolutionLoadError, ex);
            }
        }
        
        public void WorkSpacePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Handle Solution change
            //TODO: cleanup close current biz flow etc...
            if (e.PropertyName == nameof(WorkSpace.Solution))
            {
                SetSolutionDependedUIElements();
                if ( WorkSpace.Instance.Solution == null)
                {
                    xSolutionTabsListView.SelectedItem = null;
                    xSolutionNameTextBlock.Text = "Please Load Solution";                
                }
                else
                {
                    xNoLoadedSolutionImg.Visibility = Visibility.Collapsed;

                    GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xSolutionNameTextBlock, TextBlock.TextProperty,  WorkSpace.Instance.Solution, nameof(Solution.Name), System.Windows.Data.BindingMode.OneWay);
                    GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xSolutionNameTextBlock, TextBlock.ToolTipProperty,  WorkSpace.Instance.Solution, nameof(Solution.Folder), System.Windows.Data.BindingMode.OneWay);
                    xSolutionTabsListView.SelectedItem = null;
                    xSolutionTabsListView.SelectedItem = xBusinessFlowsListItem;
                }
            }
        }

        public void CloseWithoutAsking()
        {
            mAskUserIfToClose = false;
            this.Close();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {            
            if (mAskUserIfToClose == false || Reporter.ToUser(eUserMsgKey.AskIfSureWantToClose) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                AppCleanUp();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void AppCleanUp()
        {
            Telemetry.eventHandler -= TelemetryEventHandler;

            ClosingWindow CW = new ClosingWindow();
            CW.Show();
            GingerCore.General.DoEvents();

            if(WorkSpace.Instance!=null && WorkSpace.Instance.Solution!=null && WorkSpace.Instance.Solution.LoggerConfigurations!=null)
            {
                while (WorkSpace.Instance.Solution.LoggerConfigurations.IsPublishToCentralDBRunning == true)
                {
                    Thread.Sleep(500);
                    GingerCore.General.DoEvents();
                }
            }           

            WorkSpace.Instance.Close();

            GingerCore.General.CleanDirectory(GingerCore.Actions.Act.ScreenshotTempFolder, true);


            CW.Close();
        }

       

        private void xSolutionTopNavigationListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xMainWindowFrame == null)
            {
                return;
            }

            SelectedSolutionTab = eSolutionTabType.None;
            if (!(xMainWindowFrame.Content is LoadingPage))
            {
                xMainWindowFrame.Visibility = Visibility.Collapsed;
            }
            ListViewItem selectedTopListItem = (ListViewItem)xSolutionTabsListView.SelectedItem;

            if (selectedTopListItem != null)
            {
                if (selectedTopListItem == xBusinessFlowsListItem)
                {
                    if (xBusinessFlowsListItem.Tag == null)
                    {
                        xBusinessFlowsListItem.Tag = new BusinessFlowsAutomatePage();
                    }
                    SelectedSolutionTab = eSolutionTabType.BusinessFlows;
                }
                else if (selectedTopListItem == xRunListItem)
                {
                    if (xRunListItem.Tag == null)
                    {
                        xRunListItem.Tag = RunMenu.MenusPage;
                    }
                    SelectedSolutionTab = eSolutionTabType.Run;
                }
                else if (selectedTopListItem == xConfigurationsListItem)
                {
                    if (xConfigurationsListItem.Tag == null)
                    {
                        xConfigurationsListItem.Tag = ConfigurationsMenu.MenusPage;
                    }
                    SelectedSolutionTab = eSolutionTabType.Configurations;
                }
                else
                {
                    if (xResourcesListItem.Tag == null)
                    {
                        xResourcesListItem.Tag = ResourcesMenu.MenusPage;
                    }
                    SelectedSolutionTab = eSolutionTabType.Resources;
                }

                xMainWindowFrame.Content = selectedTopListItem.Tag;
                xMainWindowFrame.Visibility = Visibility.Visible;
            }
        }

       
        private void xOpenSolutionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string solutionFolder = General.OpenSelectFolderDialog("Select Ginger Solution Folder");
            if (solutionFolder != null)
            {
                string solutionFileName = System.IO.Path.Combine(solutionFolder, @"Ginger.Solution.xml");
                if (System.IO.File.Exists(PathHelper.GetLongPath(solutionFileName)))
                {
                    WorkSpace.Instance.OpenSolution(Path.GetDirectoryName(PathHelper.GetLongPath(solutionFolder)));                  
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.SolutionFileNotFound, solutionFileName);
                }
            }
        }

        private void xCreateNewSolutionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Solution s1 = new Solution();
            AddSolutionPage addSol = new AddSolutionPage(s1);
            addSol.ShowAsWindow();        
        }

        public void SetSolutionDependedUIElements()
        {
            if ( WorkSpace.Instance.Solution != null)
            {
                xLoadedSolutionMenusPnl.Visibility = Visibility.Visible;
                if ( WorkSpace.Instance.UserProfile.UserTypeHelper.IsSupportAutomate)
                {
                    xRunListItem.Visibility = Visibility.Visible;
                }
                else
                {
                    xRunListItem.Visibility = Visibility.Collapsed;
                }
                if ( WorkSpace.Instance.Solution.SourceControl != null)
                {
                    xSolutionSourceControlMenu.Visibility = Visibility.Visible;
                }
                else
                {
                    xSolutionSourceControlMenu.Visibility = Visibility.Collapsed;
                }

            }
            else
            {
                xLoadedSolutionMenusPnl.Visibility = Visibility.Collapsed;
            }
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Alt + CTRL + Shift + G = show beta features
            if (e.Key == Key.G && Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.LeftShift) && Keyboard.IsKeyDown(Key.LeftAlt))
            {
                BetaFeaturesPage p = new BetaFeaturesPage();
                p.ShowAsWindow();
            }
            else if (e.Key == Key.F && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                SolutionFindAndReplace();
            }
        }

        private void ALMConfigButton_Click(object sender, RoutedEventArgs e)
        {
            ALMConnectionPage almConnPage = new ALMConnectionPage(ALMIntegration.eALMConnectType.SettingsPage);
            almConnPage.ShowAsWindow();
        }

        private void ALMFieldsConfiguration_Click(object sender, RoutedEventArgs e)
        {
            ALMIntegration.Instance.OpenALMItemsFieldsPage();
        }

        private void ALMDefectsProfiles_Click(object sender, RoutedEventArgs e)
        {
            //open defect profile page for each almtype
            ALMDefectsProfilesPage defectsProfilesPage = new ALMDefectsProfilesPage();
            defectsProfilesPage.ShowAsWindow();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            App.MainWindow.Close();
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {            
            AboutPage AP = new AboutPage();
            AP.ShowAsWindow();
        }

        private void ViewSolutionFiles_Click(object sender, RoutedEventArgs e)
        {
            //show solution folder files
            if ( WorkSpace.Instance.Solution != null)
                Process.Start( WorkSpace.Instance.Solution.Folder);
        }

        private void btnSourceControlConnectionDetails_Click(object sender, RoutedEventArgs e)
        {
            SourceControlConnDetailsPage p = new SourceControlConnDetailsPage();
            p.ShowAsWindow(eWindowShowStyle.Dialog);
        }

        private void xDownloadSolutionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SourceControlProjectsPage p = new SourceControl.SourceControlProjectsPage();
            p.ShowAsWindow();
        }

        private void btnSourceControlCheckIn_Click(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.LoseChangesWarn) == Amdocs.Ginger.Common.eUserMsgSelection.No) return;            
            App.CheckIn( WorkSpace.Instance.Solution.Folder);            
        }

        private void btnSourceControlGetLatest_Click(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.LoseChangesWarn) == Amdocs.Ginger.Common.eUserMsgSelection.No) return;

            Reporter.ToStatus(eStatusMsgKey.GetLatestFromSourceControl);
            if (string.IsNullOrEmpty( WorkSpace.Instance.Solution.Folder))
                Reporter.ToUser(eUserMsgKey.SourceControlUpdateFailed, "Invalid Path provided");
            else
                SourceControlUI.GetLatest( WorkSpace.Instance.Solution.Folder,  WorkSpace.Instance.Solution.SourceControl);

            App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.UpdateAppAgentsMapping,null);
            Reporter.HideStatusMessage();
            
        }

        private void AnalyzerButton_Click(object sender, RoutedEventArgs e)
        {            
            AnalyzerPage AP = new AnalyzerPage();
            AP.Init( WorkSpace.Instance.Solution);
            AP.ShowAsWindow();            
        }

        private void ResolveSourceControlConflicts(eResolveConflictsSide side)
        {            
            Reporter.ToStatus(eStatusMsgKey.ResolveSourceControlConflicts);
            SourceControlIntegration.ResolveConflicts( WorkSpace.Instance.Solution.SourceControl,  WorkSpace.Instance.Solution.Folder, side);
            Reporter.HideStatusMessage();
        }

        private void ResolveConflictsLocalMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ResolveSourceControlConflicts(eResolveConflictsSide.Local);
        }

        private void ResolveConflictsServerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ResolveSourceControlConflicts(eResolveConflictsSide.Server);
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            General.ShowGingerHelpWindow();
        }

        private void btnUpgrade_Click(object sender, RoutedEventArgs e)
        {
            if ( WorkSpace.Instance.Solution != null)
            {
                Solution sol =  WorkSpace.Instance.Solution;
                SolutionUpgrade.ClearPreviousScans();
                ConcurrentBag<string> lowerVersionFiles = SolutionUpgrade.GetSolutionFilesCreatedWithRequiredGingerVersion(SolutionUpgrade.GetSolutionFilesWithVersion(Solution.SolutionFiles(sol.Folder)), eGingerVersionComparisonResult.LowerVersion);
                if (lowerVersionFiles != null && lowerVersionFiles.Count > 0)
                {
                    UpgradePage solutionUpgradePage = new UpgradePage(SolutionUpgradePageViewMode.UpgradeSolution, sol.Folder, sol.Name, lowerVersionFiles.ToList());
                    solutionUpgradePage.ShowAsWindow();
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Upgrade is not needed, all solution items were created with latest version.");
                }
            }
        }

        private void btnRecover_Click(object sender, RoutedEventArgs e)
        {            
            if ( WorkSpace.Instance.Solution != null)
            {
                WorkSpace.Instance.AppSolutionRecover.SolutionRecoverStart(true);
            }
        }

        private void btnViewLog_Click(object sender, RoutedEventArgs e)
        {
            ShowGingerLog();
        }

        LogDetailsPage mLogDetailsPage = null;
        private void btnViewLogDetails_Click(object sender, RoutedEventArgs e)
        {
            if (mLogDetailsPage == null)
            {
                mLogDetailsPage = new LogDetailsPage(LogDetailsPage.eLogShowLevel.ALL);
            }
            else
            {
                mLogDetailsPage.Refresh();
            }
            mLogDetailsPage.ShowAsWindow();
        }

        private void ShowGingerLog()
        {                        
            if (System.IO.File.Exists(Amdocs.Ginger.CoreNET.log4netLib.GingerLog.GingerLogFile))
            {
                Process.Start(Amdocs.Ginger.CoreNET.log4netLib.GingerLog.GingerLogFile);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Ginger log file was not found in the Path:'" + Amdocs.Ginger.CoreNET.log4netLib.GingerLog.GingerLogFile + "'");
            }
        }

        private void btnSourceControlRepositoryDetails_Click(object sender, RoutedEventArgs e)
        {

            SourceControlItemInfoDetails SCIInfoDetails = SourceControlIntegration.GetRepositoryInfo( WorkSpace.Instance.Solution.SourceControl);
            if (SCIInfoDetails != null)
            {
                SourceControlItemInfoPage SCIIP = new SourceControlItemInfoPage(SCIInfoDetails);
                SCIIP.ShowAsWindow();
            }
        }

        private void btnViewLogLocation_Click(object sender, RoutedEventArgs e)
        {
                        
            string folder = System.IO.Path.GetDirectoryName(Amdocs.Ginger.CoreNET.log4netLib.GingerLog.GingerLogFile);
            if (System.IO.Directory.Exists(folder))
            {
                Process.Start(folder);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Ginger log file folder was not found in the path: '" + folder + "'");
            }
        }

        DebugConsoleWindow mDebugConsoleWin = null;
        private void btnLaunchConsole_Click(object sender, RoutedEventArgs e)
        {
            Reporter.ReportAllAlsoToConsole = true;

            if (mDebugConsoleWin == null)
            {
                mDebugConsoleWin = new DebugConsoleWindow();
            }
            else
            {
                mDebugConsoleWin.ClearConsole();
            }

            mDebugConsoleWin.ShowAsWindow();
        }

        private void xBetaFeaturesIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {            
            BetaFeaturesPage p = new BetaFeaturesPage();
            p.ShowAsWindow();
        }

        private void xLogErrors_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {            
            LogDetailsPage logDetailsPage = new LogDetailsPage(LogDetailsPage.eLogShowLevel.ERROR);
            logDetailsPage.ShowAsWindow();

            xLogErrorsPnl.Visibility = Visibility.Collapsed;            
            Reporter.ReporterData.ResetErrorCounter();
        }

        private void xFindAndReplaceSolutionButton_Click(object sender, RoutedEventArgs e)
        {
            SolutionFindAndReplace();
        }

        FindAndReplacePage mfindAndReplacePageSolution = null;
        private void SolutionFindAndReplace()
        {
            if (mfindAndReplacePageSolution == null)
            {
                mfindAndReplacePageSolution = new FindAndReplacePage(FindAndReplacePage.eContext.SolutionPage);
            }
            mfindAndReplacePageSolution.ShowAsWindow();
        }

        private void App_AutomateBusinessFlowEvent(AutomateEventArgs args)
        {
            if (args.EventType == AutomateEventArgs.eEventType.Automate)
            {
                //TODO: load Business Flows tab
                xSolutionTabsListView.SelectedItem = xBusinessFlowsListItem;
                //App.BusinessFlow = (BusinessFlow)args.Object;
                //App.BusinessFlow.SaveBackup();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        private void xSolutionEditBtn_Click(object sender, RoutedEventArgs e)
        {
            string newName =  WorkSpace.Instance.Solution.Name;
            if (GingerCore.GeneralLib.InputBoxWindow.GetInputWithValidation("Solution Rename", "New Solution Name:", ref newName))
            {
                 WorkSpace.Instance.Solution.Name = newName;
            }
        }

        private void xLoadSupportSiteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://ilrnaginger01/");
        }

        private void xLoadForumSiteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://ilrnaginger01:81/");
        }

        private void xGingerGithubMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Ginger-Automation");
        }

        private void xOpenTicketMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://ilrnaginger01/Ticket/Create");
        }

        private void xSupportTeamMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("mailto:AmdocsTestingGingerDVCISupport@int.amdocs.com");
        }

        private void xCoreTeamMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("mailto:GingerCoreTeam@int.amdocs.com");
        }

        private void RecentSolutionSelection_Click(object sender, RoutedEventArgs e)
        {
            Solution selectedSol = (Solution)((MenuItem)sender).Tag;

            if (selectedSol != null && Directory.Exists(selectedSol.Folder))
            {
                WorkSpace.Instance.OpenSolution(selectedSol.Folder);                
            }
            else
                Reporter.ToUser(eUserMsgKey.SolutionLoadError, "Selected Solution was not found");

            e.Handled = true;
        }

        UserSettingsPage mUserSettingsPage;
        private void xUserSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (mUserSettingsPage == null)
            {
                mUserSettingsPage = new UserSettingsPage();
            }

            mUserSettingsPage.ShowAsWindow();
        }

        UserProfilePage mUserProfilePage;
        private void xUserProfileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (mUserProfilePage == null)
            {
                mUserProfilePage = new UserProfilePage();
            }

            mUserProfilePage.ShowAsWindow();
            UpdateUserDetails();
        }

        private void UpdateUserDetails()
        {
            if (string.IsNullOrEmpty( WorkSpace.Instance.UserProfile.ProfileImage))
            {
                xProfileImageImgBrush.ImageSource = ImageMakerControl.GetImageSource(Amdocs.Ginger.Common.Enums.eImageType.User, foreground: (System.Windows.Media.SolidColorBrush)FindResource("$BackgroundColor_LightGray"), width: 50);
            }
            else
            {
                xProfileImageImgBrush.ImageSource = Ginger.General.GetImageStream(Ginger.General.Base64StringToImage( WorkSpace.Instance.UserProfile.ProfileImage));
            }

            if (String.IsNullOrEmpty( WorkSpace.Instance.UserProfile.UserFirstName))
            {
                xUserNameLbl.Content =  WorkSpace.Instance.UserProfile.UserName;
            }
            else
            {
                xUserNameLbl.Content =  WorkSpace.Instance.UserProfile.UserFirstName;
            }
        }

        private void xLogOptionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (xLogOptionsMenuItem.Tag == null)
            {
                xLogOptionsMenuItem.Tag = "Expanded";//expanded
            }
            else
            {
                xLogOptionsMenuItem.Tag = null;
            }

            SetLogOptionsMenuItems();
        }

        private void SetLogOptionsMenuItems()
        {
            //delete all shown Log options sub menu items
            for (int i = 0; i < xUserOperationsMainMenuItem.Items.Count; i++)
            {
                if ((string)((MenuItem)xUserOperationsMainMenuItem.Items[i]).Tag == "Log")
                {
                    xUserOperationsMainMenuItem.Items.RemoveAt(i);
                    i--;
                }
            }

            if (xLogOptionsMenuItem.Tag != null)
            {
                //Insert
                int insertIndex = xUserOperationsMainMenuItem.Items.IndexOf(xLogOptionsMenuItem) + 1;

                AddSubMenuItem(xUserOperationsMainMenuItem, "View Current Log Details", "Log", btnViewLogDetails_Click, insertIndex++, iconType: eImageType.View);
                AddSubMenuItem(xUserOperationsMainMenuItem, "Open Ginger Console Window", "Log", btnLaunchConsole_Click, insertIndex, iconType: eImageType.Window);
                AddSubMenuItem(xUserOperationsMainMenuItem, "Open Full Log File", "Log", btnViewLog_Click, insertIndex++, iconType: eImageType.File);
                AddSubMenuItem(xUserOperationsMainMenuItem, "Open Log File Folder", "Log", btnViewLogLocation_Click, insertIndex++, iconType: eImageType.OpenFolder);                
            }
        }

        private void AddSubMenuItem(MenuItem parentMenuItem, string itemHeader, object itemTag, RoutedEventHandler clickEventHandler, int insertIndex, string toolTip = "", eImageType iconType = eImageType.Null)
        {
            MenuItem subMenuItem = new MenuItem();
            subMenuItem.Style = (Style)TryFindResource("$MenuItemStyle_ButtonSubMenuItem");
            subMenuItem.Header = itemHeader;
            subMenuItem.Tag = itemTag;
            subMenuItem.Click += clickEventHandler;
            if (!string.IsNullOrEmpty(toolTip))
            {
                subMenuItem.ToolTip = toolTip;
            }
            if (iconType != eImageType.Null)
            {
                //< usercontrols:ImageMakerControl SetAsFontImageWithSize = "16" ImageType = "Edit" />
                ImageMakerControl imageMaker = new ImageMakerControl();
                imageMaker.SetAsFontImageWithSize = 16;
                imageMaker.ImageType = iconType;
                subMenuItem.Icon = imageMaker;
            }
            parentMenuItem.Items.Insert(insertIndex, subMenuItem);
        }

        private void xSupportOptionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (xSupportOptionsMenuItem.Tag == null)
            {
                xSupportOptionsMenuItem.Tag = "Expanded";//expanded
            }
            else
            {
                xSupportOptionsMenuItem.Tag = null;
            }

            SetSupportOptionsMenuItems();
        }

        private void SetSupportOptionsMenuItems()
        {
            //delete all Support options Sub menu items
            for (int i = 0; i < xExtraOperationsMainMenuItem.Items.Count; i++)
            {
                if ((string)((MenuItem)xExtraOperationsMainMenuItem.Items[i]).Tag == "Support")
                {
                    xExtraOperationsMainMenuItem.Items.RemoveAt(i);
                    i--;
                }
            }

            if (xSupportOptionsMenuItem.Tag != null)
            {
                //Insert
                int insertIndex = xExtraOperationsMainMenuItem.Items.IndexOf(xSupportOptionsMenuItem) + 1;

                AddSubMenuItem(xExtraOperationsMainMenuItem, "Ginger Public Site", "Support", xLoadPublicSiteMenuItem_Click, insertIndex++, iconType: eImageType.Website);               
                AddSubMenuItem(xExtraOperationsMainMenuItem, "Ginger Support Site", "Support", xLoadSupportSiteMenuItem_Click, insertIndex++, iconType: eImageType.Website);
                AddSubMenuItem(xExtraOperationsMainMenuItem, "Ginger Q&A Fourm Site", "Support", xLoadForumSiteMenuItem_Click, insertIndex++, iconType: eImageType.Forum);
                AddSubMenuItem(xExtraOperationsMainMenuItem, "Raise Ticket to Core Team", "Support", xOpenTicketMenuItem_Click, insertIndex++, iconType: eImageType.Ticket);                
            }
        }

        private void xContactOptionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (xContactOptionsMenuItem.Tag == null)
            {
                xContactOptionsMenuItem.Tag = "Expanded";//expanded
            }
            else
            {
                xContactOptionsMenuItem.Tag = null;
            }

            SetContactOptionsMenuItems();
        }

        private void SetContactOptionsMenuItems()
        {
            //delete all Support options Sub menu items
            for (int i = 0; i < xExtraOperationsMainMenuItem.Items.Count; i++)
            {
                if ((string)((MenuItem)xExtraOperationsMainMenuItem.Items[i]).Tag == "Contact")
                {
                    xExtraOperationsMainMenuItem.Items.RemoveAt(i);
                    i--;
                }
            }

            if (xContactOptionsMenuItem.Tag != null)
            {
                //Insert
                int insertIndex = xExtraOperationsMainMenuItem.Items.IndexOf(xContactOptionsMenuItem) + 1;

                AddSubMenuItem(xExtraOperationsMainMenuItem, "Contact Support Team", "Contact", xSupportTeamMenuItem_Click, insertIndex++, "AmdocsTestingGingerDVCISupport@int.amdocs.com", iconType: eImageType.Email);
                AddSubMenuItem(xExtraOperationsMainMenuItem, "Contact Core Team", "Contact", xCoreTeamMenuItem_Click, insertIndex, "GingerCoreTeam@int.amdocs.com", iconType: eImageType.Email);
            }
        }

        SolutionPage mSolutionPage;
        private void EditSolutionDetailsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (mSolutionPage == null)
            {
                mSolutionPage = new SolutionPage();
            }

            mSolutionPage.ShowAsWindow();
        }

        private void xCheckForUpgradeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://ginger.amdocs.com/#downloads");
        }

        private void xLoadPublicSiteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://ginger.amdocs.com/");
        }

        internal void LoadingInfo(string text)
        {
            ShowStatus(eStatusMsgType.PROCESS, text);
            GingerCore.General.DoEvents();
        }

        void HideSplash()
        {            
            // Hide the splash after one second
                this.Dispatcher.Invoke(() => {
                    if (xSplashGrid.Visibility == Visibility.Collapsed)
                    {
                        return;
                    }
                    Thread.Sleep(1000);
                    xSplashGrid.Visibility = Visibility.Collapsed;
                });
        }

        public void AddHelpLayoutToShow(string helpLayoutKey, FrameworkElement focusedControl, string helpText)
        {
            if (WorkSpace.Instance.RunningInExecutionMode)
            {
                return;//not showing help in automatic run mode
            }
            HelpLayoutArgs helpLayoutArgs = new HelpLayoutArgs(helpLayoutKey, focusedControl, helpText);
            if (WorkSpace.Instance.UserProfile.ShownHelpLayoutsKeys.Contains(helpLayoutArgs.HelpLayoutKey) == false)
            {
                mHelpLayoutList.Add(helpLayoutArgs);
            }
        }

        private void MHelpLayoutList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (mHelpLayoutList.Count > 0) // check if help were loaded and need to  be shown
            {
                ShowHelpLayout();
            }
        }

        private void ShowHelpLayout()
        {
            if (mHelpLayoutList.Count == 0)
            {
                return;
            }

            HelpLayoutArgs helpArgs = mHelpLayoutList[0];

            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    //--general canvas setup                
                    xHelpLayoutCanvas.Width = xMainWindowPnl.ActualWidth;
                    xHelpLayoutCanvas.Height = xMainWindowPnl.ActualHeight;

                    //---get control to focus details
                    FrameworkElement controlToFocus = helpArgs.FocusedControl;
                    double controlToFocusWidth = controlToFocus.ActualWidth;
                    double controlToFocusHeight = controlToFocus.ActualHeight;
                    Point controlToFocusLocation = controlToFocus.TransformToAncestor(App.MainWindow).Transform(new Point(0, 0));

                    //---set background rectangles
                    double gapSize = 0.25;
                    xHelpLayoutRectangleLeft.Width = controlToFocusLocation.X + gapSize;
                    xHelpLayoutRectangleLeft.Height = xMainWindowPnl.ActualHeight;

                    xHelpLayoutRectangleRight.SetValue(Canvas.LeftProperty, controlToFocusLocation.X + controlToFocusWidth - gapSize);
                    var calcWidth = xMainWindowPnl.ActualWidth - (controlToFocusLocation.X + controlToFocusWidth);
                    xHelpLayoutRectangleRight.Width = (calcWidth > 0) ? calcWidth : xMainWindowPnl.ActualWidth;
                    xHelpLayoutRectangleRight.Height = xMainWindowPnl.ActualHeight;

                    xHelpLayoutRectangleTop.SetValue(Canvas.LeftProperty, controlToFocusLocation.X);
                    xHelpLayoutRectangleTop.Width = controlToFocusWidth;
                    xHelpLayoutRectangleTop.Height = controlToFocusLocation.Y;

                    xHelpLayoutRectangleBottom.SetValue(Canvas.LeftProperty, controlToFocusLocation.X);
                    xHelpLayoutRectangleBottom.SetValue(Canvas.TopProperty, controlToFocusLocation.Y + controlToFocusHeight);
                    xHelpLayoutRectangleBottom.Width = controlToFocusWidth ;
                    xHelpLayoutRectangleBottom.Height = xMainWindowPnl.ActualHeight - (controlToFocusLocation.Y + controlToFocusHeight);

                    //xHelpLayoutRectangleFocusedItem.SetValue(Canvas.LeftProperty, controlToFocusLocation.X);
                    //xHelpLayoutRectangleFocusedItem.SetValue(Canvas.TopProperty, controlToFocusLocation.Y);
                    //xHelpLayoutRectangleFocusedItem.Width = controlToFocusWidth;
                    //xHelpLayoutRectangleFocusedItem.Height = controlToFocusHeight;

                    //-- set text and it location 
                    xHelpLayoutTextBlock.Text = helpArgs.HelpText;
                    double textNeededWidth = 450;
                    double textNeededHeight = 250;
                    double arrowNeededLength = 100;
                    double arrowDistanceFromTarget = 10;
                    Point helpTextLocation = new Point();
                    Point arrowSourceLocation = new Point();
                    Point arrowTargetLocation = new Point();
                    //focused item top left corner
                    if (controlToFocusLocation.X >= textNeededWidth && controlToFocusLocation.Y >= textNeededHeight)
                    {
                        helpTextLocation.X = controlToFocusLocation.X - textNeededWidth - arrowNeededLength; ;
                        helpTextLocation.Y = controlToFocusLocation.Y - arrowNeededLength;
                        arrowSourceLocation = new Point(helpTextLocation.X + textNeededWidth, helpTextLocation.Y + 50);
                        arrowTargetLocation = new Point(controlToFocusLocation.X, controlToFocusLocation.Y - arrowDistanceFromTarget);
                    }
                    //focused item bottom left corner
                    else if (controlToFocusLocation.X >= textNeededWidth && (xMainWindowPnl.ActualHeight - (controlToFocusLocation.Y + controlToFocusHeight)) >= textNeededHeight)
                    {
                        helpTextLocation.X = controlToFocusLocation.X - textNeededWidth;
                        helpTextLocation.Y = controlToFocusLocation.Y + controlToFocusHeight + arrowNeededLength;
                        arrowSourceLocation = new Point(helpTextLocation.X + textNeededWidth / 2, helpTextLocation.Y);
                        arrowTargetLocation = new Point(controlToFocusLocation.X, controlToFocusLocation.Y + controlToFocusHeight + arrowDistanceFromTarget);
                    }
                    //focused item top right corner
                    else if ((xMainWindowPnl.ActualWidth - (controlToFocusLocation.X + controlToFocusWidth)) >= textNeededWidth && controlToFocusLocation.Y >= textNeededHeight)
                    {
                        helpTextLocation.X = controlToFocusLocation.X + controlToFocusWidth + textNeededWidth;
                        helpTextLocation.Y = controlToFocusLocation.Y - arrowNeededLength;
                        arrowSourceLocation = new Point(helpTextLocation.X - 20, helpTextLocation.Y + 20);
                        arrowTargetLocation = new Point(controlToFocusLocation.X + controlToFocusWidth, controlToFocusLocation.Y - arrowDistanceFromTarget);
                    }
                    //focused item bottom right corner
                    else if ((xMainWindowPnl.ActualWidth - (controlToFocusLocation.X + controlToFocusWidth)) >= textNeededWidth && (xMainWindowPnl.ActualHeight - (controlToFocusLocation.Y + controlToFocusHeight)) >= textNeededHeight)
                    {
                        helpTextLocation.X = controlToFocusLocation.X + controlToFocusWidth + textNeededWidth;
                        helpTextLocation.Y = controlToFocusLocation.Y + controlToFocusHeight + arrowNeededLength;
                        arrowSourceLocation = new Point(helpTextLocation.X, helpTextLocation.Y);
                        arrowTargetLocation = new Point(controlToFocusLocation.X + controlToFocusWidth, controlToFocusLocation.Y + controlToFocusHeight + arrowDistanceFromTarget);
                    }
                    //middle of screen
                    else
                    {
                        helpTextLocation.X = xMainWindowPnl.ActualWidth / 2;
                        helpTextLocation.Y = xMainWindowPnl.ActualHeight / 2;
                    }
                    xHelpLayoutTextBlock.SetValue(Canvas.LeftProperty, helpTextLocation.X);
                    xHelpLayoutTextBlock.SetValue(Canvas.TopProperty, helpTextLocation.Y);

                    //-- draw Arrow
                    while (xHelpLayoutCanvas.Children.Count > 5)//removing previous dynamic arrows
                    {
                        xHelpLayoutCanvas.Children.RemoveAt(xHelpLayoutCanvas.Children.Count - 1);
                    }
                    xHelpLayoutCanvas.Children.Add(GeneralWindows.HelpLayout.DrawArrow.DrawLinkArrow(arrowSourceLocation, arrowTargetLocation));

                    xHelpLayoutCanvas.Visibility = Visibility.Visible;
                });

                if (!WorkSpace.Instance.UserProfile.ShownHelpLayoutsKeys.Contains(helpArgs.HelpLayoutKey))
                {
                    WorkSpace.Instance.UserProfile.ShownHelpLayoutsKeys.Add(helpArgs.HelpLayoutKey);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to show user Help Layout, help key ='{0}'", helpArgs.HelpLayoutKey), ex);
                HideHelpLayout();
            }
        }

        private void HideHelpLayout()
        {
            this.Dispatcher.Invoke(() =>
            {
                xHelpLayoutCanvas.Visibility = Visibility.Collapsed;
                mHelpLayoutList.RemoveAt(0);//remove help just shown               
            });
        }

        private void xHelpLayoutCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HideHelpLayout();
        }

        private void xMainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (xHelpLayoutCanvas.Visibility == Visibility.Visible)
            {
                ShowHelpLayout();//need to show again in correct size
            }
        }
    }
}