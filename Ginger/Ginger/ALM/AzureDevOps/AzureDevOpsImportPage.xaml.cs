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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.ALMLib.Azure;
using DocumentFormat.OpenXml.Drawing.Charts;
using Ginger.UserControls;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ginger.ALM.AzureDevOps
{
    /// <summary>
    /// Interaction logic for AzureDevOpsImportPage.xaml
    /// </summary>
    public partial class AzureDevOpsImportPage : Page
    {

        GenericWindow _pageGenericWin = null;
        private AzureTestPlan mTestSet = null;
        private string mImportDestinationPath = string.Empty;


        public AzureDevOpsImportPage(string importDestinationPath = " ")
        {
            InitializeComponent();
            mImportDestinationPath = importDestinationPath;
            SetGridView();
            SetGridData();
        }

        private void SetGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = AzureTestPlan.Fields.AzureID, Header = "Azure ID", WidthWeight = 15, ReadOnly = true, AllowSorting = true },
                new GridColView() { Field = AzureTestPlan.Fields.Name, Header = "Test Suite Name", ReadOnly = true, AllowSorting = true },
                new GridColView() { Field = AzureTestPlan.Fields.State, Header = "State", WidthWeight = 25, ReadOnly = true, AllowSorting = true },
                new GridColView() { Field = AzureTestPlan.Fields.Project, Header = "Project", WidthWeight = 25, ReadOnly = true, AllowSorting = true },
                new GridColView() { Field = "Import Test Set", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["ImportButton"] },
            ]
            };
            grdAzureTestPlan.SetAllColumnsDefaultView(view);
            grdAzureTestPlan.InitViewItems();

            grdAzureTestPlan.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGrid));
            grdAzureTestPlan.AddToolbarTool("@ImportScript_16x16.png", "Import The Selected TestSet", new RoutedEventHandler(ImportTestSet));
        }

        private void SetGridData()
        {

            Mouse.OverrideCursor = Cursors.Wait;
            ObservableList<AzureTestPlan> mAzureTestPlansListSortedByDate = [];
            foreach (AzureTestPlan item in ALMIntegration.Instance.GetTestSetExplorer(""))
            {
                mAzureTestPlansListSortedByDate.Add(item);
            }
            
            grdAzureTestPlan.DataSourceList = mAzureTestPlansListSortedByDate;
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

        private void ImportTestSet(object sender, RoutedEventArgs e)
        {
            ImportTestSet();
        }


        private void ImportTestSet()
        {
            if (grdAzureTestPlan.CurrentItem == null)
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
                return;
            }

            if (ALMIntegration.Instance.ShowImportReviewPage(mImportDestinationPath, grdAzureTestPlan.CurrentItem))
            {
                ObservableList<Object> AzureTestPlanList = [mTestSet];

                if (ALMIntegration.Instance.ImportSelectedTestSets(mImportDestinationPath, AzureTestPlanList))
                {
                    _pageGenericWin.Close();
                }
            }
        }

        private void ImportBtnClicked(object sender, RoutedEventArgs e)
        {
            ImportTestSet(sender);
        }

        private void ImportTestSet(object sender)
        {
            ObservableList<AzureTestPlan> AzureTestPlanList = [];
            if (grdAzureTestPlan.CurrentItem == null)
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
                return;
            }
            AzureTestPlanList.Add(grdAzureTestPlan.CurrentItem as AzureTestPlan);

            if (ALMIntegration.Instance.ImportSelectedTestSets(mImportDestinationPath, AzureTestPlanList))
            {
                _pageGenericWin.Close();
            }
        }
    }
}
