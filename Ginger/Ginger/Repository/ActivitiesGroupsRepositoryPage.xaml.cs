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

namespace Ginger.Repository
{
    /// <summary>
    /// Interaction logic for ActivitiesGroupsRepositoryPage.xaml
    /// </summary>
    public partial class ActivitiesGroupsRepositoryPage : Page
    {
        bool TreeInitDone = false;
        bool mInTreeModeView = false;

        BusinessFlow mBusinessFlow;

        public ActivitiesGroupsRepositoryPage(string Folder, BusinessFlow businessFlow = null)
        {
            InitializeComponent();

            if (businessFlow != null)
                mBusinessFlow = businessFlow;
            else
            {
                mBusinessFlow = App.BusinessFlow;
                App.PropertyChanged += AppPropertychanged;
            }
            
            SetActivitiesRepositoryGridView();
            SetActivitiesRepositoryTreeView();

            App.LocalRepository.AttachHandlerToSolutionRepoActivitiesGroupsChange(RefreshGridActivitiesGroups);         
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
            grdActivitiesGroupsRepository.SetAllColumnsDefaultView(view);
            grdActivitiesGroupsRepository.InitViewItems();
            
            grdActivitiesGroupsRepository.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGridActivitiesGroups));                       
            grdActivitiesGroupsRepository.AddToolbarTool("@LeftArrow_16x16.png", "Add to Flow", new RoutedEventHandler(AddFromRepository));
            grdActivitiesGroupsRepository.AddToolbarTool("@Edit_16x16.png", "Edit Item", new RoutedEventHandler(EditActivityGroup));
            grdActivitiesGroupsRepository.RowDoubleClick += grdActivitiesGroupsRepository_grdMain_MouseDoubleClick;
            grdActivitiesGroupsRepository.ItemDropped += grdActivitiesGroupsRepository_ItemDropped;
            grdActivitiesGroupsRepository.PreviewDragItem += grdActivitiesGroupsRepository_PreviewDragItem;
            grdActivitiesGroupsRepository.DataSourceList = App.LocalRepository.GetSolutionRepoActivitiesGroups();
            grdActivitiesGroupsRepository.ShowTagsFilter = Visibility.Visible;
        }

        private void SetActivitiesRepositoryTreeView()
        {
            treeActivitiesGroupsRepository.TreeTitle = GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " Repository";
            treeActivitiesGroupsRepository.TreeTitleStyle = (Style)TryFindResource("@ucTitleStyle_3");
        }

        private void ShowTreeView()
        {
            //if not done..
            if (!TreeInitDone)
            {
                treeActivitiesGroupsRepository.TreeTitle = GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " Repository";
                treeActivitiesGroupsRepository.TreeTitleStyle = (Style)TryFindResource("@ucTitleStyle_3");
                //Add Activities
                SharedActivitiesGroupsFolderTreeItem SAGFTI = new SharedActivitiesGroupsFolderTreeItem(SharedActivitiesGroupsFolderTreeItem.eActivitiesGroupsItemsShowMode.ReadOnly);
                SAGFTI.Folder = GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups);
                SAGFTI.Path = App.UserProfile.Solution.Folder + @"\SharedRepository\ActivitiesGroups\";
                treeActivitiesGroupsRepository.Tree.AddItem(SAGFTI);

                TreeInitDone = true;
            }
        }    

        private void RefreshGridActivitiesGroups(object sender, RoutedEventArgs e)
        {
            App.LocalRepository.RefreshSolutionRepoActivitiesGroups();
            grdActivitiesGroupsRepository.DataSourceList = App.LocalRepository.GetSolutionRepoActivitiesGroups();
        }

        private void RefreshGridActivitiesGroups(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            grdActivitiesGroupsRepository.DataSourceList = App.LocalRepository.GetSolutionRepoActivitiesGroups();
        }

        private void AddFromRepository(object sender, RoutedEventArgs e)
        {
            if (mInTreeModeView == false)
            {
                if (grdActivitiesGroupsRepository.Grid.SelectedItems != null && grdActivitiesGroupsRepository.Grid.SelectedItems.Count > 0)
                {
                    if (mBusinessFlow == null) return;
                    foreach (ActivitiesGroup selectedItem in grdActivitiesGroupsRepository.Grid.SelectedItems)
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
                    Reporter.ToUser(eUserMsgKeys.NoItemWasSelected);
            }
        }
        private void EditActivityGroup(object sender, RoutedEventArgs e)
        {
            if (grdActivitiesGroupsRepository.CurrentItem != null)
            {
                ActivitiesGroup activityGroup = (ActivitiesGroup)grdActivitiesGroupsRepository.CurrentItem;
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
            if (grdActivitiesGroupsRepository.Grid.SelectedItem != null)
            {
                RepositoryItemUsagePage usagePage = new RepositoryItemUsagePage((RepositoryItem)grdActivitiesGroupsRepository.Grid.SelectedItem);
                usagePage.ShowAsWindow();
            }
            else
                Reporter.ToUser(eUserMsgKeys.NoItemWasSelected); 
        }

        private void GridTreeViewButton_Click(object sender, RoutedEventArgs e)
        {
            Image image = new Image();

            if (((Button)sender).ToolTip.ToString().Contains("Tree") == true)
            {
                //switch to tree view
                treeActivitiesGroupsRepository.Visibility = System.Windows.Visibility.Visible;
                grdActivitiesGroupsRepository.Visibility = System.Windows.Visibility.Collapsed;
                ShowTreeView();

                image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@Grid_24x24.png"));
                GridTreeViewButton.Content = image;
                GridTreeViewButton.ToolTip = "Switch to Grid View";

                mInTreeModeView = true;
            }
            else
            {
                //switch to grid view
                treeActivitiesGroupsRepository.Visibility = System.Windows.Visibility.Collapsed;
                grdActivitiesGroupsRepository.Visibility = System.Windows.Visibility.Visible;

                image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@TreeView_24x24.png"));
                GridTreeViewButton.Content = image;
                GridTreeViewButton.ToolTip = "Switch to Tree View";

                mInTreeModeView = false;
            }
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
                    ActivitiesGroup dragedItemInGrid = ((IEnumerable<ActivitiesGroup>)grdActivitiesGroupsRepository.DataSourceList).Where(x => x.Guid == dragedItem.Guid).FirstOrDefault();
                    if (dragedItemInGrid != null)
                        grdActivitiesGroupsRepository.Grid.SelectedItem = dragedItemInGrid;
                }
                catch(Exception ex){ Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}"); }
            }
        }

        private void grdActivitiesGroupsRepository_grdMain_MouseDoubleClick(object sender, EventArgs e)
        {
            EditActivityGroup(sender, new RoutedEventArgs());
        }
    }
}
