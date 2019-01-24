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
using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Execution;

using Amdocs.Ginger.Repository;
using Ginger.Actions;
using Ginger.AnalyzerLib;
using Ginger.BusinessFlowFolder;
using Ginger.Functionalities;
using Ginger.MoveToGingerWPF.Run_Set_Pages;
using Ginger.Reports;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.UserControlsLib.VisualFlow;
using GingerCore;
using GingerCore.Actions;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCoreNET.RunLib;
using GingerWPF.UserControlsLib.UCTreeView;
using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Ginger.Run
{
    /// <summary>
    /// Interaction logic for RunPage.xaml
    /// </summary>
    public partial class NewRunSetPage : Page
    {
        public RunnerPage mCurrentSelectedRunner;

        GenericWindow _pageGenericWin = null;
        FlowDiagramPage mFlowDiagram;
        int mFlowX = 0;
        int mFlowY = 0;
        bool IsSelectedItemSyncWithExecution = true;//execution and selected items are synced as default   
        SingleItemTreeViewSelectionPage mRunSetsSelectionPage = null;
        SingleItemTreeViewSelectionPage mBusFlowsSelectionPage = null;
        RunsetOperationsPage mRunsetOperations = null;
        RunSetConfig mRunSetConfig = null;
        RunSetsExecutionsHistoryPage mRunSetsExecutionsPage = null;
        RunSetsALMDefectsOpeningPage mRunSetsALMDefectsOpeningPage = null;
        private FileSystemWatcher mBusinessFlowsXmlsChangeWatcher = null;
        private bool mRunSetBusinessFlowWasChanged = false;
        private bool mSolutionWasChanged = false;
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
                if (xBusinessflowsRunnerItemsListView != null && xBusinessflowsRunnerItemsListView.Items.Count > 0)
                {
                    if (xBusinessflowsRunnerItemsListView.SelectedItem == null)
                    {
                        xBusinessflowsRunnerItemsListView.SelectedIndex = 0;
                    }

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
                    return (BusinessFlow)((RunnerItemPage)xBusinessflowsRunnerItemsListView.SelectedItem).ItemObject;
                else
                    return null;
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
                    return (Activity)((RunnerItemPage)xActivitiesRunnerItemsListView.SelectedItem).ItemObject;
                else
                    return null;
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


        public enum eEditMode
        {
            ExecutionFlow = 0,
            View = 1,
        }
        public eEditMode mEditMode { get; set; }

        public NewRunSetPage()//when window opened manually
        {
            InitializeComponent();

            if ( WorkSpace.UserProfile.Solution != null)
            {
                //Init
                Init();

                //load Run Set
                RunSetConfig defualtRunSet = GetDefualtRunSetConfig();
                if (defualtRunSet != null)
                    LoadRunSetConfig(defualtRunSet);
                else
                {
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, string.Format("No {0} found to load, please add {0}.", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                    //TODO: hide all pages elements
                }
            }
        }

        public NewRunSetPage(RunSetConfig runSetConfig, eEditMode editMode = eEditMode.ExecutionFlow)//when window opened automatically when running from command line
        {
            InitializeComponent();

            //Init
            Init();

            mEditMode = editMode;
            if (mEditMode == eEditMode.View)
            {
                xResetRunsetBtn.IsEnabled = false;
                xRunRunsetBtn.IsEnabled = false;
                xStopRunsetBtn.IsEnabled = false;
                xContinueRunsetBtn.IsEnabled = false;
                controls.IsEnabled = false;
                xRunSetOperationsPanel.IsEnabled = false;
                LoadRunSetConfig(runSetConfig, false, true);
            }
        //load Run Set
        if (runSetConfig != null)
                LoadRunSetConfig(runSetConfig, false);
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, string.Format("No {0} found to load, please add {0}.", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                //TODO: hide all pages elements
            }           
        }

        private void Init()
        {
            SetNonSpecificRunSetEventsTracking();
            SetBusinessFlowsChangesLisener();
        }

        private void SetNonSpecificRunSetEventsTracking()
        {
             WorkSpace.UserProfile.PropertyChanged -= UserProfilePropertyChanged;
             WorkSpace.UserProfile.PropertyChanged += UserProfilePropertyChanged;

            App.RunsetExecutor.PropertyChanged -= RunsetExecutor_PropertyChanged;
            App.RunsetExecutor.PropertyChanged += RunsetExecutor_PropertyChanged;

            WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>().CollectionChanged -= AgentsCache_CollectionChanged;
            WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>().CollectionChanged += AgentsCache_CollectionChanged;

            xBusinessflowsRunnerItemsListView.SelectionChanged -= xActivitiesListView_SelectionChanged;
            xBusinessflowsRunnerItemsListView.SelectionChanged += xActivitiesListView_SelectionChanged;

            xActivitiesRunnerItemsListView.SelectionChanged -= xActionsListView_SelectionChanged;
            xActivitiesRunnerItemsListView.SelectionChanged += xActionsListView_SelectionChanged;

            ((INotifyCollectionChanged)xActivitiesRunnerItemsListView.Items).CollectionChanged -= xActivitiesRunnerItemsListView_CollectionChanged;
            ((INotifyCollectionChanged)xActivitiesRunnerItemsListView.Items).CollectionChanged += xActivitiesRunnerItemsListView_CollectionChanged;

            RunnerItemPage.SetRunnerItemEvent(RunnerItem_RunnerItemEvent);            
        }

        private void AgentsCache_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Task.Run(() =>
            {
                if (mRunSetConfig != null)
                {
                    Parallel.ForEach(mRunSetConfig.GingerRunners, Runner =>
                    {
                        Runner.SolutionAgents = new ObservableList<IAgent>(WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>().ListItems.ConvertAll(x => (IAgent)x).ToList());
                        //to get the latest list of applications agents
                        Runner.UpdateApplicationAgents();
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
                xActivitiesName.Content = GingerDicser.GetTermResValue(eTermResKey.Activities)+" (" + xActivitiesRunnerItemsListView.Items.Count + ")";
            });
        }

        private void Runner_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (e.PropertyName == nameof(GingerRunner.Status))
                {
                    if (IsSelectedItemSyncWithExecution && mRunSetConfig.RunModeParallel == false
                                && ((GingerRunner)sender).Status == eRunStatus.Running)
                    {
                        List<FlowElement> fe = mFlowDiagram.GetAllFlowElements();
                        foreach (FlowElement f in fe)
                        {
                            RunnerPage rp = (RunnerPage)f.GetCustomeShape().Content;
                            if (rp != null && rp.Runner.Guid.Equals(((GingerRunner)sender).Guid))
                            {
                                GingerRunnerHighlight(rp);
                            }
                        }
                    }
                }
            });
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
            App.RunsetExecutor.DefectSuggestionsList = new ObservableList<DefectSuggestion>();
            xALMDefectsOpening.IsEnabled = false;
            UpdateRunsetALMDefectsOpeningTabHeader();
        }

        public void UpdateRunsetALMDefectsOpeningTabHeader()
        {
            if (App.RunsetExecutor.DefectSuggestionsList.Count > 0)
            {
                ALMDefects.Text = string.Format("ALM Defects Opening ({0})", App.RunsetExecutor.DefectSuggestionsList.Count);
                //ALMDefects.Foreground = (Brush)Application.Current.Resources["$HighlightColor_Purple"];
            }
            else
            {
                ALMDefects.Text = string.Format("ALM Defects Opening");
                //ALMDefects.Foreground = (Brush)Application.Current.Resources["$HighlightColor_Purple"];
            }
        }

        private async void RunnerItem_RunnerItemEvent(RunnerItemEventArgs EventArgs)
        {
            Run.GingerRunner currentSelectedRunner = mCurrentSelectedRunner.Runner;

            switch (EventArgs.EventType)
            {
                case RunnerItemEventArgs.eEventType.ContinueRunRequired:
                    if (currentSelectedRunner.IsRunning)
                    {
                        Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Runner is already running, please stop it first.");
                        return;
                    }
                    App.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder = null;
                    AutoLogProxy.UserOperationStart("Continue Clicked");
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
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                ((BusinessFlow)e.NewItems[0]).AttachActivitiesGroupsAndActivities();
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (mCurrentSelectedRunner.BusinessflowRunnerItems.Count > 0)
                    mCurrentSelectedRunner.BusinessflowRunnerItems.RemoveAt(e.OldStartingIndex);
            }
            else if (e.Action == NotifyCollectionChangedAction.Move)
            {
                mCurrentSelectedRunner.BusinessflowRunnerItems.Move(e.OldStartingIndex, e.NewStartingIndex);
            }
            this.Dispatcher.Invoke(() =>
            {
                mCurrentSelectedRunner.Runner.TotalBusinessflow = ((IList<BusinessFlow>)sender).Count;
                mCurrentSelectedRunner.UpdateExecutionStats();
                UpdateBusinessflowCounter();
                mCurrentSelectedRunner.UpdateRunnerInfo();
            });
        }
        private void Runners_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                RunnerPage runnerPage = InitRunnerFlowElement((GingerRunner)e.NewItems[0], e.NewStartingIndex);
                GingerRunnerHighlight(runnerPage);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                mFlowDiagram.RemoveFlowElem(((GingerRunner)e.OldItems[0]).Guid.ToString(), mRunSetConfig.RunModeParallel);
            }
            else if (e.Action == NotifyCollectionChangedAction.Move)
            {
                mFlowDiagram.MoveFlowElement(e.OldStartingIndex, e.NewStartingIndex);
            }
            this.Dispatcher.Invoke(() =>
            {
                UpdateRunnersTabHeader();
            });
        }
        private void UpdateBusinessflowCounter()
        {
            xBusinessflowName.Content = string.Format("{0} ({1})", GingerDicser.GetTermResValue(eTermResKey.BusinessFlows), mCurrentSelectedRunner.Runner.BusinessFlows.Count);
        }


        void InitRunSetConfigurations()
        {
            GingerWPF.BindingLib.ControlsBinding.ObjFieldBinding(xRunSetNameTextBox, TextBox.TextProperty, mRunSetConfig, nameof(RunSetConfig.Name));
            GingerWPF.BindingLib.ControlsBinding.ObjFieldBinding(xRunSetDescriptionTextBox, TextBox.TextProperty, mRunSetConfig, nameof(RunSetConfig.Description));
            TagsViewer.Init(mRunSetConfig.Tags);
            GingerWPF.BindingLib.ControlsBinding.ObjFieldBinding(xRunWithAnalyzercheckbox, CheckBox.IsCheckedProperty, mRunSetConfig, nameof(RunSetConfig.RunWithAnalyzer));
        }

        void InitRunSetInfoSection()
        {
            GingerWPF.BindingLib.ControlsBinding.ObjFieldBinding(xRunSetNameLbl, Label.ContentProperty, mRunSetConfig, nameof(RunSetConfig.Name));
            mRunSetConfig.PropertyChanged += MRunSetConfig_PropertyChanged;
            SetRunSetDescription();
        }

        private void SetRunSetDescription()
        {
            if (string.IsNullOrEmpty(RunSetConfig.Description) == false)
            {
                string runSetDesc = RunSetConfig.Description;
                if (RunSetConfig.Description.Length > 500)
                    runSetDesc = RunSetConfig.Description.Substring(0, 500) + "...";
                xRunsetDescriptionTextBlock.Text = "Description: " + runSetDesc;
                xRunsetDescriptionTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                xRunsetDescriptionTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        private void MRunSetConfig_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RunSetConfig.Description))
            {
                SetRunSetDescription();
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
                if ( WorkSpace.UserProfile.Solution == null) return null;

                ObservableList<RunSetConfig> allRunsets = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>();

                //looking for last used Run Set
                if ( WorkSpace.UserProfile.RecentRunset != null &&
                             WorkSpace.UserProfile.RecentRunset != Guid.Empty)
                {
                    RunSetConfig recentRunset = allRunsets.Where(runsets => runsets.Guid ==  WorkSpace.UserProfile.RecentRunset).FirstOrDefault();
                    if (recentRunset != null)
                        return recentRunset;
                }

                //return first Run set in Solution
                if (allRunsets.Count > 0)
                {
                    return allRunsets[0];
                }

                //create new defualt run set
                RunSetConfig newRunSet = RunSetOperations.CreateNewRunset("Default " + GingerDicser.GetTermResValue(eTermResKey.RunSet));
                if (newRunSet != null)
                    return newRunSet;
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
            if (CheckCurrentRunnerIsNotRuning()) return;
            Run.GingerRunner CGR = mCurrentSelectedRunner.Runner;
            int Indx = mRunSetConfig.GingerRunners.IndexOf(CGR);

            if (Indx > 0)
                mRunSetConfig.GingerRunners.Move(Indx, Indx - 1);
            else
                return;
        }

        private void MoveRunnerRight_Click(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentRunnerIsNotRuning()) return;
            Run.GingerRunner CGR = mCurrentSelectedRunner.Runner;
            int Indx = mRunSetConfig.GingerRunners.IndexOf(CGR);
            if (Indx < (mRunSetConfig.GingerRunners.Count - 1))
                mRunSetConfig.GingerRunners.Move(Indx, Indx + 1);
            else
                return;
        }
        
        private void RunsetExecutor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Handle Run Set change
            if (e.PropertyName == nameof(RunsetExecutor.RunSetConfig))
            {
                if (App.RunsetExecutor.RunSetConfig == null || App.RunsetExecutor.RunSetConfig.Equals(RunSetConfig) == false)
                {
                    if (!mSolutionWasChanged)//avoid the change if shifting solution
                        ResetLoadedRunSet(App.RunsetExecutor.RunSetConfig);
                }
            }
        }

        private void UserProfilePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Handle Solution change
            if (e.PropertyName == nameof(UserProfile.Solution))
            {
                if ( WorkSpace.UserProfile.Solution == null)
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

        private void ResetLoadedRunSet(RunSetConfig runSetToSet=null)
        {
            //reset selection trees
            mBusFlowsSelectionPage = null;
            mRunSetsSelectionPage = null;

            //init BF's watcher
            mRunSetBusinessFlowWasChanged = false;
            SetBusinessFlowsChangesLisener();

            //load new run set
            if (runSetToSet == null)
                runSetToSet = GetDefualtRunSetConfig();
            if (runSetToSet != null)
                LoadRunSetConfig(runSetToSet);
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, string.Format("No {0} found to load, please add {0}.", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                //TODO: hide all pages elements
            }
        }

        private void GingerRunnerHighlight(RunnerPage GRP)
        {
            if (mCurrentSelectedRunner != null && GRP != null && mCurrentSelectedRunner.Equals(GRP)) return;//the Runner is already selected


            //de-highlight previous selected Ginger
            if (mCurrentSelectedRunner != null)
            {
                mCurrentSelectedRunner.xBorder.Visibility = System.Windows.Visibility.Collapsed;
                mCurrentSelectedRunner.xRunnerInfoSplitterBorder.Background = FindResource("$BackgroundColor_DarkBlue") as Brush;
                mCurrentSelectedRunner.xRunnerInfoSplitterBorder.Height = 1;
                if (!((GingerRunner)mCurrentSelectedRunner.Runner).Active)
                {
                    mCurrentSelectedRunner.xRunnerNameTxtBlock.Foreground = Brushes.Gray;
                }
                else
                {
                    mCurrentSelectedRunner.xRunnerNameTxtBlock.Foreground = FindResource("$BackgroundColor_DarkBlue") as Brush;
                }
            }

            //highlight the selected Ginger
            GRP.xBorder.Visibility = System.Windows.Visibility.Visible;
            GRP.xRunnerInfoSplitterBorder.Background = FindResource("$amdocsLogoLinarGradientBrush") as Brush;
            GRP.xRunnerInfoSplitterBorder.Height = 4;
            if (!((GingerRunner)GRP.Runner).Active)
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
            UpdateRunnerTime();

            // mCurrentSelectedRunner.Runner.GingerRunnerEvent += Runner_GingerRunnerEvent;

            //set it as flow diagram current item
            List<FlowElement> fe = mFlowDiagram.GetAllFlowElements();
            foreach (FlowElement f in fe)
            {
                RunnerPage rp = (RunnerPage)f.GetCustomeShape().Content;
                if (rp != null && rp.Runner.Guid.Equals(GRP.Runner.Guid))
                {
                    mFlowDiagram.mCurrentFlowElem = f;
                    break;
                }
            }

            //set the runner items section
            InitRunnerExecutionDebugSection();
        }

        
        private void Runner_GingerRunnerEvent(GingerRunnerEventArgs EventArgs)
        {
            switch (EventArgs.EventType)
            {
                case GingerRunnerEventArgs.eEventType.BusinessflowWasReset:
                case GingerRunnerEventArgs.eEventType.DynamicActivityWasAddedToBusinessflow:
                    this.Dispatcher.Invoke(() =>
                    {
                        if (EventArgs.Object is BusinessFlow)
                        {
                            BusinessFlow changedBusinessflow = (BusinessFlow)EventArgs.Object;
                            if (mCurrentBusinessFlowRunnerItem.ItemObject == changedBusinessflow)
                            {
                                mCurrentBusinessFlowRunnerItem.LoadChildRunnerItems();//reloading activities to make sure include dynamically added/removed activities.
                                xActivitiesRunnerItemsListView.ItemsSource = mCurrentBusinessFlowRunnerItem.ItemChilds;
                            }
                        }
                    });
                    break;
            }
        }

        private void InitRunnerExecutionDebugSection()
        {
            try
            {
                GingerWPF.BindingLib.ControlsBinding.ObjFieldBinding(xRunnerNamelbl, Label.ContentProperty, mCurrentSelectedRunner.Runner, nameof(GingerRunner.Name));

                xBusinessflowsRunnerItemsLoadingIcon.Visibility = Visibility.Visible;
                xBusinessflowsRunnerItemsListView.Visibility = Visibility.Collapsed;
                General.DoEvents();//for seeing the processing icon better to do with Async                

                //load needed Bf's and clear other runners BF's pages to save memory
                List<FlowElement> fe = mFlowDiagram.GetAllFlowElements();
                foreach (FlowElement flowElem in fe)
                {
                    if (flowElem == null) continue;
                    RunnerPage rp = (RunnerPage)flowElem.GetCustomeShape().Content;
                    if (rp == null) continue;

                    if(rp.Runner.Guid.Equals(mCurrentSelectedRunner.Runner.Guid))
                        //load BF's items
                        xBusinessflowsRunnerItemsListView.ItemsSource = rp.BusinessflowRunnerItems;
                    else
                        rp.ClearBusinessflowRunnerItems();
                }
                GC.Collect();//to help with memory free

                mCurrentSelectedRunner.Runner.BusinessFlows.CollectionChanged -= BusinessFlows_CollectionChanged;
                mCurrentSelectedRunner.Runner.BusinessFlows.CollectionChanged += BusinessFlows_CollectionChanged;

                //FIXME !!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //mCurrentSelectedRunner.Runner.RunnerExecutionWatch.dispatcherTimerElapsed.Tick -= dispatcherTimerElapsedTick;
                //mCurrentSelectedRunner.Runner.RunnerExecutionWatch.dispatcherTimerElapsed.Tick += dispatcherTimerElapsedTick;               

                GingerWPF.BindingLib.ControlsBinding.ObjFieldBinding(xStatus, StatusItem.StatusProperty, mCurrentSelectedRunner.Runner, nameof(GingerRunner.Status), BindingMode.OneWay);                
            }
            finally
            {
                xBusinessflowsRunnerItemsLoadingIcon.Visibility = Visibility.Collapsed;
                xBusinessflowsRunnerItemsListView.Visibility = Visibility.Visible;
            }

            xBusinessflowName.Content = string.Format("{0} ({1})", GingerDicser.GetTermResValue(eTermResKey.BusinessFlows), mCurrentSelectedRunner.Runner.BusinessFlows.Count);
            if (xBusinessflowsRunnerItemsListView.Items.Count > 0)
            {
                xBusinessflowsRunnerItemsListView.SelectedItem = mCurrentSelectedRunner.BusinessflowRunnerItems[0];
            }
        }
       
        private void RunnerPageEvent(RunnerPageEventArgs EventArgs)
        {
            switch (EventArgs.EventType)
            {               
                case RunnerPageEventArgs.eEventType.RemoveRunner:
                    removeRunner((GingerRunner)EventArgs.Object);
                    break;
                case RunnerPageEventArgs.eEventType.DuplicateRunner:
                    duplicateRunner((GingerRunner)EventArgs.Object);
                    break;
                case RunnerPageEventArgs.eEventType.ResetRunnerStatus:
                    ResetRunnerStatus((GingerRunner)EventArgs.Object);
                    break;
            }
        }
        private void ResetRunnerStatus(GingerRunner runner)
        {
            ResetRunset(runner);
        }

        private void ResetRunset(GingerRunner runner)
        {           
            if (runner.Status == eRunStatus.Running)            
                return;           
            
            xRuntimeLbl.Content = "00:00:00";            
        }

        internal void InitFlowDiagram()
        {
            if(mFlowDiagram == null)
            {
                mFlowDiagram = new FlowDiagramPage();
                mFlowDiagram.SetHighLight = false;
                mFlowDiagram.BackGround = Brushes.White;            
                mFlowDiagram.ZoomPanelContainer.Visibility = Visibility.Collapsed;
                mFlowDiagram.ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                mFlowDiagram.Height = 300;
                mFlowDiagram.Ismovable = false;
                mFlowX = 0;
                mFlowY = 0;
                xRunnersFrame.Content = mFlowDiagram;
            }
            else
            {
                mFlowDiagram.ClearAllFlowElement();
            }
        }
        internal RunnerPage InitRunnerFlowElement(GingerRunner runner, int index = -1, bool ViewMode=false)
        {
            RunnerPage GRP = new RunnerPage(runner, ViewMode);
            GRP.Tag = runner.Guid;
            GRP.MouseLeftButtonDown += GRP_MouseLeftButtonDown;
            
            GRP.Width = 515;
            FlowElement RunnerFlowelement = new FlowElement(FlowElement.eElementType.CustomeShape, GRP, mFlowX, mFlowY, 600, 220);
            RunnerFlowelement.OtherInfoVisibility = Visibility.Collapsed;
            RunnerFlowelement.Tag = GRP.Tag;
            RunnerFlowelement.MouseDoubleClick += RunnerFlowelement_MouseDoubleClick;
            mFlowDiagram.AddFlowElem(RunnerFlowelement, index);
            if (mFlowDiagram.mCurrentFlowElem != null)
            {
                FlowLink FL = new FlowLink(mFlowDiagram.mCurrentFlowElem, RunnerFlowelement,true);
                FL.LinkStyle = FlowLink.eLinkStyle.Arrow;
                FL.SourcePosition = FlowLink.eFlowElementPosition.Right;
                FL.Tag = RunnerFlowelement.Tag;
                FL.DestinationPosition = FlowLink.eFlowElementPosition.Left;
                FL.Margin = new Thickness(0, 0, mFlowX, 0);

                GingerWPF.BindingLib.ControlsBinding.ObjFieldBinding(FL, FlowLink.VisibilityProperty, mRunSetConfig, nameof(RunSetConfig.RunModeParallel), System.Windows.Data.BindingMode.OneWay, bindingConvertor: new ReverseBooleanToVisibilityConverter());
                mFlowDiagram.AddConnector(FL);
            }

            mFlowDiagram.mCurrentFlowElem = RunnerFlowelement;
            
            GRP.xBorder.Visibility = System.Windows.Visibility.Collapsed;
            GRP.xRunnerInfoSplitterBorder.Height = 1;
            GRP.xRunnerInfoSplitterBorder.Background = FindResource("$BackgroundColor_DarkBlue") as Brush;
            GRP.xRunnerNameTxtBlock.Foreground = FindResource("$BackgroundColor_DarkBlue") as Brush;
            mFlowX = mFlowX + 610;
            return GRP;
        }

        private void RunnerFlowelement_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RunnerPage rp = (RunnerPage)((FlowElement)sender).GetCustomeShape().Content;
            GingerRunnerConfigurationsPage PACW = new GingerRunnerConfigurationsPage(rp.Runner, GingerRunnerConfigurationsPage.ePageContext.RunTab);
            PACW.ShowAsWindow();
            rp.UpdateRunnerInfo();
        }
        
        private void GRP_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GingerRunnerHighlight((RunnerPage)sender);
        }
        
        private async Task<int> InitRunnersSection(bool runAsync = true, bool ViewMode=false)
        {
            this.Dispatcher.Invoke(() =>
            {
                xRunnersFrame.Refresh();
                xRunnersFrame.NavigationService.Refresh();
                //Init Runner FlowDiagram            
                InitFlowDiagram();
            });

            RunnerPage firstRunnerPage = null;
            foreach (GingerRunner GR in mRunSetConfig.GingerRunners)
            {
                if (runAsync)
                    await Task.Run(() => App.RunsetExecutor.InitRunner(GR));
                else
                    App.RunsetExecutor.InitRunner(GR);

                this.Dispatcher.Invoke(() =>
                {
                    RunnerPage runnerPage= InitRunnerFlowElement(GR, mRunSetConfig.GingerRunners.IndexOf(GR), ViewMode);
                    if (firstRunnerPage == null)
                        firstRunnerPage = runnerPage;

                    GR.PropertyChanged -= Runner_PropertyChanged;
                    GR.PropertyChanged += Runner_PropertyChanged;
                    
                    runnerPage.RunnerPageEvent -= RunnerPageEvent;
                    runnerPage.RunnerPageEvent += RunnerPageEvent;
                });
            }

            this.Dispatcher.Invoke(() =>
            {
                mRunSetConfig.GingerRunners.CollectionChanged -= Runners_CollectionChanged;
                mRunSetConfig.GingerRunners.CollectionChanged += Runners_CollectionChanged;

                //highlight first Runner
                if (firstRunnerPage != null)
                    GingerRunnerHighlight(firstRunnerPage);

                // to check run mode of already created runset
                SetExecutionModeIcon();
                SetEnvironmentsCombo();
                SetRunnersCombo();
                UpdateRunnersTabHeader();
                xZoomPanel.ZoomSliderContainer.ValueChanged += ZoomSliderContainer_ValueChanged;
            });
            return 1;
        }

        private void ZoomSliderContainer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            if ((mFlowDiagram.GetCanvas()) == null) return;  // will happen only at page load

            // Set the Canvas scale based on ZoomSlider value
            ScaleTransform ST = new ScaleTransform(e.NewValue, e.NewValue);
            (mFlowDiagram.GetCanvas()).LayoutTransform = ST;

            xZoomPanel.PercentLabel.Content = (int)(e.NewValue * 100) + "%";
        }
        private void SetBusinessFlowsChangesLisener()
        {
            if ( WorkSpace.UserProfile.Solution != null)
            {
                mBusinessFlowsXmlsChangeWatcher = new FileSystemWatcher();
                mBusinessFlowsXmlsChangeWatcher.Path =  WorkSpace.UserProfile.Solution.BusinessFlowsMainFolder;
                mBusinessFlowsXmlsChangeWatcher.Filter = "*.xml";
                mBusinessFlowsXmlsChangeWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
                mBusinessFlowsXmlsChangeWatcher.IncludeSubdirectories = true;

                mBusinessFlowsXmlsChangeWatcher.Changed -= new FileSystemEventHandler(OnBusinessFlowsXmlsChange);
                mBusinessFlowsXmlsChangeWatcher.Changed += new FileSystemEventHandler(OnBusinessFlowsXmlsChange);

                mBusinessFlowsXmlsChangeWatcher.Deleted -= new FileSystemEventHandler(OnBusinessFlowsXmlsChange);
                mBusinessFlowsXmlsChangeWatcher.Deleted += new FileSystemEventHandler(OnBusinessFlowsXmlsChange);

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
                                Parallel.ForEach(Runner.BusinessFlows, businessFlow =>
                                {
                                    BusinessFlow originalBF = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>().Where(x => x.Guid == businessFlow.Guid).FirstOrDefault();
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
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occured while checking Run Set Business Flow files change", ex);
            }
        }

        private void SetEnvironmentsCombo()
        {
            xRunsetEnvironmentCombo.ItemsSource = null;

            if ( WorkSpace.UserProfile.Solution != null)
            {
                xRunsetEnvironmentCombo.ItemsSource = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>();
                xRunsetEnvironmentCombo.DisplayMemberPath = nameof(ProjEnvironment.Name);
                xRunsetEnvironmentCombo.SelectedValuePath = nameof(RepositoryItemBase.Guid);

                GingerWPF.BindingLib.ControlsBinding.ObjFieldBinding(xRunsetEnvironmentCombo, ComboBox.SelectedItemProperty, App.RunsetExecutor, nameof(RunsetExecutor.RunsetExecutionEnvironment));        

                //select last used environment
                if (xRunsetEnvironmentCombo.Items != null && xRunsetEnvironmentCombo.Items.Count > 0)
                {
                    if (xRunsetEnvironmentCombo.Items.Count > 1 &&  WorkSpace.UserProfile.RecentEnvironment != null &&  WorkSpace.UserProfile.RecentEnvironment != Guid.Empty)
                    {
                        foreach (object env in xRunsetEnvironmentCombo.Items)
                        {
                            if (((ProjEnvironment)env).Guid ==  WorkSpace.UserProfile.RecentEnvironment)
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

            if ( WorkSpace.UserProfile.Solution != null)
            {
                xRunnersCombo.ItemsSource = mRunSetConfig.GingerRunners;
                xRunnersCombo.DisplayMemberPath = nameof(GingerRunner.Name);
                xRunnersCombo.SelectedValuePath = nameof(GingerRunner.Guid);

                GingerWPF.BindingLib.ControlsBinding.ObjFieldBinding(xRunnersCombo, ComboBox.SelectedItemProperty, mRunSetConfig.GingerRunners, nameof(GingerRunner.Guid));              
            }
        }        
        public async void LoadRunSetConfig(RunSetConfig runSetConfig, bool runAsync = true, bool ViewMode=false)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    xRunSetLoadingPnl.Visibility = Visibility.Visible;
                    xRunsetPageGrid.Visibility = Visibility.Collapsed;

                    runSetConfig.SaveBackup();
                    runSetConfig.StartDirtyTracking();

                    mRunSetConfig = runSetConfig;
                    App.RunsetExecutor.RunSetConfig = RunSetConfig;
                    
                    //Init Run Set Details Section
                    InitRunSetInfoSection();
                });

                //Init Runners Section  
                int res = 0;
                if (runAsync)
                    res = await InitRunnersSection().ConfigureAwait(false);
                else
                    InitRunnersSection(false, ViewMode);

                this.Dispatcher.Invoke(() =>
                {
                    //Init Operations Section
                    InitOperationsSection();

                    // Init Runset Config Section
                    InitRunSetConfigurations();

                    //Init Execution History Section
                    InitExecutionHistorySection();

                     WorkSpace.UserProfile.RecentRunset = mRunSetConfig.Guid;//to be loaded automatically next time
                });

            }
            finally
            {
                this.Dispatcher.Invoke(() =>
                {
                    xRunSetLoadingPnl.Visibility = Visibility.Collapsed;
                    xRunsetPageGrid.Visibility = Visibility.Visible;
                });
            }
        }

        void InitOperationsSection()
        {
            if (mRunsetOperations == null)
            {
                mRunsetOperations = new RunsetOperationsPage(RunSetConfig);
                xRunsetOperationsTab.Content = mRunsetOperations;
            }
            else
                mRunsetOperations.Init(RunSetConfig);

            RunSetConfig.RunSetActions.CollectionChanged += RunSetActions_CollectionChanged;
            UpdateRunsetOperationsTabHeader();
        }



        private void InitExecutionHistorySection()
        {
            if (mRunSetsExecutionsPage == null)
            {
                mRunSetsExecutionsPage = new RunSetsExecutionsHistoryPage(RunSetsExecutionsHistoryPage.eExecutionHistoryLevel.SpecificRunSet, RunSetConfig);
                mExecutionSummary.Content = mRunSetsExecutionsPage;
                mRunSetsExecutionsPage.ExecutionsHistoryList.CollectionChanged += ExecutionsHistoryList_CollectionChanged;
            }
            else
            {
                mRunSetsExecutionsPage.RunsetConfig = mRunSetConfig;
                mRunSetsExecutionsPage.ReloadData();
            }

            UpdateRunsetExecutionHistoryTabHeader();
        }

        private void InitALMDefectsOpeningSection()
        {
            if (mRunSetsALMDefectsOpeningPage == null)
            {
                mRunSetsALMDefectsOpeningPage = new RunSetsALMDefectsOpeningPage();
                mALMDefectsOpening.Content = mRunSetsALMDefectsOpeningPage;
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
            //Do Runset Save Preperations
            foreach (GingerRunner GR in mRunSetConfig.GingerRunners)
            {
                GR.UpdateBusinessFlowsRunList();
            }

            WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mRunSetConfig);
            

            Reporter.ToUser(eUserMsgKey.StaticInfoMessage, GingerDicser.GetTermResValue(eTermResKey.RunSet) + " was saved successfully");
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
                if (mCurrentSelectedRunner != null && mCurrentSelectedRunner.Runner != null)
                    index = mRunSetConfig.GingerRunners.IndexOf(mCurrentSelectedRunner.Runner) + 1;
                GingerRunner newRunner = new GingerRunner();
                if (gingerRunner != null)
                {
                    newRunner = gingerRunner;
                }
                else
                {
                    newRunner.Name = "Runner " + ++Count;
                    //set unique name
                    while (mRunSetConfig.GingerRunners.Where(x => x.Name == newRunner.Name).FirstOrDefault() != null)
                    {
                        newRunner.Name = "Runner " + ++Count;
                    }
                }
                newRunner.PropertyChanged -= Runner_PropertyChanged;
                newRunner.PropertyChanged += Runner_PropertyChanged;
                App.RunsetExecutor.InitRunner(newRunner);
                if(Count !=index && index > 0) //TODO : Check if need to add in between runner.
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
                Reporter.ToUser(eUserMsgKey.CannotAddGinger);
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
        
        private void CreateRunSetShortCut(string RunSet, string Env)
        {
            object shDesktop = (object)"Desktop";
            WshShell shell = new WshShell();
            string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\Ginger " + RunSet + " " + Env + ".lnk";
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.Description = "Ginger Solution=" +  WorkSpace.UserProfile.Solution.Name + ", RunSet=" + RunSet + ", Env=" + Env;
            string GingerPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string SolFolder =  WorkSpace.UserProfile.Solution.Folder;
            if (SolFolder.EndsWith(@"\"))
            {
                SolFolder = SolFolder.Substring(0, SolFolder.Length - 1);
            }
            shortcut.TargetPath = GingerPath; 
            string sConfig = "Solution=" + SolFolder + Environment.NewLine;
            sConfig += "Env=" + Env + Environment.NewLine;
            sConfig += "RunSet=" + RunSet + Environment.NewLine;
            string sConfigFile = SolFolder + @"\Documents\RunSetShortCuts\" + RunSet + "_" + Env + ".Ginger.Config";

            if (!System.IO.Directory.Exists(SolFolder + @"\Documents\RunSetShortCuts\")) { System.IO.Directory.CreateDirectory(SolFolder + @"\Documents\RunSetShortCuts\"); }
            System.IO.File.WriteAllText(sConfigFile, sConfig);

            shortcut.Arguments = "ConfigFile=\"" + sConfigFile + "\"";
            shortcut.Save();
            Reporter.ToUser(eUserMsgKey.ShortcutCreated, shortcut.Description);
        }

        private void createShortcutRunset_Click(object sender, RoutedEventArgs e)
        {
            AutoLogProxy.UserOperationStart("CreateShortcutButton_Click");

            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();

            if (mRunSetConfig.Name.IndexOfAny(invalidChars) >= 0 )
            {
                foreach (char value in invalidChars)
                {
                    if(mRunSetConfig.Name.Contains(value))
                        mRunSetConfig.Name = mRunSetConfig.Name.Replace(value, '_');
                }
            }

            CreateRunSetShortCut(mRunSetConfig.Name, xRunsetEnvironmentCombo.Text);

            AutoLogProxy.UserOperationEnd();
        }

        private void analyzerRunset_Click(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentRunnerIsNotRuning()) return;
            AutoLogProxy.UserOperationStart("RunTabAnalyzerButton_Click");

            AnalyzerPage AP = new AnalyzerPage();
            Run.RunSetConfig RSC = mRunSetConfig;
            if (RSC.ContainingFolder == "")
            {
                Reporter.ToUser(eUserMsgKey.AnalyzerSaveRunSet);
                return;
            }
            AP.Init( WorkSpace.UserProfile.Solution, RSC);
            AP.ShowAsWindow();

            AutoLogProxy.UserOperationEnd();
        }

        private async void xRunRunsetBtn_Click(object sender, RoutedEventArgs e)
        {
            AutoLogProxy.UserOperationStart("xRunRunsetBtn_Click");
            ResetALMDefectsSuggestions();

            //run analyzer
            int analyzeRes = await App.RunsetExecutor.RunRunsetAnalyzerBeforeRun().ConfigureAwait(false);
            if (analyzeRes == 1) return;//cancel run because issues found

            //run             
            var result = await App.RunsetExecutor.RunRunsetAsync().ConfigureAwait(false);

            // handling ALM Defects Opening
            ObservableList<ALMDefectProfile> ALMDefectProfiles = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ALMDefectProfile>();
            if ((App.RunsetExecutor.DefectSuggestionsList != null) && (App.RunsetExecutor.DefectSuggestionsList.Count > 0) &&
                (ALMDefectProfiles != null) && (ALMDefectProfiles.Count > 0))
            {
                this.Dispatcher.Invoke(() =>
                {
                    InitALMDefectsOpeningSection();
                });
            }

            AutoLogProxy.UserOperationEnd();
        }

        private void xResetRunsetBtn_Click(object sender, RoutedEventArgs e)
        {
            AutoLogProxy.UserOperationStart("RunsetResetButton_Click");
            ResetALMDefectsSuggestions();
       
            foreach (GingerRunner runner in mRunSetConfig.GingerRunners) //reset only none running Runners to avoid execution issues
            {
                if (runner.IsRunning == false)
                {
                    runner.ResetRunnerExecutionDetails();
                }
            }

            //update RunnersPages stats
            List<FlowElement> fe = mFlowDiagram.GetAllFlowElements();
            foreach (FlowElement f in fe)
            {
                ((RunnerPage)f.GetCustomeShape().Content).UpdateExecutionStats();
                ((RunnerPage)f.GetCustomeShape().Content).xruntime.Content = "00:00:00";
                ((RunnerPage)f.GetCustomeShape().Content).Runner.RunnerExecutionWatch.runWatch.Reset();
            }
            xRuntimeLbl.Content = "00:00:00";
            AutoLogProxy.UserOperationEnd();
        }
        
        private void xStopRunsetBtn_Click(object sender, RoutedEventArgs e)
        {
            AutoLogProxy.UserOperationStart("RunsetStopButton_Click");

            if (RunSetConfig.GingerRunners.Where(x => x.IsRunning == true).FirstOrDefault() == null)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "There are no Running Runners to Stop.");
                return;
            }

            App.RunsetExecutor.StopRun();//stops only running runners

            AutoLogProxy.UserOperationEnd();
        }

        private async void xContinueRunsetBtn_Click(object sender, RoutedEventArgs e)
        {
            AutoLogProxy.UserOperationStart("RunsetContinueButton_Click");

            if (RunSetConfig.GingerRunners.Where(x=>x.Status==eRunStatus.Stopped).FirstOrDefault() == null)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "There are no Stopped Runners to Continue.");
                return;
            }

            //run analyzer
            int analyzeRes = await App.RunsetExecutor.RunRunsetAnalyzerBeforeRun().ConfigureAwait(false);
            if (analyzeRes == 1) return;//cancel run because issues found

            //continue run            
            await App.RunsetExecutor.RunRunsetAsync(true);//doing continue run
            AutoLogProxy.UserOperationEnd();
        }

        private void xRunSetChange_Click(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentRunnerIsNotRuning()) return;
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
            if (CheckCurrentRunnerIsNotRuning()) return;
            AddRunner();
        }
        
        private void clearAllRunner_Click(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentRunnerIsNotRuning()) return;
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

        private void xRunsetReportBtn_Click(object sender, RoutedEventArgs e)
        {
            if(WorkSpace.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder != null)
            {
                ExecutionLoggerConfiguration _selectedExecutionLoggerConfiguration =  WorkSpace.UserProfile.Solution.ExecutionLoggerConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
                HTMLReportsConfiguration currentConf =  WorkSpace.UserProfile.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
                
                string reportsResultFolder = string.Empty;
                if (!_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled)
                {
                    Reporter.ToUser(eUserMsgKey.ExecutionsResultsProdIsNotOn);
                    return;
                }
                if (WorkSpace.RunsetExecutor.RunSetConfig.RunsetExecLoggerPopulated)
                {
                    string runSetFolder = App.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder;
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
                            System.Diagnostics.Process.Start(reportsResultFolder);
                            System.Diagnostics.Process.Start(reportsResultFolder + "\\" + fileName);
                        }
                    }
                }
            }
            else
                ExecutionLogger.GenerateRunSetOfflineReport();

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
            if (CheckCurrentRunnerIsNotRuning()) return;
            AutoLogProxy.UserOperationStart("SaveRunConfigButton_Click");

            SaveRunSetConfig();

            AutoLogProxy.UserOperationEnd();
        }
        
        private void SetExecutionModeIcon()
        {
            if (RunSetConfig.RunModeParallel)
            {
                xExecutionModeBtn.ButtonImageType = eImageType.ParallelExecution;
                xExecutionModeBtn.ToolTip = "Runners Configured to Run in Parallel, Click to Change it to Run in Sequence";
            }
            else
            {
                xExecutionModeBtn.ButtonImageType = eImageType.SequentialExecution;
                xExecutionModeBtn.ToolTip = "Runners Configured to Run in Sequence, Click to Change it to Run in Parallel";
            }
        }
        private void executionMode_Click(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentRunnerIsNotRuning()) return;
            RunSetConfig.RunModeParallel = (!RunSetConfig.RunModeParallel);
            SetExecutionModeIcon();
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
                if(!((BusinessFlow)currentitem.ItemObject).Active)
                {
                    currentitem.xItemName.Foreground = Brushes.Gray;
                }
                else
                {
                    currentitem.xItemName.Foreground = FindResource("$BackgroundColor_DarkBlue") as Brush;
                }                
            }
            if (mCurrentBusinessFlowRunnerItem != null)
            {
                try
                {
                    xActivitiesRunnerItemsLoadingIcon.Visibility = Visibility.Visible;
                    xActivitiesRunnerItemsListView.Visibility = Visibility.Collapsed;
                    General.DoEvents();//for seeing the processing icon better to do with Async

                    //load needes Activities and clear other BF's Activities pages to save memory                 
                    foreach (RunnerItemPage bfPage in mCurrentSelectedRunner.BusinessflowRunnerItems)
                    {
                        if (bfPage == null) continue;

                        if (bfPage == mCurrentBusinessFlowRunnerItem)
                            //load Activities
                            xActivitiesRunnerItemsListView.ItemsSource = bfPage.ItemChilds;
                        else
                            bfPage.ClearItemChilds();
                    }
                    GC.Collect();//to help with memory free
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
            }
            else
            {
                xActivitiesRunnerItemsListView.ItemsSource = null;
            }
        }

        private void xActivitiesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                ListView view = sender as ListView;
                view.ScrollIntoView(view.SelectedItem);
            });

            foreach (RunnerItemPage currentitem in xActivitiesRunnerItemsListView.Items)
            {
                currentitem.xItemName.Foreground = FindResource("$BackgroundColor_DarkBlue") as Brush;
            }
            if (mCurrentActivityRunnerItem != null)
            {
                try
                {
                    xActionsRunnerItemsLoadingIcon.Visibility = Visibility.Visible;
                    xActionsRunnerItemsListView.Visibility = Visibility.Collapsed;
                    General.DoEvents();//for seeing the processing icon better to it with Async
                    //load items
                    xActionsRunnerItemsListView.ItemsSource = mCurrentActivityRunnerItem.ItemChilds;
                }
                finally
                {
                    xActionsRunnerItemsLoadingIcon.Visibility = Visibility.Collapsed;
                    xActionsRunnerItemsListView.Visibility = Visibility.Visible;
                }

                mCurrentActivityRunnerItem.xItemName.Foreground = FindResource("$SelectionColor_Pink") as Brush;
            }
            else
            {
                xActionsRunnerItemsListView.ItemsSource = null;
            }
            xActionsName.Content = "Actions (" + xActionsRunnerItemsListView.Items.Count + ")";
            SetHeighlightActionRunnerItem();
        }
        private void SetHeighlightActionRunnerItem()
        {
            this.Dispatcher.Invoke(() =>
            {
                ListView view = xActionsRunnerItemsListView as ListView;
                view.ScrollIntoView(view.SelectedItem);
            });

            foreach (RunnerItemPage currentitem in xActionsRunnerItemsListView.Items)
            {
                currentitem.xItemName.Foreground = FindResource("$BackgroundColor_DarkBlue") as Brush;
            }
            //Need it for first time.
            if (xActionsRunnerItemsListView.SelectedItem == null)
            {
                xActionsRunnerItemsListView.SelectedIndex = 0;
            }
            if(mCurrentActionRunnerItem!=null)
            {
                mCurrentActionRunnerItem.xItemName.Foreground = FindResource("$SelectionColor_Pink") as Brush;
            }
        }
        private void xActionsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetHeighlightActionRunnerItem();
        }

        private bool CheckCurrentRunnerIsNotRuning()
        {
            if (mCurrentSelectedRunner != null && mCurrentSelectedRunner.Runner != null)
            {
                if (mCurrentSelectedRunner.Runner.Status == eRunStatus.Running)
                {
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Please wait for Runner to complete run.");
                    return true;
                }
            }

            return false;
        }
        private void xaddBusinessflow_Click(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentRunnerIsNotRuning()) return;

            BusinessFlowsFolderTreeItem bfsFolder;
            
            bfsFolder = new BusinessFlowsFolderTreeItem(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<BusinessFlow>());                        
            mBusFlowsSelectionPage = new SingleItemTreeViewSelectionPage(GingerDicser.GetTermResValue(eTermResKey.BusinessFlows), eImageType.BusinessFlow, bfsFolder, SingleItemTreeViewSelectionPage.eItemSelectionType.MultiStayOpenOnDoubleClick, false);
            mBusFlowsSelectionPage.SelectionDone += MBusFlowsSelectionPage_SelectionDone;
            
            List<object> selectedBfs = mBusFlowsSelectionPage.ShowAsWindow();
            AddSelectedBuinessFlows(selectedBfs);
        }

        private void AddSelectedBuinessFlows(List<object> selectedBfs)
        {
            if (selectedBfs != null && selectedBfs.Count > 0)
            {
                foreach (BusinessFlow bf in selectedBfs)
                {
                    BusinessFlow bfToAdd = (BusinessFlow)bf.CreateCopy(false);
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
            if (CheckCurrentRunnerIsNotRuning()) return;

            if (mCurrentSelectedRunner.Runner.BusinessFlows.Count > 0)
            {
                if (Reporter.ToUser(eUserMsgKey.DeleteBusinessflows) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                {
                    mCurrentSelectedRunner.Runner.BusinessFlows.Clear();
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
            if(mCurrentActivityRunnerItem!=null)
            {
                viewActivity((Activity)(mCurrentActivityRunnerItem).ItemObject);
            }
        }

        private void xActionsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(mCurrentActionRunnerItem!=null)
            {
                viewAction((Act)(mCurrentActionRunnerItem).ItemObject);
            }
        }

        public void viewBusinessflow(BusinessFlow businessFlow)
        {
            BusinessFlowPage w = new BusinessFlowPage(businessFlow, false, General.RepositoryItemPageViewMode.View);
            w.Width = 1000;
            w.Height = 800;
            w.ShowAsWindow();
        }
        public void viewBusinessflowConfiguration(BusinessFlow businessFlow)
        {
            //BusinessFlow bf = (BusinessFlow)(mCurrentBusinessFlowRunnerItem).ItemObject;
            ObservableList<BusinessFlow> prevBFs = new ObservableList<BusinessFlow>();
            for (int i = 0; i < mCurrentSelectedRunner.BusinessflowRunnerItems.IndexOf(mCurrentBusinessFlowRunnerItem); i++)
                prevBFs.Add((BusinessFlow)((RunnerItemPage)mCurrentSelectedRunner.BusinessflowRunnerItems[i]).ItemObject);
            BusinessFlowRunConfigurationsPage varsPage = new BusinessFlowRunConfigurationsPage(mCurrentSelectedRunner.Runner, businessFlow, prevBFs);
            varsPage.ShowAsWindow();
        }
        public void viewActivity(Activity activitytoView)
        {
            Activity ac = activitytoView;
            BusinessFlowWindows.ActivityEditPage w = new BusinessFlowWindows.ActivityEditPage(ac, General.RepositoryItemPageViewMode.View,mCurrentBusinessFlowRunnerItemObject);
            w.ShowAsWindow();
        }

        public void viewAction(Act actiontoView)
        {
            Act act = actiontoView;
            ActionEditPage w = new ActionEditPage(act, General.RepositoryItemPageViewMode.View,mCurrentBusinessFlowRunnerItemObject,mCurrentActivityRunnerItemObject);
            w.ShowAsWindow();
        }

        private void xremoveBusinessflow_Click(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentRunnerIsNotRuning()) return;

            if (mCurrentBusinessFlowRunnerItem != null)
            {
                if (Reporter.ToUser(eUserMsgKey.DeleteBusinessflow) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                {
                    BusinessFlow bff = (BusinessFlow)(mCurrentBusinessFlowRunnerItem).ItemObject;
                    mCurrentSelectedRunner.Runner.BusinessFlows.Remove(bff);
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.SelectItemToDelete);
            }
        }

        private void removeRunner(GingerRunner runner)
        {
            if (CheckCurrentRunnerIsNotRuning()) return;
            if (mRunSetConfig.GingerRunners.Count == 1)
            {
                Reporter.ToUser(eUserMsgKey.CantDeleteRunner);
                return;
            }
            if (runner != null)
            {
                if (Reporter.ToUser(eUserMsgKey.DeleteRunner) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                {
                    int index = mRunSetConfig.GingerRunners.IndexOf(runner);
                    List<FlowElement> fe = mFlowDiagram.GetAllFlowElements();
                    mRunSetConfig.GingerRunners.Remove(runner);
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
            if (CheckCurrentRunnerIsNotRuning()) return;

            if (mCurrentBusinessFlowRunnerItem != null)
            {
                BusinessFlow bf = (BusinessFlow)mCurrentBusinessFlowRunnerItem.ItemObject;
                int Indx = mCurrentSelectedRunner.Runner.BusinessFlows.IndexOf(bf);

                if (Indx > 0)
                    mCurrentSelectedRunner.Runner.BusinessFlows.Move(Indx, Indx - 1);
                else
                    return;
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
        }

        private void xmovedownBusinessflow_Click(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentRunnerIsNotRuning()) return;

            if (mCurrentBusinessFlowRunnerItem != null)
            {
                BusinessFlow bf = (BusinessFlow)mCurrentBusinessFlowRunnerItem.ItemObject;
                int Indx = mCurrentSelectedRunner.Runner.BusinessFlows.IndexOf(bf);
                if (Indx < (mCurrentSelectedRunner.Runner.BusinessFlows.Count - 1))
                    mCurrentSelectedRunner.Runner.BusinessFlows.Move(Indx, Indx + 1);
                else
                    return;
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
            if (CheckCurrentRunnerIsNotRuning()) return;

            if (mCurrentBusinessFlowRunnerItem != null)
            {
                BusinessFlow bf = (BusinessFlow)mCurrentBusinessFlowRunnerItem.ItemObject;
                BusinessFlow bCopy = (BusinessFlow)bf.CreateCopy(false);                
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
            if (CheckCurrentRunnerIsNotRuning()) return;
            AddNewRunSetConfig();
            mRunSetsSelectionPage = null;//for new run set to be shown 
        }

        // if page has been loaded from any other tab to Runset tab, it will show user message to refresh the runset
        private void Page_loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                xRunnersFrame.Refresh();
                xRunnersFrame.NavigationService.Refresh();

                if (mSolutionWasChanged)
                {
                    ResetLoadedRunSet();
                    mSolutionWasChanged = false;
                }
                else if (mRunSetBusinessFlowWasChanged)
                {
                    if (Reporter.ToUser(eUserMsgKey.RunsetBuinessFlowWasChanged) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                        RefreshCurrentRunSet();
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
            if (CheckCurrentRunnerIsNotRuning()) return;
            if (Reporter.ToUser(eUserMsgKey.RunSetReloadhWarn) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                RefreshCurrentRunSet();
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


        private void duplicateRunner(GingerRunner runner)
        {
            if (CheckCurrentRunnerIsNotRuning()) return;
            if (runner != null)
            {
                GingerRunner GR = (GingerRunner)runner;
                GingerRunner GRCopy = (GingerRunner)GR.CreateCopy(false);               
                GRCopy.Guid = Guid.NewGuid();
                GRCopy.ParentGuid = GR.Guid;
                List<string> runnerNamesList = (from grs in RunSetConfig.GingerRunners select grs.Name).ToList<string>();
                GRCopy.Name = General.GetItemUniqueName(GR.Name, runnerNamesList);
                App.RunsetExecutor.InitRunner(GRCopy);
                AddRunner(GRCopy);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
        }
        private void xRunnerDetailView_Click(object sender, RoutedEventArgs e)
        {
            if (xRunnersGrid.RowDefinitions[1].Height.Value == 0)
            {               
                xRunnersGrid.RowDefinitions[1].Height = new GridLength(270);
                xRunnerDetailViewBtn.ButtonImageType = eImageType.CollapseAll;
                controls.Visibility = Visibility.Visible;
                xRunnersCombo.Visibility = Visibility.Collapsed;
                xRunnerNamelbl.Visibility = Visibility.Visible;
                xOtherControlsDocPanel.Visibility = Visibility.Visible;
                xActionsRunnerItemsGrid.RowDefinitions[1].Height = new GridLength(375);
                xActivitiesRunnerItemsGrid.RowDefinitions[1].Height = new GridLength(375);
                xBusinessFlowsRunnerItemsGrid.RowDefinitions[1].Height = new GridLength(375);
                xRunnerButtonsDocPanel.Visibility = Visibility.Collapsed;
                xRunnerInfoDocPanel.Visibility = Visibility.Collapsed;
                xRunnersFrame.Visibility = Visibility.Visible;                
            }
            else
            {               
                xRunnersGrid.RowDefinitions[1].Height = new GridLength(0);
                xRunnerDetailViewBtn.ButtonImageType = eImageType.ExpandAll;
                controls.Visibility = Visibility.Collapsed;
                xRunnersCombo.Visibility = Visibility.Visible;
                xRunnerNamelbl.Visibility = Visibility.Collapsed;
                xOtherControlsDocPanel.Visibility = Visibility.Collapsed;
                xActionsRunnerItemsGrid.RowDefinitions[1].Height = new GridLength(640);
                xActivitiesRunnerItemsGrid.RowDefinitions[1].Height = new GridLength(640);
                xBusinessFlowsRunnerItemsGrid.RowDefinitions[1].Height = new GridLength(640);
                xRunnerButtonsDocPanel.Visibility = Visibility.Visible;
                xRunnerInfoDocPanel.Visibility = Visibility.Visible;
                xRunnersFrame.Visibility = Visibility.Collapsed;
                SetComboRunnerInitialView();
            }
        }
        private void SetComboRunnerInitialView()
        {
            xRunnersCombo.SelectionChanged -= xRunnersCombo_SelectionChanged;
            xRunnersCombo.SelectedItem = mCurrentSelectedRunner.Runner;
            xRunnersCombo.SelectionChanged += xRunnersCombo_SelectionChanged;
        }
        private void xRunnersCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xRunnersCombo.SelectedValue == null)
                return;
            if (mCurrentSelectedRunner.Runner.Guid == (Guid)xRunnersCombo.SelectedValue)
                return;
            GingerRunner SelectedRunner = (GingerRunner) mRunSetConfig.GingerRunners.Where(x => x.Guid.ToString() == xRunnersCombo.SelectedValue.ToString()).FirstOrDefault();
            int index = mRunSetConfig.GingerRunners.IndexOf(SelectedRunner);
            List<FlowElement> fe = mFlowDiagram.GetAllFlowElements();
            GingerRunnerHighlight(((RunnerPage)fe[index].GetCustomeShape().Content));
        }

        private void xBusinessflowsDetailView_Click(object sender, RoutedEventArgs e)
        {
            if (xBusinessflowsRunnerItemsListView == null || xBusinessflowsRunnerItemsListView.Items.Count == 0) return;

            RunnerItemPage rii = (RunnerItemPage)xBusinessflowsRunnerItemsListView.Items[0];
            bool isExpand = false;
            if (rii.pageGrid.RowDefinitions[1].Height.Value == 0)
                isExpand = true;
            else
                isExpand = false;
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
            if (CheckCurrentRunnerIsNotRuning()) return;
            bool SetBusinessflowActive=false;
            if (xBusinessflowsRunnerItemsListView.Items.Count>0)
            {
                BusinessFlow bff = (BusinessFlow)((RunnerItemPage)xBusinessflowsRunnerItemsListView.Items[0]).ItemObject;
                SetBusinessflowActive = !bff.Active;

                foreach (RunnerItemPage ri in xBusinessflowsRunnerItemsListView.Items)
                {
                    BusinessFlow bf = (BusinessFlow)((RunnerItemPage)ri).ItemObject;
                    bf.Active = SetBusinessflowActive;
                    if (SetBusinessflowActive)
                    {
                        ((RunnerItemPage)ri).xItemName.Foreground = FindResource("$BackgroundColor_DarkBlue") as Brush;
                    }
                    else
                    {
                        ((RunnerItemPage)ri).xItemName.Foreground = Brushes.Gray;
                    }
                }
            }                                 
        }

        private void xActivitiesDetailView_Click(object sender, RoutedEventArgs e)
        {
            if (xActivitiesRunnerItemsListView == null || xActivitiesRunnerItemsListView.Items.Count == 0) return;

            RunnerItemPage rii = (RunnerItemPage)xActivitiesRunnerItemsListView.Items[0];
            bool isExpand = false;
            if (rii.pageGrid.RowDefinitions[1].Height.Value == 0)
                isExpand = true;
            else
                isExpand = false;
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
            if (xActionsRunnerItemsListView == null || xActionsRunnerItemsListView.Items.Count == 0) return;

            RunnerItemPage rii =(RunnerItemPage)xActionsRunnerItemsListView.Items[0];
            bool isExpand = false;
            if (rii.pageGrid.RowDefinitions[1].Height.Value == 0)
                isExpand = true;
            else
                isExpand = false;
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
            if (mCurrentSelectedRunner.Runner.IsRunning)
            {
                UpdateRunnerTime();
            }
        }
        private void UpdateRunnerTime()
        {            
            xRuntimeLbl.Content = mCurrentSelectedRunner.Runner.RunnerExecutionWatch.runWatch.Elapsed.ToString(@"hh\:mm\:ss");         
        }
        private void xRunnersActive_Click(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentRunnerIsNotRuning()) return;
            bool SetRunnerActive = false;
            List<FlowElement> fe = mFlowDiagram.GetAllFlowElements();           
            if (fe.Count > 0)
            {
                GingerRunner grr = (GingerRunner)((RunnerPage)fe[0].GetCustomeShape().Content).Runner;
                SetRunnerActive = !grr.Active;
            }
            foreach (FlowElement rp in fe)
            {
                GingerRunner gr = (GingerRunner)((RunnerPage)rp.GetCustomeShape().Content).Runner;
                gr.Active = SetRunnerActive;
                if (SetRunnerActive)
                {
                    ((RunnerPage)rp.GetCustomeShape().Content).xRunnerNameTxtBlock.Foreground = FindResource("$BackgroundColor_DarkBlue") as Brush;
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
            ObservableList<Button> winButtons = new ObservableList<Button>();
            if (mEditMode)
            {
                title = "View " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);

            }

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, title, this, winButtons, true, "Close");
        }

        private void xExportToAlmBtn_Click(object sender, RoutedEventArgs e)
        {            
            ObservableList<BusinessFlow> bfs = new ObservableList<BusinessFlow>();
            
            foreach (GingerRunner GR in App.RunsetExecutor.Runners)
            {
                bfs.Append(GR.BusinessFlows);                
            }
            if (!ExportResultsToALMConfigPage.Instance.IsProcessing)
            {
                ExportResultsToALMConfigPage.Instance.Init(bfs, new GingerCore.ValueExpression(App.RunsetExecutor.RunsetExecutionEnvironment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>(), false, "", false,  WorkSpace.UserProfile.Solution.Variables));
                ExportResultsToALMConfigPage.Instance.ShowAsWindow();
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.ExportedExecDetailsToALMIsInProcess);
            }
        }

        FindAndReplacePage mfindAndReplacePageRunSet = null;

        private void xFind_Click(object sender, RoutedEventArgs e)
        {
            ShowFindAndReplacePage();
        }

        public void ShowFindAndReplacePage()
        {
            if (mfindAndReplacePageRunSet == null)
            {
                mfindAndReplacePageRunSet = new FindAndReplacePage(FindAndReplacePage.eContext.RunsetPage);

            }
            mfindAndReplacePageRunSet.ShowAsWindow();
        }
    }
}
