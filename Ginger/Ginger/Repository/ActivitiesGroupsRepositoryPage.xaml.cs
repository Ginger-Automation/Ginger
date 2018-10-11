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
using Ginger.Activities;
using GingerWPF.DragDropLib;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Ginger.BusinessFlowWindows;
using Ginger.BusinessFlowFolder;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;

namespace Ginger.Repository
{
    /// <summary>
    /// Interaction logic for ActivitiesGroupsRepositoryPage.xaml
    /// </summary>
    public partial class ActivitiesGroupsRepositoryPage : Page
    {
        readonly RepositoryFolder<ActivitiesGroup> mActivitiesGroupFolder;
        BusinessFlow mBusinessFlow;

        bool TreeInitDone = false;
        bool mInTreeModeView = false;

        

        public ActivitiesGroupsRepositoryPage(RepositoryFolder<ActivitiesGroup> activitiesGroupFolder,  BusinessFlow businessFlow = null)
        {
            InitializeComponent();

            mActivitiesGroupFolder = activitiesGroupFolder;
            if (businessFlow != null)
                mBusinessFlow = businessFlow;
            else
            {
                mBusinessFlow = App.BusinessFlow;
                App.PropertyChanged += AppPropertychanged;
            }
            
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

        private void AppPropertychanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BusinessFlow")
            {
                if (App.BusinessFlow != mBusinessFlow)
                {
                    mBusinessFlow = App.BusinessFlow;
                }
            }
        }

        private void SetActivitiesRepositoryGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            view.GridColsView = viewCols;

            viewCols.Add(new GridColView() { Field = ActivitiesGroup.Fields.Name, WidthWeight = 50, AllowSorting = true });
            viewCols.Add(new GridColView() { Field = ActivitiesGroup.Fields.Description, WidthWeight = 35, AllowSorting = true });
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
                    if (mBusinessFlow == null)
                    {
                        return;
                    }

                    foreach (ActivitiesGroup selectedItem in xActivitiesGroupsRepositoryGrid.Grid.SelectedItems)
                    {                       
                        ActivitiesGroup droppedGroupIns = (ActivitiesGroup)selectedItem.CreateInstance(true);
                        mBusinessFlow.AddActivitiesGroup(droppedGroupIns);
                        ObservableList<Activity> activities = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
                        mBusinessFlow.ImportActivitiesGroupActivitiesFromRepository(droppedGroupIns, activities, false);
                        
                        int selectedActIndex = -1;
                        ObservableList<ActivitiesGroup> actsList = App.BusinessFlow.ActivitiesGroups;
                        if (actsList.CurrentItem != null)
                        {
                            selectedActIndex = actsList.IndexOf((ActivitiesGroup)actsList.CurrentItem);
                        }
                        if (selectedActIndex >= 0)
                        {
                            actsList.Move(actsList.Count - 1, selectedActIndex + 1);

                        }
                    }
                    mBusinessFlow.AttachActivitiesGroupsAndActivities();
                }
                else
                {
                    Reporter.ToUser(eUserMsgKeys.NoItemWasSelected);
                }                    
            }
        }
        private void EditActivityGroup(object sender, RoutedEventArgs e)
        {
            if (xActivitiesGroupsRepositoryGrid.CurrentItem != null)
            {
                ActivitiesGroup activityGroup = (ActivitiesGroup)xActivitiesGroupsRepositoryGrid.CurrentItem;
                ActivitiesGroupPage mActivitiesGroupPage = new ActivitiesGroupPage(activityGroup,ActivitiesGroupPage.eEditMode.SharedRepository);
                mActivitiesGroupPage.ShowAsWindow();
            }
            else
            {
                Reporter.ToUser(eUserMsgKeys.AskToSelectItem);
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
                Reporter.ToUser(eUserMsgKeys.NoItemWasSelected); 
        }

        private void grdActivitiesGroupsRepository_PreviewDragItem(object sender, EventArgs e)
        {
            if (DragDrop2.DragInfo.DataIsAssignableToType(typeof(ActivitiesGroup)))
            {
                // OK to drop                         
                DragDrop2.DragInfo.DragIcon = DragInfo.eDragIcon.Copy;
            }
        }

        private void grdActivitiesGroupsRepository_ItemDropped(object sender, EventArgs e)
        {
            ActivitiesGroup dragedItem = (ActivitiesGroup)((DragInfo)sender).Data;
            if (dragedItem != null)
            {
                //add the Group to repository
                // App.LocalRepository.AddItemToRepositoryWithPreChecks(dragedItem, mBusinessFlow);
                SharedRepositoryOperations.AddItemToRepository(dragedItem);
                //refresh and select the item
                try
                {
                    ActivitiesGroup dragedItemInGrid = ((IEnumerable<ActivitiesGroup>)xActivitiesGroupsRepositoryGrid.DataSourceList).Where(x => x.Guid == dragedItem.Guid).FirstOrDefault();
                    if (dragedItemInGrid != null)
                        xActivitiesGroupsRepositoryGrid.Grid.SelectedItem = dragedItemInGrid;
                }
                catch(Exception ex){ Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}"); }
            }
        }

        private void grdActivitiesGroupsRepository_grdMain_MouseDoubleClick(object sender, EventArgs e)
        {
            EditActivityGroup(sender, new RoutedEventArgs());
        }
    }
}
