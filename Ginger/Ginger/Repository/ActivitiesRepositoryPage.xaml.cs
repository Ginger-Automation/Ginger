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
using GingerWPF.DragDropLib;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.UserControls;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore;
using GingerCore.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;

namespace Ginger.Repository
{
    /// <summary>
    /// Interaction logic for ActivitiesRepositoryPage.xaml
    /// </summary>
    public partial class ActivitiesRepositoryPage : Page
    {
        bool TreeInitDone = false;
        bool mInTreeModeView = false;
        BusinessFlow mBusinessFlow;
        ObservableList<Guid> mTags = new ObservableList<Guid>();
        RoutedEventHandler mAddActivityHandler;

        public ActivitiesRepositoryPage(string Folder, BusinessFlow businessFlow=null,ObservableList<Guid> Tags=null, RoutedEventHandler AddActivityHandler = null)
        {          

            InitializeComponent();            
            if (Tags != null)
            {
                mTags = Tags;
                grdActivitiesRepository.Tags = mTags;
            }

            if (AddActivityHandler != null)
                mAddActivityHandler = AddActivityHandler;
            else
                mAddActivityHandler = AddFromRepository;

            if (businessFlow != null)
                mBusinessFlow = businessFlow;
            else
            {
                mBusinessFlow = App.BusinessFlow;
                App.PropertyChanged += AppPropertychanged;
            }

            SetActivitiesRepositoryGridView();
            SetActivitiesRepositoryTreeView();            
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
            viewCols.Add(new GridColView() { Field = Activity.Fields.ActivityName, Header="Name", WidthWeight = 50, AllowSorting = true });
            viewCols.Add(new GridColView() { Field = Activity.Fields.Description, WidthWeight = 35, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = "Inst.", WidthWeight = 15, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["ViewInstancesButton"] });
            grdActivitiesRepository.SetAllColumnsDefaultView(view);           
            grdActivitiesRepository.InitViewItems();

            grdActivitiesRepository.btnRefresh.Visibility = Visibility.Collapsed;
            //grdActivitiesRepository.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGridActivities));                       
            grdActivitiesRepository.AddToolbarTool("@LeftArrow_16x16.png", "Add to Flow", new RoutedEventHandler(mAddActivityHandler));
            grdActivitiesRepository.AddToolbarTool("@Edit_16x16.png", "Edit Item", new RoutedEventHandler(EditActivity));
            
            grdActivitiesRepository.RowDoubleClick += grdActivitiesRepository_grdMain_MouseDoubleClick;
            grdActivitiesRepository.ItemDropped += grdActivitiesRepository_ItemDropped;
            grdActivitiesRepository.PreviewDragItem += grdActivitiesRepository_PreviewDragItem;            

            grdActivitiesRepository.DataSourceList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
            grdActivitiesRepository.ShowTagsFilter = Visibility.Visible;
        }

        private void SetActivitiesRepositoryTreeView()
        {
            ActivitiesRepositoryTreeView.TreeTitle = GingerDicser.GetTermResValue(eTermResKey.Activities) + " Repository";
            ActivitiesRepositoryTreeView.TreeTitleStyle = (Style)TryFindResource("@ucTitleStyle_3");

            ActivitiesRepositoryTreeView.Tree.ItemSelected += ActivityTreeView_ItemSelected;
        }
        private void ShowTreeView()
        {            
            //if not done..
            if (!TreeInitDone)
            {
                ActivitiesRepositoryTreeView.TreeTitle = GingerDicser.GetTermResValue(eTermResKey.Activities) + " Repository";
                ActivitiesRepositoryTreeView.TreeTitleStyle = (Style)TryFindResource("@ucTitleStyle_3");
                //Add Activities
                RepositoryFolder<Activity> repositoryFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Activity>();

                SharedActivitiesFolderTreeItem SAFTI = new SharedActivitiesFolderTreeItem(repositoryFolder, SharedActivitiesFolderTreeItem.eActivitiesItemsShowMode.ReadOnly);
                SAFTI.Folder = GingerDicser.GetTermResValue(eTermResKey.Activities);
                SAFTI.Path = App.UserProfile.Solution.Folder + @"\SharedRepository\Activities\";
                ActivitiesRepositoryTreeView.Tree.AddItem(SAFTI);

                TreeInitDone = true;
            }
        }      
        

        private void RefreshGridActivities(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            grdActivitiesRepository.DataSourceList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
        }

        private void AddFromRepository(object sender, RoutedEventArgs e)
        {
            if (mInTreeModeView == false)
            {
                if (grdActivitiesRepository.Grid.SelectedItems != null && grdActivitiesRepository.Grid.SelectedItems.Count > 0)
                {
                    foreach (Activity selectedItem in grdActivitiesRepository.Grid.SelectedItems)
                        if (mBusinessFlow != null)
                        {
                            Activity instance = (Activity)selectedItem.CreateInstance(true);
                            instance.Active = true;
                            mBusinessFlow.SetActivityTargetApplication(instance);
                            mBusinessFlow.AddActivity(instance,true);
                        }
                }
                else
                    Reporter.ToUser(eUserMsgKeys.NoItemWasSelected);
            }
        }

        private void EditActivity(object sender, RoutedEventArgs e)
        {
            if (grdActivitiesRepository.CurrentItem != null)
            {
                Activity a = (Activity)grdActivitiesRepository.CurrentItem;
                BusinessFlowWindows.ActivityEditPage w = new BusinessFlowWindows.ActivityEditPage(a, General.RepositoryItemPageViewMode.SharedReposiotry);
                w.ShowAsWindow();
            }
            else
            {
                Reporter.ToUser(eUserMsgKeys.AskToSelectItem);
            }
        }

        private void ViewRepositoryItemUsage(object sender, RoutedEventArgs e)
        {
            if (grdActivitiesRepository.Grid.SelectedItem != null)
            {
                RepositoryItemUsagePage usagePage = new RepositoryItemUsagePage((RepositoryItem)grdActivitiesRepository.Grid.SelectedItem);
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
                ActivitiesRepositoryTreeView.Visibility = System.Windows.Visibility.Visible;
                grdActivitiesRepository.Visibility = System.Windows.Visibility.Collapsed;
                ShowTreeView();

                image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@Grid_24x24.png"));
                GridTreeViewButton.Content = image;
                GridTreeViewButton.ToolTip = "Switch to Grid View";

                mInTreeModeView = true;
            }
            else
            {
                //switch to grid view
                ActivitiesRepositoryTreeView.Visibility = System.Windows.Visibility.Collapsed;
                grdActivitiesRepository.Visibility = System.Windows.Visibility.Visible;

                image.Source = new BitmapImage(new Uri("pack://application:,,,/Ginger;component/Images/" + "@TreeView_24x24.png"));
                GridTreeViewButton.Content = image;
                GridTreeViewButton.ToolTip = "Switch to Tree View";

                mInTreeModeView = false;
            }
        }

        private void ActivityTreeView_ItemSelected(object sender, EventArgs e)
        {
            // update user selection
            TreeViewItem i = (TreeViewItem)sender;
            if (i != null)
            {
                ITreeViewItem iv = (ITreeViewItem)i.Tag;
                if (iv.NodeObject() != null)
                {
                }
            }
        }

        private void treeViewTree_MouseMove(object sender, MouseEventArgs e)
        {
        }

        private void ActivitiesRepositoryTreeView_Drop(object sender, DragEventArgs e)
        {
        }

        private void grdActivitiesRepository_PreviewDragItem(object sender, EventArgs e)
        {
            if (DragDrop2.DragInfo.DataIsAssignableToType(typeof(Activity)))
            {
                // OK to drop                         
                DragDrop2.DragInfo.DragIcon = GingerWPF.DragDropLib.DragInfo.eDragIcon.Copy;
            }
        }

        private void grdActivitiesRepository_ItemDropped(object sender, EventArgs e)
        {
            Activity dragedItem = (Activity)((DragInfo)sender).Data;
            if (dragedItem != null)
            {
                ////check if the Activity is part of a group which not exist in ActivitiesGroups repository
                // App.LocalRepository.AddItemToRepositoryWithPreChecks(dragedItem, mBusinessFlow);
                SharedRepositoryOperations.AddItemToRepository(dragedItem);

                //refresh and select the item
                try
                {
                    Activity dragedItemInGrid = ((IEnumerable<Activity>)grdActivitiesRepository.DataSourceList).Where(x => x.Guid == dragedItem.Guid).FirstOrDefault();
                    if (dragedItemInGrid != null)
                        grdActivitiesRepository.Grid.SelectedItem = dragedItemInGrid;
                }
                catch(Exception ex){ Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}"); }
            }
        }

        private void grdActivitiesRepository_grdMain_MouseDoubleClick(object sender, EventArgs e)
        {
            EditActivity(sender, new RoutedEventArgs());
        }
    }
}
