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

using Amdocs.Ginger.Common;
using GingerCore.ALM.RQM;
using Ginger.UserControls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace Ginger.ALM.RQM
{
    /// <summary>
    /// Interaction logic for RQMImportReviewPage.xaml
    /// </summary>
    public partial class RQMImportReviewPage : Page
    {
        GenericWindow _pageGenericWin = null;
        private RQMTestPlan mTestPlan = null;
        private string mImportDestinationPath = string.Empty;

        public RQMImportReviewPage(RQMTestPlan testPlan = null, string importDestinationPath = "")
        {
            InitializeComponent();
            mTestPlan = testPlan;
            mImportDestinationPath = importDestinationPath;
            SetGridView();
            SetGridData();
        }

        private void SetGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = RQMTestCase.Fields.RQMID, Header = "Test Case ID", WidthWeight = 7, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = RQMTestCase.Fields.Name, Header = "Test Case Name", WidthWeight = 30,  ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = RQMTestCase.Fields.Name, Header = "Test Case Name", WidthWeight = 30, ReadOnly = true });
            if (mTestPlan.TestCases.Where(z => z.TestSuiteId != null && z.TestSuiteId != string.Empty).ToList().Count > 0)
                view.GridColsView.Add(new GridColView() { Field = RQMTestCase.Fields.TestSuiteTitle, Header = "RQM Test Suite", WidthWeight = 30, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = RQMTestCase.Fields.TestScriptsQuantity, Header = "Test Scripts Quantity", WidthWeight = 10, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = RQMTestCase.Fields.SelectedTestScriptName, Header = "Selected Test Script", WidthWeight = 40, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(RQMTestCase.Fields.TestScriptsNamesList, RQMTestCase.Fields.SelectedTestScriptName, true, true)});
            grdRQMTestPlaneImportReview.SetAllColumnsDefaultView(view);
            grdRQMTestPlaneImportReview.InitViewItems();

            grdRQMTestPlaneImportReview.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGrid));
        }

        private void SetGridData()
        {
            grdRQMTestPlaneImportReview.DataSourceList = mTestPlan.TestCases;
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button importButton = new Button();
            importButton.Content = "Import";
            importButton.ToolTip = "Import current Test Plan's Test Cases with selected Test Scripts";
            importButton.Click += new RoutedEventHandler(ImportTestPlan);
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, new ObservableList<Button> { importButton });
        }

        private void RefreshGrid(object sender, RoutedEventArgs e)
        {
            SetGridData();
        }

        private void ImportTestPlan(object sender, RoutedEventArgs e)
        {
            ImportTestPlan();
        }

        private void ImportTestPlan()
        {
            ObservableList<Object> rqmTestPlanList = new ObservableList<Object>();
            rqmTestPlanList.Add(mTestPlan);

            if (ALMIntegration.Instance.ImportSelectedTestSets(mImportDestinationPath, rqmTestPlanList) == true)
                {
                _pageGenericWin.Close();
            }
        }
    }
}
