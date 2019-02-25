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
using Ginger.ALM;
using Ginger.ALM.QC;
using GingerWPF.DragDropLib;
using Ginger.TagsLib;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Activities;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Linq;
using Amdocs.Ginger.Repository;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.InterfacesLib;

namespace Ginger.Activities
{
    /// <summary>
    /// Interaction logic for ActivitiesGroupsManagerPage.xaml
    /// </summary>
    public partial class ActivitiesGroupsManagerPage : Page
    {
        BusinessFlow mBusinessFlow;
        ActivitiesGroup mSelectedGroup;
        Context mContext = new Context();
        GenericWindow _pageGenericWin = null;

        public ActivitiesGroupsManagerPage(BusinessFlow businessFlow)
        {
            InitializeComponent();

            mBusinessFlow = businessFlow;
            mContext.BusinessFlow = mBusinessFlow;
            mBusinessFlow.SaveBackup();            

            mBusinessFlow.AttachActivitiesGroupsAndActivities();
            mBusinessFlow.UpdateActivitiesGroupDetails(BusinessFlow.eUpdateActivitiesGroupDetailsType.All);

            SetGridsView();
            SetGroupsGridData();
        }

        private void SetGridsView()
        {
            //Groups grid
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();

            defView.GridColsView.Add(new GridColView() { Field = ActivitiesGroup.Fields.Name, Header="Name", WidthWeight = 40 });
            defView.GridColsView.Add(new GridColView() { Field = ActivitiesGroup.Fields.Description, Header = "Description", WidthWeight = 40 });
            if (mBusinessFlow.ActivitiesGroups.Where(z => z.TestSuiteId != null && z.TestSuiteId != string.Empty).ToList().Count > 0)
                defView.GridColsView.Add(new GridColView() { Field = ActivitiesGroup.Fields.TestSuiteTitle, Header = "Test Suite Name", WidthWeight = 40 });
            defView.GridColsView.Add(new GridColView() { Field = ActivitiesGroup.Fields.AutomationPrecentage, Header = "Automation %", WidthWeight = 20, BindingMode = BindingMode.OneWay, ReadOnly=true });
            grdGroups.SetAllColumnsDefaultView(defView);
            grdGroups.InitViewItems();

            grdGroups.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddGroup));
            grdGroups.btnDelete.AddHandler(Button.ClickEvent, new RoutedEventHandler(UpdateActivitiesAfterGroupRemoved));
            grdGroups.btnClearAll.AddHandler(Button.ClickEvent, new RoutedEventHandler(UpdateActivitiesAfterGroupRemoved));           
            grdGroups.AddToolbarTool("@Share_16x16.png", "Export the Selected " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " to ALM", new RoutedEventHandler(ExportToALM));
            grdGroups.AddToolbarTool("@UploadStar_16x16.png", "Add the Selected " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " and it " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " to Shared Repository", new RoutedEventHandler(AddToRepository));

            grdGroups.ItemDropped += grdGroups_ItemDropped;
            grdGroups.PreviewDragItem += grdGroups_PreviewDragItem;
            grdGroups.SetTitleLightStyle = true;

            //Activities grid
            GridViewDef defView2 = new GridViewDef(GridViewDef.DefaultViewName);
            defView2.GridColsView = new ObservableList<GridColView>();

            defView2.GridColsView.Add(new GridColView() { Field = ActivityIdentifiers.Fields.ActivityName, Header = "Name", WidthWeight = 40, ReadOnly = true });
            defView2.GridColsView.Add(new GridColView() { Field = ActivityIdentifiers.Fields.ActivityDescription, Header = "Description", WidthWeight = 40, ReadOnly = true });
            defView2.GridColsView.Add(new GridColView() { Field = ActivityIdentifiers.Fields.ActivityAutomationStatus, Header = "Auto. Status", WidthWeight = 20, ReadOnly = true });
            grdActivities.SetAllColumnsDefaultView(defView2);
            grdActivities.InitViewItems();

            grdActivities.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddActivityToGroup));
            grdActivities.btnDelete.AddHandler(Button.ClickEvent, new RoutedEventHandler(UpdateRemovedActivities));
            grdActivities.btnClearAll.AddHandler(Button.ClickEvent, new RoutedEventHandler(UpdateRemovedActivities));
            grdActivities.SetTitleLightStyle = true;
        }

        private void SetGroupsGridData()
        {            
            grdGroups.DataSourceList = mBusinessFlow.ActivitiesGroups;
            grdGroups.Grid.SelectedCellsChanged += grdGroups_SelectedCellsChanged;
            grdGroups.Grid.CellEditEnding += grdGroups_CellEditEnding;
            if (grdGroups.Grid.Items.Count > 0)
            {
                grdGroups.Grid.SelectedItem = grdGroups.Grid.Items[0];
                mSelectedGroup = (ActivitiesGroup)grdGroups.Grid.Items[0];
                SetActivitiesGridData(mSelectedGroup);
            }
            else
            {
                SetActivitiesGridData();//empty
            }
        }

        private void grdGroups_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //Validate the name of the Groups is unique
            if(e.Column.DisplayIndex == 0)//Group Name Column
            {
                ActivitiesGroup editedActg = (ActivitiesGroup)grdGroups.CurrentItem;
                if (editedActg.Name == string.Empty) editedActg.Name = "Group";
                mBusinessFlow.SetUniqueActivitiesGroupName(editedActg);
                if (grdActivities != null)
                    grdActivities.Title = "'" + editedActg.Name + "' - Grouped " + GingerDicser.GetTermResValue(eTermResKey.Activities);
            }
        }

        private void grdGroups_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (grdGroups.Grid.Items.Count > 0 && grdGroups.Grid.SelectedItem != null)
            {
                if ((ActivitiesGroup)grdGroups.Grid.SelectedItem != mSelectedGroup)
                {
                    mSelectedGroup = (ActivitiesGroup)grdGroups.Grid.SelectedItem;
                    SetActivitiesGridData(mSelectedGroup);
                }
            }
            else
            {
                SetActivitiesGridData();
            }
        }

        private void SetActivitiesGridData(ActivitiesGroup group = null)
        {
            if (group != null)
            {
                grdActivities.Title = "'" + mSelectedGroup.Name + "' - Grouped " + GingerDicser.GetTermResValue(eTermResKey.Activities);
                grdActivities.DataSourceList = mSelectedGroup.ActivitiesIdentifiers;
            }
            else
            {
                grdActivities.Title = "Grouped " + GingerDicser.GetTermResValue(eTermResKey.Activities);
                grdActivities.DataSourceList = new ObservableList<ActivityIdentifiers>();
            }
        }

        private void AddGroup(object sender, RoutedEventArgs e)
        {
            mBusinessFlow.AddActivitiesGroup();
        }

        private void UpdateActivitiesAfterGroupRemoved(object sender, RoutedEventArgs e)
        {
            mBusinessFlow.UpdateActivitiesGroupDetails(BusinessFlow.eUpdateActivitiesGroupDetailsType.ClearUnExistedGroups);
        }

        private void AddActivityToGroup(object sender, RoutedEventArgs e)
        {
            ActivitiesGroup selectedGroup = (ActivitiesGroup)grdGroups.Grid.SelectedItem;
            if (selectedGroup == null)
                Reporter.ToUser(eUserMsgKey.NoActivitiesGroupWasSelected);
            else
            {
                ActivitiesGroupsActivitiesSelectionPage actsSlecPage = new ActivitiesGroupsActivitiesSelectionPage(mBusinessFlow, selectedGroup);
                actsSlecPage.ShowAsWindow();
                selectedGroup.OnPropertyChanged(ActivitiesGroup.Fields.AutomationPrecentage);
            }            
        }

        private void UpdateRemovedActivities(object sender, RoutedEventArgs e)
        {
            mBusinessFlow.UpdateActivitiesGroupDetails(BusinessFlow.eUpdateActivitiesGroupDetailsType.FreeUnAttachedActivities);
            if ((ActivitiesGroup)grdGroups.Grid.SelectedItem != null)
                ((ActivitiesGroup)grdGroups.Grid.SelectedItem).OnPropertyChanged(ActivitiesGroup.Fields.AutomationPrecentage);
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button okBtn = new Button();
            okBtn.Content = "Ok";
            okBtn.Click += new RoutedEventHandler(okBtn_Click);

            Button undoBtn = new Button();
            undoBtn.Content = "Undo & Close";
            undoBtn.Click += new RoutedEventHandler(undoBtn_Click);
           
            ObservableList<Button> winButtons = new ObservableList<Button>();
            winButtons.Add(okBtn);
            winButtons.Add(undoBtn);

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, "'" + mBusinessFlow.Name + "'- " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " Manager", this, winButtons, false, "Undo & Close", CloseWinClicked);             
        }

        private void AddToRepository(object sender, RoutedEventArgs e)
        {
            (new Repository.SharedRepositoryOperations()).AddItemsToRepository(mContext, grdGroups.Grid.SelectedItems.Cast<RepositoryItemBase>().ToList());
        }

        private void ExportToALM(object sender, RoutedEventArgs e)
        {
            if (grdGroups.Grid.SelectedItems != null)
            {
                ObservableList<ActivitiesGroup> selectedAGs = new ObservableList<ActivitiesGroup>();
                foreach (ActivitiesGroup ag in grdGroups.Grid.SelectedItems) selectedAGs.Add(ag);
                ALMIntegration.Instance.ExportBfActivitiesGroupsToALM(mBusinessFlow, selectedAGs);
            }
            else
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
        }

        private void grdGroups_PreviewDragItem(object sender, EventArgs e)
        {
            if (DragDrop2.DragInfo.DataIsAssignableToType(typeof(ActivitiesGroup)))
            {
                // OK to drop                         
                DragDrop2.DragInfo.DragIcon = GingerWPF.DragDropLib.DragInfo.eDragIcon.Copy;
            }
        }

        private void grdGroups_ItemDropped(object sender, EventArgs e)
        {
            object droppedItem = ((DragInfo)sender).Data;
            if (droppedItem.GetType() == typeof(Activity))
            {
                Activity instance = (Activity)((Activity)droppedItem).CreateInstance();
                instance.Active = true;
                mBusinessFlow.SetActivityTargetApplication(instance);
                mBusinessFlow.AddActivity(instance);
            }
            else if (droppedItem.GetType() == typeof(ActivitiesGroup))
            {
                ActivitiesGroup droppedGroupIns = (ActivitiesGroup)((ActivitiesGroup)droppedItem).CreateInstance();
                mBusinessFlow.AddActivitiesGroup(droppedGroupIns);
                ObservableList<Activity> activities = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
                mBusinessFlow.ImportActivitiesGroupActivitiesFromRepository(droppedGroupIns, activities, false);
                mBusinessFlow.AttachActivitiesGroupsAndActivities();
            }
        }

        private void UndoChangesAndClose()
        {
            Mouse.OverrideCursor = Cursors.Wait;            
            mBusinessFlow.RestoreFromBackup(true);
            Mouse.OverrideCursor = null;

            _pageGenericWin.Close();
        }

        private void CloseWinClicked(object sender, EventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.AskIfToUndoChanges) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                UndoChangesAndClose();
            }
        }

        private void undoBtn_Click(object sender, RoutedEventArgs e)
        {
            UndoChangesAndClose();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {            
            _pageGenericWin.Close();
        }
    }
}
