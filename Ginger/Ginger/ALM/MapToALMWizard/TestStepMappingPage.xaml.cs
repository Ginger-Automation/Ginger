#region License
/*
Copyright © 2014-2021 European Support Limited

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
using Amdocs.Ginger.Common.Enums;
using Ginger.UserControls;
using GingerCore;
using GingerCore.ALM.QC;
using GingerWPF.DragDropLib;
using GingerWPF.WizardLib;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.ALM.MapToALMWizard
{
    /// <summary>
    /// Interaction logic for TestStepMappingPage.xaml
    /// </summary>
    public partial class TestStepMappingPage : Page, IWizardPage
    {
        AddMapToALMWizard mWizard;
        int testCaseListIndex = 0;
        string getCurrentTestCaseId;
        ObservableList<ALMTestCaseManualMappingConfig> mappedTestCasesList = new ObservableList<ALMTestCaseManualMappingConfig>();

        public TestStepMappingPage()
        {
            InitializeComponent();
            Bind();
        }
        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            mWizard = (AddMapToALMWizard)WizardEventArgs.Wizard;
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    xMapTestStepsGrid.isSourceEqualTargetDragAndDrop = true;
                    break;
                case EventType.Active:
                    SetInitialGridsData();
                    break;
            }
        }
        #region Bind
        private void SetInitialGridsData()
        {
            mappedTestCasesList.Clear();
            foreach(ALMTestCaseManualMappingConfig mtc in mWizard.testCasesMappingList)
            {
                if (mtc.aLMTSTest is not null)
                {
                    mappedTestCasesList.Add(mtc);
                }
            }
            xMapTestCasesGrid.DataSourceList = mappedTestCasesList;
            xMapTestStepsGrid.DataSourceList = mappedTestCasesList[testCaseListIndex].testStepsMappingList;
            foreach(ALMTestCaseManualMappingConfig tc in mappedTestCasesList)
            {
                if (tc.aLMTSTest is not null && !mWizard.testCaseUnmappedStepsDic.ContainsKey(tc.aLMTSTest.TestID))
                {
                    mWizard.testCaseUnmappedStepsDic.Add(tc.aLMTSTest.TestID, new ObservableList<ALMTSTestStep>());
                    tc.UpdateTestCaseMapStatus(mWizard.testCaseUnmappedStepsDic[tc.aLMTSTest.TestID].Count);
                }
            }
            xUnMapTestStepsGrid.DataSourceList = mWizard.testCaseUnmappedStepsDic[getCurrentTestCaseId];
            xMapTestCasesGrid.Title = GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups, "Ginger", "& ALM Test Cases - Steps Mapping Status");
            xMapTestStepsGrid.Title = GingerDicser.GetTermResValue(eTermResKey.Activities, $"Ginger ‘{mappedTestCasesList[testCaseListIndex].ActivityGroupName}’ ", "- ALM Steps Mapping");
            xUnMapTestStepsGrid.Title = $"ALM ‘{mappedTestCasesList[testCaseListIndex].TestCaseName}’ Steps";
        }
        
        private void Bind()
        {
            BindMapTestCasesGrid();
            BindMapTestStepsGrid();
            BindUnMapTestStepsGrid();
        }

        private void BindUnMapTestStepsGrid()
        {
            xUnMapTestStepsGrid.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(ALMTSTestStep.StepName), Header = "ALM Step", WidthWeight = 25, AllowSorting = true });
            GridViewDef mRegularView2 = new GridViewDef(eGridView.RegularView.ToString());
            xUnMapTestStepsGrid.AddToolbarTool(eImageType.MapSigns, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup, "Map selected test case to selected ginger") , new RoutedEventHandler(MapTestStepToolbarHandler));
            xUnMapTestStepsGrid.SetAllColumnsDefaultView(view);
            xUnMapTestStepsGrid.InitViewItems();
        }

        private void MapTestStepToolbarHandler(object sender, RoutedEventArgs e)
        {
            if (xUnMapTestStepsGrid.Grid.SelectedItems.Count == 1 && xMapTestStepsGrid.Grid.SelectedItems.Count == 1)
            {
                MapTestStepHandler((xUnMapTestStepsGrid.Grid.SelectedItem as ALMTSTestStep)
                    , (xMapTestStepsGrid.Grid.SelectedItem as ALMTestStepManualMappingConfig));
            }
        }

        private void BindMapTestStepsGrid()
        {
            xMapTestStepsGrid.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(ALMTestStepManualMappingConfig.ActivityName), Header = GingerDicser.GetTermResValue(eTermResKey.Activity, "Ginger"), WidthWeight = 25, AllowSorting = true, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = nameof(ALMTestStepManualMappingConfig.StepName), Header = "Mapped ALM Test Step", WidthWeight = 25, AllowSorting = true, BindingMode = BindingMode.OneWay });
            GridViewDef mRegularView2 = new GridViewDef(eGridView.RegularView.ToString());
            xMapTestStepsGrid.SetAllColumnsDefaultView(view);
            xMapTestStepsGrid.InitViewItems();
            xMapTestStepsGrid.SetbtnDeleteHandler(UnMapSelectedElementRowsHandler);
            xMapTestStepsGrid.SetbtnClearAllHandler(UnmapAllRows);
        }

        private void UnmapAllRows(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (ALMTestStepManualMappingConfig ats in mappedTestCasesList[testCaseListIndex].testStepsMappingList)
                {
                    UnMapTestStepHandler(ats);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, ex.Message);
            }
        }

        private void BindMapTestCasesGrid()
        {
            xMapTestCasesGrid.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(ALMTestCaseManualMappingConfig.ActivityGroupName), Header = GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup, "Ginger"), WidthWeight = 25, AllowSorting = true, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = nameof(ALMTestCaseManualMappingConfig.TestCaseName), Header = "Mapped ALM Test Case", WidthWeight = 25, AllowSorting = true, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = nameof(ALMTestCaseManualMappingConfig.MappingStatusIcon), Header = "Steps Mapping Status", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)xMappingGrid.Resources["xMappingStatusIconTemplate"] });
            GridViewDef mRegularView = new GridViewDef(eGridView.RegularView.ToString());
            xMapTestCasesGrid.SetAllColumnsDefaultView(view);
            xMapTestCasesGrid.InitViewItems();
            xMapTestCasesGrid.Grid.SelectionChanged += xMapTestCasesGrid_SelectionChanged;
        }
        #endregion

        public enum eGridView
        {
            RegularView,
        }
        

       

        #region Functions
        // TODO add to helper functions
        private void MapTestStepHandler(ALMTSTestStep source, ALMTestStepManualMappingConfig target)
        {
            // if target already mapped, Add to unmapped list.
            if (target.almTestStep is not null)
            {
                mWizard.testCaseUnmappedStepsDic[getCurrentTestCaseId].Add(target.almTestStep);
            }
            // Map test step. 
            target.UpdateMappedTestStep(source);
            // Remove test step from unmapped list. 
            mWizard.testCaseUnmappedStepsDic[getCurrentTestCaseId].Remove(source);
            mappedTestCasesList[testCaseListIndex].UpdateTestCaseMapStatus(mWizard.testCaseUnmappedStepsDic[getCurrentTestCaseId].Count);
        }
        private void UnMapTestStepHandler(ALMTestStepManualMappingConfig source)
        {
            // Add test step to unmapped list
            mWizard.testCaseUnmappedStepsDic[getCurrentTestCaseId].Add(source.almTestStep);
            // Remove test step from Map row.
            source.UpdateMappedTestStep(null);
            mappedTestCasesList[testCaseListIndex].UpdateTestCaseMapStatus(mWizard.testCaseUnmappedStepsDic[getCurrentTestCaseId].Count);
        }
        private void ReplaceMappedTestCaseHandler(ALMTestStepManualMappingConfig source, ALMTestStepManualMappingConfig target)
        {
            if(target is null)
            {
                return;
            }
            ALMTSTestStep targetTemp = target.almTestStep;
            // Map test step to Map row.
            target.UpdateMappedTestStep(source.almTestStep);
            // Replace target with source.
            source.UpdateMappedTestStep(targetTemp);
        }
        private int GetItemIndex(ucGrid itemGrid, object item)
        {
            return itemGrid.DataSourceList.IndexOf(item);
        }

        #endregion
        #region Events
        private void xMapTestCasesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateMapTestCasesData();
        }
        private void xMapTestCasesGrid_SelectedItemChanged(object selectedItem)
        {
            UpdateMapTestCasesData();
        }

        private void UpdateMapTestCasesData()
        {
            testCaseListIndex = xMapTestCasesGrid.Grid.SelectedIndex;
            if (testCaseListIndex >= 0)
            {
                getCurrentTestCaseId = mappedTestCasesList[testCaseListIndex].aLMTSTest.TestID;
                xMapTestStepsGrid.DataSourceList = mappedTestCasesList[testCaseListIndex].testStepsMappingList;
                if (mWizard.testCaseUnmappedStepsDic.Count > 0 && mWizard.testCaseUnmappedStepsDic.ContainsKey(getCurrentTestCaseId))
                {
                    xUnMapTestStepsGrid.DataSourceList = mWizard.testCaseUnmappedStepsDic[getCurrentTestCaseId];
                    mappedTestCasesList[testCaseListIndex].UpdateTestCaseMapStatus(mWizard.testCaseUnmappedStepsDic[getCurrentTestCaseId].Count);
                }
                xMapTestStepsGrid.Title = GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup, $"Ginger ‘{mappedTestCasesList[testCaseListIndex].ActivityGroupName}’");
                xUnMapTestStepsGrid.Title = $"ALM ‘{mappedTestCasesList[testCaseListIndex].TestCaseName}’ Steps";
            }
        }

        private void TestStepsMapping_PreviewDragItem(object sender, EventArgs e)
        {
            if (DragDrop2.DrgInfo.DataIsAssignableToType(typeof(ALMTestStepManualMappingConfig), true) ||
                DragDrop2.DrgInfo.DataIsAssignableToType(typeof(ALMTSTestStep), true))
            {
                // OK to drop
                if (DragDrop2.DrgInfo.Data is ALMTestStepManualMappingConfig)
                {
                    if (DragDrop2.DrgInfo.DragSource == DragDrop2.DrgInfo.DragTarget)
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
        private void  TestStepsMapping_ItemDropped(object sender, EventArgs e)
        {
            object draggedItem = ((DragInfo)sender).Data as object;
            if (draggedItem == null)
            {
                return;
            }
            // Get Drop Item Data
            var dropItem = DragDrop2.GetGridItemHit(xMapTestStepsGrid);
            int targetIndex = GetItemIndex(xMapTestStepsGrid, dropItem as ALMTestStepManualMappingConfig);
            // Drag item from Mapped grid.
            if (draggedItem is ALMTestStepManualMappingConfig)
            {
                if ((draggedItem as ALMTestStepManualMappingConfig).almTestStep is null)
                {
                    return;
                }
                int sourceIndex = GetItemIndex(xMapTestStepsGrid, draggedItem);
                // Drag Source & Target is Mapped Grid items.
                if (((DragInfo)sender).DragSource == ((DragInfo)sender).DragTarget)
                {
                    // Drag & Drop to same row.
                    if (sourceIndex == targetIndex)
                    {
                        return;
                    }
                    ReplaceMappedTestCaseHandler(draggedItem as ALMTestStepManualMappingConfig, dropItem as ALMTestStepManualMappingConfig);
                }
                // Drag from Mapped grid to UnMapped Grid items.
                else
                {
                    UnMapTestStepHandler(draggedItem as ALMTestStepManualMappingConfig);
                }
            }
            // Source item is from UnMapped Grid.
            if (draggedItem is ALMTSTestStep)
            {
                MapTestStepHandler(draggedItem as ALMTSTestStep, (ALMTestStepManualMappingConfig)xMapTestStepsGrid.DataSourceList[targetIndex]);
            }
        }
        private void UnMapSelectedElementRowsHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (ALMTestStepManualMappingConfig ats in xMapTestStepsGrid.Grid.SelectedItems)
                {
                    UnMapTestStepHandler(ats);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, ex.Message);
            }
        }
        #endregion

    }
}
