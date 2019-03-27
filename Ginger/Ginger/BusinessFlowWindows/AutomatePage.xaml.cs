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
using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Run;
using Amdocs.Ginger.UserControls;
using Ginger.Actions;
using Ginger.Actions.ActionConversion;
using Ginger.Activities;
using Ginger.Agents;
using Ginger.ALM;
using Ginger.AnalyzerLib;
using Ginger.BusinessFlowFolder;
using Ginger.BusinessFlowWindows;
using Ginger.Extensions;
using Ginger.Functionalities;
using Ginger.GherkinLib;
using Ginger.Reports;
using Ginger.Repository;
using Ginger.Run;
using Ginger.TimeLineLib;
using Ginger.UserControlsLib.TextEditor;
using Ginger.Variables;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.PlugIns;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.Platforms;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ginger
{
    public enum eAutomatePageViewStyles
    {
        Execution, 
        Design,
        Details
    }

    /// <summary>
    /// Interaction logic for AutomatePage.xaml
    /// </summary>
    public partial class AutomatePage : Page 
    {
        GingerRunner mRunner;
        BusinessFlow mBusinessFlow = null;
        Activity mCurrentActivity = null;
        ProjEnvironment mEnvironment = null;
        Context mContext = new Context();
        BusinessFlowPage mCurrentBusPage;
        VariablesPage mVariablesPage;
        ActivitiesGroupsPage mActivitiesGroupsPage;
        ActivitiesPage mActivitiesPage;
        ActivitiesMiniViewPage mActivitiesMiniPage;        
        VariablesPage mActivityVariablesPage;
        public ActionsPage mActionsPage;
        RepositoryPage mReposiotryPage;
                       
        GridLength mlastRepositoryColWidth = new GridLength(300);
        GridLength mlastBFVariablesRowHeight = new GridLength(200, GridUnitType.Star);
        GridLength mlastBFActivitiesGroupsRowHeight = new GridLength(200, GridUnitType.Star);
        GridLength mlastBFActivitiesRowHeight = new GridLength(300, GridUnitType.Star);
        GridLength mlastActivityVariablesRowHeight = new GridLength(200, GridUnitType.Star);
        GridLength mlastActivitiyActionsRowHeight = new GridLength(300, GridUnitType.Star);
        readonly GridLength mMinRowsExpanderSize = new GridLength(35);
        readonly GridLength mMinColsExpanderSize = new GridLength(35);

        

        public AutomatePage(BusinessFlow businessFlow)
        {
            InitializeComponent();

            App.AutomateBusinessFlowEvent += App_AutomateBusinessFlowEvent;
            WorkSpace.Instance.PropertyChanged += WorkSpacePropertyChanged;

            SetAutomateRunner();
            UpdateAutomateRunner();
            LoadBusinessFlowToAutomate(businessFlow);           

            //UI Updates
            btnRunActivity.Label = "Run " + GingerDicser.GetTermResValue(eTermResKey.Activity);
            ContinuteRunActiviy.Header = "Continue Run from Current " + GingerDicser.GetTermResValue(eTermResKey.Activity);
            btnResetStatus.LargeImageSource = ImageMakerControl.GetImageSource(eImageType.Reset, width: 32);
            btnResetStatus.SmallImageSource = ImageMakerControl.GetImageSource(eImageType.Reset, width: 16);
            btnResetFlow.ImageSource = ImageMakerControl.GetImageSource(eImageType.Reset, width: 14);
            btnResetFromCurrentActivity.ImageSource = ImageMakerControl.GetImageSource(eImageType.Reset, width: 14);
            btnResetFromCurrentAction.ImageSource = ImageMakerControl.GetImageSource(eImageType.Reset, width: 14);
            cboSpeed.Text = "0";
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SimulationMode, CheckBox.IsCheckedProperty, mRunner, Ginger.Run.GingerRunner.Fields.RunInSimulationMode);
            AppAgentsMappingExpander2Frame.Content = new ApplicationAgentsMapPage(mContext);
            SetExpanders();
            //Bind between Menu expanders and actual grid expanders
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(BFVariablesExpander, Expander.IsExpandedProperty, BFVariablesExpander2, "IsExpanded");
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(BFActivitiesGroupsExpander, Expander.IsExpandedProperty, BFActivitiesGroupsExpander2, "IsExpanded");
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(BFActivitiesExpander, Expander.IsExpandedProperty, BFActivitiesExpander2, "IsExpanded");
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ActivityVariablesExpander, Expander.IsExpandedProperty, ActivityVariablesExpander2, "IsExpanded");
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ActivityActionsExpander, Expander.IsExpandedProperty, ActivityActionsExpander2, "IsExpanded");
            BFVariablesExpander.IsExpanded = false;
            BFActivitiesGroupsExpander.IsExpanded = false;
            ActivityVariablesExpander.IsExpanded = false;
            mlastBFVariablesRowHeight = new GridLength(200, GridUnitType.Star);
            mlastActivityVariablesRowHeight = new GridLength(200, GridUnitType.Star);
            SetFramesContent();
            App.AutomateBusinessFlowEvent -= App_AutomateBusinessFlowEvent;
            App.AutomateBusinessFlowEvent += App_AutomateBusinessFlowEvent;
            SetGridsView(eAutomatePageViewStyles.Design.ToString());
            SetGherkinOptions();
            BindEnvsCombo();

            GoToBusFlowsListHandler();

            if (mBusinessFlow == null)
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, string.Format("Failed to find {0} to load into Automate page.", GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)));
                return;
            }
        }

        public void GoToBusFlowsListHandler()
        {
            xToBusinessFlowsListBtn.Visibility = Visibility.Visible;
            xToBusinessFlowsListBtn.LargeImageSource = ImageMakerControl.GetImageSource(eImageType.GoBack, width: 32);
            xToBusinessFlowsListBtn.SmallImageSource = ImageMakerControl.GetImageSource(eImageType.GoBack, width: 16);
            xToBusinessFlowsListBtn.Label = GingerDicser.GetTermResValue(eTermResKey.BusinessFlows, "Back to ", " List");
            xToBusinessFlowsListBtn.ToolTip = GingerDicser.GetTermResValue(eTermResKey.BusinessFlows, "Back to ", " List");
            xToBusinessFlowsListBtn.Click += XToBusinessFlowsListBtn_Click; ;
        }

        private void XToBusinessFlowsListBtn_Click(object sender, RoutedEventArgs e)
        {
            App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.ShowBusinessFlowsList, mBusinessFlow);
        }

            

        public void GoToBusFlowsListHandler(RoutedEventHandler clickHandler)
        {
            xToBusinessFlowsListBtn.Visibility = Visibility.Visible;
            xToBusinessFlowsListBtn.LargeImageSource = ImageMakerControl.GetImageSource(eImageType.GoBack, width: 32);
            xToBusinessFlowsListBtn.SmallImageSource = ImageMakerControl.GetImageSource(eImageType.GoBack, width: 16);
            xToBusinessFlowsListBtn.Label = GingerDicser.GetTermResValue(eTermResKey.BusinessFlows, "Back to ", " List");
            xToBusinessFlowsListBtn.ToolTip = GingerDicser.GetTermResValue(eTermResKey.BusinessFlows, "Back to ", " List");
            xToBusinessFlowsListBtn.Click += clickHandler;
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
                    SetAutomateTabRunnerForExecution();
                    break;
                case AutomateEventArgs.eEventType.RunCurrentAction:
                    RunCurrentAutomatePageAction(false);
                    break;
                case AutomateEventArgs.eEventType.RunCurrentActivity:
                    btnGridViewExecution_Click(null, null);
                    RunActivity();
                    break;
                case AutomateEventArgs.eEventType.ContinueActionRun:
                    ContinueRunFRomAutomateTab(eContinueFrom.SpecificAction);
                    break;
                case AutomateEventArgs.eEventType.ContinueActivityRun:
                    ContinueRunFRomAutomateTab(eContinueFrom.SpecificActivity);
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

        private void SetFramesContent()
        {
            mCurrentBusPage = new BusinessFlowPage(mBusinessFlow, true);
            CurrentBusFrame.Content = mCurrentBusPage;
            CurrentBusExpander.IsExpanded = false;

            mVariablesPage = new VariablesPage(eVariablesLevel.BusinessFlow, mBusinessFlow, General.RepositoryItemPageViewMode.Automation);
            mVariablesPage.grdVariables.ShowTitle = System.Windows.Visibility.Collapsed;
            BFVariablesFrame.Content = mVariablesPage;
            mActivitiesGroupsPage = new ActivitiesGroupsPage(mBusinessFlow, General.RepositoryItemPageViewMode.Automation);
            mActivitiesGroupsPage.grdActivitiesGroups.ShowTitle = System.Windows.Visibility.Collapsed;
            BFActivitiesGroupsFrame.Content = mActivitiesGroupsPage;

            mActivitiesPage = new ActivitiesPage(mBusinessFlow, General.RepositoryItemPageViewMode.Automation, mContext);
            mActivitiesPage.grdActivities.ShowTitle = System.Windows.Visibility.Collapsed;
            BFActivitiesFrame.Content = mActivitiesPage;

            mActivityVariablesPage = new VariablesPage(eVariablesLevel.Activity, mBusinessFlow.CurrentActivity, General.RepositoryItemPageViewMode.Automation);
            mActivityVariablesPage.grdVariables.ShowTitle = System.Windows.Visibility.Collapsed;
            ActivityVariablesFrame.Content = mActivityVariablesPage;

            mActionsPage = new ActionsPage(businessFlow:mBusinessFlow, editMode: General.RepositoryItemPageViewMode.Automation, context: mContext);
            mActionsPage.grdActions.ShowTitle = System.Windows.Visibility.Collapsed;
            ActivityActionsFrame.Content = mActionsPage;

            mReposiotryPage = new RepositoryPage(mBusinessFlow);
            RepositoryFrame.Content = mReposiotryPage;
        }

        private void SetExpanders()
        {
            // We do UI changes using the dispatcher since it might trigger from STA like IB
            App.MainWindow.Dispatcher.Invoke(() =>
                    {
                        //set dynamic expanders titles
                        if (mBusinessFlow != null)//??
                        {                            
                            mCurrentBusPage = new BusinessFlowPage(mBusinessFlow, true);
                            CurrentBusFrame.Content = mCurrentBusPage;

                            UpdateMainBFLabel();
                            mBusinessFlow.PropertyChanged -= BusinessFlow_PropertyChanged;
                            mBusinessFlow.PropertyChanged += BusinessFlow_PropertyChanged;

                            UpdateBusinessFlowVariabelsExpanders();
                            mBusinessFlow.Variables.CollectionChanged -= BusinessFlowVariables_CollectionChanged;
                            mBusinessFlow.Variables.CollectionChanged += BusinessFlowVariables_CollectionChanged;
                            if (mVariablesPage != null)
                            {
                                mVariablesPage.UpdateBusinessFlow(mBusinessFlow);
                            }

                            UpdateBusinessFlowActivitiesGroupsExpanders();
                            if (mActivitiesGroupsPage != null)
                            {
                                mActivitiesGroupsPage.UpdateBusinessFlow(mBusinessFlow);
                            }
                            mBusinessFlow.ActivitiesGroups.CollectionChanged -= ActivitiesGroups_CollectionChanged;
                            mBusinessFlow.ActivitiesGroups.CollectionChanged += ActivitiesGroups_CollectionChanged;
                            if (mActivitiesPage != null)
                            {
                                mActivitiesPage.UpdateBusinessFlow(mBusinessFlow);
                            }

                            UpdateBusinessFlowActivitiesExpanders();
                            mBusinessFlow.Activities.CollectionChanged -= Activities_CollectionChanged;
                            mBusinessFlow.Activities.CollectionChanged += Activities_CollectionChanged;
                            mCurrentActivity = (Activity)mBusinessFlow.CurrentActivity;
                            if (mActivitiesMiniPage != null)
                            {
                                mActivitiesMiniPage.UpdateBusinessFlow(mBusinessFlow);
                            }

                            UpdateCurrentActivityVariabelsExpanders();
                            if (mActivityVariablesPage != null)
                            {
                                mActivityVariablesPage.UpdateActivity(mBusinessFlow.CurrentActivity);
                            }

                            UpdateCurrentActivityActionsExpanders();

                            if (mReposiotryPage != null)
                            {
                                mReposiotryPage.UpdateBusinessFlow(mBusinessFlow);
                            }
                        }

                    });
        }

        private void UpdateMainBFLabel()
        {
            App.MainWindow.Dispatcher.Invoke(() =>
            {
                xMainBusinessFlowlabel.Content = String.Format("'{0}' {1}", mBusinessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
            });
        }
        private void UpdateBusinessFlowVariabelsExpanders()
        {
            if (Dispatcher.CheckAccess())
            {
                string label;
                if (mBusinessFlow != null)
                {
                    label= string.Format("{0} ({1})", GingerDicser.GetTermResValue(eTermResKey.Variables), mBusinessFlow.Variables.Count);
                    BusinessFlowVariablesExpanderLabel.Content = label;
                    BusinessFlowVariablesExpander2Label.Content = label;
                }
                else
                {
                    label= string.Format("{0}", GingerDicser.GetTermResValue(eTermResKey.Variables));
                    BusinessFlowVariablesExpanderLabel.Content = label;
                    BusinessFlowVariablesExpander2Label.Content = label;
                }
            }
        }

        private void UpdateBusinessFlowActivitiesGroupsExpanders()
        {
            if (Dispatcher.CheckAccess())
            {
                string label;
                if (mBusinessFlow != null)
                {
                    label = string.Format("{0} ({1})", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups), mBusinessFlow.ActivitiesGroups.Count);
                    BBFActivitiesGroupsExpanderLabel.Content = label;
                    BBFActivitiesGroupsExpander2Label.Content = label;
                }
                else
                {
                    label = string.Format("{0}", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups));
                    BBFActivitiesGroupsExpanderLabel.Content = label;
                    BBFActivitiesGroupsExpander2Label.Content = label;
                }                
            }
        }

        private void UpdateBusinessFlowActivitiesExpanders()
        {
            if (Dispatcher.CheckAccess())
            {
                string label;
                if (mBusinessFlow != null)
                {
                    label = string.Format("{0} ({1})", GingerDicser.GetTermResValue(eTermResKey.Activities), mBusinessFlow.Activities.Count);
                    BBFActivitiesExpanderLabel.Content = label;
                    BBFActivitiesExpander2Label.Content = label;
                }
                else
                {
                    label = string.Format("{0}", GingerDicser.GetTermResValue(eTermResKey.Activities));
                    BBFActivitiesExpanderLabel.Content = label;
                    BBFActivitiesExpander2Label.Content = label;
                }
            }
        }

        private void UpdateCurrentActivityVariabelsExpanders()
        {
            if (Dispatcher.CheckAccess())
            {
                string label;
                if (mBusinessFlow.CurrentActivity != null)
                {
                    label = string.Format("'{0}'- {1} {2} ({3})", mBusinessFlow.CurrentActivity.ActivityName, GingerDicser.GetTermResValue(eTermResKey.Activity), GingerDicser.GetTermResValue(eTermResKey.Variables), mBusinessFlow.CurrentActivity.Variables.Count);
                    ActivityVariablesExpanderLabel.Content = label;
                    ActivityVariablesExpander2Label.Content = label;
                }
                else
                {
                    label = string.Format("{0}- {1}", GingerDicser.GetTermResValue(eTermResKey.Activity), GingerDicser.GetTermResValue(eTermResKey.Variables));
                    ActivityVariablesExpanderLabel.Content = label;
                    ActivityVariablesExpander2Label.Content = label;
                }
            }
        }

        private void UpdateCurrentActivityActionsExpanders()
        {
            if (Dispatcher.CheckAccess())
            {
                string label;
                if (mBusinessFlow.CurrentActivity != null)
                {
                    label = string.Format("'{0}'- {1} Actions ({2})", mBusinessFlow.CurrentActivity.ActivityName, GingerDicser.GetTermResValue(eTermResKey.Activity), mBusinessFlow.CurrentActivity.Acts.Count);
                    ActivityActionsExpanderLabel.Content = label;
                    ActivityActionsExpander2Label.Content = label;
                }
                else
                {
                    label = string.Format("{0}- Actions", GingerDicser.GetTermResValue(eTermResKey.Activity));
                    ActivityActionsExpanderLabel.Content = label;
                    ActivityActionsExpander2Label.Content = label;
                }
                if (mActionsPage != null)
                {
                    mActionsPage.UpdateParentBusinessFlow(mBusinessFlow);
                }
            }
        }

        private void Activities_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateBusinessFlowActivitiesExpanders();
        }

        private void ActivitiesGroups_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateBusinessFlowActivitiesGroupsExpanders();
        }

        private void BusinessFlowVariables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateBusinessFlowVariabelsExpanders();
        }

        private void SetGherkinOptions()
        {
            if (mBusinessFlow != null && mBusinessFlow.Source == BusinessFlow.eSource.Gherkin)
            {
                Gherkin.Visibility = Visibility.Visible;
                GenerateScenario.Visibility = Visibility.Visible;
                CleanScenario.Visibility = Visibility.Visible;
                OpenFeatureFile.Visibility = Visibility.Visible;
            }
            else
            {
                Gherkin.Visibility = Visibility.Collapsed;
                GenerateScenario.Visibility = Visibility.Collapsed;
                CleanScenario.Visibility = Visibility.Collapsed;
                OpenFeatureFile.Visibility = Visibility.Collapsed;
            }
        }

        public void SetGridsView(string viewName)
        {
            if (mActionsPage != null)
            {
                mActionsPage.grdActions.ChangeGridView(viewName);
            }
            if (mActivitiesPage != null)
            {
                mActivitiesPage.grdActivities.ChangeGridView(viewName);
            }
        }

        private void WorkSpacePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WorkSpace.Solution))
            {                
                lstEnvs.ItemsSource = null;

                if (WorkSpace.Instance.Solution == null)
                {
                    DoCleanUp();
                    return;
                }

                UpdateToNewSolution();
            }
        }

        private void SetAutomateRunner()
        {
            mRunner = new GingerRunner(eExecutedFrom.Automation);
            mRunner.ExecutionLogger.Configuration = WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();

            // Add Listener so we can do GiveUserFeedback            
            AutomatePageRunnerListener automatePageRunnerListener = new AutomatePageRunnerListener();
            automatePageRunnerListener.AutomatePageRunnerListenerGiveUserFeedback = GiveUserFeedback;
            mRunner.RunListeners.Add(automatePageRunnerListener);

            mContext.Runner = mRunner;
        }

        private void UpdateAutomateRunner()
        {            
            mRunner.CurrentSolution = WorkSpace.Instance.Solution;
            mRunner.SolutionFolder = WorkSpace.Instance.Solution.Folder;
            mRunner.SolutionAgents = new ObservableList<Agent>();
            mRunner.SolutionApplications = WorkSpace.Instance.Solution.ApplicationPlatforms;
            mRunner.DSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();            
        }

        private void DoCleanUp()
        {
            mRunner.ClearAgents();
        }

        private void UpdateToNewSolution()
        {
            if (mReposiotryPage != null)
            {
                mReposiotryPage.RefreshCurrentRepo();
            }

            BindEnvsCombo();
            UpdateAutomateRunner();
        }
        public void UpdateApplicationsAgentsMapping(bool useAgentsCache = true)
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
                            mBusinessFlow.TargetApplications = new ObservableList<TargetBase>();

                        mBusinessFlow.TargetApplications.Add(new TargetApplication() { AppName = WorkSpace.Instance.Solution.MainApplication });
                    }
                }
            }

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

        private void BindEnvsCombo()
        {
            lstEnvs.ItemsSource = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().AsCollectionViewOrderBy(nameof(ProjEnvironment.Name));
            lstEnvs.DisplayMemberPath = nameof(ProjEnvironment.Name);
            lstEnvs.SelectedValuePath = nameof(ProjEnvironment.Guid);

            if ( WorkSpace.Instance.Solution != null)
            {
                //select last used environment
                if (lstEnvs.Items != null && lstEnvs.Items.Count > 0)
                {
                    if (lstEnvs.Items.Count > 1 &&  WorkSpace.Instance.UserProfile.RecentEnvironment != null &&  WorkSpace.Instance.UserProfile.RecentEnvironment != Guid.Empty)
                    {
                        foreach (object env in lstEnvs.Items)
                        {
                            if (((ProjEnvironment)env).Guid ==  WorkSpace.Instance.UserProfile.RecentEnvironment)
                            {
                                lstEnvs.SelectedIndex = lstEnvs.Items.IndexOf(env);
                                return;
                            }
                        }
                    }

                    //default selection
                    lstEnvs.SelectedIndex = 0;
                }
            }

            //move to top after bind
            if (lstEnvs.Items.Count == 0)
            {
                CreateDefaultEnvironment();
                lstEnvs.SelectedItem = lstEnvs.Items[0];
            }
        }

        public static void CreateDefaultEnvironment()
        {
            ObservableList<ProjEnvironment> environments = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>();
            if (environments.Count == 0)
            {
                ProjEnvironment newEnv = new ProjEnvironment() { Name = "Default" };

                // Add all solution target app
                foreach (ApplicationPlatform AP in  WorkSpace.Instance.Solution.ApplicationPlatforms)
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

        private void CurrentBusExpander_Expanded(object sender, RoutedEventArgs e)
        {
            CurrentBusRow.Height = new GridLength(215, GridUnitType.Star);
        }

        private void CurrentBusExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            CurrentBusRow.Height = new GridLength(35);
        } 

        private void RepositoryExpander_ExpandedCollapsed(object sender, RoutedEventArgs e)
        {
            if (RepositoryExpander.IsExpanded)
            {
                RepositoryExpanderLabel.Visibility = System.Windows.Visibility.Collapsed;
                RepositoryGridColumn.Width = mlastRepositoryColWidth;
            }
            else
            {
                mlastRepositoryColWidth = RepositoryGridColumn.Width;
                RepositoryExpanderLabel.Visibility = System.Windows.Visibility.Visible;
                RepositoryGridColumn.Width = mMinColsExpanderSize;
            }
        }

        private void RepositoryExpander_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (RepositoryExpander.IsExpanded == false && e.NewSize.Width > mMinColsExpanderSize.Value)
            {
                RepositoryGridColumn.Width = mMinColsExpanderSize;
            }
        }

        private void RepositoryFrame_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (RepositoryExpander.IsExpanded && e.NewSize.Width <= 50)
            {
                RepositoryExpander.IsExpanded = false;                
                mlastRepositoryColWidth = new GridLength(300);
                RepositoryGridColumn.Width = mMinColsExpanderSize;
            }
        }
      
        private void BFActivitiesFrame_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height < 70)
            {
                if (mActivitiesMiniPage == null)
                {
                    mActivitiesMiniPage = new ActivitiesMiniViewPage(mBusinessFlow);
                }
                BFActivitiesFrame.Content = mActivitiesMiniPage;
            }
            else
            {
                BFActivitiesFrame.Content = mActivitiesPage;
            }
        }        

        private void Expanders_Changed(object sender, RoutedEventArgs e)
        {
            Expanders_Changed();
        }

        private void Expanders_Changed()
        {
            if (BFVariablesExpander == null || BFActivitiesExpander == null || ActivityVariablesExpander == null || ActivityActionsExpander == null)
            {
                return;
            }

            int rowIndex = 3;   //were dynamic content starts       
            if (BFVariablesExpander != null)
            {
                if (BFVariablesExpander.IsExpanded)
                {
                    if (BFVariablesFrame != null && BFVariablesFrame.ActualHeight != 0)
                    {
                        mlastBFVariablesRowHeight = new GridLength(BFVariablesFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    }
                    BFVariablesExpander.SetValue(Grid.RowProperty, ++rowIndex);
                    PageMainGrid.RowDefinitions[rowIndex].MinHeight = mMinRowsExpanderSize.Value;
                    PageMainGrid.RowDefinitions[rowIndex].Height = mlastBFVariablesRowHeight;
                    BFVariablesExpander.Visibility = System.Windows.Visibility.Visible;
                    BFVariablesExpander2.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    if (BFVariablesFrame != null && BFVariablesFrame.ActualHeight != 0)
                    {
                        mlastBFVariablesRowHeight = new GridLength(BFVariablesFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    }                        
                    BFVariablesExpander.Visibility = System.Windows.Visibility.Collapsed;
                    BFVariablesExpander2.Visibility = System.Windows.Visibility.Visible;
                }
            }

            if (BFActivitiesGroupsExpander != null)
            {
                if (BFActivitiesGroupsExpander.IsExpanded)
                {
                    if (BFActivitiesGroupsFrame != null && BFActivitiesGroupsFrame.ActualHeight != 0)
                    {
                        mlastBFActivitiesGroupsRowHeight = new GridLength(BFActivitiesGroupsFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    }
                    BFActivitiesGroupsExpander.SetValue(Grid.RowProperty, ++rowIndex);
                    PageMainGrid.RowDefinitions[rowIndex].MinHeight = mMinRowsExpanderSize.Value;
                    PageMainGrid.RowDefinitions[rowIndex].Height = mlastBFActivitiesGroupsRowHeight;
                    BFActivitiesGroupsExpander.Visibility = System.Windows.Visibility.Visible;
                    BFActivitiesGroupsExpander2.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    if (BFActivitiesGroupsFrame != null && BFActivitiesGroupsFrame.ActualHeight != 0)
                    {
                        mlastBFActivitiesGroupsRowHeight = new GridLength(BFActivitiesGroupsFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    }
                    BFActivitiesGroupsExpander.Visibility = System.Windows.Visibility.Collapsed;
                    BFActivitiesGroupsExpander2.Visibility = System.Windows.Visibility.Visible;
                }
            }

            if (BFActivitiesExpander != null)
            {
                if (BFActivitiesExpander.IsExpanded)
                {
                    if (BFActivitiesFrame != null && BFActivitiesFrame.ActualHeight != 0)
                    {
                        mlastBFActivitiesRowHeight = new GridLength(BFActivitiesFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    }
                    BFActivitiesExpander.SetValue(Grid.RowProperty, ++rowIndex);
                    PageMainGrid.RowDefinitions[rowIndex].MinHeight = mMinRowsExpanderSize.Value;
                    PageMainGrid.RowDefinitions[rowIndex].Height = mlastBFActivitiesRowHeight;
                    BFActivitiesExpander.Visibility = System.Windows.Visibility.Visible;
                    BFActivitiesExpander2.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    if (BFActivitiesFrame != null && BFActivitiesFrame.ActualHeight != 0)
                    {
                        mlastBFActivitiesRowHeight = new GridLength(BFActivitiesFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    }
                    BFActivitiesExpander.Visibility = System.Windows.Visibility.Collapsed;
                    BFActivitiesExpander2.Visibility = System.Windows.Visibility.Visible;
                }
            }

            if (ActivityVariablesExpander != null)
            {
                if (ActivityVariablesExpander.IsExpanded)
                {
                    if (ActivityVariablesFrame != null && ActivityVariablesFrame.ActualHeight != 0)
                    {
                        mlastActivityVariablesRowHeight = new GridLength(ActivityVariablesFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    }
                    ActivityVariablesExpander.SetValue(Grid.RowProperty, ++rowIndex);
                    PageMainGrid.RowDefinitions[rowIndex].MinHeight = mMinRowsExpanderSize.Value;
                    PageMainGrid.RowDefinitions[rowIndex].Height = mlastActivityVariablesRowHeight;
                    ActivityVariablesExpander.Visibility = System.Windows.Visibility.Visible;
                    ActivityVariablesExpander2.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    if (ActivityVariablesFrame != null && ActivityVariablesFrame.ActualHeight != 0)
                    {
                        mlastActivityVariablesRowHeight = new GridLength(ActivityVariablesFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    }
                    ActivityVariablesExpander.Visibility = System.Windows.Visibility.Collapsed;
                    ActivityVariablesExpander2.Visibility = System.Windows.Visibility.Visible;
                }
            }

            if (ActivityActionsExpander != null)
            {
                if (ActivityActionsExpander.IsExpanded)
                {
                    if (ActivityActionsFrame != null && ActivityActionsFrame.ActualHeight != 0)
                    {
                        mlastActivitiyActionsRowHeight = new GridLength(ActivityActionsFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    }
                    ActivityActionsExpander.SetValue(Grid.RowProperty, ++rowIndex);
                    PageMainGrid.RowDefinitions[rowIndex].MinHeight = mMinRowsExpanderSize.Value;
                    PageMainGrid.RowDefinitions[rowIndex].Height = mlastActivitiyActionsRowHeight;
                    ActivityActionsExpander.Visibility = System.Windows.Visibility.Visible;
                    ActivityActionsExpander2.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    if (ActivityActionsFrame != null && ActivityActionsFrame.ActualHeight != 0)
                    {
                        mlastActivitiyActionsRowHeight = new GridLength(ActivityActionsFrame.ActualHeight + mMinRowsExpanderSize.Value, GridUnitType.Star);
                    }
                    ActivityActionsExpander.Visibility = System.Windows.Visibility.Collapsed;
                    ActivityActionsExpander2.Visibility = System.Windows.Visibility.Visible;
                }
            }

            //hide all unneeded rows
            for (int indx = 8; indx > rowIndex; indx--)
            {
                PageMainGrid.RowDefinitions[indx].MinHeight = 0;
                PageMainGrid.RowDefinitions[indx].Height = new GridLength(0);
            }

            //Disable all unneeded splitters
            List<GridSplitter> gridSplitters = new List<GridSplitter>();
            gridSplitters.Add(Row1Splitter);
            gridSplitters.Add(Row2Splitter);
            gridSplitters.Add(Row3Splitter);
            gridSplitters.Add(Row4Splitter);
            gridSplitters.Add(Row5Splitter);
            for (int indx = 1; indx <= gridSplitters.Count; indx++)
            {
                if (gridSplitters[indx - 1] != null)
                {
                    if (indx < (rowIndex-1))
                    {
                        gridSplitters[indx - 1].IsEnabled = true;
                    }                        
                    else
                    {
                        gridSplitters[indx - 1].IsEnabled = false;
                    }                        
                }
            }

            //arrange expanders menu look 
            if (rowIndex == 8)
            {
                PageMainGrid.RowDefinitions[3].Height = new GridLength(0);
            }
            else
            {
                PageMainGrid.RowDefinitions[3].Height = new GridLength(35);
            }

            //make sure at least one expander open
            if (rowIndex == 1)
            {
                BFActivitiesExpander.IsExpanded = true;
            }
        }

        private void BusinessFlow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (e.PropertyName == nameof(BusinessFlow.CurrentActivity))
                {
                    UpdateCurrentActivityVariabelsExpanders();
                    UpdateCurrentActivityActionsExpanders();
                    if (mCurrentActivity != null)
                    {
                        //removing previous Activity events registration
                        mCurrentActivity.Variables.CollectionChanged -= CurrentActivityVariables_CollectionChanged;
                        mCurrentActivity.Acts.CollectionChanged -= CurrentActivityActions_CollectionChanged;
                        mCurrentActivity.PropertyChanged -= MCurrentActivity_PropertyChanged;
                    }
                    mCurrentActivity =(Activity) mBusinessFlow.CurrentActivity;
                    mContext.Activity = mCurrentActivity;
                    if (mCurrentActivity != null)
                    {
                        //Add current Activity events registration
                        mCurrentActivity.Variables.CollectionChanged += CurrentActivityVariables_CollectionChanged;
                        mCurrentActivity.Acts.CollectionChanged += CurrentActivityActions_CollectionChanged;
                        mCurrentActivity.PropertyChanged += MCurrentActivity_PropertyChanged;
                    }                                  
                }
                else if(e.PropertyName == BusinessFlow.Fields.Name)
                {
                    UpdateMainBFLabel();
                }
            });
        }

        private void MCurrentActivity_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Activity.ActivityName))
            {
                UpdateCurrentActivityVariabelsExpanders();
                UpdateCurrentActivityActionsExpanders();                
            }
        }

        private void CurrentActivityVariables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateCurrentActivityVariabelsExpanders();
        }

        private void CurrentActivityActions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateCurrentActivityActionsExpanders();
        }

        //private void AppPropertychanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == nameof(App.BusinessFlow))
        //    {
        //        StopAutomateRun();
        //        SetExpanders();
        //        SetGherkinOptions();
        //    }
        //}

        private void LoadBusinessFlowToAutomate(BusinessFlow businessFlowToLoad)
        {
            if (mBusinessFlow != businessFlowToLoad)
            {
                RemoveCurrentBusinessFlow();
                mBusinessFlow = businessFlowToLoad;
                mContext.BusinessFlow = mBusinessFlow;
                if (businessFlowToLoad != null)
                {                    
                    mBusinessFlow.SaveBackup();
                    mBusinessFlow.PropertyChanged += mBusinessFlow_PropertyChanged;
                    mBusinessFlow.TargetApplications.CollectionChanged += mBusinessFlowTargetApplications_CollectionChanged;
                    
                    SetExpanders();
                    SetGherkinOptions();
                    if (mBusinessFlow.Activities.Count > 0)
                    {
                        mBusinessFlow.CurrentActivity = mBusinessFlow.Activities[0];
                    }
                    //Set Business Flow on AutomateTabGingerRunner

                    mRunner.BusinessFlows.Add(mBusinessFlow);
                    mRunner.CurrentBusinessFlow = mBusinessFlow;
                    UpdateApplicationsAgentsMapping();
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

        private void mBusinessFlow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BusinessFlow.CurrentActivity) && mActivityVariablesPage != null)
            {
                mActivityVariablesPage.UpdateActivity(mBusinessFlow.CurrentActivity);
            }
        }

        private void SaveBizFlowButton_Click(object sender, RoutedEventArgs e)
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

        private void UndoBizFlowChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (mBusinessFlow != null && Reporter.ToUser(eUserMsgKey.AskIfSureWantToUndoChange) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                Reporter.ToStatus(eStatusMsgKey.UndoChanges, null, mBusinessFlow.Name);
                mBusinessFlow.RestoreFromBackup();
                mBusinessFlow.SaveBackup();
                Reporter.HideStatusMessage();
            }
        }

        private void AutomateAnalyzerButton_Click(object sender, RoutedEventArgs e)
        {
            AutoLogProxy.UserOperationStart("AutomateAnalyzerButton_Click");

            AnalyzerPage AP = new AnalyzerPage();
            AP.Init( WorkSpace.Instance.Solution, mBusinessFlow);
            AP.ShowAsWindow();

            AutoLogProxy.UserOperationEnd();
        }

        private void btnActionConversion_Click(object sender, RoutedEventArgs e)
        {
            AutoLogProxy.UserOperationStart("btnConversionMechanism_Click");
            

            WizardWindow.ShowWizard(new ActionsConversionWizard(mContext), 900, 700);
            AutoLogProxy.UserOperationEnd();
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

        private async Task ResetStatusRunFRomAutomateTab(Run.GingerRunner.eResetStatus resetFrom)
        {
            try
            {
                AutoLogProxy.UserOperationStart("ResetStatusFrom" + resetFrom.ToString() + "_Click",  WorkSpace.Instance.Solution.Name, GetProjEnvironmentName());
                mRunner.ExecutionLogger.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.Reset;
                switch (resetFrom)
                {
                    case Run.GingerRunner.eResetStatus.All:
                        mRunner.ResetStatus(eContinueLevel.StandalonBusinessFlow, Run.GingerRunner.eResetStatus.All, mBusinessFlow);
                        break;
                    case Run.GingerRunner.eResetStatus.FromSpecificActivityOnwards:
                        mRunner.ResetStatus(eContinueLevel.StandalonBusinessFlow, Run.GingerRunner.eResetStatus.FromSpecificActivityOnwards, mBusinessFlow, (Activity)mBusinessFlow.CurrentActivity);
                        break;
                    case Run.GingerRunner.eResetStatus.FromSpecificActionOnwards:
                        mRunner.ResetStatus(eContinueLevel.StandalonBusinessFlow, Run.GingerRunner.eResetStatus.FromSpecificActionOnwards, mBusinessFlow, (Activity)mBusinessFlow.CurrentActivity, (Act)mBusinessFlow.CurrentActivity.Acts.CurrentItem);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                AutoLogProxy.UserOperationEnd();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred on Reset Status Run from Automate Tab", ex);
                throw ex;                
            }
        }

        private string GetProjEnvironmentName()
        {
            if (mEnvironment != null)
                return mEnvironment.Name;
            else
                return null;
        }
        private void FindAndReplaceAutomatePage_Click(object sender, RoutedEventArgs e)
        {
            AutomateFindAndReplace();
        }

        FindAndReplacePage mfindAndReplacePageAutomate = null;
        private void AutomateFindAndReplace()
        {
            mfindAndReplacePageAutomate = new FindAndReplacePage(FindAndReplacePage.eContext.AutomatePage, mBusinessFlow);
            mfindAndReplacePageAutomate.ShowAsWindow();
        }

        private void GenerateScenarioButton_Click(object sender, RoutedEventArgs e)
        {
            if (mBusinessFlow != null)
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
                Mouse.OverrideCursor = null;
            }
        }

        private void CleanScenarioButton_Click(object sender, RoutedEventArgs e)
        {
            ScenariosGenerator SG = new ScenariosGenerator();
            SG.ClearOptimizedScenariosVariables(mBusinessFlow);
            SG.ClearGeneretedActivites(mBusinessFlow);
        }

        private void OpenFeatureFileButton_Click(object sender, RoutedEventArgs e)
        {
            DocumentEditorPage documentEditorPage = new DocumentEditorPage(mRunner.CurrentBusinessFlow.ExternalID.Replace("~",  WorkSpace.Instance.Solution.Folder), true);
            documentEditorPage.Title = "Gherkin Page";
            documentEditorPage.Height = 700;
            documentEditorPage.Width = 1000;
            documentEditorPage.ShowAsWindow();
        }

        public void RunActionButton_Click(object sender, RoutedEventArgs e)
        {
            RunCurrentAutomatePageAction();
        }

        private void btnRunActivity_Click(object sender, RoutedEventArgs e)
        {
            ActionsPage ActPage = (ActionsPage)ActivityActionsFrame.Content;
            try
            {
                btnGridViewExecution_Click(sender, e);

                DisableGridSelectedItemChangeOnClick(ActPage.grdActions);
                AutoLogProxy.UserOperationStart("btnRunActivity_Click",  WorkSpace.Instance.Solution.Name, GetProjEnvironmentName());

                SetAutomateTabRunnerForExecution();

                mRunner.ExecutionLogger.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.ActivityRun;
                
                RunActivity();
                AutoLogProxy.UserOperationEnd();
            }
            finally
            {
                EnabelGridSelectedItemChangeOnClick(ActPage.grdActions);
                if (mRunner.CurrentBusinessFlow.CurrentActivity.CurrentAgent != null)
                {
                   ((Agent)mRunner.CurrentBusinessFlow.CurrentActivity.CurrentAgent).IsFailedToStart = false;
                }                    
            }
        }

        // Run Preparations before execution of Action/activity/Flow/start agent
        public void SetAutomateTabRunnerForExecution()
        {
            mRunner.ProjEnvironment = mEnvironment;
            mRunner.SolutionFolder =  WorkSpace.Instance.Solution.Folder;
            mRunner.DSList = new ObservableList<DataSourceBase>(WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>());
            mRunner.SolutionAgents = new ObservableList<Agent>(WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>());
            mRunner.SolutionApplications =  WorkSpace.Instance.Solution.ApplicationPlatforms;
        }

        public async Task RunActivity()
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

        private void DisableGridSelectedItemChangeOnClick(ucGrid UCGrid)
        {
            UCGrid.DisableUserItemSelectionChange = true;
        }

        private void EnabelGridSelectedItemChangeOnClick(ucGrid UCGrid)
        {
            UCGrid.DisableUserItemSelectionChange = false;
        }


        private async void btnRunFlow_Click(object sender, RoutedEventArgs e)
        {
            await RunAutomateTabFlow(true);
        }

        private async void btnRunFlowNoAnaylze_Click(object sender, RoutedEventArgs e)
        {
            await RunAutomateTabFlow();
        }

        private async void btnRunFlowAndGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            await RunAutomateTabFlow(true, true);
        }
        
        private async Task RunAutomateTabFlow(bool Analyz = false, bool ReportNeeded = false)
        {
            if (Analyz)
            {
                //Run Analyzer check if not including any High or Critical issues before execution
                Reporter.ToStatus(eStatusMsgKey.AnalyzerIsAnalyzing, null, mBusinessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                try
                {
                    AnalyzerPage analyzerPage = new AnalyzerPage();
                    analyzerPage.Init( WorkSpace.Instance.Solution, mBusinessFlow);
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
                if (ReportNeeded)
                {
                    RunAutomateTabFlowConf("Run Automate Tab Flow And Generate Report");
                }
                else
                {
                    RunAutomateTabFlowConf("Run Automate Tab Flow");
                }
                //execute
                await mRunner.RunBusinessFlowAsync(mBusinessFlow, true, false).ConfigureAwait(false);
                this.Dispatcher.Invoke(() =>
                {
                    AutoLogProxy.UserOperationEnd();
                    if (ReportNeeded)
                    {
                        btnLastExecutionHTMLReport_click(this, null);
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
                //enable grids
                EnableDisableAutomateTabGrids(true);
                mRunner.ResetFailedToStartFlagForAgents();
            }
        }

        private void ShowExecutionSummaryPage()
        {
            ExecutionSummaryPage w = new ExecutionSummaryPage(mContext);
            w.ShowAsWindow();
        }
    
        private void RunAutomateTabFlowConf(string runType)
        {
            btnGridViewExecution_Click(null, null);//shift to execution view

            AutoLogProxy.UserOperationStart(runType,  WorkSpace.Instance.Solution.Name, GetProjEnvironmentName());

            //disable grids  
            EnableDisableAutomateTabGrids(false);

            //execute preparations
            SetAutomateTabRunnerForExecution();
            mRunner.ResetRunnerExecutionDetails();
            mRunner.ExecutionLogger.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.BussinessFlowRun;
            
        }

        private void EnableDisableAutomateTabGrids(bool enableGrids)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    ActivitiesPage AP = null;
                    if (BFActivitiesFrame.Content != null && BFActivitiesFrame.Content.GetType() == typeof(ActivitiesPage)) // it can be the Activities MiniView
                    {
                        AP = (ActivitiesPage)BFActivitiesFrame.Content;
                    }

                    ActionsPage ActPage = (ActionsPage)ActivityActionsFrame.Content;
                    if (ActPage != null)
                    {
                        if (enableGrids)
                        {
                            EnabelGridSelectedItemChangeOnClick(ActPage.grdActions);
                        }
                        else
                        {
                            DisableGridSelectedItemChangeOnClick(ActPage.grdActions);
                        }
                    }

                    if (AP != null)
                    {
                        if (enableGrids)
                        {
                            EnabelGridSelectedItemChangeOnClick(AP.grdActivities);
                        }
                        else
                        {
                            DisableGridSelectedItemChangeOnClick(AP.grdActivities);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to disable Automate Tab grids for execution", ex);
            }
        }


        public async Task RunCurrentAutomatePageAction(bool checkIfActionAllowedToRun = true)
        {
            AutoLogProxy.UserOperationStart("RunActionButton_Click",  WorkSpace.Instance.Solution.Name, GetProjEnvironmentName());

            //TODO: Check if grid we are in execution view, no need to try and change of already in correct view
            btnGridViewExecution_Click(null, null);

            if (mBusinessFlow.CurrentActivity.Acts.Count() == 0)
            {
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "No Action to Run.");
                return;
            }

            SetAutomateTabRunnerForExecution();

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
            
            if ((typeof(ActPlugIn).IsAssignableFrom(actType)))
            {
                mRunner.SetCurrentActivityAgent(); 
            }

            mRunner.ExecutionLogger.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.ActionRun;
            var result = await mRunner.RunActionAsync((Act)mBusinessFlow.CurrentActivity.Acts.CurrentItem, checkIfActionAllowedToRun, true).ConfigureAwait(false);

            if (mRunner.CurrentBusinessFlow.CurrentActivity.CurrentAgent != null)
            {
                ((Agent)mRunner.CurrentBusinessFlow.CurrentActivity.CurrentAgent).IsFailedToStart = false;
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
                mRunner.StopRun();
            }
            finally
            {
                EnableDisableAutomateTabGrids(true);
            }
        }

        private void btnContinute_Click(object sender, RoutedEventArgs e)
        {
            ContinueRunFRomAutomateTab(eContinueFrom.LastStoppedAction);
        }

        private void ContinuteRunActiviytButton_Click(object sender, RoutedEventArgs e)
        {
            ContinueRunFRomAutomateTab(eContinueFrom.SpecificActivity);
        }

        private async Task ContinueRunFRomAutomateTab(eContinueFrom continueFrom)
        {
            try
            {
                btnGridViewExecution_Click(null, null);
                EnableDisableAutomateTabGrids(false);

                AutoLogProxy.UserOperationStart("ContinuteRunFrom" + continueFrom.ToString() + "_Click",  WorkSpace.Instance.Solution.Name, GetProjEnvironmentName());
                mRunner.ExecutionLogger.Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.ContinueRun;
                switch (continueFrom)
                {
                    case eContinueFrom.LastStoppedAction:
                        await mRunner.ContinueRunAsync(eContinueLevel.StandalonBusinessFlow, eContinueFrom.LastStoppedAction);
                        break;
                    case eContinueFrom.SpecificAction:
                        await mRunner.ContinueRunAsync(eContinueLevel.StandalonBusinessFlow, eContinueFrom.SpecificAction, mBusinessFlow, (Activity)mBusinessFlow.CurrentActivity, (Act)mBusinessFlow.CurrentActivity.Acts.CurrentItem);
                        break;
                    case eContinueFrom.SpecificActivity:
                        await mRunner.ContinueRunAsync(eContinueLevel.StandalonBusinessFlow, eContinueFrom.SpecificActivity, mBusinessFlow,(Activity) mBusinessFlow.CurrentActivity);
                        break;
                    default:
                        throw new NotImplementedException();
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
            ContinueRunFRomAutomateTab(eContinueFrom.SpecificAction);
        }

        private void StartAgent_Click(object sender, RoutedEventArgs e)
        {
            AutoLogProxy.UserOperationStart("StartAgent_Click");

            string agentsNames = mRunner.GetAgentsNameToRun();
            Reporter.ToStatus(eStatusMsgKey.StartAgents, null, agentsNames);

            mRunner.StopAgents();
            SetAutomateTabRunnerForExecution();
            mRunner.StartAgents();

            Reporter.HideStatusMessage();
            AutoLogProxy.UserOperationEnd();
        }

        private void btnGridViewAll_Click(object sender, RoutedEventArgs e)
        {          
           SetGridsView(Ginger.UserControls.GridViewDef.DefaultViewName);
        }

        private void btnGridViewDesign_Click(object sender, RoutedEventArgs e)
        {
            SetGridsView(eAutomatePageViewStyles.Design.ToString());
        }

        private void btnGridViewExecution_Click(object sender, RoutedEventArgs e)
        {
           SetGridsView(eAutomatePageViewStyles.Execution.ToString());
        }

        private void SummeryReportButton_Click(object sender, RoutedEventArgs e)
        {
            ShowExecutionSummaryPage();
        }

        public void btnLastExecutionHTMLReport_click(object sender, RoutedEventArgs e)
        {
            GenerateLastExecutedItemReport();
        }

        private void GenerateLastExecutedItemReport()
        {
            ExecutionLoggerConfiguration _selectedExecutionLoggerConfiguration =  WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            if (!_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled)
            {
                Reporter.ToUser(eUserMsgKey.ExecutionsResultsProdIsNotOn);
                return;
            }
            HTMLReportsConfiguration currentConf =  WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            //get logger files
            string exec_folder = Ginger.Run.ExecutionLogger.GetLoggerDirectory(_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationExecResultsFolder + "\\" + Ginger.Run.ExecutionLogger.defaultAutomationTabLogName);
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

        private void btnOfflineExecutionHTMLReport_click(object sender, RoutedEventArgs e)
        {
            ExecutionLoggerConfiguration _selectedExecutionLoggerConfiguration =  WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            if (!_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled)
            {
                Reporter.ToUser(eUserMsgKey.ExecutionsResultsProdIsNotOn);
                return;
            }
            HTMLReportsConfiguration currentConf =  WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            //create the execution logger files            
            string exec_folder = Ginger.Run.ExecutionLogger.GetLoggerDirectory(_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationExecResultsFolder + "\\" + Ginger.Run.ExecutionLogger.defaultAutomationTabOfflineLogName);

            if (Directory.Exists(exec_folder))
            {
                GingerCore.General.ClearDirectoryContent(exec_folder);
            }                
            else
            {
                Directory.CreateDirectory(exec_folder);
            }
            
            if (((ExecutionLogger)mRunner.ExecutionLogger).OfflineBusinessFlowExecutionLog(mBusinessFlow, exec_folder))
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

        private void ExportBizFlowButton_Click(object sender, RoutedEventArgs e)
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

        private void RefreshFromALM_Click(object sender, RoutedEventArgs e)
        {
            if (mBusinessFlow != null && mBusinessFlow.ActivitiesGroups != null && mBusinessFlow.ActivitiesGroups.Count > 0)
            {
                ALMIntegration.Instance.RefreshAllGroupsFromALM(mBusinessFlow);
            }
        }

        private void ExportExecutionResultsToALM_Click(object sender, RoutedEventArgs e)
        {
            ObservableList<BusinessFlow> bfs = new ObservableList<BusinessFlow>();
            bfs.Add(mBusinessFlow);
            ExportResultsToALMConfigPage.Instance.Init(bfs, new GingerCore.ValueExpression(mEnvironment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>(), false, "", false));
            ExportResultsToALMConfigPage.Instance.ShowAsWindow();
        }

        private void lstEnvs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstEnvs != null && lstEnvs.SelectedItem != null)
            {
                UpdateUsedEnvironment((ProjEnvironment)lstEnvs.SelectedItem);                
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
            mRunner.ProjEnvironment = mEnvironment;
            mRunner.ExecutionLogger.ExecutionEnvironment = mEnvironment;
            if (mEnvironment != null)
            {
                WorkSpace.Instance.UserProfile.RecentEnvironment = mEnvironment.Guid;
            }
        }

        private void xRibbon_Loaded(object sender, RoutedEventArgs e)
        {
            Grid child = VisualTreeHelper.GetChild((DependencyObject)sender, 0) as Grid;
            if (child != null)
            {
                child.RowDefinitions[0].Height = new GridLength(0);
            }

            RibbonRow.Height = new GridLength(120);
        }

        private void TimeLineReportButton_Click(object sender, RoutedEventArgs e)
        {
            GingerRunnerTimeLine gingerRunnerTimeLine = (GingerRunnerTimeLine)(from x in mRunner.RunListeners where x.GetType() == typeof(GingerRunnerTimeLine) select x).SingleOrDefault();
            TimeLinePage timeLinePage = new TimeLinePage(gingerRunnerTimeLine.timeLineEvents);
            timeLinePage.ShowAsWindow();
        }

        private void CboSpeed_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mRunner.SetSpeed(int.Parse(cboSpeed.Text));
        }
    }
}
