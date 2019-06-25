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
using Amdocs.Ginger.Common.Repository;
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
using Ginger.Reports;
using Ginger.Run;
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
using GingerWPF.GeneralLib;
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

namespace GingerWPF.BusinessFlowsLib
{
    /// <summary>
    /// Interaction logic for BusinessFlowPage.xaml
    /// </summary>
    public partial class NewAutomatePage : Page, INotifyPropertyChanged
    {
        GingerRunner mRunner;
        BusinessFlow mBusinessFlow = null;        
        ProjEnvironment mEnvironment = null;
        Context mContext = new Context();

        ActivitiesListViewPage mBfActivitiesPage;
        VariabelsListViewPage mBfVariabelsPage;
        BusinessFlowConfigurationsPage mBfConfigurationsPage;
        ActivityPage mActivityPage;
        MainAddActionsNavigationPage mMainNavigationPage;

        GridLength mLastAddActionsColumnWidth = new GridLength(350);

        ObjectId runnerLiteDbId;
        ObjectId runSetLiteDbId;

        private bool mAutoRunAnalyzer = true;
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

        private bool mAutoGenerateReport = false;
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

            App.AutomateBusinessFlowEvent += App_AutomateBusinessFlowEvent;
            WorkSpace.Instance.PropertyChanged += WorkSpacePropertyChanged;

            InitAutomatePageRunner();
            UpdateAutomatePageRunner();
            LoadBusinessFlowToAutomate(businessFlow);

            SetUIControls();
        }

        private void SetUIControls()
        {
            xBusinessFlowItemComboBox.Items.Add(GingerDicser.GetTermResValue(eTermResKey.Activities));
            xBusinessFlowItemComboBox.Items.Add(GingerDicser.GetTermResValue(eTermResKey.Variables));
            xBusinessFlowItemComboBox.Items.Add("Configurations");
            xBusinessFlowItemComboBox.SelectedIndex = 0;

            BindingHandler.ObjFieldBinding(xAutoAnalyzeConfigMenuItemIcon, ImageMakerControl.ImageTypeProperty, this, nameof(AutoRunAnalyzer), bindingConvertor: new ActiveImageTypeConverter(), BindingMode.OneWay);
            BindingHandler.ObjFieldBinding(xAutoReportConfigMenuItemIcon, ImageMakerControl.ImageTypeProperty, this, nameof(AutoGenerateReport), bindingConvertor: new ActiveImageTypeConverter(), BindingMode.OneWay);

            xAppsAgentsMappingFrame.Content = new ApplicationAgentsMapPage(mRunner, mContext);
            BindEnvsCombo();
            UpdateContext();
        }

        private void UpdateContext()
        {
            if(mContext != null)
            {
                if (mContext.BusinessFlow.CurrentActivity != null)
                {
                    mContext.BusinessFlow.CurrentActivity.PropertyChanged -= Activity_PropertyChanged;
                    mContext.BusinessFlow.CurrentActivity.PropertyChanged += Activity_PropertyChanged; 
                }

                if (mContext.Agent == null)
                {
                    SetContextAgent(mContext.Agent, mContext.BusinessFlow.CurrentActivity, mContext.Runner, mContext);
                }
            }
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
                SetContextAgent(mContext.Agent, mContext.BusinessFlow.CurrentActivity, mContext.Runner, mContext);
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
                SetContextAgent(((ApplicationAgent)sender).Agent, mContext.BusinessFlow.CurrentActivity, mContext.Runner, mContext);                
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
        /// This method will set the Agent for current activity in Context
        /// </summary>
        private void SetContextAgent(Agent agent, Activity activity, GingerRunner runner, Context context)
        {
            ApplicationAgent appAgent = AgentHelper.GetAppAgent(activity, runner, context);
            if (appAgent != null)
            {
                if (agent != null && mContext.Agent != agent)
                {
                    mContext.Agent = agent;
                }
                else if (mContext.Agent != appAgent.Agent)
                {
                    mContext.Agent = appAgent.Agent;
                }

                string targetApp = Convert.ToString(activity.TargetApplication);
                TargetBase tBase = (from x in mContext.BusinessFlow.TargetApplications where x.ItemName == targetApp select x).FirstOrDefault();
                if (tBase != null)
                {
                    mContext.Target = tBase;
                    mContext.Platform = (from x in WorkSpace.Instance.Solution.ApplicationPlatforms
                                                 where x.AppName == targetApp
                                                 select x.Platform).FirstOrDefault();
                }
                               
                if (mContext.Agent != null)
                {
                    mContext.Agent.PropertyChanged -= Agent_PropertyChanged;
                    mContext.Agent.PropertyChanged += Agent_PropertyChanged;
                }

                appAgent.PropertyChanged -= AppAgent_PropertyChanged;
                appAgent.PropertyChanged += AppAgent_PropertyChanged;
            }
        }


        //private void GingerRunner_GingerRunnerEvent(GingerRunnerEventArgs EventArgs)
        //{
        //    switch (EventArgs.EventType)
        //    {
        //        case GingerRunnerEventArgs.eEventType.ActivityStart:
        //            Activity a = (Activity)EventArgs.Object;
        //            // Just to show we can display progress
        //            this.Dispatcher.Invoke(() =>
        //            {
        //                //StatusLabel.Content = "Running " + a.ActivityName;
        //            });

        //            break;
        //        case GingerRunnerEventArgs.eEventType.ActionEnd:
        //            this.Dispatcher.Invoke(() =>
        //            {
        //                // just quick code to show activity progress..
        //                int c = (from x in mBusinessFlow.Activities where x.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending select x).Count();
        //            });
        //            break;
        //    }
        //}

        private void AddActivityButton_Click(object sender, RoutedEventArgs e)
        {
            List<ActionSelectorItem> actions = new List<ActionSelectorItem>();
            actions.Add(new ActionSelectorItem() { Name = "Add Activity using recording", Action = AddActivity });
            actions.Add(new ActionSelectorItem() { Name = "Add Empty Activity", Action = AddActivity });
            actions.Add(new ActionSelectorItem() { Name = "Add Activity from shared repository", Action = AddActivity });

            ActionSelectorWindow w = new ActionSelectorWindow("What would you like to add?", actions);
            w.Show();
        }

        private void AddActivity()
        {
            Activity activity = new Activity();
            bool b = InputBoxWindow.OpenDialog("Add new Activity", "Activity Name", activity, nameof(Activity.ActivityName));
            if (b)
            {
                mBusinessFlow.Activities.Add(activity);
            }
        }

        private void XAddActionsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (xAddActionsBtn.ButtonImageType == Amdocs.Ginger.Common.Enums.eImageType.Add)
            {
                //Expand
                xAddActionsColumn.Width = mLastAddActionsColumnWidth;
                xAddActionsBtn.ButtonImageType = Amdocs.Ginger.Common.Enums.eImageType.ArrowRight;
                xAddActionsBtn.ToolTip = "Collapse Add Actions Section";
                xAddActionsBtn.ButtonStyle = (Style)FindResource("$AddActionsMenuBtnStyle");
                xAddActionSectionSpliter.IsEnabled = true;                
            }
            else
            {
                //Collapse
                mLastAddActionsColumnWidth = xAddActionsColumn.Width;
                xAddActionsColumn.Width = new GridLength(10);
                xAddActionsBtn.ButtonImageType = Amdocs.Ginger.Common.Enums.eImageType.Add;
                xAddActionsBtn.ToolTip = "Add Actions";
                xAddActionsBtn.ButtonStyle = (Style)FindResource("$AddActionsMenuBtnStyle");
                xAddActionSectionSpliter.IsEnabled = false;
            }
        }

        private void InitAutomatePageRunner()
        {
            mRunner = new GingerRunner(eExecutedFrom.Automation);
            mRunner.ExecutionLoggerManager.Configuration = WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();

            // Add Listener so we can do GiveUserFeedback            
            AutomatePageRunnerListener automatePageRunnerListener = new AutomatePageRunnerListener();
            automatePageRunnerListener.AutomatePageRunnerListenerGiveUserFeedback = GiveUserFeedback;
            mRunner.RunListeners.Add(automatePageRunnerListener);

            mContext.Runner = mRunner;
        }

        private void LoadBusinessFlowToAutomate(BusinessFlow businessFlowToLoad)
        {
            if (mBusinessFlow != businessFlowToLoad)
            {
                RemoveCurrentBusinessFlow();
                mBusinessFlow = businessFlowToLoad;
                mContext.BusinessFlow = mBusinessFlow;

                mRunner.BusinessFlows.Add(mBusinessFlow);
                mRunner.CurrentBusinessFlow = mBusinessFlow;
                UpdateApplicationsAgentsMapping();

                mContext.Activity = mBusinessFlow.CurrentActivity;
                if (businessFlowToLoad != null)
                {
                    mBusinessFlow.SaveBackup();
                    mBusinessFlow.PropertyChanged += mBusinessFlow_PropertyChanged;

                    BindingHandler.ObjFieldBinding(xBusinessFlowNameTxtBlock, TextBlock.TextProperty, mBusinessFlow, nameof(BusinessFlow.Name));
                    xBusinessFlowNameTxtBlock.ToolTip = System.IO.Path.Combine(mBusinessFlow.ContainingFolder, mBusinessFlow.Name);

                    mBusinessFlow.AttachActivitiesGroupsAndActivities();
                    if (mBfActivitiesPage == null)
                    {
                        mBfActivitiesPage = new ActivitiesListViewPage(mBusinessFlow, mContext, Ginger.General.eRIPageViewMode.Automation);
                        //mBfActivitiesPage.ListView.ListTitleVisibility = Visibility.Collapsed;
                        mBfActivitiesPage.ListView.List.SelectionChanged += ActivitiesList_SelectionChanged;
                        xActivitiesListFrame.Content = mBfActivitiesPage;
                    }
                    else
                    {
                        mBfActivitiesPage.UpdateBusinessFlow(mBusinessFlow);
                    }
                    //mBusinessFlow.Activities.CollectionChanged += BfActivities_CollectionChanged;
                    //UpdateBfActivitiesTabHeader();


                    if (mBfVariabelsPage == null)
                    {
                        mBfVariabelsPage = new VariabelsListViewPage(mBusinessFlow, mContext, Ginger.General.eRIPageViewMode.Automation);
                        //mBfVariabelsPage.ListView.ListTitleVisibility = Visibility.Collapsed;
                        xBfVariablesTabFrame.Content = mBfVariabelsPage;
                    }
                    else
                    {
                        mBfVariabelsPage.UpdateParent(mBusinessFlow);
                    }
                    //mBusinessFlow.Variables.CollectionChanged += BfVariables_CollectionChanged;
                    //UpdateBfVariabelsTabHeader();

                    if (mBfConfigurationsPage == null)
                    {
                        mBfConfigurationsPage = new BusinessFlowConfigurationsPage(mBusinessFlow, mContext);
                        xBfConfigurationsTabFrame.Content = mBfConfigurationsPage;
                    }
                    else
                    {
                        mBfConfigurationsPage.UpdateBusinessFlow(mBusinessFlow);
                    }

                    if (mBusinessFlow.Activities.Count > 0)
                    {
                        mBusinessFlow.CurrentActivity = mBusinessFlow.Activities[0];
                        if (mContext.Activity == null)
                        {
                            mContext.Activity = mBusinessFlow.CurrentActivity;
                        }
                        //xCurrentActivityFrame.Content = new NewActivityEditPage(mBusinessFlow.CurrentActivity, mContext);  // TODO: use binding? or keep each activity page                        
                    }
                    SetActivityEditPage();


                    SetBusinessFlowTargetAppIfNeeded();
                    mBusinessFlow.TargetApplications.CollectionChanged += mBusinessFlowTargetApplications_CollectionChanged;

                    UpdateRunnerAgentsUsedBusinessFlow();
                    if (mMainNavigationPage == null)
                    {
                        mMainNavigationPage = new MainAddActionsNavigationPage(mContext); 
                    }
                    xAddActionMenuFrame.Content = mMainNavigationPage;
                }
            }
        }

        private void ActivitiesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mBusinessFlow.CurrentActivity = (Activity)mBfActivitiesPage.ListView.CurrentItem;
            mContext.Activity = mBusinessFlow.CurrentActivity;            
            SetActivityEditPage();
        }

        //private void BfVariables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    UpdateBfVariabelsTabHeader();
        //}

        //private void BfActivities_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    UpdateBfActivitiesTabHeader();
        //}

        //private void UpdateBfVariabelsTabHeader()
        //{
        //    this.Dispatcher.Invoke(() =>
        //    {
        //        xBfVariablesTabHeaderText.Text = string.Format("{0} ({1})", GingerDicser.GetTermResValue(eTermResKey.Variables), mBusinessFlow.Variables.Count);
        //    });
        //}
        //private void UpdateBfActivitiesTabHeader()
        //{
        //    this.Dispatcher.Invoke(() =>
        //    {
        //        xBfActiVitiesTabHeaderText.Text = string.Format("{0} ({1})", GingerDicser.GetTermResValue(eTermResKey.Activities), mBusinessFlow.Activities.Count);
        //    });
        //}

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

        //private void SetActivityEditPage()
        //{
        //    if (mBusinessFlow.CurrentActivity != null)
        //    {
        //        //mBusinessFlow.Activities.CurrentItem = mBusinessFlow.Activities[0];
        //        //mBusinessFlow.CurrentActivity = mBusinessFlow.Activities[0];

        //        if (mActivityPage == null)
        //        {
        //            mActivityPage = new ActivityPage(mBusinessFlow.CurrentActivity, mContext, Ginger.General.RepositoryItemPageViewMode.Automation);
        //            xCurrentActivityFrame.Content = mActivityPage;
        //        }
        //        else
        //        {
        //            mActivityPage.UpdateActivity(mBusinessFlow.CurrentActivity);
        //        }
        //    }
        //    else
        //    {
        //        xCurrentActivityFrame.Content = null;
        //    }           
        //}

        private void SetActivityEditPage()
        {
            if (mBusinessFlow != null && mBusinessFlow.CurrentActivity != null)
            {
                if (mActivityPage == null)
                {
                    mActivityPage = new ActivityPage(mBusinessFlow.CurrentActivity, mContext, Ginger.General.eRIPageViewMode.Automation);
                }
                else
                {
                    mActivityPage.UpdateActivity(mBusinessFlow.CurrentActivity);
                }
            }
            else
            {
                mActivityPage = null;
            }
            xCurrentActivityFrame.Content = mActivityPage;
        }

        private void mBusinessFlow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if (e.PropertyName == nameof(BusinessFlow.CurrentActivity))
            //{
            //    SetActivityEditPage();
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
            StopAutomateRun();
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

        public void StopAutomateRun()
        {
            try
            {
                mRunner.StopRun();
            }
            finally
            {
                //EnableDisableAutomateTabGrids(true);
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
                    RemoveCurrentBusinessFlow();
                    break;
                case AutomateEventArgs.eEventType.UpdateAppAgentsMapping:
                    UpdateApplicationsAgentsMapping();
                    break;
                case AutomateEventArgs.eEventType.SetupRunnerForExecution:
                    UpdateAutomatePageRunner();
                    break;
                case AutomateEventArgs.eEventType.RunCurrentAction:
                    RunAutomatePageAction(false);
                    break;
                case AutomateEventArgs.eEventType.RunCurrentActivity:
                    RunAutomatePageActivity();
                    break;
                case AutomateEventArgs.eEventType.ContinueActionRun:
                    ContinueRunFromAutomatePage(eContinueFrom.SpecificAction);
                    break;
                case AutomateEventArgs.eEventType.ContinueActivityRun:
                    ContinueRunFromAutomatePage(eContinueFrom.SpecificActivity);
                    break;
                case AutomateEventArgs.eEventType.StopRun:
                    StopAutomateRun();
                    break;
                case AutomateEventArgs.eEventType.GenerateLastExecutedItemReport:
                    //GenerateLastExecutedItemReport();
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
            mRunner.ProjEnvironment = mEnvironment;
            mRunner.SolutionAgents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>();
            mRunner.SolutionApplications = WorkSpace.Instance.Solution.ApplicationPlatforms;
            mRunner.DSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
            mRunner.ExecutionLoggerManager.ExecutionLogfolder = string.Empty;
            mRunner.ExecutionLoggerManager.Configuration = WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
        }

        public async Task RunAutomatePageAction(bool checkIfActionAllowedToRun = true)
        {
            if (mBusinessFlow.CurrentActivity.Acts.Count() == 0)
            {
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "No Action to Run.");
                return;
            }

            UpdateAutomatePageRunner();//why each time?

            // If no action selected move to the first.
            if (mBusinessFlow.CurrentActivity.Acts.CurrentItem == null && mBusinessFlow.CurrentActivity.Acts.Count() > 0)
            {
                mBusinessFlow.CurrentActivity.Acts.CurrentItem = mBusinessFlow.CurrentActivity.Acts[0];
            }

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

            mRunner.ExecutionLoggerManager.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.ActionRun;

            var result = await mRunner.RunActionAsync((Act)mBusinessFlow.CurrentActivity.Acts.CurrentItem, checkIfActionAllowedToRun, true).ConfigureAwait(false);

            if (mRunner.CurrentBusinessFlow.CurrentActivity.CurrentAgent != null)
            {
                ((Agent)mRunner.CurrentBusinessFlow.CurrentActivity.CurrentAgent).IsFailedToStart = false;
            }
        }

        public async Task RunAutomatePageActivity()
        {
            await mRunner.RunActivityAsync((Activity)mBusinessFlow.CurrentActivity, false).ConfigureAwait(false);

            //When running Runactivity as standalone from GUI, SetActionSkipStatus is not called. Handling it here for now.
            foreach (Act act in mBusinessFlow.CurrentActivity.Acts)
            {
                if (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending)
                {
                    act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                }
            }
        }

        private async Task ContinueRunFromAutomatePage(eContinueFrom continueFrom)
        {
            try
            {
                mRunner.ExecutionLoggerManager.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.ContinueRun;
                switch (continueFrom)
                {
                    case eContinueFrom.LastStoppedAction:
                        await mRunner.ContinueRunAsync(eContinueLevel.StandalonBusinessFlow, eContinueFrom.LastStoppedAction);
                        break;
                    case eContinueFrom.SpecificAction:
                        await mRunner.ContinueRunAsync(eContinueLevel.StandalonBusinessFlow, eContinueFrom.SpecificAction, mBusinessFlow, (Activity)mBusinessFlow.CurrentActivity, (Act)mBusinessFlow.CurrentActivity.Acts.CurrentItem);
                        break;
                    case eContinueFrom.SpecificActivity:
                        await mRunner.ContinueRunAsync(eContinueLevel.StandalonBusinessFlow, eContinueFrom.SpecificActivity, mBusinessFlow, (Activity)mBusinessFlow.CurrentActivity);
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
            }
        }

        private void WorkSpacePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WorkSpace.Solution))
            {
                xEnvironmentComboBox.ItemsSource = null;

                if (WorkSpace.Instance.Solution == null)
                {
                    DoCleanUp();
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
            //if (mReposiotryPage != null)
            //{
            //    mReposiotryPage.RefreshCurrentRepo();
            //}

            BindEnvsCombo();
            UpdateAutomatePageRunner();
        }

        private void BindEnvsCombo()
        {
            xEnvironmentComboBox.ItemsSource = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().AsCollectionViewOrderBy(nameof(ProjEnvironment.Name));
            xEnvironmentComboBox.DisplayMemberPath = nameof(ProjEnvironment.Name);
            xEnvironmentComboBox.SelectedValuePath = nameof(ProjEnvironment.Guid);

            if (WorkSpace.Instance.Solution != null)
            {
                //select last used environment
                if (xEnvironmentComboBox.Items != null && xEnvironmentComboBox.Items.Count > 0)
                {
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

            //move to top after bind
            if (xEnvironmentComboBox.Items.Count == 0)
            {
                CreateDefaultEnvironment();
                xEnvironmentComboBox.SelectedItem = xEnvironmentComboBox.Items[0];
            }

            SetRunnerSpecificEnv((ProjEnvironment)xEnvironmentComboBox.SelectedItem);
        }

        private void SetRunnerSpecificEnv(ProjEnvironment env)
        {
            if (xEnvironmentComboBox.SelectedItem != null)
            {
                mRunner.UseSpecificEnvironment = true;
                mRunner.ProjEnvironment = env;
                mRunner.SpecificEnvironmentName = env.Name;
            }
            else
            {
                mRunner.UseSpecificEnvironment = false;
                mRunner.ProjEnvironment = null;
                mRunner.SpecificEnvironmentName = string.Empty;
            }
        }

        public static void CreateDefaultEnvironment()
        {
            ObservableList<ProjEnvironment> environments = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>();
            if (environments.Count == 0)
            {
                ProjEnvironment newEnv = new ProjEnvironment() { Name = "Default" };

                // Add all solution target app
                foreach (ApplicationPlatform AP in WorkSpace.Instance.Solution.ApplicationPlatforms)
                {
                    EnvApplication EA = new EnvApplication();
                    EA.Name = AP.AppName;
                    EA.CoreProductName = AP.Core;
                    EA.CoreVersion = AP.CoreVersion;
                    EA.Active = true;
                    newEnv.Applications.Add(EA);
                }
                WorkSpace.Instance.SolutionRepository.AddRepositoryItem(newEnv);
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

        private void xEnvironmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xEnvironmentComboBox != null && xEnvironmentComboBox.SelectedItem != null)
            {
                UpdateUsedEnvironment((ProjEnvironment)xEnvironmentComboBox.SelectedItem);
            }
            else
            {
                UpdateUsedEnvironment(null);
            }
        }

        private void UpdateUsedEnvironment(ProjEnvironment env)
        {
            mEnvironment = env;
            mContext.Environment = mEnvironment;
            SetRunnerSpecificEnv(mEnvironment);
            if (mEnvironment != null)
            {
                WorkSpace.Instance.UserProfile.RecentEnvironment = mEnvironment.Guid;
            }
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
            if (mBusinessFlow != null && Reporter.ToUser(eUserMsgKey.AskIfSureWantToUndoChange) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                Reporter.ToStatus(eStatusMsgKey.UndoChanges, null, mBusinessFlow.Name);
                mBusinessFlow.RestoreFromBackup();
                mBusinessFlow.SaveBackup();
                Reporter.HideStatusMessage();
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
            AnalyzerPage AP = new AnalyzerPage();
            AP.Init(WorkSpace.Instance.Solution, mBusinessFlow);
            AP.ShowAsWindow();
        }

        private void xAutomationRunnerConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            GingerRunnerConfigurationsPage runnerConfigurationsPage = new GingerRunnerConfigurationsPage(mRunner, GingerRunnerConfigurationsPage.ePageViewMode.AutomatePage, mContext);
            runnerConfigurationsPage.ShowAsWindow();
        }

        private void xResetFlowBtn_Click(object sender, RoutedEventArgs e)
        {
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
            WizardWindow.ShowWizard(new ActionsConversionWizard(mContext), 900, 700);
        }

        private void xRefreshFromAlmMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (mBusinessFlow != null && mBusinessFlow.ActivitiesGroups != null && mBusinessFlow.ActivitiesGroups.Count > 0)
            {
                ALMIntegration.Instance.RefreshAllGroupsFromALM(mBusinessFlow);
            }
        }

        private void xExportToAlmMenuItem_Click(object sender, RoutedEventArgs e)
        {
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
            ObservableList<BusinessFlow> bfs = new ObservableList<BusinessFlow>();
            bfs.Add(mBusinessFlow);
            ExportResultsToALMConfigPage.Instance.Init(bfs, new GingerCore.ValueExpression(mEnvironment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>(), false, "", false));
            ExportResultsToALMConfigPage.Instance.ShowAsWindow();
        }

        private async Task RunAutomateTabFlow()
        {
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
            try
            {
                //execute preparations
                SetAutomateTabRunnerForExecution();
                mRunner.ResetRunnerExecutionDetails();
                mRunner.ExecutionLoggerManager.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.BussinessFlowRun;

                //execute
                await mRunner.RunBusinessFlowAsync(mBusinessFlow, true, false).ConfigureAwait(false);
                if (WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
                {
                    mRunner.ExecutionLoggerManager.mExecutionLogger.RunSetUpdate(runSetLiteDbId, runnerLiteDbId, mRunner);
                }
                this.Dispatcher.Invoke(() =>
                {                    
                    if (AutoGenerateReport)
                    {
                        GenerateReport();
                    }
                    //else
                    //{
                    //    ShowExecutionSummaryPage();
                    //}
                });

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //enable grids
                //EnableDisableAutomateTabGrids(true);
                mRunner.ResetFailedToStartFlagForAgents();
            }
        }

        public void SetAutomateTabRunnerForExecution()
        {
            mRunner.ProjEnvironment = mEnvironment;
            mRunner.SolutionFolder = WorkSpace.Instance.Solution.Folder;
            mRunner.DSList = new ObservableList<DataSourceBase>(WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>());
            mRunner.SolutionAgents = new ObservableList<Agent>(WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>());
            mRunner.SolutionApplications = WorkSpace.Instance.Solution.ApplicationPlatforms;
        }

        private void xReportMenuItem_Click(object sender, RoutedEventArgs e)
        {
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
            string exec_folder = mRunner.ExecutionLoggerManager.executionLoggerHelper.GetLoggerDirectory(_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationExecResultsFolder + "\\" + Ginger.Run.ExecutionLoggerManager.defaultAutomationTabOfflineLogName);

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
                    Reporter.ToLog(eLogLevel.WARN, "Failed to generate offline full business flow report", ex);
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
            if (runSetLiteDbId != null)
            {
                var selectedGuid = runSetLiteDbId;
                WebReportGenerator webReporterRunner = new WebReportGenerator();
                webReporterRunner.RunNewHtmlReport(selectedGuid.ToString());
            }
        }

        private void xSummaryPageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ExecutionSummaryPage w = new ExecutionSummaryPage(mContext);
            w.ShowAsWindow();
        }

        private void xAutoAnalyzeConfigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AutoRunAnalyzer = !AutoRunAnalyzer;
        }

        private void xAutoReportConfigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AutoGenerateReport = !AutoGenerateReport;
        }

        private async void xContinueRunsetBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mRunner.ExecutionLoggerManager.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.ContinueRun;
                await mRunner.ContinueRunAsync(eContinueLevel.StandalonBusinessFlow, eContinueFrom.LastStoppedAction);
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
