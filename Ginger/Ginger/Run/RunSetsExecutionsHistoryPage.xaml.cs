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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.External.Configurations;
using Amdocs.Ginger.Common.Telemetry;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.CoreNET.BPMN.Exportation;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.LiteDBFolder;
using Amdocs.Ginger.CoreNET.Logger;
using Amdocs.Ginger.CoreNET.Reports;
using Amdocs.Ginger.CoreNET.Run.RunListenerLib;
using Amdocs.Ginger.CoreNET.Utility;
using Ginger.Reports;
using Ginger.Repository.AddItemToRepositoryWizard;
using Ginger.Repository.ItemToRepositoryWizard;
using Ginger.UserControls;
using GingerCore;
using GingerCoreNET.GeneralLib;
using GingerWPF.WizardLib;
using GraphQL;
using GraphQLClient.Clients;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static Amdocs.Ginger.CoreNET.BPMN.Exportation.RunSetExecutionHistoryToBPMNExporter;
using static Ginger.Actions.ActionEditPage;

namespace Ginger.Run
{
    /// <summary>
    /// Interaction logic for RunSetsExecutionsPage.xaml
    /// </summary>
    public partial class RunSetsExecutionsHistoryPage : Page
    {
        private const string BPMNExportPath = @"~\\Documents\BPMN";

        private readonly RunsetFromReportLoader _runsetFromReportLoader;

        ObservableList<RunSetReport> mExecutionsHistoryList = [];
        ExecutionLoggerHelper executionLoggerHelper = new();

        private HttpClient? _httpClient;
        private Task<GraphQLResponse<GraphQLRunsetResponse>> response;
        private GraphQLResponse<GraphQLRunsetResponse> data;
        private GraphQlClient graphQlClient = null;
        private ExecutionReportGraphQLClient executionReportGraphQLClient;
        private bool isGraphQlClinetConfigure = false;
        private bool waitForPageCreation = true;
        private RadioButton remoteRadioButton;
        private RadioButton localRadioButton;
        private Button openExecutionFolder;
        private Button deleteExecutionResults;
        private Button deleteAllExecutionResults;
        ExecutionLoggerConfiguration execLoggerConfig;


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

        public enum ePageAction
        {
            firstPage,
            lastPage,
            nextPage,
            previousPage
        }

        private eExecutionHistoryLevel mExecutionHistoryLevel;

        public delegate void LoadRunsetEventHandler(RunSetConfig runset);

        public event LoadRunsetEventHandler? LoadRunset;

        public RunSetsExecutionsHistoryPage(eExecutionHistoryLevel executionHistoryLevel, RunSetConfig runsetConfig = null)
        {
            waitForPageCreation = true;
            InitializeComponent();

            mExecutionHistoryLevel = executionHistoryLevel;
            RunsetConfig = runsetConfig;
            _runsetFromReportLoader = new();
            this.Unloaded += OnUnloaded;
            this.Loaded += OnLoaded;
            SetGridView();
            execLoggerConfig = WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.FirstOrDefault(c => c.IsSelected);
            if (execLoggerConfig != null)
            {
                PropertyChangedEventManager.AddHandler(execLoggerConfig, OnExecutionLoggerConfigPublishLogToCentralDB_Changed, nameof(ExecutionLoggerConfiguration.PublishLogToCentralDB));
            }
            waitForPageCreation = false;
        }

        /// <summary>
        /// Handles the change event for the PublishLogToCentralDB property in ExecutionLoggerConfiguration.
        /// Updates the execution logger configuration and sets the visibility of execution history controls.
        /// </summary>
        private void OnExecutionLoggerConfigPublishLogToCentralDB_Changed(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ExecutionLoggerConfiguration execLoggerConfig1)
            {
                return;
            }
            execLoggerConfig = execLoggerConfig1;
            SetExectionHistoryVisibility(execLoggerConfig);
        }

        /// <summary>
        /// Sets the visibility of execution history controls based on the PublishLogToCentralDB property.
        /// Enables or disables the remote radio button and sets the local radio button accordingly.
        /// </summary>
        private bool SetExectionHistoryVisibility(ExecutionLoggerConfiguration execLoggerConfig)
        {
            if (execLoggerConfig.PublishLogToCentralDB == ExecutionLoggerConfiguration.ePublishToCentralDB.Yes && (GingerPlayUtils.IsGingerPlayGatewayUrlConfigured() || GingerPlayUtils.IsGingerPlayBackwardUrlConfigured()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Assigns the GraphQL endpoint for the execution report client.
        /// Retrieves the endpoint URL, configures the GraphQL client, and handles any connection errors.
        /// </summary>
        private bool AssignGraphQLObjectEndPoint()
        {
            try
            {
                if (execLoggerConfig.PublishLogToCentralDB == ExecutionLoggerConfiguration.ePublishToCentralDB.No)
                {
                    isGraphQlClinetConfigure = false;
                    return false;
                }
                string endPoint = GingerRemoteExecutionUtils.GetReportDataServiceUrl();
                if (string.IsNullOrEmpty(endPoint))
                {
                    isGraphQlClinetConfigure = false;
                    return false;
                }

                string graphQlUrl = $"{endPoint.TrimEnd('/')}/api/graphql";

                // GraphQL POST health check
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(3);

                    var healthCheckQuery = new
                    {
                        query = "{ __typename }"
                    };

                    var content = new StringContent(
                        System.Text.Json.JsonSerializer.Serialize(healthCheckQuery),
                        Encoding.UTF8,
                        "application/json"
                    );

                    var response = httpClient.PostAsync(graphQlUrl, content).Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        Reporter.ToLog(eLogLevel.WARN,
                            $"GraphQL endpoint responded with {response.StatusCode}. Marking as unavailable.");
                        isGraphQlClinetConfigure = false;
                        return false;
                    }
                }

                // healthy, initialize the client
                graphQlClient = new GraphQlClient(graphQlUrl);
                executionReportGraphQLClient = new ExecutionReportGraphQLClient(graphQlClient);
                isGraphQlClinetConfigure = true;
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error while verifying or connecting GraphQL endpoint.", ex);
                isGraphQlClinetConfigure = false;
                return false;
            }
        }


        /// <summary>
        /// Checks the centralized execution logger configuration and sets the appropriate radio button and loads the executions history data.
        /// </summary>
        private void LoadExectionHistory()
        {
            if (isGraphQlClinetConfigure)
            {
                LoadRemoteData();
            }
            else
            {
                LocalRadioButton_Selected(null, null);
            }
        }

        /// <summary>
        /// Loads remote execution history data asynchronously.
        /// Shows loading indicators, fetches the data, and hides the loading indicators.
        /// </summary>
        private async Task LoadRemoteData()
        {
            xButtonPnl.Visibility = Visibility.Visible;
            GraphQlLoadingVisible();
            await LoadExecutionsHistoryDataGraphQl();
            GraphQlLoadingCollapsed();
        }

        /// <summary>
        /// Event handler for the remote radio button. Shows the button panel and loads the executions history data using GraphQL.
        /// </summary>
        private async void RemoteRadioButton_Selected(object sender, RoutedEventArgs e)
        {
            xButtonPnl.Visibility = Visibility.Visible;
            GraphQlLoadingVisible();
            if (SetExectionHistoryVisibility(execLoggerConfig) && AssignGraphQLObjectEndPoint())
            {
                await LoadExecutionsHistoryDataGraphQl();
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error occurred while connecting remote.");
                xButtonPnl.Visibility = Visibility.Collapsed;
                localRadioButton.IsChecked = true;
                return;
            }

            GraphQlLoadingCollapsed();
        }
        /// <summary>
        /// Event handler for the local radio button. Hides the button panel and loads the executions history data using LiteDB.
        /// </summary>
        private void LocalRadioButton_Selected(object sender, RoutedEventArgs e)
        {
            xButtonPnl.Visibility = Visibility.Collapsed;
            localRadioButton.IsChecked = true;
            LoadExecutionsHistoryDataLiteDb();
        }

        ///<summary>
        /// Refreshes the data based on the selected radio button.
        /// </summary>
        private async Task RefreshDataAsync()
        {
            GraphQlLoadingVisible();

            if ((bool)remoteRadioButton.IsChecked)
            {
                await LoadExecutionsHistoryDataGraphQl();
            }
            else if ((bool)localRadioButton.IsChecked)
            {
                LoadExecutionsHistoryDataLiteDb();
            }
            GraphQlLoadingCollapsed();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
                _httpClient = null;
                _runsetFromReportLoader.Dispose();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            execLoggerConfig = WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.FirstOrDefault(c => c.IsSelected);
            ReloadExecutionHistoryData();

        }
        /// <summary>
        /// Reloads the data for the RunSetsExecutionsHistoryPage.
        /// </summary>
        public void ReloadExecutionHistoryData()
        {

            if (AssignGraphQLObjectEndPoint() && SetExectionHistoryVisibility(execLoggerConfig))
            {
                remoteRadioButton.IsChecked = true;
                remoteRadioButton.IsEnabled = true;

            }
            else
            {
                remoteRadioButton.IsEnabled = false;
                localRadioButton.IsChecked = true;
            }
            LoadExectionHistory();
        }

        /// <summary>
        /// Sets up the GridView control for displaying the execution history.
        /// </summary>
        private void SetGridView()
        {
            if (mExecutionHistoryLevel == eExecutionHistoryLevel.Solution)
            {
                xGridExecutionsHistory.SetGridEnhancedHeader(Amdocs.Ginger.Common.Enums.eImageType.History, GingerDicser.GetTermResValue(eTermResKey.RunSets, "All", "Executions History"), saveAllHandler: null, addHandler: null);
            }
            xPageSizeComboBox.Items.Add(25);
            xPageSizeComboBox.Items.Add(50);
            xPageSizeComboBox.Items.Add(100);
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new() {
                    Field = nameof(RunSetReport.GUID),
                    Header = "Execution ID",
                    WidthWeight = 25
                },
                new() {
                    Field = RunSetReport.Fields.Name,
                    WidthWeight = 30,
                    ReadOnly = true
                },
                new()
                {
                    Field = RunSetReport.Fields.RunSetExecutionStatus,
                    Header = "Status",
                    WidthWeight = 8,
                    ReadOnly = true,
                    BindingMode = BindingMode.OneWay,
                    PropertyConverter = (new ColumnPropertyConverter(new ActReturnValueStatusConverter(), TextBlock.ForegroundProperty))
                },
                new()
                {
                    Field = RunSetReport.Fields.SourceApplication,
                    Header = "Requested From",
                    WidthWeight = 15,
                    ReadOnly = true,
                },
                new()
                {
                    Field = RunSetReport.Fields.SourceApplicationUser,
                    Header = "Requested By",
                    WidthWeight = 15,
                    ReadOnly = true,
                },
                new()
                {
                    Field = RunSetReport.Fields.StartTimeStamp,
                    Header = "Start Time",
                    WidthWeight = 15,
                    ReadOnly = true
                },
                new()
                {
                    Field = RunSetReport.Fields.EndTimeStamp,
                    Header = "End Time",
                    WidthWeight = 15,
                    ReadOnly = true
                },
                new()
                {
                    Field = RunSetReport.Fields.ExecutionDurationHHMMSS,
                    Header = "Duration",
                    WidthWeight = 15,
                    ReadOnly = true
                },
                new()
                {
                    Field = "Actions",
                    WidthWeight = 15,
                    StyleType = GridColView.eGridColStyleType.Template,
                    CellTemplate = GetActionsDataTemplate()
                }
            ]
            };

            xGridExecutionsHistory.SetAllColumnsDefaultView(view);
            xGridExecutionsHistory.InitViewItems();

            xGridExecutionsHistory.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGrid));

            xGridExecutionsHistory.AddToolbarTool("@Open_16x16.png", "Open Execution Results Main Folder", new RoutedEventHandler(GetExecutionResultsFolder));
            xGridExecutionsHistory.AddToolbarTool("@Delete_16x16.png", "Delete Selected Execution Results", new RoutedEventHandler(DeleteSelectedExecutionResults));
            xGridExecutionsHistory.AddToolbarTool("@Trash_16x16.png", "Delete All Execution Results", new RoutedEventHandler(DeleteAllSelectedExecutionResults));

            WeakEventManager<ucGrid, RoutedEventArgs>.AddHandler(source: xGridExecutionsHistory, eventName: nameof(ucGrid.RowDoubleClick), handler: OpenExecutionResultsFolder);
            xGridExecutionsHistory.AddLabel("Load Execution History Data:");

            string groupName = "ExecutionLoggerGroup";
            if (isGraphQlClinetConfigure)
            {
                remoteRadioButton = xGridExecutionsHistory.AddRadioButton("Remote ", groupName, new RoutedEventHandler(RemoteRadioButton_Selected), isChecked: true);
                localRadioButton = xGridExecutionsHistory.AddRadioButton("Local", groupName, new RoutedEventHandler(LocalRadioButton_Selected));
            }
            else
            {
                remoteRadioButton = xGridExecutionsHistory.AddRadioButton("Remote ", groupName, new RoutedEventHandler(RemoteRadioButton_Selected), isEnabled: false);
                localRadioButton = xGridExecutionsHistory.AddRadioButton("Local", groupName, new RoutedEventHandler(LocalRadioButton_Selected), isChecked: true);
            }
        }

        private DataTemplate GetActionsDataTemplate()
        {
            if (mExecutionHistoryLevel == eExecutionHistoryLevel.SpecificRunSet)
            {
                return (DataTemplate)pageGrid.Resources["ActionsDataTemplateWithoutLoadRunset"];
            }
            else
            {
                return (DataTemplate)pageGrid.Resources["ActionsDataTemplate"];
            }
        }



        public string NameInDb<T>()
        {
            var name = typeof(T).Name + "s";
            return name;
        }
        /// <summary>
        /// Loads the execution history data from LiteDB.
        /// </summary>
        private async Task LoadExecutionsHistoryDataLiteDb()
        {
            xGridExecutionsHistory.Visibility = Visibility.Collapsed;
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
                            runSetReport.RunSetGuid = Guid.Parse(runSetReport.GUID);
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
                            RunSetReport runSetReport = new RunSetReport
                            {
                                DataRepMethod = ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB
                            };
                            runSetReport.SetLiteDBData(runSet);
                            mExecutionsHistoryList.Add(runSetReport);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error Occurred during LoadExecutionHistory.", ex);
                }
            });

            SetContentInGrid();
        }
        /// <summary>
        /// Loads the execution history data using GraphQL.
        /// </summary>
        private async Task LoadExecutionsHistoryDataGraphQl()
        {
            xGridExecutionsHistory.Visibility = Visibility.Collapsed;
            Loading.Visibility = Visibility.Visible;
            mExecutionsHistoryList.Clear();
            int recordLimit = (int)xPageSizeComboBox.SelectedItem;
            graphQlClient.ResetPagination();
            await LoadRemoteDataFromGraphQL(recordLimit, ePageAction.firstPage);
            SetContentInGrid();


        }
        /// <summary>
        /// Sets the content in the grid by populating the data source list with sorted and filtered execution history.
        /// </summary>
        void SetContentInGrid()
        {
            ObservableList<RunSetReport> executionsHistoryListSortedByDate = [];
            if (mExecutionsHistoryList != null && mExecutionsHistoryList.Count > 0)
            {
                IEnumerable<RunSetReport> sortedAndFilteredExecutionHistoryList = mExecutionsHistoryList
                   .Where(report => report.RunSetExecutionStatus != eRunStatus.Automated)
                   .OrderByDescending(item => item.StartTimeStamp);

                foreach (RunSetReport runSetReport in sortedAndFilteredExecutionHistoryList)
                {
                    runSetReport.StartTimeStamp = runSetReport.StartTimeStamp.ToLocalTime();
                    runSetReport.EndTimeStamp = runSetReport.EndTimeStamp.ToLocalTime();
                    executionsHistoryListSortedByDate.Add(runSetReport);
                }
            }

            xGridExecutionsHistory.DataSourceList = executionsHistoryListSortedByDate;
            xGridExecutionsHistory.Visibility = Visibility.Visible;
            Loading.Visibility = Visibility.Collapsed;
        }


        /// <summary>
        /// Handles pagination button click events.
        /// </summary>
        private async Task LoadRemoteDataFromGraphQL(int recordLimit, ePageAction pageButton, string endCursor = null, string startCursor = null, bool firstPage = false, bool lastPage = false, bool afterOrBefore = true)
        {
            try
            {
                if (mExecutionHistoryLevel == eExecutionHistoryLevel.Solution)
                {
                    response = executionReportGraphQLClient.ExecuteReportQuery(recordLimit, WorkSpace.Instance.Solution.Guid, endCursor: endCursor, startCursor: startCursor, firstPage: firstPage, lastPage: lastPage, afterOrBefore: afterOrBefore);
                }
                else
                {
                    response = executionReportGraphQLClient.ExecuteReportQuery(recordLimit, WorkSpace.Instance.Solution.Guid, RunsetConfig.Guid, endCursor: endCursor, startCursor: startCursor, firstPage: firstPage, lastPage: lastPage, afterOrBefore: afterOrBefore);
                }


                AddRemoteDataToList(await response);
                UpdatePageInfo(pageButton);
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error occurred while connecting remote.", ex);
                LocalRadioButton_Selected(null, null);



            }
        }

        /// <summary>
        /// Updates the pagination information displayed to the user.
        /// </summary>
        private void UpdatePageInfo(ePageAction pageAction)
        {
            int recordCount = (int)xPageSizeComboBox.SelectedItem;
            int totalEntries = graphQlClient.TotalCount;
            int itemsFetched = graphQlClient.ItemsFetchedSoFar;
            int currentCount = graphQlClient.CurrentRecordCount;
            int start, end;

            // Calculate the record range (start and end)
            CalculatePageRange(pageAction, recordCount, totalEntries, itemsFetched, currentCount, out start, out end);

            // Calculate the page number
            int pageNumber = CalculatePageNumber(pageAction, recordCount, totalEntries, itemsFetched, start);
            if (pageAction == ePageAction.lastPage)
            {
                graphQlClient.ItemsFetchedSoFar = totalEntries;
            }

            // Update the UI with the calculated range and page number
            xRangelbl.Content = $"Showing {start} to {end} of {totalEntries} entries";
            xPageNumber.Content = $"Page {pageNumber}";
        }

        /// <summary>
        /// Calculates the range of page numbers based on the given page action, record count, total entries, items fetched, and current count.
        /// </summary>
        /// <param name="pageAction">The page action (e.g., "firstPage", "lastPage", "nextPage", "previousPage").</param>
        /// <param name="recordCount">The number of records per page.</param>
        /// <param name="totalEntries">The total number of entries.</param>
        /// <param name="itemsFetched">The number of items fetched.</param>
        /// <param name="currentCount">The current count of items.</param>
        /// <param name="start">The start page number of the range.</param>
        /// <param name="end">The end page number of the range.</param>
        public static void CalculatePageRange(ePageAction pageAction, int recordCount, int totalEntries, int itemsFetched, int currentCount, out int start, out int end)
        {
            start = itemsFetched - recordCount + 1;
            if (start < 1)
            {
                start = 1;
            }
            end = itemsFetched;

            switch (pageAction)
            {
                case ePageAction.firstPage:
                    start = 1;
                    end = Math.Min(recordCount, totalEntries);
                    break;

                case ePageAction.lastPage:
                    float result = MathF.Floor((float)totalEntries / recordCount);
                    int remainingRecords = totalEntries - (int)result * recordCount;
                    recordCount = remainingRecords == 0 ? recordCount : remainingRecords;

                    start = totalEntries - recordCount + 1;
                    if (start < 1)
                    {
                        start = 1;
                    }
                    end = totalEntries;

                    break;

                case ePageAction.nextPage:
                    if (currentCount < recordCount)
                    {
                        start = totalEntries - currentCount + 1;
                    }
                    end = Math.Min(itemsFetched, totalEntries);
                    break;

                case ePageAction.previousPage:
                    start = itemsFetched - recordCount + 1;
                    end = itemsFetched;
                    if (end < 1)
                    {
                        end = recordCount;
                    }
                    break;
                default:
                    return;

            }
        }

        /// <summary>
        /// Calculates the page number based on the page action and other parameters.
        /// </summary>
        /// <param name="pageAction">The action performed on the page (firstPage, lastPage, nextPage, previousPage).</param>
        /// <param name="recordCount">The number of records per page.</param>
        /// <param name="totalEntries">The total number of entries.</param>
        /// <param name="itemsFetched">The number of items fetched.</param>
        /// <param name="start">The starting index of the current page.</param>
        /// <returns>The calculated page number.</returns>
        public static int CalculatePageNumber(ePageAction pageAction, int recordCount, int totalEntries, int itemsFetched, int start)
        {
            int pageNumber = 1;

            switch (pageAction)
            {
                case ePageAction.firstPage:
                    pageNumber = 1;
                    break;

                case ePageAction.lastPage:
                    float result = MathF.Floor((float)totalEntries / recordCount);
                    int remainingRecords = totalEntries - (int)result * recordCount;
                    pageNumber = (int)result + (remainingRecords == 0 ? 0 : 1);
                    break;

                case ePageAction.nextPage:
                    if (itemsFetched % recordCount == 0)
                    {
                        pageNumber = (itemsFetched / recordCount);
                    }
                    else
                    {
                        pageNumber = (itemsFetched / recordCount) + 1;
                    }
                    break;

                case ePageAction.previousPage:
                    pageNumber = (start - 1) / recordCount + 1;
                    break;
                default:
                    return 0;
            }

            return pageNumber;
        }

        /// <summary>
        /// Updates the enabled/disabled state of pagination buttons based on current pagination state.
        /// </summary>
        private void UpdateButtonStates()
        {
            btnFirst.IsEnabled = graphQlClient.HasPreviousPage;
            btnPrevious.IsEnabled = graphQlClient.HasPreviousPage;
            btnNext.IsEnabled = graphQlClient.HasNextPage;
            btnLast.IsEnabled = graphQlClient.HasNextPage;
        }

        /// <summary>
        /// Adds the data fetched from GraphQL to the list.
        /// </summary>
        private void AddRemoteDataToList(GraphQLResponse<GraphQLRunsetResponse> data)
        {
            mExecutionsHistoryList.Clear();

            foreach (var node in data.Data.Runsets.Nodes)
            {
                var runSetReport = new RunSetReport
                {
                    GUID = node.ExecutionId.ToString(),
                    RunSetGuid = node.EntityId.Value,
                    Name = node.Name,
                    Description = node.Description,
                    SourceApplication = node.SourceApplication,
                    SourceApplicationUser = node.SourceApplicationUser,
                    StartTimeStamp = node.StartTime.Value.ToUniversalTime(),
                    EndTimeStamp = node.EndTime.Value.ToUniversalTime(),
                    Elapsed = node.ElapsedEndTimeStamp,
                    DataRepMethod = ExecutionLoggerConfiguration.DataRepositoryMethod.Remote
                };

                if (node.ElapsedEndTimeStamp != null)
                {
                    runSetReport.ExecutionDurationHHMMSS = GingerCoreNET.GeneralLib.General.TimeConvert((node.ElapsedEndTimeStamp / 1000).ToString());
                }

                if (Enum.TryParse<eRunStatus>(node.Status, out eRunStatus runStatus))
                {
                    runSetReport.RunSetExecutionStatus = runStatus;
                }

                mExecutionsHistoryList.Add(runSetReport);
            }

            SetContentInGrid();
        }


        private void DeleteSelectedExecutionResults(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.ExecutionsResultsToDelete) == eUserMsgSelection.Yes)
            {
                DeleteExecutionReports(xGridExecutionsHistory.Grid.SelectedItems);
            }
        }
        private void DeleteAllSelectedExecutionResults(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.AllExecutionsResultsToDelete) == eUserMsgSelection.Yes)
            {
                DeleteExecutionReports(xGridExecutionsHistory.Grid.Items);
            }
        }

        private void DeleteExecutionReports(System.Collections.IList runSetReports)
        {
            if ((bool)remoteRadioButton.IsChecked)
            {
                Reporter.ToUser(eUserMsgKey.RemoteExecutionResultsCannotBeAccessed);
                return;
            }
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

            }

            if (xGridExecutionsHistory.Grid.SelectedItems.Count > 0)
            {
                RefreshDataAsync();
            }
        }

        private void RefreshGrid(object sender, RoutedEventArgs e)
        {
            RefreshDataAsync();
        }

        private void GetExecutionResultsFolder(object sender, RoutedEventArgs e)
        {


            if ((bool)remoteRadioButton.IsChecked)
            {
                Reporter.ToUser(eUserMsgKey.RemoteExecutionReportFolder);
                return;
            }


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
            if (xGridExecutionsHistory.CurrentItem == null)
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
                return;
            }

            string runSetFolder = ((RunSetReport)xGridExecutionsHistory.CurrentItem).LogFolder;

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
            if (xGridExecutionsHistory.CurrentItem == null)
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
                return;
            }
            if (((RunSetReport)xGridExecutionsHistory.CurrentItem).DataRepMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
            {
                var selectedGuid = ((RunSetReport)xGridExecutionsHistory.CurrentItem).GUID;
                WebReportGenerator webReporterRunner = new WebReportGenerator();
                webReporterRunner.RunNewHtmlReport(string.Empty, selectedGuid);
            }
            else if (((RunSetReport)xGridExecutionsHistory.CurrentItem).DataRepMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.Remote)
            {
                var executionGuid = ((RunSetReport)xGridExecutionsHistory.CurrentItem).GUID;
                GingerRemoteExecutionUtils.GenerateHTMLReport(executionGuid);
            }
            else
            {


                string runSetFolder = executionLoggerHelper.GetLoggerDirectory(((RunSetReport)xGridExecutionsHistory.CurrentItem).LogFolder);

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

        private void LoadRunsetButton_Click(object sender, RoutedEventArgs e)
        {
            Button LoadRunsetButton = (Button)sender;
            RunSetReport runsetReport = (RunSetReport)LoadRunsetButton.Tag;
            _ = Task.Run(async () =>
            {
                try
                {
                    Reporter.ToStatus(eStatusMsgKey.LoadingRunSet, messageArgs: runsetReport.Name);
                    RunSetConfig? runset = await _runsetFromReportLoader.LoadAsync(runsetReport);

                    if (runset == null)
                    {
                        Dispatcher.Invoke(() => Reporter.ToUser(eUserMsgKey.RunsetNotFoundForLoading));
                        return;
                    }

                    Dispatcher.Invoke(() =>
                    {
                        LoadRunsetEventHandler? handler = LoadRunset;
                        handler?.Invoke(runset);
                        if (runset.IsVirtual)
                        {
                            runset.DirtyStatus = Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified;
                        }
                    });
                }
                catch (Exception ex)
                {
                    Reporter.ToUser(eUserMsgKey.RunSetLoadFromReportError, ex.Message);
                }
                finally
                {
                    Reporter.HideStatusMessage();
                }
            });
        }

        private void BPMNButton_Click(object sender, RoutedEventArgs e)
        {
            Button BPMNButton = (Button)sender;
            RunSetReport runSetReport = (RunSetReport)BPMNButton.Tag;
            _ = ExportBPMNFromRunSetReportAsync(runSetReport);
        }

        private async Task ExportBPMNFromRunSetReportAsync(RunSetReport runSetReport)
        {
            try
            {
                Dispatcher.Invoke(() => Reporter.ToStatus(eStatusMsgKey.ExportingToBPMNZIP));

                RunSetExecutionHistoryToBPMNExporter exporter = new();

                IEnumerable<ExecutedBusinessFlow> executedBusinessFlows;
                executedBusinessFlows = await exporter.GetExecutedBusinessFlowsAsync(runSetReport.GUID, runSetReport.DataRepMethod);

                executedBusinessFlows = RemoveNonSharedRepositoryActivitiesFromExecutionData(executedBusinessFlows);
                if (!executedBusinessFlows.Any())
                {
                    return;
                }

                int exportedSuccessfullyCount = 0;
                using (IFeatureTracker featureTracker = Reporter.StartFeatureTracking(FeatureId.ExportRunSetExecutionHistoryBPMN))
                {
                    try
                    {
                        featureTracker.Metadata.Add("BusinessFlowCount", executedBusinessFlows.Count().ToString());

                        int activityGroupCount = executedBusinessFlows
                            .Where(ex => ex != null)
                            .Select(ex => ex.BusinessFlow)
                            .Where(bf => bf != null && bf.Active && bf.ActivitiesGroups != null)
                            .SelectMany(bf => bf.ActivitiesGroups)
                            .Where(ag => ag != null)
                            .Count();
                        featureTracker.Metadata.Add("ActivityGroupCount", activityGroupCount.ToString());

                        int activityCount = executedBusinessFlows
                            .Where(ex => ex != null)
                            .Select(ex => ex.BusinessFlow)
                            .Where(bf => bf != null && bf.Active && bf.Activities != null)
                            .SelectMany(bf => bf.Activities)
                            .Where(activity => activity != null && activity.Active)
                            .Count();
                        featureTracker.Metadata.Add("ActivityCount", activityCount.ToString());

                        int actionCount = executedBusinessFlows
                            .Where(ex => ex != null)
                            .Select(ex => ex.BusinessFlow)
                            .Where(bf => bf != null && bf.Active && bf.Activities != null)
                            .SelectMany(bf => bf.Activities)
                            .Where(activity => activity != null && activity.Active && activity.Acts != null)
                            .SelectMany(activity => activity.Acts)
                            .Where(act => act != null && act.Active)
                            .Count();
                        featureTracker.Metadata.Add("ActionCount", actionCount.ToString());
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, $"error while capturing '{FeatureId.ExportRunSetExecutionHistoryBPMN}' feature metadata", ex);
                    }

                    foreach (ExecutedBusinessFlow executedBusinessFlow in executedBusinessFlows)
                    {
                        try
                        {
                            exporter.Export(executedBusinessFlow, BPMNExportPath);
                            exportedSuccessfullyCount++;
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, $"Error occurred while exporting BPMN for business flow {executedBusinessFlow.Name}.", ex);
                        }
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
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => Reporter.ToUser(eUserMsgKey.GingerEntityToBPMNConversionError, "Unexpected Error, check logs for more details."));
                Reporter.ToLog(eLogLevel.ERROR, "Exception occurred while exporting BPMN from execution history.", ex);
            }
            finally
            {
                Dispatcher.Invoke(() => Reporter.HideStatusMessage());
            }
        }

        private IEnumerable<ExecutedBusinessFlow> RemoveNonSharedRepositoryActivitiesFromExecutionData(IEnumerable<ExecutedBusinessFlow> executedBusinessFlows)
        {
            List<(BusinessFlow, IEnumerable<GingerCore.Activity>)> activitiesMissingFromSR = [];
            foreach (ExecutedBusinessFlow executedBusinessFlow in executedBusinessFlows)
            {
                activitiesMissingFromSR.Add((
                    executedBusinessFlow.BusinessFlow,
                    executedBusinessFlow
                        .ExecutedActivities
                        .Where(executedActivity => !executedActivity.ExistInSharedRepository)
                        .Select(executedActivity => executedActivity.Activity)
                        .ToArray()));
            }

            bool allItemsExistInSR = !activitiesMissingFromSR
                .SelectMany(pair => pair.Item2)
                .Any();

            if (allItemsExistInSR)
            {
                return executedBusinessFlows;
            }

            //ask user if they want to add missing activities to Shared Repository
            eUserMsgSelection userResponse = eUserMsgSelection.Cancel;
            Dispatcher.Invoke(() => userResponse = Reporter.ToUser(eUserMsgKey.AddActivitiesToSharedRepositoryForBPMNConversion));
            if (userResponse == eUserMsgSelection.Yes)
            {
                bool wasAllAdded = TryAddActivitiesToSharedRepository(activitiesMissingFromSR);
                if (!wasAllAdded)
                {
                    Dispatcher.Invoke(() => Reporter.ToUser(eUserMsgKey.AllActivitiesMustBeAddedToSharedRepositoryForBPMNExport));
                    return Array.Empty<ExecutedBusinessFlow>();
                }

                return executedBusinessFlows;
            }
            else if (userResponse == eUserMsgSelection.No)
            {
                List<ExecutedBusinessFlow> filteredExecutedBusinessFlows = [];
                foreach (ExecutedBusinessFlow executedBusinessFlow in executedBusinessFlows)
                {
                    IEnumerable<ExecutedActivity> activitiesExistingInSR = executedBusinessFlow
                                .ExecutedActivities
                                .Where(execitedActivity => execitedActivity.ExistInSharedRepository)
                                .ToList();

                    filteredExecutedBusinessFlows.Add(new ExecutedBusinessFlow(
                        executedBusinessFlow.BusinessFlow,
                        activitiesExistingInSR));
                }
                return filteredExecutedBusinessFlows;
            }
            else
            {
                return Array.Empty<ExecutedBusinessFlow>();

            }
        }

        private bool ActivityExistInSharedRepository(string name)
        {
            return WorkSpace
                .Instance
                .SolutionRepository
                .GetAllRepositoryItems<GingerCore.Activity>()
                .Any(activity => string.Equals(activity.ActivityName, name));
        }

        private bool TryAddActivitiesToSharedRepository(IEnumerable<(BusinessFlow, IEnumerable<GingerCore.Activity>)> bfActivities)
        {
            bool wasAllAdded = false;
            Dispatcher.Invoke(() =>
            {
                List<UploadItemSelection> uploadItems = [];
                foreach (var bfActivitiesPair in bfActivities)
                {
                    BusinessFlow bf = bfActivitiesPair.Item1;
                    foreach (GingerCore.Activity activity in bfActivitiesPair.Item2)
                    {
                        uploadItems.Add(UploadItemToRepositoryWizard.CreateUploadItem(
                            activity,
                            new Context()
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
        private async void btnFirst_Click(object sender, RoutedEventArgs e)
        {
            GraphQlLoadingVisible();
            btnFirst.IsEnabled = false;
            LoadExecutionsHistoryDataGraphQl();
            UpdatePageInfo(ePageAction.firstPage);
            GraphQlLoadingCollapsed();
        }

        private async void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            GraphQlLoadingVisible();
            btnPrevious.IsEnabled = false;

            float result = MathF.Floor(graphQlClient.TotalCount / (int)xPageSizeComboBox.SelectedItem);
            int remainingRecords = graphQlClient.TotalCount - (int)result * (int)xPageSizeComboBox.SelectedItem;
            int recordCount;

            if (graphQlClient.ItemsFetchedSoFar % (int)xPageSizeComboBox.SelectedItem == 0)
            {
                recordCount = (int)xPageSizeComboBox.SelectedItem;
            }
            else
            {
                recordCount = remainingRecords;
            }
            string endCursor = graphQlClient.EndCursor;
            string startCursor = graphQlClient.StartCursor;
            bool firstPage = false;
            bool lastPage = false;
            bool afterOrBefore = false;

            if (graphQlClient.HasPreviousPage)
            {
                graphQlClient.DecreaseFetchedItemsCount(recordCount);
                await LoadRemoteDataFromGraphQL((int)xPageSizeComboBox.SelectedItem, pageButton: ePageAction.previousPage, endCursor: endCursor, startCursor: startCursor, firstPage: firstPage, lastPage: lastPage, afterOrBefore: afterOrBefore);
            }
            GraphQlLoadingCollapsed();
        }

        private async void btnNext_Click(object sender, RoutedEventArgs e)
        {
            GraphQlLoadingVisible();
            btnNext.IsEnabled = false;
            string endCursor = graphQlClient.EndCursor;
            string startCursor = graphQlClient.StartCursor;
            bool firstPage = false;
            bool lastPage = false;
            bool afterOrBefore = true;

            if (graphQlClient.HasNextPage)
            {
                await LoadRemoteDataFromGraphQL((int)xPageSizeComboBox.SelectedItem, pageButton: ePageAction.nextPage, endCursor: endCursor, startCursor: startCursor, firstPage: firstPage, lastPage: lastPage, afterOrBefore: afterOrBefore);
            }
            GraphQlLoadingCollapsed();
        }

        private async void btnLast_Click(object sender, RoutedEventArgs e)
        {
            GraphQlLoadingVisible();
            btnLast.IsEnabled = false;
            float result = MathF.Floor(graphQlClient.TotalCount / (int)xPageSizeComboBox.SelectedItem);
            int remainingRecords = graphQlClient.TotalCount - (int)result * (int)xPageSizeComboBox.SelectedItem;
            int recordCount = remainingRecords == 0 ? (int)xPageSizeComboBox.SelectedItem : remainingRecords;

            string endCursor = null;
            string startCursor = null;
            bool firstPage = false;
            bool lastPage = true;
            bool afterOrBefore = false;

            await LoadRemoteDataFromGraphQL(recordLimit: recordCount, pageButton: ePageAction.lastPage, endCursor: endCursor, startCursor: startCursor, firstPage: firstPage, lastPage: lastPage, afterOrBefore: afterOrBefore);
            GraphQlLoadingCollapsed();
        }
        void GraphQlLoadingVisible()
        {
            xGraphQlLoading.Visibility = Visibility.Visible;
        }
        void GraphQlLoadingCollapsed()
        {
            xGraphQlLoading.Visibility = Visibility.Hidden;
        }

        private void xPageSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (waitForPageCreation)
            {
                return; // Exit the method if still loading
            }
            GraphQlLoadingVisible();
            LoadExectionHistory();
            GraphQlLoadingCollapsed();
        }

    }
}