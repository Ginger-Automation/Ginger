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
using Amdocs.Ginger.CoreNET.Utility;
using Amdocs.Ginger.Repository;
using Applitools.Utils;
using Ginger.Reports;
using Ginger.Repository;
using Ginger.Repository.AddItemToRepositoryWizard;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Activities;
using GingerWPF.WizardLib;
using MongoDB.Driver.Linq;
using OpenQA.Selenium.Appium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
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
        private const string AccountReportAPIBaseURL = "http://usstlattstl01:7711";

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

            SetGridView();
            LoadExecutionsHistoryData();
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
                    Field = "Generate BPMN",
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
            if (runSetReport.DataRepMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
                CreateBPMNFromLiteDB(runSetReport.GUID);
            else
                _ = CreateBPMNFromRemoteAsync(runSetReport.GUID);
        }

        private void CreateBPMNFromLiteDB(string executionId)
        {
            LiteDbManager liteDbManager = new(new ExecutionLoggerHelper().GetLoggerDirectory(WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder));
            LiteDbRunSet liteRunSet = liteDbManager.GetLatestExecutionRunsetData(executionId);
            IEnumerable<LiteDbBusinessFlow> liteBFs = liteRunSet
                .RunnersColl
                .SelectMany(liteRunner => liteRunner.AllBusinessFlowsColl);
            
            foreach (LiteDbBusinessFlow liteBF in liteBFs)
            {
                IEnumerable<GingerCore.Activity> activities = liteBF
                    .AllActivitiesColl
                    .Select(liteActivity => new GingerCore.Activity()
                    {
                        ActivityName = liteActivity.Name
                    });
                ActivitiesGroup activitiesGroup = new()
                {
                    Name = $"{liteBF.Name}_ActivityGroup",
                    ActivitiesIdentifiers = new(activities
                        .Select(activity => new ActivityIdentifiers()
                        {
                            ActivityName = activity.ActivityName
                        }))
                };
                BusinessFlow businessFlow = new()
                {
                    Name = liteBF.Name,
                    Activities = new(activities),
                    ActivitiesGroups = [activitiesGroup]
                };
                ExportUseCaseFromBusinessFlow(businessFlow);
            }
        }

        private async System.Threading.Tasks.Task CreateBPMNFromRemoteAsync(string executionId)
        {
            JsonObject response;
            try
            {
                response = await GetExecutionDataFromAccountReport(executionId);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while getting execution data from AccountReport api.", ex);
                return;
            }

            IEnumerable<BusinessFlow> businessFlows;
            try
            {
                businessFlows = ParseBusinessFlowsFromAccountReportApiResponse(response);

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while creating business flow by parsing AccountReport api response.", ex);
                return;
            }

            foreach (BusinessFlow businessFlow in businessFlows)
            {
                ExportUseCaseFromBusinessFlow(businessFlow);
                bool wasMissingItemsHandled = HandleMissingActivitiesAndGroupsToSharedRepository(businessFlow);
                if (!wasMissingItemsHandled)
                {
                    return;
                }
            }
        }

        private async Task<JsonObject> GetExecutionDataFromAccountReport(string executionId)
        {
            if (_httpClient == null)
                _httpClient = new();

            HttpRequestMessage request = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{AccountReportAPIBaseURL}/api/AccountReport/GetAccountHtmlReport/{executionId}")
            };
            request.Headers.Add("accept", "application/json");

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"AccountReport api returned unsuccessful response while getting execution data.\nStatusCode: {response.StatusCode},\nContent: {await response.Content.ReadAsStringAsync()}");
            }

            JsonObject? responseJson = (JsonObject?)JsonNode.Parse(await response.Content.ReadAsStringAsync());

            if (responseJson == null)
            {
                throw new Exception("AccountReport api response parsed to null json.");
            }

            return responseJson;
        }

        private IEnumerable<BusinessFlow> ParseBusinessFlowsFromAccountReportApiResponse(JsonObject response)
        {
            List<BusinessFlow> businessFlows = [];

            IEnumerable<GingerCore.Activity> sharedRepositoryActivities = WorkSpace.Instance
                .SolutionRepository
                .GetAllRepositoryItems<GingerCore.Activity>();
            bool activityExistsInSharedRepository(string activityName)
            {
                return sharedRepositoryActivities.Any(srActivity =>
                    string.Equals(srActivity.ActivityName, activityName));
            }

            JsonArray runnersColl = response["RunnersColl"]!.AsArray();
            foreach (JsonObject runner in runnersColl.Select(item => item!.AsObject()))
            {
                JsonArray businessFlowsColl = runner["BusinessFlowsColl"]!.AsArray();
                foreach (JsonObject businessFlow in businessFlowsColl.Select(item => item!.AsObject()))
                {
                    string bfName = businessFlow["Name"]!.ToString();

                    IEnumerable<ActivitiesGroup> allGroupsInBF = businessFlow["ActivitiesGroupsColl"]!
                        .AsArray()
                        .Select(item =>
                            new ActivitiesGroup()
                            {
                                Name = item!["Name"]!.ToString()
                            })
                        .ToList();

                    IEnumerable<GingerCore.Activity> activitiesExistingInSR = businessFlow["ActivitiesColl"]!
                        .AsArray()
                        .Where(item => activityExistsInSharedRepository(item!["Name"]!.ToString()))
                        .Select(item =>
                        {
                            string activityName = item!["Name"]!.ToString();

                            string groupName = item!["ActivityGroupName"]!.ToString();
                            ActivitiesGroup group = allGroupsInBF.First(ag => string.Equals(ag.Name, groupName));

                            group.ActivitiesIdentifiers.Add(new ActivityIdentifiers()
                            {
                                ActivityName = activityName
                            });

                            return new GingerCore.Activity()
                            {
                                ActivityName = activityName,
                                ActivitiesGroupID = group.Name
                            };
                        })
                        .Where(activity => activity != null)
                        .ToList();

                    businessFlows.Add(new BusinessFlow()
                    {
                        Name = bfName,
                        Activities = new(activitiesExistingInSR),
                        ActivitiesGroups = new(allGroupsInBF.Where(ag => ag.ActivitiesIdentifiers.Count > 0))
                    });
                }
            }

            return businessFlows;
        }

        private bool HandleMissingActivitiesAndGroupsToSharedRepository(BusinessFlow businessFlow)
        {
            bool wasHandled;
            try
            {
                IEnumerable<ActivitiesGroup> activityGroups = GetActivityGroupsMissingFromSharedRepository(businessFlow);
                bool allActivityGroupsAlreadyInSharedRepository = !activityGroups.Any();
                if (allActivityGroupsAlreadyInSharedRepository)
                {
                    wasHandled = true;
                    return wasHandled;
                }

                eUserMsgSelection userResponse = Reporter.ToUser(eUserMsgKey.AddActivityGroupsToSharedRepositoryForBPMNConversion);
                if (userResponse == eUserMsgSelection.Cancel)
                {
                    wasHandled = false;
                    return wasHandled;
                }
                if (userResponse == eUserMsgSelection.No)
                {
                    wasHandled = true;
                    return wasHandled;
                }

                Context context = new()
                {
                    BusinessFlow = businessFlow
                };
                IEnumerable<RepositoryItemBase> activitiesAndGroups = activityGroups
                    .SelectMany(ag => ag.ActivitiesIdentifiers.Select(ai => ai.IdentifiedActivity))
                    .Cast<RepositoryItemBase>()
                    .Concat(activityGroups);
                WizardWindow.ShowWizard(new UploadItemToRepositoryWizard(context, activitiesAndGroups));

                wasHandled = activityGroups.All(ag => ag.IsSharedRepositoryInstance);
                return wasHandled;
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.FailedToAddItemsToSharedRepository, "Unexpected error, check logs for more details");
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while adding missing activities and activity groups to shared repository.", ex);
                wasHandled = false;
                return wasHandled;
            }
        }

        private IEnumerable<ActivitiesGroup> GetActivityGroupsMissingFromSharedRepository(BusinessFlow businessFlow)
        {
            ObservableList<ActivitiesGroup> sharedActivitiesGroups = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
            SharedRepositoryOperations.MarkSharedRepositoryItems((IEnumerable<object>)businessFlow.ActivitiesGroups, (IEnumerable<object>)sharedActivitiesGroups);
            return businessFlow.ActivitiesGroups.Where(ag =>
            {
                bool missingFromSharedRepo = !ag.IsSharedRepositoryInstance;
                bool hasActivities = ag.ActivitiesIdentifiers.Any();
                return missingFromSharedRepo && hasActivities;
            });
        }

        private void ExportUseCaseFromBusinessFlow(BusinessFlow businessFlow)
        {
            try
            {
                Reporter.ToStatus(eStatusMsgKey.ExportingToBPMNZIP);

                string fullBPMNExportPath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(BPMNExportPath);
                BusinessFlowToBPMNExporter bpmnExporter = new(
                    businessFlow,
                    new CollaborationFromActivityGroupCreator.Options()
                    {
                        IgnoreInterActivityFlowControls = true,
                    },
                    fullBPMNExportPath);
                string exportPath = bpmnExporter.Export();
                string solutionRelativeExportPath = WorkSpace.Instance.SolutionRepository.ConvertFullPathToBeRelative(exportPath);

                Reporter.ToUser(eUserMsgKey.ExportToBPMNSuccessful, solutionRelativeExportPath);
            }
            catch (Exception ex)
            {
                if (ex is BPMNException)
                {
                    Reporter.ToUser(eUserMsgKey.GingerEntityToBPMNConversionError, ex.Message);
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.GingerEntityToBPMNConversionError, "Unexpected Error, check logs for more details.");
                }
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while exporting BPMN", ex);
            }
            finally
            {
                Reporter.HideStatusMessage();
            }
        }
    }
}
