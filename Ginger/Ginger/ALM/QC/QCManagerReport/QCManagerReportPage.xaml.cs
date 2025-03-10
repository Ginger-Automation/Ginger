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
using Ginger.ALM.QC.TreeViewItems;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Activities;
using GingerCore.ALM.QC;
using GingerCoreNET.GeneralLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Ginger.ALM.QC
{
    enum eViewType { Coverage, Execution }

    enum eExecutionPeriod
    {
        [EnumValueDescription("Any")]
        Any = -1,
        [EnumValueDescription("Last 30 Days")]
        Last30Days = 30,
        [EnumValueDescription("Last 10 Days")]
        Last10Days = 10,
        [EnumValueDescription("Last 5 Days")]
        Last5Days = 5,
        [EnumValueDescription("Last Day")]
        LastDay = 1,
    }

    public partial class QCManagerReportPage : Page
    {
        GenericWindow _pageGenericWin = null;

        eViewType mViewType = eViewType.Coverage;
        ObservableList<QCTestSetTreeItem> mSelectQcTestSets;

        ObservableList<ALMTSTest> mQcTestCasesList = [];
        ObservableList<QCManagerReportTestCaseDetails> mTestCaseDetailsList = [];

        eExecutionPeriod mExecutionPeriodSelectedFilter;
        string mExecutionTesterSelectedFilter;

        double mAutomatedPrecentage = 0;
        int mExecutedNumber = 0;
        int mRunsNumber = 0;
        double mPassedRunsPrecentage = 0;

        public QCManagerReportPage()
        {
            InitializeComponent();

            SetGridView();

            GingerCore.General.FillComboFromEnumObj(ViewComboBox, mViewType);
            ViewComboBox.SelectedValue = eViewType.Coverage;

            GingerCore.General.FillComboFromEnumObj(ExecutionPeriodFilterComboBox, mExecutionPeriodSelectedFilter);
            ExecutionPeriodFilterComboBox.SelectedValue = eExecutionPeriod.Any;

            ExecutionTesterFilterComboBox.Items.Add("Any");
            ExecutionTesterFilterComboBox.SelectedValue = "Any";

            mExecutionPeriodSelectedFilter = eExecutionPeriod.Any;
            mExecutionTesterSelectedFilter = "Any";

            ExecutionPeriodFilterComboBox.SelectionChanged += ExecutionPeriodFilterComboBox_SelectionChanged;
            ExecutionTesterFilterComboBox.SelectionChanged += ExecutionTesterFilterComboBox_SelectionChanged;
        }

        private void ViewComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewComboBox.SelectedValue != null)
            {
                if (ViewComboBox.SelectedValue.ToString() == eViewType.Coverage.ToString())
                {
                    mViewType = eViewType.Coverage;
                    DetailsGrid.Title = "Ginger Automation Coverage";
                    DetailsGrid.ChangeGridView("Coverage");

                    ExecutionFilters.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    mViewType = eViewType.Execution;
                    DetailsGrid.Title = "Ginger Executions History";
                    DetailsGrid.ChangeGridView("Execution");

                    ExecutionFilters.Visibility = System.Windows.Visibility.Visible;
                }
            }
            SetPieData();
        }

        private void SetGridView()
        {
            DetailsGrid.Title = "Ginger Automation Coverage";

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = QCManagerReportTestCaseDetails.Fields.TestSetName, Header = "Test Set", WidthWeight = 25, BindingMode = BindingMode.OneWay },
                new GridColView() { Field = QCManagerReportTestCaseDetails.Fields.TestCaseName, Header = "Test Case", WidthWeight = 25, BindingMode = BindingMode.OneWay },
                new GridColView() { Field = QCManagerReportTestCaseDetails.Fields.ActivitiesGroupName, Header = "Matching " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), WidthWeight = 25, BindingMode = BindingMode.OneWay },
                new GridColView() { Field = QCManagerReportTestCaseDetails.Fields.ActivitiesGroupAutomationPrecentage, Header = "Automation Coverage", HorizontalAlignment = System.Windows.HorizontalAlignment.Center, WidthWeight = 20, BindingMode = BindingMode.OneWay },
                new GridColView() { Field = QCManagerReportTestCaseDetails.Fields.NumberOfExecutions, Header = "Executions Count.", HorizontalAlignment = System.Windows.HorizontalAlignment.Center, WidthWeight = 20, BindingMode = BindingMode.OneWay },
                new GridColView() { Field = QCManagerReportTestCaseDetails.Fields.PassRate, Header = "Pass Rate", HorizontalAlignment = System.Windows.HorizontalAlignment.Center, WidthWeight = 20, BindingMode = BindingMode.OneWay },
                new GridColView() { Field = QCManagerReportTestCaseDetails.Fields.LastExecutionTime, Header = "Last Execution Time", HorizontalAlignment = System.Windows.HorizontalAlignment.Center, WidthWeight = 20, BindingMode = BindingMode.OneWay },
                new GridColView() { Field = QCManagerReportTestCaseDetails.Fields.LastExecutionStatus, Header = "Last Execution Status", HorizontalAlignment = System.Windows.HorizontalAlignment.Center, WidthWeight = 20, BindingMode = BindingMode.OneWay },
            ]
            };
            DetailsGrid.SetAllColumnsDefaultView(view);

            //# Custom Views
            GridViewDef coverageView = new GridViewDef("Coverage")
            {
                GridColsView =
            [
                new GridColView() { Field = QCManagerReportTestCaseDetails.Fields.NumberOfExecutions, Visible = false },
                new GridColView() { Field = QCManagerReportTestCaseDetails.Fields.PassRate, Visible = false },
                new GridColView() { Field = QCManagerReportTestCaseDetails.Fields.LastExecutionTime, Visible = false },
                new GridColView() { Field = QCManagerReportTestCaseDetails.Fields.LastExecutionStatus, Visible = false },
            ]
            };
            DetailsGrid.AddCustomView(coverageView);

            GridViewDef executionView = new GridViewDef("Execution")
            {
                GridColsView =
            [
                new GridColView() { Field = QCManagerReportTestCaseDetails.Fields.ActivitiesGroupName, Visible = false },
                new GridColView() { Field = QCManagerReportTestCaseDetails.Fields.ActivitiesGroupAutomationPrecentage, Visible = false },
            ]
            };
            DetailsGrid.AddCustomView(executionView);

            DetailsGrid.InitViewItems();
            DetailsGrid.ShowViewCombo = System.Windows.Visibility.Collapsed;
            DetailsGrid.DataSourceList = mTestCaseDetailsList;
        }

        private void QCTestCaseBrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            mSelectQcTestSets = (ObservableList<QCTestSetTreeItem>)ALMIntegration.Instance.SelectALMTestSets();

            if (mSelectQcTestSets != null)
            {
                QCTestSetsPathTextBox.Text = string.Empty;
                if (mSelectQcTestSets.Count == 1)
                {
                    QCTestSetsPathTextBox.Text = mSelectQcTestSets[0].Path;
                }
                else if (mSelectQcTestSets.Count > 1)
                {
                    //get the main folder name
                    if (mSelectQcTestSets[0].Path.Contains('\\'))
                    {
                        string mainFolderPath = mSelectQcTestSets[0].Path.Remove(mSelectQcTestSets[0].Path.LastIndexOf('\\'), mSelectQcTestSets[0].Path.Length - mSelectQcTestSets[0].Path.LastIndexOf('\\'));
                        foreach (QCTestSetTreeItem ts in mSelectQcTestSets)
                        {
                            string path = ts.Path.Remove(ts.Path.LastIndexOf('\\'), ts.Path.Length - ts.Path.LastIndexOf('\\'));
                            if (path.Length < mainFolderPath.Length)
                            {
                                mainFolderPath = path;
                            }
                        }
                        QCTestSetsPathTextBox.Text = mainFolderPath;
                    }
                    else
                    {
                        QCTestSetsPathTextBox.Text = mSelectQcTestSets[0].Path;
                    }
                }

                LoadData();
            }
        }

        private void LoadData()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            SetTestSetsData();
            SetPieData();
            Mouse.OverrideCursor = null;
        }

        private void SetTestSetsData()
        {
            mQcTestCasesList.Clear();
            mTestCaseDetailsList.Clear();
            mAutomatedPrecentage = 0;
            mExecutedNumber = 0;
            mRunsNumber = 0;
            mPassedRunsPrecentage = 0;
            if (mSelectQcTestSets != null && mSelectQcTestSets.Count > 0)
            {
                foreach (QCTestSetTreeItem testSetItem in mSelectQcTestSets)
                {
                    ALMTestSet TS = new ALMTestSet
                    {
                        TestSetID = testSetItem.TestSetID,
                        TestSetName = testSetItem.TestSetName,
                        TestSetPath = testSetItem.Path
                    };
                    TS = ImportFromQC.ImportTestSetData(TS);//get test cases

                    foreach (ALMTSTest tc in TS.Tests)
                    {
                        mQcTestCasesList.Add(tc);
                        int automatedStepsCouter = 0;
                        QCManagerReportTestCaseDetails testCaseDetails = new QCManagerReportTestCaseDetails
                        {
                            TestSetID = TS.TestSetID,
                            TestSetName = TS.TestSetName,
                            TestCaseID = tc.LinkedTestID,
                            TestCaseName = tc.TestName
                        };

                        //check if the TC is already exist in repository
                        ActivitiesGroup repoActivsGroup = null;

                        ObservableList<ActivitiesGroup> activitiesGroup = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
                        if (tc.LinkedTestID != null && tc.LinkedTestID != string.Empty)
                        {
                            repoActivsGroup = activitiesGroup.FirstOrDefault(x => x.ExternalID == tc.LinkedTestID);
                        }

                        if (repoActivsGroup == null)
                        {
                            repoActivsGroup = activitiesGroup.FirstOrDefault(x => x.ExternalID == tc.TestID);
                        }

                        if (repoActivsGroup != null)
                        {
                            testCaseDetails.ActivitiesGroupID = repoActivsGroup.Guid;
                            testCaseDetails.ActivitiesGroupName = repoActivsGroup.Name;
                            //check for automation percentage
                            foreach (ALMTSTestStep step in tc.Steps)
                            {
                                ObservableList<Activity> activities = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
                                Activity repoStepActivity = activities.FirstOrDefault(x => x.ExternalID == step.StepID);
                                if (repoStepActivity != null)
                                {
                                    if (repoStepActivity.AutomationStatus == eActivityAutomationStatus.Automated)
                                    {
                                        automatedStepsCouter++;
                                    }
                                }
                            }
                        }
                        else
                        {
                            testCaseDetails.ActivitiesGroupName = "NA";
                        }
                        //set automation percentage
                        double automatedActsPrecanteg = 0;
                        if (tc.Steps.Count > 0)
                        {
                            automatedActsPrecanteg = (automatedStepsCouter / (double)tc.Steps.Count);
                            mAutomatedPrecentage += automatedActsPrecanteg;
                            automatedActsPrecanteg = Math.Floor(automatedActsPrecanteg * 100);
                            testCaseDetails.ActivitiesGroupAutomationPrecentage = automatedActsPrecanteg.ToString() + "%";
                        }
                        else
                        {
                            testCaseDetails.ActivitiesGroupAutomationPrecentage = "0%";
                        }

                        //set execution details
                        CalculateTCExecutionDetails(tc, testCaseDetails);

                        mTestCaseDetailsList.Add(testCaseDetails);
                    }
                }
            }

            //get the executers names
            var groups = mQcTestCasesList.SelectMany(x => x.Runs).GroupBy(y => y.Tester)
                .Select(n => new
                {
                    TesterName = n.Key.ToString(),
                    Count = n.Count()
                }
            )
            .OrderBy(n => n.TesterName);
            ExecutionTesterFilterComboBox.SelectionChanged -= ExecutionTesterFilterComboBox_SelectionChanged;
            ExecutionTesterFilterComboBox.SelectedItem = null;
            ExecutionTesterFilterComboBox.Items.Clear();
            ExecutionTesterFilterComboBox.Items.Add("Any");
            foreach (var v in groups)
            {
                ExecutionTesterFilterComboBox.Items.Add(v.TesterName);
            }

            ExecutionTesterFilterComboBox.SelectedValue = "Any";
            ExecutionTesterFilterComboBox.SelectionChanged += ExecutionTesterFilterComboBox_SelectionChanged;
        }

        private void RefreshExecutionDetails()
        {
            mExecutedNumber = 0;
            mRunsNumber = 0;
            mPassedRunsPrecentage = 0;
            foreach (QCManagerReportTestCaseDetails tcDetails in mTestCaseDetailsList)
            {
                CalculateTCExecutionDetails(mQcTestCasesList.FirstOrDefault(x => x.LinkedTestID == tcDetails.TestCaseID), tcDetails);
            }
        }

        private void CalculateTCExecutionDetails(ALMTSTest tc, QCManagerReportTestCaseDetails testCaseDetails)
        {
            List<ALMTSTestRun> filteredRuns = FilterRuns(tc.Runs);
            if (filteredRuns != null && filteredRuns.Count > 0)
            {
                testCaseDetails.NumberOfExecutions = filteredRuns.Where(x => x.RunName.ToUpper().Contains("GINGER") == true).Count();
                testCaseDetails.NumberOfPassedExecutions = filteredRuns.Where(x => x.RunName.ToUpper().Contains("GINGER") == true && x.Status == "Passed").Count();

                if (testCaseDetails.NumberOfExecutions > 0)
                {
                    mExecutedNumber++;
                    mRunsNumber += testCaseDetails.NumberOfExecutions;
                    mPassedRunsPrecentage += mRunsNumber * (testCaseDetails.NumberOfPassedExecutions / (double)testCaseDetails.NumberOfExecutions);
                }
                try
                {
                    DateTime date = DateTime.Parse((filteredRuns[0].ExecutionDate));
                    testCaseDetails.LastExecutionTime = date.ToString("MM/dd/yyyy") + " " + filteredRuns[0].ExecutionTime;
                }
                catch (Exception ex)
                {
                    testCaseDetails.LastExecutionTime = (filteredRuns[0].ExecutionDate);
                    Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                }
                testCaseDetails.LastExecutionStatus = filteredRuns[0].Status;
            }
            else
            {
                testCaseDetails.NumberOfExecutions = 0;
                testCaseDetails.NumberOfPassedExecutions = 0;
                testCaseDetails.LastExecutionTime = "NA";
                testCaseDetails.LastExecutionStatus = "NA";
            }
        }

        private List<ALMTSTestRun> FilterRuns(List<ALMTSTestRun> runs)
        {
            //filter by tester
            List<ALMTSTestRun> filteredRuns = null;
            if (mExecutionTesterSelectedFilter == "Any")
            {
                filteredRuns = runs;
            }
            else
            {
                filteredRuns = runs.Where(x => x.Tester == mExecutionTesterSelectedFilter).ToList();
            }

            if (filteredRuns.Count > 0)
            {
                //filter by period
                if (mExecutionPeriodSelectedFilter != eExecutionPeriod.Any)
                {
                    return filteredRuns.Where(x => Math.Floor((DateTime.Now - (DateTime.Parse((x.ExecutionDate)))).TotalDays) <= ((int)mExecutionPeriodSelectedFilter)).ToList(); //(expiryDate - DateTime.Now).TotalDays < 30                                    
                }
            }

            return filteredRuns;
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Free)
        {
            Button RefreshButton = new Button
            {
                Content = "Refresh"
            };
            RefreshButton.Click += new RoutedEventHandler(RefreshButton_Click);

            ObservableList<Button> winButtons = [RefreshButton];

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, null, windowStyle, "QC/ALM Manager Report", this, winButtons);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void SetPieData()
        {
            if (DetailsGrid.DataSourceList.Count > 0)
            {
                PieRow.Height = new GridLength(180);

                switch (mViewType)
                {
                    case eViewType.Coverage:
                        PieChartLayout.LegendHeader = "Coverage";
                        PieChartLayout.PlottedProperty = "Count";
                        List<StatItem> lst =
                        [
                            new StatItem() { Description = "Automated", Count = Math.Round(mAutomatedPrecentage, 1) },
                            new StatItem() { Description = "Not Automated", Count = Math.Round(DetailsGrid.DataSourceList.Count - mAutomatedPrecentage, 1) },
                        ];
                        PieChartLayout.DataContext = lst;

                        PiePassRate.Visibility = System.Windows.Visibility.Collapsed;
                        break;

                    case eViewType.Execution:
                        PieChartLayout.LegendHeader = "Executed";
                        PieChartLayout.PlottedProperty = "Count";
                        List<StatItem> lst2 =
                        [
                            new StatItem() { Description = "Executed", Count = mExecutedNumber },
                            new StatItem() { Description = "Not Executed", Count = DetailsGrid.DataSourceList.Count - mExecutedNumber },
                        ];
                        PieChartLayout.DataContext = lst2;

                        PiePassRate.Visibility = System.Windows.Visibility.Visible;
                        PieChartLayout2.LegendHeader = "Pass Rate";
                        PieChartLayout2.PlottedProperty = "Count";
                        List<StatItem> lst3 =
                        [
                            new StatItem() { Description = "Passed", Count = Math.Round(mPassedRunsPrecentage, 1) },
                            new StatItem() { Description = "Not Passed", Count = Math.Round(mRunsNumber - mPassedRunsPrecentage, 1) },
                        ];
                        PieChartLayout2.DataContext = lst3;
                        break;
                }
            }
            else
            {
                PieRow.Height = new GridLength(0);
            }
        }

        private void ExecutionTesterFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mExecutionTesterSelectedFilter = ExecutionTesterFilterComboBox.SelectedValue.ToString();
            RefreshExecutionDetails();
            DetailsGrid.DataSourceList = mTestCaseDetailsList;
            SetPieData();
        }

        private void ExecutionPeriodFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mExecutionPeriodSelectedFilter = (eExecutionPeriod)ExecutionPeriodFilterComboBox.SelectedValue;
            RefreshExecutionDetails();
            DetailsGrid.DataSourceList = mTestCaseDetailsList;
            SetPieData();
        }
    }
}
