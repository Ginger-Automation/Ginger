#region License
/*
Copyright © 2014-2019 European Support Limited

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
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.CoreNET.LiteDBFolder;
using Amdocs.Ginger.CoreNET.Logger;
using Amdocs.Ginger.Run;
using Amdocs.Ginger.UserControls;
using Ginger;
using Ginger.Actions.ActionConversion;
using Ginger.Agents;
using Ginger.ALM;
using Ginger.AnalyzerLib;
using Ginger.BusinessFlowPages;
using Ginger.BusinessFlowsLibNew.AddActionMenu;
using Ginger.BusinessFlowWindows;
using Ginger.Extensions;
using Ginger.Functionalities;
using Ginger.GherkinLib;
using Ginger.Reports;
using Ginger.Run;
using Ginger.TimeLineLib;
using Ginger.UserControlsLib.TextEditor;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.PlugIns;
using GingerCore.DataSource;
using GingerCore.Drivers;
using GingerCore.Environments;
using GingerCore.GeneralLib;
using GingerCore.Platforms;
using GingerCoreNET;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using LiteDB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace GingerWPF.BusinessFlowsLib
{
    /// <summary>
    /// Page used to design the Business Flow automation flow
    /// </summary>
    public partial class NewAutomatePage : Page, INotifyPropertyChanged
    {
        GingerRunner mRunner;
        ProjEnvironment mEnvironment = null;
        BusinessFlow mBusinessFlow;
        Activity mActivity = null;
        Context mContext = new Context();

        ApplicationAgentsMapPage mApplicationAgentsMapPage;
        ActivitiesListViewPage mActivitiesPage;
        VariabelsListViewPage mVariabelsPage;
        BusinessFlowConfigurationsPage mConfigurationsPage;
        ActivityPage mActivityPage;
        MainAddActionsNavigationPage mAddActionMainPage;

        bool mExecutionIsInProgress = false;
        bool mSyncSelectedItemWithExecution = true;

        GridLength mLastAddActionsColumnWidth = new GridLength(400);

        ObjectId mRunnerLiteDbId;
        ObjectId mRunSetLiteDbId;

        bool mAutoRunAnalyzer = true;
        public bool AutoRunAnalyzer
        {
            get
            {
                return mAutoRunAnalyzer;
            }
            set
            {
                if (mAutoRunAnalyzer != value)
                {
                    mAutoRunAnalyzer = value;
                    OnPropertyChanged(nameof(AutoRunAnalyzer));
                }
            }
        }

        bool mAutoGenerateReport = false;
        public bool AutoGenerateReport
        {
            get
            {
                return mAutoGenerateReport;
            }
            set
            {
                if (mAutoGenerateReport != value)
                {
                    mAutoGenerateReport = value;
                    OnPropertyChanged(nameof(AutoGenerateReport));
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public NewAutomatePage(BusinessFlow businessFlow)
        {
            InitializeComponent();

            App.AutomateBusinessFlowEvent -= App_AutomateBusinessFlowEvent;
            App.AutomateBusinessFlowEvent += App_AutomateBusinessFlowEvent;
            WorkSpace.Instance.PropertyChanged -= WorkSpacePropertyChanged;
            WorkSpace.Instance.PropertyChanged += WorkSpacePropertyChanged;

            InitAutomatePageRunner();
            UpdateAutomatePageRunner();

            LoadBusinessFlowToAutomate(businessFlow);

            SetRunSetDBLite(businessFlow);

            SetUIControls();
        }

        #region LiteDB
        private void SetRunSetDBLite(BusinessFlow businessFlowToLoad)
        {
            if (mRunner.ExecutionLoggerManager.Configuration.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
            {
                bool isAutoRunSetExists = AutoRunSetDocumentExistsInLiteDB();
                if (ExecutionLoggerManager.RunSetReport == null)
                {
                    ExecutionLoggerManager.RunSetReport = new RunSetReport();
                    ExecutionLoggerManager.RunSetReport.SetDataForAutomateTab();
                    if (!isAutoRunSetExists)
                    {
                        mRunner.ExecutionLoggerManager.BusinessFlowEnd(0, businessFlowToLoad, true);
                        mRunner.ExecutionLoggerManager.RunnerRunEnd(0, mRunner, offlineMode:true);
                        mRunner.ExecutionLoggerManager.mExecutionLogger.SetReportRunSet(ExecutionLoggerManager.RunSetReport, "");
                        isAutoRunSetExists = AutoRunSetDocumentExistsInLiteDB();
                        return;
                    }
                    DeleteRunSetBFRefData();
                }
            }
        }

        private void DeleteRunSetBFRefData()
        {
            LiteDbManager dbManager = new LiteDbManager(mRunner.ExecutionLoggerManager.Configuration.CalculatedLoggerFolder);
            var result = dbManager.GetRunSetLiteData();
            List<LiteDbRunSet> filterData = null;
            filterData = result.IncludeAll().Find(a => a.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Automated.ToString()).ToList();
            if (filterData != null && filterData.Count > 0)
            {
                LiteDbConnector dbConnector = new LiteDbConnector(Path.Combine(mRunner.ExecutionLoggerManager.Configuration.CalculatedLoggerFolder, "GingerExecutionResults.db"));
                dbConnector.DeleteDocumentByLiteDbRunSet(filterData[0], eExecutedFrom.Automation);
            }
        }

        private void ClearAutomatedId(BusinessFlow businessFlowToLoad)
        {
            foreach (var activity in businessFlowToLoad.Activities)
            {
                foreach (var action in activity.Acts)
                {
                    (action as Act).LiteDbId = null;
                }
                activity.LiteDbId = null;
            }
            foreach (var ag in businessFlowToLoad.ActivitiesGroups)
            {
                ag.LiteDbId = null;
            }
            businessFlowToLoad.LiteDbId = null;
        }

        private bool AutoRunSetDocumentExistsInLiteDB()
        {
            bool isExist = false;
            LiteDbManager dbManager = new LiteDbManager(mRunner.ExecutionLoggerManager.Configuration.CalculatedLoggerFolder);
            var result = dbManager.GetRunSetLiteData();
            List<LiteDbRunSet> filterData = null;
            filterData = result.IncludeAll().Find(a => a.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Automated.ToString()).ToList();
            isExist = (filterData == null) ? false : filterData.Count > 0;
            if (isExist)
            {
                mRunSetLiteDbId = filterData[0]._id;
                mRunnerLiteDbId = filterData[0].RunnersColl[0]._id;
            }
            return isExist;
        }
        #endregion LiteDB

        private void SetUIControls()
        {
            xBusinessFlowItemComboBox.Items.Add(GingerDicser.GetTermResValue(eTermResKey.Activities));
            xBusinessFlowItemComboBox.Items.Add(GingerDicser.GetTermResValue(eTermResKey.Variables));
            xBusinessFlowItemComboBox.Items.Add("Configurations");
            xBusinessFlowItemComboBox.SelectedIndex = 0;

            BindingHandler.ObjFieldBinding(xAutoAnalyzeConfigMenuItemIcon, ImageMakerControl.ImageTypeProperty, this, nameof(AutoRunAnalyzer), bindingConvertor: new ActiveImageTypeConverter(), BindingMode.OneWay);
            BindingHandler.ObjFieldBinding(xAutoReportConfigMenuItemIcon, ImageMakerControl.ImageTypeProperty, this, nameof(AutoGenerateReport), bindingConvertor: new ActiveImageTypeConverter(), BindingMode.OneWay);

            mApplicationAgentsMapPage = new ApplicationAgentsMapPage(mRunner, mContext);
            xAppsAgentsMappingFrame.SetContent(mApplicationAgentsMapPage);
            SetEnvsCombo();
        }


        /// <summary>
        /// This event is used to handle the Activity's TargetApplciation changed functionality
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Activity_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Activity.TargetApplication))
            {
                UpdateContextWithActivityDependencies();
            }
        }

        /// <summary>
        /// This event is used to handle the ApplicationAgents Agent changed functionality
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppAgent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Agent))
            {
                UpdateContextWithActivityDependencies();
            }
        }

        /// <summary>
        /// This event is used to handle the Agent's Status changed functionality
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Agent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Agent.Status))
            {
                mContext.AgentStatus = Convert.ToString(((Agent)sender).Status);
            }
        }

        /// <summary>
        /// This method will set the Agent, Target and Platform for current activity in Context
        /// </summary>
        private void UpdateContextWithActivityDependencies()
        {
            ApplicationAgent appAgent = AgentHelper.GetAppAgent(mContext.Activity, mContext.Runner, mContext);
            if (appAgent != null)
            {
                appAgent.PropertyChanged -= AppAgent_PropertyChanged;
                appAgent.PropertyChanged += AppAgent_PropertyChanged;

                UpdateTargetAndPlatform();

                if (mContext.Agent != appAgent.Agent)
                {
                    mContext.Agent = appAgent.Agent;
                }

                if (mContext.Agent != null)
                {
                    mContext.Agent.PropertyChanged -= Agent_PropertyChanged;
                    mContext.Agent.PropertyChanged += Agent_PropertyChanged;
                }
            }
            else
            {
                mContext.Agent = null;
            }
        }

        private void UpdateTargetAndPlatform()
        {
            TargetBase tBase = (from x in mContext.BusinessFlow.TargetApplications where x.ItemName == mContext.Activity.TargetApplication select x).FirstOrDefault();
            if (tBase != null)
            {
                if (mContext.Target == null || mContext.Target.ItemName != tBase.ItemName)
                {
                    mContext.Target = tBase;
                    mContext.Platform = (from x in WorkSpace.Instance.Solution.ApplicationPlatforms
                                         where x.AppName == mContext.Activity.TargetApplication
                                         select x.Platform).FirstOrDefault();
                }
            }
            else
            {
                mContext.Target = null;
                mContext.Platform = GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ePlatformType.NA;
            }
        }

        private void XAddActionsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress()) return;

            if (xAddActionsBtn.ButtonImageType == Amdocs.Ginger.Common.Enums.eImageType.Add)
            {
                ExpandAddActionsPnl();
            }
            else
            {
                CollapseAddActionsPnl();
            }
        }

        private void ExpandAddActionsPnl()
        {
            //Expand
            xAddActionsColumn.Width = mLastAddActionsColumnWidth;
            xAddActionsBtn.ButtonImageType = Amdocs.Ginger.Common.Enums.eImageType.ArrowRight;
            xAddActionsBtn.ToolTip = "Collapse Add Actions Section";
            xAddActionsBtn.ButtonStyle = (Style)FindResource("$AddActionsMenuBtnStyle");
            xAddActionSectionSpliter.IsEnabled = true;
            MainAddActionsNavigationPage.IsPanelExpanded = true;
            mAddActionMainPage.ReloadPagesOnExpand();
        }

        private void CollapseAddActionsPnl()
        {
            //Collapse
            mLastAddActionsColumnWidth = xAddActionsColumn.Width;
            xAddActionsColumn.Width = new GridLength(5);
            xAddActionsBtn.ButtonImageType = Amdocs.Ginger.Common.Enums.eImageType.Add;
            xAddActionsBtn.ToolTip = "Add Actions";
            xAddActionsBtn.ButtonStyle = (Style)FindResource("$AddActionsMenuBtnStyle");
            xAddActionSectionSpliter.IsEnabled = false;
            MainAddActionsNavigationPage.IsPanelExpanded = false;
        }

        private void InitAutomatePageRunner()
        {
            mRunner = new GingerRunner(eExecutedFrom.Automation);
            mRunner.PropertyChanged += MRunner_PropertyChanged;

            // Add Listener so we can do GiveUserFeedback            
            AutomatePageRunnerListener automatePageRunnerListener = new AutomatePageRunnerListener();
            automatePageRunnerListener.AutomatePageRunnerListenerGiveUserFeedback = GiveUserFeedback;
            mRunner.RunListeners.Add(automatePageRunnerListener);

            mRunner.Context = mContext;
            mContext.Runner = mRunner;
        }

        private void MRunner_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GingerRunner.SpecificEnvironmentName))
            {
                if (!string.IsNullOrEmpty(mRunner.SpecificEnvironmentName))
                {
                    xEnvironmentComboBox.SelectedItem = (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().Where(x => x.Name == mRunner.SpecificEnvironmentName).First());
                }
            }
            //else if (e.PropertyName == nameof(GingerRunner.Status))
            //{
            //    if (mRunner.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Started
            //        || mRunner.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running
            //            || mRunner.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Canceling)
            //    {
            //        mExecutionIsInProgress = true;
            //    }
            //    else
            //    {
            //        mExecutionIsInProgress = false;
            //    }
            //    SetUIElementsBehaverDuringExecution();
            //}
            else if (e.PropertyName == nameof(GingerRunner.IsRunning))
            {
                mExecutionIsInProgress = mRunner.IsRunning;
                SetUIElementsBehaverDuringExecution();
            }
        }

        private void LoadBusinessFlowToAutomate(BusinessFlow businessFlowToLoad)
        {
            if (mExecutionIsInProgress)
            {
                StopAutomateRun();
            }

            if (mBusinessFlow != businessFlowToLoad)
            {
                RemoveCurrentBusinessFlow();
                ResetPageUI();

                mBusinessFlow = businessFlowToLoad;
                mBusinessFlow.SaveBackup();
                mContext.BusinessFlow = mBusinessFlow;

                mRunner.BusinessFlows.Add(mBusinessFlow);
                mRunner.CurrentBusinessFlow = mBusinessFlow;
                UpdateApplicationsAgentsMapping();

                if (AutoRunSetDocumentExistsInLiteDB() && ExecutionLoggerManager.RunSetReport != null && mRunner.ExecutionLoggerManager.Configuration.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
                {
                    DeleteRunSetBFRefData();
                    ClearAutomatedId(businessFlowToLoad);
                    mRunner.ExecutionLoggerManager.mExecutionLogger.RunSetUpdate(mRunSetLiteDbId, mRunnerLiteDbId, mRunner);
                }

                if (mBusinessFlow != null)
                {
                    mBusinessFlow.SaveBackup();
                    mBusinessFlow.PropertyChanged += mBusinessFlow_PropertyChanged;

                    BindingHandler.ObjFieldBinding(xBusinessFlowNameTxtBlock, TextBlock.TextProperty, mBusinessFlow, nameof(BusinessFlow.Name));
                    xBusinessFlowNameTxtBlock.ToolTip = mBusinessFlow.ContainingFolder + "\\" + mBusinessFlow.Name;

                    if (mBusinessFlow.Source == BusinessFlow.eSource.Gherkin)
                    {
                        xBDDOperationsMenu.Visibility = Visibility.Visible;
                    }
                    
                    if (mActivitiesPage == null)
                    {
                        mActivitiesPage = new ActivitiesListViewPage(mBusinessFlow, mContext, Ginger.General.eRIPageViewMode.Automation);
                        mActivitiesPage.ListView.List.SelectionChanged -= ActivitiesList_SelectionChanged;
                        mActivitiesPage.ListView.List.SelectionChanged += ActivitiesList_SelectionChanged;
                        xActivitiesListFrame.Content = mActivitiesPage;
                    }
                    else
                    {
                        mActivitiesPage.UpdateBusinessFlow(mBusinessFlow);
                    }

                    if (mVariabelsPage == null)
                    {
                        mVariabelsPage = new VariabelsListViewPage(mBusinessFlow, mContext, Ginger.General.eRIPageViewMode.Automation);
                        xBfVariablesTabFrame.Content = mVariabelsPage;
                    }
                    else
                    {
                        mVariabelsPage.UpdateParent(mBusinessFlow);
                    }

                    if (mConfigurationsPage == null)
                    {
                        mConfigurationsPage = new BusinessFlowConfigurationsPage(mBusinessFlow, mContext, Ginger.General.eRIPageViewMode.Automation);
                        xBfConfigurationsTabFrame.Content = mConfigurationsPage;
                    }
                    else
                    {
                        mConfigurationsPage.UpdateBusinessFlow(mBusinessFlow);
                    }

                    if (mBusinessFlow.Activities.Count > 0)
                    {
                        mActivity = mBusinessFlow.Activities[0];
                        mBusinessFlow.CurrentActivity = mActivity;
                        mContext.Activity = mActivity;

                        if (mContext.Platform == ePlatformType.NA)
                            UpdateContextWithActivityDependencies();
                    }
                    SetActivityEditPage();

                    SetBusinessFlowTargetAppIfNeeded();
                    mBusinessFlow.TargetApplications.CollectionChanged += mBusinessFlowTargetApplications_CollectionChanged;

                    UpdateRunnerAgentsUsedBusinessFlow();

                    if (mAddActionMainPage == null)
                    {
                        mAddActionMainPage = new MainAddActionsNavigationPage(mContext);
                    }
                    xAddActionMenuFrame.Content = mAddActionMainPage;
                }
            }
        }

        private void ResetPageUI()
        {
            xBDDOperationsMenu.Visibility = Visibility.Collapsed;
            xContinueRunBtn.Visibility = Visibility.Collapsed;
        }

        private void ActivitiesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mActivity != null)
            {
                mActivity.PropertyChanged -= Activity_PropertyChanged;
            }
            mActivity = (Activity)mActivitiesPage.ListView.CurrentItem;

            if (mBusinessFlow.Activities.SyncCurrentItemWithViewSelectedItem)
            {
                mBusinessFlow.CurrentActivity = mActivity;
            }

            mContext.Activity = mActivity;
            if (mActivity != null)
            {
                mActivity.PropertyChanged -= Activity_PropertyChanged;
                mActivity.PropertyChanged += Activity_PropertyChanged;
            }

            UpdateContextWithActivityDependencies();

            SetActivityEditPage();
        }

        public void UpdateRunnerAgentsUsedBusinessFlow()
        {
            foreach (ApplicationAgent appAgent in mRunner.ApplicationAgents)
            {
                if (appAgent.Agent != null && appAgent.Agent.Status == Agent.eStatus.Running)
                {
                    ((DriverBase)appAgent.Agent.Driver).UpdateContext(mContext);
                }
            }
        }

        private void SetActivityEditPage()
        {
            try
            {
                xCurrentActivityLoadingIconPnl.Visibility = Visibility.Visible;
                xCurrentActivityFrame.Visibility = Visibility.Collapsed;
                Ginger.General.DoEvents();

                if (mContext.Activity != null)
                {
                    if (mActivityPage == null)
                    {
                        mActivityPage = new ActivityPage(mContext.Activity, mContext, Ginger.General.eRIPageViewMode.Automation);
                    }
                    else
                    {
                        mActivityPage.UpdateActivity(mContext.Activity);
                        //GC.Collect();
                    }
                }
                else
                {
                    mActivityPage = null;
                }
            }
            finally
            {
                xCurrentActivityLoadingIconPnl.Visibility = Visibility.Collapsed;
                xCurrentActivityFrame.Visibility = Visibility.Visible;
                xCurrentActivityFrame.SetContent(mActivityPage);
            }
        }

        private void mBusinessFlow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if (e.PropertyName == nameof(BusinessFlow.CurrentActivity))
            //{
            //}
        }

        private void SetBusinessFlowTargetAppIfNeeded()
        {
            if (WorkSpace.Instance.Solution != null && mBusinessFlow != null)
            {
                //First we check if biz flow have target apps if not add one based on solution, fast convert for old or deleted
                if (mBusinessFlow.TargetApplications.Count() == 0)
                {
                    if (string.IsNullOrEmpty(WorkSpace.Instance.Solution.MainApplication))
                    {
                        Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "You must have at least one Target Application configured, please set it up.");
                        return;
                    }
                    else
                    {
                        // take it from solution main platform
                        if (mBusinessFlow.TargetApplications == null)
                        {
                            mBusinessFlow.TargetApplications = new ObservableList<TargetBase>();
                        }

                        mBusinessFlow.TargetApplications.Add(new TargetApplication() { AppName = WorkSpace.Instance.Solution.MainApplication });
                    }
                }
            }
        }

        private void RemoveCurrentBusinessFlow()
        {
            if (mBusinessFlow != null)
            {
                mBusinessFlow.PropertyChanged -= mBusinessFlow_PropertyChanged;
                mBusinessFlow.TargetApplications.CollectionChanged -= mBusinessFlowTargetApplications_CollectionChanged;
                mBusinessFlow = null;
                mRunner.BusinessFlows.Clear();
            }
        }

        private void mBusinessFlowTargetApplications_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateApplicationsAgentsMapping();
        }

        public void UpdateApplicationsAgentsMapping(bool useAgentsCache = true)
        {
            if (WorkSpace.Instance.Solution != null)
            {
                mRunner.SolutionAgents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>();
            }
            else
            {
                mRunner.SolutionAgents = null;
            }
            mRunner.UpdateApplicationAgents();
        }

        private void SetUIElementsBehaverDuringExecution()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (mExecutionIsInProgress)
                {
                    xRunFlowBtn.ButtonImageType = eImageType.Running;
                    xRunFlowBtn.ButtonText = "Running";
                    xRunFlowBtn.ToolTip = "Execution is in progress";
                    xRunFlowBtn.IsEnabled = false;
                    xStopRunBtn.Visibility = Visibility.Visible;
                    xRunFlowBtn.ButtonStyle = (Style)FindResource("$RoundTextAndImageButtonStyle_ExecutionRunning");
                    xRunFlowBtn.ButtonImageForground = (SolidColorBrush)FindResource("$SelectionColor_LightBlue");
                    xEnvironmentComboBox.IsEnabled = false;
                    if (mApplicationAgentsMapPage != null)
                    {
                        mApplicationAgentsMapPage.MappingList.IsEnabled = false;
                    }

                    if (xAddActionsBtn.ButtonImageType != Amdocs.Ginger.Common.Enums.eImageType.Add)
                    {
                        CollapseAddActionsPnl();
                    }
                    xAddActionsBtn.Visibility = Visibility.Collapsed;
                    xAddActionSectionSpliter.Visibility = Visibility.Collapsed;

                    if (mBusinessFlow != null)
                    {
                        mBusinessFlow.Activities.SyncCurrentItemWithViewSelectedItem = false;
                        mBusinessFlow.Activities.SyncViewSelectedItemWithCurrentItem = mSyncSelectedItemWithExecution;
                        foreach (Activity activity in mBusinessFlow.Activities)
                        {
                            activity.Acts.SyncCurrentItemWithViewSelectedItem = false;
                            activity.Acts.SyncViewSelectedItemWithCurrentItem = mSyncSelectedItemWithExecution;
                        }
                    }
                }
                else
                {
                    xRunFlowBtn.ButtonImageType = eImageType.Run;
                    xRunFlowBtn.ButtonText = "Run Flow";
                    xRunFlowBtn.ToolTip = "Reset & Run Flow";
                    xRunFlowBtn.IsEnabled = true;
                    xRunFlowBtn.ButtonStyle = (Style)FindResource("$RoundTextAndImageButtonStyle_Execution");
                    xRunFlowBtn.ButtonImageForground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                    xStopRunBtn.Visibility = Visibility.Collapsed;

                    xEnvironmentComboBox.IsEnabled = true;
                    if (mApplicationAgentsMapPage != null)
                    {
                        mApplicationAgentsMapPage.MappingList.IsEnabled = true;
                    }

                    xAddActionsBtn.Visibility = Visibility.Visible;
                    xAddActionSectionSpliter.Visibility = Visibility.Visible;

                    if (mBusinessFlow != null)
                    {
                        mBusinessFlow.Activities.SyncCurrentItemWithViewSelectedItem = true;
                        mBusinessFlow.Activities.SyncViewSelectedItemWithCurrentItem = true;
                        foreach (Activity activity in mBusinessFlow.Activities)
                        {
                            activity.Acts.SyncCurrentItemWithViewSelectedItem = true;
                            activity.Acts.SyncViewSelectedItemWithCurrentItem = true;
                        }
                    }
                }                
            });
        }

        private bool CheckIfExecutionIsInProgress()
        {
            if (mExecutionIsInProgress)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Operation can't be done during execution.");
                return true;
            }

            return false;
        }

        public void StopAutomateRun()
        {
            try
            {
                mRunner.StopRun();
                this.Dispatcher.Invoke(() =>
                {
                    xContinueRunBtn.Visibility = Visibility.Visible;
                });

            }
            finally
            {
                //mExecutionIsInProgress = false;
                //SetUIElementsBehaverDuringExecution();
            }
        }



        private void App_AutomateBusinessFlowEvent(AutomateEventArgs args)
        {
            switch (args.EventType)
            {
                case AutomateEventArgs.eEventType.Automate:
                    LoadBusinessFlowToAutomate((BusinessFlow)args.Object);
                    break;
                case AutomateEventArgs.eEventType.ClearAutomate:
                    StopAutomateRun();
                    RemoveCurrentBusinessFlow();
                    break;
                case AutomateEventArgs.eEventType.UpdateAppAgentsMapping:
                    UpdateApplicationsAgentsMapping();
                    break;
                case AutomateEventArgs.eEventType.SetupRunnerForExecution:
                    UpdateAutomatePageRunner();
                    break;
                case AutomateEventArgs.eEventType.RunCurrentAction:
                    RunAutomatePageAction((Tuple<Activity,Act>)args.Object,  false);
                    break;
                case AutomateEventArgs.eEventType.RunCurrentActivity:
                    RunAutomatePageActivity((Activity)args.Object);
                    break;
                case AutomateEventArgs.eEventType.ContinueActionRun:
                    ContinueRunFromAutomatePage(eContinueFrom.SpecificAction, args.Object);
                    break;
                case AutomateEventArgs.eEventType.ContinueActivityRun:
                    ContinueRunFromAutomatePage(eContinueFrom.SpecificActivity, args.Object);
                    break;
                case AutomateEventArgs.eEventType.StopRun:
                    StopAutomateRun();
                    break;
                case AutomateEventArgs.eEventType.GenerateLastExecutedItemReport:
                    GenerateLastExecutedItemReport();
                    break;
                default:
                    //Avoid other operations
                    break;
            }
        }

        private void UpdateAutomatePageRunner()
        {
            mRunner.CurrentSolution = WorkSpace.Instance.Solution;
            mRunner.SolutionFolder = WorkSpace.Instance.Solution.Folder;
            mRunner.SolutionAgents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>();
            mRunner.SolutionApplications = WorkSpace.Instance.Solution.ApplicationPlatforms;
            mRunner.DSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();

            mRunner.ExecutionLoggerManager.ExecutionLogfolder = string.Empty;
            mRunner.ExecutionLoggerManager.Configuration = WorkSpace.Instance.Solution.LoggerConfigurations;
        }


        private async Task RunAutomateTabFlow()
        {
            if (CheckIfExecutionIsInProgress()) return;

            try
            {
                //mExecutionIsInProgress = true;
                //SetUIElementsBehaverDuringExecution();

                if (AutoRunAnalyzer)
                {
                    //Run Analyzer check if not including any High or Critical issues before execution
                    Reporter.ToStatus(eStatusMsgKey.AnalyzerIsAnalyzing, null, mBusinessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                    try
                    {
                        AnalyzerPage analyzerPage = new AnalyzerPage();
                        analyzerPage.Init(WorkSpace.Instance.Solution, mBusinessFlow);
                        await analyzerPage.AnalyzeWithoutUI();
                        Reporter.HideStatusMessage();
                        if (analyzerPage.TotalHighAndCriticalIssues > 0)
                        {
                            Reporter.ToUser(eUserMsgKey.AnalyzerFoundIssues);
                            analyzerPage.ShowAsWindow();
                            return;
                        }
                    }
                    finally
                    {
                        Reporter.HideStatusMessage();
                    }
                }

                //execute preparations               
                mRunner.ResetRunnerExecutionDetails();
                mRunner.ExecutionLoggerManager.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.BussinessFlowRun;

                //execute                
                await mRunner.RunBusinessFlowAsync(mBusinessFlow, true, false).ConfigureAwait(false);
                if (WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
                {
                    mRunner.ExecutionLoggerManager.mExecutionLogger.RunSetUpdate(mRunSetLiteDbId, mRunnerLiteDbId, mRunner);
                }
                this.Dispatcher.Invoke(() =>
                {
                    if (AutoGenerateReport)
                    {
                        GenerateReport();
                    }
                    else
                    {
                        ShowExecutionSummaryPage();
                    }
                });

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //mExecutionIsInProgress = false;
                //SetUIElementsBehaverDuringExecution();
                mRunner.ResetFailedToStartFlagForAgents();
            }
        }

        public async Task RunAutomatePageActivity(Activity activity)
        {
            if (CheckIfExecutionIsInProgress()) return;

            try
            {
                //mExecutionIsInProgress = true;
                //SetUIElementsBehaverDuringExecution();

                mContext.BusinessFlow.CurrentActivity = activity;
                mContext.Runner.ExecutionLoggerManager.Configuration.ExecutionLoggerAutomationTabContext = Ginger.Reports.ExecutionLoggerConfiguration.AutomationTabContext.ActivityRun;

                await mRunner.RunActivityAsync((Activity)activity, false, true).ConfigureAwait(false);

                //When running Runactivity as standalone from GUI, SetActionSkipStatus is not called. Handling it here for now.
                foreach (Act act in activity.Acts)
                {
                    if (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending)
                    {
                        act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                    }
                }
                if (mRunner.ExecutionLoggerManager.Configuration.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
                {
                    mRunner.ExecutionLoggerManager.BusinessFlowEnd(0, mBusinessFlow);
                    mRunner.ExecutionLoggerManager.mExecutionLogger.RunSetUpdate(mRunSetLiteDbId, mRunnerLiteDbId, mRunner);
                }
            }
            finally
            {
                if (activity.CurrentAgent != null)
                {
                    ((Agent)activity.CurrentAgent).IsFailedToStart = false;
                }
            }
        }

        public async Task RunAutomatePageAction(Tuple<Activity,Act> actionToExecuteInfo,  bool checkIfActionAllowedToRun = true)
        {
            if (CheckIfExecutionIsInProgress()) return;

            Activity parentActivity = actionToExecuteInfo.Item1;
            Act actionToExecute = actionToExecuteInfo.Item2;
            if (parentActivity.Acts.Count() == 0)
            {
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "No Action to Run.");
                return;
            }

            // If no action selected move to the first.
            if (actionToExecute == null)
            {
                actionToExecute = (Act)parentActivity.Acts[0];
            }

            try
            {
                //mExecutionIsInProgress = true;
                //SetUIElementsBehaverDuringExecution();

                //No need of agent for actions like DB and read for excel. For other need agent  
                Type actType = mRunner.CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem.GetType();
                if (!(typeof(ActWithoutDriver).IsAssignableFrom(actType)) || actType == typeof(ActAgentManipulation))   // ActAgentManipulation not needed
                {
                    mRunner.SetCurrentActivityAgent();
                }
                else if ((typeof(ActPlugIn).IsAssignableFrom(actType)))
                {
                    mRunner.SetCurrentActivityAgent();
                }

                mBusinessFlow.CurrentActivity = parentActivity;
                mBusinessFlow.CurrentActivity.Acts.CurrentItem = actionToExecute;
                mRunner.ExecutionLoggerManager.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.ActionRun;

                var result = await mRunner.RunActionAsync(actionToExecute, checkIfActionAllowedToRun, true).ConfigureAwait(false);

               

                if (mRunner.ExecutionLoggerManager.Configuration.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
                {
                    mRunner.ExecutionLoggerManager.ActivityEnd(0, parentActivity);
                    mRunner.ExecutionLoggerManager.BusinessFlowEnd(0, mBusinessFlow);
                    mRunner.ExecutionLoggerManager.mExecutionLogger.RunSetUpdate(mRunSetLiteDbId, mRunnerLiteDbId, mRunner);
                }
            }
            finally
            {
                if (mRunner.CurrentBusinessFlow.CurrentActivity.CurrentAgent != null)
                {
                    ((Agent)mRunner.CurrentBusinessFlow.CurrentActivity.CurrentAgent).IsFailedToStart = false;
                }
            }
        }

        private async Task ContinueRunFromAutomatePage(eContinueFrom continueFrom, object executedItem = null)
        {
            if (CheckIfExecutionIsInProgress()) return;

            try
            {
                //mExecutionIsInProgress = true;
                //SetUIElementsBehaverDuringExecution();

                mRunner.ExecutionLoggerManager.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.ContinueRun;

                switch (continueFrom)
                {
                    case eContinueFrom.LastStoppedAction:
                        await mRunner.ContinueRunAsync(eContinueLevel.StandalonBusinessFlow, eContinueFrom.LastStoppedAction);
                        break;
                    case eContinueFrom.SpecificAction:
                        await mRunner.ContinueRunAsync(eContinueLevel.StandalonBusinessFlow, eContinueFrom.SpecificAction, mBusinessFlow, (Activity)((Tuple<Activity,Act>)executedItem).Item1, (Act)((Tuple<Activity, Act>)executedItem).Item2);
                        break;
                    case eContinueFrom.SpecificActivity:
                        mBusinessFlow.CurrentActivity = (Activity)executedItem;
                        await mRunner.ContinueRunAsync(eContinueLevel.StandalonBusinessFlow, eContinueFrom.SpecificActivity, mBusinessFlow, (Activity)executedItem);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //mExecutionIsInProgress = false;
                //SetUIElementsBehaverDuringExecution();
            }
        }

        private void WorkSpacePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WorkSpace.Solution))
            {
                if (WorkSpace.Instance.Solution == null)
                {
                    DoCleanUp();

                    if (mAddActionMainPage != null)
                        mAddActionMainPage.ResetAddActionPages();

                    return;
                }

                UpdateToNewSolution();
            }
        }

        private void DoCleanUp()
        {
            mRunner.ClearAgents();
        }

        private void UpdateToNewSolution()
        {
            SetEnvsCombo();
            UpdateAutomatePageRunner();
        }

        private void SetEnvsCombo()
        {
            xEnvironmentComboBox.DisplayMemberPath = nameof(ProjEnvironment.Name);
            xEnvironmentComboBox.SelectedValuePath = nameof(ProjEnvironment.Guid);
            xEnvironmentComboBox.ItemsSource = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().AsCollectionViewOrderBy(nameof(ProjEnvironment.Name));
            
            if(GingerCoreNET.GeneralLib.General.CreateDefaultEnvironment())
            {
                xEnvironmentComboBox.SelectedIndex = 0;
            }            
            else
            {
                //select last used environment
                if (xEnvironmentComboBox.Items.Count > 1 && WorkSpace.Instance.UserProfile.RecentEnvironment != null && WorkSpace.Instance.UserProfile.RecentEnvironment != Guid.Empty)
                {
                    foreach (object env in xEnvironmentComboBox.Items)
                    {
                        if (((ProjEnvironment)env).Guid == WorkSpace.Instance.UserProfile.RecentEnvironment)
                        {
                            xEnvironmentComboBox.SelectedIndex = xEnvironmentComboBox.Items.IndexOf(env);
                            return;
                        }
                    }
                }

                //default selection
                xEnvironmentComboBox.SelectedIndex = 0;
            }
        }

        private void xEnvironmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xEnvironmentComboBox != null)
            {
                UpdateUsedEnvironment((ProjEnvironment)xEnvironmentComboBox.SelectedItem);
            }
        }

        private void UpdateUsedEnvironment(ProjEnvironment env)
        {
            mEnvironment = env;
            mContext.Environment = env;
            if (env != null)
            {
                mRunner.UseSpecificEnvironment = true;
                mRunner.ProjEnvironment = env;
                mRunner.SpecificEnvironmentName = env.Name;
                WorkSpace.Instance.UserProfile.RecentEnvironment = mEnvironment.Guid;
            }
            else
            {
                mRunner.UseSpecificEnvironment = false;
                mRunner.ProjEnvironment = null;
                mRunner.SpecificEnvironmentName = string.Empty;
            }
        }

        public void GiveUserFeedback(object sender, EventArgs e)
        {
            // Run Do events on sepertate task so will not impact performance
            Task.Factory.StartNew(() =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    GingerCore.General.DoEvents();
                });
            });

        }

        private void xGoToBFsTreeBtn_Click(object sender, RoutedEventArgs e)
        {
            App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.ShowBusinessFlowsList, mBusinessFlow);
        }

        private void xSaveBusinessFlowBtn_Click(object sender, RoutedEventArgs e)
        {
            //warn in case dynamic shared repository Activities are included and going to be deleted
            if (mBusinessFlow.Activities.Where(x => x.AddDynamicly == true).FirstOrDefault() != null)
            {
                if (Reporter.ToUser(eUserMsgKey.WarnOnDynamicActivities) == Amdocs.Ginger.Common.eUserMsgSelection.No)
                {
                    return;
                }
            }

            Reporter.ToStatus(eStatusMsgKey.SaveItem, null, mBusinessFlow.Name,
                                      GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
            WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mBusinessFlow);
            Reporter.HideStatusMessage();
        }

        private void xUndoChangesBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress()) return;

            if (Ginger.General.UndoChangesInRepositoryItem(mBusinessFlow, true))
            {
                mBusinessFlow.SaveBackup();
            }
        }

        FindAndReplacePage mfindAndReplacePageAutomate = null;
        private void xSearchBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mfindAndReplacePageAutomate == null)
            {
                mfindAndReplacePageAutomate = new FindAndReplacePage(FindAndReplacePage.eContext.AutomatePage, mBusinessFlow);
            }
            mfindAndReplacePageAutomate.ShowAsWindow();
        }

        private void xAnalyzeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress()) return;

            AnalyzerPage AP = new AnalyzerPage();
            AP.Init(WorkSpace.Instance.Solution, mBusinessFlow);
            AP.ShowAsWindow();
        }

        private void xAutomationRunnerConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress()) return;

            GingerRunnerConfigurationsPage runnerConfigurationsPage = new GingerRunnerConfigurationsPage(mRunner, GingerRunnerConfigurationsPage.ePageViewMode.AutomatePage, mContext);
            runnerConfigurationsPage.ShowAsWindow();
        }

        private void xResetFlowBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress()) return;

            mRunner.ExecutionLoggerManager.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.Reset;
            mBusinessFlow.Reset();
        }

        private async void xRunFlowBtn_Click(object sender, RoutedEventArgs e)
        {
            await RunAutomateTabFlow();
        }

        private void xStopRunBtn_Click(object sender, RoutedEventArgs e)
        {
            StopAutomateRun();
        }

        private void xBusinessFlowItemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(xBusinessFlowItemComboBox.SelectedItem == GingerDicser.GetTermResValue(eTermResKey.Activities))
            {
                xItemsTabs.SelectedItem = xBfActiVitiesTab;
            }
            else if (xBusinessFlowItemComboBox.SelectedItem == GingerDicser.GetTermResValue(eTermResKey.Variables))
            {
                xItemsTabs.SelectedItem = xBfVariablesTab;
            }
            else if (xBusinessFlowItemComboBox.SelectedItem == "Configurations")
            {
                xItemsTabs.SelectedItem = xBfConfigurationsTab;
            }
            else
            {
                xItemsTabs.SelectedItem = xBfActiVitiesTab;
            }
        }

        private void xActionsConvertionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress()) return;

            ObservableList<BusinessFlow> lst = new ObservableList<BusinessFlow>() { mBusinessFlow };
            WizardWindow.ShowWizard(new ActionsConversionWizard(ActionsConversionWizard.eActionConversionType.SingleBusinessFlow, mContext, lst), 900, 700, true);
        }

        private void xRefreshFromAlmMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress()) return;

            if (string.IsNullOrEmpty(mBusinessFlow.ExternalID))
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, string.Format("{0} is not mapped to any ALM.", GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)));
                return;
            }

            if (mBusinessFlow != null && mBusinessFlow.ActivitiesGroups != null && mBusinessFlow.ActivitiesGroups.Count > 0)
            {
                ALMIntegration.Instance.RefreshAllGroupsFromALM(mBusinessFlow);
            }
        }

        private void xExportToAlmMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress()) return;

            if (ALMIntegration.Instance.ExportBusinessFlowToALM(mBusinessFlow))
            {
                if (Reporter.ToUser(eUserMsgKey.AskIfToSaveBFAfterExport, mBusinessFlow.Name) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                {
                    Reporter.ToStatus(eStatusMsgKey.SaveItem, null, mBusinessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mBusinessFlow);
                    Reporter.HideStatusMessage();
                }
            }
        }

        private void xExportResultsToAlmMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress()) return;

            if (string.IsNullOrEmpty(mBusinessFlow.ExternalID))
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, string.Format("{0} is not mapped to any ALM.", GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)));
                return;
            }

            ObservableList<BusinessFlow> bfs = new ObservableList<BusinessFlow>();
            bfs.Add(mBusinessFlow);
            ExportResultsToALMConfigPage.Instance.Init(bfs, new GingerCore.ValueExpression(mEnvironment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>(), false, "", false));
            ExportResultsToALMConfigPage.Instance.ShowAsWindow();
        }

        private void xReportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress()) return;

            GenerateReport();
        }

        private void GenerateReport()
        {
            if (mRunner.ExecutionLoggerManager.Configuration.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
            {
                CreateLiteDBReport();
                return;
            }
            ExecutionLoggerConfiguration _selectedExecutionLoggerConfiguration = WorkSpace.Instance.Solution.LoggerConfigurations;
            if (!_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled)
            {
                Reporter.ToUser(eUserMsgKey.ExecutionsResultsProdIsNotOn);
                return;
            }
            HTMLReportsConfiguration currentConf = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            //create the execution logger files            
            string exec_folder = mRunner.ExecutionLoggerManager.executionLoggerHelper.GetLoggerDirectory(Path.Combine(_selectedExecutionLoggerConfiguration.CalculatedLoggerFolder,Ginger.Run.ExecutionLoggerManager.defaultAutomationTabOfflineLogName));

            if (Directory.Exists(exec_folder))
            {
                GingerCore.General.ClearDirectoryContent(exec_folder);
            }
            else
            {
                Directory.CreateDirectory(exec_folder);
            }

            if (((ExecutionLoggerManager)mRunner.ExecutionLoggerManager).OfflineBusinessFlowExecutionLog(mBusinessFlow, exec_folder))
            {
                //create the HTML report
                try
                {
                    string reportsResultFolder = Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(exec_folder), true, null, null, false, currentConf.HTMLReportConfigurationMaximalFolderSize);
                    if (reportsResultFolder == string.Empty)
                    {
                        Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Failed to generate the report for the '" + mBusinessFlow.Name + "' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ", please execute it fully first.");
                        return;
                    }
                    else
                    {
                        foreach (string txt_file in System.IO.Directory.GetFiles(reportsResultFolder))
                        {
                            string fileName = Path.GetFileName(txt_file);
                            if (fileName.Contains(".html"))
                            {
                                System.Diagnostics.Process.Start(reportsResultFolder);
                                System.Diagnostics.Process.Start(reportsResultFolder + "\\" + fileName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.WARN, "Failed to generate offline full " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " report", ex);
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Failed to generate the report for the '" + mBusinessFlow.Name + "' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ", please execute it fully first.");
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Failed to generate the report for the '" + mBusinessFlow.Name + "' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ", please execute it fully first.");
            }
        }

        private void CreateLiteDBReport()
        {
            if (mRunSetLiteDbId != null)
            {
                var selectedGuid = mRunSetLiteDbId;
                WebReportGenerator webReporterRunner = new WebReportGenerator();
                webReporterRunner.RunNewHtmlReport(selectedGuid.ToString());
            }
        }

        private void ShowExecutionSummaryPage()
        {
            ExecutionSummaryPage w = new ExecutionSummaryPage(mContext);
            w.ShowAsWindow();
        }

        private void xSummaryPageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress()) return;

            ShowExecutionSummaryPage();
        }

        private void xAutoAnalyzeConfigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AutoRunAnalyzer = !AutoRunAnalyzer;
        }

        private void xAutoReportConfigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AutoGenerateReport = !AutoGenerateReport;
        }

        private void xContinueRunsetBtn_Click(object sender, RoutedEventArgs e)
        {
            ContinueRunFromAutomatePage(eContinueFrom.LastStoppedAction);
        }

        private void xBDDGenerateScenarioMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress()) return;

            if (mBusinessFlow != null)
            {
                try
                {
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                    ScenariosGenerator SG = new ScenariosGenerator();
                    SG.CreateScenarios(mBusinessFlow);
                    int cnt = mBusinessFlow.ActivitiesGroups.Count;
                    int optCount = mBusinessFlow.ActivitiesGroups.Where(z => z.Name.StartsWith("Optimized Activities")).Count();
                    if (optCount > 0)
                    {
                        cnt = cnt - optCount;
                    }
                    Reporter.ToUser(eUserMsgKey.GherkinScenariosGenerated, cnt);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void xBDDCleanScenariosMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress()) return;

            ScenariosGenerator SG = new ScenariosGenerator();
            SG.ClearOptimizedScenariosVariables(mBusinessFlow);
            SG.ClearGeneretedActivites(mBusinessFlow);
        }

        private void xBDDOpenFeatureFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress()) return;

            DocumentEditorPage documentEditorPage = new DocumentEditorPage(mRunner.CurrentBusinessFlow.ExternalID.Replace("~", WorkSpace.Instance.Solution.Folder), true);
            documentEditorPage.Title = "Gherkin Page";
            documentEditorPage.Height = 700;
            documentEditorPage.Width = 1000;
            documentEditorPage.ShowAsWindow();
        }

        private void xLastItemReportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress()) return;

            GenerateLastExecutedItemReport();
        }

        private void GenerateLastExecutedItemReport()
        {
            if (mRunner.ExecutionLoggerManager.Configuration.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
            {
                CreateLiteDBReport();
                return;
            }
            ExecutionLoggerConfiguration _selectedExecutionLoggerConfiguration = WorkSpace.Instance.Solution.LoggerConfigurations;
            if (!_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled)
            {
                Reporter.ToUser(eUserMsgKey.ExecutionsResultsProdIsNotOn);
                return;
            }
            HTMLReportsConfiguration currentConf = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            //get logger files
            string exec_folder = mRunner.ExecutionLoggerManager.executionLoggerHelper.GetLoggerDirectory(Path.Combine(_selectedExecutionLoggerConfiguration.CalculatedLoggerFolder,Ginger.Run.ExecutionLoggerManager.defaultAutomationTabLogName));
            //create the report
            string reportsResultFolder = Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(exec_folder), true, null, null, false, currentConf.HTMLReportConfigurationMaximalFolderSize);

            if (reportsResultFolder == string.Empty)
            {
                Reporter.ToUser(eUserMsgKey.AutomationTabExecResultsNotExists);
            }
            else
            {
                foreach (string txt_file in System.IO.Directory.GetFiles(reportsResultFolder))
                {
                    string fileName = Path.GetFileName(txt_file);
                    if (fileName.Contains(".html"))
                    {
                        System.Diagnostics.Process.Start(reportsResultFolder);
                        System.Diagnostics.Process.Start(reportsResultFolder + "\\" + fileName);
                    }
                }
            }
        }
        private void xTimelineReportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress()) return;

            GingerRunnerTimeLine gingerRunnerTimeLine = (GingerRunnerTimeLine)(from x in mRunner.RunListeners where x.GetType() == typeof(GingerRunnerTimeLine) select x).SingleOrDefault();
            TimeLinePage timeLinePage = new TimeLinePage(gingerRunnerTimeLine.timeLineEvents);
            timeLinePage.ShowAsWindow();
        }

        private void xSelectedItemExecutionSyncBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mSyncSelectedItemWithExecution)
            {
                mSyncSelectedItemWithExecution = false;
                xSelectedItemExecutionSyncBtn.ButtonImageType = eImageType.Invisible;
                xSelectedItemExecutionSyncBtn.ToolTip = "Lists items selection not in sync with execution progress. Click to sync it";
            }
            else
            {
                mSyncSelectedItemWithExecution = true;
                xSelectedItemExecutionSyncBtn.ButtonImageType = eImageType.Visible;
                xSelectedItemExecutionSyncBtn.ToolTip = "Lists items selection in sync with execution progress. Click to un-sync it";
            }

            if (mBusinessFlow != null)
            {
                mBusinessFlow.Activities.SyncViewSelectedItemWithCurrentItem = mSyncSelectedItemWithExecution;
                foreach (Activity activity in mBusinessFlow.Activities)
                {
                    activity.Acts.SyncViewSelectedItemWithCurrentItem = mSyncSelectedItemWithExecution;
                }
            }
        }

        private void RunBtn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ((ucButton)sender).ButtonImageForground = (SolidColorBrush)FindResource("$SelectionColor_LightBlue");
        }

        private void RunBtn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ((ucButton)sender).ButtonImageForground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
        }

        private void xExportToCSVMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Ginger.Export.GingerToCSV.BrowseForFilename();
            Ginger.Export.GingerToCSV.BusinessFlowToCSV(mBusinessFlow);
        }
    }

    public class ActiveImageTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == false)
            {
                return Amdocs.Ginger.Common.Enums.eImageType.InActive;
            }
            else
            {
                return Amdocs.Ginger.Common.Enums.eImageType.Active;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
