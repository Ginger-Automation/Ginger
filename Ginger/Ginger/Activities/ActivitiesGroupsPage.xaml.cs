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
using GingerWPF.DragDropLib;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Activities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Ginger.Repository;
using Amdocs.Ginger.Repository;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.InterfacesLib;

namespace Ginger.Activities
{
    /// <summary>
    /// Interaction logic for ActivitiesGroupsPage.xaml
    /// </summary>
    public partial class ActivitiesGroupsPage : Page
    {
        BusinessFlow mBusinessFlow;
        Context mContext = new Context();
        public ActivitiesGroupsPage(BusinessFlow businessFlow, General.RepositoryItemPageViewMode editMode = General.RepositoryItemPageViewMode.SharedReposiotry)
        {
            InitializeComponent();

            mBusinessFlow = businessFlow;
            mContext.BusinessFlow = mBusinessFlow;
            if (businessFlow != null)
            {
                mBusinessFlow.PropertyChanged += BusinessFlow_PropertyChanged;
            }

            SetActivitiesGroupsGridView();
            RefreshActivitiesGroupsGrid();

            if (editMode == General.RepositoryItemPageViewMode.View)
            {
                grdActivitiesGroups.ShowToolsBar = Visibility.Collapsed;
                grdActivitiesGroups.ToolsTray.Visibility = Visibility.Collapsed;
                grdActivitiesGroups.RowDoubleClick -= grdActivitiesGroups_grdMain_MouseDoubleClick;
                grdActivitiesGroups.DisableGridColoumns();
            }
        }

        private void grdActivitiesGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mBusinessFlow != null)
            {
                foreach (Activity a in mBusinessFlow.Activities)
                {
                    a.AGSelected = false;
                }
                grdActivitiesGroups_RowChangedEvent(sender, e);
            }
        }

        private void grdActivitiesGroups_RowChangedEvent(object sender, EventArgs e)
        {
            if (((GingerCore.Activities.ActivitiesGroup)((DataGrid)sender).SelectedItem) == null)
                return;

            foreach (Activity a in mBusinessFlow.Activities)
            {
                a.AGSelected = false;
            }

            string Name = ((GingerCore.Activities.ActivitiesGroup)((DataGrid)sender).SelectedItem).Name;
            if (Name == "Optimized Activities" || Name == "Optimized Activities - Not in Use")
                return;
            List<Activity> RelatedActivities = mBusinessFlow.Activities.Where(x => x.ActivitiesGroupID == Name).ToList();

            foreach (Activity a in RelatedActivities)
            {
                a.AGSelected = true;
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (grdActivitiesGroups.btnEdit.IsMouseOver)
            {
                if (grdActivitiesGroups.Grid.SelectedItem != null)
                {
                    ActivitiesGroupPage page = new ActivitiesGroupPage((ActivitiesGroup)grdActivitiesGroups.Grid.SelectedItem, mBusinessFlow, ActivitiesGroupPage.eEditMode.ExecutionFlow);
                    page.ShowAsWindow();
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
                }
            }
        }

        public void UpdateBusinessFlow(BusinessFlow bf)
        {
            if (bf != mBusinessFlow)
            {
                mBusinessFlow = bf;
                mContext.BusinessFlow = mBusinessFlow;
                if (mBusinessFlow != null)
                    mBusinessFlow.PropertyChanged += BusinessFlow_PropertyChanged;
            }
            RefreshActivitiesGroupsGrid();
        }

        private void BusinessFlow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == BusinessFlow.Fields.ActivitiesGroups)
            {
                RefreshActivitiesGroupsGrid();
            }
        }

        private void grdActivitiesGroups_PreviewDragItem(object sender, EventArgs e)
        {
            if (DragDrop2.DragInfo.DataIsAssignableToType(typeof(ActivitiesGroup)))
            {
                // OK to drop                         
                DragDrop2.DragInfo.DragIcon = GingerWPF.DragDropLib.DragInfo.eDragIcon.Copy;
            }            
        }

        private void grdActivitiesGroups_ItemDropped(object sender, EventArgs e)
        {
            object droppedItem = ((DragInfo)sender).Data;
           
            if (droppedItem.GetType() == typeof(ActivitiesGroup))
            {
                ActivitiesGroup droppedGroupIns = (ActivitiesGroup)((ActivitiesGroup)droppedItem).CreateInstance(true);
                mBusinessFlow.AddActivitiesGroup(droppedGroupIns);
                ObservableList<Activity> activities = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();                
                mBusinessFlow.ImportActivitiesGroupActivitiesFromRepository(droppedGroupIns, activities, false, false);
                mBusinessFlow.AttachActivitiesGroupsAndActivities();

                int selectedActIndex = -1;
                ObservableList<ActivitiesGroup> actsList = mBusinessFlow.ActivitiesGroups;
                if (actsList.CurrentItem != null)
                {
                    selectedActIndex = actsList.IndexOf((ActivitiesGroup)actsList.CurrentItem);
                }
                if (selectedActIndex >= 0)
                {
                    actsList.Move(actsList.Count - 1, selectedActIndex + 1);
                }
            }            
        }

        private void RefreshActivitiesGroupsGridHandler(object sender, RoutedEventArgs e)
        {
            RefreshActivitiesGroupsGrid();
        }

        private void SetActivitiesGroupsGridView()
        {
            //Columns view
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = nameof(RepositoryItemBase.ItemImageType), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 2.5, MaxWidth = 20 });
            defView.GridColsView.Add(new GridColView() { Field = nameof(RepositoryItemBase.SharedRepoInstanceImage), Header = "S.R.", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 2.5, MaxWidth = 20 });
            defView.GridColsView.Add(new GridColView() { Field = ActivitiesGroup.Fields.Name, Header = "Name", WidthWeight = 40 });
            defView.GridColsView.Add(new GridColView() { Field = ActivitiesGroup.Fields.Description, Header = "Description", WidthWeight = 40 });
            if (mBusinessFlow.ActivitiesGroups.Where(z => z.TestSuiteId != null && z.TestSuiteId != string.Empty).ToList().Count > 0)
                defView.GridColsView.Add(new GridColView() { Field = ActivitiesGroup.Fields.TestSuiteTitle, Header = "Test Suite Name", WidthWeight = 40 });
            defView.GridColsView.Add(new GridColView() { Field = ActivitiesGroup.Fields.AutomationPrecentage, Header = "Automation %", WidthWeight = 20, BindingMode = BindingMode.OneWay, ReadOnly = true });
            grdActivitiesGroups.SetAllColumnsDefaultView(defView);
            grdActivitiesGroups.InitViewItems();

            //Tool bar
            grdActivitiesGroups.ShowEdit = Visibility.Visible;
            grdActivitiesGroups.ShowTagsFilter = Visibility.Visible;
            grdActivitiesGroups.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshActivitiesGroupsGridHandler));
            grdActivitiesGroups.AddToolbarTool("@Group_16x16.png", GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " Manager", new RoutedEventHandler(LoadActivitiesGroupsManagr));
            grdActivitiesGroups.AddToolbarTool("@ExportToALM_16x16.png", "Export the Selected " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " to ALM", new RoutedEventHandler(ExportToALM));
            grdActivitiesGroups.AddToolbarTool("@RefreshALM_16x16.png", "Refresh from ALM", new RoutedEventHandler(RefereshFromALM));
            grdActivitiesGroups.AddToolbarTool("@UploadStar_16x16.png", "Add the Selected " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " and it " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " to Shared Repository", new RoutedEventHandler(AddToRepository));

            //Events
            grdActivitiesGroups.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnEdit_Click));
            grdActivitiesGroups.RowDoubleClick += grdActivitiesGroups_grdMain_MouseDoubleClick;
            grdActivitiesGroups.ItemDropped += grdActivitiesGroups_ItemDropped;
            grdActivitiesGroups.PreviewDragItem += grdActivitiesGroups_PreviewDragItem;
            grdActivitiesGroups.RowChangedEvent += grdActivitiesGroups_RowChangedEvent;
            grdActivitiesGroups.Grid.SelectionChanged += grdActivitiesGroups_SelectionChanged;
        }

        private void UpdateActivitiesGroupsGridViewTestSuiteColumn()
        {
            //Columns view
            GridViewDef defView = new GridViewDef(GridViewDef.DefaultViewName);
            defView.GridColsView = new ObservableList<GridColView>();
            defView.GridColsView.Add(new GridColView() { Field = nameof(RepositoryItemBase.ItemImageType), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 2.5, MaxWidth = 20 });
            defView.GridColsView.Add(new GridColView() { Field = nameof(RepositoryItemBase.SharedRepoInstanceImage), Header = "S.R.", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 2.5, MaxWidth = 20 });
            defView.GridColsView.Add(new GridColView() { Field = ActivitiesGroup.Fields.Name, Header = "Name", WidthWeight = 40 });
            defView.GridColsView.Add(new GridColView() { Field = ActivitiesGroup.Fields.Description, Header = "Description", WidthWeight = 40 });
            if (mBusinessFlow.ActivitiesGroups.Where(z => z.TestSuiteId != null && z.TestSuiteId != string.Empty).ToList().Count > 0)
                defView.GridColsView.Add(new GridColView() { Field = ActivitiesGroup.Fields.TestSuiteTitle, Header = "RQM Test Suite", WidthWeight = 40 });
            defView.GridColsView.Add(new GridColView() { Field = ActivitiesGroup.Fields.AutomationPrecentage, Header = "Automation %", WidthWeight = 20, BindingMode = BindingMode.OneWay, ReadOnly = true });

            grdActivitiesGroups.updateAndSelectCustomView(defView);
            grdActivitiesGroups.InitViewItems();
        }

        private void AddToRepository(object sender, RoutedEventArgs e)
        {
            List<RepositoryItemBase> listOfGroups = grdActivitiesGroups.Grid.SelectedItems.Cast<RepositoryItemBase>().ToList();

            List<RepositoryItemBase> itemsToUpload = new List<RepositoryItemBase>();
            foreach (RepositoryItemBase group in listOfGroups)
            {
                foreach (ActivityIdentifiers AI in ((ActivitiesGroup)group).ActivitiesIdentifiers)
                {                    
                    itemsToUpload.Add(AI.IdentifiedActivity);                 
                }
            }
            itemsToUpload.AddRange(listOfGroups);

            (new SharedRepositoryOperations()).AddItemsToRepository(mContext, itemsToUpload);
        }

        private void RefereshFromALM(object sender, RoutedEventArgs e)
        {
            if (grdActivitiesGroups.Grid.SelectedItems != null)
            {
                ALMIntegration.Instance.UpdateActivitiesGroup(ref mBusinessFlow, grdActivitiesGroups.Grid.SelectedItems.Cast<ActivitiesGroup>().Select(x => Tuple.Create<string, string>(x.ExternalID, x.ExternalID2)).ToList());
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
        }

        private void ExportToALM(object sender, RoutedEventArgs e)
        {
            if (grdActivitiesGroups.Grid.SelectedItems != null)
            {
                ObservableList<ActivitiesGroup> selectedAGs = new ObservableList<ActivitiesGroup>();
                foreach (ActivitiesGroup ag in grdActivitiesGroups.Grid.SelectedItems) selectedAGs.Add(ag);
                ALMIntegration.Instance.ExportBfActivitiesGroupsToALM(mBusinessFlow, selectedAGs);
            }
            else
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
        }

        private void RefreshActivitiesGroupsGrid()
        {
            if (mBusinessFlow != null)
            {
                grdActivitiesGroups.Title = GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups);
                
                ObservableList<ActivitiesGroup> sharedActivitiesGroups = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
                SharedRepositoryOperations.MarkSharedRepositoryItems((IEnumerable<object>)mBusinessFlow.ActivitiesGroups, (IEnumerable<object>)sharedActivitiesGroups);

                UpdateActivitiesGroupsGridViewTestSuiteColumn();
                grdActivitiesGroups.DataSourceList = mBusinessFlow.ActivitiesGroups;
            }
            else
            {
                grdActivitiesGroups.DataSourceList = new ObservableList<Activity>();
            }
        }       

        private void grdActivitiesGroups_grdMain_MouseDoubleClick(object sender, EventArgs e)
        {
            grdActivitiesGroups.Grid.CommitEdit();
            grdActivitiesGroups.Grid.CancelEdit();
            ActivitiesGroupPage page = new ActivitiesGroupPage(((ActivitiesGroup)grdActivitiesGroups.Grid.SelectedItem), mBusinessFlow, ActivitiesGroupPage.eEditMode.ExecutionFlow);
            page.ShowAsWindow();
        }

        private void LoadActivitiesGroupsManagr(object sender, RoutedEventArgs e)
        {
            grdActivitiesGroups.Grid.CommitEdit();
            grdActivitiesGroups.Grid.CancelEdit(); 
            ActivitiesGroupsManagerPage activitiesGroupsManagerPage = new ActivitiesGroupsManagerPage(mBusinessFlow);
            activitiesGroupsManagerPage.ShowAsWindow();
        }
    }
}
