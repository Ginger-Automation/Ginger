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

        GingerRunner mRunner;
        public GingerRunner Runner
        {
            get
            {
                return mRunner;
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
                    LoadBusinessflowRunnerItem(ViewMode1);
                return mBusinessflowRunnerItems;
            }
        }

        public void ClearBusinessflowRunnerItems()
        {
            if (mBusinessflowRunnerItems != null)
            {
                for (int i = 0; i < mBusinessflowRunnerItems.Count; i++)
                {
                    RunnerItemPage page = (RunnerItemPage)mBusinessflowRunnerItems[i];
                    page.ClearBindings();
                    page= null;//to make sure memory gets free
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
        HTMLReportsConfiguration currentConf =  WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
        ChartType SelectedChartType { get; set; }
        public bool ViewMode1 = false;

        Context mContext = null;

        public RunnerPage(GingerRunner runner, Context context, bool Viewmode=false)
        {
            InitializeComponent();
            mRunner = runner;
            mContext = context;
            ViewMode1 = Viewmode;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xBusinessflowsTotalCount, Label.ContentProperty, mRunner, nameof(GingerRunner.TotalBusinessflow));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xStatus, StatusItem.StatusProperty, mRunner, nameof(GingerRunner.Status), BindingMode.OneWay);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xStatusLabel, ImageMakerControl.ImageTypeProperty, mRunner, nameof(GingerRunner.Status),  bindingConvertor: new StatusIconConverter(), BindingMode.OneWay);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xRunnerActive, ucButton.ButtonImageTypeProperty, mRunner, nameof(GingerRunner.Active),  bindingConvertor: new ActiveIconConverter(), BindingMode.TwoWay);
            UpdateRunnerInfo();
            if(Viewmode)
            {
                pageGrid.IsEnabled = false;
            }

            mDispatcherTimer = new DispatcherTimer();
            mDispatcherTimer.Interval = new TimeSpan(0, 0, 1); // one second
            mDispatcherTimer.Tick += dispatcherTimerElapsedTick;
            mDispatcherTimer.Start();
            UpdateExecutionStats();

            mRunnerPageListener = new RunnerPageListener();
            mRunnerPageListener.UpdateStat = HandleUpdateStat;
            runner.RunListeners.Add(mRunnerPageListener);

            if (WorkSpace.Instance.RunningInExecutionMode)
            {
                xExecutionOperationsPnl.Visibility = Visibility.Collapsed;
                xOperationsPnl.Visibility = Visibility.Collapsed;
            }
        }

        private void HandleUpdateStat(object sender, EventArgs e)
        {            
            mGiveUserFeedback = true;
        }

        private RunnerItemPage CreateBusinessFlowRunnerItem(BusinessFlow bf, bool ViewMode=false)
        {
            RunnerItemPage ri = new RunnerItemPage(bf, ViewMode1);           
            ri.ItemName = bf.Name;
            ri.ItemTitleTooltip = string.Format(@"{0}\{1}", bf.ContainingFolder, bf.Name);
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
                ri.Click += BusinessflowConfig_Click;
                ri.ClickAutomate += Businessflow_ClickAutomate;
                ri.ClickActive += Businessflow_ClickActive;
                ri.xGenerateReport.Visibility = Visibility.Visible;
                ri.ClickGenerateReport += Businessflow_ClickGenerateReport;
                ri.xViewRunnerItem.Visibility = Visibility.Visible;
                ri.DuplicateClick += Businessflow_DuplicateClick;
                ri.RemoveClick += Businessflow_RemoveClick;
                ri.ResetBusinessFlowStatus += BusinessFlow_ResetStatus;
                ri.xRunnerItemMenu.Visibility = Visibility.Visible;
                ri.xremoveBusinessflow.Visibility = Visibility.Visible;
                ri.pageGrid.RowDefinitions[1].Height = new GridLength(30);
                ri.xRunnerItemButtons.Visibility = Visibility.Visible;
                ri.xDetailView.ButtonImageType = eImageType.Collapse;
                ri.xDetailView.ToolTip = "Expand / Collapse " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
            }
            ri.Context = new Context() { BusinessFlow = bf, Runner = mRunner, Environment = GetEnvForContext() };
            return ri;
        }

        private void BusinessFlow_ResetStatus(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentRunnerIsNotRuning()) return;

            BusinessFlow bff = (BusinessFlow)((RunnerItemPage)sender).ItemObject;
            bff.Reset();
        }

        private void Businessflow_RemoveClick(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentRunnerIsNotRuning()) return;
           
                if (Reporter.ToUser(eUserMsgKey.DeleteBusinessflow) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                {
                    BusinessFlow bff = (BusinessFlow)((RunnerItemPage)sender).ItemObject;
                    Runner.BusinessFlows.Remove(bff);
                }           
        }

        private void Businessflow_DuplicateClick(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentRunnerIsNotRuning()) return;     
            
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
                mRunner.BusinessFlows.Insert(indexToInsertTo, bf);
            }
            else
            {
                mRunner.BusinessFlows.Add(bf);
            }
        }

        public void LoadBusinessflowRunnerItem(bool ViewMode=false)
        {
            mBusinessflowRunnerItems = new ObservableList<RunnerItemPage>();
            foreach (BusinessFlow bff in mRunner.BusinessFlows)
            {
                RunnerItemPage bfItem = CreateBusinessFlowRunnerItem(bff, ViewMode);                                             
                mBusinessflowRunnerItems.Add(bfItem);
            }
        }

        private ProjEnvironment GetEnvForContext()
        {
            if (mRunner.UseSpecificEnvironment)
            {
                return mRunner.ProjEnvironment;
            }
            else
            {
                return mContext.Environment;
            }
        }

        private void Businessflow_ClickGenerateReport(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentRunnerIsNotRuning()) return;
            BusinessFlow bf = (BusinessFlow)((RunnerItemPage)sender).ItemObject;
            if (mRunner.ExecutionLoggerManager.Configuration.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
            {
                GenerateBFReport(bf);
                return;
            }

            ExecutionLoggerConfiguration _selectedExecutionLoggerConfiguration =  WorkSpace.Instance.Solution.LoggerConfigurations;
            HTMLReportsConfiguration currentConf =  WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder!=null)
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
                            Process.Start(reportsResultFolder);
                            Process.Start(reportsResultFolder + "\\" + fileName);
                        }
                    }
                }
            }
            else
            {
                Context context = new Context();
                context.BusinessFlow = bf;
                context.Runner = mRunner;
                context.Environment = mRunner.ProjEnvironment;
                mRunner.ExecutionLoggerManager.GenerateBusinessFlowOfflineReport(context, currentConf.HTMLReportsFolder + bf.Name, WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name);
            }
        }

        private void GenerateBFReport(BusinessFlow bf)
        {
            try
            {
                LiteDbManager dbManager = new LiteDbManager(WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder);
                var result = dbManager.GetRunSetLiteData();
                List<LiteDbRunSet> filterData = dbManager.FilterCollection(result, Query.All());

                LiteDbRunSet runSetLast = filterData.Last();
                //runSetLast._id = new ObjectId();

                LiteDbRunner runnerFilter = runSetLast.RunnersColl.Find(r => r.GUID.ToString() == mRunner.Guid.ToString());
                //runnerFilter._id = new ObjectId();
                //runSetLast.RunnersColl = new List<LiteDbRunner>() { runnerFilter };

                LiteDbBusinessFlow bfFilter = runnerFilter.BusinessFlowsColl.Find(b => b.GUID.ToString() == bf.Guid.ToString() && b.StartTimeStamp.ToString() == bf.StartTimeStamp.ToLocalTime().ToString());
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
                webReporterRunner.RunNewHtmlReport(runSetLast._id.ToString(), new WebReportFilter() { Guid = bfFilter.GUID.ToString() });

                //var newRSData = dbManager.GetRunSetLiteData();
                //newRSData.Delete(runSetLast._id);
                //var newRunnerData = dbManager.GetRunnerLiteData();
                //newRunnerData.Delete(runnerFilter._id);
            }
            catch(Exception ex)
            {

            }
        }

        private void Businessflow_ClickActive(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentRunnerIsNotRuning()) return;

            BusinessFlow bf = (BusinessFlow)((RunnerItemPage)sender).ItemObject;
            bf.Active = !bf.Active;
            if(!bf.Active)
            {
                ((RunnerItemPage)sender).xItemName.Foreground = Brushes.Gray;
            }
            else
            {
                ((RunnerItemPage)sender).xItemName.Foreground = FindResource("$BackgroundColor_DarkBlue") as Brush;
            }
        }
        private void Businessflow_ClickAutomate(object sender, RoutedEventArgs e)
        {
            BusinessFlow bf = (BusinessFlow)((RunnerItemPage)sender).ItemObject;
            BusinessFlow actualBf = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>().Where(x => x.Guid == bf.Guid).FirstOrDefault();
            if (actualBf != null)
                App.OnAutomateBusinessFlowEvent(BusinessFlowWindows.AutomateEventArgs.eEventType.Automate, actualBf);
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

            if (mRunner.IsRunning)
            {
                xruntime.Content = mRunner.RunnerExecutionWatch.runWatch.Elapsed.ToString(@"hh\:mm\:ss");                
            }
        }

        private bool CheckCurrentRunnerIsNotRuning()
        {
            if (Runner != null)
            {
                if (Runner.Status == eRunStatus.Running)
                {
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Please wait for Runner to complete run.");
                    return true;
                }
            }

            return false;
        }

        private void BusinessflowConfig_Click(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentRunnerIsNotRuning()) return;

            BusinessFlow bf = (BusinessFlow)((RunnerItemPage)sender).ItemObject;
            ObservableList<BusinessFlow> prevBFs = new ObservableList<BusinessFlow>();
            for (int i = 0; i < mBusinessflowRunnerItems.IndexOf((RunnerItemPage)sender); i++)
                prevBFs.Add((BusinessFlow)((RunnerItemPage)mBusinessflowRunnerItems[i]).ItemObject);
            BusinessFlowRunConfigurationsPage varsPage = new BusinessFlowRunConfigurationsPage(mRunner, bf, prevBFs);
            varsPage.ShowAsWindow();
        }
        //Contains event fired for Change of BusinessFlow/Activities/Actions changed
        private void UpdateBusinessFlowGrid()
        {
        }
        public void UpdateExecutionStats()
        {
            //TODO: break to smaller funcs            
            List<GingerCoreNET.GeneralLib.StatItem> allItems = new List<GingerCoreNET.GeneralLib.StatItem>();
            int totalActivityCount = 0;
            int totalActionCount = 0;

            try//Added try catch, to catch the exception caused when acts object is modified by other thread
            {
                this.Dispatcher.Invoke(() =>
                {
                    mBFESs = mRunner.BusinessFlows;
                    //Business Flows
                    List<GingerCoreNET.GeneralLib.StatItem> bizsList = new List<GingerCoreNET.GeneralLib.StatItem>();
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
                    }
                    BizFlowsPieChartLayout.DataContext = bizsList;
                    CreateStatistics(bizsList, eObjectType.BusinessFlow);                  
                    //Activities
                    List<GingerCoreNET.GeneralLib.StatItem> activitiesList = new List<GingerCoreNET.GeneralLib.StatItem>();
                    var activitiesGroups = mBFESs.SelectMany(b => b.Activities).Where(b=> b.GetType() != typeof(ErrorHandler))
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
                    List<GingerCoreNET.GeneralLib.StatItem> actionsList = new List<GingerCoreNET.GeneralLib.StatItem>();
                    var actionsGroups = mBFESs.SelectMany(b => b.Activities).Where(b => b.GetType() != typeof(ErrorHandler)).SelectMany(x => x.Acts)
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
                    xActivitiesTotalCount.Content = totalActivityCount;                  
                    xActionsTotalCount.Content = totalActionCount;
                    allItems = bizsList.Concat(activitiesList.Concat(actionsList)).GroupBy(n => n.Description)
                     .Select(n => n.First())
                     .ToList();
                    CreateStatistics(allItems, eObjectType.Legend);
                    if (mRunner.IsRunning)
                    {
                        xruntime.Content = mRunner.RunnerExecutionWatch.runWatch.Elapsed.ToString(@"hh\:mm\:ss");
                    }
                });

                
            }
            catch (InvalidOperationException e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to Update Stats", e);
            }
        }
        //ToDo : move to piechart section or create usercontrol
        public void CreateStatistics(List<GingerCoreNET.GeneralLib.StatItem> list,eObjectType obj)
        {
            switch (obj)
            {
                case eObjectType.BusinessFlow:
                    xBusinessflowsStatistics.Text = string.Empty;
                    foreach (GingerCoreNET.GeneralLib.StatItem s in list)
                    {
                        System.Windows.Documents.Run statCount = createRun(s);
                        xBusinessflowsStatistics.Inlines.Add(statCount);
                        if (list.IndexOf(s) < list.Count-1)
                        {
                            System.Windows.Documents.Run statSeperator = new System.Windows.Documents.Run();
                            statSeperator.Text = " | ";
                            statSeperator.Foreground = Brushes.DarkGray;
                            statSeperator.FontWeight = FontWeights.Bold;
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
                            System.Windows.Documents.Run statSeperator = new System.Windows.Documents.Run();
                            statSeperator.Text = " | ";
                            statSeperator.Foreground = Brushes.DarkGray;
                            statSeperator.FontWeight = FontWeights.Bold;
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
                            System.Windows.Documents.Run statSeperator = new System.Windows.Documents.Run();
                            statSeperator.Text = " | ";
                            statSeperator.Foreground = Brushes.DarkGray;
                            statSeperator.FontWeight = FontWeights.Bold;
                            xActionsStatistics.Inlines.Add(statSeperator);
                        }
                    }
                    break;
                case eObjectType.Legend:
                    xLegend.Text = string.Empty;
                    foreach (GingerCoreNET.GeneralLib.StatItem s in list)
                    {
                        System.Windows.Documents.Run runText = new System.Windows.Documents.Run();
                        runText.Text = s.Description.ToString() + " ";
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
            runText = new System.Windows.Documents.Run();
            runText.Text = status.Count.ToString();
            runText.Foreground = ColorSelector != null ? ColorSelector.SelectBrush(status, 0) : Brushes.Black;
            return runText;
        }
               

        private void MarkUnMarkInActive(bool status)
        {
            if (mRunner.BusinessFlows.Count <= 0) return;

            foreach (BusinessFlow mBusinessFlow in mRunner.BusinessFlows)
            {
                mBusinessFlow.Active = status;//decide if to take or not
            }
            UpdateBusinessFlowGrid();
        }

        private void xRunRunnerBtn_Click(object sender, RoutedEventArgs e) 
        {
            if (mRunner.BusinessFlows.Count <= 0)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Please add at least one " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " to '" + mRunner.Name + "' to start run.");
                return;
            }
            mRunner.RunLevel = eRunLevel.Runner;
            RunRunner();
        }
        public async void RunRunner()
        {
            if (mRunner.IsRunning)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Runner is already running.");
                return;
            }
            WorkSpace.Instance.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder = null;
            mRunner.ResetRunnerExecutionDetails();
            WorkSpace.Instance.RunsetExecutor.ConfigureRunnerForExecution(mRunner);
            await mRunner.RunRunnerAsync();
            GingerCore.General.DoEvents();   //needed?  
        }
        public void UpdateRunnerInfo()
        {
            //name
            xRunnerNameTxtBlock.Text = mRunner.Name;

            //App-Agents
            mRunner.UpdateApplicationAgents();
            xRunnerInfoTextBlock.Text = string.Empty;
            TextBlockHelper TBH = new TextBlockHelper(xRunnerInfoTextBlock);
            foreach (ApplicationAgent appAgent in mRunner.ApplicationAgents)
            {
                if ( WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => x.AppName == appAgent.AppName && x.Platform == ePlatformType.NA).FirstOrDefault() != null)
                    continue;
                TBH.AddText(LimitstringLength(appAgent.AppName, 10));
                TBH.AddText(" > ");
                TBH.AddText(LimitstringLength(appAgent.AgentName, 10));
                TBH.AddLineBreak();
            }
            //set info icons
        }

        private string LimitstringLength(string str, int maxLen)
        {
            if (str.Length > maxLen)
            {
                string shortStr= (str.Substring(0, maxLen - 3) + "...");
                return shortStr;
            }
            else
                return str;
        }

        private void xStopRunnerBtn_Click(object sender, RoutedEventArgs e)
        {
            StopRunner();
        }

        public void StopRunner()
        {
            if (!mRunner.IsRunning)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Runner is not running.");
                return;
            }
            mRunner.StopRun();
        }
        private void xConfigButton_Click(object sender, RoutedEventArgs e)
        {
            GingerRunnerConfigurationsPage PACW = new GingerRunnerConfigurationsPage(mRunner, GingerRunnerConfigurationsPage.ePageViewMode.RunsetPage, mContext);
            PACW.ShowAsWindow();

            UpdateRunnerInfo();
        }

        private void xContinueRunnerBtn_Click(object sender, RoutedEventArgs e)
        {
            ContinueRunner();
        }

        public async void ContinueRunner()
        {
            if (mRunner.Status != eRunStatus.Stopped)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Runner was not stopped.");
                return;
            }
            WorkSpace.Instance.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder = null;
            await mRunner.ContinueRunAsync(eContinueLevel.Runner,eContinueFrom.LastStoppedAction);
        }
        private void ViewReportBtn_Click(object sender, RoutedEventArgs e)
        {
            ExecutionLoggerConfiguration _selectedExecutionLoggerConfiguration =  WorkSpace.Instance.Solution.LoggerConfigurations;
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
                        Process.Start(reportsResultFolder);
                        Process.Start(reportsResultFolder + "\\" + fileName);
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
            if (CheckCurrentRunnerIsNotRuning()) return;
            Runner.Active = !Runner.Active;
            if (!Runner.Active)
            {
                xRunnerNameTxtBlock.Foreground = Brushes.Gray;
            }
            else
            {
                xRunnerNameTxtBlock.Foreground = FindResource("$BackgroundColor_DarkBlue") as Brush;
            }
        }

       


        private void xremoveRunner_Click(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentRunnerIsNotRuning()) return;            
            OnGingerRunnerEvent(RunnerPageEventArgs.eEventType.RemoveRunner, Runner);
        }

        private void xDuplicateRunner_Click(object sender, RoutedEventArgs e)
        {            
            OnGingerRunnerEvent(RunnerPageEventArgs.eEventType.DuplicateRunner, Runner);
        }

        private void xResetRunSetStatus_Click(object sender, RoutedEventArgs e)
        {
            if (CheckCurrentRunnerIsNotRuning()) return;
            
            Runner.ResetRunnerExecutionDetails();

            UpdateExecutionStats();
            xruntime.Content = "00:00:00";
            Runner.RunnerExecutionWatch.runWatch.Reset();

            
            OnGingerRunnerEvent(RunnerPageEventArgs.eEventType.ResetRunnerStatus, Runner);
        }
    }


    
}