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
using Ginger.Actions;
using Ginger.Actions.ActionConversion;
using Ginger.Agents;
using Ginger.ALM;
using Ginger.ALM.QC;
using Ginger.AnalyzerLib;
using Ginger.BDD;
using Ginger.BusinessFlowFolder;
using Ginger.BusinessFlowWindows;
using Ginger.Dictionaries;
using Ginger.Environments;
using Ginger.Functionalities;
using Ginger.GeneralLib;
using Ginger.GeneralWindows;
using Ginger.GherkinLib;
using Ginger.MoveToGingerWPF;
using Ginger.Reports;
using Ginger.Run;
using Ginger.SolutionWindows;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.SourceControl;
using Ginger.Support;
using Ginger.UserConfig;
using Ginger.UserControlsLib.TextEditor;
using GingerCore;
using GingerCore.Actions;
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
using GingerWPF.BusinessFlowsLib;
using Amdocs.Ginger;
using Amdocs.Ginger.Repository;
using GingerCore.DataSource;
using GingerWPF.UserControlsLib;

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

            //ImportBusFlowMenuBtn.Label = "Import " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
            btnRunActivity.Label = "Run " + GingerDicser.GetTermResValue(eTermResKey.Activity);
            ContinuteRunActiviy.Header = "Continue Run from Current " + GingerDicser.GetTermResValue(eTermResKey.Activity);
            this.WindowState = System.Windows.WindowState.Maximized;

            //Make sure msgbox will apear running from Main Window STA
            Reporter.MainWindowDispatcher = this.Dispatcher;

            Reporter.HandlerGingerHelperEvent += Reporter_HandlerGingerHelperEvent;
            App.AutomateTabGingerRunner.GingerRunnerEvent += GingerRunner_GingerRunnerEvent;

            lblBetaFeatures.BindControl(WorkSpace.Instance.BetaFeatures, nameof(BetaFeatures.UsingStatus));
            ErrorsLabel.Visibility = Visibility.Collapsed;

            RefreshSolutionBtn.LargeImageSource = ImageMakerControl.GetImage(eImageType.Refresh, 32, 32);
            RefreshSolutionBtn.SmallImageSource = ImageMakerControl.GetImage(eImageType.Refresh, 16, 16);

            btnRefresh.LargeImageSource = ImageMakerControl.GetImage(eImageType.Refresh, 32, 32);
            btnRefresh.SmallImageSource = ImageMakerControl.GetImage(eImageType.Refresh, 16, 16);


            btnResetStatus.LargeImageSource = ImageMakerControl.GetImage(eImageType.Reset, 32, 32);
            btnResetStatus.SmallImageSource = ImageMakerControl.GetImage(eImageType.Reset, 16, 16);
            btnResetFlow.ImageSource = ImageMakerControl.GetImage(eImageType.Reset, 14, 14);
            btnResetFromCurrentActivity.ImageSource = ImageMakerControl.GetImage(eImageType.Reset, 14, 14);
            btnResetFromCurrentAction.ImageSource = ImageMakerControl.GetImage(eImageType.Reset, 14, 14);

            btnRecover.LargeImageSource = ImageMakerControl.GetImage(eImageType.Reset, 32, 32);
            btnRecover.SmallImageSource = ImageMakerControl.GetImage(eImageType.Reset, 16, 16);

            SetNewAutomateRibbon();
        }

        private void SetNewAutomateRibbon()
        {
            // TODO: use Bind on feature to visible

            if (WorkSpace.Instance.BetaFeatures.ShowNewautomate)
            {
                xNewAutomate.Visibility = Visibility.Visible;
            }
            else
            {
                xNewAutomate.Visibility = Visibility.Collapsed;
            }

        }

        // same as GingerCoreNET
        private void GingerRunner_GingerRunnerEvent(GingerCoreNET.RunLib.GingerRunnerEventArgs EventArgs)
        {
            this.Dispatcher.Invoke(() => {
                switch (EventArgs.EventType)
                {
                    case GingerRunnerEventArgs.eEventType.DoEventsRequired:
                        GingerCore.General.DoEvents();
                        break;
                }
            });
        }

        private async void  Reporter_HandlerGingerHelperEvent(GingerHelperEventArgs e)
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
                cboSpeed.Text = "0";
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


                App.ObjFieldBinding(SimulationMode, CheckBox.IsCheckedProperty, App.AutomateTabGingerRunner, Ginger.Run.GingerRunner.Fields.RunInSimulationMode);

                if (App.UserProfile.NewHelpLibraryMessgeShown == false)
                {
                    Reporter.ToGingerHelper(eGingerHelperMsgKey.GingerHelpLibrary);
                    App.UserProfile.NewHelpLibraryMessgeShown = true;
                }

                AppAgentsMappingExpander2Frame.Content = new ApplicationAgentsMapPage(App.AutomateTabGingerRunner);
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
            if (e.PropertyName == "Solution")
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
                lstEnvs.ItemsSource = null;
                App.UpdateApplicationsAgentsMapping();

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
            if (e.PropertyName == "BusinessFlow")
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


            if (MainRibbon.SelectedItem == AutomateRibbon)
            {
                if (App.BusinessFlow == null)
                {
                    //TODO: load last biz flow the user worked on 
                    //else load new
                    App.LoadDefaultBusinessFlow();
                }

                LoadOrShowPage(typeof(AutomatePage));
                return;
            }

            if (MainRibbon.SelectedItem == RunRibbon)
            {
                LoadOrShowPage(typeof(NewRunSetPage));
            }

            if (MainRibbon.SelectedItem == SupportRibbon)
            {
                LoadOrShowPage(typeof(WebSupportPage));
                return;
            }

            if (MainRibbon.SelectedItem == SolutionRibbon)
            {

                LoadOrShowPage(typeof(SolutionExplorerPage));
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

            if (MainRibbon.SelectedItem == xNewAutomate)
            {

                LoadOrShowPage(typeof(BusinessFlowAutomatePage));
                return;
            }

            if (MainRibbon.SelectedItem == xBusinessFlows)
            {
                BusinessFlowsFolderTreeItem busFlowsRootFolder = new BusinessFlowsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<BusinessFlow>());
                busFlowsRootFolder.IsGingerDefualtFolder = true;
                SingleItemTreeViewExplorerPage busFlowsPage = new SingleItemTreeViewExplorerPage("Business Flows", eImageType.BusinessFlow, busFlowsRootFolder, busFlowsRootFolder.SaveAllTreeFolderItemsHandler, busFlowsRootFolder.AddItemHandler);                
                ShowPage(busFlowsPage);
                return;
            }


        }

        public void ToggleMainWindow()
        {
            if (MainRibbon.SelectedItem != RunRibbon && MainRibbon.SelectedItem != xResources && MainRibbon.SelectedItem != xConfigurations && MainRibbon.SelectedItem != xBusinessFlows)
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


        private void StartAgent_Click(object sender, RoutedEventArgs e)
        {
            AutoLogProxy.UserOperationStart("StartAgent_Click");

            string agentsNames = App.AutomateTabGingerRunner.GetAgentsNameToRun();
            Reporter.ToGingerHelper(eGingerHelperMsgKey.StartAgents, null, agentsNames);

            App.AutomateTabGingerRunner.StopAgents();
            SetAutomateTabRunnerForExecution();
            App.AutomateTabGingerRunner.StartAgents();

            Reporter.CloseGingerHelper();
            AutoLogProxy.UserOperationEnd();
        }

        public void RunActionButton_Click(object sender, RoutedEventArgs e)
        {
            RunCurrentAutomatePageAction();
        }

        public void FloatingRunActionButton_Click(object sender, RoutedEventArgs e)
        {
            RunCurrentAutomatePageAction(false);
        }
        public void FloatingContinueRunActionButton_Click(object sender, RoutedEventArgs e)
        {
            ContinueRunFRomAutomateTab(Run.GingerRunner.eContinueFrom.SpecificAction);
        }
        public void FloatingContinueRunActivityButton_Click(object sender, RoutedEventArgs e)
        {
            ContinueRunFRomAutomateTab(Run.GingerRunner.eContinueFrom.SpecificActivity);
        }
        public async void RunCurrentAutomatePageAction(bool checkIfActionAllowedToRun = true)
        {
            AutoLogProxy.UserOperationStart("RunActionButton_Click", App.UserProfile.Solution.Name, App.GetProjEnvironmentName());

            //TODO: Check if grid we are in execution view, no need to try and change of already in correct view
            btnGridViewExecution_Click(null, null);

            if (App.BusinessFlow.CurrentActivity.Acts.Count() == 0)
            {
                Reporter.ToUser(eUserMsgKeys.StaticInfoMessage, "No Action to Run.");
                return;
            }

            SetAutomateTabRunnerForExecution();

            // If no action selected move to the first.
            if (App.BusinessFlow.CurrentActivity.Acts.CurrentItem == null && App.BusinessFlow.CurrentActivity.Acts.Count() > 0)
            {
                App.BusinessFlow.CurrentActivity.Acts.CurrentItem = App.BusinessFlow.CurrentActivity.Acts[0];
            }

            //No need of agent for actions like DB and read for excel. For other need agent   
            if (!(typeof(ActWithoutDriver).IsAssignableFrom(App.AutomateTabGingerRunner.CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem.GetType()))|| App.BusinessFlow.CurrentActivity.Acts.CurrentItem.GetType()==typeof(ActAgentManipulation))
            {
                App.AutomateTabGingerRunner.SetCurrentActivityAgent();
            }

            App.AutomateTabGingerRunner.ExecutionLogger.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.ActionRun;
            var result = await App.AutomateTabGingerRunner.RunActionAsync((Act)App.BusinessFlow.CurrentActivity.Acts.CurrentItem, checkIfActionAllowedToRun, true).ConfigureAwait(false);

            if (App.AutomateTabGingerRunner.CurrentBusinessFlow.CurrentActivity.CurrentAgent!=null)
                App.AutomateTabGingerRunner.CurrentBusinessFlow.CurrentActivity.CurrentAgent.IsFailedToStart = false;

            AutoLogProxy.UserOperationEnd();
        }

        private void btnRunActivity_Click(object sender, RoutedEventArgs e)
        {
            ActionsPage ActPage = (ActionsPage)((AutomatePage)MainFrame.Content).ActivityActionsFrame.Content;
            try
            {
                btnGridViewExecution_Click(sender, e);

                DisableGridSelectedItemChangeOnClick(ActPage.grdActions);
                AutoLogProxy.UserOperationStart("btnRunActivity_Click", App.UserProfile.Solution.Name, App.GetProjEnvironmentName());

                SetAutomateTabRunnerForExecution();

                App.AutomateTabGingerRunner.ExecutionLogger.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.ActivityRun;
                RunActivity();
                AutoLogProxy.UserOperationEnd();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                EnabelGridSelectedItemChangeOnClick(ActPage.grdActions);
                if (App.AutomateTabGingerRunner.CurrentBusinessFlow.CurrentActivity.CurrentAgent!=null)
                    App.AutomateTabGingerRunner.CurrentBusinessFlow.CurrentActivity.CurrentAgent.IsFailedToStart = false;
            }
        }

        public async void RunActivity()
        {
            await App.AutomateTabGingerRunner.RunActivityAsync(App.BusinessFlow.CurrentActivity, false).ConfigureAwait(false);

            //When running Runactivity as standalone from GUI, SetActionSkipStatus is not called. Handling it here for now.
            foreach (Act act in App.BusinessFlow.CurrentActivity.Acts)
            {
                if (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending)
                    act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
            }
        }

        private void SetGingerRunnerSpeed()
        {
            App.AutomateTabGingerRunner.SetSpeed(int.Parse(cboSpeed.Text));
        }

        private void btnRunFlow_Click(object sender, RoutedEventArgs e)
        {
            RunAutomateTabFlow(true);
        }

        private void btnRunFlowNoAnaylze_Click(object sender, RoutedEventArgs e)
        {
            RunAutomateTabFlow();
        }
        private void btnRunFlowAndGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            RunAutomateTabFlow(true,true);
        }
        private async void RunAutomateTabFlow(bool Analyz=false, bool ReportNeeded=false)
        {
            if(Analyz)
            {
                //Run Analyzer check if not including any High or Critical issues before execution
                Reporter.ToGingerHelper(eGingerHelperMsgKey.AnalyzerIsAnalyzing, null, App.BusinessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                try
                {
                    AnalyzerPage analyzerPage = new AnalyzerPage();
                    analyzerPage.Init(App.UserProfile.Solution, App.BusinessFlow);
                    analyzerPage.AnalyzeWithoutUI();
                    while (analyzerPage.IsAnalyzeDone == false)
                        System.Threading.Thread.Sleep(100);
                    Reporter.CloseGingerHelper();
                    if (analyzerPage.TotalHighAndCriticalIssues > 0)
                    {
                        Reporter.ToUser(eUserMsgKeys.AnalyzerFoundIssues);
                        analyzerPage.ShowAsWindow();
                        return;
                    }
                }
                finally
                {
                    Reporter.CloseGingerHelper();
                }
            }
            try
            {
                if(ReportNeeded)
                {
                    RunAutomateTabFlowConf("Run Automate Tab Flow And Generate Report");
                }
                else
                {
                    RunAutomateTabFlowConf("Run Automate Tab Flow");
                }
                //execute
                await App.AutomateTabGingerRunner.RunBusinessFlowAsync(App.BusinessFlow, true, false).ConfigureAwait(false);
                this.Dispatcher.Invoke(() =>
                {
                    AutoLogProxy.UserOperationEnd();
                    if (ReportNeeded)
                    {
                        btnLastExecutionHTMLReport_click(this, null);
                    }
                    else
                    {
                        ExecutionSummaryPage w = new ExecutionSummaryPage(App.BusinessFlow);
                        w.ShowAsWindow();
                    }
                });

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //enable grids
                EnableDisableAutomateTabGrids(true);
                App.AutomateTabGingerRunner.ResetFailedToStartFlagForAgents();
            }
        }
        private void RunAutomateTabFlowConf(string runType)
        {
            btnGridViewExecution_Click(null, null);//shift to execution view

            AutoLogProxy.UserOperationStart(runType, App.UserProfile.Solution.Name, App.GetProjEnvironmentName());

            //disable grids  
            EnableDisableAutomateTabGrids(false);

            //execute preperations
            SetAutomateTabRunnerForExecution();
            App.AutomateTabGingerRunner.ResetRunnerExecutionDetails(true);
            App.AutomateTabGingerRunner.ExecutionLogger.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.BussinessFlowRun;
        }

        // Run Preparations before execution of Action/activity/Flow/start agent
        public void SetAutomateTabRunnerForExecution()
        {
            App.AutomateTabGingerRunner.ProjEnvironment = App.AutomateTabEnvironment;            
            App.AutomateTabGingerRunner.SolutionFolder = App.UserProfile.Solution.Folder;
            App.AutomateTabGingerRunner.DSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
            App.AutomateTabGingerRunner.SolutionAgents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>();
            //App.AutomateTabGingerRunner.PlugInsList = App.LocalRepository.GetSolutionPlugIns();
            App.AutomateTabGingerRunner.SolutionApplications = App.UserProfile.Solution.ApplicationPlatforms;

            SetGingerRunnerSpeed();
        }



        public void btnLastExecutionHTMLReport_click(object sender, RoutedEventArgs e)
        {
            ExecutionLoggerConfiguration _selectedExecutionLoggerConfiguration = App.UserProfile.Solution.ExecutionLoggerConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            if (!_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled)
            {
                Reporter.ToUser(eUserMsgKeys.ExecutionsResultsProdIsNotOn);
                return;
            }
            HTMLReportsConfiguration currentConf = App.UserProfile.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            //get logger files
            string exec_folder = Ginger.Run.ExecutionLogger.GetLoggerDirectory(_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationExecResultsFolder + "\\" + Ginger.Run.ExecutionLogger.defaultAutomationTabLogName);
            //create the report
            string reportsResultFolder = Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(exec_folder), true, null, null, false, currentConf.HTMLReportConfigurationMaximalFolderSize);

            if (reportsResultFolder == string.Empty)
            {
                Reporter.ToUser(eUserMsgKeys.AutomationTabExecResultsNotExists);
                return;
            }
            else
            {
                foreach (string txt_file in System.IO.Directory.GetFiles(reportsResultFolder))
                {
                    string fileName = Path.GetFileName(txt_file);
                    if (fileName.Contains(".html"))
                    {
                        Process.Start(reportsResultFolder);
                        Process.Start(reportsResultFolder + "\\" + fileName);
                    }
                }
            }
        }

        private void btnOfflineExecutionHTMLReport_click(object sender, RoutedEventArgs e)
        {
            ExecutionLoggerConfiguration _selectedExecutionLoggerConfiguration = App.UserProfile.Solution.ExecutionLoggerConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            if (!_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled)
            {
                Reporter.ToUser(eUserMsgKeys.ExecutionsResultsProdIsNotOn);
                return;
            }
            HTMLReportsConfiguration currentConf = App.UserProfile.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            //create the execution logger files            
            string exec_folder = Ginger.Run.ExecutionLogger.GetLoggerDirectory(_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationExecResultsFolder + "\\" + Ginger.Run.ExecutionLogger.defaultAutomationTabOfflineLogName);

            if (Directory.Exists(exec_folder))
                GingerCore.General.ClearDirectoryContent(exec_folder);
            else
                Directory.CreateDirectory(exec_folder);
            if (App.AutomateTabGingerRunner.ExecutionLogger.OfflineBusinessFlowExecutionLog(App.BusinessFlow, exec_folder))
            {
                //create the HTML report
                try
                {
                    string reportsResultFolder = Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(exec_folder), true, null, null, false, currentConf.HTMLReportConfigurationMaximalFolderSize);
                    if (reportsResultFolder == string.Empty)
                    {
                        Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Failed to generate the report for the '" + App.BusinessFlow.Name + "' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ", please execute it fully first.");
                        return;
                    }
                    else
                    {
                        foreach (string txt_file in System.IO.Directory.GetFiles(reportsResultFolder))
                        {
                            string fileName = Path.GetFileName(txt_file);
                            if (fileName.Contains(".html"))
                            {
                                Process.Start(reportsResultFolder);
                                Process.Start(reportsResultFolder + "\\" + fileName);
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    Reporter.ToLog(eLogLevel.WARN, "Failed to generate offline full business flow report", ex);
                    Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Failed to generate the report for the '" + App.BusinessFlow.Name + "' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ", please execute it fully first.");
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Failed to generate the report for the '" + App.BusinessFlow.Name + "' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ", please execute it fully first.");
            }
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
                    if(App.SetSolution(Path.GetDirectoryName(PathHelper.GetLongPath(solutionFolder))))
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
                    AutomateRibbon.Visibility = Visibility.Visible;
                    RunRibbon.Visibility = Visibility.Visible;
                }
                else
                {
                    AutomateRibbon.Visibility = Visibility.Collapsed;
                    RunRibbon.Visibility = Visibility.Collapsed;
                }

                if (App.UserProfile.UserTypeHelper.IsSupportAnalyzer)
                    AnalyzerButton.Visibility  = Visibility.Visible;
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

                RefreshSolutionBtn.Visibility = Visibility.Visible;
                SaveAllBtn.Visibility = Visibility.Visible;
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
                AutomateRibbon.Visibility = Visibility.Collapsed;
                RunRibbon.Visibility = Visibility.Collapsed;
                xResources.Visibility = Visibility.Collapsed;
                xBusinessFlows.Visibility = Visibility.Collapsed;
                xConfigurations.Visibility = Visibility.Collapsed;
                RefreshSolutionBtn.Visibility = Visibility.Collapsed;
                SaveAllBtn.Visibility = Visibility.Collapsed;
                btnUpgrade.Visibility = Visibility.Collapsed;
                btnRecover.Visibility = Visibility.Collapsed;
                ViewSolutionFiles.Visibility = Visibility.Collapsed;
                xFindAndReplaceSolutionPageButton.Visibility = Visibility.Collapsed;
            }

        }

        private void lstEnvs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstEnvs != null && lstEnvs.SelectedItem != null)
                App.AutomateTabEnvironment = (ProjEnvironment)lstEnvs.SelectedItem;
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

        private void StopRunButton_Click(object sender, RoutedEventArgs e)
        {
            StopAutomateRun();
        }

        public void StopAutomateRun()
        {
            try
            {
                App.AutomateTabGingerRunner.StopRun();
                AutoLogProxy.UserOperationStart("StopRunButton_Click");
            }
            finally
            {
                EnableDisableAutomateTabGrids(true);
                AutoLogProxy.UserOperationEnd();
            }
        }

        private void EnableDisableAutomateTabGrids(bool enableGrids)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    ActivitiesPage AP = null;
                    if (((AutomatePage)MainFrame.Content).BFActivitiesFrame.Content.GetType() == typeof(ActivitiesPage)) // it can be the Activities MiniView
                        AP = (ActivitiesPage)((AutomatePage)MainFrame.Content).BFActivitiesFrame.Content;

                    ActionsPage ActPage = (ActionsPage)((AutomatePage)MainFrame.Content).ActivityActionsFrame.Content;
                    if (ActPage != null)
                        if (enableGrids)
                            EnabelGridSelectedItemChangeOnClick(ActPage.grdActions);
                        else
                            DisableGridSelectedItemChangeOnClick(ActPage.grdActions);

                    if (AP != null)
                        if (enableGrids)
                            EnabelGridSelectedItemChangeOnClick(AP.grdActivities);
                        else
                            DisableGridSelectedItemChangeOnClick(AP.grdActivities);
                });
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to disable Automate Tab grids for execution", ex);
            }
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
            else if (e.Key == Key.G && Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.LeftShift)  && Keyboard.IsKeyDown(Key.LeftAlt))
            {
                BetaFeaturesPage p = new BetaFeaturesPage();
                p.ShowAsWindow();
            }
            else if (e.Key == Key.F && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                FindAndReplace();
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
                RefreshSolutionBtn.IsEnabled = false;
                ViewSolutionFiles.IsEnabled = false;
                CheckInSolutionBtn.IsEnabled = false;
                GetLatestSolutionBtn.IsEnabled = false;
                btnUpgrade.IsEnabled = false;
            }
            else
            {
                RefreshSolutionBtn.IsEnabled = true;
                ViewSolutionFiles.IsEnabled = true;
                CheckInSolutionBtn.IsEnabled = true;
                GetLatestSolutionBtn.IsEnabled = true;
                btnUpgrade.IsEnabled = true;
            }
        }

        public void RefreshSolution_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RefreshSolutionBtn.IsEnabled = false;
                RefreshSolutionPage();
            }
            finally
            {
                RefreshSolutionBtn.IsEnabled = true;
            }
        }

        public void RefreshSolutionPage(SolutionExplorerPage.eRefreshSolutionType refreshType = SolutionExplorerPage.eRefreshSolutionType.InitAllPage, Type treeFolderType = null, bool RefreshSol = true)
        {
            SolutionExplorerPage solExplorPage = (SolutionExplorerPage)(from p1 in mPageList where p1.GetType() == typeof(SolutionExplorerPage) select p1).FirstOrDefault();
            if (solExplorPage != null)
            {
                if (refreshType == SolutionExplorerPage.eRefreshSolutionType.InitAllPage)
                    solExplorPage.Init(App.UserProfile.Solution, RefreshSol);
                else
                    solExplorPage.RefreshTreeItemFolder(treeFolderType);
            }
        }

       

        private void btnGridViewAll_Click(object sender, RoutedEventArgs e)
        {
            AutomatePage autoPage =
                   (AutomatePage)(from p1 in mPageList where p1.GetType() == typeof(AutomatePage) select p1).FirstOrDefault();
            if (autoPage != null)
                autoPage.SetGridsView(Ginger.UserControls.GridViewDef.DefaultViewName);
        }

        private void btnGridViewDesign_Click(object sender, RoutedEventArgs e)
        {
            AutomatePage autoPage =
       (AutomatePage)(from p1 in mPageList where p1.GetType() == typeof(AutomatePage) select p1).FirstOrDefault();
            if (autoPage != null)
                autoPage.SetGridsView(eAutomatePageViewStyles.Design.ToString());
        }

        private void btnGridViewExecution_Click(object sender, RoutedEventArgs e)
        {
            AutomatePage autoPage =
       (AutomatePage)(from p1 in mPageList where p1.GetType() == typeof(AutomatePage) select p1).FirstOrDefault();
            if (autoPage != null)
                autoPage.SetGridsView(eAutomatePageViewStyles.Execution.ToString());
        }

        private void btnSourceControlConnectionDetails_Click(object sender, RoutedEventArgs e)
        {
            SourceControlConnDetailsPage p = new SourceControlConnDetailsPage();
            p.ShowAsWindow(eWindowShowStyle.Dialog);
        }

        private void SaveBizFlowButton_Click(object sender, RoutedEventArgs e)
        {
            //warn in case dynamic shared reposiotry Activities are included and going to be deleted
            if (App.BusinessFlow.Activities.Where(x=>x.AddDynamicly == true).FirstOrDefault() != null)
            {
                if (Reporter.ToUser(eUserMsgKeys.WarnOnDynamicActivities) == MessageBoxResult.No)
                    return;
            }

            Reporter.ToGingerHelper(eGingerHelperMsgKey.SaveItem, null, App.BusinessFlow.Name,
                                      GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
            
            WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(App.BusinessFlow);

            Reporter.CloseGingerHelper();
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

            SolutionExplorerPage solPage =
      (SolutionExplorerPage)(from p1 in mPageList where p1.GetType() == typeof(SolutionExplorerPage) select p1).FirstOrDefault();
            if (solPage != null)
                App.CheckIn(App.UserProfile.Solution.Folder);

            AutoLogProxy.UserOperationEnd();
        }

        private void btnSourceControlGetLatest_Click(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKeys.LoseChangesWarn) == MessageBoxResult.No) return;

            AutoLogProxy.UserOperationStart("btnSourceControlGetLatest_Click");
            SolutionExplorerPage solPage =
                  (SolutionExplorerPage)(from p1 in mPageList where p1.GetType() == typeof(SolutionExplorerPage) select p1).FirstOrDefault();
            if (solPage != null)
            {
                Reporter.ToGingerHelper(eGingerHelperMsgKey.GetLatestFromSourceControl);
                if (string.IsNullOrEmpty(App.UserProfile.Solution.Folder))
                    Reporter.ToUser(eUserMsgKeys.SourceControlUpdateFailed, "Invalid Path provided");
                else
                    SourceControlIntegration.GetLatest(App.UserProfile.Solution.Folder, App.UserProfile.Solution.SourceControl);

                if (Reporter.ToUser(eUserMsgKeys.RefreshWholeSolution) == MessageBoxResult.Yes)
                    RefreshSolutionPage();
                //App.GingerRunner.UpdateApplicationAgents();
                App.UpdateApplicationsAgentsMapping(false);
                Reporter.CloseGingerHelper();
            }

            AutoLogProxy.UserOperationEnd();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Application.Current.Shutdown();
        }

        private void btnContinute_Click(object sender, RoutedEventArgs e)
        {
            ContinueRunFRomAutomateTab(Run.GingerRunner.eContinueFrom.LastStoppedAction);
        }

        private void ContinuteRunActiviytButton_Click(object sender, RoutedEventArgs e)
        {
            ContinueRunFRomAutomateTab(Run.GingerRunner.eContinueFrom.SpecificActivity);
        }

        private async void ContinueRunFRomAutomateTab(Run.GingerRunner.eContinueFrom continueFrom)
        {
            try
            {
                btnGridViewExecution_Click(null, null);
                EnableDisableAutomateTabGrids(false);

                AutoLogProxy.UserOperationStart("ContinuteRunFrom"+ continueFrom.ToString() + "_Click", App.UserProfile.Solution.Name, App.GetProjEnvironmentName());
                App.AutomateTabGingerRunner.ExecutionLogger.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.ContinueRun;
                switch (continueFrom)
                {
                    case Run.GingerRunner.eContinueFrom.LastStoppedAction:
                        await App.AutomateTabGingerRunner.ContinueRunAsync(Run.GingerRunner.eContinueLevel.StandalonBusinessFlow, Run.GingerRunner.eContinueFrom.LastStoppedAction);
                        break;
                    case Run.GingerRunner.eContinueFrom.SpecificAction:
                        await App.AutomateTabGingerRunner.ContinueRunAsync(Run.GingerRunner.eContinueLevel.StandalonBusinessFlow, Run.GingerRunner.eContinueFrom.SpecificAction, App.BusinessFlow, App.BusinessFlow.CurrentActivity, (Act)App.BusinessFlow.CurrentActivity.Acts.CurrentItem);
                        break;
                    case Run.GingerRunner.eContinueFrom.SpecificActivity:
                        await App.AutomateTabGingerRunner.ContinueRunAsync(Run.GingerRunner.eContinueLevel.StandalonBusinessFlow, Run.GingerRunner.eContinueFrom.SpecificActivity, App.BusinessFlow, App.BusinessFlow.CurrentActivity);
                        break;
                }

                AutoLogProxy.UserOperationEnd();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                EnableDisableAutomateTabGrids(true);
            }
        }

        private void ContinuteRunButton_Click(object sender, RoutedEventArgs e)
        {
            ContinueRunFRomAutomateTab(Run.GingerRunner.eContinueFrom.SpecificAction);
        }

        private void AnalyzerButton_Click(object sender, RoutedEventArgs e)
        {
            AutoLogProxy.UserOperationStart("MainWindow.AnalyzerButton_Click");
            AnalyzerPage AP = new AnalyzerPage();
            AP.Init(App.UserProfile.Solution);
            AP.ShowAsWindow();
            AutoLogProxy.UserOperationEnd();
        }

        private void AutomateAnalyzerButton_Click(object sender, RoutedEventArgs e)
        {
            AutoLogProxy.UserOperationStart("AutomateAnalyzerButton_Click");

            AnalyzerPage AP = new AnalyzerPage();
            AP.Init(App.UserProfile.Solution, App.BusinessFlow);
            AP.ShowAsWindow();

            AutoLogProxy.UserOperationEnd();
        }

        private void RefreshFromALM_Click(object sender, RoutedEventArgs e)
        {
            if (App.BusinessFlow != null && App.BusinessFlow.ActivitiesGroups != null && App.BusinessFlow.ActivitiesGroups.Count > 0)
            {
                ALMIntegration.Instance.RefreshAllGroupsFromALM(App.BusinessFlow);
            }
        }

        private void ExportExecutionResultsToALM_Click(object sender, RoutedEventArgs e)
        {
            ObservableList<BusinessFlow> bfs = new ObservableList<BusinessFlow>();
            bfs.Add(App.BusinessFlow);
            ExportResultsToALMConfigPage.Instance.Init(bfs, new GingerCore.ValueExpression(App.AutomateTabEnvironment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>(), false, "", false, App.UserProfile.Solution.Variables));
            ExportResultsToALMConfigPage.Instance.ShowAsWindow();
        }

        private async void btnSaveSolutions_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveAllBtn.IsEnabled = false;
                throw new NotImplementedException();
                // App.LocalRepository.SaveAllSolutionDirtyItems(true);
            }
            finally
            {
                SaveAllBtn.IsEnabled = true;
            }
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

        private void UndoBizFlowChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.BusinessFlow != null && Reporter.ToUser(eUserMsgKeys.AskIfSureWantToUndoChange) == MessageBoxResult.Yes)
            {
                Reporter.ToGingerHelper(eGingerHelperMsgKey.UndoChanges, null, App.BusinessFlow.Name);
                App.BusinessFlow.RestoreFromBackup();
                App.BusinessFlow.SaveBackup();
                Reporter.CloseGingerHelper();
            }
        }

        private void SummeryReportButton_Click(object sender, RoutedEventArgs e)
        {
            ExecutionSummaryPage w = new ExecutionSummaryPage(App.BusinessFlow);
            w.ShowAsWindow();
        }

        private void QCManagerReport_Click(object sender, RoutedEventArgs e)
        {
            QCManagerReportPage QCMRP = new QCManagerReportPage();
            QCMRP.ShowAsWindow();
        }

        private void ExportBizFlowButton_Click(object sender, RoutedEventArgs e)
        {
            if (ALMIntegration.Instance.ExportBusinessFlowToALM(App.BusinessFlow))
            {
                if (Reporter.ToUser(eUserMsgKeys.AskIfToSaveBFAfterExport, App.BusinessFlow.Name) == MessageBoxResult.Yes)
                {
                    Reporter.ToGingerHelper(eGingerHelperMsgKey.SaveItem, null, App.BusinessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));                    
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(App.BusinessFlow);
                    Reporter.CloseGingerHelper();
                }
            }
        }

        private void DisableGridSelectedItemChangeOnClick(ucGrid UCGrid)
        {
            UCGrid.DisableUserItemSelectionChange = true;
        }

        private void EnabelGridSelectedItemChangeOnClick(ucGrid UCGrid)
        {
            UCGrid.DisableUserItemSelectionChange = false;
        }

        private void ResolveSourceControlConflicts(eResolveConflictsSide side)
        {
            AutoLogProxy.UserOperationStart("ResolveConflictsBtn_Click");

            SolutionExplorerPage solPage =
                  (SolutionExplorerPage)(from p1 in mPageList where p1.GetType() == typeof(SolutionExplorerPage) select p1).FirstOrDefault();
            if (solPage != null)
            {
                Reporter.ToGingerHelper(eGingerHelperMsgKey.ResolveSourceControlConflicts);
                SourceControlIntegration.ResolveConflicts(App.UserProfile.Solution.SourceControl,App.UserProfile.Solution.Folder, side );
                App.MainWindow.RefreshSolutionPage();
                Reporter.CloseGingerHelper();
            }

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

        public void AutomateBusinessFlow(BusinessFlow businessFlowToAutomate, bool loadAutomateTab = true)
        {
            App.MainWindow.CheckIfUserWantToSaveCurrentBF(businessFlowToAutomate);
            App.BusinessFlow = businessFlowToAutomate;
            App.BusinessFlow.SaveBackup();
            if (loadAutomateTab)
                App.MainWindow.MainRibbonSelectedTab = eRibbonTab.Automate.ToString();
        }

        public void CheckIfUserWantToSaveCurrentBF(BusinessFlow newBF)
        {
            if (newBF != App.BusinessFlow)
            {
                //check if Automate tab was used before
                bool automateTabExist = false;
                foreach(Page page in mPageList)
                    if (page.GetType() == typeof(AutomatePage)) automateTabExist=true;
                if (!automateTabExist) return;

                if ((newBF != null) && App.BusinessFlow != null && Reporter.ToUser(eUserMsgKeys.IFSaveChangesOfBF, App.BusinessFlow.Name) == MessageBoxResult.Yes)
                {
                    Reporter.ToGingerHelper(eGingerHelperMsgKey.SaveItem, null, App.BusinessFlow.Name,
                                  GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(App.BusinessFlow);
                    Reporter.CloseGingerHelper();
                }
            }
        }

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

        private void GenerateScenarioButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.BusinessFlow != null)
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                ScenariosGenerator SG = new ScenariosGenerator();
                SG.CreateScenarios(App.BusinessFlow);
                int cnt = App.BusinessFlow.ActivitiesGroups.Count;
                //MessageBox.Show("Cretaed " + cnt + " Scenarios");
                int optCount = App.BusinessFlow.ActivitiesGroups.Where(z => z.Name.StartsWith("Optimized Activities")).Count();
                if (optCount > 0)
                    cnt = cnt - optCount;
                Reporter.ToUser(eUserMsgKeys.GherkinScenariosGenerated, cnt);
                Mouse.OverrideCursor = null;
            }
        }

        private void CleanScenarioButton_Click(object sender, RoutedEventArgs e)
        {
            ScenariosGenerator SG = new ScenariosGenerator();
            SG.ClearOptimizedScenariosVariables(App.BusinessFlow);
            SG.ClearGeneretedActivites(App.BusinessFlow);
        }

        private void OpenFeatureFileButton_Click(object sender, RoutedEventArgs e)
        {
            DocumentEditorPage documentEditorPage = new DocumentEditorPage(App.AutomateTabGingerRunner.CurrentBusinessFlow.ExternalID.Replace("~", App.UserProfile.Solution.Folder), true);
            documentEditorPage.Title = "Gherkin Page";
            documentEditorPage.Height = 700;
            documentEditorPage.Width = 1000;
            documentEditorPage.ShowAsWindow();
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
                Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Ginger log file folder was not found in the path: '" + folder +"'");
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
            if (imported)
                RefreshSolutionPage();

            Page SolutionPage = App.PageList.Where(x => x.Title == "Solution Explorer").FirstOrDefault();
            //((SolutionExplorerPage)SolutionPage).LoadSoultionTree2();
            object o = ((SolutionExplorerPage)SolutionPage).SolutionTreeView.Tree.GetItemAt(0);
            ((TreeViewItem)((TreeViewItem)o).Items[0]).IsSelected = true;
            foreach (TreeViewItem item in ((ItemCollection)((TreeViewItem)o).Items))
            {
                if (item.Tag is BusinessFlowsFolderTreeItem)
                {
                    ((Ginger.SolutionWindows.TreeViewItems.BusinessFlowsFolderTreeItem)item.Tag).mTreeView.Tree.ExpandTreeNodeByName("Business Flows", Refresh: true, ExpandAll: false);
                    ((Ginger.SolutionWindows.TreeViewItems.BusinessFlowsFolderTreeItem)item.Tag).mTreeView.Tree.ExpandTreeNodeByName("Documents", Refresh: true, ExpandAll: true);
                    ((Ginger.SolutionWindows.TreeViewItems.BusinessFlowsFolderTreeItem)item.Tag).mTreeView.Tree.ExpandTreeNodeByName("Features", Refresh: true, ExpandAll: true);
                }
            }

        }

        private void CreateFeatureFile_Click(object sender, RoutedEventArgs e)
        {


            BDDIntegration BDDI = new BDDIntegration();
            BDDI.CreateFeatureFile();
            RefreshSolutionPage();

            Page SolutionPage = App.PageList.Where(x => x.Title == "Solution Explorer").FirstOrDefault();
            //((SolutionExplorerPage)SolutionPage).LoadSoultionTree2();
            object o = ((SolutionExplorerPage)SolutionPage).SolutionTreeView.Tree.GetItemAt(0);
            ((TreeViewItem)((TreeViewItem)o).Items[0]).IsSelected = true;
            foreach (TreeViewItem item in ((ItemCollection)((TreeViewItem)o).Items))
            {
                if (item.Tag is BusinessFlowsFolderTreeItem)
                {
                    ((Ginger.SolutionWindows.TreeViewItems.BusinessFlowsFolderTreeItem)item.Tag).mTreeView.Tree.ExpandTreeNodeByName("Documents", Refresh: true, ExpandAll: true);
                    ((Ginger.SolutionWindows.TreeViewItems.BusinessFlowsFolderTreeItem)item.Tag).mTreeView.Tree.ExpandTreeNodeByName("Features", Refresh: true, ExpandAll: true);
                }
            }
        }


        private void btnActionConversion_Click(object sender, RoutedEventArgs e)
        {
            AutoLogProxy.UserOperationStart("btnConversionMechanism_Click");
            ActionConverterPage gtb = new ActionConverterPage(App.BusinessFlow);
            // combine in the abover constructor
            gtb.Init(App.UserProfile.Solution, App.BusinessFlow);
            gtb.ShowAsWindow();
            AutoLogProxy.UserOperationEnd();
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

        private void btnResetFlow_Click(object sender, RoutedEventArgs e)
        {
            ResetStatusRunFRomAutomateTab(Run.GingerRunner.eResetStatus.All);
        }

        private void btnResetFromCurrentActivity_Click(object sender, RoutedEventArgs e)
        {
            ResetStatusRunFRomAutomateTab(Run.GingerRunner.eResetStatus.FromSpecificActivityOnwards);
        }

        private void btnResetFromCurrentAction_Click(object sender, RoutedEventArgs e)
        {
            ResetStatusRunFRomAutomateTab(Run.GingerRunner.eResetStatus.FromSpecificActionOnwards);
        }
        private async void ResetStatusRunFRomAutomateTab(Run.GingerRunner.eResetStatus resetFrom)
        {
            try
            {
                AutoLogProxy.UserOperationStart("ResetStatusFrom" + resetFrom.ToString() + "_Click", App.UserProfile.Solution.Name, App.GetProjEnvironmentName());
                App.AutomateTabGingerRunner.ExecutionLogger.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.Reset;
                switch (resetFrom)
                {
                    case Run.GingerRunner.eResetStatus.All:
                        App.AutomateTabGingerRunner.ResetStatus(Run.GingerRunner.eContinueLevel.StandalonBusinessFlow, Run.GingerRunner.eResetStatus.All, App.BusinessFlow);
                        break;
                    case Run.GingerRunner.eResetStatus.FromSpecificActivityOnwards:
                        App.AutomateTabGingerRunner.ResetStatus(Run.GingerRunner.eContinueLevel.StandalonBusinessFlow, Run.GingerRunner.eResetStatus.FromSpecificActivityOnwards, App.BusinessFlow, App.BusinessFlow.CurrentActivity);
                        break;
                    case Run.GingerRunner.eResetStatus.FromSpecificActionOnwards:
                        App.AutomateTabGingerRunner.ResetStatus(Run.GingerRunner.eContinueLevel.StandalonBusinessFlow, Run.GingerRunner.eResetStatus.FromSpecificActionOnwards, App.BusinessFlow, App.BusinessFlow.CurrentActivity, (Act)App.BusinessFlow.CurrentActivity.Acts.CurrentItem);
                        break;
                }

                AutoLogProxy.UserOperationEnd();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //EnableDisableAutomateTabGrids(true);
            }
        }

        FindAndReplacePage mfindAndReplacePageSolution = null;
        FindAndReplacePage mfindAndReplacePageAutomate = null;

        private void FindAndReplaceSolutionPageButton_Click(object sender, RoutedEventArgs e)
        {
            FindAndReplace();
        }

        private void FindAndReplaceAutomatePage_Click(object sender, RoutedEventArgs e)
        {
            FindAndReplace();
        }

        private void FindAndReplace()
        {
            if (MainRibbon.SelectedItem == SolutionRibbon)
            {
                if (mfindAndReplacePageSolution == null)
                {
                    mfindAndReplacePageSolution = new FindAndReplacePage(FindAndReplacePage.eContext.SolutionPage);

                }
                mfindAndReplacePageSolution.ShowAsWindow();
            }

            else if (MainRibbon.SelectedItem == AutomateRibbon)
            {
                if (mfindAndReplacePageAutomate == null)
                {
                    mfindAndReplacePageAutomate = new FindAndReplacePage(FindAndReplacePage.eContext.AutomatePage);

                }
                mfindAndReplacePageAutomate.ShowAsWindow();
            }

            else if (MainRibbon.SelectedItem == RunRibbon)
            {
                NewRunSetPage runSetPage = (NewRunSetPage)(from p1 in mPageList where p1.GetType() == typeof(NewRunSetPage) select p1).SingleOrDefault();
                runSetPage.ShowFindAndReplacePage();
            }

        }
    }
}