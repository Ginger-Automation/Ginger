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
using Ginger.ALM;
using Ginger.ALM.QC;
using Ginger.AnalyzerLib;
using Ginger.BDD;
using Ginger.BusinessFlowWindows;
using Ginger.Dictionaries;
using Ginger.SolutionGeneral;
using Ginger.Functionalities;
using Ginger.GeneralLib;
using Ginger.Reports;
using Ginger.SolutionWindows;
using Ginger.SourceControl;
using Ginger.UserConfig;
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
using Ginger.SolutionLibNew;

namespace Ginger
{
    public partial class MainWindow : Window
    {        
        public enum eSolutionTabType { None,BusinessFlows,Run,Configurations,Resources};
        public eSolutionTabType SelectedSolutionTab;
        
        GingerHelperWindow Helper;

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

                //App
                App.AutomateBusinessFlowEvent += App_AutomateBusinessFlowEvent;

                //User Profile
                App.PropertyChanged += App_PropertyChanged;
                App.UserProfile.PropertyChanged += UserProfilePropertyChanged;
                GingerWPF.BindingLib.ControlsBinding.ObjFieldBinding(xAutoLoadLastSolutionMenuItem, MenuItem.IsCheckedProperty, App.UserProfile, nameof(UserProfile.AutoLoadLastSolution));
                GingerWPF.BindingLib.ControlsBinding.ObjFieldBinding(xAskToUpgradeMenuItem, MenuItem.IsCheckedProperty, App.UserProfile, nameof(UserProfile.DoNotAskToUpgradeSolutions));
                GingerWPF.BindingLib.ControlsBinding.ObjFieldBinding(xShowBFSaveWarnMenuItem, MenuItem.IsCheckedProperty, App.UserProfile, nameof(UserProfile.AskToSaveBusinessFlow));
                if (App.UserProfile.GingerStatus == eGingerStatus.Active)
                {
                    Reporter.ToGingerHelper(eGingerHelperMsgKey.ExitMode);
                }
                App.UserProfile.GingerStatus = eGingerStatus.Active;
                App.UserProfile.SaveUserProfile();

                //Reporter
                Reporter.MainWindowDispatcher = this.Dispatcher; //Make sure msgbox will apear running from Main Window STA
                Reporter.HandlerGingerHelperEvent += Reporter_HandlerGingerHelperEvent;

                //Status Bar            
                ErrorsLabel.Visibility = Visibility.Collapsed;
                lblBetaFeatures.BindControl(WorkSpace.Instance.BetaFeatures, nameof(BetaFeatures.UsingStatus));
                lblVersion.Content = "Version " + Ginger.App.AppVersion;

                //Solution                     
                SetRecentSolutionsMenu();
                SetSolutionDependedUIElements();
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
                Reporter.ToLog(eLogLevel.ERROR, "Error in Init Main Window", ex);
            }
        }

        private void App_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(App.LoadingSolution))
            {
                if (App.LoadingSolution)
                {
                    xMainWindowFrame.Content = new LoadingSolutionPage();
                    xMainWindowFrame.Visibility = Visibility.Visible;
                    GingerCore.General.DoEvents();
                }
                else if (xMainWindowFrame.Content is LoadingSolutionPage)
                {
                    xMainWindowFrame.Visibility= Visibility.Collapsed;
                }
            }
        }

        private void SetRecentSolutionsMenu()
        {
            xRecentSolutionsMenuItem.Items.Clear();

            foreach (Solution sol in App.UserProfile.RecentSolutionsAsObjects)
            {
                MenuItem mi = new MenuItem();
                mi.Header = sol.Name;
                mi.ToolTip = sol.Folder;
                mi.Tag = sol;
                mi.Click += RecentSolutionSelection_Click;
                xRecentSolutionsMenuItem.Items.Add(mi);
            }

            App.UserProfile.RecentSolutionsAsObjects.CollectionChanged -= RecentSolutionsObjects_CollectionChanged;
            App.UserProfile.RecentSolutionsAsObjects.CollectionChanged += RecentSolutionsObjects_CollectionChanged;
        }

        private void RecentSolutionsObjects_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SetRecentSolutionsMenu();
        }

        private async void Reporter_HandlerGingerHelperEvent(GingerHelperEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (e.GingerHelperEventActions == GingerHelperEventArgs.eGingerHelperEventActions.Show)
                {
                    if (Helper != null)
                        Helper.Close();

                    Helper = new GingerHelperWindow();
                    Helper.showHelper(e.HelperMsg, e.ButtonHandler);
                }
                else
                {
                    if (Helper != null)
                    {
                        while ((DateTime.Now - Helper.LoadStartTime).TotalSeconds < 1)
                        {
                            Task.Delay(100);
                        }
                        Helper.Close();
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
                    xSolutionTopNavigationListView.SelectedItem = null;
                    xSolutionTopNavigationListView.SelectedItem = xBusinessFlowsListItem;
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
                    xSolutionTopNavigationListView.SelectedItem = null;
                    xSolutionNameTextBlock.Text = "Please Load Solution";
                }
                else
                {
                    App.LastBusinessFlow = null;                  
                    GingerWPF.BindingLib.ControlsBinding.ObjFieldBinding(xSolutionNameTextBlock, TextBlock.TextProperty, App.UserProfile.Solution, nameof(Solution.Name));
                    xSolutionTopNavigationListView.SelectedItem = null;
                    xSolutionTopNavigationListView.SelectedItem = xBusinessFlowsListItem;
                }
            }
        }

        //private void ShowBizFlowInfo()
        //{
        //    if (App.BusinessFlow != null)
        //        lblBizFlow.Content = App.BusinessFlow.Name;
        //    else
        //        lblBizFlow.Content = "No " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
        //}

        //private void AppPropertychanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == nameof(App.BusinessFlow))
        //    {
        //        ShowBizFlowInfo();
        //    }
        //}

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
                catch(Exception ex)
                {
                    Reporter.ToLog(eLogLevel.WARN, "Failed to delete Auto Save folder", ex);
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
                    Reporter.ToLog(eLogLevel.WARN, "Failed to delete Recover folder", ex);
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
                Reporter.ToLog(eLogLevel.ERROR, "Failed to write ExecutionLog.LogAppClosed() into the autlog folder.");
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
            if (!(xMainWindowFrame.Content is LoadingSolutionPage))
            {
                xMainWindowFrame.Visibility = Visibility.Collapsed;
            }
            ListViewItem selectedTopListItem = (ListViewItem)xSolutionTopNavigationListView.SelectedItem;

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
                string solutionFileName = solutionFolder + @"\Ginger.Solution.xml";
                if (System.IO.File.Exists(PathHelper.GetLongPath(solutionFileName)))
                {
                    if (App.SetSolution(Path.GetDirectoryName(PathHelper.GetLongPath(solutionFolder))))
                    {
                        App.UserProfile.AddsolutionToRecent(App.UserProfile.Solution);
                    }
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
                xSolutionTopNavigationListView.Visibility = Visibility.Visible;
                if (App.UserProfile.UserTypeHelper.IsSupportAutomate)
                {
                    xRunListItem.Visibility = Visibility.Visible;
                }
                else
                {
                    xRunListItem.Visibility = Visibility.Collapsed;
                }

                //TODO: Fix with all solution operations
                //if (App.UserProfile.UserTypeHelper.IsSupportAnalyzer)
                //    AnalyzerButton.Visibility = Visibility.Visible;
                //else
                //    AnalyzerButton.Visibility = Visibility.Collapsed;

                //if (App.UserProfile.UserTypeHelper.IsSupportALM)
                //    ALMConfigurationsGroup.Visibility = Visibility.Visible;
                //else
                //    ALMConfigurationsGroup.Visibility = Visibility.Collapsed;

                //if (App.UserProfile.Solution.SourceControl != null)
                //{
                //    CheckInSolutionBtn.Visibility = Visibility.Visible;
                //    GetLatestSolutionBtn.Visibility = Visibility.Visible;
                //    ResolveConflictsBtn.Visibility = Visibility.Visible;
                //    ConnectionDetailsBtn.Visibility = Visibility.Visible;
                //    RepositoryDetailsBtn.Visibility = Visibility.Visible;
                //    SourceControlSolutioRibbonGroup.Visibility = Visibility.Visible;
                //}

            }
            else
            {
                xSolutionTopNavigationListView.Visibility = Visibility.Collapsed;
                xSolutionOperationsMenu.Visibility = Visibility.Collapsed;
            }
        }

        
          

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.S && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            //{
            //    throw new NotImplementedException();                
            //}
            //else if (e.Key == Key.S && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            //{
            //    SaveAppCurrentItem();
            //}
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

        private void QCFieldConfiguration_Click(object sender, RoutedEventArgs e)
        {
            ALMIntegration.Instance.OpenALMItemsFieldsPage();
        }

        private void ALMDefectsProfiles_Click(object sender, RoutedEventArgs e)
        {
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

        private void btnSetTerminologyType_Click(object sender, RoutedEventArgs e)
        {
            SetTerminologyTypePage setTermPage = new SetTerminologyTypePage();
            setTermPage.ShowAsWindow();
        }

        private void btnSetLoggingLevel_Click(object sender, RoutedEventArgs e)
        {
            SetAppLogLevelPage setLogLevelPage = new SetAppLogLevelPage();
            setLogLevelPage.ShowAsWindow();
        }
        private void btnSetUserType_Click(object sender, RoutedEventArgs e)
        {
            SetUserTypePage setTermPage = new SetUserTypePage();
            setTermPage.ShowAsWindow();
        }

        private void QCManagerReport_Click(object sender, RoutedEventArgs e)
        {
            QCManagerReportPage QCMRP = new QCManagerReportPage();
            QCMRP.ShowAsWindow();
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
        
        private void ImportFeatureFile_Click(object sender, RoutedEventArgs e)
        {
            BDDIntegration BDDI = new BDDIntegration();
            bool imported = BDDI.ImportFeatureFile();
        }

        private void CreateFeatureFile_Click(object sender, RoutedEventArgs e)
        {
            BDDIntegration BDDI = new BDDIntegration();
            BDDI.CreateFeatureFile();
        }

        private void btnLaunchConsole_Click(object sender, RoutedEventArgs e)
        {
            DebugConsoleWindow.Show();
        }

        private void lblBetaFeatures_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BetaFeaturesPage p = new BetaFeaturesPage();
            p.ShowAsWindow();
        }

        private void ErrorsLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {           
            ShowGingerLog();
        }
                
        private void FindAndReplaceSolutionPageButton_Click(object sender, RoutedEventArgs e)
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
                xSolutionTopNavigationListView.SelectedItem = xBusinessFlowsListItem;
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
                //App.UserProfile.Solution.Save();
            }
        }

        private void xSolutionChangeBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void xSolutionCheckInBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void xSolutionGetLatestBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void xLoadSupportSiteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://ilrnaginger01/");
        }

        private void xLoadForumSiteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://ilrnaginger01:81/");
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

    }
}