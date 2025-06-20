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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.CoreNET.LiteDBFolder;
using Amdocs.Ginger.CoreNET.Logger;
using Amdocs.Ginger.CoreNET.Run.RunListenerLib;
using Amdocs.Ginger.Repository;
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
using Ginger.Repository;
using Ginger.Run;
using Ginger.TimeLineLib;
using Ginger.UserControlsLib;
using Ginger.UserControlsLib.TextEditor;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.PlugIns;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.GeneralLib;
using GingerCore.Platforms;
using GingerCoreNET;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using LiteDB;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace GingerWPF.BusinessFlowsLib
{
    /// <summary>
    /// Page used to design the Business Flow automation flow
    /// </summary>
    public partial class NewAutomatePage : GingerUIPage, INotifyPropertyChanged
    {
        GingerExecutionEngine mExecutionEngine;
        ProjEnvironment mEnvironment = null;
        BusinessFlow mBusinessFlow;
        Activity mActivity = null;
        Context mContext = new();

        ApplicationAgentsMapPage mApplicationAgentsMapPage;
        ActivitiesListViewPage mActivitiesPage;
        VariabelsListViewPage mVariabelsPage;
        BusinessFlowConfigurationsPage mConfigurationsPage;
        ActivityPage mActivityPage;
        MainAddActionsNavigationPage mAddActionMainPage;
        ActivityDetailsPage mActivityDetailsPage;
        bool mExecutionIsInProgress = false;
        bool mSyncSelectedItemWithExecution = true;

        GridLength mLastAddActionsColumnWidth = new(350);

        ObjectId mRunnerLiteDbId;
        ObjectId mRunSetLiteDbId;
        RunSetReport mRunSetReport;
        string allProperties = string.Empty;
        public event PropertyChangedEventHandler PropertyChanged;
        public static event EventHandler RaiseEnvComboBoxChanged;
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
            App.AutomateBusinessFlowEvent -= App_AutomateBusinessFlowEventAsync;
            App.AutomateBusinessFlowEvent += App_AutomateBusinessFlowEventAsync;
            WorkSpace.Instance.PropertyChanged -= WorkSpacePropertyChanged;
            WorkSpace.Instance.PropertyChanged += WorkSpacePropertyChanged;

            InitAutomatePageRunner();
            UpdateAutomatePageRunner();

            SetUIControls();
        }

        #region LiteDB
        private void SetRunSetDBLite(BusinessFlow businessFlowToLoad)
        {
            if (mExecutionEngine.ExecutionLoggerManager.Configuration.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
            {
                if (mRunSetReport == null)
                {
                    mRunSetReport = new RunSetReport();
                    mRunSetReport.SetDataForAutomateTab();
                    ExecutionLoggerManager.RunSetReport = mRunSetReport;
                    bool isAutoRunSetExists = SetOrClearPreviousAutoRunSetDocumentLiteDB(false);
                    if (!isAutoRunSetExists)
                    {
                        mExecutionEngine.ExecutionLoggerManager.BusinessFlowEnd(0, businessFlowToLoad, true);
                        mExecutionEngine.ExecutionLoggerManager.RunnerRunEnd(0, mExecutionEngine.GingerRunner, offlineMode: true);
                        ((ExecutionLogger)mExecutionEngine.ExecutionLoggerManager.mExecutionLogger).SetReportRunSet(mRunSetReport, "", eExecutedFrom.Automation);
                        SetOrClearPreviousAutoRunSetDocumentLiteDB(false);
                        return;
                    }
                    SetOrClearPreviousAutoRunSetDocumentLiteDB(true);
                }
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

        private bool SetOrClearPreviousAutoRunSetDocumentLiteDB(bool isClear = true)
        {
            try
            {
                LiteDbManager dbManager = new LiteDbManager(mExecutionEngine.ExecutionLoggerManager.Configuration.CalculatedLoggerFolder);
                var result = dbManager.GetRunSetLiteData();
                var filterData = result.FindOne(a => a.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Automated.ToString());
                if (filterData != null)
                {
                    if (isClear)
                    {
                        LiteDbConnector dbConnector = new LiteDbConnector(Path.Combine(mExecutionEngine.ExecutionLoggerManager.Configuration.CalculatedLoggerFolder, "GingerExecutionResults.db"));
                        dbConnector.DeleteDocumentByLiteDbRunSet(filterData, eExecutedFrom.Automation);
                    }
                    else
                    {
                        mRunSetLiteDbId = filterData._id;
                        mRunnerLiteDbId = filterData.RunnersColl[0]._id;
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception during accessing litedb file", ex);
            }
            return false;

        }
        #endregion LiteDB

        private void SetUIControls()
        {
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xALMMenuItem, Expander.VisibilityProperty, WorkSpace.Instance.UserProfile, nameof(WorkSpace.Instance.UserProfile.ShowEnterpriseFeatures), bindingConvertor: new GingerCore.GeneralLib.BoolVisibilityConverter());

            xBusinessFlowItemComboBox.Items.Add(GingerDicser.GetTermResValue(eTermResKey.Activities));
            xBusinessFlowItemComboBox.Items.Add(GingerDicser.GetTermResValue(eTermResKey.Variables));
            xBusinessFlowItemComboBox.Items.Add("Details");
            xBusinessFlowItemComboBox.SelectedIndex = 0;

            BindingHandler.ObjFieldBinding(xAutoAnalyzeConfigMenuItemIcon, ImageMakerControl.ImageTypeProperty, WorkSpace.Instance.UserProfile, nameof(UserProfile.AutoRunAutomatePageAnalyzer), bindingConvertor: new ActiveImageTypeConverter(), BindingMode.OneWay);
            BindingHandler.ObjFieldBinding(xAutoReportConfigMenuItemIcon, ImageMakerControl.ImageTypeProperty, WorkSpace.Instance.UserProfile, nameof(UserProfile.AutoGenerateAutomatePageReport), bindingConvertor: new ActiveImageTypeConverter(), BindingMode.OneWay);
            mApplicationAgentsMapPage = new ApplicationAgentsMapPage(mExecutionEngine, mContext);
            mApplicationAgentsMapPage.MappingList.Height = 60;
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
                OnTargetApplicationChanged(sender, null);
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
            if (e.PropertyName == nameof(AgentOperations.Status))
            {
                if ((sender as Agent) != null && (sender as Agent) == mContext.Agent)
                {
                    mContext.AgentStatus = Convert.ToString(((AgentOperations)((Agent)sender).AgentOperations).Status);
                }
            }
        }

        /// <summary>
        /// This method will set the Agent, Target and Platform for current activity in Context
        /// </summary>
        private void UpdateContextWithActivityDependencies()
        {
            ApplicationAgent appAgent = AgentHelper.GetAppAgent(mContext.Activity, (GingerExecutionEngine)mContext.Runner, mContext);
            if (appAgent != null)
            {
                PropertyChangedEventManager.RemoveHandler(source: appAgent, handler: AppAgent_PropertyChanged, propertyName: allProperties);
                PropertyChangedEventManager.AddHandler(source: appAgent, handler: AppAgent_PropertyChanged, propertyName: allProperties);

                UpdateTargetAndPlatform();

                if (mContext.Agent != appAgent.Agent)
                {
                    mContext.Agent = appAgent.Agent;
                    //mContext.AgentStatus = appAgent.Agent.Status.ToString();
                }

                if (mContext.Agent != null)
                {
                    PropertyChangedEventManager.RemoveHandler(source: mContext.Agent, handler: Agent_PropertyChanged, propertyName: allProperties);
                    PropertyChangedEventManager.AddHandler(source: mContext.Agent, handler: Agent_PropertyChanged, propertyName: allProperties);
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
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            if (xAddActionsBtn.ButtonImageType == Amdocs.Ginger.Common.Enums.eImageType.Add)
            {
                if (mAddActionMainPage == null)
                {
                    mAddActionMainPage = new MainAddActionsNavigationPage(mContext);
                }
                xAddActionMenuFrame.ClearAndSetContent(mAddActionMainPage);

                ExpandAddActionsPnl();
                Ginger.General.DoEvents();
                App.MainWindow.AddHelpLayoutToShow("AutomatePage_AddActionsPageHelp", xAddActionMenuFrame, string.Format("List of options is dynamic, options are loaded based on the target platform of current {0} and on it mapped Agent status.For example, “Record” option will be added only if the platform is UI based (like Web, Java, etc.) and Agent it loaded", GingerDicser.GetTermResValue(eTermResKey.Activity)));
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
            GingerRunner gingerRunner = new GingerRunner();
            mExecutionEngine = new GingerExecutionEngine(gingerRunner, eExecutedFrom.Automation);
            PropertyChangedEventManager.AddHandler(source: mExecutionEngine.GingerRunner, handler: MRunner_PropertyChanged, propertyName: allProperties);

            // Add Listener so we can do GiveUserFeedback            
            AutomatePageRunnerListener automatePageRunnerListener = new AutomatePageRunnerListener
            {
                AutomatePageRunnerListenerGiveUserFeedback = GiveUserFeedback
            };
            mExecutionEngine.RunListeners.Add(automatePageRunnerListener);

            mExecutionEngine.Context = mContext;
            mContext.Runner = mExecutionEngine;
        }

        private void MRunner_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GingerRunner.SpecificEnvironmentName))
            {
                if (!string.IsNullOrEmpty(mExecutionEngine.GingerRunner.SpecificEnvironmentName))
                {
                    xEnvironmentComboBox.SelectedItem = (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().First(x => x.Name == mExecutionEngine.GingerRunner.SpecificEnvironmentName));
                    if (RaiseEnvComboBoxChanged != null)
                    {
                        RaiseEnvComboBoxChanged(null, null);
                    }
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
            else if (e.PropertyName == nameof(GingerExecutionEngine.IsRunning))
            {
                mExecutionIsInProgress = mExecutionEngine.IsRunning;
                SetUIElementsBehaverDuringExecution();
            }
        }

        public async Task LoadBusinessFlowToAutomate(BusinessFlow businessFlowToLoad)
        {
            if (mExecutionIsInProgress)
            {
                StopAutomateRun();
            }
            SetEnvsCombo();
            if (mBusinessFlow != businessFlowToLoad)
            {
                RemoveCurrentBusinessFlow();
                ResetPageUI();

                mBusinessFlow = businessFlowToLoad;

                CurrentItemToSave = mBusinessFlow;
                if (mBusinessFlow != null)
                {
                    try
                    {
                        xActivitiesLoadingPnl.Visibility = Visibility.Visible;
                        xActivitiesDataGrid.Visibility = Visibility.Collapsed;

                        mContext.BusinessFlow = mBusinessFlow;

                        //Runner update
                        mExecutionEngine.BusinessFlows.Add(mBusinessFlow);
                        mExecutionEngine.CurrentBusinessFlow = mBusinessFlow;

                        //Upper menu update
                        BindingHandler.ObjFieldBinding(xBusinessFlowNameTxtBlock, TextBlock.TextProperty, mBusinessFlow, nameof(BusinessFlow.Name));
                        xBusinessFlowNameTxtBlock.ToolTip = mBusinessFlow.ContainingFolder + "\\" + mBusinessFlow.Name;
                        if (mBusinessFlow.Source == BusinessFlow.eSource.Gherkin)
                        {
                            xBDDOperationsMenu.Visibility = Visibility.Visible;
                        }
                        UpdateApplicationsAgentsMapping();

                        SetBusinessFlowTargetAppIfNeeded();
                        CollectionChangedEventManager.AddHandler(source: mBusinessFlow.TargetApplications, handler: mBusinessFlowTargetApplications_CollectionChanged);
                        UpdateRunnerAgentsUsedBusinessFlow();

                        Ginger.General.DoEvents();//so UI will refresh


                        PropertyChangedEventManager.AddHandler(source: mBusinessFlow, handler: mBusinessFlow_PropertyChanged, propertyName: allProperties);
                        CollectionChangedEventManager.RemoveHandler(source: mBusinessFlow.Activities, handler: OnActivitiesListChanged);
                        CollectionChangedEventManager.AddHandler(source: mBusinessFlow.Activities, handler: OnActivitiesListChanged);


                        //--BF sections updates
                        //Environments
                        UpdateUsedEnvironment((ProjEnvironment)xEnvironmentComboBox.SelectedItem);
                        //Configurations
                        if (mConfigurationsPage == null)
                        {
                            mConfigurationsPage = new BusinessFlowConfigurationsPage(mBusinessFlow, mContext, Ginger.General.eRIPageViewMode.Automation);
                            xBfConfigurationsTabFrame.ClearAndSetContent(mConfigurationsPage);
                        }
                        else
                        {
                            mConfigurationsPage.UpdateBusinessFlow(mBusinessFlow);
                        }
                        //Variables
                        if (mVariabelsPage == null)
                        {
                            mVariabelsPage = new VariabelsListViewPage(mBusinessFlow, mContext, Ginger.General.eRIPageViewMode.Automation);
                            xBfVariablesTabFrame.ClearAndSetContent(mVariabelsPage);
                        }
                        else
                        {
                            mVariabelsPage.UpdateParent(mBusinessFlow);
                        }
                        //Activities
                        if (mActivitiesPage == null)
                        {
                            mActivitiesPage = new ActivitiesListViewPage(mBusinessFlow, mContext, Ginger.General.eRIPageViewMode.Automation);
                            WeakEventManager<Selector, SelectionChangedEventArgs>.RemoveHandler(source: mActivitiesPage.ListView.List, eventName: nameof(Selector.SelectionChanged), handler: ActivitiesList_SelectionChanged);
                            WeakEventManager<Selector, SelectionChangedEventArgs>.AddHandler(source: mActivitiesPage.ListView.List, eventName: nameof(Selector.SelectionChanged), handler: ActivitiesList_SelectionChanged);
                            xActivitiesListFrame.ClearAndSetContent(mActivitiesPage);
                        }
                        else
                        {
                            mActivitiesPage.UpdateBusinessFlow(mBusinessFlow);
                        }
                        if (mBusinessFlow.Activities.Count > 0)
                        {
                            mActivity = mBusinessFlow.Activities[0];
                            mBusinessFlow.CurrentActivity = mActivity;
                            mContext.Activity = mActivity;
                            PropertyChangedEventManager.RemoveHandler(source: mActivity, handler: Activity_PropertyChanged, propertyName: allProperties);
                            PropertyChangedEventManager.AddHandler(source: mActivity, handler: Activity_PropertyChanged, propertyName: allProperties);

                            if (mContext.Platform == ePlatformType.NA)
                            {
                                UpdateContextWithActivityDependencies();
                            }
                        }

                        //LiteDB updates
                        if (SetOrClearPreviousAutoRunSetDocumentLiteDB(false) && mRunSetReport != null && mExecutionEngine.ExecutionLoggerManager.Configuration.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
                        {
                            SetOrClearPreviousAutoRunSetDocumentLiteDB(true);
                            ClearAutomatedId(businessFlowToLoad);
                            ((ExecutionLogger)mExecutionEngine.ExecutionLoggerManager.mExecutionLogger).RunSetUpdate(mRunSetLiteDbId, mRunnerLiteDbId, mExecutionEngine);
                        }
                        SetRunSetDBLite(mBusinessFlow);//TODO: check if we must do it per BF change

                        SetActivityEditPage();//Validate we don't do twice
                    }
                    finally
                    {
                        xActivitiesLoadingPnl.Visibility = Visibility.Collapsed;
                        xActivitiesDataGrid.Visibility = Visibility.Visible;
                    }

                    //Create backup
                    if (mBusinessFlow.DirtyStatus == eDirtyStatus.NoChange)
                    {
                        Reporter.ToStatus(eStatusMsgKey.CreatingBackupProcess, null, mBusinessFlow.Name);
                        await mBusinessFlow.SaveBackupAsync().ConfigureAwait(false);
                        Reporter.HideStatusMessage();
                    }
                }
            }
        }

        private void OnActivitiesListChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnTargetApplicationChanged(sender, null);
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
                PropertyChangedEventManager.RemoveHandler(source: mActivity, handler: Activity_PropertyChanged, propertyName: allProperties);
                mActivity.StopTimer();
            }
            mActivity = (Activity)mActivitiesPage.ListView.CurrentItem;

            if (mBusinessFlow.Activities.SyncCurrentItemWithViewSelectedItem)
            {
                mBusinessFlow.CurrentActivity = mActivity;
            }

            mContext.Activity = mActivity;
            if (mActivity != null)
            {
                PropertyChangedEventManager.RemoveHandler(source: mActivity, handler: Activity_PropertyChanged, propertyName: allProperties);
                PropertyChangedEventManager.AddHandler(source: mActivity, handler: Activity_PropertyChanged, propertyName: allProperties);
            }

            UpdateContextWithActivityDependencies();
            OnTargetApplicationChanged(null, null);
            SetActivityEditPage();
        }

        public void UpdateRunnerAgentsUsedBusinessFlow()
        {
            foreach (ApplicationAgent appAgent in mExecutionEngine.GingerRunner.ApplicationAgents)
            {
                ApplicationAgentOperations applicationAgentOperations = new ApplicationAgentOperations(appAgent);
                appAgent.ApplicationAgentOperations = applicationAgentOperations;

                if (appAgent.Agent != null)
                {
                    if (appAgent.Agent.AgentOperations == null)
                    {
                        appAgent.Agent.AgentOperations = new AgentOperations(appAgent.Agent);
                    }
                    if (((AgentOperations)appAgent.Agent.AgentOperations).Status == Agent.eStatus.Running)
                    {
                        ((AgentOperations)appAgent.Agent.AgentOperations).Driver.UpdateContext(mContext);
                    }
                }
            }
        }

        private void SetActivityEditPage()
        {
            try
            {
                if (mActivityPage != null && mActivityPage.Activity == mContext.Activity)
                {
                    return;
                }

                xCurrentActivityLoadingIconPnl.Visibility = Visibility.Visible;
                xCurrentActivityFrame.Visibility = Visibility.Collapsed;
                Ginger.General.DoEvents();

                if (mContext.Activity != null)
                {
                    if (mActivityPage == null)
                    {
                        var pageViewMode = mContext.Activity.Type == Amdocs.Ginger.Repository.eSharedItemType.Regular ? Ginger.General.eRIPageViewMode.Automation : Ginger.General.eRIPageViewMode.ViewAndExecute;
                        mActivityPage = new ActivityPage(mContext.Activity, mContext, pageViewMode, highlightActivityName: true);
                    }
                    else
                    {
                        mActivityPage.UpdateActivity(mContext.Activity);
                        ToggleActivityPageUIButtons(!mExecutionIsInProgress);
                    }
                    mActivityDetailsPage = new ActivityDetailsPage(mContext.Activity, mContext, mContext.Activity.Type == Amdocs.Ginger.Repository.eSharedItemType.Regular ? Ginger.General.eRIPageViewMode.Automation : Ginger.General.eRIPageViewMode.ViewAndExecute);
                    /*                    mActivityDetailsPage.xTargetApplicationComboBox.SelectionChanged -= OnTargetApplicationChanged;
                                        mActivityDetailsPage.xTargetApplicationComboBox.SelectionChanged += OnTargetApplicationChanged;
                    */
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

        private void OnTargetApplicationChanged(object arg1, SelectionChangedEventArgs args)
        {
            var selectedTargetApplication = (mActivity != null) ? WorkSpace.Instance.Solution.GetSolutionTargetApplications().FirstOrDefault((targetApp) => targetApp.Name.Equals(mActivity.TargetApplication)) as TargetApplication : null;

            if (selectedTargetApplication != null && !mBusinessFlow.TargetApplications.Any(bfTA => ((TargetApplication)bfTA).AppName.Equals(selectedTargetApplication.AppName)))
            {
                //    ApplicationAgent applicationAgent = new ApplicationAgent() { AppName = ((TargetApplication)actTargetApp).AppName };
                //    applicationAgent.ApplicationAgentOperations = new ApplicationAgentOperations(applicationAgent);
                //    applicationAgent.Agent = applicationAgent.PossibleAgents?.FirstOrDefault((agent) => agent.Name.Equals(actTargetApp.LastExecutingAgentName)) as Agent;
                //    if (applicationAgent.Agent == null && applicationAgent.PossibleAgents?.Count >= 1)
                //    {
                //        applicationAgent.Agent = applicationAgent.PossibleAgents[0] as Agent;
                //    }
                mBusinessFlow.TargetApplications.Add(selectedTargetApplication);
            }


            // Create a list to store the items to be removed
            List<TargetBase> TargetApplicationsToRemove = [];
            var userTA = mBusinessFlow.Activities.Select(f => f.TargetApplication);

            // Iterate through the Business Flow Target Application
            foreach (var existingTargetApp in mBusinessFlow.TargetApplications.OfType<TargetApplication>())
            {
                // Check if the existing target application is not present in mBusinessFlow.TargetApplications
                if (!userTA.Contains(existingTargetApp.AppName))
                {
                    // If not present, add to the removal list
                    TargetApplicationsToRemove.Add(existingTargetApp);
                }
            }

            // Remove the target applications from mBusinessFlow.TargetApplications
            foreach (var TargetAppToRemove in TargetApplicationsToRemove)
            {
                mBusinessFlow.TargetApplications.Remove(TargetAppToRemove);
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
                if (mBusinessFlow.TargetApplications.Count == 0)
                {
                    if (string.IsNullOrEmpty(WorkSpace.Instance.Solution.MainApplication))
                    {
                        Reporter.ToUser(eUserMsgKey.StaticWarnMessage, $"You must have at least one {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} configured, please set it up.");
                        return;
                    }
                    else
                    {
                        // take it from solution main platform
                        if (mBusinessFlow.TargetApplications == null)
                        {
                            mBusinessFlow.TargetApplications = [];
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
                mExecutionEngine.BusinessFlows.Clear();
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
                mExecutionEngine.SolutionAgents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>();
            }
            else
            {
                mExecutionEngine.SolutionAgents = null;
            }
            mExecutionEngine.UpdateApplicationAgents();
        }

        void ToggleProcessButtons(bool isEnabled)
        {
            xSaveBusinessFlowBtn.IsEnabled = isEnabled;
            xUndoChangesBtn.IsEnabled = isEnabled;
            xResetFlowBtn.IsEnabled = isEnabled;
            xBusinessFlowOperationssPnl.IsEnabled = isEnabled;
            xRunFlowBtn.IsEnabled = isEnabled;
            xEnvironmentComboBox.IsEnabled = isEnabled;

            if (mApplicationAgentsMapPage != null)
            {
                mApplicationAgentsMapPage.MappingList.IsEnabled = isEnabled;
            }
        }

        void ToggleActivityPageUIButtons(bool IsEnabled)
        {
            mActivityPage.SetUIElementsBehaverBasedOnRunnerStatus(IsEnabled);
        }

        private void SetUIElementsBehaverDuringExecution()
        {
            this.Dispatcher.Invoke(() =>
            {
                ToggleProcessButtons(!mExecutionIsInProgress);
                ToggleActivityPageUIButtons(!mExecutionIsInProgress);

                if (mExecutionIsInProgress)
                {
                    xRunFlowBtn.ButtonImageType = eImageType.Running;
                    xRunFlowBtn.ButtonText = "Running";
                    xRunFlowBtn.ToolTip = "Execution is in progress";
                    xRunFlowBtn.IsEnabled = false;
                    xRunFlowBtn.ButtonStyle = (Style)FindResource("$RoundTextAndImageButtonStyle_ExecutionRunning");
                    xRunFlowBtn.ButtonImageForground = (SolidColorBrush)FindResource("$HighlightColor_LightBlue");

                    xStopRunBtn.ButtonImageType = eImageType.Stop;
                    xStopRunBtn.ButtonText = "Stop";
                    xStopRunBtn.ToolTip = "Stop Execution";
                    xStopRunBtn.IsEnabled = true;
                    xStopRunBtn.Visibility = Visibility.Visible;
                    xStopRunBtn.ButtonStyle = (Style)FindResource("$RoundTextAndImageButtonStyle_ExecutionStop");

                    xEnvironmentComboBox.IsEnabled = false;
                    if (mApplicationAgentsMapPage != null)
                    {
                        mApplicationAgentsMapPage.MappingList.IsEnabled = false;
                    }

                    if (xAddActionsBtn.ButtonImageType != Amdocs.Ginger.Common.Enums.eImageType.Add && !WindowExplorerCommon.IsTestActionRunning)
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

        private void xRunFlowBtn_UpdateDuringAnalyzing()
        {
            Dispatcher.Invoke(() =>
            {
                xRunFlowBtn.ButtonImageType = eImageType.Running;
                xRunFlowBtn.ButtonText = "Analyzing...";
                xRunFlowBtn.ToolTip = "Analyzing in progress";
                xRunFlowBtn.IsEnabled = false;
                xRunFlowBtn.ButtonStyle = (Style)FindResource("$RoundTextAndImageButtonStyle_ExecutionRunning");
                xRunFlowBtn.ButtonImageForground = (SolidColorBrush)FindResource("$HighlightColor_LightBlue");
            });
        }

        private void xStopRunBtn_UpdateDuringStopping()
        {
            Dispatcher.Invoke(() =>
            {
                xStopRunBtn.ButtonImageType = eImageType.Running;
                xStopRunBtn.ButtonText = "Stopping...";
                xStopRunBtn.ToolTip = "Stopping execution";
                xStopRunBtn.IsEnabled = false;
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
                mExecutionEngine.StopRun();
                xStopRunBtn_UpdateDuringStopping();
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



        private async void App_AutomateBusinessFlowEventAsync(AutomateEventArgs args)
        {
            switch (args.EventType)
            {
                case AutomateEventArgs.eEventType.Automate:
                    await LoadBusinessFlowToAutomate((BusinessFlow)args.Object);
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
                    await RunAutomatePageAction((Tuple<Activity, Act, bool>)args.Object, false).ConfigureAwait(false);
                    break;
                case AutomateEventArgs.eEventType.RunCurrentActionAndMoveOn:
                    await RunAutomatePageAction((Tuple<Activity, Act, bool>)args.Object).ConfigureAwait(false);
                    break;
                case AutomateEventArgs.eEventType.RunCurrentActivity:
                    await RunAutomatePageActivity((Activity)args.Object).ConfigureAwait(false);
                    break;
                case AutomateEventArgs.eEventType.ContinueActionRun:
                    await ContinueRunFromAutomatePage(eContinueFrom.SpecificAction, args.Object).ConfigureAwait(false);
                    break;
                case AutomateEventArgs.eEventType.ContinueActivityRun:
                    await ContinueRunFromAutomatePage(eContinueFrom.SpecificActivity, args.Object).ConfigureAwait(false);
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
        private void InitLiteDBItems()
        {
            mRunSetReport = null;
            mRunSetLiteDbId = null;
            mRunnerLiteDbId = null;
            ExecutionLoggerConfiguration.DataRepositoryMethod dataRepositoryMethod = WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList[0].SelectedDataRepositoryMethod;
            if (dataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
            {
                mExecutionEngine.ExecutionLoggerManager.mExecutionLogger = new LiteDBRepository();
            }
            else if (dataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.TextFile)
            {
                mExecutionEngine.ExecutionLoggerManager.mExecutionLogger = new TextFileRepository();
            }
            mExecutionEngine.ExecutionLoggerManager.ExecutionLogfolder = string.Empty;
            mExecutionEngine.ExecutionLoggerManager.Configuration = WorkSpace.Instance.Solution.LoggerConfigurations;
        }

        private void UpdateAutomatePageRunner()
        {
            mExecutionEngine.CurrentSolution = WorkSpace.Instance.Solution;
            mExecutionEngine.SolutionFolder = WorkSpace.Instance.Solution.Folder;
            mExecutionEngine.SolutionAgents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>();
            mExecutionEngine.SolutionApplications = WorkSpace.Instance.Solution.ApplicationPlatforms;
            mExecutionEngine.GingerRunner.DSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
            InitLiteDBItems();
        }


        private async Task RunAutomateTabFlow()
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            IEnumerable<RepositoryItemBase> itemsWithDevTimeTracking = PauseBusinessFlowAndActivitiesDevelopmentTimeTracking();
            try
            {
                //mExecutionIsInProgress = true;
                //SetUIElementsBehaverDuringExecution();

                if (WorkSpace.Instance.UserProfile.AutoRunAutomatePageAnalyzer)
                {
                    //Run Analyzer check if not including any High or Critical issues before execution
                    Reporter.ToStatus(eStatusMsgKey.AnalyzerIsAnalyzing, null, mBusinessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                    xRunFlowBtn_UpdateDuringAnalyzing();
                    try
                    {
                        AnalyzerPage analyzerPage = new AnalyzerPage();

                        analyzerPage.Init(mBusinessFlow, solution: null, mExecutionEngine.GingerRunner.ApplicationAgents, WorkSpace.Instance.AutomateTabSelfHealingConfiguration.AutoFixAnalyzerIssue);
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
                        SetUIElementsBehaverDuringExecution();
                        Reporter.HideStatusMessage();
                    }
                }

                //execute preparations               
                mExecutionEngine.ResetRunnerExecutionDetails(false, true);
                mExecutionEngine.ExecutionLoggerManager.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.BussinessFlowRun;

                //execute                
                await mExecutionEngine.RunBusinessFlowAsync(mBusinessFlow, true, false).ConfigureAwait(false);
                this.Dispatcher.Invoke(() =>
                {
                    CalculateFinalStatusForReport();
                    if (WorkSpace.Instance.UserProfile.AutoGenerateAutomatePageReport)
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
                Reporter.ToLog(eLogLevel.ERROR, "Exception during flow execution from automate tab", ex);
            }
            finally
            {
                //mExecutionIsInProgress = false;
                //SetUIElementsBehaverDuringExecution();
                mExecutionEngine.ResetFailedToStartFlagForAgents();

                ResumeBusinessFlowAndActivitiesDevelopmentTimeTracking(itemsWithDevTimeTracking);
            }
        }

        public async Task RunAutomatePageActivity(Activity activityToExecute)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            IEnumerable<RepositoryItemBase> itemsWithDevTimeTracking = PauseBusinessFlowAndActivitiesDevelopmentTimeTracking();
            try
            {
                //mExecutionIsInProgress = true;
                //SetUIElementsBehaverDuringExecution();

                mContext.BusinessFlow.CurrentActivity = activityToExecute;
                mContext.Runner.ExecutionLoggerManager.Configuration.ExecutionLoggerAutomationTabContext = Ginger.Reports.ExecutionLoggerConfiguration.AutomationTabContext.ActivityRun;

                if (mExecutionEngine.ExecutionLoggerManager.Configuration.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
                {
                    try
                    {
                        foreach (Activity activity in mBusinessFlow.Activities)
                        {
                            if (activity == activityToExecute)
                            {
                                break;
                            }
                            foreach (Act action in activity.Acts.Cast<Act>())
                            {
                                mExecutionEngine.ExecutionLoggerManager.ActionEnd(0, action);
                            }
                            mExecutionEngine.ExecutionLoggerManager.ActivityEnd(0, activity);
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Error while logging execution data to LiteDB", ex);
                    }
                }

                await mExecutionEngine.RunActivityAsync(activityToExecute, false, true, resetErrorHandlerExecutedFlag: true).ConfigureAwait(false);

                //When running Runactivity as standalone from GUI, SetActionSkipStatus is not called. Handling it here for now.
                foreach (Act act in activityToExecute.Acts)
                {
                    if (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending)
                    {
                        act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                    }
                }

                if (mExecutionEngine.ExecutionLoggerManager.Configuration.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
                {
                    try
                    {
                        bool reachedCurrentActivity = false;
                        foreach (Activity activity in mBusinessFlow.Activities)
                        {
                            reachedCurrentActivity = activity == activityToExecute || reachedCurrentActivity;
                            if (!reachedCurrentActivity || activity == activityToExecute)
                            {
                                continue;
                            }
                            foreach (Act action in activity.Acts.Cast<Act>())
                            {
                                mExecutionEngine.ExecutionLoggerManager.ActionEnd(0, action);
                            }
                            mExecutionEngine.ExecutionLoggerManager.ActivityEnd(0, activity);
                        }
                        mExecutionEngine.ExecutionLoggerManager.BusinessFlowEnd(0, mBusinessFlow);
                        ((ExecutionLogger)mExecutionEngine.ExecutionLoggerManager.mExecutionLogger).RunSetUpdate(mRunSetLiteDbId, mRunnerLiteDbId, mExecutionEngine);
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Error while logging execution data to LiteDB", ex);
                    }
                }
            }
            finally
            {
                if (activityToExecute.CurrentAgent != null)
                {
                    ((AgentOperations)((Agent)activityToExecute.CurrentAgent).AgentOperations).IsFailedToStart = false;
                }

                ResumeBusinessFlowAndActivitiesDevelopmentTimeTracking(itemsWithDevTimeTracking);
            }
        }

        public async Task RunAutomatePageAction(Tuple<Activity, Act, bool> actionToExecuteInfo, bool moveToNextAction = true, bool checkIfActionAllowedToRun = true)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            bool skipInternalValidations = actionToExecuteInfo.Item3;

            Activity parentActivity;

            if (skipInternalValidations)
            {
                parentActivity = mBusinessFlow.CurrentActivity;
            }
            else
            {
                parentActivity = actionToExecuteInfo.Item1;
            }

            Act actionToExecute = actionToExecuteInfo.Item2;

            if (actionToExecute == null)
            {
                Reporter.ToUser(eUserMsgKey.NoActionAvailable);
                return;
            }

            // set errorhandler execution status
            actionToExecute.ErrorHandlerExecuted = false;

            if (!parentActivity.Acts.Any() && !skipInternalValidations)
            {
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "No Action to Run.");
                return;
            }

            // If no action selected move to the first.
            if (actionToExecute == null)
            {
                actionToExecute = (Act)parentActivity.Acts[0];
            }

            IEnumerable<RepositoryItemBase> itemsWithDevTimeTracking = PauseBusinessFlowAndActivitiesDevelopmentTimeTracking();
            try
            {
                //mExecutionIsInProgress = true;
                //SetUIElementsBehaverDuringExecution();

                mBusinessFlow.CurrentActivity = parentActivity;
                if (!skipInternalValidations)
                {
                    mBusinessFlow.CurrentActivity.Acts.CurrentItem = actionToExecute;
                }
                mExecutionEngine.ExecutionLoggerManager.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.ActionRun;

                //No need of agent for actions like DB and read for excel. For other need agent  
                Type actType = actionToExecute.GetType();
                if (!(typeof(ActWithoutDriver).IsAssignableFrom(actType)) || actType == typeof(ActAgentManipulation))   // ActAgentManipulation not needed
                {
                    mExecutionEngine.SetCurrentActivityAgent();
                }
                else if ((typeof(ActPlugIn).IsAssignableFrom(actType)))
                {
                    mExecutionEngine.SetCurrentActivityAgent();
                }

                mExecutionEngine.ExecutionLoggerManager.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.ActionRun;

                if (mExecutionEngine.ExecutionLoggerManager.Configuration.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
                {
                    try
                    {
                        bool reachedCurrentAction = false;
                        foreach (Activity activity in mBusinessFlow.Activities)
                        {
                            foreach (Act action in activity.Acts.Cast<Act>())
                            {
                                if (activity == parentActivity && action == actionToExecute)
                                {
                                    reachedCurrentAction = true;
                                    break;
                                }
                                mExecutionEngine.ExecutionLoggerManager.ActionEnd(0, action);
                            }
                            if (reachedCurrentAction)
                            {
                                break;
                            }
                            mExecutionEngine.ExecutionLoggerManager.ActivityEnd(0, activity);
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Error while logging execution data to LiteDB", ex);
                    }
                }

                var result = await mExecutionEngine.RunActionAsync(actionToExecute, checkIfActionAllowedToRun, moveToNextAction).ConfigureAwait(false);

                if (mExecutionEngine.ExecutionLoggerManager.Configuration.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
                {
                    try
                    {
                        bool reachedCurrentActivity = false;
                        bool reachedCurrentAction = false;
                        foreach (Activity activity in mBusinessFlow.Activities)
                        {
                            reachedCurrentActivity = activity == parentActivity || reachedCurrentActivity;
                            if (!reachedCurrentActivity)
                            {
                                continue;
                            }
                            foreach (Act action in activity.Acts.Cast<Act>())
                            {
                                reachedCurrentAction = action == actionToExecute || reachedCurrentAction;
                                if (!reachedCurrentAction || action == actionToExecute)
                                {
                                    continue;
                                }
                                mExecutionEngine.ExecutionLoggerManager.ActionEnd(0, action);
                            }
                            mExecutionEngine.ExecutionLoggerManager.ActivityEnd(0, activity);
                        }
                        mExecutionEngine.ExecutionLoggerManager.BusinessFlowEnd(0, mBusinessFlow);
                        ((ExecutionLogger)mExecutionEngine.ExecutionLoggerManager.mExecutionLogger).RunSetUpdate(mRunSetLiteDbId, mRunnerLiteDbId, mExecutionEngine);
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Error while logging execution data to LiteDB", ex);
                    }
                }

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception in RunAutomatePageAction", ex);
            }
            finally
            {
                if (mExecutionEngine.CurrentBusinessFlow.CurrentActivity.CurrentAgent != null)
                {
                    if (((Agent)mExecutionEngine.CurrentBusinessFlow.CurrentActivity.CurrentAgent).Status == Agent.eStatus.NotStarted)
                    {

                        ((AgentOperations)((Agent)mExecutionEngine.CurrentBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations).Close();
                    }

                    ((AgentOperations)((Agent)mExecutionEngine.CurrentBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations).IsFailedToStart = false;
                }

                ResumeBusinessFlowAndActivitiesDevelopmentTimeTracking(itemsWithDevTimeTracking);
            }
        }

        private IEnumerable<RepositoryItemBase> PauseBusinessFlowAndActivitiesDevelopmentTimeTracking()
        {
            List<RepositoryItemBase> itemsWithTimerRunning = [];

            try
            {
                if (mBusinessFlow.IsTimerRunning())
                {
                    itemsWithTimerRunning.Add(mBusinessFlow);
                    mBusinessFlow.StopTimer();
                }
                itemsWithTimerRunning.AddRange(mBusinessFlow.StopTimerWithActivities());
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "error while pausing development time tracker for business flow and activities", ex);
            }

            return itemsWithTimerRunning;
        }

        private void ResumeBusinessFlowAndActivitiesDevelopmentTimeTracking(IEnumerable<RepositoryItemBase> items)
        {
            try
            {
                foreach (RepositoryItemBase item in items)
                {
                    if (item is BusinessFlow bf)
                    {
                        bf.StartTimer();
                    }
                    else if (item is Activity activity)
                    {
                        activity.StartTimer();
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "error while pausing development time tracker for business flow and activities", ex);
            }
        }

        private async Task ContinueRunFromAutomatePage(eContinueFrom continueFrom, object executedItem = null)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            try
            {
                //mExecutionIsInProgress = true;
                //SetUIElementsBehaverDuringExecution();

                mExecutionEngine.ExecutionLoggerManager.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.ContinueRun;

                switch (continueFrom)
                {
                    case eContinueFrom.LastStoppedAction:

                        LogExecutionDetailsUpToLastExecutedAction(mExecutionEngine.ExecutedActivityWhenStopped, mExecutionEngine.ExecutedActionWhenStopped);
                        await mExecutionEngine.ContinueRunAsync(eContinueLevel.StandalonBusinessFlow, eContinueFrom.LastStoppedAction);
                        break;
                    case eContinueFrom.SpecificAction:
                        Activity parentActivity = ((Tuple<Activity, Act>)executedItem).Item1;
                        Act actionToExecute = ((Tuple<Activity, Act>)executedItem).Item2;
                        LogExecutionDetailsUpToLastExecutedAction(parentActivity, actionToExecute);

                        await mExecutionEngine.ContinueRunAsync(eContinueLevel.StandalonBusinessFlow, eContinueFrom.SpecificAction, mBusinessFlow, parentActivity, actionToExecute);
                        break;
                    case eContinueFrom.SpecificActivity:
                        Activity activityToExecute = (Activity)executedItem;

                        LogExecutionDetailsUpToLastExecutedActivity(activityToExecute);

                        mBusinessFlow.CurrentActivity = activityToExecute;

                        await mExecutionEngine.ContinueRunAsync(eContinueLevel.StandalonBusinessFlow, eContinueFrom.SpecificActivity, mBusinessFlow, (Activity)executedItem);
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
                CalculateFinalStatusForReport();
                //mExecutionIsInProgress = false;
                //SetUIElementsBehaverDuringExecution();
            }
        }

        /// <summary>
        /// Logs the execution details up to the last executed activity.
        /// </summary>
        /// <param name="activityToExecute">The activity to execute.</param>
        private void LogExecutionDetailsUpToLastExecutedActivity(Activity activityToExecute)
        {
            try
            {
                if (mExecutionEngine.ExecutionLoggerManager.Configuration.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
                {
                    foreach (Activity activity in mBusinessFlow.Activities)
                    {
                        if (activity == activityToExecute)
                        {
                            break;
                        }
                        foreach (Act action in activity.Acts.Cast<Act>())
                        {
                            mExecutionEngine.ExecutionLoggerManager.ActionEnd(0, action);
                        }
                        mExecutionEngine.ExecutionLoggerManager.ActivityEnd(0, activity);
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error while logging previous activity and actions", ex);
            }
        }

        /// <summary>
        /// Logs the execution details of actions and activities up to the last executed action.
        /// </summary>
        /// <param name="parentActivity">The parent activity containing the actions.</param>
        /// <param name="actionToExecute">The action up to which the logging should be performed.</param>
        private void LogExecutionDetailsUpToLastExecutedAction(Activity parentActivity, Act actionToExecute)
        {
            try
            {
                if (mExecutionEngine.ExecutionLoggerManager.Configuration.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
                {
                    bool reachedCurrentAction = false;
                    foreach (Activity activity in mBusinessFlow.Activities)
                    {
                        foreach (Act action in activity.Acts.Cast<Act>())
                        {
                            if (activity == parentActivity && action == actionToExecute)
                            {
                                reachedCurrentAction = true;
                                break;
                            }
                            mExecutionEngine.ExecutionLoggerManager.ActionEnd(0, action);
                        }
                        if (reachedCurrentAction)
                        {
                            break;
                        }
                        mExecutionEngine.ExecutionLoggerManager.ActivityEnd(0, activity);
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error while logging previous activity and actions", ex);
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
                    {
                        mAddActionMainPage.ResetAddActionPages();
                    }

                    return;
                }

                UpdateToNewSolution();
            }
        }

        private void DoCleanUp()
        {
            mExecutionEngine.ClearAgents();
            ClearBinding();
        }

        private void ClearBinding()
        {
            App.AutomateBusinessFlowEvent -= App_AutomateBusinessFlowEventAsync;
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

            if (GingerCoreNET.GeneralLib.General.CreateDefaultEnvironment())
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
            if (mBusinessFlow != null)
            {
                mBusinessFlow.Environment = env?.Name.ToString();
            }
            if (env != null)
            {
                mExecutionEngine.GingerRunner.UseSpecificEnvironment = true;
                mExecutionEngine.GingerRunner.ProjEnvironment = env;
                mExecutionEngine.GingerRunner.SpecificEnvironmentName = env.Name;
                WorkSpace.Instance.UserProfile.RecentEnvironment = mEnvironment.Guid;
            }
            else
            {
                mExecutionEngine.GingerRunner.UseSpecificEnvironment = false;
                mExecutionEngine.GingerRunner.ProjEnvironment = null;
                mExecutionEngine.GingerRunner.SpecificEnvironmentName = string.Empty;
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

        private async void xSaveBusinessFlowBtn_Click(object sender, RoutedEventArgs e)
        {
            //warn in case dynamic shared repository Activities are included and going to be deleted
            if (mBusinessFlow.Activities.Any(x => x.AddDynamicly))
            {
                if (Reporter.ToUser(eUserMsgKey.WarnOnDynamicActivities) == Amdocs.Ginger.Common.eUserMsgSelection.No)
                {
                    return;
                }
            }
            var dirtyLinkedActivities = mBusinessFlow.Activities.Where(x => x.IsLinkedItem && x.EnableEdit);
            if (dirtyLinkedActivities.Any())
            {
                foreach (Activity dirtyLinkedActivity in dirtyLinkedActivities)
                {
                    Reporter.ToStatus(eStatusMsgKey.SaveItem, null, dirtyLinkedActivity.ActivityName,
                                    "Linked " + GingerDicser.GetTermResValue(eTermResKey.Activity));
                    SwapLoadingPrefixText("Saving", false);

                    await SharedRepositoryOperations.SaveLinkedActivity(dirtyLinkedActivity, mBusinessFlow.Guid.ToString());
                }
            }

            try
            {

                Reporter.ToStatus(eStatusMsgKey.SaveItem, null, mBusinessFlow.Name,
                                      GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                SwapLoadingPrefixText("Saving", false);

                await WorkSpace.Instance.SolutionRepository.SaveRepositoryItemAsync(mBusinessFlow).ConfigureAwait(false);
            }
            finally
            {
                Reporter.HideStatusMessage();
                SwapLoadingPrefixText("Saving", true);
            }
        }

        public string loadingText { get; set; } = "Loading [BusinessFlow]...";

        void SwapLoadingPrefixText(string swappedText, bool IsReset)
        {
            Dispatcher.Invoke(() =>
            {
                if (string.IsNullOrEmpty(swappedText))
                {
                    return;
                }

                if (IsReset)
                {
                    loadingText = loadingText.Replace(swappedText, "Loading");
                    xItemsTabsSection.Visibility = Visibility.Visible;
                    xItemsLoadingPnl.Visibility = Visibility.Collapsed;
                }
                else
                {
                    loadingText = loadingText.Replace("Loading", swappedText);
                    xItemsTabsSection.Visibility = Visibility.Collapsed;
                    xItemsLoadingPnl.Visibility = Visibility.Visible;
                }

                xLoadWindowText.Text = loadingText;
                ToggleProcessButtons(IsReset);
            });
        }

        private async void xUndoChangesBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            if (mBusinessFlow.IsLocalBackupExist == false)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, string.Format("Backup not created or still in progress for '{0}'", mBusinessFlow.ItemName));
                return;
            }

            if (Reporter.ToUser(eUserMsgKey.AskIfToUndoItemChanges, mBusinessFlow.ItemName) == eUserMsgSelection.Yes)
            {
                try
                {
                    SwapLoadingPrefixText("Undoing", false);
                    Reporter.ToStatus(eStatusMsgKey.StaticStatusProcess, null, string.Format("Undoing changes for '{0}'...", mBusinessFlow.ItemName));
                    await Task.Run(() =>
                    {
                        try
                        {
                            mBusinessFlow.RestoreFromBackup(true, true);
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Failed to Restore backup", ex);
                        }
                    });

                    mActivitiesPage.ListView.UpdateGrouping();
                    mBusinessFlow.SaveBackup();
                }
                catch (Exception ex)
                {
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, string.Format("Failed to undo changes to the item '{0}', please view log for more details", mBusinessFlow.ItemName));
                    Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to undo changes to the item '{0}'", mBusinessFlow.ItemName), ex);
                }
                finally
                {
                    Reporter.HideStatusMessage();
                    SwapLoadingPrefixText("Undoing", true);
                }
            }
        }

        
        private void xSearchBtn_Click(object sender, RoutedEventArgs e)
        {
            FindAndReplacePage mfindAndReplacePageAutomate = new FindAndReplacePage(FindAndReplacePage.eContext.AutomatePage, mBusinessFlow);
            mfindAndReplacePageAutomate.ShowAsWindow();
        }

        private void xAnalyzeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            AnalyzerPage AP = new AnalyzerPage();
            AP.Init(mBusinessFlow, solution: null, mExecutionEngine.GingerRunner.ApplicationAgents);
            AP.ShowAsWindow();
        }

        private void xAutomationRunnerConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            GingerRunnerConfigurationsPage runnerConfigurationsPage = new GingerRunnerConfigurationsPage(mExecutionEngine, GingerRunnerConfigurationsPage.ePageViewMode.AutomatePage, mContext);
            runnerConfigurationsPage.ShowAsWindow();
        }

        private void xResetFlowBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            mExecutionEngine.ExecutionLoggerManager.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.Reset;
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
            if (xBusinessFlowItemComboBox.SelectedItem == GingerDicser.GetTermResValue(eTermResKey.Activities))
            {
                xItemsTabs.SelectedItem = xBfActiVitiesTab;
            }
            else if (xBusinessFlowItemComboBox.SelectedItem == GingerDicser.GetTermResValue(eTermResKey.Variables))
            {
                xItemsTabs.SelectedItem = xBfVariablesTab;
            }
            else if (xBusinessFlowItemComboBox.SelectedItem == "Details")
            {
                xItemsTabs.SelectedItem = xBfConfigurationsTab;
            }
            else
            {
                xItemsTabs.SelectedItem = xBfActiVitiesTab;
            }
        }

        private void xLegacyActionsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            ObservableList<BusinessFlow> lst = [mBusinessFlow];
            WizardWindow.ShowWizard(new ActionsConversionWizard(ActionsConversionWizard.eActionConversionType.SingleBusinessFlow, mContext, lst), 900, 700, true);
        }

        private async void xLegacyActionsRemoveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            ObservableList<BusinessFlowToConvert> lstBFToConvert = [];

            BusinessFlowToConvert flowToConvert = new BusinessFlowToConvert
            {
                BusinessFlow = mBusinessFlow
            };
            lstBFToConvert.Add(flowToConvert);

            ActionConversionUtils utils = new ActionConversionUtils();

            await Task.Run(() =>
            {
                try
                {
                    utils.RemoveLegacyActionsHandler(lstBFToConvert);
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to Remove Legacy Handler", ex);
                }
            });
        }

        private void xRefreshFromAlmMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

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
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

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
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            if (string.IsNullOrEmpty(mBusinessFlow.ExternalID))
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, string.Format("{0} is not mapped to any ALM.", GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)));
                return;
            }

            ObservableList<BusinessFlow> bfs = [mBusinessFlow];
            if (ExportResultsToALMConfigPage.Instance.Init(bfs, new GingerCore.ValueExpression(mEnvironment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>(), false, "", false)))
            {
                ExportResultsToALMConfigPage.Instance.ShowAsWindow();
            }
        }

        private void xReportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            GenerateReport();
        }

        private void GenerateReport()
        {
            if (!WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>().Any(htmlRC => htmlRC.IsDefault))
            {
                Reporter.ToLog(eLogLevel.ERROR, "No Default HTML Report template available to generate report. Please set a default template in Configurations -> Reports -> Reports Template.");
                return;
            }
            if (mExecutionEngine.ExecutionLoggerManager.Configuration.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
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
            HTMLReportsConfiguration currentConf = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.FirstOrDefault(x => (x.IsSelected == true));
            //create the execution logger files            
            string exec_folder = mExecutionEngine.ExecutionLoggerManager.executionLoggerHelper.GetLoggerDirectory(Path.Combine(_selectedExecutionLoggerConfiguration.CalculatedLoggerFolder, Ginger.Run.ExecutionLoggerManager.defaultAutomationTabOfflineLogName));

            if (Directory.Exists(exec_folder))
            {
                GingerCore.General.ClearDirectoryContent(exec_folder);
            }
            else
            {
                Directory.CreateDirectory(exec_folder);
            }

            if (mExecutionEngine.ExecutionLoggerManager.OfflineBusinessFlowExecutionLog(mBusinessFlow, exec_folder))
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
                                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = reportsResultFolder, UseShellExecute = true });
                                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = reportsResultFolder + "\\" + fileName, UseShellExecute = true });
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
                webReporterRunner.RunNewHtmlReport(string.Empty, selectedGuid.ToString());
            }
        }

        private void ShowExecutionSummaryPage()
        {
            ExecutionSummaryPage w = new ExecutionSummaryPage(mContext);
            w.ShowAsWindow();
        }

        private void CalculateFinalStatusForReport()
        {
            try
            {
                if (mExecutionEngine.ExecutionLoggerManager.Configuration.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
                {
                    ((ExecutionLogger)mExecutionEngine.ExecutionLoggerManager.mExecutionLogger).RunSetUpdate(mRunSetLiteDbId, mRunnerLiteDbId, mExecutionEngine);
                }

                foreach (Activity activity in mContext.BusinessFlow.Activities)
                {
                    mExecutionEngine.CalculateActivityFinalStatus(activity);
                }
                mExecutionEngine.CalculateBusinessFlowFinalStatus(mContext.BusinessFlow);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Error while CalculateFinalStatusForReport", ex);
            }
        }

        private void xSummaryPageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            ShowExecutionSummaryPage();
        }

        private void xAutoAnalyzeConfigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WorkSpace.Instance.UserProfile.AutoRunAutomatePageAnalyzer = !WorkSpace.Instance.UserProfile.AutoRunAutomatePageAnalyzer;
        }

        private void xAutoReportConfigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WorkSpace.Instance.UserProfile.AutoGenerateAutomatePageReport = !WorkSpace.Instance.UserProfile.AutoGenerateAutomatePageReport;
        }

        private void xContinueRunsetBtn_Click(object sender, RoutedEventArgs e)
        {
            ContinueRunFromAutomatePage(eContinueFrom.LastStoppedAction);
        }

        private void xBDDGenerateScenarioMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            if (mBusinessFlow != null)
            {
                try
                {
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                    ScenariosGenerator SG = new ScenariosGenerator();
                    SG.CreateScenarios(mBusinessFlow);
                    int cnt = mBusinessFlow.ActivitiesGroups.Count;
                    int optCount = mBusinessFlow.ActivitiesGroups.Count(z => z.Name.StartsWith("Optimized Activities"));
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
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            ScenariosGenerator SG = new ScenariosGenerator();
            SG.ClearOptimizedScenariosVariables(mBusinessFlow);
            SG.ClearGeneretedActivites(mBusinessFlow);
        }

        private void xBDDOpenFeatureFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            DocumentEditorPage documentEditorPage = new DocumentEditorPage(mExecutionEngine.CurrentBusinessFlow.ExternalID.Replace("~", WorkSpace.Instance.Solution.Folder), true)
            {
                Title = "Gherkin Page",
                Height = 700,
                Width = 1000
            };
            documentEditorPage.ShowAsWindow();
        }

        private void xLastItemReportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            GenerateLastExecutedItemReport();
        }

        private void GenerateLastExecutedItemReport()
        {
            if (mExecutionEngine.ExecutionLoggerManager.Configuration.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
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
            HTMLReportsConfiguration currentConf = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.FirstOrDefault(x => (x.IsSelected == true));
            //get logger files
            string exec_folder = mExecutionEngine.ExecutionLoggerManager.executionLoggerHelper.GetLoggerDirectory(Path.Combine(_selectedExecutionLoggerConfiguration.CalculatedLoggerFolder, Ginger.Run.ExecutionLoggerManager.defaultAutomationTabLogName));
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
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = reportsResultFolder, UseShellExecute = true });
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = reportsResultFolder + "\\" + fileName, UseShellExecute = true });
                    }
                }
            }
        }
        private void xTimelineReportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            GingerRunnerTimeLine gingerRunnerTimeLine = (GingerRunnerTimeLine)mExecutionEngine.RunListeners.FirstOrDefault(x => x.GetType() == typeof(GingerRunnerTimeLine));
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
            ((ucButton)sender).ButtonImageForground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
        }

        private void RunBtn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ((ucButton)sender).ButtonImageForground = (SolidColorBrush)FindResource("$HighlightColor_LightBlue");
        }

        private void xExportToCSVMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Ginger.Export.GingerToCSV.BrowseForFilename();
            Ginger.Export.GingerToCSV.BusinessFlowToCSV(mBusinessFlow);
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (mRunSetReport != null)//Workaround for LiteDB report issue
            {
                ExecutionLoggerManager.RunSetReport = mRunSetReport;
            }

            //Help Layouts            
            App.MainWindow.AddHelpLayoutToShow("AutomatePage_BusinessFlowLayerHelp", xBusinessFlowItemComboBox, string.Format("Select here which layer of {0} you want to configure: {1}, {2} or Details", GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), GingerDicser.GetTermResValue(eTermResKey.Activities), GingerDicser.GetTermResValue(eTermResKey.Variables)));
            App.MainWindow.AddHelpLayoutToShow("AutomatePage_AppsAgentsMappingHelp", xAppsAgentsMappingFrame, "Here you should match between the Application and the Agent which will be used for communicating and automating it");
            App.MainWindow.AddHelpLayoutToShow("AutomatePage_EnvironmentSelectionHelp", xEnvironmentComboBox, "Environments should be used for storing environment level parameters, DB connection details and more, go to “Resources-> Environments” to configure all environments you need and select here which environment data to use in execution time");
            App.MainWindow.AddHelpLayoutToShow("AutomatePage_AddActionsBtnHelp", xAddActionsBtn, "Click here to view all options to add automation Actions into your flow");
        }

        private void xSelfHealingConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            GingerSelfHealingConfiguration selfHealingConfiguration = new GingerSelfHealingConfiguration();
            selfHealingConfiguration.ShowAsWindow();
        }

        private void xMapToAlmMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            if (ALMIntegration.Instance.MapBusinessFlowToALM(mBusinessFlow))
            {
                if (Reporter.ToUser(eUserMsgKey.AskIfToSaveBFAfterExport, mBusinessFlow.Name) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                {
                    Reporter.ToStatus(eStatusMsgKey.SaveItem, null, mBusinessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mBusinessFlow);
                    Reporter.HideStatusMessage();
                }
            }
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