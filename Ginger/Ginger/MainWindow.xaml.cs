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
using Ginger.Agents;
using Ginger.ALM;
using Ginger.ALM.QC;
using Ginger.AnalyzerLib;
using Ginger.BDD;
using Ginger.BusinessFlowWindows;
using Ginger.Dictionaries;
using Ginger.Environments;
using Ginger.Functionalities;
using Ginger.GeneralLib;
using Ginger.Reports;
using Ginger.Run;
using Ginger.SolutionWindows;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.SourceControl;
using Ginger.Support;
using Ginger.UserConfig;
using GingerCore;
using GingerCore.Environments;
using GingerCore.Repository.UpgradeLib;
using GingerCoreNET.RunLib;
using GingerCoreNET.SourceControl;
using GingerWPF;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Input;
using System.Windows.Media;
using Amdocs.Ginger.UserControls;
using Amdocs.Ginger.IO;
using Amdocs.Ginger.Common.Enums;
using Ginger.ConfigurationsLib;
using Ginger.MenusLib;
using Amdocs.Ginger;
using Amdocs.Ginger.Repository;

namespace Ginger
{
    public enum eRibbonTab
    {
        Home, Solution, Automate, Run, Support
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    // public partial class MainWindow  : RibbonWindow   {
    public partial class MainWindow : RibbonWindow
    {
        // Keep all the open pages 
        private List<Page> mPageList = new List<Page>();

        GingerHelperWindow Helper = null;

        private bool mAskUserIfToClose = true;

        private long _currentClickedTabIndex = -1;

        public MainWindow()
        {
            InitializeComponent();

            this.WindowState = System.Windows.WindowState.Maximized;

            //Make sure msgbox will apear running from Main Window STA
            Reporter.MainWindowDispatcher = this.Dispatcher;
            Reporter.HandlerGingerHelperEvent += Reporter_HandlerGingerHelperEvent;
            
            lblBetaFeatures.BindControl(WorkSpace.Instance.BetaFeatures, nameof(BetaFeatures.UsingStatus));
            ErrorsLabel.Visibility = Visibility.Collapsed;

            btnRefresh.LargeImageSource = ImageMakerControl.GetImageSource(eImageType.Refresh, 32);
            btnRefresh.SmallImageSource = ImageMakerControl.GetImageSource(eImageType.Refresh, 16);

            btnRecover.LargeImageSource = ImageMakerControl.GetImageSource(eImageType.Reset, 32);
            btnRecover.SmallImageSource = ImageMakerControl.GetImageSource(eImageType.Reset, 16);

            App.AutomateBusinessFlowEvent += App_AutomateBusinessFlowEvent;
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

        public void Init()
        {
            try
            {
                App.PageList = mPageList;

                //TODO: load from User Profile - screen combo                
                App.PropertyChanged += AppPropertychanged;
                App.AppProgressBar.ProgressBarControl = pbStatus;
                App.AppProgressBar.ProgressBarTextControl = pbText;
                //Moved to below in order to avoid multiple calls to ResetSolutionDependedTabs
                App.UserProfile.PropertyChanged += UserProfilePropertyChanged;

                //Disable solution tabs till a solution will be loaded
                ResetSolutionDependedUIElements(false);
                SetUserTypeButtons();

                if (App.UserProfile.AutoLoadLastSolution && App.RunningFromConfigFile == false)
                {
                    AutoLoadLastSolution();
                }
                if (App.UserProfile.GingerStatus == eGingerStatus.Active)
                {
                    Reporter.ToGingerHelper(eGingerHelperMsgKey.ExitMode);
                }

                App.UserProfile.GingerStatus = eGingerStatus.Active;
                App.UserProfile.SaveUserProfile();

                lblVersion.Content = "Version " + Ginger.App.AppVersion;

                if (App.UserProfile.NewHelpLibraryMessgeShown == false)
                {
                    Reporter.ToGingerHelper(eGingerHelperMsgKey.GingerHelpLibrary);
                    App.UserProfile.NewHelpLibraryMessgeShown = true;
                }
                
            }
            catch (Exception e)
            {
                App.AppSplashWindow.Close();
                Reporter.ToUser(eUserMsgKeys.ApplicationInitError, e.Message);
            }
        }

        internal void AutoLoadLastSolution()
        {
            try
            {
                if (App.UserProfile.RecentSolutionsObjects.Count > 0)
                {
                    App.SetSolution(App.UserProfile.RecentSolutionsObjects[0].Folder);
                    App.UserProfile.AddsolutionToRecent(App.UserProfile.RecentSolutionsObjects[0]);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKeys.SolutionLoadError, ex);
            }
        }

        private void SetUserTypeButtons()
        {
            if (App.UserProfile.UserType == eUserType.Business && App.UserProfile.Solution != null)
            {
                SolutionGherkin.Visibility = Visibility.Visible;
                ImportFeatureFile.Visibility = Visibility.Visible;
                CreateFeatureFile.Visibility = Visibility.Visible;
            }
        }

        public string MainRibbonSelectedTab
        {
            get
            {
                return ((RibbonTab)MainRibbon.SelectedItem).Header.ToString();
            }
            set
            {
                foreach (RibbonTab tab in MainRibbon.Items)
                {
                    if ((tab != null) && (value != null) &&
                           (tab.Header.ToString().ToUpper().Trim() == value.ToString().ToUpper().Trim()))
                    {
                        MainRibbon.SelectedItem = tab;
                    }
                }
            }
        }


        public void UserProfilePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Handle Solution change
            //TODO: cleanup close current biz flow etc...
            if (e.PropertyName == nameof(UserProfile.Solution))
            {
                bool isSolutionLoaded = true;
                if (App.UserProfile.Solution == null)
                {
                    isSolutionLoaded = false;
                    lblSolution.Content = "No Solution";
                    return;
                }

                ResetSolutionDependedUIElements(isSolutionLoaded);
                App.LastBusinessFlow = null;
                lblSolution.Content = App.UserProfile.Solution.Name;
            }
        }


        private void ShowBizFlowInfo()
        {
            if (App.BusinessFlow != null)
                lblBizFlow.Content = App.BusinessFlow.Name;
            else
                lblBizFlow.Content = "No " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
        }

        private void AppPropertychanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(App.BusinessFlow))
            {
                ShowBizFlowInfo();
            }
        }

        public void CloseWithoutAsking()
        {
            mAskUserIfToClose = false;
            this.Close();
        }

        private void RibbonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //To Clear the AutoSave Directory Folder
            if (Directory.Exists(App.AppSolutionAutoSave.AutoSaveFolderPath))
            {
                try
                {
                    Directory.Delete(App.AppSolutionAutoSave.AutoSaveFolderPath, true);
                }
                catch
                {

                }
            }
            if (Directory.Exists(App.AppSolutionRecover.RecoverFolderPath))
            {
                try
                {
                    Directory.Delete(App.AppSolutionRecover.RecoverFolderPath, true);
                }
                catch
                {

                }
            }
            if (mAskUserIfToClose == false || Reporter.ToUser(eUserMsgKeys.AskIfSureWantToClose) == MessageBoxResult.Yes)
            {
                AppCleanUp();
            }
            else
                e.Cancel = true;
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

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            LoadOrShowPage(typeof(StartPage));
        }

        private void btnNews_Click(object sender, RoutedEventArgs e)
        {
            LoadOrShowPage(typeof(NewsPage));
        }

        private void MainRibbon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RibbonTab rt = (RibbonTab)MainRibbon.SelectedItem;
            ToggleMainWindow();
            if (rt.Tag != null)
            {
                MainFrame.Content = rt.Tag;
                return;
            }

            //Canoot do switch so we do if and return 
            if (MainRibbon.SelectedItem == HomeRibbon)
            {
                LoadOrShowPage(typeof(StartPage));
                return;
            }

            if (MainRibbon.SelectedItem == xRun)
            {
                ShowPage(RunMenu.MenusPage);
                return;
            }

            if (MainRibbon.SelectedItem == SupportRibbon)
            {
                LoadOrShowPage(typeof(WebSupportPage));
                return;
            }

            if (MainRibbon.SelectedItem == SolutionRibbon)
            {

                LoadOrShowPage(typeof(SolutionPage));
                return;
            }

            if (MainRibbon.SelectedItem == xResources)
            {
                ShowPage(ResourcesMenu.menusPage);
                return;
            }

            if (MainRibbon.SelectedItem == xConfigurations)
            {

                ShowPage(ConfigurationsMenu.menusPage);
                return;
            }

            if (MainRibbon.SelectedItem == xBusinessFlows)
            {
                ShowPage(new BusinessFlowsAutomatePage());
                return;
            }
        }

        public void ToggleMainWindow()
        {
            if (MainRibbon.SelectedItem != xRun && MainRibbon.SelectedItem != xResources && MainRibbon.SelectedItem != xConfigurations && MainRibbon.SelectedItem != xBusinessFlows)
            {
                MainFrame.Margin = new Thickness(0, 0, 0, 0);
            }
            else
            {
                MainFrame.Margin = new Thickness(0, -90, 0, 0);
            }
        }

        private void LoadOrShowPage(Type PageType)
        {
            Page p = (from p1 in mPageList where p1.GetType() == PageType select p1).SingleOrDefault();

            // Page not found so create
            if (p == null)
            {
                try
                {
                    // TODO: show loading message with spinner
                    p = (Page)Activator.CreateInstance(PageType);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
                mPageList.Add(p);
            }

            ShowPage(p);
        }

        private void ShowPage(Page page)
        {
            RibbonTab rt = (RibbonTab)MainRibbon.SelectedItem;
            MainFrame.Content = page;
            // save the current page in the tag, so can return it back in tab click
            rt.Tag = page;
        }

    
        
        private void btnOpenSolutions_Click(object sender, RoutedEventArgs e)
        {
            OpenSolution();
        }

        private void btnNewSolutions_Click(object sender, RoutedEventArgs e)
        {
            Solution s1 = new Solution();
            AddSolutionPage addSol = new AddSolutionPage(s1);
            addSol.ShowAsWindow();
        }

        private void OpenSolution()
        {
            string solutionFolder = General.OpenSelectFolderDialog("Select Ginger Solution Folder");
            if (solutionFolder != null)
            {
                string solutionFileName = solutionFolder + @"\Ginger.Solution.xml";
                //string realPath = Path.GetFullPath(SoFileName);
                //string ll = PathHelper.GetLongPath(realPath);
                if (System.IO.File.Exists(PathHelper.GetLongPath(solutionFileName)))
                {
                    if (App.SetSolution(Path.GetDirectoryName(PathHelper.GetLongPath(solutionFolder))))
                        App.UserProfile.AddsolutionToRecent(Path.GetDirectoryName(solutionFolder));
                }
                else
                {
                    Reporter.ToUser(eUserMsgKeys.SolutionFileNotFound, solutionFileName);
                }
            }
        }

        public void ResetSolutionDependedUIElements(bool enableTabs)
        {
            if (enableTabs)
            {
                if (App.UserProfile.UserTypeHelper.IsSupportAutomate)
                {
                    //AutomateRibbon.Visibility = Visibility.Visible;
                    xRun.Visibility = Visibility.Visible;
                }
                else
                {
                    //AutomateRibbon.Visibility = Visibility.Collapsed;
                    xRun.Visibility = Visibility.Collapsed;
                }

                if (App.UserProfile.UserTypeHelper.IsSupportAnalyzer)
                    AnalyzerButton.Visibility = Visibility.Visible;
                else
                    AnalyzerButton.Visibility = Visibility.Collapsed;

                if (App.UserProfile.UserTypeHelper.IsSupportALM)
                    ALMConfigurationsGroup.Visibility = Visibility.Visible;
                else
                    ALMConfigurationsGroup.Visibility = Visibility.Collapsed;

                if (App.UserProfile.UserTypeHelper.IsSupportSupport)
                    SupportRibbon.Visibility = Visibility.Visible;
                else
                    SupportRibbon.Visibility = Visibility.Collapsed;
               
                btnUpgrade.Visibility = Visibility.Visible;
                ViewSolutionFiles.Visibility = Visibility.Visible;
                xFindAndReplaceSolutionPageButton.Visibility = Visibility.Visible;
                xResources.Visibility = Visibility.Visible;
                xBusinessFlows.Visibility = Visibility.Visible;
                xConfigurations.Visibility = Visibility.Visible;
                btnRecover.Visibility = Visibility.Visible;
            }
            else
            {
                //AutomateRibbon.Visibility = Visibility.Collapsed;
                xRun.Visibility = Visibility.Collapsed;
                xResources.Visibility = Visibility.Collapsed;
                xBusinessFlows.Visibility = Visibility.Collapsed;
                xConfigurations.Visibility = Visibility.Collapsed;               
                btnUpgrade.Visibility = Visibility.Collapsed;
                btnRecover.Visibility = Visibility.Collapsed;
                ViewSolutionFiles.Visibility = Visibility.Collapsed;
                xFindAndReplaceSolutionPageButton.Visibility = Visibility.Collapsed;
            }

        }

        private void NewTicketbutton_Click(object sender, RoutedEventArgs e)
        {
            AutoLogProxy.UserOperationStart("NewTicketbutton_Click");

            try
            {
                WebBrowser WB = ((WebSupportPage)mPageList.Where(p => p.GetType() == typeof(WebSupportPage)).FirstOrDefault()).ViewWebBrowser;
                WB.Navigate("http://ginger/Ticket/Create");
            }
            catch (Exception)
            {
                //TODO: add different flow for offline click of New Ticket
            }
            AutoLogProxy.UserOperationEnd();
        }

        //Support tab back button for web page
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ((WebSupportPage)mPageList.Where(p => p.GetType() == typeof(WebSupportPage)).FirstOrDefault()).ViewWebBrowser.GoBack();
            }
            catch (Exception) { }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ((WebSupportPage)mPageList.Where(p => p.GetType() == typeof(WebSupportPage)).FirstOrDefault()).ViewWebBrowser.Refresh();
            }
            catch (Exception) { }
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

        public void SaveAppCurrentItem()
        {

            RepositoryItemBase RIS = null;
            bool OnRunPage = false;

            RibbonTab Selectedtab = MainRibbon.SelectedItem as RibbonTab;
            switch (Selectedtab.Header.ToString())
            {

                case "Solution":
                    RIS = App.CurrentRepositoryItem;
                    break;
                case "Automate":
                    RIS = App.BusinessFlow;
                    break;
                default:
                    Reporter.ToUser(eUserMsgKeys.CtrlSMissingItemToSave);
                    break;
            }

            if (!OnRunPage)
            {
                if (RIS == null)
                    Reporter.ToUser(eUserMsgKeys.CtrlSMissingItemToSave);

                else if (RIS is EnvApplication)
                {
                    Reporter.ToUser(eUserMsgKeys.CtrlSsaveEnvApp);
                    return;
                }

                else
                {
                    Reporter.ToGingerHelper(eGingerHelperMsgKey.SaveItem, null, RIS.GetNameForFileName(), "item");
                    RIS.SaveToFile(RIS.FileName);
                    Reporter.CloseGingerHelper();
                }
            }
        }

        private void RibbonWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            {
                throw new NotImplementedException();
                //   App.LocalRepository.SaveAllSolutionDirtyItems(true);
            }
            else if (e.Key == Key.S && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                SaveAppCurrentItem();
            }
            // Alt + CTRL + Shift + G = show beta features
            else if (e.Key == Key.G && Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.LeftShift) && Keyboard.IsKeyDown(Key.LeftAlt))
            {
                BetaFeaturesPage p = new BetaFeaturesPage();
                p.ShowAsWindow();
            }
            else if (e.Key == Key.F && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                SolutionFindAndReplace();
            }
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

        public void RefreshSolutionTabRibbon()
        {
            //handle solution ribbon tools
            if (App.UserProfile.Solution == null)
            {                
                ViewSolutionFiles.IsEnabled = false;
                CheckInSolutionBtn.IsEnabled = false;
                GetLatestSolutionBtn.IsEnabled = false;
                btnUpgrade.IsEnabled = false;
            }
            else
            {                
                ViewSolutionFiles.IsEnabled = true;
                CheckInSolutionBtn.IsEnabled = true;
                GetLatestSolutionBtn.IsEnabled = true;
                btnUpgrade.IsEnabled = true;
            }
        }

     

        private void btnSourceControlConnectionDetails_Click(object sender, RoutedEventArgs e)
        {
            SourceControlConnDetailsPage p = new SourceControlConnDetailsPage();
            p.ShowAsWindow(eWindowShowStyle.Dialog);
        }

        private void btnSourceControlDownloadSolution_Click(object sender, RoutedEventArgs e)
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

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Application.Current.Shutdown();
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

        private void MainRibbon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // check where click was done
            Point pt = e.GetPosition((UIElement)sender);
            HitTestResult result = VisualTreeHelper.HitTest(this, pt);
            if (result != null)
            {
                var element = result.VisualHit;
                while (element != null && !(element is RibbonTabHeader))
                {
                    element = VisualTreeHelper.GetParent(element);
                }
                if (element != null)
                {
                    // meaning here - click was done at RibbonTabHeader!
                    if (MainRibbon.SelectedIndex == _currentClickedTabIndex)
                    {
                        ((RibbonTab)MainRibbon.Items[MainRibbon.SelectedIndex]).Ribbon.IsMinimized = false; // meaning - third click done on same tab - IsMinimized may be used by WPF - to override it
                        //  MainFrame.Margin = new Thickness(0, 85, 0, 0);
                        _currentClickedTabIndex = -1;
                    }
                    else
                    {
                        //  MainFrame.Margin = new Thickness(0, 0, 0, 0);
                        _currentClickedTabIndex = MainRibbon.SelectedIndex; // meaning - second click done on same tab
                    }
                }
            }
        }

        private void MainRibbon_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // check where click was done
            Point pt = e.GetPosition((UIElement)sender);
            HitTestResult result = VisualTreeHelper.HitTest(this, pt);
            if (result != null)
            {
                var element = result.VisualHit;
                while (element != null && !(element is RibbonTabHeader))
                {
                    element = VisualTreeHelper.GetParent(element);
                }
                if (element != null)
                {
                    // meaning here - click was done at RibbonTabHeader!
                    if (MainRibbon.SelectedIndex == _currentClickedTabIndex)
                    {
                        ((RibbonTab)MainRibbon.Items[MainRibbon.SelectedIndex]).Ribbon.IsMinimized = false; // meaning - third click done on same tab - IsMinimized may be used by WPF - to override it
                        //  MainFrame.Margin = new Thickness(0, 85, 0, 0);
                        _currentClickedTabIndex = -1;                      // meaning - third click done on same tab - IsMinimized may be used by WPF - to override it
                    }
                    else
                    {
                        //  MainFrame.Margin = new Thickness(0, 0, 0, 0);
                        _currentClickedTabIndex = MainRibbon.SelectedIndex; // meaning - second click done on same tab
                    }
                }
            }
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

        //public void CheckIfUserWantToSaveCurrentBF(BusinessFlow newBF)
        //{
        //    if (newBF != App.BusinessFlow)
        //    {
        //        //check if Automate tab was used before
        //        bool automateTabExist = false;
        //        foreach (Page page in mPageList)
        //            if (page.GetType() == typeof(AutomatePage)) automateTabExist = true;
        //        if (!automateTabExist) return;

        //        if ((newBF != null) && App.BusinessFlow != null && Reporter.ToUser(eUserMsgKeys.IFSaveChangesOfBF, App.BusinessFlow.Name) == MessageBoxResult.Yes)
        //        {
        //            Reporter.ToGingerHelper(eGingerHelperMsgKey.SaveItem, null, App.BusinessFlow.Name,
        //                          GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
        //            WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(App.BusinessFlow);
        //            Reporter.CloseGingerHelper();
        //        }
        //    }
        //}

        private void btnSourceControlRepositoryDetails_Click(object sender, RoutedEventArgs e)
        {

            SourceControlItemInfoDetails SCIInfoDetails = SourceControlIntegration.GetRepositoryInfo(App.UserProfile.Solution.SourceControl);
            SourceControlItemInfoPage SCIIP = new SourceControlItemInfoPage(SCIInfoDetails);
            SCIIP.ShowAsWindow();
        }

        private void GingerForum(object sender, RoutedEventArgs e)
        {
            AutoLogProxy.UserOperationStart("NewTicketbutton_Click");

            try
            {
                WebBrowser WB = ((WebSupportPage)mPageList.Where(p => p.GetType() == typeof(WebSupportPage)).FirstOrDefault()).ViewWebBrowser;
                WB.Navigate("http://ginger:81");

            }
            catch (Exception)
            {
                //TODO: add different flow for offline click of New Ticket
            }

            AutoLogProxy.UserOperationEnd();
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

        private void SupportSitebutton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WebBrowser WB = ((WebSupportPage)mPageList.Where(p => p.GetType() == typeof(WebSupportPage)).FirstOrDefault()).ViewWebBrowser;
                WB.Navigate("http://ginger/");
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }
        }

        private void ImportFeatureFile_Click(object sender, RoutedEventArgs e)
        {
            BDDIntegration BDDI = new BDDIntegration();
            bool imported = BDDI.ImportFeatureFile();

            //Page SolutionPage = App.PageList.Where(x => x.Title == "Solution Explorer").FirstOrDefault();
            ////((SolutionExplorerPage)SolutionPage).LoadSoultionTree2();
            //object o = ((SolutionExplorerPage)SolutionPage).SolutionTreeView.Tree.GetItemAt(0);
            //((TreeViewItem)((TreeViewItem)o).Items[0]).IsSelected = true;
            //foreach (TreeViewItem item in ((ItemCollection)((TreeViewItem)o).Items))
            //{
            //    if (item.Tag is BusinessFlowsFolderTreeItem)
            //    {
            //        ((Ginger.SolutionWindows.TreeViewItems.BusinessFlowsFolderTreeItem)item.Tag).mTreeView.Tree.ExpandTreeNodeByName("Business Flows", Refresh: true, ExpandAll: false);
            //        ((Ginger.SolutionWindows.TreeViewItems.BusinessFlowsFolderTreeItem)item.Tag).mTreeView.Tree.ExpandTreeNodeByName("Documents", Refresh: true, ExpandAll: true);
            //        ((Ginger.SolutionWindows.TreeViewItems.BusinessFlowsFolderTreeItem)item.Tag).mTreeView.Tree.ExpandTreeNodeByName("Features", Refresh: true, ExpandAll: true);
            //    }
            //}

        }

        private void CreateFeatureFile_Click(object sender, RoutedEventArgs e)
        {
            BDDIntegration BDDI = new BDDIntegration();
            BDDI.CreateFeatureFile();
            //RefreshSolutionPage();
            //Page SolutionPage = App.PageList.Where(x => x.Title == "Solution Explorer").FirstOrDefault();
            ////((SolutionExplorerPage)SolutionPage).LoadSoultionTree2();
            //object o = ((SolutionExplorerPage)SolutionPage).SolutionTreeView.Tree.GetItemAt(0);
            //((TreeViewItem)((TreeViewItem)o).Items[0]).IsSelected = true;
            //foreach (TreeViewItem item in ((ItemCollection)((TreeViewItem)o).Items))
            //{
            //    if (item.Tag is BusinessFlowsFolderTreeItem)
            //    {
            //        ((Ginger.SolutionWindows.TreeViewItems.BusinessFlowsFolderTreeItem)item.Tag).mTreeView.Tree.ExpandTreeNodeByName("Documents", Refresh: true, ExpandAll: true);
            //        ((Ginger.SolutionWindows.TreeViewItems.BusinessFlowsFolderTreeItem)item.Tag).mTreeView.Tree.ExpandTreeNodeByName("Features", Refresh: true, ExpandAll: true);
            //    }
            //}
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
            // DebugConsoleWindow.Show();
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

            //else if (MainRibbon.SelectedItem == xRun)
            //{
            //    NewRunSetPage runSetPage = (NewRunSetPage)(from p1 in mPageList where p1.GetType() == typeof(NewRunSetPage) select p1).SingleOrDefault();
            //    runSetPage.ShowFindAndReplacePage();
            //}
        }


        


        //public void AutomateBusinessFlow(BusinessFlow businessFlowToAutomate, bool loadAutomateTab = true)
        //{
        //    //App.MainWindow.CheckIfUserWantToSaveCurrentBF(businessFlowToAutomate);
            
        //}

        private void App_AutomateBusinessFlowEvent(AutomateEventArgs args)
        {
            if (args.EventType == AutomateEventArgs.eEventType.Automate)
            {
                //TODO: load Business Flows tab
                MainRibbon.SelectedItem = xBusinessFlows;
                App.BusinessFlow = (BusinessFlow)args.Object;
                App.BusinessFlow.SaveBackup();
            }
        }
    }
}