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
        readonly RepositoryFolder<Activity> mActivitiesFolder;
        bool TreeInitDone = false;
        bool mInTreeModeView = false;
        BusinessFlow mBusinessFlow;
        ObservableList<Guid> mTags = new ObservableList<Guid>();
        RoutedEventHandler mAddActivityHandler;

        public ActivitiesRepositoryPage(RepositoryFolder<Activity> activitiesFolder, BusinessFlow businessFlow=null,ObservableList<Guid> Tags=null, RoutedEventHandler AddActivityHandler = null)
        {          
            InitializeComponent();

            mActivitiesFolder = activitiesFolder;

            if (Tags != null)
            {
                mTags = Tags;
                xActivitiesRepositoryGrid.Tags = mTags;
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
            SetGridAndTreeData();
        }

        private void SetGridAndTreeData()
        {
            if (mActivitiesFolder.IsRootFolder)
            {
                xActivitiesRepositoryGrid.DataSourceList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
            }
            else
            {
                xActivitiesRepositoryGrid.DataSourceList = mActivitiesFolder.GetFolderItems();
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
            viewCols.Add(new GridColView() { Field = Activity.Fields.ActivityName, Header="Name", WidthWeight = 50, AllowSorting = true });
            viewCols.Add(new GridColView() { Field = Activity.Fields.Description, WidthWeight = 35, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = "Inst.", WidthWeight = 15, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["ViewInstancesButton"] });
            xActivitiesRepositoryGrid.SetAllColumnsDefaultView(view);           
            xActivitiesRepositoryGrid.InitViewItems();

            xActivitiesRepositoryGrid.btnRefresh.Visibility = Visibility.Collapsed;
            //grdActivitiesRepository.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGridActivities));                       
            xActivitiesRepositoryGrid.AddToolbarTool("@LeftArrow_16x16.png", "Add to Flow", new RoutedEventHandler(mAddActivityHandler));
            xActivitiesRepositoryGrid.AddToolbarTool("@Edit_16x16.png", "Edit Item", new RoutedEventHandler(EditActivity));
            
            xActivitiesRepositoryGrid.RowDoubleClick += grdActivitiesRepository_grdMain_MouseDoubleClick;
            xActivitiesRepositoryGrid.ItemDropped += grdActivitiesRepository_ItemDropped;
            xActivitiesRepositoryGrid.PreviewDragItem += grdActivitiesRepository_PreviewDragItem;                        
            xActivitiesRepositoryGrid.ShowTagsFilter = Visibility.Visible;
        }

        private void AddFromRepository(object sender, RoutedEventArgs e)
        {
            if (mInTreeModeView == false)
            {
                if (xActivitiesRepositoryGrid.Grid.SelectedItems != null && xActivitiesRepositoryGrid.Grid.SelectedItems.Count > 0)
                {
                    foreach (Activity selectedItem in xActivitiesRepositoryGrid.Grid.SelectedItems)
                    {
                        if (mBusinessFlow != null)
                        {
                            Activity instance = (Activity)selectedItem.CreateInstance(true);
                            instance.Active = true;
                            mBusinessFlow.SetActivityTargetApplication(instance);
                            mBusinessFlow.AddActivity(instance, true);
                        }
                    }
                }
                else
                    Reporter.ToUser(eUserMsgKeys.NoItemWasSelected);
            }
        }

        private void EditActivity(object sender, RoutedEventArgs e)
        {
            if (xActivitiesRepositoryGrid.CurrentItem != null)
            {
                Activity a = (Activity)xActivitiesRepositoryGrid.CurrentItem;
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
            if (xActivitiesRepositoryGrid.Grid.SelectedItem != null)
            {
                RepositoryItemUsagePage usagePage = new RepositoryItemUsagePage((RepositoryItemBase)xActivitiesRepositoryGrid.Grid.SelectedItem);
                usagePage.ShowAsWindow();
            }
            else
            {
                Reporter.ToUser(eUserMsgKeys.NoItemWasSelected);
            }
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
                SharedRepositoryOperations.AddItemToRepository(dragedItem);

                //refresh and select the item
                try
                {
                    Activity dragedItemInGrid = ((IEnumerable<Activity>)xActivitiesRepositoryGrid.DataSourceList).Where(x => x.Guid == dragedItem.Guid).FirstOrDefault();
                    if (dragedItemInGrid != null)
                        xActivitiesRepositoryGrid.Grid.SelectedItem = dragedItemInGrid;
                }
                catch(Exception ex){ Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}"); }
            }
        }

        private void grdActivitiesRepository_grdMain_MouseDoubleClick(object sender, EventArgs e)
        {
            EditActivity(sender, new RoutedEventArgs());
        }
    }
}
