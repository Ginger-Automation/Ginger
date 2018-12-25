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
using Ginger.ALM;
using Ginger.AnalyzerLib;
using Ginger.BusinessFlowWindows;
using Ginger.SolutionGeneral;
using Ginger.Functionalities;
using Ginger.GeneralLib;
using Ginger.SolutionWindows;
using Ginger.SourceControl;
using GingerCore;
using GingerCore.Repository.UpgradeLib;
using GingerCoreNET.SourceControl;
using GingerWPF;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Amdocs.Ginger.IO;
using Ginger.ConfigurationsLib;
using Ginger.MenusLib;
using Amdocs.Ginger;
using Ginger.User;
using Amdocs.Ginger.UserControls;
using System.Drawing;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common;

namespace Ginger
{
    public partial class MainWindow : Window
    {
        public enum eSolutionTabType { None, BusinessFlows, Run, Configurations, Resources };
        public eSolutionTabType SelectedSolutionTab;

        private bool mAskUserIfToClose = true;
        private long _currentClickedTabIndex = -1;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void Init()
        {
            try
            {
                //General
                this.WindowState = System.Windows.WindowState.Maximized;
                Reporter.MainWindowDispatcher = this.Dispatcher; //Make sure msgbox will appear running from Main Window STA

                //App
                App.AutomateBusinessFlowEvent += App_AutomateBusinessFlowEvent;

                //User Profile
                App.PropertyChanged += App_PropertyChanged;
                App.UserProfile.PropertyChanged += UserProfilePropertyChanged;
                if (App.UserProfile.GingerStatus == eGingerStatus.Active)
                {
                    Reporter.ToGingerHelper(eGingerHelperMsgKey.ExitMode);
                }
                App.UserProfile.GingerStatus = eGingerStatus.Active;
                App.UserProfile.SaveUserProfile();
                App.UserProfile.RecentSolutionsAsObjects.CollectionChanged += RecentSolutionsObjects_CollectionChanged;

                //Reporter                
                Reporter.HandlerGingerHelperEvent += Reporter_HandlerGingerHelperEvent;
                Reporter.ErrorReportedEvent += Reporter_ErrorReportedEvent;

                //Main Menu                            
                xGingerIconImg.ToolTip = App.AppFullProductName + Environment.NewLine + "Version " + App.AppVersion;
                SetSolutionDependedUIElements();
                UpdateUserDetails();
                if (App.UserProfile.RecentSolutionsAsObjects.Count > 0)
                {
                    xRecentSolutionsMenuItem.Visibility = Visibility.Visible;
                }

                //Status Bar            
                xLogErrorsPnl.Visibility = Visibility.Collapsed;
                xProcessMsgPnl.Visibility = Visibility.Collapsed;                
                WorkSpace.Instance.BetaFeatures.PropertyChanged += BetaFeatures_PropertyChanged;
                SetBetaFlagIconVisibility();
                lblVersion.Content = "Version " + Ginger.App.AppVersion;

                //Solution                                    
                if (App.UserProfile.AutoLoadLastSolution && App.RunningFromConfigFile == false && App.RunningFromUnitTest == false)
                {
                    AutoLoadLastSolution();
                }

                //Messages
                if (App.UserProfile.NewHelpLibraryMessgeShown == false)
                {
                    Reporter.ToGingerHelper(eGingerHelperMsgKey.GingerHelpLibrary);
                    App.UserProfile.NewHelpLibraryMessgeShown = true;
                }
            }
            catch (Exception ex)
            {
                App.AppSplashWindow.Close();
                Reporter.ToUser(eUserMsgKeys.ApplicationInitError, ex.Message);
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error in Init Main Window", ex);
            }
        }

        private int mErrorsNum = 0;
        private void Reporter_ErrorReportedEvent()
        {
            try
            {
                UpdateErrorNotification();
            }
            catch(Exception ex)
            {
                //something went wrong
            }
        }

        private void UpdateErrorNotification()
        {
            Task.Factory.StartNew(() =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (xLogErrorsPnl != null)
                    {
                        xLogErrorsPnl.Visibility = Visibility.Visible;
                        xLogErrorsLbl.Content = "[" + ++mErrorsNum + "]";
                        xLogErrorsPnl.ToolTip = mErrorsNum + " Errors were logged to Ginger log, click to view log file";
                    }
                });
            });
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

        private void App_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(App.LoadingSolution))
            {
                if (App.LoadingSolution)
                {
                    xNoLoadedSolutionImg.Visibility = Visibility.Collapsed;
                    xMainWindowFrame.Content = new LoadingPage("Loading Solution...");
                    xMainWindowFrame.Visibility = Visibility.Visible;                    
                    GingerCore.General.DoEvents();
                }
                else if (xMainWindowFrame.Content is LoadingPage && SelectedSolutionTab == eSolutionTabType.None)
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
                if (App.UserProfile.RecentSolutionsAsObjects.Count > 0)
                {
                    xRecentSolutionsMenuItem.Visibility = Visibility.Visible;

                    foreach (Solution sol in App.UserProfile.RecentSolutionsAsObjects)
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

        private DateTime mProcessMsgShowTime;
        private async void Reporter_HandlerGingerHelperEvent(GingerHelperEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (e.GingerHelperEventActions == GingerHelperEventArgs.eGingerHelperEventActions.Show)
                {
                    if (e.MessageType == eGingerHelperMsgType.PROCESS)
                    {
                        xProcessMsgIcon.ImageType = eImageType.Processing;
                    }
                    else
                    {
                        xProcessMsgIcon.ImageType = eImageType.Info;
                    }
                    xProcessMsgPnl.Visibility = Visibility.Visible;
                    xProcessMsgTxtBlock.Text = e.HelperMsg.MsgContent;
                    xProcessMsgTxtBlock.ToolTip= e.HelperMsg.MsgContent;
                    mProcessMsgShowTime = DateTime.Now;
                    GingerCore.General.DoEvents();
                }
                else
                {
                    if (xProcessMsgPnl.Visibility == Visibility.Visible)
                    {
                        while ((DateTime.Now - mProcessMsgShowTime).TotalSeconds < 1)
                        {
                            Task.Delay(100);
                        }
                        xProcessMsgPnl.Visibility = Visibility.Collapsed;
                    }
                }
            });
        }

        internal void AutoLoadLastSolution()
        {
            try
            {
                if (App.UserProfile.RecentSolutionsAsObjects.Count > 0)
                {
                    App.SetSolution(App.UserProfile.RecentSolutionsAsObjects[0].Folder);
                    xSolutionTabsListView.SelectedItem = null;
                    xSolutionTabsListView.SelectedItem = xBusinessFlowsListItem;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKeys.SolutionLoadError, ex);
            }
        }

        public void UserProfilePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Handle Solution change
            //TODO: cleanup close current biz flow etc...
            if (e.PropertyName == nameof(UserProfile.Solution))
            {
                SetSolutionDependedUIElements();
                if (App.UserProfile.Solution == null)
                {
                    xSolutionTabsListView.SelectedItem = null;
                    xSolutionNameTextBlock.Text = "Please Load Solution";                    
                }
                else
                {
                    xNoLoadedSolutionImg.Visibility = Visibility.Collapsed;
                    App.LastBusinessFlow = null;
                    GingerWPF.BindingLib.ControlsBinding.ObjFieldBinding(xSolutionNameTextBlock, TextBlock.TextProperty, App.UserProfile.Solution, nameof(Solution.Name), System.Windows.Data.BindingMode.OneWay);
                    GingerWPF.BindingLib.ControlsBinding.ObjFieldBinding(xSolutionNameTextBlock, TextBlock.ToolTipProperty, App.UserProfile.Solution, nameof(Solution.Folder), System.Windows.Data.BindingMode.OneWay);
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
            //To Clear the AutoSave Directory Folder
            if (Directory.Exists(App.AppSolutionAutoSave.AutoSaveFolderPath))
            {
                try
                {
                    Directory.Delete(App.AppSolutionAutoSave.AutoSaveFolderPath, true);
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eAppReporterLogLevel.WARN, "Failed to delete Auto Save folder", ex);
                }
            }
            if (Directory.Exists(App.AppSolutionRecover.RecoverFolderPath))
            {
                try
                {
                    Directory.Delete(App.AppSolutionRecover.RecoverFolderPath, true);
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eAppReporterLogLevel.WARN, "Failed to delete Recover folder", ex);
                }
            }
            if (mAskUserIfToClose == false || Reporter.ToUser(eUserMsgKeys.AskIfSureWantToClose) == MessageBoxResult.Yes)
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
            ClosingWindow CW = new ClosingWindow();
            CW.Show();
            GingerCore.General.DoEvents();
            App.AutomateTabGingerRunner.CloseAgents();
            GingerCore.General.CleanDirectory(GingerCore.Actions.Act.ScreenshotTempFolder, true);
            
            if (!App.RunningFromConfigFile)
            {
                App.UserProfile.GingerStatus = eGingerStatus.Closed;
                App.UserProfile.SaveUserProfile();
                App.AppSolutionAutoSave.SolutionAutoSaveEnd();
                try
                {
                    //TODO: no need to to log if running from comamnd line
                    AutoLogProxy.LogAppClosed();
                }
                catch
                {
                    Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to write ExecutionLog.LogAppClosed() into the autlog folder.");
                }
            }
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
                    App.SetSolution(Path.GetDirectoryName(PathHelper.GetLongPath(solutionFolder)));
                }
                else
                {
                    Reporter.ToUser(eUserMsgKeys.SolutionFileNotFound, solutionFileName);
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
            if (App.UserProfile.Solution != null)
            {
                xLoadedSolutionMenusPnl.Visibility = Visibility.Visible;
                if (App.UserProfile.UserTypeHelper.IsSupportAutomate)
                {
                    xRunListItem.Visibility = Visibility.Visible;
                }
                else
                {
                    xRunListItem.Visibility = Visibility.Collapsed;
                }
                if (App.UserProfile.Solution.SourceControl != null)
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
            if(!ALMIntegration.Instance.AlmConfigurations.UseRest)
            {
                Reporter.ToUser(eUserMsgKeys.ALMDefectsUserInOtaAPI, "");
                return;
            }
            ALMIntegration.Instance.ALMDefectsProfilesPage();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            App.MainWindow.Close();
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            AutoLogProxy.UserOperationStart("btnAbout_Click");

            AboutPage AP = new AboutPage();
            AP.ShowAsWindow();

            AutoLogProxy.UserOperationEnd();
        }

        private void ViewSolutionFiles_Click(object sender, RoutedEventArgs e)
        {
            //show solution folder files
            if (App.UserProfile.Solution != null)
                Process.Start(App.UserProfile.Solution.Folder);
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
            if (Reporter.ToUser(eUserMsgKeys.LoseChangesWarn) == MessageBoxResult.No) return;

            AutoLogProxy.UserOperationStart("btnSourceControlCheckIn_Click");

            App.CheckIn(App.UserProfile.Solution.Folder);

            AutoLogProxy.UserOperationEnd();
        }

        private void btnSourceControlGetLatest_Click(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKeys.LoseChangesWarn) == MessageBoxResult.No) return;

            AutoLogProxy.UserOperationStart("btnSourceControlGetLatest_Click");

            Reporter.ToGingerHelper(eGingerHelperMsgKey.GetLatestFromSourceControl);
            if (string.IsNullOrEmpty(App.UserProfile.Solution.Folder))
                Reporter.ToUser(eUserMsgKeys.SourceControlUpdateFailed, "Invalid Path provided");
            else
                SourceControlIntegration.GetLatest(App.UserProfile.Solution.Folder, App.UserProfile.Solution.SourceControl);

            App.UpdateApplicationsAgentsMapping(false);
            Reporter.CloseGingerHelper();

            AutoLogProxy.UserOperationEnd();
        }

        private void AnalyzerButton_Click(object sender, RoutedEventArgs e)
        {
            AutoLogProxy.UserOperationStart("MainWindow.AnalyzerButton_Click");
            AnalyzerPage AP = new AnalyzerPage();
            AP.Init(App.UserProfile.Solution);
            AP.ShowAsWindow();
            AutoLogProxy.UserOperationEnd();
        }

        private void ResolveSourceControlConflicts(eResolveConflictsSide side)
        {
            AutoLogProxy.UserOperationStart("ResolveConflictsBtn_Click");

            Reporter.ToGingerHelper(eGingerHelperMsgKey.ResolveSourceControlConflicts);
            SourceControlIntegration.ResolveConflicts(App.UserProfile.Solution.SourceControl, App.UserProfile.Solution.Folder, side);
            Reporter.CloseGingerHelper();

            AutoLogProxy.UserOperationEnd();
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
            if (App.UserProfile.Solution != null)
            {
                Solution sol = App.UserProfile.Solution;
                ConcurrentBag<string> lowerVersionFiles = SolutionUpgrade.GetSolutionFilesCreatedWithRequiredGingerVersion(Solution.SolutionFiles(sol.Folder), SolutionUpgrade.eGingerVersionComparisonResult.LowerVersion);
                if (lowerVersionFiles != null && lowerVersionFiles.Count > 0)
                {
                    UpgradePage solutionUpgradePage = new UpgradePage(SolutionUpgradePageViewMode.UpgradeSolution, sol.Folder, sol.Name, lowerVersionFiles.ToList());
                    solutionUpgradePage.ShowAsWindow();
                }
                else
                {
                    Reporter.ToUser(eUserMsgKeys.StaticInfoMessage, "Upgrade is not needed, all solution items were created with latest version.");
                }
            }
        }

        private void btnRecover_Click(object sender, RoutedEventArgs e)
        {
            if (App.UserProfile.Solution != null)
            {
                App.AppSolutionRecover.SolutionRecoverStart(true);
            }
        }

        private void btnViewLog_Click(object sender, RoutedEventArgs e)
        {
            ShowGingerLog();
        }

        private void btnViewLogDetails_Click(object sender, RoutedEventArgs e)
        {
            LogDetailsPage log = new LogDetailsPage();
            log.ShowAsWindow();
        }

        private void ShowGingerLog()
        {
            string mLogFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\\amdocs\\Ginger\\WorkingFolder\\Logs\\Ginger_Log.txt";
            if (System.IO.File.Exists(mLogFilePath))
            {
                Process.Start(mLogFilePath);
            }
            else
            {
                Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Ginger log file was not found in the Path:'" + mLogFilePath + "'");
            }
        }

        private void btnSourceControlRepositoryDetails_Click(object sender, RoutedEventArgs e)
        {

            SourceControlItemInfoDetails SCIInfoDetails = SourceControlIntegration.GetRepositoryInfo(App.UserProfile.Solution.SourceControl);
            SourceControlItemInfoPage SCIIP = new SourceControlItemInfoPage(SCIInfoDetails);
            SCIIP.ShowAsWindow();
        }

        private void btnViewLogLocation_Click(object sender, RoutedEventArgs e)
        {
            string mLogFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\\amdocs\\Ginger\\WorkingFolder\\Logs\\Ginger_Log.txt";
            string folder = System.IO.Path.GetDirectoryName(mLogFilePath);
            if (System.IO.Directory.Exists(folder))
            {
                Process.Start(folder);
            }
            else
            {
                Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Ginger log file folder was not found in the path: '" + folder + "'");
            }
        }

        private void btnLaunchConsole_Click(object sender, RoutedEventArgs e)
        {
            DebugConsoleWindow.Show();
        }

        private void xBetaFeaturesIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
            BetaFeaturesPage p = new BetaFeaturesPage();
            p.ShowAsWindow();
        }

        private void xLogErrors_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //ShowGingerLog();
            LogDetailsPage logDetailsPage = new LogDetailsPage();
            logDetailsPage.ShowAsWindow();

            xLogErrorsPnl.Visibility = Visibility.Collapsed;
            mErrorsNum = 0;
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
                App.BusinessFlow = (BusinessFlow)args.Object;
                App.BusinessFlow.SaveBackup();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Application.Current.Shutdown();
        }

        private void xSolutionEditBtn_Click(object sender, RoutedEventArgs e)
        {
            string newName = App.UserProfile.Solution.Name;
            if (GingerCore.GeneralLib.InputBoxWindow.GetInputWithValidation("Solution Rename", "New Solution Name:", ref newName, System.IO.Path.GetInvalidPathChars()))
            {
                App.UserProfile.Solution.Name = newName;
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
                App.SetSolution(selectedSol.Folder);
            }
            else
                Reporter.ToUser(eUserMsgKeys.SolutionLoadError, "Selected Solution was not found");

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
            if (string.IsNullOrEmpty(App.UserProfile.ProfileImage))
            {
                xProfileImageImgBrush.ImageSource = ImageMakerControl.GetImageSource(Amdocs.Ginger.Common.Enums.eImageType.User, foreground: (System.Windows.Media.SolidColorBrush)FindResource("$BackgroundColor_LightGray"), width: 50);
            }
            else
            {
                xProfileImageImgBrush.ImageSource = Ginger.General.GetImageStream(Ginger.General.Base64StringToImage(App.UserProfile.ProfileImage));
            }

            if (String.IsNullOrEmpty(App.UserProfile.UserFirstName))
            {
                xUserNameLbl.Content = App.UserProfile.UserName;
            }
            else
            {
                xUserNameLbl.Content = App.UserProfile.UserFirstName;
            }
        }

        private void xLogOptionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (xLogOptionsMenuItem.Tag == null)
            {
                xLogOptionsMenuItem.Tag = true;//expanded
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
                if (((MenuItem)xUserOperationsMainMenuItem.Items[i]).Tag == "Log")
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
                AddSubMenuItem(xUserOperationsMainMenuItem, "Open Full Log File", "Log", btnViewLog_Click, insertIndex++, iconType: eImageType.File);
                AddSubMenuItem(xUserOperationsMainMenuItem, "Open Log File Folder", "Log", btnViewLogLocation_Click, insertIndex++, iconType: eImageType.OpenFolder);
                AddSubMenuItem(xUserOperationsMainMenuItem, "Open Debug Console", "Log", btnLaunchConsole_Click, insertIndex, iconType: eImageType.Screen);
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
                xSupportOptionsMenuItem.Tag = true;//expanded
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
                if (((MenuItem)xExtraOperationsMainMenuItem.Items[i]).Tag == "Support")
                {
                    xExtraOperationsMainMenuItem.Items.RemoveAt(i);
                    i--;
                }
            }

            if (xSupportOptionsMenuItem.Tag != null)
            {
                //Insert
                int insertIndex = xExtraOperationsMainMenuItem.Items.IndexOf(xSupportOptionsMenuItem) + 1;

                AddSubMenuItem(xExtraOperationsMainMenuItem, "Ginger Support Site", "Support", xLoadSupportSiteMenuItem_Click, insertIndex++, iconType: eImageType.Website);
                AddSubMenuItem(xExtraOperationsMainMenuItem, "Ginger Q&A Fourm Site", "Support", xLoadForumSiteMenuItem_Click, insertIndex++, iconType: eImageType.Forum);
                AddSubMenuItem(xExtraOperationsMainMenuItem, "Raise Ticket to Core Team", "Support", xOpenTicketMenuItem_Click, insertIndex++, iconType: eImageType.Ticket);
                AddSubMenuItem(xExtraOperationsMainMenuItem, "Ginger on GitHub", "Support", xGingerGithubMenuItem_Click, insertIndex, iconType: eImageType.GitHub);
            }
        }

        private void xContactOptionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (xContactOptionsMenuItem.Tag == null)
            {
                xContactOptionsMenuItem.Tag = true;//expanded
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
                if (((MenuItem)xExtraOperationsMainMenuItem.Items[i]).Tag == "Contact")
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
    }
}