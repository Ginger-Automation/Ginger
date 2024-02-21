#region License
/*
Copyright © 2014-2023 European Support Limited

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

using AccountReport.Contracts.ResponseModels;
using ACL_Data_Contract;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.CoreNET.BPMN.Conversion;
using Amdocs.Ginger.CoreNET.BPMN.Exceptions;
using Amdocs.Ginger.CoreNET.BPMN.Exportation;
using Amdocs.Ginger.CoreNET.BPMN.Models;
using Amdocs.Ginger.CoreNET.LiteDBFolder;
using Amdocs.Ginger.CoreNET.Logger;
using Amdocs.Ginger.CoreNET.Run.RunListenerLib;
using Amdocs.Ginger.CoreNET.Run.RunListenerLib.CenteralizedExecutionLogger;
using Amdocs.Ginger.CoreNET.Utility;
using Ginger.Reports;
using Ginger.Repository;
using Ginger.Repository.AddItemToRepositoryWizard;
using Ginger.Repository.ItemToRepositoryWizard;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Activities;
using GingerWPF.WizardLib;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.Run
{
    /// <summary>
    /// Interaction logic for RunSetsExecutionsPage.xaml
    /// </summary>
    public partial class RunSetsExecutionsHistoryPage : Page
    {
        private const string BPMNExportPath = @"~\\Documents\BPMN";

        ObservableList<RunSetReport> mExecutionsHistoryList = new ObservableList<RunSetReport>();
        ExecutionLoggerHelper executionLoggerHelper = new ExecutionLoggerHelper();

        private HttpClient? _httpClient;

        public bool AutoLoadExecutionData = false;
        public ObservableList<RunSetReport> ExecutionsHistoryList
        {
            get { return mExecutionsHistoryList; }
        }

        string mRunSetExecsRootFolder = string.Empty;
        public RunSetConfig RunsetConfig { get; set; }

        public enum eExecutionHistoryLevel
        {
            Solution,
            SpecificRunSet
        }

        private eExecutionHistoryLevel mExecutionHistoryLevel;
        public RunSetsExecutionsHistoryPage(eExecutionHistoryLevel executionHistoryLevel, RunSetConfig runsetConfig = null)
        {
            InitializeComponent();

            mExecutionHistoryLevel = executionHistoryLevel;
            RunsetConfig = runsetConfig;

            this.Unloaded += OnUnloaded;

            SetGridView();
            LoadExecutionsHistoryData();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
                _httpClient = null;
            }
        }

        public void ReloadData()
        {
            LoadExecutionsHistoryData();
        }

        private void SetGridView()
        {
            if (mExecutionHistoryLevel == eExecutionHistoryLevel.Solution)
            {
                grdExecutionsHistory.SetGridEnhancedHeader(Amdocs.Ginger.Common.Enums.eImageType.History, GingerDicser.GetTermResValue(eTermResKey.RunSets, "All ", " Executions History"), saveAllHandler: null, addHandler: null);
            }

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView =
            [
                new(){ 
                    Field = nameof(RunSetReport.GUID), 
                    Header = "Execution ID", 
                    WidthWeight = 15 
                },
                new(){ 
                    Field = RunSetReport.Fields.Name, 
                    WidthWeight = 20, 
                    ReadOnly = true },
                new()
                { 
                    Field = RunSetReport.Fields.Description, 
                    WidthWeight = 20, 
                    ReadOnly = true },
                new()
                { 
                    Field = RunSetReport.Fields.StartTimeStamp, 
                    Header = "Execution Start Time", 
                    WidthWeight = 10, 
                    ReadOnly = true 
                },
                new()
                { 
                    Field = RunSetReport.Fields.EndTimeStamp, 
                    Header = "Execution End Time", 
                    WidthWeight = 10, 
                    ReadOnly = true 
                },
                new()
                { 
                    Field = RunSetReport.Fields.ExecutionDurationHHMMSS, 
                    Header = "Execution Duration", 
                    WidthWeight = 10, 
                    ReadOnly = true 
                },
                new()
                { 
                    Field = RunSetReport.Fields.RunSetExecutionStatus, 
                    Header = "Execution Status", 
                    WidthWeight = 10, 
                    ReadOnly = true, 
                    BindingMode = BindingMode.OneWay 
                },
                new()
                { 
                    Field = RunSetReport.Fields.DataRepMethod, 
                    Header = "Type", 
                    Visible = true, 
                    ReadOnly = true,
                    WidthWeight = 5, 
                    BindingMode = BindingMode.OneWay 
                },
                new(){ 
                    Field = "Generate Report", 
                    WidthWeight = 8, 
                    StyleType = GridColView.eGridColStyleType.Template, 
                    CellTemplate = (DataTemplate)this.pageGrid.Resources["ReportButton"] 
                },
                new()
                {
                    Field = "Export BPMN",
                    WidthWeight = 8,
                    StyleType = GridColView.eGridColStyleType.Template,
                    CellTemplate = (DataTemplate)this.pageGrid.Resources["BPMNButtonDataTemplate"]
                },
            ];

            grdExecutionsHistory.SetAllColumnsDefaultView(view);
            grdExecutionsHistory.InitViewItems();

            grdExecutionsHistory.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGrid));
            grdExecutionsHistory.AddToolbarTool("@Open_16x16.png", "Open Execution Results Main Folder", new RoutedEventHandler(GetExecutionResultsFolder));
            grdExecutionsHistory.AddToolbarTool("@Delete_16x16.png", "Delete Selected Execution Results", new RoutedEventHandler(DeleteSelectedExecutionResults));
            grdExecutionsHistory.AddToolbarTool("@Trash_16x16.png", "Delete All Execution Results", new RoutedEventHandler(DeleteAllSelectedExecutionResults));
            WeakEventManager<ucGrid, RoutedEventArgs>.AddHandler(source: grdExecutionsHistory, eventName: nameof(ucGrid.RowDoubleClick), handler: OpenExecutionResultsFolder);

            if (mExecutionHistoryLevel == eExecutionHistoryLevel.SpecificRunSet)
            {
                grdExecutionsHistory.AddCheckBox("Auto Load Execution History", new RoutedEventHandler(AutoLoadExecutionHistory));
            }
        }

        private void AutoLoadExecutionHistory(object sender, RoutedEventArgs e)
        {
            if (((System.Windows.Controls.Primitives.ToggleButton)sender).IsChecked == true)
            {
                AutoLoadExecutionData = true;
            }
            else
            {
                AutoLoadExecutionData = false;
            }
        }

        public string NameInDb<T>()
        {
            var name = typeof(T).Name + "s";
            return name;
        }
        private async void LoadExecutionsHistoryData()
        {
            grdExecutionsHistory.Visibility = Visibility.Collapsed;
            Loading.Visibility = Visibility.Visible;
            mExecutionsHistoryList.Clear();
            await System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    if (WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.LoggerConfigurations != null)
                    {
                        mRunSetExecsRootFolder = executionLoggerHelper.GetLoggerDirectory(WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder);
                        //pull all RunSets JSON files from it
                        string[] runSetsfiles = Directory.GetFiles(mRunSetExecsRootFolder, "RunSet.txt", SearchOption.AllDirectories);

                        foreach (string runSetFile in runSetsfiles)
                        {
                            RunSetReport runSetReport = (RunSetReport)JsonLib.LoadObjFromJSonFile(runSetFile, typeof(RunSetReport));
                            runSetReport.DataRepMethod = ExecutionLoggerConfiguration.DataRepositoryMethod.TextFile;
                            runSetReport.LogFolder = System.IO.Path.GetDirectoryName(runSetFile);
                            if (mExecutionHistoryLevel == eExecutionHistoryLevel.SpecificRunSet)
                            {
                                //filer the run sets by GUID
                                if (RunsetConfig != null && string.IsNullOrEmpty(runSetReport.GUID) == false)
                                {
                                    Guid runSetReportGuid = Guid.Empty;
                                    Guid.TryParse(runSetReport.GUID, out runSetReportGuid);
                                    if (RunsetConfig.Guid.Equals(runSetReportGuid))
                                    {
                                        mExecutionsHistoryList.Add(runSetReport);
                                    }
                                }
                            }
                            else
                            {
                                mExecutionsHistoryList.Add(runSetReport);
                            }
                        }
                        LiteDbConnector dbConnector = new LiteDbConnector(Path.Combine(mRunSetExecsRootFolder, "GingerExecutionResults.db"));
                        var rsLiteColl = dbConnector.GetCollection<LiteDbRunSet>(NameInDb<LiteDbRunSet>());

                        IEnumerable<LiteDbRunSet> runSetDataColl = null;
                        if (RunsetConfig != null)
                        {
                            runSetDataColl = rsLiteColl.Find(x => x.GUID == RunsetConfig.Guid);
                        }
                        else
                        {
                            runSetDataColl = rsLiteColl.FindAll();
                        }

                        foreach (var runSet in runSetDataColl)
                        {
                            RunSetReport runSetReport = new RunSetReport();
                            runSetReport.DataRepMethod = ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB;
                            runSetReport.SetLiteDBData(runSet);
                            mExecutionsHistoryList.Add(runSetReport);
                        }
                        AddRemoteExucationRunsetData();
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error Occurred during LoadExecutionHistory.", ex);
                }
            });

            ObservableList<RunSetReport> executionsHistoryListSortedByDate = new ObservableList<RunSetReport>();
            if (mExecutionsHistoryList != null && mExecutionsHistoryList.Count > 0)
            {
                foreach (RunSetReport runSetReport in mExecutionsHistoryList.OrderByDescending(item => item.StartTimeStamp))
                {
                    runSetReport.StartTimeStamp = runSetReport.StartTimeStamp.ToLocalTime();
                    runSetReport.EndTimeStamp = runSetReport.EndTimeStamp.ToLocalTime();
                    executionsHistoryListSortedByDate.Add(runSetReport);
                }
            }

            grdExecutionsHistory.DataSourceList = executionsHistoryListSortedByDate;
            grdExecutionsHistory.Visibility = Visibility.Visible;
            Loading.Visibility = Visibility.Collapsed;
        }

        private void AddRemoteExucationRunsetData()
        {
            List<RunSetReport> runsetsReport = new List<RunSetReport>();
            if (RunsetConfig != null)
            {
                runsetsReport = new GingerRemoteExecutionUtils().GetRunsetExecutionInfo(WorkSpace.Instance.Solution.Guid, RunsetConfig.Guid);
            }
            else
            {
                runsetsReport = new GingerRemoteExecutionUtils().GetSolutionRunsetsExecutionInfo(WorkSpace.Instance.Solution.Guid);
            }
            foreach (var item in runsetsReport)
            {
                mExecutionsHistoryList.Add(item);
            }
        }

        private void DeleteSelectedExecutionResults(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.ExecutionsResultsToDelete) == eUserMsgSelection.Yes)
            {
                DeleteExecutionReports(grdExecutionsHistory.Grid.SelectedItems);
            }
        }
        private void DeleteAllSelectedExecutionResults(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.AllExecutionsResultsToDelete) == eUserMsgSelection.Yes)
            {
                DeleteExecutionReports(grdExecutionsHistory.Grid.Items);
            }
        }

        private void DeleteExecutionReports(System.Collections.IList runSetReports)
        {
            bool remoteDeletionFlag = false;
            foreach (RunSetReport runSetReport in runSetReports)
            {
                if (runSetReport.DataRepMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
                {
                    LiteDbManager dbManager = new LiteDbManager(executionLoggerHelper.GetLoggerDirectory(WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder));
                    var result = dbManager.GetRunSetLiteData();
                    List<LiteDbRunSet> filterData = LiteDbRunSet.IncludeAllReferences(result).Find(a => a._id.Equals(new LiteDB.ObjectId(runSetReport.GUID))).ToList();

                    LiteDbConnector dbConnector = new LiteDbConnector(Path.Combine(mRunSetExecsRootFolder, "GingerExecutionResults.db"));
                    dbConnector.DeleteDocumentByLiteDbRunSet(filterData[0]);
                }
                else if (runSetReport.DataRepMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.TextFile)
                {
                    string runSetFolder = executionLoggerHelper.GetLoggerDirectory(runSetReport.LogFolder);
                    TextFileRepository.DeleteLocalData(runSetFolder);
                }
                else if (runSetReport.DataRepMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.Remote && !remoteDeletionFlag)
                {
                    Reporter.ToUser(eUserMsgKey.RemoteExecutionResultsCannotBeAccessed);
                    remoteDeletionFlag = true;
                }
            }

            if (grdExecutionsHistory.Grid.SelectedItems.Count > 0)
            {
                LoadExecutionsHistoryData();
            }
        }

        private void RefreshGrid(object sender, RoutedEventArgs e)
        {
            ReloadData();
        }

        private void GetExecutionResultsFolder(object sender, RoutedEventArgs e)
        {
            if (WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.LoggerConfigurations != null)
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo() { FileName = executionLoggerHelper.GetLoggerDirectory(WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder), UseShellExecute = true });
            }
            else
            {
                return;
            }
        }

        private void OpenExecutionResultsFolder()
        {
            if (grdExecutionsHistory.CurrentItem == null)
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
                return;
            }

            string runSetFolder = ((RunSetReport)grdExecutionsHistory.CurrentItem).LogFolder;

            if (string.IsNullOrEmpty(runSetFolder))
            {
                return;
            }

            if (!Directory.Exists(runSetFolder))
            {
                Directory.CreateDirectory(runSetFolder);
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = runSetFolder, UseShellExecute = true });
        }

        private void OpenExecutionResultsFolder(object sender, RoutedEventArgs e)
        {
            OpenExecutionResultsFolder();
        }

        private void OpenExecutionResultsFolder(object sender, EventArgs e)
        {
            OpenExecutionResultsFolder();
        }

        private void ReportBtnClicked(object sender, RoutedEventArgs e)
        {
            HTMLReportsConfiguration currentConf = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.FirstOrDefault(x => (x.IsSelected == true));
            if (grdExecutionsHistory.CurrentItem == null)
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
                return;
            }
            if (((RunSetReport)grdExecutionsHistory.CurrentItem).DataRepMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
            {
                var selectedGuid = ((RunSetReport)grdExecutionsHistory.CurrentItem).GUID;
                WebReportGenerator webReporterRunner = new WebReportGenerator();
                webReporterRunner.RunNewHtmlReport(string.Empty, selectedGuid);
            }
            else if (((RunSetReport)grdExecutionsHistory.CurrentItem).DataRepMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.Remote)
            {
                var executionGuid = ((RunSetReport)grdExecutionsHistory.CurrentItem).GUID;
                new GingerRemoteExecutionUtils().GenerateHTMLReport(executionGuid);
            }
            else
            {


                string runSetFolder = executionLoggerHelper.GetLoggerDirectory(((RunSetReport)grdExecutionsHistory.CurrentItem).LogFolder);

                string reportsResultFolder = Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(runSetFolder), false, null, null, false, currentConf.HTMLReportConfigurationMaximalFolderSize);

                if (reportsResultFolder == string.Empty)
                {
                    Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
                    return;
                }
                else
                {
                    System.Diagnostics.Process.Start(new ProcessStartInfo() { FileName = reportsResultFolder, UseShellExecute = true });
                    System.Diagnostics.Process.Start(new ProcessStartInfo() { FileName = reportsResultFolder + "\\" + "GingerExecutionReport.html", UseShellExecute = true });
                }
            }
        }

        private void BPMNButton_Click(object sender, RoutedEventArgs e)
        {
            Button BPMNButton = (Button)sender;
            RunSetReport runSetReport = (RunSetReport)BPMNButton.Tag;

            System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    Dispatcher.Invoke(() => Reporter.ToStatus(eStatusMsgKey.ExportingToBPMNZIP));

                    IEnumerable<BusinessFlowExecutionSequence> bfSequences;
                    if (runSetReport.DataRepMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
                    {
                        bfSequences = GetBusinessFlowsExecutionSequencesFromLiteDb(runSetReport.GUID);
                    }
                    else
                    {
                        bfSequences = await GetBusinessFlowsExecutionSequencesRemoteAsync(runSetReport.GUID);
                    }
                    List<(BusinessFlow,IEnumerable<GingerCore.Activity>)> activitiesMissingFromSR = [];
                    foreach (BusinessFlowExecutionSequence bfSequence in bfSequences)
                    {
                        activitiesMissingFromSR.Add((
                            bfSequence.BusinessFlow,
                            bfSequence
                                .ExecutedActivities
                                .Where(executedActivity => !executedActivity.ExistInSharedRepository)
                                .Select(executedActivity => executedActivity.Activity)
                                .ToList()));
                    }

                    bool allItemsExistInSR = !activitiesMissingFromSR
                        .SelectMany(pair => pair.Item2)
                        .Any();
                    if (allItemsExistInSR)
                    {
                        int exportedSuccessfullyCount = 0;
                        foreach (BusinessFlowExecutionSequence bfSequence in bfSequences)
                        {
                            try
                            {
                                ExportUseCaseFromBusinessFlowExecutionSequence(bfSequence);
                                exportedSuccessfullyCount++;
                            }
                            catch(Exception ex)
                            {
                                Reporter.ToLog(eLogLevel.ERROR, $"Error occurred while exporting BPMN for business flow {bfSequence.Name}.", ex);
                            }
                        }
                        if (exportedSuccessfullyCount > 0)
                        {
                            Dispatcher.Invoke(() => Reporter.ToUser(eUserMsgKey.MultipleExportToBPMNSuccessful, exportedSuccessfullyCount));
                        }
                        else
                        {
                            Dispatcher.Invoke(() => Reporter.ToUser(eUserMsgKey.GingerEntityToBPMNConversionError, "Unexpected Error, check logs for more details."));
                        }
                        return;
                    }

                    //ask user if they want to add missing activities to Shared Repository
                    eUserMsgSelection userResponse = Reporter.ToUser(eUserMsgKey.AddActivitiesToSharedRepositoryForBPMNConversion);
                    if (userResponse == eUserMsgSelection.Cancel)
                    {
                        return;
                    }

                    if (userResponse == eUserMsgSelection.No)
                    {
                        int exportedSuccessfullyCount = 0;
                        foreach (BusinessFlowExecutionSequence bfSequence in bfSequences)
                        {
                            try
                            {
                                IEnumerable<ExecutedActivity> activitiesExistingInSR = bfSequence
                                        .ExecutedActivities
                                        .Where(execitedActivity => execitedActivity.ExistInSharedRepository)
                                        .ToList();

                                BusinessFlowExecutionSequence filteredBfSequence = new(
                                    bfSequence.BusinessFlow,
                                    activitiesExistingInSR);

                                ExportUseCaseFromBusinessFlowExecutionSequence(filteredBfSequence);
                                exportedSuccessfullyCount++;
                            }
                            catch (Exception ex)
                            {
                                Reporter.ToLog(eLogLevel.ERROR, $"Error occurred while exporting BPMN for business flow {bfSequence.Name}.", ex);
                            }
                        }
                        if (exportedSuccessfullyCount > 0)
                        {
                            Dispatcher.Invoke(() => Reporter.ToUser(eUserMsgKey.MultipleExportToBPMNSuccessful, exportedSuccessfullyCount));
                        }
                        else
                        {
                            Dispatcher.Invoke(() => Reporter.ToUser(eUserMsgKey.GingerEntityToBPMNConversionError, "Unexpected Error, check logs for more details."));
                        }
                        return;
                    }

                    if (userResponse == eUserMsgSelection.Yes)
                    {
                        bool wasAllAdded = TryAddActivitiesToSharedRepository(activitiesMissingFromSR);
                        if (!wasAllAdded)
                        {
                            return;
                        }

                        int exportedSuccessfullyCount = 0;
                        foreach (BusinessFlowExecutionSequence bfSequence in bfSequences)
                        {
                            try
                            {
                                ExportUseCaseFromBusinessFlowExecutionSequence(bfSequence);
                                exportedSuccessfullyCount++;
                            }
                            catch (Exception ex)
                            {
                                Reporter.ToLog(eLogLevel.ERROR, $"Error occurred while exporting BPMN for business flow {bfSequence.Name}.", ex);
                            }
                        }
                        if (exportedSuccessfullyCount > 0)
                        {
                            Dispatcher.Invoke(() => Reporter.ToUser(eUserMsgKey.MultipleExportToBPMNSuccessful, exportedSuccessfullyCount));
                        }
                        else
                        {
                            Dispatcher.Invoke(() => Reporter.ToUser(eUserMsgKey.GingerEntityToBPMNConversionError, "Unexpected Error, check logs for more details."));
                        }
                        return;
                    }
                }
                catch(Exception ex)
                {
                    Dispatcher.Invoke(() => Reporter.ToUser(eUserMsgKey.GingerEntityToBPMNConversionError, "Unexpected Error, check logs for more details."));
                    Reporter.ToLog(eLogLevel.ERROR, "Exception occurred while exporting BPMN from execution history.", ex);
                }
                finally
                {
                    Dispatcher.Invoke(() => Reporter.HideStatusMessage());
                }
            });
        }

        private IEnumerable<BusinessFlowExecutionSequence> GetBusinessFlowsExecutionSequencesFromLiteDb(string executionId)
        {
            LiteDbManager liteDbManager = new(new ExecutionLoggerHelper().GetLoggerDirectory(WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder));
            LiteDbRunSet liteRunSet = liteDbManager.GetLatestExecutionRunsetData(executionId);
            IEnumerable<LiteDbBusinessFlow> liteBFs = liteRunSet
                .RunnersColl
                .SelectMany(liteRunner => liteRunner.AllBusinessFlowsColl);

            List<BusinessFlowExecutionSequence> bfExecSequences = [];

            foreach (LiteDbBusinessFlow liteBF in liteBFs)
            {
                BusinessFlow bf = GetBusinessFlowByName(liteBF.Name);
                IEnumerable<ExecutedActivity> activities = liteBF
                    .AllActivitiesColl
                    .Select(liteActivity => GetBusinessFlowActivityByName(bf, liteActivity.Name)!)
                    .Where(activity => activity != null)
                    .Select(activity => new ExecutedActivity(
                        activity,
                        existInSR: ActivityExistInSharedRepository(activity.ActivityName),
                        existInBF: true));

                bfExecSequences.Add(new(bf, activities));
            }

            return bfExecSequences;
        }

        private async Task<IEnumerable<BusinessFlowExecutionSequence>> GetBusinessFlowsExecutionSequencesRemoteAsync(string executionId)
        {
            List<BusinessFlowExecutionSequence> bfExecSequences = [];

            AccountReportRunSetClient runset = await GetExecutionDataFromAccountReportAsync(executionId);
            foreach (AccountReportRunnerClient runner in runset.RunnersColl)
            {
                foreach (AccountReportBusinessFlowClient businessFlow in runner.BusinessFlowsColl)
                {
                    BusinessFlow bf = GetBusinessFlowByName(businessFlow.Name);
                    List<AccountReportActivityClient> activities = businessFlow.ActivitiesColl; 

                    IEnumerable<ExecutedActivity> executedActivities = activities
                        .Select(activity => GetBusinessFlowActivityByName(bf, activity.Name)!)
                        .Where(activity => activity != null)
                        .Select(activity => new ExecutedActivity(
                            activity,
                            existInSR: ActivityExistInSharedRepository(activity.ActivityName),
                            existInBF: true))
                        .ToList();

                    bfExecSequences.Add(new(bf, executedActivities));
                }
            }

            return bfExecSequences;
        }

        private BusinessFlow GetBusinessFlowByName(string name)
        {
            BusinessFlow? bf = WorkSpace
                .Instance
                .SolutionRepository
                .GetAllRepositoryItems<BusinessFlow>()
                .FirstOrDefault(bf => string.Equals(bf.Name, name));

            if (bf == null)
            {
                throw new Exception($"No {GingerDicser.GetTermResValue(eTermResKey.BusinessFlows)} found by name '{name}'.");
            }

            return bf;
        }

        private GingerCore.Activity? GetBusinessFlowActivityByName(BusinessFlow bf, string activityName)
        {
            return bf
                .Activities
                .FirstOrDefault(activity => string.Equals(activity.ActivityName, activityName));
        }

        private bool ActivityExistInSharedRepository(string name)
        {
            return WorkSpace
                .Instance
                .SolutionRepository
                .GetAllRepositoryItems<GingerCore.Activity>()
                .Any(activity => string.Equals(activity.ActivityName, name));
        }

        private bool ActivityGroupExistInSharedRepository(string name)
        {
            return WorkSpace
                .Instance
                .SolutionRepository
                .GetAllRepositoryItems<ActivitiesGroup>()
                .Any(activityGroup => string.Equals(activityGroup.Name, name));
        }

        private bool TryAddActivitiesToSharedRepository(IEnumerable<(BusinessFlow,IEnumerable<GingerCore.Activity>)> bfActivities)
        {
            bool wasAllAdded = false;
            Dispatcher.Invoke(() =>
            {
                List<UploadItemSelection> uploadItems = new();
                foreach(var bfActivitiesPair in bfActivities)
                {
                    BusinessFlow bf = bfActivitiesPair.Item1;
                    foreach(GingerCore.Activity activity in bfActivitiesPair.Item2)
                    {
                        uploadItems.Add(UploadItemToRepositoryWizard.CreateUploadItem(
                            activity,
                            new Amdocs.Ginger.Common.Context()
                            {
                                BusinessFlow = bf
                            }));
                    }
                }
                UploadItemToRepositoryWizard uploadItemToRepositoryWizard = new(uploadItems);
                WizardWindow.ShowWizard(uploadItemToRepositoryWizard);

                wasAllAdded = bfActivities
                    .SelectMany(pair => pair.Item2)
                    .All(activity => ActivityExistInSharedRepository(activity.ActivityName));
            });
            return wasAllAdded;
        }

        private void ExportUseCaseFromBusinessFlowExecutionSequence(BusinessFlowExecutionSequence bfSequence)
        {
            BusinessFlow bf = GetBusinessFlowByName(bfSequence.Name);
            IEnumerable<ActivitiesGroup> groups = bf
                .ActivitiesGroups
                .Select(group => new ActivitiesGroup()
                {
                    Name = group.Name
                })
                .ToArray();
            IEnumerable<GingerCore.Activity> activities = bf
                .Activities
                .Select(activity =>
                {
                    ActivitiesGroup group = groups.First(g => string.Equals(g.Name, activity.ActivitiesGroupID));
                    group.ActivitiesIdentifiers.Add(new()
                    {
                        ActivityName = activity.ActivityName,
                    });
                    return new GingerCore.Activity()
                    {
                        ActivityName = activity.ActivityName,
                        ActivitiesGroupID = activity.ActivitiesGroupID
                    };
                })
                .ToArray();

            BusinessFlow executedBF = new()
            {
                Name = bfSequence.Name,
                ActivitiesGroups = new(groups),
                Activities = new(activities)
            };

            ExportUseCaseFromBusinessFlow(executedBF);
        }

        private Task<AccountReportRunSetClient> GetExecutionDataFromAccountReportAsync(string executionId)
        {
            AccountReportApiHandler handler = new(WorkSpace.Instance.Solution.LoggerConfigurations.CentralLoggerEndPointUrl);
            return handler.GetAccountHTMLReportAsync(Guid.Parse(executionId));
        }

        private void ExportUseCaseFromBusinessFlow(BusinessFlow businessFlow)
        {
            string fullBPMNExportPath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(BPMNExportPath);
            BusinessFlowToBPMNExporter bpmnExporter = new(
                businessFlow,
                new CollaborationFromActivityGroupCreator.Options()
                {
                    NonDeterministicFlowControlHandlingStrategy = NonDeterministicFlowControlHandlingStrategy.Ignore
                },
                fullBPMNExportPath);
            bpmnExporter.Export();
        }

        private sealed class BusinessFlowExecutionSequence
        {
            public BusinessFlow BusinessFlow { get; }

            public string Name => BusinessFlow.Name;

            public IEnumerable<ExecutedActivity> ExecutedActivities { get; }

            public BusinessFlowExecutionSequence(BusinessFlow businessFlow, IEnumerable<ExecutedActivity> executedActivities)
            {
                BusinessFlow = businessFlow;
                ExecutedActivities = new List<ExecutedActivity>(executedActivities);
            }
        }

        private sealed class ExecutedActivity
        {
            public GingerCore.Activity Activity { get; }

            public bool ExistInSharedRepository { get; }

            public bool ExistInBusinessFlow { get; }

            public ExecutedActivity(GingerCore.Activity activity, bool existInSR, bool existInBF)
            {
                Activity = activity;
                ExistInSharedRepository = existInSR;
                ExistInBusinessFlow = existInBF;
            }
        }
    }
}
