using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Ginger.Actions;
using Ginger.UserControls;
using GingerCore;
using GingerCore.ALM.QC;
using GingerWPF.DragDropLib;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ginger.ALM.MappedToALMWizard
{
    /// <summary>
    /// Interaction logic for TestCasesMappingPage.xaml
    /// </summary>
    public partial class TestCasesMappingPage : Page, IWizardPage
    {
        AddMappedToALMWizard mWizard;
        private BusinessFlow mapBusinessFlow;
        public TestCasesMappingPage()
        {
            InitializeComponent();
        }

        public TestCasesMappingPage(BusinessFlow mapBusinessFlow)
        {
            InitializeComponent();
            var rowStyle = new Style { TargetType = typeof(DataGridRow) };
            var rowTrigger = new Trigger { Property = DataGridRow.IsMouseOverProperty, Value = true };
            rowTrigger.Setters.Add(new Setter(ForegroundProperty, Brushes.Red));
            rowTrigger.Setters.Add(new Setter(BackgroundProperty, Brushes.Black));
            rowStyle.Triggers.Add(rowTrigger);
            xMapActivityGroupToTestCaseGrid.grdMain.RowStyle = rowStyle;
            this.mapBusinessFlow = mapBusinessFlow;
            Bind();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            mWizard = (AddMappedToALMWizard)WizardEventArgs.Wizard;
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    xMapActivityGroupToTestCaseGrid.isSourceEqualTargetDragAndDrop = true;
                    break;
                case EventType.Active:
                    Bind2();
                    break;
            }
        }
        private void Bind2()
        {
            xMapActivityGroupToTestCaseGrid.DataSourceList = mWizard.testCasesMappingList;
            xUnMapTestCaseGrid.DataSourceList = mWizard.testCasesUnMappedList;
        }
        private void Bind()
        {
            xMapActivityGroupToTestCaseGrid.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ALMTestCaseManualMappingConfig.ActivityGroupName), Header = "Activity Group", WidthWeight = 25, AllowSorting = true, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = nameof(ALMTestCaseManualMappingConfig.TestCaseName), Header = "Test Case", WidthWeight = 25, AllowSorting = true, BindingMode = BindingMode.OneWay });
            GridViewDef mRegularView = new GridViewDef(eGridView.RegularView.ToString());
            //mRegularView.GridColsView = new ObservableList<GridColView>();
            //mRegularView.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.StatusIcon), Visible = false });
            //xMapActivityGroupToTestCaseGrid.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(DeleteTestCaseHandler));
            //xMapActivityGroupToTestCaseGrid.AddToolbarTool(eImageType.MapSigns, "Remove elements from mapped list", new RoutedEventHandler(RemoveElementsToMappedBtnClicked));
            //xMapActivityGroupTestCaseGrid.AddCustomView(mRegularView);
            xMapActivityGroupToTestCaseGrid.SetAllColumnsDefaultView(view);
            xMapActivityGroupToTestCaseGrid.InitViewItems();

            xUnMapTestCaseGrid.SetTitleLightStyle = true;
            GridViewDef view2 = new GridViewDef(GridViewDef.DefaultViewName);
            view2.GridColsView = new ObservableList<GridColView>();

            view2.GridColsView.Add(new GridColView() { Field = nameof(ALMTSTest.TestName), Header = "Test Case", WidthWeight = 25, AllowSorting = true });
            GridViewDef mRegularView2 = new GridViewDef(eGridView.RegularView.ToString());
            //mRegularView.GridColsView = new ObservableList<GridColView>();
            //mRegularView.GridColsView.Add(new GridColView() { Field = nameof(ElementInfo.StatusIcon), Visible = false });
            //xMapActivityGroupToTestCaseGrid.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(DeleteTestCaseHandler));
            //xMapActivityGroupToTestCaseGrid.AddToolbarTool(eImageType.MapSigns, "Remove elements from mapped list", new RoutedEventHandler(RemoveElementsToMappedBtnClicked));
            //xMapActivityGroupTestCaseGrid.AddCustomView(mRegularView);
            xUnMapTestCaseGrid.SetAllColumnsDefaultView(view2);
            xUnMapTestCaseGrid.InitViewItems();
        }

        private void xUnMapTestCaseGrid_PreviewDragItem(object sender, EventArgs e)
        {
            if (DragDrop2.DrgInfo.DataIsAssignableToType(typeof(ALMTestCaseManualMappingConfig), true) ||
                DragDrop2.DrgInfo.DataIsAssignableToType(typeof(ALMTSTest) , true))
            {
                // OK to drop
                if (DragDrop2.DrgInfo.Data is ALMTestCaseManualMappingConfig)
                {
                    if(DragDrop2.DrgInfo.DragSource == DragDrop2.DrgInfo.DragTarget)
                    {
                        return;
                    }
                    DragDrop2.SetDragIcon(true, false);
                }
                else
                {
                    DragDrop2.SetDragIcon(true);
                }
            }
            else
            {
                // Do Not Drop
                DragDrop2.SetDragIcon(false);
            }
        }
        private void MapTestCaseHandler(ALMTSTest source, ALMTestCaseManualMappingConfig target)
        {
            // if target already mapped, Add to unmapped list.
            if (target.aLMTSTest is not null)
            {
                mWizard.testCasesUnMappedList.Add(target.aLMTSTest);
            }
            // Map test case. 
            target.UpdateMappedTestCase(source);
            // Remove test case from unmapped list. 
            mWizard.testCasesUnMappedList.Remove(source);
        }
        private void UnMapTestCaseHandler(ALMTestCaseManualMappingConfig source)
        {
            // Add test case to unmapped list
            mWizard.testCasesUnMappedList.Add(source.aLMTSTest);
            // Remove test case from Map row.
            source.UpdateMappedTestCase(null);
        }
        private void ReplaceMappedTestCaseHandler(ALMTestCaseManualMappingConfig source, ALMTestCaseManualMappingConfig target)
        {
            ALMTSTest targetTemp = target.aLMTSTest;
            // Map test case to Map row.
            target.UpdateMappedTestCase(source.aLMTSTest);
            // Replace target with source.
            source.UpdateMappedTestCase(targetTemp);
        }
        private int GetItemIndex(ucGrid itemGrid, object item)
        {
            return itemGrid.DataSourceList.IndexOf(item);
        }
        private void xUnMapTestCaseGrid_ItemDropped(object sender, EventArgs e)
        {
            object draggedItem = ((DragInfo)sender).Data as object;
            if(draggedItem == null)
            {
                return;
            }
            // Get Drop Item Data
            var dropItem = DragDrop2.GetGridItemHit(xMapActivityGroupToTestCaseGrid);
            int targetIndex = GetItemIndex(xMapActivityGroupToTestCaseGrid, dropItem as ALMTestCaseManualMappingConfig);
            // Drag item from Mapped grid.
            if (draggedItem is ALMTestCaseManualMappingConfig)
            {
                if((draggedItem as ALMTestCaseManualMappingConfig).aLMTSTest is null)
                {
                    return;
                }
                int sourceIndex = GetItemIndex(xMapActivityGroupToTestCaseGrid, draggedItem);
                // Drag Source & Target is Mapped Grid items.
                if (((DragInfo)sender).DragSource == ((DragInfo)sender).DragTarget)
                {
                    // Drag & Drop to same row.
                    if (sourceIndex == targetIndex)
                    {
                        return;
                    }
                    ReplaceMappedTestCaseHandler(draggedItem as ALMTestCaseManualMappingConfig, dropItem as ALMTestCaseManualMappingConfig);
                }
                // Drag from Mapped grid to UnMapped Grid items.
                else
                {
                    UnMapTestCaseHandler(draggedItem as ALMTestCaseManualMappingConfig);
                }
            }
            // Source item is from UnMapped Grid.
            if (draggedItem is ALMTSTest)
            {
                MapTestCaseHandler(draggedItem as ALMTSTest, (ALMTestCaseManualMappingConfig)xMapActivityGroupToTestCaseGrid.DataSourceList[targetIndex]);
            }
        }

        private void DroppedItemHandler(object droppedItem, int mouseIndex)
        {
            //int lastAddedIndex = -1;

            //lastAddedIndex = ActionsFactory.AddActionsHandler(droppedItem, mContext, mouseIndex);

            //if (lastAddedIndex > mouseIndex)
            //{
            //    ListView.xListView.SelectedItems.Clear();
            //    for (int itemIndex = mouseIndex; itemIndex < lastAddedIndex; itemIndex++)
            //    {
            //        RepositoryItemBase repoBaseItem = ListView.DataSourceList[itemIndex] as RepositoryItemBase;
            //        if (repoBaseItem != null)
            //        {
            //            ListView.xListView.SelectedItems.Add(repoBaseItem);
            //        }
            //    }
            //}
        }

        private void xMapActivityGroupToTestCaseGrid_DragOver(object sender, DragEventArgs e)
        {
            ucGrid r = (ucGrid)sender;
            if (sender is ucGrid && (sender as ucGrid).Name == xMapActivityGroupToTestCaseGrid.Name)
            {
                var obj = DragDrop2.GetGridItemHit(xMapActivityGroupToTestCaseGrid);
                if(obj is ALMTestCaseManualMappingConfig && (obj as ALMTestCaseManualMappingConfig).activitiesGroup != null)
                {

                    int selectedIndex = xMapActivityGroupToTestCaseGrid.DataSourceList.IndexOf((obj as ALMTestCaseManualMappingConfig));
                    xMapActivityGroupToTestCaseGrid.Drop -= xUnMapTestCaseGrid_ItemDropped;
                    xMapActivityGroupToTestCaseGrid.Drop += XMapActivityGroupToTestCaseGrid_Drop; 
                }
            }
        }

        private void XMapActivityGroupToTestCaseGrid_Drop(object sender, DragEventArgs e)
        {
            //object droppedItem = ((DragInfo)sender).Data as object;
            //if (droppedItem is ALMTestCaseManualMappingConfig)
            //{
            //    // Drag Source is Mapped Grid.
            //    if (((System.Windows.FrameworkElement)((DragInfo)sender).DragSource).Name == xMapActivityGroupToTestCaseGrid.Name)
            //    {
            //        // Test Case in Mapped Grid --- TODO if not in mapped grid and then don't need the if
            //        if ((droppedItem as ALMTestCaseManualMappingConfig).aLMTSTest != null)
            //        {
            //            if (((System.Windows.FrameworkElement)((DragInfo)sender).DragSource).Name == ((System.Windows.FrameworkElement)((DragInfo)sender).DragTarget).Name)
            //            {
            //                int selectedIndexSource = xMapActivityGroupToTestCaseGrid.DataSourceList.IndexOf(droppedItem);

            //                return;
            //            }
            //            ALMTestCaseManualMappingConfig dragedItem2 = (ALMTestCaseManualMappingConfig)((DragInfo)sender).Data;
            //            ALMTSTest draggedTestCase = dragedItem2.aLMTSTest;
            //            //Add Item to Unmapped Grid - to add it in separate function
            //            xUnMapTestCaseGrid.DataSourceList.Add(draggedTestCase);
            //            //Remove Item  from Mapped Grid - to add it in separate function
            //            dragedItem2.aLMTSTest = null;
            //            xMapActivityGroupToTestCaseGrid.Grid.Items.Refresh();
            //        }
            //    }
            //}
        }

        private void xMapActivityGroupToTestCaseGrid_DragEnter(object sender, DragEventArgs e)
        {
            if (((ucGrid)sender).Name == xMapActivityGroupToTestCaseGrid.Name)
            {
                //DragDrop2.SetDragIcon(true);
                //DragDrop2.DrgInfo.DragIcon = DragInfo.eDragIcon.Move;
                //EventHandler handler = xUnMapTestCaseGrid_PreviewDragItem;
                //if (handler != null)
                //{
                //    handler(DragDrop2.DrgInfo, new EventArgs());
                //}
            }

        }
    }
}
