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
using GingerCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GingerCore.ALM.Rally;
using amdocs.ginger.GingerCoreNET;
using GingerCore.ALM;

namespace Ginger.ALM.Rally
{
    /// <summary>
    /// Interaction logic for RallyPlansExplorerPage.xaml
    /// </summary>
    public partial class RallyPlansExplorerPage : Page
    {
        GenericWindow _pageGenericWin = null;
        private string mImportDestinationPath = string.Empty;

        public RallyPlansExplorerPage(string importDestinationPath = "")
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

            view.GridColsView.Add(new GridColView() { Field = RallyTestPlan.Fields.RallyID, Header = "Test Plan ID", WidthWeight = 15, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = RallyTestPlan.Fields.Name, Header = "Test Plan Name", ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = RallyTestPlan.Fields.CreatedBy, Header = "Created By", WidthWeight = 25, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = RallyTestPlan.Fields.CreationDate, Header = "Creation Date", WidthWeight = 25, ReadOnly = true });
            view.GridColsView.Add(new GridColView() { Field = "Import Test Plan", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["ImportButton"] });

            grdRallyTestPlanes.SetAllColumnsDefaultView(view);
            grdRallyTestPlanes.InitViewItems();

            grdRallyTestPlanes.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGrid));
            grdRallyTestPlanes.AddToolbarTool("@ImportScript_16x16.png", "Import The Selected TestPlan", new RoutedEventHandler(ImportTestPlan));
        }

        private void SetGridData()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            ObservableList<RallyTestPlan> mRallyTestPlansListSortedByDate = new ObservableList<RallyTestPlan>();
            //foreach (RallyTestPlan testPlan in RallyConnect.Instance.GetRallyTestPlansByProject( ALMCore.DefaultAlmConfig.ALMServerURL, ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMProjectName,  WorkSpace.Instance.Solution.Folder + @"Documents\ALM\RQM_Configs", ALMCore.DefaultAlmConfig.ALMProjectName).OrderByDescending(item => item.CreationDate))
            //{
            //    mRallyTestPlansListSortedByDate.Add(testPlan);
            //}
            grdRallyTestPlanes.DataSourceList = mRallyTestPlansListSortedByDate;
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
        
        private void ImportTestPlan(object sender, RoutedEventArgs e)
        {
            ImportTestPlan();
        }

        private void ImportTestPlan()
        {
            if (grdRallyTestPlanes.CurrentItem == null)
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
                return;
            }

            if (ALMIntegration.Instance.ShowImportReviewPage(mImportDestinationPath, grdRallyTestPlanes.CurrentItem) == true)
            {

            }
        }

        private void ImportBtnClicked(object sender, RoutedEventArgs e)
        {
            ImportTestPlan();
        }
    }
}
