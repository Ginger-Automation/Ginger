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

using AccountReport.Contracts.GraphQL.ResponseModels;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.LiteDBFolder;
using Amdocs.Ginger.CoreNET.Logger;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.UserControls;
using Ginger.Actions;
using Ginger.AnalyzerLib;
using Ginger.Configurations;
using Ginger.Functionalities;
using Ginger.MoveToGingerWPF.Run_Set_Pages;
using Ginger.Reports;
using Ginger.RunSetLib.CreateCLIWizardLib;
using Ginger.SolutionCategories;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.UserControlsLib;
using Ginger.UserControlsLib.VisualFlow;
using Ginger.ValidationRules;
using GingerCore;
using GingerCore.Actions;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.GeneralLib;
using GingerCore.Helpers;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WizardLib;
using GraphQL;
using GraphQLClient.Clients;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Ginger.Run
{
    /// <summary>
    /// Interaction logic for RunPage.xaml
    /// </summary>
    public partial class NewRunSetPage : GingerUIPage
    {
        public RunnerPage mCurrentSelectedRunner;

        GenericWindow _pageGenericWin = null;
        FlowDiagramPage mFlowDiagram;
        int mFlowX = 0;
        int mFlowY = 0;
        bool IsSelectedItemSyncWithExecution = true;//execution and selected items are synced as default   
        private bool IsCalledFromxUndoBtn = false;
        SingleItemTreeViewSelectionPage mRunSetsSelectionPage = null;
        SingleItemTreeViewSelectionPage mBusFlowsSelectionPage = null;
        RunsetOperationsPage mRunsetOperations = null;
        RunSetConfig mRunSetConfig = null;
        RunSetsExecutionsHistoryPage mRunSetsExecutionsPage = null;
        SolutionCategoriesPage mSolutionCategoriesPage = null;
        RunSetsALMDefectsOpeningPage mRunSetsALMDefectsOpeningPage = null;
        private FileSystemWatcher mBusinessFlowsXmlsChangeWatcher = null;
        private bool mRunSetBusinessFlowWasChanged = false;
        private bool mSolutionWasChanged = false;
        Context mContext = new();
        private readonly bool _ignoreValidationRules;
        private Task<GraphQLResponse<GraphQLRunsetResponse>> response;
        private GraphQLResponse<GraphQLRunsetResponse> data;
        private GraphQlClient graphQlClient = null;
        private ExecutionReportGraphQLClient executionReportGraphQLClient;

        public enum eObjectType
        {
            BusinessFlow,
            Activity,
            Action,
            Legend
        }
        public RunSetConfig RunSetConfig
        {
            get { return mRunSetConfig; }
            set
            {
                if (mRunSetConfig == null || mRunSetConfig.Equals(value) == false)
                {
                    LoadRunSetConfig(value);
                }
            }
        }

        RunnerItemPage mCurrentBusinessFlowRunnerItem
        {
            get
            {
                if (xBusinessflowsRunnerItemsListView != null && xBusinessflowsRunnerItemsListView.Items.Count > 0
                        && xBusinessflowsRunnerItemsListView.SelectedItem != null)
                {
                    /// Avoid selecting the first BusinessFlow as default to avoid Activities loading for saving the Runset load time
                    //if (xBusinessflowsRunnerItemsListView.SelectedItem == null)
                    //{
                    //    xBusinessflowsRunnerItemsListView.SelectedIndex = 0;
                    //}

                    return (RunnerItemPage)xBusinessflowsRunnerItemsListView.SelectedItem;
                }
                else
                {
                    return null;
                }
            }
        }

        BusinessFlow mCurrentBusinessFlowRunnerItemObject
        {
            get
            {
                if (mCurrentActivityRunnerItem != null)
                {
                    return (BusinessFlow)((RunnerItemPage)xBusinessflowsRunnerItemsListView.SelectedItem).ItemObject;
                }
                else
                {
                    return null;
                }
            }
        }

        RunnerItemPage mCurrentActivityRunnerItem
        {
            get
            {
                if (xActivitiesRunnerItemsListView != null && xActivitiesRunnerItemsListView.Items.Count > 0)
                {
                    if (xActivitiesRunnerItemsListView.SelectedItem == null)
                    {
                        xActivitiesRunnerItemsListView.SelectedIndex = 0;
                    }

                    return (RunnerItemPage)xActivitiesRunnerItemsListView.SelectedItem;
                }
                else
                {
                    return null;
                }
            }
        }

        Activity mCurrentActivityRunnerItemObject
        {
            get
            {
                if (mCurrentActivityRunnerItem != null)
                {
                    return (Activity)((RunnerItemPage)xActivitiesRunnerItemsListView.SelectedItem).ItemObject;
                }
                else
                {
                    return null;
                }
            }
        }

        RunnerItemPage mCurrentActionRunnerItem
        {
            get
            {
                if (xActionsRunnerItemsListView != null && xActionsRunnerItemsListView.Items.Count > 0)
                {
                    if (xActionsRunnerItemsListView.SelectedItem == null)
                    {
                        xActionsRunnerItemsListView.SelectedIndex = 0;
                    }

                    return (RunnerItemPage)xActionsRunnerItemsListView.SelectedItem;
                }
                else
                {
                    return null;
                }
            }
        }


        // Names !!!
        public enum eEditMode
        {
            ExecutionFlow = 0,
            View = 1,
        }
        public eEditMode mEditMode { get; set; }

        public NewRunSetPage()//when window opened manually
        {
            InitializeComponent();

            if (WorkSpace.Instance.Solution != null)
            {
                Init();

                //load Run Set
                RunSetConfig defaultRunSet = GetDefualtRunSetConfig();

                if (defaultRunSet != null)
                {
                    if (WorkSpace.Instance.UserProfile.AutoLoadLastRunSet)
                    {
                        LoadRunSetConfig(defaultRunSet);
                    }
                    else
                    {
                        LoadRunSetConfigBySelection(defaultRunSet);
                    }
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, string.Format("No {0} found to load, please add {0}.", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                }
            }
        }

        private void LoadRunSetConfigBySelection(RunSetConfig defaultRunSet)
        {
            //hide current Run set UI
            xRunsetPageGrid.Visibility = Visibility.Collapsed;

            if (mRunSetsSelectionPage == null)
            {
                RunSetFolderTreeItem runSetsRootfolder = new RunSetFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<RunSetConfig>());
                mRunSetsSelectionPage = new SingleItemTreeViewSelectionPage(GingerDicser.GetTermResValue(eTermResKey.RunSets), eImageType.RunSet, runSetsRootfolder, SingleItemTreeViewSelectionPage.eItemSelectionType.Single, true);
                mRunSetsSelectionPage.xTreeView.Tree.GetChildItembyNameandSelect(defaultRunSet.Name);
            }

            List<object> selectedRunSet = mRunSetsSelectionPage.ShowAsWindow();
            if (selectedRunSet != null && selectedRunSet.Count > 0)
            {
                RunSetConfig selectedRunset = (RunSetConfig)selectedRunSet[0];
                LoadRunSetConfig(selectedRunset);
            }
            else
            {
                LoadRunSetConfig(defaultRunSet);
            }
        }

        public NewRunSetPage(RunSetConfig runSetConfig, eEditMode editMode = eEditMode.ExecutionFlow, bool ignoreValidationRules = false)//when window opened automatically when running from command line
        {
            InitializeComponent();
            //Init
            Init();

            mEditMode = editMode;
            _ignoreValidationRules = ignoreValidationRules;
            if (mEditMode == eEditMode.View)
            {
                Config.IsEnabled = false;
                xOperationsPnl.IsEnabled = false;
                xRunnersCanvasControls.IsEnabled = false;
                xRunnersExecutionControls.IsEnabled = false;
                xBusinessFlowsListOperationsPnl.IsEnabled = false;
                xRunsetOperationsTab.IsEnabled = false;
                mALMDefectsOpening.IsEnabled = false;
                mExecutionSummary.IsEnabled = false;
                LoadRunSetConfig(runSetConfig, false, true);
                return;
            }
            else
            {
                Config.IsEnabled = true;
                xRunsetOperationsTab.IsEnabled = true;
                mALMDefectsOpening.IsEnabled = true;
                mExecutionSummary.IsEnabled = true;
            }

            if (WorkSpace.Instance.RunningInExecutionMode)
            {
                xOperationsPnl.Visibility = Visibility.Collapsed;
                xRunnersCanvasControls.Visibility = Visibility.Collapsed;
                xRunnersExecutionControls.Visibility = Visibility.Collapsed;
                xBusinessFlowsListOperationsPnl.Visibility = Visibility.Collapsed;
                xMiniRunnerExecutionPanel.Visibility = Visibility.Collapsed;
            }

            //load Run Set
            if (runSetConfig != null)
            {
                LoadRunSetConfig(runSetConfig, false);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, string.Format("No {0} found to load, please add {0}.", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
            }
        }

        private void Init()
        {
            SetNonSpecificRunSetEventsTracking();
            SetBusinessFlowsChangesLisener();
        }
        private string allProperties = string.Empty;
        private void SetNonSpecificRunSetEventsTracking()
        {
            WorkSpace.Instance.PropertyChanged -= WorkSpacePropertyChanged;
            WorkSpace.Instance.PropertyChanged += WorkSpacePropertyChanged;

            PropertyChangedEventManager.RemoveHandler(source: WorkSpace.Instance.RunsetExecutor, handler: RunsetExecutor_PropertyChanged, propertyName: allProperties);
            PropertyChangedEventManager.AddHandler(source: WorkSpace.Instance.RunsetExecutor, handler: RunsetExecutor_PropertyChanged, propertyName: allProperties);

            CollectionChangedEventManager.RemoveHandler(source: WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>(), handler: AgentsCache_CollectionChanged);
            CollectionChangedEventManager.AddHandler(source: WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>(), handler: AgentsCache_CollectionChanged);

            WeakEventManager<Selector, SelectionChangedEventArgs>.RemoveHandler(source: xBusinessflowsRunnerItemsListView, eventName: nameof(Selector.SelectionChanged), handler: xActivitiesListView_SelectionChanged);
            WeakEventManager<Selector, SelectionChangedEventArgs>.AddHandler(source: xBusinessflowsRunnerItemsListView, eventName: nameof(Selector.SelectionChanged), handler: xActivitiesListView_SelectionChanged);

            WeakEventManager<Selector, SelectionChangedEventArgs>.RemoveHandler(source: xActivitiesRunnerItemsListView, eventName: nameof(Selector.SelectionChanged), handler: xActionsListView_SelectionChanged);
            WeakEventManager<Selector, SelectionChangedEventArgs>.AddHandler(source: xActivitiesRunnerItemsListView, eventName: nameof(Selector.SelectionChanged), handler: xActionsListView_SelectionChanged);

            CollectionChangedEventManager.RemoveHandler(source: xActivitiesRunnerItemsListView.Items, handler: xActivitiesRunnerItemsListView_CollectionChanged);
            CollectionChangedEventManager.AddHandler(source: xActivitiesRunnerItemsListView.Items, handler: xActivitiesRunnerItemsListView_CollectionChanged);
        }

        private void AgentsCache_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Task.Run(() =>
            {
                if (mRunSetConfig != null)
                {
                    Parallel.ForEach(mRunSetConfig.GingerRunners, Runner =>
                    {
                        ((GingerExecutionEngine)Runner.Executor).SolutionAgents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>();
                        //to get the latest list of applications agents
                        ((GingerExecutionEngine)Runner.Executor).UpdateApplicationAgents();
                    });
                    this.Dispatcher.Invoke(() =>
                    {
                        List<FlowElement> fe = mFlowDiagram.GetAllFlowElements();
                        foreach (FlowElement f in fe)
                        {
                            ((RunnerPage)f.GetCustomeShape().Content).UpdateRunnerInfo();
                        }
                    });
                }
            });
        }

        private void xActivitiesRunnerItemsListView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                xActivitiesName.Content = GingerDicser.GetTermResValue(eTermResKey.Activities) + " (" + xActivitiesRunnerItemsListView.Items.Count + ")";
            });
        }

        private void Runner_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (e.PropertyName == nameof(GingerExecutionEngine.Status))
                {
                    if (IsSelectedItemSyncWithExecution && mRunSetConfig.RunModeParallel == false
                                && (((GingerExecutionEngine)((GingerRunner)sender).Executor).Status == eRunStatus.Running))
                    {
                        List<FlowElement> fe = mFlowDiagram.GetAllFlowElements();
                        foreach (FlowElement f in fe)
                        {
                            RunnerPage rp = (RunnerPage)f.GetCustomeShape().Content;
                            if (rp != null && rp.ExecutorEngine.GingerRunner.Guid.Equals(((GingerRunner)sender).Guid))
                            {
                                GingerRunnerHighlight(rp);
                            }
                        }
                    }

                    UpdateRunButtonIcon();
                    UpdateReRunFailedButtonIcon();
                }
            });
        }

        private void UpdateRunButtonIcon(bool isRunStarted = false)
        {
            this.Dispatcher.Invoke(() =>
            {
                bool setAsRunning = false;

                if (isRunStarted)
                {
                    xRunRunsetBtn.ButtonText = "Starting...";
                    xRunRunsetBtn.ToolTip = "Performing Run Preparations";
                    setAsRunning = true;
                }
                else if (mRunSetConfig.IsRunning)
                {
                    xRunRunsetBtn.ButtonText = "Running...";
                    setAsRunning = true;
                }
                else if (RunSetConfig.GingerRunners.FirstOrDefault(x => x.Status == eRunStatus.Running) != null)
                {
                    xRunRunsetBtn.ButtonText = "Running...";
                    xRunRunsetBtn.ToolTip = "Execution of at least one Runner is in progress";
                    setAsRunning = true;
                }
                else
                {
                    xRunRunsetBtn.ButtonText = "Run";
                    xRunRunsetBtn.ToolTip = "Reset & Run All Runners";
                    setAsRunning = false;
                }

                if (setAsRunning)
                {
                    xRunRunsetBtn.ButtonImageType = eImageType.Running;
                    xRunRunsetBtn.ButtonStyle = (Style)FindResource("$RoundTextAndImageButtonStyle_ExecutionRunning");
                    xRunRunsetBtn.ButtonImageForground = (SolidColorBrush)FindResource("$HighlightColor_LightBlue");
                    xRunRunsetBtn.IsEnabled = false;
                    if (RunSetConfig.GingerRunners.Any(x => x.Executor.IsRunning == true))
                    {
                        xStopRunsetBtn.ButtonText = "Stop";
                        xStopRunsetBtn.ButtonImageType = eImageType.Stop;
                        xStopRunsetBtn.ButtonStyle = (Style)FindResource("$RoundTextAndImageButtonStyle_ExecutionStop");
                        xStopRunsetBtn.IsEnabled = true;
                        xStopRunsetBtn.Visibility = Visibility.Visible;
                    }
                    xContinueRunsetBtn.Visibility = Visibility.Collapsed;
                    xResetRunsetBtn.Visibility = Visibility.Collapsed;
                }
                else
                {
                    xRunRunsetBtn.ButtonImageType = eImageType.Run;
                    xRunRunsetBtn.ButtonStyle = (Style)FindResource("$RoundTextAndImageButtonStyle_Execution");
                    xRunRunsetBtn.ButtonImageForground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                    xRunRunsetBtn.IsEnabled = true;
                    xStopRunsetBtn.Visibility = Visibility.Collapsed;
                    xContinueRunsetBtn.Visibility = Visibility.Visible;
                    xResetRunsetBtn.Visibility = Visibility.Visible;
                    xRunsetSaveBtn.IsEnabled = true;
                }


            });
        }

        private void UpdateReRunFailedButtonIcon(bool isRunStarted = false)
        {

            this.Dispatcher.Invoke(() =>
            {
                bool setAsRunning = false;
                if (WorkSpace.Instance.RunsetExecutor.Runners.Any(x => x.Executor != null && x.Executor.BusinessFlows.Any(x => x.RunStatus == eRunStatus.Failed)))
                {
                    xReRunFailedRunsetBtn.Visibility = Visibility.Visible;
                    xRunRunsetBtn.Visibility = Visibility.Collapsed;

                    if (isRunStarted)
                    {
                        xReRunFailedRunsetBtn.ButtonText = "Starting...";
                        xReRunFailedRunsetBtn.ToolTip = "Performing Run Preparations";
                        setAsRunning = true;
                    }
                    else if (mRunSetConfig.IsRunning)
                    {
                        xReRunFailedRunsetBtn.ButtonText = "Running...";
                        xReRunFailedRunsetBtn.ToolTip = "Execution of at least one Runner is in progress";
                        setAsRunning = true;
                    }
                    else
                    {
                        xReRunFailedRunsetBtn.ButtonText = "Re-Run Failed Flows";
                        xReRunFailedRunsetBtn.ToolTip = "Re-Run All Failed Business flows";
                        setAsRunning = false;
                    }

                    if (setAsRunning)
                    {
                        xReRunFailedRunsetBtn.ButtonImageType = eImageType.Running;
                        xReRunFailedRunsetBtn.ButtonStyle = (Style)FindResource("$RoundTextAndImageButtonStyle_ExecutionRunning");
                        xReRunFailedRunsetBtn.ButtonImageForground = (SolidColorBrush)FindResource("$SelectionColor_LightBlue");
                        xReRunFailedRunsetBtn.IsEnabled = false;
                        if (RunSetConfig.GingerRunners.Any(x => x.Executor.IsRunning == true))
                        {
                            xStopRunsetBtn.ButtonText = "Stop";
                            xStopRunsetBtn.ButtonImageType = eImageType.Stop;
                            xStopRunsetBtn.ButtonStyle = (Style)FindResource("$RoundTextAndImageButtonStyle_ExecutionStop");
                            xStopRunsetBtn.IsEnabled = true;
                            xStopRunsetBtn.Visibility = Visibility.Visible;
                        }
                        xContinueRunsetBtn.Visibility = Visibility.Collapsed;
                        xResetRunsetBtn.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        xReRunFailedRunsetBtn.ButtonImageType = eImageType.Run;
                        xReRunFailedRunsetBtn.ButtonStyle = (Style)FindResource("$RoundTextAndImageButtonStyle_Execution");
                        xReRunFailedRunsetBtn.ButtonImageForground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                        xReRunFailedRunsetBtn.IsEnabled = true;
                        xStopRunsetBtn.Visibility = Visibility.Collapsed;
                        xContinueRunsetBtn.Visibility = Visibility.Visible;
                        xResetRunsetBtn.Visibility = Visibility.Visible;
                        xRunsetSaveBtn.IsEnabled = true;
                    }
                }
                else
                {
                    xReRunFailedRunsetBtn.Visibility = Visibility.Collapsed;
                    xRunRunsetBtn.Visibility = Visibility.Visible;
                }
            });
        }
        private bool CheckIfExecutionIsInProgress()
        {
            if (mRunSetConfig.IsRunning || RunSetConfig.GingerRunners.FirstOrDefault(x => x.Status == eRunStatus.Running || x.Executor.IsRunning == true) != null)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Operation can't be done during execution.");
                return true;
            }

            return false;
        }

        private void ExecutionsHistoryList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                UpdateRunsetExecutionHistoryTabHeader();
            });
        }

        public void UpdateRunsetExecutionHistoryTabHeader()
        {
            ExecutionSummary.Text = string.Format("Executions History ({0})", mRunSetsExecutionsPage.ExecutionsHistoryList.Count);
            //ExecutionSummary.Foreground = (Brush)Application.Current.Resources["$SelectionColor_Pink"];
        }

        public void ResetALMDefectsSuggestions()
        {
            WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList = [];
            xALMDefectsOpening.IsEnabled = false;
            UpdateRunsetALMDefectsOpeningTabHeader();
        }

        public void UpdateRunsetALMDefectsOpeningTabHeader()
        {
            if (WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList.Count > 0)
            {
                ALMDefects.Text = string.Format("ALM Defects Opening ({0})", WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList.Count);
                //ALMDefects.Foreground = (Brush)Application.Current.Resources["$HighlightColor_Purple"];
            }
            else
            {
                ALMDefects.Text = string.Format("ALM Defects Opening");
                //ALMDefects.Foreground = (Brush)Application.Current.Resources["$HighlightColor_Purple"];
            }
        }

        private async void RunnerItem_RunnerItemEvent(object? sender, RunnerItemEventArgs EventArgs)
        {
            Run.GingerExecutionEngine currentSelectedRunner = mCurrentSelectedRunner.ExecutorEngine;

            switch (EventArgs.EventType)
            {
                case RunnerItemEventArgs.eEventType.ContinueRunRequired:
                    if (currentSelectedRunner.IsRunning)
                    {
                        Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Runner is already running, please stop it first.");
                        return;
                    }
                    WorkSpace.Instance.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder = null;
                    if (RunSetConfig.ExecutionID == null)
                    {
                        RunSetConfig.ExecutionID = Guid.NewGuid();
                    }
                    switch (EventArgs.RunnerItemType)
                    {
                        case RunnerItemPage.eRunnerItemType.BusinessFlow:
                            await currentSelectedRunner.ContinueRunAsync(eContinueLevel.Runner, eContinueFrom.SpecificBusinessFlow, (BusinessFlow)EventArgs.RunnerItemObject, null, null);
                            break;
                        case RunnerItemPage.eRunnerItemType.Activity:
                            await currentSelectedRunner.ContinueRunAsync(eContinueLevel.Runner, eContinueFrom.SpecificActivity, mCurrentBusinessFlowRunnerItemObject, (Activity)EventArgs.RunnerItemObject, null);
                            break;
                        case RunnerItemPage.eRunnerItemType.Action:
                            await currentSelectedRunner.ContinueRunAsync(eContinueLevel.Runner, eContinueFrom.SpecificAction, mCurrentBusinessFlowRunnerItemObject, mCurrentActivityRunnerItemObject, (Act)EventArgs.RunnerItemObject);
                            break;
                    }
                    break;
                case RunnerItemEventArgs.eEventType.SetAsSelectedRequired:
                    if (IsSelectedItemSyncWithExecution)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            switch (EventArgs.RunnerItemType)
                            {
                                case RunnerItemPage.eRunnerItemType.BusinessFlow:
                                    xBusinessflowsRunnerItemsListView.SelectedItem = EventArgs.RunnerItemPage;
                                    break;
                                case RunnerItemPage.eRunnerItemType.Activity:
                                    xActivitiesRunnerItemsListView.SelectedItem = EventArgs.RunnerItemPage;
                                    break;
                                case RunnerItemPage.eRunnerItemType.Action:
                                    xActionsRunnerItemsListView.SelectedItem = EventArgs.RunnerItemPage;
                                    break;
                            }
                        });
                    }
                    break;
                case RunnerItemEventArgs.eEventType.ViewRunnerItemRequired:
                    switch (EventArgs.RunnerItemType)
                    {
                        case RunnerItemPage.eRunnerItemType.BusinessFlow:
                            viewBusinessflow((BusinessFlow)EventArgs.RunnerItemObject);
                            break;
                        case RunnerItemPage.eRunnerItemType.Activity:
                            viewActivity((Activity)EventArgs.RunnerItemObject);
                            break;
                        case RunnerItemPage.eRunnerItemType.Action:
                            viewAction((Act)EventArgs.RunnerItemObject);
                            break;
                    }
                    break;
                case RunnerItemEventArgs.eEventType.ViewConfiguration:
                    switch (EventArgs.RunnerItemType)
                    {
                        case RunnerItemPage.eRunnerItemType.BusinessFlow:
                            viewBusinessflowConfiguration((BusinessFlow)EventArgs.RunnerItemObject);
                            break;
                    }
                    break;

            }
        }


        private void BusinessFlows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (mCurrentSelectedRunner.BusinessflowRunnerItems.Count > 0)
                {
                    mCurrentSelectedRunner.BusinessflowRunnerItems.RemoveAt(e.OldStartingIndex);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Move)
            {
                mCurrentSelectedRunner.BusinessflowRunnerItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                mCurrentSelectedRunner.ExecutorEngine.GingerRunner.DirtyStatus = eDirtyStatus.Modified;
            }
            this.Dispatcher.Invoke(() =>
            {
                mCurrentSelectedRunner.ExecutorEngine.TotalBusinessflow = ((IList<BusinessFlow>)sender).Count;
                mCurrentSelectedRunner.UpdateExecutionStats();
                UpdateBusinessflowCounter();
                mCurrentSelectedRunner.UpdateRunnerInfo();
            });
        }
        private void Runners_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                GingerRunner gr = (GingerRunner)e.NewItems[0];
                Dispatcher.InvokeAsync(() =>
                {
                    if (gr.Executor == null)
                    {
                        gr.Executor = new GingerExecutionEngine(gr);
                    }
                    RunnerPage runnerPage = new(runnerItemEventHandler: RunnerItem_RunnerItemEvent);
                    runnerPages.Add(runnerPage);
                    InitRunnerFlowElement(runnerPage, (GingerExecutionEngine)gr.Executor, e.NewStartingIndex);
                    GingerRunnerHighlight(runnerPage);
                });
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                GingerRunner gr = (GingerRunner)e.OldItems[0];
                mFlowDiagram.RemoveFlowElem(gr.Guid.ToString(), mRunSetConfig.RunModeParallel);
            }
            else if (e.Action == NotifyCollectionChangedAction.Move)
            {
                mFlowDiagram.MoveFlowElement(e.OldStartingIndex, e.NewStartingIndex);
            }
            this.Dispatcher.Invoke(() =>
            {
                UpdateRunnersTabHeader();
                UpdateRunnersCanvasSize();
            });
        }
        private void UpdateBusinessflowCounter()
        {
            xBusinessflowName.Content = string.Format("{0} ({1})", GingerDicser.GetTermResValue(eTermResKey.BusinessFlows), mCurrentSelectedRunner.ExecutorEngine.BusinessFlows.Count);
        }

        private void UpdateRunnersCanvasSize()
        {
            if (mFlowDiagram != null)
            {
                mFlowDiagram.Height = 240;
                mFlowDiagram.CanvasHeight = 240;
                mFlowDiagram.CanvasWidth = mRunSetConfig.GingerRunners.Count * 620;
            }
        }

        private void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        void InitRunSetConfigurations()
        {
            BindingHandler.ObjFieldBinding(xRunSetNameTextBox, TextBox.TextProperty, mRunSetConfig, nameof(RunSetConfig.Name));
            if (!_ignoreValidationRules)
            {
                xRunSetNameTextBox.AddValidationRule(new RunSetNameValidationRule());
            }
            xShowIDUC.Init(mRunSetConfig);
            BindingHandler.ObjFieldBinding(xRunSetDescriptionTextBox, TextBox.TextProperty, mRunSetConfig, nameof(RunSetConfig.Description));
            TagsViewer.Init(mRunSetConfig.Tags);
            BindingHandler.ObjFieldBinding(xPublishcheckbox, CheckBox.IsCheckedProperty, mRunSetConfig, nameof(RepositoryItemBase.Publish));

            // Value expression textboxes
            xSealightsTestStageTextBox.Init(mContext, mRunSetConfig, nameof(RunSetConfig.SealightsTestStage));
            xSealightsLabIdTextBox.Init(mContext, mRunSetConfig, nameof(RunSetConfig.SealightsLabId));
            xSealightsBuildSessionIDTextBox.Init(mContext, mRunSetConfig, nameof(RunSetConfig.SealightsBuildSessionID));

            // check if fields have been populated (front-end validation)
            if (!_ignoreValidationRules)
            {
                xSealightsLabIdTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValueWithDependency(mRunSetConfig, nameof(RunSetConfig.SealightsBuildSessionID), "Lab ID or Build Session ID must be provided"));
                xSealightsBuildSessionIDTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValueWithDependency(mRunSetConfig, nameof(RunSetConfig.SealightsLabId), "Lab ID or Build Session ID must be provided"));
                xSealightsTestStageTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("Test Stage cannot be empty"));
            }

            WeakEventManager<ToggleButton, RoutedEventArgs>.AddHandler(source: xDefaultTestStageRadioBtn, eventName: nameof(ToggleButton.Checked), handler: XDefaultTestStageRadioBtn_Checked);
            WeakEventManager<ToggleButton, RoutedEventArgs>.AddHandler(source: xDefaultLabIdRadioBtn, eventName: nameof(ToggleButton.Checked), handler: XDefaultLabIdRadioBtn_Checked);
            WeakEventManager<ToggleButton, RoutedEventArgs>.AddHandler(source: xDefaultSessionIdRadioBtn, eventName: nameof(ToggleButton.Checked), handler: XDefaultSessionIdRadioBtn_Checked);
            WeakEventManager<ToggleButton, RoutedEventArgs>.AddHandler(source: xDefaultTestRecommendationsRadioBtn, eventName: nameof(ToggleButton.Checked), handler: XDefaultTestRecommendationsRadioBtn_Checked);

            WeakEventManager<ToggleButton, RoutedEventArgs>.AddHandler(source: xCustomTestStageRadioBtn, eventName: nameof(ToggleButton.Checked), handler: XCustomTestStageRadioBtn_Checked);
            WeakEventManager<ToggleButton, RoutedEventArgs>.AddHandler(source: xCustomLabIdRadioBtn, eventName: nameof(ToggleButton.Checked), handler: XCustomLabIdRadioBtn_Checked);
            WeakEventManager<ToggleButton, RoutedEventArgs>.AddHandler(source: xCustomSessionIdRadioBtn, eventName: nameof(ToggleButton.Checked), handler: XCustomSessionIdRadioBtn_Checked);


            if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsTestStage == null) // init values
            {
                xDefaultTestStageRadioBtn.IsChecked = true;
                xSealightsTestStageTextBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                xCustomTestStageRadioBtn.IsChecked = true;
                XCustomTestStageRadioBtn_Checked(null, null); // Check the custom radion btn 
            }


            if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsLabId == null)
            {
                xDefaultLabIdRadioBtn.IsChecked = true;
                xSealightsLabIdTextBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                xCustomLabIdRadioBtn.IsChecked = true;
                XCustomLabIdRadioBtn_Checked(null, null);
            }

            if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsBuildSessionID == null)
            {
                xDefaultSessionIdRadioBtn.IsChecked = true;
                xSealightsBuildSessionIDTextBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                xCustomSessionIdRadioBtn.IsChecked = true;
                XCustomSessionIdRadioBtn_Checked(null, null);
            }

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xSealightsExpander, Expander.VisibilityProperty, WorkSpace.Instance.UserProfile, nameof(WorkSpace.Instance.UserProfile.ShowEnterpriseFeatures), bindingConvertor: new GingerCore.GeneralLib.BoolVisibilityConverter(), BindingMode: System.Windows.Data.BindingMode.OneWay);

            if (WorkSpace.Instance.UserProfile.ShowEnterpriseFeatures)
            {
                //Sealights Logger configuration settings should be visible only if the Sealight logger is set to yes in Configurations tab
                if (WorkSpace.Instance.Solution.SealightsConfiguration.SealightsLog == SealightsConfiguration.eSealightsLog.No)
                {
                    xSealightsExpander.Visibility = Visibility.Collapsed;
                }
                else if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsBuildSessionID == null &&
                    WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsLabId == null &&
                    WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsTestStage == null)
                {
                    xSealightsExpander.IsExpanded = false; //Sealight expand control should collapsed if all 3 Sealights' settings are in ‘Default’ mode.
                }
            }
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xALMDefectsOpening, Expander.VisibilityProperty, WorkSpace.Instance.UserProfile, nameof(WorkSpace.Instance.UserProfile.ShowEnterpriseFeatures), bindingConvertor: new GingerCore.GeneralLib.BoolVisibilityConverter(), BindingMode: System.Windows.Data.BindingMode.OneWay);
        }

        private void XCustomSessionIdRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            xSealightsBuildSessionIDTextBox.Visibility = Visibility.Visible;

            if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsBuildSessionID == null || WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsBuildSessionID.Trim() == "")
            {
                WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsBuildSessionID = WorkSpace.Instance.Solution.SealightsConfiguration.SealightsBuildSessionID;
            }
        }

        private void XCustomLabIdRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            xSealightsLabIdTextBox.Visibility = Visibility.Visible;

            if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsLabId == null || WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsLabId.Trim() == "")
            {
                WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsLabId = WorkSpace.Instance.Solution.SealightsConfiguration.SealightsLabId;
            }
        }

        private void XCustomTestStageRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            xSealightsTestStageTextBox.Visibility = Visibility.Visible;

            if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsTestStage == null || WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsTestStage.Trim() == "")
            {
                WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsTestStage = WorkSpace.Instance.Solution.SealightsConfiguration.SealightsTestStage;
            }
        }

        private void XDefaultTestStageRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            xSealightsTestStageTextBox.Visibility = Visibility.Collapsed;
            WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsTestStage = null;
        }
        private void XDefaultLabIdRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            xSealightsLabIdTextBox.Visibility = Visibility.Collapsed;
            WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsLabId = null;
        }
        private void XDefaultSessionIdRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            xSealightsBuildSessionIDTextBox.Visibility = Visibility.Collapsed;
            WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsBuildSessionID = null;
        }
        private void XDefaultTestRecommendationsRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            mRunSetConfig.SealightsTestRecommendationsRunsetOverrideFlag = false;
            WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsTestRecommendations = WorkSpace.Instance.Solution.SealightsConfiguration.SealightsTestRecommendations;
        }
        private void SealightsTestRecommendationRadioButtons_CheckedHandler(object sender, RoutedEventArgs e)
        {
            string value = ((RadioButton)sender).Tag?.ToString();

            if (Enum.TryParse(value, out SealightsConfiguration.eSealightsTestRecommendations sealightsTestRecommendations))
            {
                if (sealightsTestRecommendations == SealightsConfiguration.eSealightsTestRecommendations.Yes)
                {
                    mRunSetConfig.SealightsTestRecommendations = SealightsConfiguration.eSealightsTestRecommendations.Yes;
                }
                else
                {
                    mRunSetConfig.SealightsTestRecommendations = SealightsConfiguration.eSealightsTestRecommendations.No;
                }
                mRunSetConfig.SealightsTestRecommendationsRunsetOverrideFlag = true;
            }
        }

        void InitRunSetInfoSection()
        {
            BindingOperations.ClearBinding(xRunSetUcLabel.xNameTextBlock, TextBlock.TextProperty);
            BindingHandler.ObjFieldBinding(xRunSetUcLabel.xNameTextBlock, TextBlock.TextProperty, mRunSetConfig, nameof(RunSetConfig.Name));
            xRunSetUcLabel.xNameTextBlock.ToolTip = GetToolTipForRunSetLabel();
            PropertyChangedEventManager.RemoveHandler(mRunSetConfig, mRunSetConfig_PropertyChanged, propertyName: allProperties);
            PropertyChangedEventManager.AddHandler(mRunSetConfig, mRunSetConfig_PropertyChanged, propertyName: allProperties);
            BindingHandler.ObjFieldBinding(xRunSetUcLabel.xNameTextBlock, TextBlock.ForegroundProperty, mRunSetConfig, nameof(RunSetConfig.IsVirtual), new BoolToRunsetLabelColorValueConverter());
            if (WorkSpace.Instance.SourceControl == null || !WorkSpace.Instance.UserProfile.ShowSourceControlStatusIcon)
            {
                xRunSetUcLabel.xSourceControlIcon.Visibility = Visibility.Collapsed;
            }
            else
            {
                xRunSetUcLabel.xSourceControlIcon.Visibility = Visibility.Visible;
                BindingOperations.ClearBinding(xRunSetUcLabel.xSourceControlIcon, ImageMakerControl.ImageTypeProperty);
                BindingHandler.ObjFieldBinding(xRunSetUcLabel.xSourceControlIcon, ImageMakerControl.ImageTypeProperty, mRunSetConfig, nameof(RunSetConfig.SourceControlStatus), BindingMode.OneWay);
            }
            BindingOperations.ClearBinding(xRunSetUcLabel.xModifiedIcon, ImageMakerControl.ImageTypeProperty);
            BindingHandler.ObjFieldBinding(xRunSetUcLabel.xModifiedIcon, ImageMakerControl.ImageTypeProperty, mRunSetConfig, nameof(RunSetConfig.DirtyStatusImage), BindingMode.OneWay);

            UpdateDescription();
            xRunDescritpion.Init(mContext, mRunSetConfig, nameof(RunSetConfig.RunDescription));
            xExternalId.Init(mContext, mRunSetConfig, nameof(RunSetConfig.ExternalID));
            if (mSolutionCategoriesPage == null)
            {
                mSolutionCategoriesPage = new SolutionCategoriesPage();
                xCategoriesFrame.ClearAndSetContent(mSolutionCategoriesPage);
            }
            mSolutionCategoriesPage.Init(eSolutionCategoriesPageMode.ValuesSelection, mRunSetConfig.CategoriesDefinitions);
            PropertyChangedEventManager.AddHandler(source: mRunSetConfig, handler: RunSetConfig_PropertyChanged, propertyName: allProperties);
            CollectionChangedEventManager.AddHandler(source: mRunSetConfig.Tags, handler: RunSetTags_CollectionChanged);
        }

        private void mRunSetConfig_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender != null && sender is RunSetConfig senderRunset && senderRunset != mRunSetConfig)
            {
                PropertyChangedEventManager.RemoveHandler(senderRunset, mRunSetConfig_PropertyChanged, propertyName: allProperties);
                return;
            }
            if (string.Equals(nameof(RunSetConfig.IsVirtual), e.PropertyName))
            {
                Dispatcher.Invoke(() =>
                {
                    xRunSetUcLabel.xNameTextBlock.ToolTip = GetToolTipForRunSetLabel();
                });
            }
        }

        private string GetToolTipForRunSetLabel()
        {
            if (mRunSetConfig.IsVirtual)
            {
                return $"{mRunSetConfig.Name} (Virtual)";
            }
            else
            {
                return mRunSetConfig.Name;
            }
        }

        private void RunSetTags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateDescription();
        }

        private void UpdateDescription()
        {
            this.Dispatcher.Invoke(() =>
            {
                xDescriptionTextBlock.Text = string.Empty;
                TextBlockHelper xDescTextBlockHelper = new TextBlockHelper(xDescriptionTextBlock);
                SolidColorBrush foregroundColor = (SolidColorBrush)new BrushConverter().ConvertFromString((TryFindResource("$PrimaryColor_Black")).ToString());

                //description
                if (!string.IsNullOrEmpty(mRunSetConfig.Description))
                {
                    xDescTextBlockHelper.AddText("Description: " + mRunSetConfig.Description);
                    xDescTextBlockHelper.AddLineBreak();
                }

                //Tags
                if (mRunSetConfig.Tags.Count > 0)
                {
                    xDescTextBlockHelper.AddText(Ginger.General.GetTagsListAsString(mRunSetConfig.Tags));
                    xDescTextBlockHelper.AddLineBreak();
                }

                ////Target apps been tested in Run set
                //List<string> targetsList = new List<string>();
                //foreach(GingerRunner runner in mRunSetConfig.GingerRunners )
                //{                    
                //    foreach(IApplicationAgent appAgent in runner.ApplicationAgents)
                //    {
                //        if (targetsList.Contains(appAgent.AppName) == false)
                //            targetsList.Add(appAgent.AppName);
                //    }
                //}
                //string targetsLbl = "Target/s: ";
                //foreach (string appName in targetsList)
                //{
                //    targetsLbl += appName + ", ";
                //}
                //targetsLbl= targetsLbl.TrimEnd(new char[] { ' ', ',' });
                //xDescTextBlockHelper.AddText(targetsLbl);
            });
        }

        private void RunSetConfig_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RunSetConfig.Description))
            {
                UpdateDescription();
            }
            else if (e.PropertyName == nameof(RunSetConfig.IsRunning))
            {
                UpdateRunButtonIcon();
                UpdateReRunFailedButtonIcon();
            }
        }

        private void RunSetActions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                UpdateRunsetOperationsTabHeader();
            });
        }

        private RunSetConfig GetDefualtRunSetConfig()
        {
            try
            {
                if (WorkSpace.Instance.Solution == null)
                {
                    return null;
                }

                ObservableList<RunSetConfig> allRunsets = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>();

                //looking for last used Run Set
                if (WorkSpace.Instance.UserProfile.RecentRunset != null &&
                             WorkSpace.Instance.UserProfile.RecentRunset != Guid.Empty)
                {
                    RunSetConfig recentRunset = allRunsets.FirstOrDefault(runsets => runsets.Guid == WorkSpace.Instance.UserProfile.RecentRunset);
                    if (recentRunset != null)
                    {
                        return recentRunset;
                    }
                }

                //return first Run set in Solution
                if (allRunsets.Count > 0)
                {
                    return allRunsets[0];
                }

                //create new defualt run set
                RunSetConfig newRunSet = RunSetOperations.CreateNewRunset("Default " + GingerDicser.GetTermResValue(eTermResKey.RunSet));
                if (newRunSet != null)
                {
                    return newRunSet;
                }

                return null;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to load the recent " + GingerDicser.GetTermResValue(eTermResKey.RunSet) + " used", ex);
                return null;
            }
        }

        private void MoveRunnerLeft_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            Run.GingerExecutionEngine CGR = mCurrentSelectedRunner.ExecutorEngine;
            int Indx = mRunSetConfig.GingerRunners.IndexOf(CGR.GingerRunner);

            if (Indx > 0)
            {
                mRunSetConfig.GingerRunners.Move(Indx, Indx - 1);
            }
            else
            {
                return;
            }
        }

        private void MoveRunnerRight_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            Run.GingerExecutionEngine CGR = mCurrentSelectedRunner.ExecutorEngine;
            int Indx = mRunSetConfig.GingerRunners.IndexOf(CGR.GingerRunner);
            if (Indx < (mRunSetConfig.GingerRunners.Count - 1))
            {
                mRunSetConfig.GingerRunners.Move(Indx, Indx + 1);
            }
            else
            {
                return;
            }
        }

        private void RunsetExecutor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Handle Run Set change
            if (e.PropertyName == nameof(RunsetExecutor.RunSetConfig))
            {
                if (WorkSpace.Instance.RunsetExecutor.RunSetConfig == null || WorkSpace.Instance.RunsetExecutor.RunSetConfig.Equals(RunSetConfig) == false)
                {
                    if (!mSolutionWasChanged)//avoid the change if shifting solution
                    {
                        ResetLoadedRunSet(WorkSpace.Instance.RunsetExecutor.RunSetConfig);
                    }
                }
            }
        }

        private void WorkSpacePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Handle Solution change
            if (e.PropertyName == nameof(WorkSpace.Solution))
            {
                if (WorkSpace.Instance.Solution == null)
                {
                    mSolutionWasChanged = true;
                    //ToDO: Clear Run Set page
                    return;
                }
                else
                {
                    mSolutionWasChanged = true;
                }
            }
        }

        private void ResetLoadedRunSet(RunSetConfig runSetToSet = null)
        {
            //reset selection trees
            mBusFlowsSelectionPage = null;
            mRunSetsSelectionPage = null;

            //init BF's watcher
            mRunSetBusinessFlowWasChanged = false;
            SetBusinessFlowsChangesLisener();

            //load new run set
            if (runSetToSet == null)
            {
                runSetToSet = GetDefualtRunSetConfig();
                if (!WorkSpace.Instance.UserProfile.AutoLoadLastRunSet)
                {
                    LoadRunSetConfigBySelection(runSetToSet);
                    return;
                }
            }

            if (runSetToSet != null)
            {
                LoadRunSetConfig(runSetToSet);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, string.Format("No {0} found to load, please add {0}.", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                //TODO: hide all pages elements
            }
        }

        private void GingerRunnerHighlight(RunnerPage GRP, bool forceReload = false)
        {
            if (mCurrentSelectedRunner != null && GRP != null && mCurrentSelectedRunner.Equals(GRP) && !forceReload)
            {
                return;//the Runner is already selected
            }


            //de-highlight previous selected Ginger
            if (mCurrentSelectedRunner != null)
            {
                mCurrentSelectedRunner.xBorder.Visibility = System.Windows.Visibility.Collapsed;
                mCurrentSelectedRunner.xRunnerInfoSplitterBorder.Background = FindResource("$BackgroundColor_Black") as Brush;
                mCurrentSelectedRunner.xRunnerInfoSplitterBorder.Height = 1;
                if (!mCurrentSelectedRunner.ExecutorEngine.GingerRunner.Active)
                {
                    mCurrentSelectedRunner.xRunnerNameTxtBlock.Foreground = Brushes.Gray;
                }
                else
                {
                    mCurrentSelectedRunner.xRunnerNameTxtBlock.Foreground = FindResource("$BackgroundColor_Black") as Brush;
                }
            }

            //highlight the selected Ginger
            GRP.xBorder.Visibility = System.Windows.Visibility.Visible;
            GRP.xRunnerInfoSplitterBorder.Background = FindResource("$amdocsLogoLinarGradientBrush") as Brush;
            GRP.xRunnerInfoSplitterBorder.Height = 1;
            if (!GRP.ExecutorEngine.GingerRunner.Active)
            {
                GRP.xRunnerNameTxtBlock.Foreground = Brushes.Gray;
            }
            else
            {
                GRP.xRunnerNameTxtBlock.Foreground = FindResource("$SelectionColor_Pink") as Brush;
            }
            mCurrentSelectedRunner = GRP;

            mCurrentSelectedRunner.RunnerPageEvent -= RunnerPageEvent;
            mCurrentSelectedRunner.RunnerPageEvent += RunnerPageEvent;

            mCurrentSelectedRunner.RunnerPageListener.UpdateBusinessflowActivities -= UpdateBusinessflowActivities;
            mCurrentSelectedRunner.RunnerPageListener.UpdateBusinessflowActivities += UpdateBusinessflowActivities;

            WeakEventManager<RunnerPage, EventArgs>.RemoveHandler(source: mCurrentSelectedRunner, eventName: nameof(RunnerPage.CheckIfRunsetDirty), handler: mCurrentSelectedRunner_CheckIfRunsetDirty);
            WeakEventManager<RunnerPage, EventArgs>.AddHandler(source: mCurrentSelectedRunner, eventName: nameof(RunnerPage.CheckIfRunsetDirty), handler: mCurrentSelectedRunner_CheckIfRunsetDirty);

            UpdateRunnerTime();

            //set it as flow diagram current item
            List<FlowElement> fe = mFlowDiagram.GetAllFlowElements();
            foreach (FlowElement f in fe)
            {
                RunnerPage rp = (RunnerPage)f.GetCustomeShape().Content;
                if (rp != null && rp.ExecutorEngine.GingerRunner.Guid.Equals(GRP.ExecutorEngine.GingerRunner.Guid))
                {
                    mFlowDiagram.mCurrentFlowElem = f;
                    break;
                }
            }

            //set the runner items section
            InitRunnerExecutionDebugSection();

            if (mCurrentSelectedRunner != null)
            {
                mContext.Runner = mCurrentSelectedRunner.ExecutorEngine;
            }
            else
            {
                mContext.Runner = null;
            }
        }

        private void mCurrentSelectedRunner_CheckIfRunsetDirty(object sender, EventArgs e)
        {
            bool bIsRunsetDirty = mRunSetConfig != null && mRunSetConfig.DirtyStatus == eDirtyStatus.Modified;
            if (bIsRunsetDirty)
            {
                UserSelectionSaveOrUndoRunsetChanges();
            }
        }

        private void UpdateBusinessflowActivities(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (sender is BusinessFlow)
                {
                    if (mCurrentBusinessFlowRunnerItem == null)
                    {
                        return;
                    }
                    BusinessFlow changedBusinessflow = (BusinessFlow)sender;
                    if (mCurrentBusinessFlowRunnerItem.ItemObject == changedBusinessflow)
                    {
                        mCurrentBusinessFlowRunnerItem.LoadChildRunnerItems();//reloading activities to make sure include dynamically added/removed activities.
                        xActivitiesRunnerItemsListView.ItemsSource = mCurrentBusinessFlowRunnerItem.ChildItemPages;
                    }
                }
            });
        }

        private void InitRunnerExecutionDebugSection()
        {
            try
            {
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xRunnerNamelbl, Label.ContentProperty, mCurrentSelectedRunner.ExecutorEngine, nameof(GingerRunner.Name));

                xBusinessflowsRunnerItemsLoadingIcon.Visibility = Visibility.Visible;
                xBusinessflowsRunnerItemsListView.Visibility = Visibility.Collapsed;
                General.DoEvents();//for seeing the processing icon better to do with Async                

                //load needed Bf's and clear other runners BF's pages to save memory
                List<FlowElement> fe = mFlowDiagram.GetAllFlowElements();
                foreach (FlowElement flowElem in fe)
                {
                    if (flowElem == null)
                    {
                        continue;
                    }

                    RunnerPage rp = (RunnerPage)flowElem.GetCustomeShape().Content;
                    if (rp == null)
                    {
                        continue;
                    }

                    if (rp.ExecutorEngine.GingerRunner.Guid.Equals(mCurrentSelectedRunner.ExecutorEngine.GingerRunner.Guid))
                    {
                        //load BF's items
                        xBusinessflowsRunnerItemsListView.ItemsSource = rp.BusinessflowRunnerItems;
                    }
                    else
                    {
                        rp.ClearBusinessflowRunnerItems();
                    }
                }
                GC.Collect();//to help with memory free

                CollectionChangedEventManager.RemoveHandler(source: mCurrentSelectedRunner.ExecutorEngine.BusinessFlows, handler: BusinessFlows_CollectionChanged);
                CollectionChangedEventManager.AddHandler(source: mCurrentSelectedRunner.ExecutorEngine.BusinessFlows, handler: BusinessFlows_CollectionChanged);


                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xStatus, StatusItem.StatusProperty, mCurrentSelectedRunner.ExecutorEngine, nameof(GingerExecutionEngine.Status), BindingMode.OneWay);
            }
            finally
            {
                xBusinessflowsRunnerItemsLoadingIcon.Visibility = Visibility.Collapsed;
                xBusinessflowsRunnerItemsListView.Visibility = Visibility.Visible;
            }

            xBusinessflowName.Content = string.Format("{0} ({1})", GingerDicser.GetTermResValue(eTermResKey.BusinessFlows), mCurrentSelectedRunner.ExecutorEngine.BusinessFlows.Count);

            /// Avoid selecting the first BusinessFlow as default to avoid Activities loading for saving the Runset load time
            //if (xBusinessflowsRunnerItemsListView.Items.Count > 0)
            //{
            //    xBusinessflowsRunnerItemsListView.SelectedItem = mCurrentSelectedRunner.BusinessflowRunnerItems[0];
            //}
        }

        private void RunnerPageEvent(RunnerPageEventArgs EventArgs)
        {
            switch (EventArgs.EventType)
            {
                case RunnerPageEventArgs.eEventType.RemoveRunner:
                    removeRunner((GingerExecutionEngine)EventArgs.Object);
                    break;
                case RunnerPageEventArgs.eEventType.DuplicateRunner:
                    duplicateRunner((GingerExecutionEngine)EventArgs.Object);
                    break;
                case RunnerPageEventArgs.eEventType.ResetRunnerStatus:
                    ResetRunnerStatus((GingerExecutionEngine)EventArgs.Object);
                    break;
            }
        }
        private void ResetRunnerStatus(GingerExecutionEngine runner)
        {
            ResetRunset(runner);
        }

        private void ResetRunset(GingerExecutionEngine runner)
        {
            if (runner.Status == eRunStatus.Running)
            {
                return;
            }

            xRuntimeLbl.Content = "00:00:00";
        }

        internal void InitFlowDiagram()
        {
            if (mFlowDiagram == null)
            {
                mFlowDiagram = new FlowDiagramPage();
                mFlowDiagram.SetView(Brushes.Transparent, false, false);
                mFlowDiagram.SetHighLight = false;
                mFlowDiagram.BackGround = Brushes.Transparent;
                mFlowDiagram.ZoomPanelContainer.Visibility = Visibility.Collapsed;
                mFlowDiagram.ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                mFlowDiagram.Height = 300;
                mFlowDiagram.Ismovable = false;
                mFlowX = 0;
                mFlowY = 0;
                xRunnersCanvasFrame.ClearAndSetContent(mFlowDiagram);
            }
            else
            {
                mFlowDiagram.ClearAllFlowElement();
            }
        }



        internal void InitRunnerFlowElement(RunnerPage GRP, GingerExecutionEngine runner, int index = -1, bool ViewMode = false)
        {
            GRP.Init(runner, mContext, ViewMode);
            GRP.Tag = runner.GingerRunner.Guid;
            WeakEventManager<UIElement, MouseButtonEventArgs>.AddHandler(source: GRP, eventName: nameof(UIElement.MouseLeftButtonDown), handler: GRP_MouseLeftButtonDown);

            GRP.Width = 515;
            FlowElement RunnerFlowelement = new FlowElement(FlowElement.eElementType.CustomeShape, GRP, mFlowX, mFlowY, 600, 220)
            {
                OtherInfoVisibility = Visibility.Collapsed,
                Tag = GRP.Tag
            };

            if (mFlowDiagram.mCurrentFlowElem != null)
            {
                AddConnectorFlow(RunnerFlowelement);
            }

            mFlowDiagram.AddFlowElem(RunnerFlowelement, index);

            mFlowDiagram.mCurrentFlowElem = RunnerFlowelement;

            GRP.xBorder.Visibility = System.Windows.Visibility.Collapsed;
            GRP.xRunnerInfoSplitterBorder.Height = 1;
            GRP.xRunnerInfoSplitterBorder.Background = FindResource("$BackgroundColor_Black") as Brush;
            GRP.xRunnerNameTxtBlock.Foreground = FindResource("$BackgroundColor_Black") as Brush;
            mFlowX = mFlowX + 610;
        }

        private void AddConnectorFlow(FlowElement RunnerFlowelement)
        {
            FlowLink FL = new FlowLink(mFlowDiagram.mCurrentFlowElem, RunnerFlowelement, true)
            {
                LinkStyle = FlowLink.eLinkStyle.Arrow,
                SourcePosition = FlowLink.eFlowElementPosition.Right,
                Tag = RunnerFlowelement.Tag,
                DestinationPosition = FlowLink.eFlowElementPosition.Left,
                Margin = new Thickness(0, 0, mFlowX, 0)
            };

            BindingHandler.ObjFieldBinding(FL, FlowLink.VisibilityProperty, mRunSetConfig, nameof(RunSetConfig.RunModeParallel), bindingConvertor: new ReverseBooleanToVisibilityConverter(), System.Windows.Data.BindingMode.OneWay);
            mFlowDiagram.AddConnector(FL);
        }


        private void GRP_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GingerRunnerHighlight((RunnerPage)sender);
        }

        private readonly List<RunnerPage> runnerPages = [];

        private async Task<int> InitRunnersSection(bool runAsync = true, bool ViewMode = false)
        {
            this.Dispatcher.Invoke(() =>
            {
                // to check run mode of already created runset                
                SetEnvironmentsCombo();
                xRunnersCanvasFrame.Refresh();
                xRunnersCanvasFrame.NavigationService.Refresh();
                //Init Runner FlowDiagram            
                InitFlowDiagram();
            });

            System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
            st.Start();
            while (WorkSpace.Instance.AppSolutionAutoSave.WaitForAutoSave)
            {
                await Task.Delay(500);
                if (st.ElapsedMilliseconds > 60000)
                {
                    break;
                }
            }
            RunnerPage firstRunnerPage = null;

            int runnerPageIndex = 0;
            while (mRunSetConfig.GingerRunners.Count > runnerPages.Count)
            {
                runnerPages.Add(new RunnerPage(runnerItemEventHandler: RunnerItem_RunnerItemEvent));
            }

            foreach (GingerRunner GR in mRunSetConfig.GingerRunners.ToList())
            {
                GingerExecutionEngine GEE = new GingerExecutionEngine(GR);
                if (runAsync)
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            WorkSpace.Instance.RunsetExecutor.InitRunner(GR, GEE);
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Failed to Init Runner", ex);
                        }
                    });
                }
                else
                {
                    WorkSpace.Instance.RunsetExecutor.InitRunner(GR, GEE);
                }

                this.Dispatcher.Invoke(() =>
                {
                    RunnerPage runnerPage = runnerPages[runnerPageIndex++];
                    InitRunnerFlowElement(runnerPage, (GingerExecutionEngine)GR.Executor, mRunSetConfig.GingerRunners.IndexOf(GR), ViewMode);
                    if (firstRunnerPage == null)
                    {
                        firstRunnerPage = runnerPage;
                    }

                    runnerPage.RunnerPageEvent -= RunnerPageEvent;
                    runnerPage.RunnerPageEvent += RunnerPageEvent;
                    runnerPage.RunnerPageListener.UpdateBusinessflowActivities -= UpdateBusinessflowActivities;
                    runnerPage.RunnerPageListener.UpdateBusinessflowActivities += UpdateBusinessflowActivities;

                    PropertyChangedEventManager.RemoveHandler(source: GR, handler: Runner_PropertyChanged, propertyName: allProperties);
                    PropertyChangedEventManager.AddHandler(source: GR, handler: Runner_PropertyChanged, propertyName: allProperties);
                    CollectionChangedEventManager.RemoveHandler(source: GR.ApplicationAgents, handler: RunnerApplicationAgents_CollectionChanged);
                    CollectionChangedEventManager.AddHandler(source: GR.ApplicationAgents, handler: RunnerApplicationAgents_CollectionChanged);

                });
            }

            this.Dispatcher.Invoke(() =>
            {
                CollectionChangedEventManager.RemoveHandler(source: mRunSetConfig.GingerRunners, handler: Runners_CollectionChanged);
                CollectionChangedEventManager.AddHandler(source: mRunSetConfig.GingerRunners, handler: Runners_CollectionChanged);

                //highlight first Runner
                if (firstRunnerPage != null)
                {
                    GingerRunnerHighlight(firstRunnerPage, forceReload: true);
                }

                SetRunnersCombo();
                UpdateRunnersTabHeader();
                UpdateRunnersCanvasSize();
                WeakEventManager<RangeBase, RoutedPropertyChangedEventArgs<double>>.AddHandler(source: xZoomPanel.ZoomSliderContainer, eventName: nameof(RangeBase.ValueChanged), handler: ZoomSliderContainer_ValueChanged);

            });

            return 1;
        }

        private void RunnerApplicationAgents_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateDescription();
        }

        private void ZoomSliderContainer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            if ((mFlowDiagram.Canvas) == null)
            {
                return;  // will happen only at page load
            }

            // Set the Canvas scale based on ZoomSlider value
            ScaleTransform ST = new ScaleTransform(e.NewValue, e.NewValue);
            (mFlowDiagram.Canvas).LayoutTransform = ST;

            xZoomPanel.PercentLabel.Content = (int)(e.NewValue * 100) + "%";
        }
        private void SetBusinessFlowsChangesLisener()
        {
            if (WorkSpace.Instance.Solution != null)
            {
                mBusinessFlowsXmlsChangeWatcher = new FileSystemWatcher
                {
                    Path = WorkSpace.Instance.Solution.BusinessFlowsMainFolder,
                    Filter = "*.xml",
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                    IncludeSubdirectories = true
                };

                WeakEventManager<FileSystemWatcher, FileSystemEventArgs>.RemoveHandler(source: mBusinessFlowsXmlsChangeWatcher, eventName: nameof(FileSystemWatcher.Changed), handler: OnBusinessFlowsXmlsChange);
                WeakEventManager<FileSystemWatcher, FileSystemEventArgs>.AddHandler(source: mBusinessFlowsXmlsChangeWatcher, eventName: nameof(FileSystemWatcher.Changed), handler: OnBusinessFlowsXmlsChange);

                WeakEventManager<FileSystemWatcher, FileSystemEventArgs>.RemoveHandler(source: mBusinessFlowsXmlsChangeWatcher, eventName: nameof(FileSystemWatcher.Deleted), handler: OnBusinessFlowsXmlsChange);
                WeakEventManager<FileSystemWatcher, FileSystemEventArgs>.AddHandler(source: mBusinessFlowsXmlsChangeWatcher, eventName: nameof(FileSystemWatcher.Deleted), handler: OnBusinessFlowsXmlsChange);

                mBusinessFlowsXmlsChangeWatcher.EnableRaisingEvents = true;
            }
        }

        private void OnBusinessFlowsXmlsChange(object source, FileSystemEventArgs e)
        {
            try
            {
                if (!mRunSetBusinessFlowWasChanged)
                {
                    //check if changed BF is related to current Run Set bf's
                    Task.Run(() =>
                    {
                        if (mRunSetConfig != null)
                        {
                            Parallel.ForEach(mRunSetConfig.GingerRunners, Runner =>
                            {
                                Parallel.ForEach(Runner.Executor.BusinessFlows, businessFlow =>
                                {
                                    BusinessFlow originalBF = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<BusinessFlow>(businessFlow.Guid);
                                    if (originalBF != null && System.IO.Path.GetFullPath(originalBF.FileName) == System.IO.Path.GetFullPath(e.FullPath))
                                    {
                                        mRunSetBusinessFlowWasChanged = true;
                                    }
                                });
                            });
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while checking " + GingerDicser.GetTermResValue(eTermResKey.RunSet) + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " files change", ex);
            }
        }

        private void SetEnvironmentsCombo()
        {
            xRunsetEnvironmentCombo.ItemsSource = null;

            if (WorkSpace.Instance.Solution != null)
            {
                xRunsetEnvironmentCombo.ItemsSource = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>();
                xRunsetEnvironmentCombo.DisplayMemberPath = nameof(ProjEnvironment.Name);
                xRunsetEnvironmentCombo.SelectedValuePath = nameof(RepositoryItemBase.Guid);

                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xRunsetEnvironmentCombo, ComboBox.SelectedItemProperty, WorkSpace.Instance.RunsetExecutor, nameof(RunsetExecutor.RunsetExecutionEnvironment));

                //select last used environment
                if (xRunsetEnvironmentCombo.Items != null && xRunsetEnvironmentCombo.Items.Count > 0)
                {
                    if (xRunsetEnvironmentCombo.Items.Count > 1 && WorkSpace.Instance.UserProfile.RecentEnvironment != null && WorkSpace.Instance.UserProfile.RecentEnvironment != Guid.Empty)
                    {
                        foreach (object env in xRunsetEnvironmentCombo.Items)
                        {
                            if (((ProjEnvironment)env).Guid == WorkSpace.Instance.UserProfile.RecentEnvironment)
                            {
                                xRunsetEnvironmentCombo.SelectedIndex = xRunsetEnvironmentCombo.Items.IndexOf(env);
                                return;
                            }
                        }
                    }

                    //defualt selection
                    xRunsetEnvironmentCombo.SelectedIndex = 0;
                }
            }
        }



        private void SetRunnersCombo()
        {
            xRunnersCombo.ItemsSource = null;

            if (WorkSpace.Instance.Solution != null)
            {
                xRunnersCombo.ItemsSource = mRunSetConfig.GingerRunners;
                xRunnersCombo.DisplayMemberPath = nameof(GingerRunner.Name);
                xRunnersCombo.SelectedValuePath = nameof(GingerRunner.Guid);

                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xRunnersCombo, ComboBox.SelectedItemProperty, mRunSetConfig.GingerRunners, nameof(GingerRunner.Guid));
            }
        }

        public async void LoadRunSetConfig(RunSetConfig runSetConfig, bool runAsync = true, bool ViewMode = false)
        {
            try
            {
                //show current Run set UI
                xRunsetPageGrid.Visibility = Visibility.Visible;

                bool isSolutionSame =
                    mRunSetConfig != null && WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<RunSetConfig>(mRunSetConfig.Guid) != null;
                bool bIsRunsetDirty = mRunSetConfig != null && mRunSetConfig.DirtyStatus == eDirtyStatus.Modified && isSolutionSame;
                if (WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList != null)
                {
                    WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList.Clear();
                }

                if (bIsRunsetDirty && !IsCalledFromxUndoBtn)
                {
                    UserSelectionSaveOrUndoRunsetChanges();
                }
                this.Dispatcher.Invoke(() =>
                {
                    xRunSetLoadingPnl.Visibility = Visibility.Visible;
                    xRunsetPageGrid.Visibility = Visibility.Collapsed;
                    mRunSetConfig = runSetConfig;
                    CurrentItemToSave = mRunSetConfig;
                    mRunSetConfig.SaveBackup();

                    mRunSetConfig.StartDirtyTracking();
                    mRunSetConfig.AllowAutoSave = false;
                    WorkSpace.Instance.RunsetExecutor.RunSetConfig = RunSetConfig;

                    //Init Run Set Details Section
                    InitRunSetInfoSection();
                });

                //Init Runners Section  
                int res = 0;
                if (runAsync)
                {
                    res = await InitRunnersSection().ConfigureAwait(false);
                }
                else
                {
                    InitRunnersSection(false, ViewMode);
                }
                UpdateReRunFailedButtonIcon();
                this.Dispatcher.Invoke(() =>
                {
                    //Init Operations Section
                    InitOperationsSection();

                    // Init Runset Config Section
                    InitRunSetConfigurations();

                    //init Defect Opeing section
                    InitALMDefectsOpeningSection();

                    //Init Execution History Section
                    InitExecutionHistorySection();

                    WorkSpace.Instance.UserProfile.RecentRunset = mRunSetConfig.Guid;//to be loaded automatically next time
                });
            }
            finally
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (!mRunSetConfig.IsVirtual)
                    {
                        mRunSetConfig.AllowAutoSave = true;
                    }
                    xRunSetLoadingPnl.Visibility = Visibility.Collapsed;
                    xRunsetPageGrid.Visibility = Visibility.Visible;
                    if (xAddBusinessflowBtn.IsLoaded && mRunSetConfig != null)
                    {
                        General.DoEvents();
                        App.MainWindow.AddHelpLayoutToShow("RunsetPage_NewAnalyzerLocationHelp", xRunnersExecutionConfigBtn, "Click here to configure if Analyzer will be used and other Runners execution settings");
                        if (mRunSetConfig.GingerRunners.Count == 1 && mCurrentSelectedRunner != null && mCurrentSelectedRunner.ExecutorEngine.BusinessFlows.Count == 0)
                        {
                            App.MainWindow.AddHelpLayoutToShow("RunsetPage_AddRunnerBusinessFlowHelp", xAddBusinessflowBtn, "Click here to add Business Flows to Runner flow");
                        }
                    }
                });
            }
        }

        void InitOperationsSection()
        {
            if (mRunsetOperations == null)
            {
                mRunsetOperations = new RunsetOperationsPage(RunSetConfig);
                xRunsetOperationsTab.ClearAndSetContent(mRunsetOperations);
            }
            else
            {
                mRunsetOperations.Init(RunSetConfig);
            }

            CollectionChangedEventManager.AddHandler(source: RunSetConfig.RunSetActions, handler: RunSetActions_CollectionChanged);
            UpdateRunsetOperationsTabHeader();
        }



        private void InitExecutionHistorySection()
        {
            if (mRunSetsExecutionsPage == null)
            {
                mRunSetsExecutionsPage = new RunSetsExecutionsHistoryPage(RunSetsExecutionsHistoryPage.eExecutionHistoryLevel.SpecificRunSet, RunSetConfig);
                mExecutionSummary.ClearAndSetContent(mRunSetsExecutionsPage);
                CollectionChangedEventManager.AddHandler(source: mRunSetsExecutionsPage.ExecutionsHistoryList, handler: ExecutionsHistoryList_CollectionChanged);
            }
            else
            {
                mRunSetsExecutionsPage.RunsetConfig = mRunSetConfig;
                mRunSetsExecutionsPage.ReloadExecutionHistoryData();
            }

            UpdateRunsetExecutionHistoryTabHeader();
        }

        public void RunSetExecutionHistoryPage_LoadRunset(RunSetConfig runset)
        {
            LoadRunSetConfig(runset);
            RunTab.SelectedItem = xRunnersTab;
        }

        private void InitALMDefectsOpeningSection()
        {
            if (mRunSetsALMDefectsOpeningPage == null)
            {
                mRunSetsALMDefectsOpeningPage = new RunSetsALMDefectsOpeningPage();
                mALMDefectsOpening.ClearAndSetContent(mRunSetsALMDefectsOpeningPage);
                xALMDefectsOpening.IsEnabled = true;
            }
            else
            {
                mRunSetsALMDefectsOpeningPage.ReloadData();
                xALMDefectsOpening.IsEnabled = true;
            }

            UpdateRunsetALMDefectsOpeningTabHeader();
        }

        internal void SaveRunSetConfig()
        {
            try
            {
                mRunSetConfig.AllowAutoSave = false;
                Reporter.ToStatus(eStatusMsgKey.SaveItem, null, mRunSetConfig.Name, GingerDicser.GetTermResValue(eTermResKey.RunSet));
                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mRunSetConfig);
            }
            finally
            {
                mRunSetConfig.AllowAutoSave = true;
                Reporter.HideStatusMessage();
            }
        }

        internal void AddNewRunSetConfig()
        {
            RunSetConfig newRunSet = RunSetOperations.CreateNewRunset();
            if (newRunSet != null)
            {
                LoadRunSetConfig(newRunSet);
                return;
            }
            else
            {
                //failed to add the run set
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Failed to add new " + GingerDicser.GetTermResValue(eTermResKey.RunSet));
            }
        }

        internal void AddRunner(GingerRunner gingerRunner = null)
        {
            if (mRunSetConfig.GingerRunners.Count < 12)
            {
                int index = -1;
                int Count = mRunSetConfig.GingerRunners.Count;
                if (mCurrentSelectedRunner != null && mCurrentSelectedRunner.ExecutorEngine != null)
                {
                    index = mRunSetConfig.GingerRunners.IndexOf(mCurrentSelectedRunner.ExecutorEngine.GingerRunner) + 1;
                }
                GingerRunner newRunner = new GingerRunner();
                newRunner.Executor = new GingerExecutionEngine(newRunner);
                if (gingerRunner != null)
                {
                    newRunner = gingerRunner;
                }
                else
                {
                    newRunner.Name = "Runner " + ++Count;
                    //set unique name
                    while (mRunSetConfig.GingerRunners.FirstOrDefault(x => x.Name == newRunner.Name) != null)
                    {
                        newRunner.Name = "Runner " + ++Count;
                    }
                }
                newRunner.PropertyChanged -= Runner_PropertyChanged;
                newRunner.PropertyChanged += Runner_PropertyChanged;
                newRunner.ApplicationAgents.CollectionChanged -= RunnerApplicationAgents_CollectionChanged;
                newRunner.ApplicationAgents.CollectionChanged += RunnerApplicationAgents_CollectionChanged;
                WorkSpace.Instance.RunsetExecutor.InitRunner(newRunner, (GingerExecutionEngine)newRunner.Executor);
                if (Count != index && index > 0) //TODO : Check if need to add in between runner.
                {
                    mRunSetConfig.GingerRunners.Insert(index, newRunner);
                }
                else
                {
                    mRunSetConfig.GingerRunners.Add(newRunner);
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.RunnerLimitReached);
            }
        }

        public void UpdateRunsetOperationsTabHeader()
        {
            RunsetActionTextbox.Text = string.Format("Operations ({0})", RunSetConfig.RunSetActions.Count);
            //RunsetActionTextbox.Foreground = (Brush)Application.Current.Resources["$HighlightColor_Purple"];

        }
        private void UpdateRunnersTabHeader()
        {
            if (mRunSetConfig.GingerRunners.Count > 0)
            {
                RunnerTextblock.Text = string.Format("Runners ({0})", mRunSetConfig.GingerRunners.Count);
                //RunnerTextblock.Foreground = (Brush)Application.Current.Resources["$HighlightColor_Purple"];
            }
            else
            {
                RunnerTextblock.Text = "Runners";
                //RunnerTextblock.Foreground = (Brush)Application.Current.Resources["$HighlightColor_Purple"];
            }
        }

        private void analyzerRunset_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            AnalyzerPage AP = new AnalyzerPage();
            Run.RunSetConfig RSC = mRunSetConfig;
            if (RSC.ContainingFolder == "")
            {
                Reporter.ToUser(eUserMsgKey.AnalyzerSaveRunSet);
                return;
            }
            AP.Init(RSC, WorkSpace.Instance.Solution);
            AP.ShowAsWindow();

        }

        /// <summary>
        /// moved dirty tracking handling to common function
        /// </summary>
        private void UserSelectionSaveOrUndoRunsetChanges()
        {
            eUserMsgSelection userSelection = Reporter.ToUser(eUserMsgKey.SaveRunsetChanges, "Please save or reset RunSet changes");
            switch (userSelection)
            {
                case eUserMsgSelection.Yes:
                    xRunsetSaveBtn.DoClick();
                    return;
                case eUserMsgSelection.No:
                    CollectionChangedEventManager.RemoveHandler(source: mRunSetConfig.GingerRunners, handler: Runners_CollectionChanged);

                    if (Ginger.General.UndoChangesInRepositoryItem(mRunSetConfig, true, true, false))
                    {
                        mRunSetConfig.SaveBackup();
                        CollectionChangedEventManager.AddHandler(source: mRunSetConfig.GingerRunners, handler: Runners_CollectionChanged);
                    }
                    mRunSetConfig.DirtyStatus = eDirtyStatus.NoChange;
                    return;
                default:
                    //do nothing
                    return;
            }
        }

        private async void xRunRunsetBtn_Click(object sender, RoutedEventArgs e)
        {
            //WorkSpace.Instance.RunningInReRunMode = false;
            try
            {
                if (RunSetConfig.DirtyStatus.Equals(eDirtyStatus.Modified))
                {
                    UserSelectionSaveOrUndoRunsetChanges();
                }
                xRunsetSaveBtn.IsEnabled = false;
                IEnumerable<string> runnerNames = WorkSpace.Instance.RunsetExecutor.Runners.Where(x => x.Executor.BusinessFlows.Count == 0).Select(y => y.Name);

                if (runnerNames.Any())
                {
                    Reporter.ToUser(eUserMsgKey.StaticInfoMessage, $"{string.Join(", ", runnerNames)} is empty, please add {GingerDicser.GetTermResValue(eTermResKey.BusinessFlows)} to run.");
                    return;
                }
                UpdateRunButtonIcon(true);

                ResetALMDefectsSuggestions();


                //run analyzer
                if (mRunSetConfig.RunWithAnalyzer)
                {
                    int analyzeRes = await AnalyzeRunsetWithUI().ConfigureAwait(false);
                    if (analyzeRes == 1)
                    {
                        return;//cancel run because issues found
                    }
                }

                ResetRunners();
                //run
                //

                var result = await WorkSpace.Instance.RunsetExecutor.RunRunsetAsync().ConfigureAwait(false);


                // handling ALM Defects Opening

                if (WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList != null && WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList.Count > 0)
                {
                    ObservableList<ALMDefectProfile> ALMDefectProfiles = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ALMDefectProfile>();
                    if (ALMDefectProfiles != null && ALMDefectProfiles.Count > 0)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            InitALMDefectsOpeningSection();
                        });
                    }

                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Runset execution failed: ", ex);
            }
            finally
            {
                UpdateRunButtonIcon();
                UpdateReRunFailedButtonIcon();
            }
        }

        private async void xReRunFailedRunsetBtn_Click(object sender, RoutedEventArgs e)
        {
            RunsetExecutor ReRunRunsetExecutor = new RunsetExecutor();
            List<Guid> DeactivatedBfInstanceGuidList = [];
            if (WorkSpace.Instance.RunsetExecutor.Runners.Any(x => x.Executor.BusinessFlows.Any(y => y.RunStatus == eRunStatus.Failed)))
            {
                WorkSpace.Instance.RunsetExecutor.RunSetConfig.ReRunConfigurations.Active = true;
            }
            else
            {
                Reporter.ToLog(eLogLevel.INFO, "No record found to re run for current execution");
                return;
            }

            if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.ReRunConfigurations.Active)
            {
                try
                {
                    if (RunSetConfig.DirtyStatus.Equals(eDirtyStatus.Modified))
                    {
                        UserSelectionSaveOrUndoRunsetChanges();
                    }
                    xRunsetSaveBtn.IsEnabled = false;
                    IEnumerable<string> runnerNames = WorkSpace.Instance.RunsetExecutor.Runners.Where(x => x.Executor.BusinessFlows.Count == 0).Select(y => y.Name);

                    if (runnerNames.Any())
                    {
                        Reporter.ToUser(eUserMsgKey.StaticInfoMessage, $"{string.Join(", ", runnerNames)} is empty, please add {GingerDicser.GetTermResValue(eTermResKey.BusinessFlows)} to run.");
                        return;
                    }
                    UpdateReRunFailedButtonIcon(true);
                    ResetALMDefectsSuggestions();


                    //run analyzer
                    if (mRunSetConfig.RunWithAnalyzer)
                    {
                        int analyzeRes = await AnalyzeRunsetWithUI().ConfigureAwait(false);
                        if (analyzeRes == 1)
                        {
                            return;//cancel run because issues found
                        }
                    }

                    var result = await WorkSpace.Instance.RunsetExecutor.RunRunsetAsync().ConfigureAwait(false);

                    // handling ALM Defects Opening

                    if (WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList != null && WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList.Count > 0)
                    {
                        ObservableList<ALMDefectProfile> ALMDefectProfiles = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ALMDefectProfile>();
                        if (ALMDefectProfiles != null && ALMDefectProfiles.Count > 0)
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                InitALMDefectsOpeningSection();
                            });
                        }

                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Runset execution failed during re run failed flows ", ex);
                }
                finally
                {
                    UpdateReRunFailedButtonIcon();
                }
            }
            else
            {
                return;
            }
        }

        private void ResetRunners()
        {
            this.Dispatcher.Invoke(() =>
            {
                foreach (FlowElement f in mFlowDiagram.GetAllFlowElements())
                {
                    RunnerPage rp = (RunnerPage)f.GetCustomeShape().Content;
                    if (rp != null)
                    {
                        rp.ResetRunnerPage();
                    }
                }

            });
        }

        private async void xContinueRunsetBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateRunButtonIcon(true);
                if (RunSetConfig.GingerRunners.FirstOrDefault(x => x.Status == eRunStatus.Stopped) == null)
                {
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "There are no Stopped Runners to Continue.");
                    return;
                }

                //run analyzer
                if (mRunSetConfig.RunWithAnalyzer)
                {
                    int analyzeRes = await AnalyzeRunsetWithUI().ConfigureAwait(false);
                    if (analyzeRes == 1)
                    {
                        return;//cancel run because issues found
                    }
                }

                //continue run            
                await WorkSpace.Instance.RunsetExecutor.RunRunsetAsync(true);//doing continue run
            }
            finally
            {
                UpdateRunButtonIcon();
            }
        }

        public async Task<int> AnalyzeRunsetWithUI()
        {
            try
            {
                AnalyzerPage analyzerPage = null;
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    Reporter.ToStatus(eStatusMsgKey.AnalyzerIsAnalyzing, null, mRunSetConfig.Name, GingerDicser.GetTermResValue(eTermResKey.RunSet));
                    analyzerPage = new AnalyzerPage();
                    analyzerPage.Init(mRunSetConfig, WorkSpace.Instance.Solution);
                });

                await analyzerPage.AnalyzeWithoutUI();

                if (analyzerPage.TotalHighAndCriticalIssues > 0)
                {
                    Reporter.ToUser(eUserMsgKey.AnalyzerFoundIssues);
                    analyzerPage.ShowAsWindow();
                    return 1;
                }
            }
            finally
            {
                Reporter.HideStatusMessage();
            }
            return 0;
        }

        private void xResetRunsetBtn_Click(object sender, RoutedEventArgs e)
        {
            CleanAndUpdateRunsetStats();
            UpdateReRunFailedButtonIcon();
            UpdateRunButtonIcon();
        }

        private void CleanAndUpdateRunsetStats()
        {
            ResetALMDefectsSuggestions();

            foreach (GingerRunner runner in mRunSetConfig.GingerRunners)
            {
                if (runner.Executor.IsRunning == false)
                {
                    runner.Executor.ResetRunnerExecutionDetails();
                    RunsetExecutor.ClearAndResetVirtualAgents(WorkSpace.Instance.RunsetExecutor.RunSetConfig, (GingerExecutionEngine)runner.Executor);
                }
            }

            //update RunnersPages stats
            List<FlowElement> fe = mFlowDiagram.GetAllFlowElements();
            foreach (FlowElement f in fe)
            {
                ((RunnerPage)f.GetCustomeShape().Content).UpdateExecutionStats();
                ((RunnerPage)f.GetCustomeShape().Content).xruntime.Content = "00:00:00";
                ((RunnerPage)f.GetCustomeShape().Content).ExecutorEngine.RunnerExecutionWatch.runWatch.Reset();
            }
            xRuntimeLbl.Content = "00:00:00";
        }

        private void xStopRunsetBtn_Click(object sender, RoutedEventArgs e)
        {
            if (RunSetConfig.GingerRunners.FirstOrDefault(x => x.Executor.IsRunning == true) == null)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "There are no Running Runners to Stop.");
                return;
            }
            xStopRunsetBtn.ButtonText = "Stopping...";
            xStopRunsetBtn.ButtonImageType = eImageType.Running;
            xStopRunsetBtn.IsEnabled = false;

            WorkSpace.Instance.RunsetExecutor.StopRun();//stops only running runners  
            xRunsetSaveBtn.IsEnabled = true;
        }



        private void xRunSetChange_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            if (mRunSetsSelectionPage == null)
            {
                RunSetFolderTreeItem runSetsRootfolder = new RunSetFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<RunSetConfig>());
                mRunSetsSelectionPage = new SingleItemTreeViewSelectionPage(GingerDicser.GetTermResValue(eTermResKey.RunSets), eImageType.RunSet, runSetsRootfolder, SingleItemTreeViewSelectionPage.eItemSelectionType.Single, true);
            }

            List<object> selectedRunSet = mRunSetsSelectionPage.ShowAsWindow();
            if (selectedRunSet != null && selectedRunSet.Count > 0)
            {
                RunSetConfig selectedRunset = (RunSetConfig)selectedRunSet[0];
                if (mRunSetConfig.Equals(selectedRunset) == false)
                {
                    //change loaded run set
                    LoadRunSetConfig(selectedRunset);
                }
            }
        }

        private void addRunner_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            AddRunner();

            if (mRunSetConfig.GingerRunners.Count == 2)
            {
                App.MainWindow.AddHelpLayoutToShow("RunsetPage_RunnersParallelSeqHelp2", xRunnersExecutionConfigBtn, "Click here to set if Runners will run in parallel or sequential order plus extra configurations");
            }
        }

        private void clearAllRunner_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            if (mRunSetConfig.GingerRunners.Count > 0)
            {
                if (Reporter.ToUser(eUserMsgKey.DeleteRunners) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                {
                    mRunSetConfig.GingerRunners.Clear();
                    mFlowDiagram.ClearAllFlowElement();
                    AddRunner();
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemToDelete);
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((TabItem)RunTab.SelectedItem != null)
            {
                HideAllborders();
                if (((TabItem)RunTab.SelectedItem).Name == xRunnersTab.Name)
                {
                    RunnerBorder.BorderBrush = FindResource("$amdocsLogoLinarGradientBrush") as Brush;
                }
                else if (((TabItem)RunTab.SelectedItem).Name == xOperationsTab.Name)
                {
                    OperationsBorder.BorderBrush = FindResource("$amdocsLogoLinarGradientBrush") as Brush;
                }
                else if (((TabItem)RunTab.SelectedItem).Name == xConfigurationTab.Name)
                {
                    ConfigurationBorder.BorderBrush = FindResource("$amdocsLogoLinarGradientBrush") as Brush;
                }
                else if (((TabItem)RunTab.SelectedItem).Name == xALMDefectsOpening.Name)
                {
                    ALMDefectsBorder.BorderBrush = FindResource("$amdocsLogoLinarGradientBrush") as Brush;
                }
                else if (((TabItem)RunTab.SelectedItem).Name == xExecution_SummaryTab.Name)
                {
                    ExecutionBorder.BorderBrush = FindResource("$amdocsLogoLinarGradientBrush") as Brush;


                }
            }
        }

        private bool AssignGraphQLObjectEndPoint()
        {
            try
            {
                string endPoint = GingerRemoteExecutionUtils.GetReportDataServiceUrl();
                if (!string.IsNullOrEmpty(endPoint))
                {
                    graphQlClient = new GraphQlClient($"{endPoint}api/graphql");
                    executionReportGraphQLClient = new ExecutionReportGraphQLClient(graphQlClient);
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error occurred while connecting remote.", ex);
                return false;

            }

        }

        private bool ValidateRemoteConfiguration()
        {
            ExecutionLoggerConfiguration execLoggerConfig = WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.FirstOrDefault(c => c.IsSelected);
            if (execLoggerConfig.PublishLogToCentralDB == ExecutionLoggerConfiguration.ePublishToCentralDB.Yes && AssignGraphQLObjectEndPoint())
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        async Task GenerateHTMLReportFromRemote()
        {

            try
            {
                response = executionReportGraphQLClient.ExecuteReportQuery(1, WorkSpace.Instance.Solution.Guid, WorkSpace.Instance.RunsetExecutor.RunSetConfig.Guid);
                data = await response;
                var executionId = data.Data.Runsets.Nodes[0].ExecutionId.ToString();
                if (!string.IsNullOrEmpty(executionId))
                {
                    new GingerRemoteExecutionUtils().GenerateHTMLReport(executionId);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Execution data not found to remote\n Loading local data.", ex);
                GenerateHTMLReportFromLocal();
            }

        }
        private void xRunsetReportBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            if (ValidateRemoteConfiguration())
            {
                GenerateHTMLReportFromRemote();
            }
            else
            {
                GenerateHTMLReportFromLocal();
            }

        }

        private void GenerateHTMLReportFromLocal()
        {
            if (WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
            {
                WebReportGenerator webReporterRunner = new WebReportGenerator();
                webReporterRunner.RunNewHtmlReport(string.Empty);
            }
            else if (WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.TextFile)
            {
                if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder != null)
                {
                    ExecutionLoggerConfiguration _selectedExecutionLoggerConfiguration = WorkSpace.Instance.Solution.LoggerConfigurations;
                    HTMLReportsConfiguration currentConf = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.FirstOrDefault(x => (x.IsSelected == true));

                    string reportsResultFolder = string.Empty;
                    if (!_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled)
                    {
                        Reporter.ToUser(eUserMsgKey.ExecutionsResultsProdIsNotOn);
                        return;
                    }
                    if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.RunsetExecLoggerPopulated)
                    {
                        string runSetFolder = WorkSpace.Instance.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder;
                        reportsResultFolder = Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(runSetFolder), false, null, null);
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKey.ExecutionsResultsNotExists);
                        return;
                    }
                    if (reportsResultFolder == string.Empty)
                    {
                        Reporter.ToUser(eUserMsgKey.AutomationTabExecResultsNotExists);
                        return;
                    }
                    else
                    {
                        foreach (string txt_file in System.IO.Directory.GetFiles(reportsResultFolder))
                        {
                            string fileName = System.IO.Path.GetFileName(txt_file);
                            if (fileName.Contains(".html"))
                            {
                                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = reportsResultFolder, UseShellExecute = true });
                                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = reportsResultFolder + "\\" + fileName, UseShellExecute = true });
                            }
                        }
                    }
                }
                else
                {
                    GingerExecutionEngine gr = new GingerExecutionEngine(new GingerRunner());
                    gr.ExecutionLoggerManager.GenerateRunSetOfflineReport();
                }

            }

        }
        private void RunClientApp(string json, string clientAppFolderPath)
        {
            try
            {
                string taskCommand = $"{Path.Combine(clientAppFolderPath, "index.html")} --allow-file-access-from-files";
                System.IO.File.WriteAllText(Path.Combine(clientAppFolderPath, "assets\\Execution_Data\\executiondata.json"), json); //TODO - Replace with the real location under Ginger installation
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = "chrome", Arguments = taskCommand, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("RunClientApp Error - " + ex.Message);
            }
        }

        private void DeleteFoldersData(string clientAppFolderPath)
        {
            DirectoryInfo dir = new DirectoryInfo(clientAppFolderPath);

            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.Delete();
            }
        }

        //TODO move it to utils class
        private void PopulateMissingFields(LiteDbRunSet liteDbRunSet, string clientAppPath)
        {
            string imageFolderPath = Path.Combine(clientAppPath, "assets", "screenshots");

            int totalRunners = liteDbRunSet.RunnersColl.Count;
            int totalPassed = liteDbRunSet.RunnersColl.Count(runner => runner.RunStatus == eRunStatus.Passed.ToString());
            int totalExecuted = totalRunners - liteDbRunSet.RunnersColl.Count(runner => runner.RunStatus == eRunStatus.Pending.ToString() || runner.RunStatus == eRunStatus.Skipped.ToString() || runner.RunStatus == eRunStatus.Blocked.ToString());

            liteDbRunSet.ExecutionRate = (totalExecuted * 100 / totalRunners).ToString();
            liteDbRunSet.PassRate = (totalPassed * 100 / totalRunners).ToString();

            foreach (LiteDbRunner liteDbRunner in liteDbRunSet.RunnersColl)
            {

                int totalBFs = liteDbRunner.BusinessFlowsColl.Count;
                int totalPassedBFs = liteDbRunner.BusinessFlowsColl.Count(bf => bf.RunStatus == eRunStatus.Passed.ToString());
                int totalExecutedBFs = totalBFs - liteDbRunner.BusinessFlowsColl.Count(bf => bf.RunStatus == eRunStatus.Pending.ToString() || bf.RunStatus == eRunStatus.Skipped.ToString() || bf.RunStatus == eRunStatus.Blocked.ToString());

                liteDbRunner.ExecutionRate = (totalExecutedBFs * 100 / totalBFs).ToString();
                liteDbRunner.PassRate = (totalPassedBFs * 100 / totalExecutedBFs).ToString();

                foreach (LiteDbBusinessFlow liteDbBusinessFlow in liteDbRunner.BusinessFlowsColl)
                {
                    int totalActivities = liteDbBusinessFlow.ActivitiesColl.Count;
                    int totalPassedActivities = liteDbBusinessFlow.ActivitiesColl.Count(ac => ac.RunStatus == eRunStatus.Passed.ToString());
                    int totalExecutedActivities = totalActivities - liteDbBusinessFlow.ActivitiesColl.Count(ac => ac.RunStatus == eRunStatus.Pending.ToString() || ac.RunStatus == eRunStatus.Skipped.ToString() || ac.RunStatus == eRunStatus.Blocked.ToString());

                    liteDbBusinessFlow.ExecutionRate = (totalExecutedActivities * 100 / totalActivities).ToString();
                    liteDbBusinessFlow.PassRate = (totalPassedActivities * 100 / totalExecutedActivities).ToString();

                    foreach (LiteDbActivity liteDbActivity in liteDbBusinessFlow.ActivitiesColl)
                    {
                        int totalActions = liteDbActivity.ActionsColl.Count;
                        int totalPassedActions = liteDbActivity.ActionsColl.Count(ac => ac.RunStatus == eRunStatus.Passed.ToString());
                        int totalExecutedActions = totalActions - liteDbActivity.ActionsColl.Count(ac => ac.RunStatus == eRunStatus.Pending.ToString() || ac.RunStatus == eRunStatus.Skipped.ToString() || ac.RunStatus == eRunStatus.Blocked.ToString());

                        liteDbActivity.ExecutionRate = (totalExecutedActions * 100 / totalActions).ToString();
                        liteDbActivity.PassRate = (totalPassedActions * 100 / totalExecutedActions).ToString();

                        foreach (LiteDbAction liteDbAction in liteDbActivity.ActionsColl)
                        {
                            List<string> newScreenShotsList = [];
                            foreach (string screenshot in liteDbAction.ScreenShots)
                            {
                                string fileName = Path.GetFileName(screenshot);
                                string newScreenshotPath = Path.Combine(imageFolderPath, fileName);
                                System.IO.File.Copy(screenshot, newScreenshotPath, true); //TODO - Replace with the real location under Ginger installation
                                newScreenShotsList.Add(fileName);
                            }
                            liteDbAction.ScreenShots = newScreenShotsList;
                        }
                    }

                }
            }
        }

        private void HideAllborders()
        {
            RunnerBorder.BorderBrush = null;
            OperationsBorder.BorderBrush = null;
            ConfigurationBorder.BorderBrush = null;
            ExecutionBorder.BorderBrush = null;
            ALMDefectsBorder.BorderBrush = null;
        }
        private void xRunsetSaveBtn_Click(object sender, RoutedEventArgs e)
        {

            if (mRunSetConfig.GingerRunners.Any(x => x.Status == eRunStatus.Stopped) && Reporter.ToUser(eUserMsgKey.SaveRunsetChangesWarn) == eUserMsgSelection.Yes)
            {
                CleanAndUpdateRunsetStats();
                SaveRunSetConfig();
            }
            else if (!mRunSetConfig.GingerRunners.Any(x => x.Status == eRunStatus.Stopped))
            {
                SaveRunSetConfig();
            }

        }


        private void xRunnersExecutionConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            RunsetRunnersConfigPage config = new RunsetRunnersConfigPage(mRunSetConfig);
            config.ShowAsWindow();
        }

        private void xBusinessflowListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                ListView view = sender as ListView;
                view.ScrollIntoView(view.SelectedItem);
            });

            foreach (RunnerItemPage currentitem in xBusinessflowsRunnerItemsListView.Items)
            {
                if (!((BusinessFlow)currentitem.ItemObject).Active)
                {
                    currentitem.xItemName.Foreground = Brushes.Gray;
                }
                else
                {
                    currentitem.xItemName.Foreground = FindResource("$BackgroundColor_Black") as Brush;
                }
            }

            if (e.RemovedItems != null && e.RemovedItems.Count != 0)
            {
                RunnerItemPage previousBusinessFlowPage = (RunnerItemPage)e.RemovedItems[0];
                previousBusinessFlowPage.ClearItemChilds();
                //GC.Collect();
            }

            if (mCurrentBusinessFlowRunnerItem != null)
            {
                try
                {
                    xActivitiesRunnerItemsLoadingIcon.Visibility = Visibility.Visible;
                    xActivitiesRunnerItemsListView.Visibility = Visibility.Collapsed;
                    General.DoEvents();//for seeing the processing icon better to do with Async

                    // added if condition for the Application to throw an error if the mCurrentBusinessFlowRunnerItem is null
                    if (mCurrentBusinessFlowRunnerItem != null)
                    {
                        xActivitiesRunnerItemsListView.ItemsSource = mCurrentBusinessFlowRunnerItem.ChildItemPages;
                    }
                }
                finally
                {
                    xActivitiesRunnerItemsLoadingIcon.Visibility = Visibility.Collapsed;
                    xActivitiesRunnerItemsListView.Visibility = Visibility.Visible;
                }
                if (!((BusinessFlow)mCurrentBusinessFlowRunnerItem.ItemObject).Active)
                {
                    mCurrentBusinessFlowRunnerItem.xItemName.Foreground = Brushes.Gray;
                }
                else
                {
                    mCurrentBusinessFlowRunnerItem.xItemName.Foreground = FindResource("$SelectionColor_Pink") as Brush;
                }

                mContext.BusinessFlow = (BusinessFlow)mCurrentBusinessFlowRunnerItem.ItemObject;
            }
            else
            {
                xActivitiesRunnerItemsListView.ItemsSource = null;
                mContext.BusinessFlow = null;
            }
        }

        private void xActivitiesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                ListView view = sender as ListView;
                view.ScrollIntoView(view.SelectedItem);
            });


            if (e.RemovedItems != null && e.RemovedItems.Count != 0)
            {
                RunnerItemPage previousActivityPage = (RunnerItemPage)e.RemovedItems[0];
                previousActivityPage.ClearItemChilds();
                //GC.Collect();
            }

            foreach (RunnerItemPage currentitem in xActivitiesRunnerItemsListView.Items)
            {
                currentitem.xItemName.Foreground = FindResource("$BackgroundColor_Black") as Brush;
            }
            if (mCurrentActivityRunnerItem != null)
            {
                try
                {
                    xActionsRunnerItemsLoadingIcon.Visibility = Visibility.Visible;
                    xActionsRunnerItemsListView.Visibility = Visibility.Collapsed;
                    General.DoEvents();//for seeing the processing icon better to it with Async
                    //load items
                    xActionsRunnerItemsListView.ItemsSource = mCurrentActivityRunnerItem.ChildItemPages;
                }
                finally
                {
                    xActionsRunnerItemsLoadingIcon.Visibility = Visibility.Collapsed;
                    xActionsRunnerItemsListView.Visibility = Visibility.Visible;
                }

                mCurrentActivityRunnerItem.xItemName.Foreground = FindResource("$SelectionColor_Pink") as Brush;

                mCurrentActivityRunnerItem.Context.Activity = (Activity)mCurrentActivityRunnerItem.ItemObject;
                mContext.Activity = (Activity)mCurrentActivityRunnerItem.ItemObject;
            }
            else
            {
                xActionsRunnerItemsListView.ItemsSource = null;
                mContext.Activity = null;
            }
            xActionsName.Content = "Actions (" + xActionsRunnerItemsListView.Items.Count + ")";
            SetHeighlightActionRunnerItem();
        }
        private void SetHeighlightActionRunnerItem()
        {
            this.Dispatcher.Invoke(() =>
            {
                ListView view = xActionsRunnerItemsListView;
                view.ScrollIntoView(view.SelectedItem);
            });

            foreach (RunnerItemPage currentitem in xActionsRunnerItemsListView.Items)
            {
                currentitem.xItemName.Foreground = FindResource("$BackgroundColor_Black") as Brush;
            }
            //Need it for first time.
            if (xActionsRunnerItemsListView.SelectedItem == null)
            {
                xActionsRunnerItemsListView.SelectedIndex = 0;
            }
            if (mCurrentActionRunnerItem != null)
            {
                mCurrentActionRunnerItem.xItemName.Foreground = FindResource("$SelectionColor_Pink") as Brush;
            }
        }
        private void xActionsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetHeighlightActionRunnerItem();
        }

        //private bool CheckCurrentRunnerIsNotRuning()
        //{
        //    if (mCurrentSelectedRunner != null && mCurrentSelectedRunner.Runner != null)
        //    {
        //        if (mCurrentSelectedRunner.Runner.Status == eRunStatus.Running)
        //        {
        //            Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Please wait for Runner to complete run.");
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        private void xaddBusinessflow_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            BusinessFlowsFolderTreeItem bfsFolder;

            bfsFolder = new BusinessFlowsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<BusinessFlow>());
            mBusFlowsSelectionPage = new SingleItemTreeViewSelectionPage(GingerDicser.GetTermResValue(eTermResKey.BusinessFlows), eImageType.BusinessFlow, bfsFolder, SingleItemTreeViewSelectionPage.eItemSelectionType.MultiStayOpenOnDoubleClick, false);
            WeakEventManager<SingleItemTreeViewSelectionPage, SelectionTreeEventArgs>.AddHandler(source: mBusFlowsSelectionPage, eventName: nameof(SingleItemTreeViewSelectionPage.SelectionDone), handler: MBusFlowsSelectionPage_SelectionDone);

            List<object> selectedBfs = mBusFlowsSelectionPage.ShowAsWindow();
            AddSelectedBuinessFlows(selectedBfs);

            if (mRunSetConfig.GingerRunners.Count == 1 && mCurrentSelectedRunner.ExecutorEngine.BusinessFlows.Count == 1)
            {
                App.MainWindow.AddHelpLayoutToShow("RunsetPage_AddRunsetOperationsHelp", xOperationsTab, "Use 'Operations' tab for adding pre/post execution operations like getting execution report via e-mail");
            }
        }

        private void AddSelectedBuinessFlows(List<object> selectedBfs)
        {
            if (selectedBfs != null && selectedBfs.Count > 0)
            {
                foreach (BusinessFlow bf in selectedBfs)
                {
                    BusinessFlow bfToAdd = (BusinessFlow)bf.CreateCopy(false);
                    bfToAdd.ContainingFolder = bf.ContainingFolder;
                    bfToAdd.Active = true;
                    bfToAdd.Reset();
                    bfToAdd.InstanceGuid = Guid.NewGuid();
                    mCurrentSelectedRunner.AddBuinessFlowToRunner(bfToAdd, xBusinessflowsRunnerItemsListView.SelectedIndex + 1);
                }
            }
        }

        private void MBusFlowsSelectionPage_SelectionDone(object sender, SelectionTreeEventArgs e)
        {
            AddSelectedBuinessFlows(e.SelectedItems);
        }

        private void clearAllBusinessflow_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            if (mCurrentSelectedRunner.ExecutorEngine.BusinessFlows.Count > 0)
            {
                if (Reporter.ToUser(eUserMsgKey.DeleteBusinessflows) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                {
                    mCurrentSelectedRunner.ExecutorEngine.BusinessFlows.Clear();
                    mCurrentSelectedRunner.BusinessflowRunnerItems.Clear();
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemToDelete);
            }
        }


        private void xActivitiesListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (mCurrentActivityRunnerItem != null)
            {
                viewActivity((Activity)(mCurrentActivityRunnerItem).ItemObject);
            }
        }

        private void xActionsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (mCurrentActionRunnerItem != null)
            {
                viewAction((Act)(mCurrentActionRunnerItem).ItemObject);
            }
        }

        public void viewBusinessflow(BusinessFlow businessFlow)
        {
            GingerWPF.BusinessFlowsLib.BusinessFlowViewPage w = new GingerWPF.BusinessFlowsLib.BusinessFlowViewPage(businessFlow, new Context(), General.eRIPageViewMode.View)
            {
                Width = 1000,
                Height = 800
            };
            w.ShowAsWindow();
        }
        public void viewBusinessflowConfiguration(BusinessFlow businessFlow)
        {
            General.eRIPageViewMode viewMode;
            if (mEditMode == eEditMode.View)
            {
                viewMode = General.eRIPageViewMode.View;
            }
            else
            {
                viewMode = General.eRIPageViewMode.Standalone;
            }
            BusinessFlowRunConfigurationsPage varsPage = new BusinessFlowRunConfigurationsPage(mCurrentSelectedRunner.ExecutorEngine.GingerRunner, businessFlow, viewMode);
            varsPage.EventRaiseVariableEdit += viewBusinessflowConfiguration_RaiseVariableEdit;
            varsPage.ShowAsWindow();
        }

        public void viewBusinessflowConfiguration_RaiseVariableEdit(object sender, EventArgs e)
        {
            mRunSetConfig.DirtyStatus = eDirtyStatus.Modified;
        }

        public void viewActivity(Activity activitytoView)
        {
            Activity ac = activitytoView;
            GingerWPF.BusinessFlowsLib.ActivityPage w = new GingerWPF.BusinessFlowsLib.ActivityPage(ac, new Context() { BusinessFlow = mCurrentBusinessFlowRunnerItemObject, Activity = ac, Environment = mContext.Environment }, General.eRIPageViewMode.View);
            mContext.BusinessFlow.CurrentActivity = activitytoView;
            w.ShowAsWindow();
        }

        public void viewAction(Act actiontoView)
        {
            mContext.BusinessFlow.CurrentActivity = (Activity)(mCurrentActivityRunnerItem).ItemObject;
            Act act = actiontoView;
            ActionEditPage w = new ActionEditPage(act, General.eRIPageViewMode.View, mCurrentBusinessFlowRunnerItemObject, mCurrentActivityRunnerItemObject);

            w.ShowAsWindow(windowStyle: eWindowShowStyle.Dialog);
        }

        private void xremoveBusinessflow_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            if (mCurrentBusinessFlowRunnerItem != null)
            {
                if (Reporter.ToUser(eUserMsgKey.DeleteBusinessflow) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                {
                    BusinessFlow bff = (BusinessFlow)(mCurrentBusinessFlowRunnerItem).ItemObject;
                    mCurrentSelectedRunner.ExecutorEngine.BusinessFlows.Remove(bff);
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.SelectItemToDelete);
            }
        }

        private void removeRunner(GingerExecutionEngine runner)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            if (mRunSetConfig.GingerRunners.Count == 1)
            {
                Reporter.ToUser(eUserMsgKey.CantDeleteRunner);
                return;
            }
            if (runner != null)
            {
                if (Reporter.ToUser(eUserMsgKey.DeleteRunner) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                {
                    int index = mRunSetConfig.GingerRunners.IndexOf(runner.GingerRunner);
                    List<FlowElement> fe = mFlowDiagram.GetAllFlowElements();
                    mRunSetConfig.GingerRunners.Remove(runner.GingerRunner);
                    int count = mRunSetConfig.GingerRunners.Count;
                    if (index > 0 && count > 0)
                    {
                        GingerRunnerHighlight(((RunnerPage)fe[index - 1].GetCustomeShape().Content));
                        mFlowDiagram.mCurrentFlowElem = fe[index - 1];
                    }
                    else if (index == 0 && count > 0)
                    {
                        GingerRunnerHighlight(((RunnerPage)fe[index + 1].GetCustomeShape().Content));
                        mFlowDiagram.mCurrentFlowElem = fe[index + 1];
                    }
                    SetComboRunnerInitialView();
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.SelectItemToDelete);
            }
        }

        private void xmoveupBusinessflow_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            if (mCurrentBusinessFlowRunnerItem != null)
            {
                BusinessFlow bf = (BusinessFlow)mCurrentBusinessFlowRunnerItem.ItemObject;
                int Indx = mCurrentSelectedRunner.ExecutorEngine.BusinessFlows.IndexOf(bf);

                if (Indx > 0)
                {
                    mCurrentSelectedRunner.ExecutorEngine.BusinessFlows.Move(Indx, Indx - 1);
                }
                else
                {
                    return;
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
        }

        private void xmovedownBusinessflow_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress()) { return; }

            if (mCurrentBusinessFlowRunnerItem != null)
            {
                BusinessFlow bf = (BusinessFlow)mCurrentBusinessFlowRunnerItem.ItemObject;
                int Indx = mCurrentSelectedRunner.ExecutorEngine.BusinessFlows.IndexOf(bf);
                if (Indx < (mCurrentSelectedRunner.ExecutorEngine.BusinessFlows.Count - 1))
                {
                    mCurrentSelectedRunner.ExecutorEngine.BusinessFlows.Move(Indx, Indx + 1);
                }
                else
                { return; }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
        }

        private void xSearch_Click(object sender, RoutedEventArgs e)
        {
            if (SearchTextblock.Visibility == Visibility.Collapsed)
            {
                SearchTextblock.Visibility = Visibility.Visible;
                xmoveupBusinessflow.Visibility = xmovedownBusinessflow.Visibility = clearAllBusinessflow.Visibility = xAddBusinessflowBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                SearchTextblock.Visibility = Visibility.Collapsed;
                xSearch.ButtonImageType = eImageType.Search;
                xmoveupBusinessflow.Visibility = xmovedownBusinessflow.Visibility = clearAllBusinessflow.Visibility = xAddBusinessflowBtn.Visibility = Visibility.Visible;
            }
        }

        private void xDuplicateBusinessflow_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            if (mCurrentBusinessFlowRunnerItem != null)
            {
                BusinessFlow bf = (BusinessFlow)mCurrentBusinessFlowRunnerItem.ItemObject;
                BusinessFlow bCopy = (BusinessFlow)bf.CreateCopy(false);
                bCopy.ContainingFolder = bf.ContainingFolder;
                bCopy.InstanceGuid = Guid.NewGuid();
                bCopy.Reset();
                mCurrentSelectedRunner.AddBuinessFlowToRunner(bCopy, xBusinessflowsRunnerItemsListView.SelectedIndex + 1);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
        }

        private void xAddRunSet_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            AddNewRunSetConfig();
            mRunSetsSelectionPage = null;//for new run set to be shown 
        }

        // if page has been loaded from any other tab to Runset tab, it will show user message to refresh the runset
        private void Page_loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                xRunnersCanvasFrame.Refresh();
                xRunnersCanvasFrame.NavigationService.Refresh();

                if (mSolutionWasChanged)
                {
                    ResetLoadedRunSet();
                    mSolutionWasChanged = false;
                }
                else if (mRunSetBusinessFlowWasChanged)
                {
                    if (Reporter.ToUser(eUserMsgKey.RunsetBuinessFlowWasChanged) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                    {
                        RefreshCurrentRunSet();
                    }

                    mRunSetBusinessFlowWasChanged = false;
                }
            }));
        }

        private void RefreshCurrentRunSet()
        {
            LoadRunSetConfig(RunSetConfig);
        }

        private void xRunSetReload_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            if (Reporter.ToUser(eUserMsgKey.RunSetReloadhWarn) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                RefreshCurrentRunSet();
            }
        }

        private void xSelectedItemExecutionSyncBtn_Click(object sender, RoutedEventArgs e)
        {
            if (IsSelectedItemSyncWithExecution)
            {
                IsSelectedItemSyncWithExecution = false;
                xSelectedItemExecutionSyncBtn.ButtonImageType = eImageType.Invisible;
                xSelectedItemExecutionSyncBtn.ToolTip = "Runner Items Selection is not Synced with Execution Progress, Click to Sync it";
            }
            else
            {
                IsSelectedItemSyncWithExecution = true;
                xSelectedItemExecutionSyncBtn.ButtonImageType = eImageType.Visible;
                xSelectedItemExecutionSyncBtn.ToolTip = "Runner Items Selection is Synced with Execution Progress, Click to Un-Sync it";
            }
        }


        private void duplicateRunner(GingerExecutionEngine runner)
        {
            if (CheckIfExecutionIsInProgress()) { return; }

            if (runner != null)
            {
                GingerRunner GR = runner.GingerRunner;
                GingerRunner GRCopy = (GingerRunner)GR.CreateCopy(false);
                GRCopy.Guid = Guid.NewGuid();
                GRCopy.ParentGuid = GR.Guid;
                List<string> runnerNamesList = (from grs in RunSetConfig.GingerRunners select grs.Name).ToList<string>();
                GRCopy.Name = General.GetItemUniqueName(GR.Name, runnerNamesList);
                WorkSpace.Instance.RunsetExecutor.InitRunner(GRCopy, new GingerExecutionEngine(GRCopy));
                AddRunner(GRCopy);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
        }

        bool mRunnersCanvasIsShown = true;
        private void xRunnerDetailView_Click(object sender, RoutedEventArgs e)
        {
            if (mRunnersCanvasIsShown)
            {
                //show Runners mini view
                xRunnersViewRow.Height = new GridLength(50);
                xRunnersCanvasControls.Visibility = Visibility.Collapsed;
                xRunnersCanvasView.Visibility = Visibility.Collapsed;
                xRunnersMiniView.Visibility = Visibility.Visible;
                xRunnerDetailViewBtn.ButtonImageType = eImageType.ExpandAll;
                xRunnerNamelbl.Visibility = Visibility.Collapsed;
                SetComboRunnerInitialView();
                mRunnersCanvasIsShown = false;
            }
            else
            {
                //show Runners Canvas view
                xRunnersViewRow.Height = new GridLength(270);
                xRunnersCanvasControls.Visibility = Visibility.Visible;
                xRunnersCanvasView.Visibility = Visibility.Visible;
                xRunnersMiniView.Visibility = Visibility.Collapsed;
                xRunnerNamelbl.Visibility = Visibility.Visible;
                xRunnerDetailViewBtn.ButtonImageType = eImageType.CollapseAll;
                mRunnersCanvasIsShown = true;
            }
        }
        private void SetComboRunnerInitialView()
        {
            WeakEventManager<Selector, SelectionChangedEventArgs>.RemoveHandler(source: xRunnersCombo, eventName: nameof(Selector.SelectionChanged), handler: xRunnersCombo_SelectionChanged);
            xRunnersCombo.SelectedItem = mCurrentSelectedRunner.ExecutorEngine;
            WeakEventManager<Selector, SelectionChangedEventArgs>.AddHandler(source: xRunnersCombo, eventName: nameof(Selector.SelectionChanged), handler: xRunnersCombo_SelectionChanged);
        }
        private void xRunnersCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xRunnersCombo.SelectedValue == null)
            { return; }
            if (mCurrentSelectedRunner.ExecutorEngine.GingerRunner.Guid == (Guid)xRunnersCombo.SelectedValue)
            { return; }
            GingerRunner SelectedRunner = mRunSetConfig.GingerRunners.FirstOrDefault(x => x.Guid.ToString() == xRunnersCombo.SelectedValue.ToString());
            int index = mRunSetConfig.GingerRunners.IndexOf(SelectedRunner);
            List<FlowElement> fe = mFlowDiagram.GetAllFlowElements();
            GingerRunnerHighlight(((RunnerPage)fe[index].GetCustomeShape().Content));
        }

        private void xBusinessflowsDetailView_Click(object sender, RoutedEventArgs e)
        {
            if (xBusinessflowsRunnerItemsListView == null || xBusinessflowsRunnerItemsListView.Items.Count == 0) { return; }

            RunnerItemPage rii = (RunnerItemPage)xBusinessflowsRunnerItemsListView.Items[0];
            bool isExpand = false;
            if (rii.pageGrid.RowDefinitions[1].Height.Value == 0)
            { isExpand = true; }
            else
            { isExpand = false; }
            foreach (RunnerItemPage ri in xBusinessflowsRunnerItemsListView.Items)
            {
                ri.ExpandCollapseRunnerItem(isExpand);
            }
            if (rii.xDetailView.ButtonImageType == eImageType.Expand)
            {
                xBusinessflowsDetailViewBtn.ButtonImageType = eImageType.ExpandAll;
            }
            else
            {
                xBusinessflowsDetailViewBtn.ButtonImageType = eImageType.CollapseAll;
            }
        }

        private void xBusinessflowsActive_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress()) { return; }

            bool SetBusinessflowActive = false;
            if (xBusinessflowsRunnerItemsListView.Items.Count > 0)
            {
                BusinessFlow bff = (BusinessFlow)((RunnerItemPage)xBusinessflowsRunnerItemsListView.Items[0]).ItemObject;
                SetBusinessflowActive = !bff.Active;

                foreach (RunnerItemPage ri in xBusinessflowsRunnerItemsListView.Items)
                {
                    BusinessFlow bf = (BusinessFlow)ri.ItemObject;
                    bf.Active = SetBusinessflowActive;
                    if (SetBusinessflowActive)
                    {
                        ri.xItemName.Foreground = FindResource("$BackgroundColor_Black") as Brush;
                    }
                    else
                    {
                        ri.xItemName.Foreground = Brushes.Gray;
                    }
                }
            }
        }

        private void xActivitiesDetailView_Click(object sender, RoutedEventArgs e)
        {
            if (xActivitiesRunnerItemsListView == null || xActivitiesRunnerItemsListView.Items.Count == 0)
            {
                return;
            }

            RunnerItemPage rii = (RunnerItemPage)xActivitiesRunnerItemsListView.Items[0];
            bool isExpand = false;
            if (rii.pageGrid.RowDefinitions[1].Height.Value == 0)
            { isExpand = true; }
            else
            { isExpand = false; }
            foreach (RunnerItemPage ri in xActivitiesRunnerItemsListView.Items)
            {
                ri.ExpandCollapseRunnerItem(isExpand);
            }
            if (rii.xDetailView.ButtonImageType == eImageType.Expand)
            {
                xActivitiesDetailViewBtn.ButtonImageType = eImageType.ExpandAll;
            }
            else
            {
                xActivitiesDetailViewBtn.ButtonImageType = eImageType.CollapseAll;
            }
        }

        private void xActionsDetailView_Click(object sender, RoutedEventArgs e)
        {
            if (xActionsRunnerItemsListView == null || xActionsRunnerItemsListView.Items.Count == 0)
            {
                return;
            }

            RunnerItemPage rii = (RunnerItemPage)xActionsRunnerItemsListView.Items[0];
            bool isExpand = false;
            if (rii.pageGrid.RowDefinitions[1].Height.Value == 0)
            { isExpand = true; }
            else
            { isExpand = false; }
            foreach (RunnerItemPage ri in xActionsRunnerItemsListView.Items)
            {
                ri.ExpandCollapseRunnerItem(isExpand);
            }
            if (rii.xDetailView.ButtonImageType == eImageType.Expand)
            {
                xActionsDetailViewBtn.ButtonImageType = eImageType.ExpandAll;
            }
            else
            {
                xActionsDetailViewBtn.ButtonImageType = eImageType.CollapseAll;
            }
        }

        private void xRunRunnerBtn_Click(object sender, RoutedEventArgs e)
        {
            mCurrentSelectedRunner.RunRunner();
        }

        private void xStopRunnerBtn_Click(object sender, RoutedEventArgs e)
        {
            mCurrentSelectedRunner.StopRunner();
        }

        private void xContinueRunnerBtn_Click(object sender, RoutedEventArgs e)
        {
            mCurrentSelectedRunner.ContinueRunner();
        }
        private void dispatcherTimerElapsedTick(object sender, EventArgs e)
        {
            if (mCurrentSelectedRunner.ExecutorEngine.IsRunning)
            {
                UpdateRunnerTime();
            }
        }
        private void UpdateRunnerTime()
        {
            xRuntimeLbl.Content = mCurrentSelectedRunner.ExecutorEngine.RunnerExecutionWatch.runWatch.Elapsed.ToString(@"hh\:mm\:ss");
        }
        private void xRunnersActive_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress()) { return; }

            bool SetRunnerActive = false;
            List<FlowElement> fe = mFlowDiagram.GetAllFlowElements();
            if (fe.Count > 0)
            {
                GingerExecutionEngine grr = ((RunnerPage)fe[0].GetCustomeShape().Content).ExecutorEngine;
                SetRunnerActive = !grr.GingerRunner.Active;
            }
            foreach (FlowElement rp in fe)
            {
                GingerExecutionEngine gr = ((RunnerPage)rp.GetCustomeShape().Content).ExecutorEngine;
                gr.GingerRunner.Active = SetRunnerActive;
                if (SetRunnerActive)
                {
                    ((RunnerPage)rp.GetCustomeShape().Content).xRunnerNameTxtBlock.Foreground = FindResource("$BackgroundColor_Black") as Brush;
                }
                else
                {
                    ((RunnerPage)rp.GetCustomeShape().Content).xRunnerNameTxtBlock.Foreground = Brushes.Gray;
                }
            }
        }
        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool startupLocationWithOffset = false, bool mEditMode = true)
        {
            string title = "Edit " + GingerDicser.GetTermResValue(eTermResKey.RunSet);
            ObservableList<Button> winButtons = [];
            if (mEditMode)
            {
                title = "View " + GingerDicser.GetTermResValue(eTermResKey.RunSet);

            }

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, title, this, winButtons, true, "Close");
        }

        private void xExportToAlmBtn_Click(object sender, RoutedEventArgs e)
        {
            ObservableList<BusinessFlow> bfs = [];

            foreach (GingerRunner GR in WorkSpace.Instance.RunsetExecutor.Runners)
            {
                bfs.Append(GR.Executor.BusinessFlows);
            }
            if (!ExportResultsToALMConfigPage.Instance.IsProcessing)
            {
                if (ExportResultsToALMConfigPage.Instance.Init(bfs, new GingerCore.ValueExpression(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>(), false, "", false)))
                {
                    ExportResultsToALMConfigPage.Instance.ShowAsWindow();
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.ExportedExecDetailsToALMIsInProcess);
            }
        }


        private void xFind_Click(object sender, RoutedEventArgs e)
        {
            ShowFindAndReplacePage();
        }

        public void ShowFindAndReplacePage()
        {
            FindAndReplacePage mfindAndReplacePageRunSet = new FindAndReplacePage(FindAndReplacePage.eContext.RunsetPage);
            mfindAndReplacePageRunSet.ShowAsWindow();
        }

        private void XRunsetEnvironmentCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mContext.Environment = (ProjEnvironment)xRunsetEnvironmentCombo.SelectedItem;
        }

        private void XAutoRunButton_Click(object sender, RoutedEventArgs e)
        {
            WizardWindow.ShowWizard(new AutoRunWizard(mRunSetConfig, mContext));

        }

        private void xUndoChangesBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CheckIfExecutionIsInProgress()) { return; }

                IsCalledFromxUndoBtn = true;
                CollectionChangedEventManager.RemoveHandler(source: mRunSetConfig.GingerRunners, handler: Runners_CollectionChanged);

                if (Ginger.General.UndoChangesInRepositoryItem(mRunSetConfig, true))
                {
                    mRunSetConfig.SaveBackup();
                    CollectionChangedEventManager.AddHandler(source: mRunSetConfig.GingerRunners, handler: Runners_CollectionChanged);
                    LoadRunSetConfig(mRunSetConfig, true);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while undoing changes", ex);
            }
            finally
            {
                if (mRunSetConfig != null)
                {
                    CollectionChangedEventManager.RemoveHandler(source: mRunSetConfig.GingerRunners, handler: Runners_CollectionChanged);
                    CollectionChangedEventManager.AddHandler(source: mRunSetConfig.GingerRunners, handler: Runners_CollectionChanged);
                    IsCalledFromxUndoBtn = false;
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

        private void xSelfHealingConfigBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfExecutionIsInProgress())
            {
                return;
            }

            GingerSelfHealingConfiguration selfHealingConfiguration = new GingerSelfHealingConfiguration(mRunSetConfig);
            selfHealingConfiguration.ShowAsWindow();
        }

        private sealed class BoolToRunsetLabelColorValueConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is bool boolValue && boolValue)
                {
                    return (SolidColorBrush)Application.Current.Resources["$HighlightColor_LightBlue"];
                }
                else
                {
                    return (SolidColorBrush)Application.Current.Resources["$SelectionColor_Pink"];
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
