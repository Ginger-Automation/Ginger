using Amdocs.Ginger.Common;
using Ginger.UserControls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Amdocs.Ginger.CoreNET.ALMLib.Azure;

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
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = AzureTestPlan.Fields.AzureID, Header = "Azure ID", WidthWeight = 15, ReadOnly = true, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = AzureTestPlan.Fields.Name, Header = "Test Suite Name", ReadOnly = true, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = AzureTestPlan.Fields.State, Header = "State", WidthWeight = 25, ReadOnly = true, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = AzureTestPlan.Fields.Project, Header = "Project", WidthWeight = 25, ReadOnly = true, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = "Import Test Set", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["ImportButton"] });
            grdAzureTestPlan.SetAllColumnsDefaultView(view);
            grdAzureTestPlan.InitViewItems();

            grdAzureTestPlan.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGrid));
            grdAzureTestPlan.AddToolbarTool("@ImportScript_16x16.png", "Import The Selected TestSet", new RoutedEventHandler(ImportTestSet));
        }

        private void SetGridData()
        {

            Mouse.OverrideCursor = Cursors.Wait;
            ObservableList<AzureTestPlan> mAzureTestPlansListSortedByDate = new ObservableList<AzureTestPlan>();
            foreach (AzureTestPlan testSet in ALMIntegration.Instance.GetTestSetExplorer(""))
            {
                mAzureTestPlansListSortedByDate.Add(testSet);
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
            ImportTestSet(sender);
        }

        private void ImportTestSet(object sender, EventArgs e)
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
                ObservableList<Object> AzureTestPlanList = new ObservableList<Object>();
                AzureTestPlanList.Add(mTestSet);

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
            ObservableList<AzureTestPlan> AzureTestPlanList = new ObservableList<AzureTestPlan>();
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
