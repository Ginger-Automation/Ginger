#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using Amdocs.Ginger.Common.Repository.BusinessFlowLib;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.LiteDBFolder;
using Amdocs.Ginger.CoreNET.Logger;
using Amdocs.Ginger.CoreNET.Run.RunListenerLib;
using Amdocs.Ginger.UserControls;
using Ginger.MoveToGingerWPF.Run_Set_Pages;
using Ginger.Reports;
using Ginger.UserControlsLib.PieChart;
using GingerCore;
using GingerCore.Environments;
using GingerCore.Helpers;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Ginger.Run
{
    /// <summary>
    /// Interaction logic for GingerRunnerPage.xaml
    /// </summary>
    public partial class RunnerPage : Page
    {
        DispatcherTimer mDispatcherTimer;

        public event RunnerPageEventHandler RunnerPageEvent;
        public delegate void RunnerPageEventHandler(RunnerPageEventArgs EventArgs);
        public event EventHandler CheckIfRunsetDirty;

        public void OnGingerRunnerEvent(RunnerPageEventArgs.eEventType EvType, Object obj)
        {
            RunnerPageEventHandler handler = RunnerPageEvent;
            if (handler != null)
            {
                handler(new RunnerPageEventArgs(EvType, obj));
            }
        }

        public TextBlock bfStat()
        {
            return xBusinessflowsStatistics;
        }

        RunnerPageListener mRunnerPageListener;
        public RunnerPageListener RunnerPageListener
        {
            get
            {
                return mRunnerPageListener;
            }
        }

        GingerExecutionEngine mExecutorEngine;
        public GingerExecutionEngine ExecutorEngine
        {
            get
            {
                return mExecutorEngine;
            }
        }

        public enum eObjectType
        {
            BusinessFlow,
            Activity,
            Action,
            Legend
        }
        StatusColorSelector ColorSelector = new StatusColorSelector();
        ObservableList<BusinessFlow> mBFESs;
        ObservableList<RunnerItemPage> mBusinessflowRunnerItems = null;
        public ObservableList<RunnerItemPage> BusinessflowRunnerItems
        {
            get
            {
                if (mBusinessflowRunnerItems == null)
                {
                    LoadBusinessflowRunnerItem(ViewMode1);
                }

                return mBusinessflowRunnerItems;
            }
        }

        public void ClearBusinessflowRunnerItems()
        {
            if (mBusinessflowRunnerItems != null)
            {
                for (int i = 0; i < mBusinessflowRunnerItems.Count; i++)
                {
                    RunnerItemPage page = mBusinessflowRunnerItems[i];
                    page.ClearBindings();
                    page = null;//to make sure memory gets free
                }
                mBusinessflowRunnerItems.Clear();
                mBusinessflowRunnerItems = null;
            }
        }

        private ObservableList<RunnerItemPage> mActivityRunnerItem;
        public ObservableList<RunnerItemPage> ActivityRunnerItem
        {
            get
            {
                return mActivityRunnerItem;
            }
            set
            {
                mActivityRunnerItem = value;
            }
        }
        private ObservableList<RunnerItemPage> mActionRunnerItem;
        public ObservableList<RunnerItemPage> ActionRunnerItem
        {
            get
            {
                return mActionRunnerItem;
            }
            set
            {
                mActionRunnerItem = value;
            }
        }
        enum ChartType
        {
            Businessflow,
            Activity,
            Action
        }
        public string totalCount { get; set; }
        bool mGiveUserFeedback { get; set; }
        HTMLReportsConfiguration currentConf = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.FirstOrDefault(x => (x.IsSelected == true));
        ChartType SelectedChartType { get; set; }
        public bool ViewMode1 = false;

        Context mContext = null;
        private readonly EventHandler<RunnerItemEventArgs>? _runnerItemEventHandler;

        public RunnerPage(EventHandler<RunnerItemEventArgs>? runnerItemEventHandler = null)
        {
            InitializeComponent();
            _runnerItemEventHandler = runnerItemEventHandler;
        }

        public void Init(GingerExecutionEngine runner, Context context, bool Viewmode = false)
        {
            Clear();
            mExecutorEngine = runner;
            mContext = context;
            ViewMode1 = Viewmode;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xBusinessflowsTotalCount, Label.ContentProperty, mExecutorEngine, nameof(GingerExecutionEngine.TotalBusinessflow));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xStatus, StatusItem.StatusProperty, mExecutorEngine.GingerRunner, nameof(GingerRunner.Status), BindingMode.OneWay);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xRunnerActive, ucButton.ButtonImageTypeProperty, mExecutorEngine.GingerRunner, nameof(GingerRunner.Active), bindingConvertor: new ActiveIconConverter(), BindingMode.TwoWay);
            UpdateRunnerInfo();
            if (Viewmode)
            {
                pageGrid.IsEnabled = false;
            }

            mDispatcherTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 1) // one second
            };
            WeakEventManager<DispatcherTimer, EventArgs>.AddHandler(source: mDispatcherTimer, eventName: nameof(DispatcherTimer.Tick), handler: dispatcherTimerElapsedTick);
            mDispatcherTimer.Start();
            UpdateExecutionStats();

            mRunnerPageListener = new RunnerPageListener
            {
                UpdateStat = HandleUpdateStat
            };
            runner.RunListeners.Add(mRunnerPageListener);

            if (WorkSpace.Instance.RunningInExecutionMode)
            {
                xExecutionOperationsPnl.Visibility = Visibility.Collapsed;
                xOperationsPnl.Visibility = Visibility.Collapsed;
            }
        }

        public void Clear()
        {
            BindingOperations.ClearBinding(xBusinessflowsTotalCount, Label.ContentProperty);
            BindingOperations.ClearBinding(xStatus, StatusItem.StatusProperty);
            BindingOperations.ClearBinding(xRunnerActive, ucButton.ButtonImageTypeProperty);
            if (ViewMode1)
            {
                pageGrid.IsEnabled = false;
            }

            if (mDispatcherTimer != null)
            {
                mDispatcherTimer.Stop();
                mDispatcherTimer.Tick -= dispatcherTimerElapsedTick;
                mDispatcherTimer = null;
            }

            if (mRunnerPageListener != null)
            {
                mExecutorEngine.RunListeners.Remove(mRunnerPageListener);
                mRunnerPageListener.UpdateStat = null;
                mRunnerPageListener = null;
            }

            if (WorkSpace.Instance.RunningInExecutionMode)
            {
                xExecutionOperationsPnl.Visibility = Visibility.Collapsed;
                xOperationsPnl.Visibility = Visibility.Collapsed;
            }

            mBusinessflowRunnerItems = null;
            mExecutorEngine = null;
            mContext = null;
            ViewMode1 = false;
        }

        private void HandleUpdateStat(object sender, EventArgs e)
        {
            mGiveUserFeedback = true;
        }

        private RunnerItemPage CreateBusinessFlowRunnerItem(BusinessFlow bf, bool ViewMode = false)
        {
            RunnerItemPage ri = new RunnerItemPage(Runnerobj: bf, ViewMode: ViewMode1, runnerItemEventHandler: _runnerItemEventHandler)
            {
                ItemName = bf.Name,
                ItemTitleTooltip = string.Format(@"{0}\{1}", bf.ContainingFolder, bf.Name)
            };
            if (string.IsNullOrEmpty(bf.Description))
            {
                ri.xItemSeparator.Visibility = Visibility.Collapsed;
            }
            else
            {
                ri.xItemSeparator.Visibility = Visibility.Visible;
            }
            ri.ItemDescription = bf.Description;
            ri.ItemGuid = bf.Guid;
            if (!WorkSpace.Instance.RunningInExecutionMode)
            {
                ri.xautomateBusinessflow.Visibility = ri.xconfig.Visibility = Visibility.Visible;
                ri.xBusinessflowActive.Visibility = Visibility.Visible;
                ri.xGenerateReport.Visibility = Visibility.Visible;
                ri.xViewRunnerItem.Visibility = Visibility.Visible;
                WeakEventManager<RunnerItemPage, RoutedEventArgs>.AddHandler(source: ri, eventName: nameof(RunnerItemPage.Click), handler: BusinessflowConfig_Click);
                WeakEventManager<RunnerItemPage, RoutedEventArgs>.AddHandler(source: ri, eventName: nameof(RunnerItemPage.ClickAutomate), handler: Businessflow_ClickAutomate);
                WeakEventManager<RunnerItemPage, RoutedEventArgs>.AddHandler(source: ri, eventName: nameof(RunnerItemPage.ClickActive), handler: Businessflow_ClickActive);
                WeakEventManager<RunnerItemPage, RoutedEventArgs>.AddHandler(source: ri, eventName: nameof(RunnerItemPage.ClickGenerateReport), handler: Businessflow_ClickGenerateReport);
                WeakEventManager<RunnerItemPage, RoutedEventArgs>.AddHandler(source: ri, eventName: nameof(RunnerItemPage.DuplicateClick), handler: Businessflow_DuplicateClick);
                WeakEventManager<RunnerItemPage, RoutedEventArgs>.AddHandler(source: ri, eventName: nameof(RunnerItemPage.RemoveClick), handler: Businessflow_RemoveClick);
                WeakEventManager<RunnerItemPage, RoutedEventArgs>.AddHandler(source: ri, eventName: nameof(RunnerItemPage.ResetBusinessFlowStatus), handler: BusinessFlow_ResetStatus);



                ri.xRunnerItemMenu.Visibility = Visibility.Visible;
                ri.xremoveBusinessflow.Visibility = Visibility.Visible;
                ri.pageGrid.RowDefinitions[1].Height = new GridLength(30);
                ri.xRunnerItemButtons.Visibility = Visibility.Visible;
                ri.xDetailView.ButtonImageType = eImageType.Collapse;
                ri.xDetailView.ToolTip = "Expand / Collapse " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
            }
            ri.Context = new Context() { BusinessFlow = bf, Runner = mExecutorEngine, Environment = GetEnvForContext() };
            return ri;
        }

        private void BusinessFlow_ResetStatus(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentRunnerIsNotRuning())
            {
                return;
            }

            BusinessFlow bff = (BusinessFlow)((RunnerItemPage)sender).ItemObject;
            bff.Reset();
        }

        private void Businessflow_RemoveClick(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentRunnerIsNotRuning())
            {
                return;
            }

            if (Reporter.ToUser(eUserMsgKey.DeleteBusinessflow) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                BusinessFlow bff = (BusinessFlow)((RunnerItemPage)sender).ItemObject;
                ExecutorEngine.BusinessFlows.Remove(bff);
                mExecutorEngine.GingerRunner.DirtyStatus = eDirtyStatus.Modified;
            }
        }

        private void Businessflow_DuplicateClick(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentRunnerIsNotRuning())
            {
                return;
            }

            BusinessFlow bf = (BusinessFlow)((RunnerItemPage)sender).ItemObject;
            BusinessFlow bCopy = (BusinessFlow)bf.CreateCopy(false);
            bCopy.InstanceGuid = Guid.NewGuid();
            bCopy.Reset();
            AddBuinessFlowToRunner(bCopy, 1);
        }

        public void AddBuinessFlowToRunner(BusinessFlow bf, int indexToInsertTo = -1)
        {
            //add bf to runner items list
            mBusinessflowRunnerItems.Add(CreateBusinessFlowRunnerItem(bf));
            //move after current item and add to Runner BF's list in same location
            if (indexToInsertTo > 0)
            {
                mBusinessflowRunnerItems.Move(mBusinessflowRunnerItems.Count - 1, indexToInsertTo);
                mExecutorEngine.BusinessFlows.Insert(indexToInsertTo, bf);
            }
            else
            {
                mExecutorEngine.BusinessFlows.Add(bf);
            }
            mExecutorEngine.GingerRunner.DirtyStatus = eDirtyStatus.Modified;
        }

        public void LoadBusinessflowRunnerItem(bool ViewMode = false)
        {
            mBusinessflowRunnerItems = [];
            foreach (BusinessFlow bff in mExecutorEngine.BusinessFlows)
            {
                RunnerItemPage bfItem = CreateBusinessFlowRunnerItem(bff, ViewMode);
                mBusinessflowRunnerItems.Add(bfItem);
            }
        }

        private ProjEnvironment GetEnvForContext()
        {
            if (mExecutorEngine.GingerRunner.UseSpecificEnvironment)
            {
                return mExecutorEngine.GingerRunner.ProjEnvironment;
            }
            else
            {
                return mContext.Environment;
            }
        }

        private void Businessflow_ClickGenerateReport(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentRunnerIsNotRuning())
            {
                return;
            }

            BusinessFlow bf = (BusinessFlow)((RunnerItemPage)sender).ItemObject;
            if (mExecutorEngine.ExecutionLoggerManager.Configuration.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
            {
                GenerateBFReport(bf);
                return;
            }

            ExecutionLoggerConfiguration _selectedExecutionLoggerConfiguration = WorkSpace.Instance.Solution.LoggerConfigurations;
            HTMLReportsConfiguration currentConf = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.FirstOrDefault(x => (x.IsSelected == true));
            if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder != null)
            {
                string reportpath = ((BusinessFlow)((RunnerItemPage)sender).ItemObject).ExecutionFullLogFolder;
                string reportsResultFolder = string.Empty;
                if (!string.IsNullOrEmpty(reportpath))
                {
                    reportsResultFolder = Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(reportpath), false, null, currentConf.HTMLReportsFolder + Ginger.Run.ExecutionLoggerManager.defaultRunTabBFName + Ginger.Reports.GingerExecutionReport.ExtensionMethods.folderNameNormalazing(bf.Name));
                }

                if (!_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled)
                {
                    Reporter.ToUser(eUserMsgKey.ExecutionsResultsProdIsNotOn);
                    return;
                }
                else if (reportsResultFolder == string.Empty)
                {
                    Reporter.ToUser(eUserMsgKey.ExecutionsResultsNotExists);
                    return;
                }
                else
                {
                    foreach (string txt_file in System.IO.Directory.GetFiles(reportsResultFolder))
                    {
                        string fileName = System.IO.Path.GetFileName(txt_file);
                        if (fileName.Contains(".html"))
                        {
                            Process.Start(new ProcessStartInfo() { FileName = reportsResultFolder, UseShellExecute = true });
                            Process.Start(new ProcessStartInfo() { FileName = reportsResultFolder + "\\" + fileName, UseShellExecute = true });
                        }
                    }
                }
            }
            else
            {
                Context context = new Context
                {
                    BusinessFlow = bf,
                    Runner = mExecutorEngine,
                    Environment = mExecutorEngine.GingerRunner.ProjEnvironment
                };
                mExecutorEngine.ExecutionLoggerManager.GenerateBusinessFlowOfflineReport(context, currentConf.HTMLReportsFolder + bf.Name, WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name);
            }
        }

        private void GenerateBFReport(BusinessFlow bf)
        {
            try
            {
                LiteDbManager dbManager = new LiteDbManager(WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder));
                var result = dbManager.GetRunSetLiteData();

                List<LiteDbRunSet> filterData = dbManager.FilterCollection<LiteDbRunSet>(LiteDbRunSet.IncludeAllReferences(result), Query.All());

                LiteDbRunSet runSetLast = filterData.Last();
                //runSetLast._id = new ObjectId();

                LiteDbRunner runnerFilter = runSetLast.RunnersColl.Find(r => r.GUID.ToString() == mExecutorEngine.GingerRunner.Guid.ToString());
                //runnerFilter._id = new ObjectId();
                //runSetLast.RunnersColl = new List<LiteDbRunner>() { runnerFilter };

                LiteDbBusinessFlow bfFilter = runnerFilter.BusinessFlowsColl.FirstOrDefault(b => b.GUID.ToString() == bf.Guid.ToString() && b.StartTimeStamp.ToString() == bf.StartTimeStamp.ToLocalTime().ToString());
                if (bfFilter == null)
                {
                    Reporter.ToUser(eUserMsgKey.BFNotExistInDB);
                    return;
                }
                //runnerFilter.RunStatus = bfFilter.RunStatus;
                //runSetLast.RunStatus = runnerFilter.RunStatus;
                //runnerFilter.BusinessFlowsColl = new List<LiteDbBusinessFlow>() { bfFilter };

                //dbManager.WriteToLiteDb(dbManager.NameInDb<LiteDbRunner>(), new List<LiteDbReportBase>() { runnerFilter });
                //dbManager.WriteToLiteDb(dbManager.NameInDb<LiteDbRunSet>(), new List<LiteDbReportBase>() { runSetLast });


                WebReportGenerator webReporterRunner = new WebReportGenerator();
                webReporterRunner.RunNewHtmlReport(string.Empty, runSetLast._id.ToString(), new WebReportFilter() { Guid = bfFilter.GUID.ToString() });

                //var newRSData = dbManager.GetRunSetLiteData();
                //newRSData.Delete(runSetLast._id);
                //var newRunnerData = dbManager.GetRunnerLiteData();
                //newRunnerData.Delete(runnerFilter._id);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to generate business flow report", ex);
            }
        }

        private void Businessflow_ClickActive(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentRunnerIsNotRuning())
            {
                return;
            }

            BusinessFlow bf = (BusinessFlow)((RunnerItemPage)sender).ItemObject;
            bf.Active = !bf.Active;
            if (!bf.Active)
            {
                ((RunnerItemPage)sender).xItemName.Foreground = Brushes.Gray;
            }
            else
            {
                ((RunnerItemPage)sender).xItemName.Foreground = FindResource("$BackgroundColor_Black") as Brush;
            }
            mExecutorEngine.GingerRunner.DirtyStatus = eDirtyStatus.Modified;
        }
        private void Businessflow_ClickAutomate(object sender, RoutedEventArgs e)
        {
            BusinessFlow bf = (BusinessFlow)((RunnerItemPage)sender).ItemObject;
            BusinessFlow actualBf = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<BusinessFlow>(bf.Guid);
            if (actualBf != null)
            {
                actualBf.StartDirtyTracking();
                App.OnAutomateBusinessFlowEvent(BusinessFlowWindows.AutomateEventArgs.eEventType.Automate, actualBf);
            }
        }
        private void dispatcherTimerElapsedTick(object sender, EventArgs e)
        {
            if (mGiveUserFeedback)
            {
                Task.Factory.StartNew(() =>
                {
                    UpdateExecutionStats();

                });
                mGiveUserFeedback = false;
            }

            if (mExecutorEngine.IsRunning)
            {
                xruntime.Content = mExecutorEngine.RunnerExecutionWatch.runWatch.Elapsed.ToString(@"hh\:mm\:ss");
            }
        }

        private bool CheckCurrentRunnerIsNotRuning()
        {
            if (ExecutorEngine != null)
            {
                if (ExecutorEngine.GingerRunner.Status == eRunStatus.Running)
                {
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Please wait for Runner to complete run.");
                    return true;
                }
            }

            return false;
        }

        private void BusinessflowConfig_Click(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentRunnerIsNotRuning())
            {
                e.Handled = false;
                return;
            }
        }
        //Contains event fired for Change of BusinessFlow/Activities/Actions changed
        private void UpdateBusinessFlowGrid()
        {
        }
        public void UpdateExecutionStats()
        {
            //TODO: break to smaller funcs            
            List<GingerCoreNET.GeneralLib.StatItem> allItems = [];
            int totalBusinessFlowCount = 0;
            int totalActivityCount = 0;
            int totalActionCount = 0;

            try//Added try catch, to catch the exception caused when acts object is modified by other thread
            {
                this.Dispatcher.Invoke(() =>
                {
                    mBFESs = mExecutorEngine.BusinessFlows;
                    //Business Flows
                    List<GingerCoreNET.GeneralLib.StatItem> bizsList = [];
                    var bizGroups = mBFESs
                    .GroupBy(n => n.RunStatus)
                    .Select(n => new
                    {
                        Status = n.Key.ToString(),
                        Count = n.Count()
                    })
                    .OrderBy(n => n.Status);
                    foreach (var v in bizGroups)
                    {
                        bizsList.Add(new GingerCoreNET.GeneralLib.StatItem() { Description = v.Status, Count = v.Count });
                        totalBusinessFlowCount += v.Count;
                    }
                    BizFlowsPieChartLayout.DataContext = bizsList;
                    CreateStatistics(bizsList, eObjectType.BusinessFlow);
                    //Activities
                    List<GingerCoreNET.GeneralLib.StatItem> activitiesList = [];
                    var activitiesGroups = mBFESs.SelectMany(b => b.Activities).Where(b => b.GetType() != typeof(ErrorHandler) && b.GetType() != typeof(CleanUpActivity))
                    .GroupBy(n => n.Status)
                    .Select(n => new
                    {
                        Status = n.Key.ToString(),
                        Count = n.Count()
                    })
                    .OrderBy(n => n.Status);
                    foreach (var v in activitiesGroups)
                    {
                        activitiesList.Add(new GingerCoreNET.GeneralLib.StatItem() { Description = v.Status, Count = v.Count });
                        totalActivityCount += v.Count;
                    }
                    ActivitiesPieChartLayout.DataContext = activitiesList;
                    CreateStatistics(activitiesList, eObjectType.Activity);
                    //Actions
                    List<GingerCoreNET.GeneralLib.StatItem> actionsList = [];
                    var actionsGroups = mBFESs.SelectMany(b => b.Activities).Where(b => b.GetType() != typeof(ErrorHandler) && b.GetType() != typeof(CleanUpActivity)).SelectMany(x => x.Acts)
                    .GroupBy(n => n.Status)
                    .Select(n => new
                    {
                        Status = n.Key.ToString(),
                        Count = n.Count()
                    })
                    .OrderBy(n => n.Status);
                    foreach (var v in actionsGroups)
                    {
                        actionsList.Add(new GingerCoreNET.GeneralLib.StatItem() { Description = v.Status, Count = v.Count });
                        totalActionCount += v.Count;
                    }
                    ActionsPieChartLayout.DataContext = actionsList;
                    CreateStatistics(actionsList, eObjectType.Action);
                    xBusinessflowsTotalCount.Content = totalBusinessFlowCount;
                    xActivitiesTotalCount.Content = totalActivityCount;
                    xActionsTotalCount.Content = totalActionCount;
                    allItems = bizsList.Concat(activitiesList.Concat(actionsList)).GroupBy(n => n.Description)
                     .Select(n => n.First())
                     .ToList();
                    CreateStatistics(allItems, eObjectType.Legend);
                    if (mExecutorEngine.IsRunning)
                    {
                        xruntime.Content = mExecutorEngine.RunnerExecutionWatch.runWatch.Elapsed.ToString(@"hh\:mm\:ss");
                    }
                });


            }
            catch (InvalidOperationException e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to Update Stats", e);
            }
        }
        //ToDo : move to piechart section or create usercontrol
        public void CreateStatistics(List<GingerCoreNET.GeneralLib.StatItem> list, eObjectType obj)
        {
            switch (obj)
            {
                case eObjectType.BusinessFlow:
                    xBusinessflowsStatistics.Text = string.Empty;
                    foreach (GingerCoreNET.GeneralLib.StatItem s in list)
                    {
                        System.Windows.Documents.Run statCount = createRun(s);
                        xBusinessflowsStatistics.Inlines.Add(statCount);
                        if (list.IndexOf(s) < list.Count - 1)
                        {
                            System.Windows.Documents.Run statSeperator = new System.Windows.Documents.Run
                            {
                                Text = " | ",
                                Foreground = Brushes.DarkGray,
                                FontWeight = FontWeights.Bold
                            };
                            xBusinessflowsStatistics.Inlines.Add(statSeperator);
                        }
                    }
                    break;
                case eObjectType.Activity:
                    xActivitiesStatistics.Text = string.Empty;
                    foreach (GingerCoreNET.GeneralLib.StatItem s in list)
                    {
                        System.Windows.Documents.Run statCount = createRun(s);
                        xActivitiesStatistics.Inlines.Add(statCount);
                        if (list.IndexOf(s) < list.Count - 1)
                        {
                            System.Windows.Documents.Run statSeperator = new System.Windows.Documents.Run
                            {
                                Text = " | ",
                                Foreground = Brushes.DarkGray,
                                FontWeight = FontWeights.Bold
                            };
                            xActivitiesStatistics.Inlines.Add(statSeperator);
                        }
                    }
                    break;
                case eObjectType.Action:
                    xActionsStatistics.Text = string.Empty;
                    foreach (GingerCoreNET.GeneralLib.StatItem s in list)
                    {
                        System.Windows.Documents.Run statCount = createRun(s);
                        xActionsStatistics.Inlines.Add(statCount);
                        if (list.IndexOf(s) < list.Count - 1)
                        {
                            System.Windows.Documents.Run statSeperator = new System.Windows.Documents.Run
                            {
                                Text = " | ",
                                Foreground = Brushes.DarkGray,
                                FontWeight = FontWeights.Bold
                            };
                            xActionsStatistics.Inlines.Add(statSeperator);
                        }
                    }
                    break;
                case eObjectType.Legend:
                    xLegend.Text = string.Empty;
                    foreach (GingerCoreNET.GeneralLib.StatItem s in list)
                    {
                        System.Windows.Documents.Run runText = new System.Windows.Documents.Run
                        {
                            Text = s.Description.ToString() + " "
                        };
                        Rectangle r = new Rectangle();
                        r.Height = r.Width = r.StrokeThickness = 10;
                        r.Margin = new Thickness(5, 0, 5, 0);
                        r.Stroke = ColorSelector != null ? ColorSelector.SelectBrush(s, 0) : Brushes.Black;
                        xLegend.Inlines.Add(r);
                        xLegend.Inlines.Add(runText);
                    }
                    break;
            }
        }
        public System.Windows.Documents.Run createRun(GingerCoreNET.GeneralLib.StatItem status)
        {
            System.Windows.Documents.Run runText = null;
            runText = new System.Windows.Documents.Run
            {
                Text = status.Count.ToString(),
                Foreground = ColorSelector != null ? ColorSelector.SelectBrush(status, 0) : Brushes.Black
            };
            return runText;
        }


        private void MarkUnMarkInActive(bool status)
        {
            if (mExecutorEngine.BusinessFlows.Count <= 0)
            {
                return;
            }

            foreach (BusinessFlow mBusinessFlow in mExecutorEngine.BusinessFlows)
            {
                mBusinessFlow.Active = status;//decide if to take or not
            }
            UpdateBusinessFlowGrid();
        }

        private void xRunRunnerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (mExecutorEngine.BusinessFlows.Count <= 0)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Please add at least one " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " to '" + mExecutorEngine.GingerRunner.Name + "' to start run.");
                return;
            }
            CheckIfRunsetDirty?.Invoke(null, null);
            mExecutorEngine.RunLevel = eRunLevel.Runner;
            if (WorkSpace.Instance.RunsetExecutor.RunSetConfig != null && WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID == null)
            {
                WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID = Guid.NewGuid();
            }
            RunRunner();
        }
        public async void RunRunner()
        {
            if (mExecutorEngine.IsRunning)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Runner is already running.");
                return;
            }
            WorkSpace.Instance.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder = null;
            mExecutorEngine.ResetRunnerExecutionDetails();
            WorkSpace.Instance.RunsetExecutor.ConfigureRunnerForExecution(mExecutorEngine);
            await mExecutorEngine.RunRunnerAsync();
            GingerCore.General.DoEvents();   //needed?  
        }
        public void UpdateRunnerInfo()
        {
            //name
            xRunnerNameTxtBlock.Text = mExecutorEngine.GingerRunner.Name;

            //App-Agents
            mExecutorEngine.UpdateApplicationAgents();
            xRunnerInfoTextBlock.Text = string.Empty;
            TextBlockHelper TBH = new TextBlockHelper(xRunnerInfoTextBlock);
            bool appAgentsMappingExist = false;
            foreach (ApplicationAgent appAgent in mExecutorEngine.GingerRunner.ApplicationAgents)
            {
                if (WorkSpace.Instance.Solution.ApplicationPlatforms.FirstOrDefault(x => x.AppName == appAgent.AppName && x.Platform == ePlatformType.NA) != null)
                {
                    continue;
                }

                TBH.AddText(LimitstringLength(appAgent.AppName, 10));
                TBH.AddText(" > ");
                TBH.AddText(LimitstringLength(appAgent.AgentName, 10));
                TBH.AddLineBreak();
                appAgentsMappingExist = true;
            }

            if (appAgentsMappingExist && xConfigButton.IsLoaded && ExecutorEngine.BusinessFlows.Count == 1)
            {
                App.MainWindow.AddHelpLayoutToShow("RunsetPage_RunnerConfigHelp", xConfigButton, "Click here to configure all Runner execution settings like: mapped Agents, selected Environment, etc.");
            }
        }

        private string LimitstringLength(string str, int maxLen)
        {
            if (str.Length > maxLen)
            {
                string shortStr = (str[..(maxLen - 3)] + "...");
                return shortStr;
            }
            else
            {
                return str;
            }
        }

        private void xStopRunnerBtn_Click(object sender, RoutedEventArgs e)
        {
            StopRunner();
        }

        public void StopRunner()
        {
            if (!mExecutorEngine.IsRunning)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Runner is not running.");
                return;
            }
            mExecutorEngine.StopRun();
        }
        private void xConfigButton_Click(object sender, RoutedEventArgs e)
        {
            GingerRunnerConfigurationsPage PACW = new GingerRunnerConfigurationsPage(mExecutorEngine, GingerRunnerConfigurationsPage.ePageViewMode.RunsetPage, mContext);
            PACW.ShowAsWindow();

            mExecutorEngine.GingerRunner.PauseDirtyTracking();
            UpdateRunnerInfo();
            mExecutorEngine.GingerRunner.ResumeDirtyTracking();
        }

        private void xContinueRunnerBtn_Click(object sender, RoutedEventArgs e)
        {
            ContinueRunner();
        }

        public async void ContinueRunner()
        {
            if (mExecutorEngine.Status != eRunStatus.Stopped)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Runner was not stopped.");
                return;
            }
            WorkSpace.Instance.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder = null;
            await mExecutorEngine.ContinueRunAsync(eContinueLevel.Runner, eContinueFrom.LastStoppedAction);
        }
        private void ViewReportBtn_Click(object sender, RoutedEventArgs e)
        {
            ExecutionLoggerConfiguration _selectedExecutionLoggerConfiguration = WorkSpace.Instance.Solution.LoggerConfigurations;
            string reportsResultFolder = "";

            if (!_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled)
            {
                Reporter.ToUser(eUserMsgKey.ExecutionsResultsProdIsNotOn);
                return;
            }
            else if (reportsResultFolder == string.Empty)
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
                        Process.Start(new ProcessStartInfo() { FileName = reportsResultFolder, UseShellExecute = true });
                        Process.Start(new ProcessStartInfo() { FileName = reportsResultFolder + "\\" + fileName, UseShellExecute = true });
                    }
                }
            }
        }

        //private void GenerateIndividualReport(object sender, RoutedEventArgs e)
        //{
        //    ReportTemplate.GenerateIndividualReport(mRunner,  WorkSpace.Instance.UserProfile.GetDefaultReport(), (ProjEnvironment)WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment, true);
        //}

        //private void GenerateConsolidatedReport(object sender, RoutedEventArgs e)
        //{
        //    var RI = new ReportInfo(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment, mRunner, true);
        //    var repFileName = ReportTemplate.GenerateReport( WorkSpace.Instance.UserProfile.GetDefaultReport(), RI);
        //    Process.Start(repFileName);
        //}

        private void xRunnerActive_Click(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentRunnerIsNotRuning())
            {
                return;
            }

            ExecutorEngine.GingerRunner.Active = !ExecutorEngine.GingerRunner.Active;
            if (!ExecutorEngine.GingerRunner.Active)
            {
                xRunnerNameTxtBlock.Foreground = Brushes.Gray;
            }
            else
            {
                xRunnerNameTxtBlock.Foreground = FindResource("$BackgroundColor_Black") as Brush;
            }
        }




        private void xremoveRunner_Click(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentRunnerIsNotRuning())
            {
                return;
            }

            OnGingerRunnerEvent(RunnerPageEventArgs.eEventType.RemoveRunner, ExecutorEngine);
        }

        private void xDuplicateRunner_Click(object sender, RoutedEventArgs e)
        {
            OnGingerRunnerEvent(RunnerPageEventArgs.eEventType.DuplicateRunner, ExecutorEngine);
        }

        private void xResetRunSetStatus_Click(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentRunnerIsNotRuning())
            {
                return;
            }

            ResetRunnerPage();
        }

        public void ResetRunnerPage()
        {
            ExecutorEngine.ResetRunnerExecutionDetails();

            UpdateExecutionStats();
            xruntime.Content = "00:00:00";
            ExecutorEngine.RunnerExecutionWatch.runWatch.Reset();


            OnGingerRunnerEvent(RunnerPageEventArgs.eEventType.ResetRunnerStatus, ExecutorEngine);
        }
    }
}
