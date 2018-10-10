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

using Amdocs.Ginger.Common;
using GingerCore.ALM.RQM;
using Ginger.UserControls;
using GingerCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ginger.ALM.RQM
{
    /// <summary>
    /// Interaction logic for RQMPlansExplorerPage.xaml
    /// </summary>
    public partial class RQMPlansExplorerPage : Page
    {
        GenericWindow _pageGenericWin = null;
        private string mImportDestinationPath = string.Empty;

        public RQMPlansExplorerPage(string importDestinationPath = "")
        {
            InitializeComponent();
            mImportDestinationPath = importDestinationPath;

            SetGridView();
            SetGridData();
        }

        private void SetGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = RQMTestPlan.Fields.RQMID, Header = "Test Plan ID", WidthWeight = 15, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = RQMTestPlan.Fields.Name, Header = "Test Plan Name", ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = RQMTestPlan.Fields.CreatedBy, Header = "Created By", WidthWeight = 25, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = RQMTestPlan.Fields.CreationDate, Header = "Creation Date", WidthWeight = 25, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = "Import Test Plan", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["ImportButton"] });

            grdRQMTestPlanes.SetAllColumnsDefaultView(view);
            grdRQMTestPlanes.InitViewItems();

            grdRQMTestPlanes.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGrid));
            grdRQMTestPlanes.AddToolbarTool("@ImportScript_16x16.png", "Import The Selected TestPlan", new RoutedEventHandler(ImportTestPlan));
        }

        private void SetGridData()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            ObservableList<RQMTestPlan> mRQMTestPlansListSortedByDate = new ObservableList<RQMTestPlan>();
            foreach (RQMTestPlan testPlan in RQMConnect.Instance.GetRQMTestPlansByProject(App.UserProfile.Solution.ALMServerURL, App.UserProfile.ALMUserName, App.UserProfile.ALMPassword, App.UserProfile.Solution.ALMProject, System.IO.Path.Combine(App.UserProfile.Solution.Folder, @"Documents\ALM\RQM_Configs")).OrderByDescending(item => item.CreationDate))
            {
                mRQMTestPlansListSortedByDate.Add(testPlan);
            }
            grdRQMTestPlanes.DataSourceList = mRQMTestPlansListSortedByDate;
            Mouse.OverrideCursor = null;
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this);
        }

        private void RefreshGrid(object sender, RoutedEventArgs e)
        {
            SetGridData();
        }

        private void ExecutionResultsConfigWindow(object sender, System.Windows.RoutedEventArgs e)
        {
            Ginger.Reports.ExecutionLoggerConfiguration.ExecutionResultsConfigurationPage();
        }

        private void ImportTestPlan(object sender, RoutedEventArgs e)
        {
            ImportTestPlan();
        }

        private void ImportTestPlan(object sender, EventArgs e)
        {
            ImportTestPlan();
        }

        private void ImportTestPlan()
        {
            if (grdRQMTestPlanes.CurrentItem == null)
            {
                Reporter.ToUser(eUserMsgKeys.NoItemWasSelected);
                return;
            }

            if (ALMIntegration.Instance.ShowImportReviewPage(mImportDestinationPath, grdRQMTestPlanes.CurrentItem) == true)
            {

            }
        }

        private void ImportBtnClicked(object sender, RoutedEventArgs e)
        {
            ImportTestPlan();
        }
    }
}
