#region License
/*
Copyright © 2014-2019 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.Activities;
using Ginger.BusinessFlowPages;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Activities;
using GingerWPF.DragDropLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.Repository
{
    /// <summary>
    /// Interaction logic for ActivitiesGroupsRepositoryPage.xaml
    /// </summary>
    public partial class ActivitiesGroupsRepositoryPage : Page
    {
        readonly RepositoryFolder<ActivitiesGroup> mActivitiesGroupFolder;      
        bool mInTreeModeView = false;

        Context mContext = null;

        public ActivitiesGroupsRepositoryPage(RepositoryFolder<ActivitiesGroup> activitiesGroupFolder, Context context)
        {
            InitializeComponent();

            mActivitiesGroupFolder = activitiesGroupFolder;
            mContext = context;

            SetActivitiesRepositoryGridView();            
            SetGridAndTreeData();
        }

        private void SetGridAndTreeData()
        {
            if (mActivitiesGroupFolder.IsRootFolder)
            {
                xActivitiesGroupsRepositoryGrid.DataSourceList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
            }                
            else
            {
                xActivitiesGroupsRepositoryGrid.DataSourceList = mActivitiesGroupFolder.GetFolderItems();
            }                
        }

        public void UpdateBusinessFlow(BusinessFlow bf)
        {
            xActivitiesGroupsRepositoryGrid.ClearFilters();
        }

        private void SetActivitiesRepositoryGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            view.GridColsView = viewCols;

            viewCols.Add(new GridColView() { Field = nameof(ActivitiesGroup.Name), WidthWeight = 50, AllowSorting = true });
            viewCols.Add(new GridColView() { Field = nameof(ActivitiesGroup.Description), WidthWeight = 35, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = "Inst.", WidthWeight = 15, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["ViewInstancesButton"] });
            xActivitiesGroupsRepositoryGrid.SetAllColumnsDefaultView(view);
            xActivitiesGroupsRepositoryGrid.InitViewItems();

            xActivitiesGroupsRepositoryGrid.btnRefresh.Visibility = Visibility.Collapsed;
            xActivitiesGroupsRepositoryGrid.AddToolbarTool("@LeftArrow_16x16.png", "Add to Flow", new RoutedEventHandler(AddFromRepository));
            xActivitiesGroupsRepositoryGrid.AddToolbarTool("@Edit_16x16.png", "Edit Item", new RoutedEventHandler(EditActivityGroup));
            xActivitiesGroupsRepositoryGrid.RowDoubleClick += grdActivitiesGroupsRepository_grdMain_MouseDoubleClick;
            xActivitiesGroupsRepositoryGrid.ItemDropped += grdActivitiesGroupsRepository_ItemDropped;
            xActivitiesGroupsRepositoryGrid.PreviewDragItem += grdActivitiesGroupsRepository_PreviewDragItem;           
            xActivitiesGroupsRepositoryGrid.ShowTagsFilter = Visibility.Visible;
        }

      
        private void AddFromRepository(object sender, RoutedEventArgs e)
        {
            if (mInTreeModeView == false)
            {
                if (xActivitiesGroupsRepositoryGrid.Grid.SelectedItems != null && xActivitiesGroupsRepositoryGrid.Grid.SelectedItems.Count > 0)
                {
                    if (mContext.BusinessFlow == null)
                    {
                        return;
                    }
                    List<ActivitiesGroup> list = new List<ActivitiesGroup>();
                    foreach (ActivitiesGroup selectedItem in xActivitiesGroupsRepositoryGrid.Grid.SelectedItems)
                    {                        
                        list.Add(selectedItem);                        
                    }
                    ActionsFactory.AddActivitiesGroupsFromSRHandler(list, mContext.BusinessFlow);
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
                }                    
            }
        }
        private void EditActivityGroup(object sender, RoutedEventArgs e)
        {
            if (xActivitiesGroupsRepositoryGrid.CurrentItem != null)
            {
                ActivitiesGroup activityGroup = (ActivitiesGroup)xActivitiesGroupsRepositoryGrid.CurrentItem;
                ActivitiesGroupPage mActivitiesGroupPage = new ActivitiesGroupPage(activityGroup, null, ActivitiesGroupPage.eEditMode.SharedRepository);
                mActivitiesGroupPage.ShowAsWindow();
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.AskToSelectItem);
            }
        }

        private void ViewRepositoryItemUsage(object sender, RoutedEventArgs e)
        {
            if (xActivitiesGroupsRepositoryGrid.Grid.SelectedItem != null)
            {
                RepositoryItemUsagePage usagePage = new RepositoryItemUsagePage((RepositoryItemBase)xActivitiesGroupsRepositoryGrid.Grid.SelectedItem);
                usagePage.ShowAsWindow();
            }
            else
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected); 
        }

        private void grdActivitiesGroupsRepository_PreviewDragItem(object sender, EventArgs e)
        {
            if (DragDrop2.DrgInfo.DataIsAssignableToType(typeof(ActivitiesGroup))
                || DragDrop2.DrgInfo.DataIsAssignableToType(typeof(CollectionViewGroup)))
            {
                // OK to drop
                DragDrop2.SetDragIcon(true);
            }
            else
            {
                // Do Not Drop
                DragDrop2.SetDragIcon(false);
            }
        }

        private void grdActivitiesGroupsRepository_ItemDropped(object sender, EventArgs e)
        {
            try
            {
                ActivitiesGroup dragedItem = null;

                if (((DragInfo)sender).Data is ActivitiesGroup)
                {
                    dragedItem = (ActivitiesGroup)((DragInfo)sender).Data;
                }
                else if (((DragInfo)sender).Data is CollectionViewGroup)
                {
                    dragedItem = mContext.BusinessFlow.ActivitiesGroups.Where(x=>x.Name == ((DragInfo)sender).Header).FirstOrDefault();
                }

                if (dragedItem != null)
                {
                    //add the Group and it Activities to repository                    
                    List<RepositoryItemBase> list = new List<RepositoryItemBase>();
                    list.Add(dragedItem);
                    foreach (ActivityIdentifiers activityIdnt in dragedItem.ActivitiesIdentifiers)
                    {
                        list.Add(activityIdnt.IdentifiedActivity);
                    }
                    (new SharedRepositoryOperations()).AddItemsToRepository(mContext, list);

                    //refresh and select the item
                    ActivitiesGroup dragedItemInGrid = ((IEnumerable<ActivitiesGroup>)xActivitiesGroupsRepositoryGrid.DataSourceList).Where(x => x.Guid == dragedItem.Guid).FirstOrDefault();
                    if (dragedItemInGrid != null)
                        xActivitiesGroupsRepositoryGrid.Grid.SelectedItem = dragedItemInGrid;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to drop " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " into Shared Repository", ex);
            }
        }

        private void grdActivitiesGroupsRepository_grdMain_MouseDoubleClick(object sender, EventArgs e)
        {
            EditActivityGroup(sender, new RoutedEventArgs());
        }
    }
}
