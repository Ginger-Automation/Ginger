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
using Ginger.UserControls;
using System;
using System.Windows;
using System.Windows.Controls;
using GingerCore.ALM.Rally;

namespace Ginger.ALM.Rally
{
    /// <summary>
    /// Interaction logic for RallyImportReviewPage.xaml
    /// </summary>
    public partial class RallyImportReviewPage : Page
    {
        GenericWindow _pageGenericWin = null;
        private RallyTestPlan mTestPlan = null;
        private string mImportDestinationPath = string.Empty;

        public RallyImportReviewPage(RallyTestPlan testPlan = null, string importDestinationPath = "")
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

            view.GridColsView.Add(new GridColView() { Field = RallyTestCase.Fields.RallyID, Header = "Test Case ID", WidthWeight = 7, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = RallyTestCase.Fields.Name, Header = "Test Case Name", WidthWeight = 43,  ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = RallyTestPlan.Fields.CreatedBy, Header = "Created By", WidthWeight = 25, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = RallyTestPlan.Fields.CreationDate, Header = "Creation Date", WidthWeight = 25, ReadOnly = true });

            grdRallyTestPlaneImportReview.SetAllColumnsDefaultView(view);
            grdRallyTestPlaneImportReview.InitViewItems();

            grdRallyTestPlaneImportReview.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGrid));
        }

        private void SetGridData()
        {
            grdRallyTestPlaneImportReview.DataSourceList = mTestPlan.TestCases;
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

            if (ALMIntegration.Instance.ImportSelectedTestSets(mImportDestinationPath, rqmTestPlanList))
                {
                _pageGenericWin.Close();
            }
        }
    }
}
