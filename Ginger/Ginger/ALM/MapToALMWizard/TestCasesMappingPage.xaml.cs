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
using Amdocs.Ginger.Common.Enums;
using Ginger.Actions;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Activities;
using GingerCore.ALM.QC;
using GingerWPF.DragDropLib;
using GingerWPF.WizardLib;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.ALM.MapToALMWizard
{
    /// <summary>
    /// Interaction logic for TestCasesMappingPage.xaml
    /// </summary>
    public partial class TestCasesMappingPage : Page, IWizardPage
    {
        AddMapToALMWizard mWizard;
        public TestCasesMappingPage()
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
                    xMapActivityGroupToTestCaseGrid.isSourceEqualTargetDragAndDrop = true;
                    BindTestcasesData();
                    break;
                case EventType.Active:
                    BindTestcasesData();
                    break;
                case EventType.LeavingForNextPage:
                    if(mWizard.testCasesMappingList.All(tc => tc.aLMTSTest == null))
                    {
                        Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Please Map At least one Test Case.");
                        WizardEventArgs.CancelEvent = true;
                        return;
                    }
                    MapCurrentTestCases();
                    break;
                case EventType.Validate:
                    break;
            }
        }
        #region Binds
        private void BindTestcasesData()
        {
            xMapActivityGroupToTestCaseGrid.DataSourceList = mWizard.testCasesMappingList;
            xUnMapTestCaseGrid.DataSourceList = mWizard.testCasesUnMappedList;
            xMapActivityGroupToTestCaseGrid.Title = GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups, $"Ginger ‘{mWizard.mapBusinessFlow.Name}’ ", "– ALM Test Cases Mapping");
            if(mWizard.AlmTestSetData.TestSetName is not null)
            {
                xUnMapTestCaseGrid.Title = $"ALM '{mWizard.AlmTestSetData.TestSetName}' Test Cases";
            }
        }
        private void Bind()
        {
            xMapActivityGroupToTestCaseGrid.SetTitleLightStyle = true;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ALMTestCaseManualMappingConfig.ActivityGroupName), Header = $"{GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup, "Ginger")}", WidthWeight = 25, AllowSorting = true, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = nameof(ALMTestCaseManualMappingConfig.TestCaseName), Header = "Mapped ALM Test Case", WidthWeight = 25, AllowSorting = true, BindingMode = BindingMode.OneWay });
            GridViewDef mRegularView = new GridViewDef(eGridView.RegularView.ToString());
            xMapActivityGroupToTestCaseGrid.SetAllColumnsDefaultView(view);
            xMapActivityGroupToTestCaseGrid.InitViewItems();

            xUnMapTestCaseGrid.SetTitleLightStyle = true;
            GridViewDef view2 = new GridViewDef(GridViewDef.DefaultViewName);
            view2.GridColsView = new ObservableList<GridColView>();

            view2.GridColsView.Add(new GridColView() { Field = nameof(ALMTSTest.TestName), Header = "ALM Test Case", WidthWeight = 25, AllowSorting = true });
            GridViewDef mRegularView2 = new GridViewDef(eGridView.RegularView.ToString());
            xUnMapTestCaseGrid.AddToolbarTool(eImageType.MapSigns, $"Map selected test case to selected ginger {GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)}", new RoutedEventHandler(MapTestCaseToolbarHandler));
            xUnMapTestCaseGrid.SetAllColumnsDefaultView(view2);
            xUnMapTestCaseGrid.InitViewItems();
            xMapActivityGroupToTestCaseGrid.SetbtnDeleteHandler(UnMapSelectedElementRowsHandler);
            xMapActivityGroupToTestCaseGrid.SetbtnClearAllHandler(UnmapAllRows);
        }
        #endregion
        #region Functions
        private void MapCurrentTestCases()
        {
            foreach (ALMTestCaseManualMappingConfig mappedTc in mWizard.testCasesMappingList)
            {
                // Mapped Activity Group only
                if (mappedTc.aLMTSTest is not null)
                {
                    int actIndex = 0;
                    foreach (ActivityIdentifiers act in mappedTc.activitiesGroup.ActivitiesIdentifiers)
                    {
                        // Check if activity already exists in list.
                        if (mappedTc.testStepsMappingList.Any(ts => ts.activity.ActivityGuid == act.ActivityGuid))
                        {
                            continue;
                        }
                        actIndex = mappedTc.testStepsMappingList.Count;
                        mappedTc.testStepsMappingList.Add(new ALMTestStepManualMappingConfig() { activity = act });
                        // Max Map Test Steps as activities count.
                        if (mappedTc.aLMTSTest.Steps.Count > actIndex)
                        {
                            mappedTc.testStepsMappingList[actIndex].almTestStep = mappedTc.aLMTSTest.Steps[actIndex];
                        }
                    }
                    // Create unmapped test steps list
                    if (!mWizard.testCaseUnmappedStepsDic.ContainsKey(mappedTc.aLMTSTest.TestID))
                    {
                        mWizard.testCaseUnmappedStepsDic.Add(mappedTc.aLMTSTest.TestID, new ObservableList<ALMTSTestStep>());
                        if (mappedTc.aLMTSTest.Steps.Count > mappedTc.activitiesGroup.ActivitiesIdentifiers.Count)
                        {
                            for (int i = mappedTc.activitiesGroup.ActivitiesIdentifiers.Count; i < mappedTc.aLMTSTest.Steps.Count; i++)
                            {
                                mWizard.testCaseUnmappedStepsDic[mappedTc.aLMTSTest.TestID].Add(mappedTc.aLMTSTest.Steps[i]);
                            }
                        }
                    }
                }
            }
        }

        private void MapTestCaseToolbarHandler(object sender, RoutedEventArgs e)
        {
            if (xUnMapTestCaseGrid.Grid.SelectedItems.Count == 1 && xMapActivityGroupToTestCaseGrid.Grid.SelectedItems.Count == 1)
            {
                MapTestCaseHandler((xUnMapTestCaseGrid.Grid.SelectedItem as ALMTSTest)
                    , (xMapActivityGroupToTestCaseGrid.Grid.SelectedItem as ALMTestCaseManualMappingConfig));
            }

        }

        private void UnmapAllRows(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (ALMTestCaseManualMappingConfig atc in mWizard.testCasesMappingList)
                {
                    UnMapTestCaseHandler(atc);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, ex.Message);
            }
        }

        private void UnMapSelectedElementRowsHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (ALMTestCaseManualMappingConfig atc in xMapActivityGroupToTestCaseGrid.Grid.SelectedItems)
                {
                    UnMapTestCaseHandler(atc);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, ex.Message);
            }
        }
        #endregion
        #region Events
        private void xUnMapTestCaseGrid_PreviewDragItem(object sender, EventArgs e)
        {
            if (DragDrop2.DrgInfo.DataIsAssignableToType(typeof(ALMTestCaseManualMappingConfig), true) ||
                DragDrop2.DrgInfo.DataIsAssignableToType(typeof(ALMTSTest), true))
            {
                // OK to drop
                if (DragDrop2.DrgInfo.Data is ALMTestCaseManualMappingConfig)
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
        private void MapTestCaseHandler(ALMTSTest source, ALMTestCaseManualMappingConfig target)
        {
            if (source is null)
            {
                return;
            }
            // if target already mapped, Add to unmapped list.
            if (target.aLMTSTest is not null)
            {
                mWizard.ClearStepsLists(target);
                mWizard.testCasesUnMappedList.Add(target.aLMTSTest);
            }
            // Map test case. 
            target.UpdateMappedTestCase(source);
            // Remove test case from unmapped list. 
            mWizard.testCasesUnMappedList.Remove(source);
        }
        private void UnMapTestCaseHandler(ALMTestCaseManualMappingConfig source)
        {
            if (source.aLMTSTest is null)
            {
                return;
            }
            mWizard.ClearStepsLists(source);
            // Add test case to unmapped list
            mWizard.testCasesUnMappedList.Add(source.aLMTSTest);
            // Remove test case from Map row.
            source.UpdateMappedTestCase(null);
        }
        private void ReplaceMappedTestCaseHandler(ALMTestCaseManualMappingConfig source, ALMTestCaseManualMappingConfig target)
        {
            mWizard.ClearStepsLists(source);
            mWizard.ClearStepsLists(target);
            ALMTSTest targetTemp = target.aLMTSTest;
            // Map test case to Map row.
            target.UpdateMappedTestCase(source.aLMTSTest);
            // Replace target with source.
            source.UpdateMappedTestCase(targetTemp);
        }
        // TODO Move to generic class.
        private int GetItemIndex(ucGrid itemGrid, object item)
        {
            return itemGrid.DataSourceList.IndexOf(item);
        }

        private void xUnMapTestCaseGrid_ItemDropped(object sender, EventArgs e)
        {
            object draggedItem = ((DragInfo)sender).Data as object;
            if (draggedItem == null)
            {
                return;
            }
            // Get Drop Item Data
            var dropItem = DragDrop2.GetGridItemHit(xMapActivityGroupToTestCaseGrid);
            int targetIndex = GetItemIndex(xMapActivityGroupToTestCaseGrid, dropItem as ALMTestCaseManualMappingConfig);
            // Drag item from Mapped grid.
            if (draggedItem is ALMTestCaseManualMappingConfig)
            {
                if ((draggedItem as ALMTestCaseManualMappingConfig).aLMTSTest is null)
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

        private void xMapActivityGroupToTestCaseGrid_DragOver(object sender, DragEventArgs e)
        {
            ucGrid r = (ucGrid)sender;
            if (sender is ucGrid && (sender as ucGrid).Name == xMapActivityGroupToTestCaseGrid.Name)
            {
                var obj = DragDrop2.GetGridItemHit(xMapActivityGroupToTestCaseGrid);
                if (obj is ALMTestCaseManualMappingConfig && (obj as ALMTestCaseManualMappingConfig).activitiesGroup != null)
                {

                    int selectedIndex = xMapActivityGroupToTestCaseGrid.DataSourceList.IndexOf((obj as ALMTestCaseManualMappingConfig));
                    xMapActivityGroupToTestCaseGrid.Drop -= xUnMapTestCaseGrid_ItemDropped;
                }
            }
        }
        #endregion
    }
}
