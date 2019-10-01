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
using Ginger.BusinessFlowPages;
using Ginger.UserControls;
using GingerCore;
using GingerWPF.DragDropLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Repository
{
    /// <summary>
    /// Interaction logic for ActivitiesRepositoryPage.xaml
    /// </summary>
    public partial class ActivitiesRepositoryPage : Page
    {
        readonly RepositoryFolder<Activity> mActivitiesFolder;        
        bool mInTreeModeView = false;
        ObservableList<Guid> mTags = new ObservableList<Guid>();
        RoutedEventHandler mAddActivityHandler;
        Context mContext;

        public ActivitiesRepositoryPage(RepositoryFolder<Activity> activitiesFolder, Context context, ObservableList<Guid> Tags=null, RoutedEventHandler AddActivityHandler = null)
        {          
            InitializeComponent();

            mActivitiesFolder = activitiesFolder;
            mContext = context;
            if (Tags != null)
            {
                mTags = Tags;
                xActivitiesRepositoryGrid.Tags = mTags;
            }

            if (AddActivityHandler != null)
                mAddActivityHandler = AddActivityHandler;
            else
                mAddActivityHandler = AddFromRepository;           

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

        public void UpdateBusinessFlow(BusinessFlow bf)
        {
            xActivitiesRepositoryGrid.ClearFilters();
        }

        private void SetActivitiesRepositoryGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            view.GridColsView = viewCols;
            viewCols.Add(new GridColView() { Field = nameof(Activity.ActivityName), Header="Name", WidthWeight = 50, AllowSorting = true });
            viewCols.Add(new GridColView() { Field = nameof(Activity.Description), WidthWeight = 35, AllowSorting = true });
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
                    if (mContext.BusinessFlow != null)
                    {
                        List<Activity> list = new List<Activity>();
                        foreach (Activity selectedItem in xActivitiesRepositoryGrid.Grid.SelectedItems)
                        {
                            list.Add(selectedItem);
                        }
                        ActionsFactory.AddActivitiesFromSRHandler(list, mContext.BusinessFlow);
                    }
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
                }
            }
        }

        private void EditActivity(object sender, RoutedEventArgs e)
        {
            if (xActivitiesRepositoryGrid.CurrentItem != null)
            {
                Activity a = (Activity)xActivitiesRepositoryGrid.CurrentItem;
                GingerWPF.BusinessFlowsLib.ActivityPage w = new GingerWPF.BusinessFlowsLib.ActivityPage(a, new Context() { Activity = a }, General.eRIPageViewMode.SharedReposiotry);
                w.ShowAsWindow();
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.AskToSelectItem);
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
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            }
        }

        private void grdActivitiesRepository_PreviewDragItem(object sender, EventArgs e)
        {
            if (DragDrop2.DrgInfo.DataIsAssignableToType(typeof(Activity)))
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

        private void grdActivitiesRepository_ItemDropped(object sender, EventArgs e)
        {
            Activity dragedItem = (Activity)((DragInfo)sender).Data;
            if (dragedItem != null)
            {
                ////check if the Activity is part of a group which not exist in ActivitiesGroups repository                
                (new SharedRepositoryOperations()).AddItemToRepository(mContext, dragedItem);

                //refresh and select the item
                try
                {
                    Activity dragedItemInGrid = ((IEnumerable<Activity>)xActivitiesRepositoryGrid.DataSourceList).Where(x => x.Guid == dragedItem.Guid).FirstOrDefault();
                    if (dragedItemInGrid != null)
                        xActivitiesRepositoryGrid.Grid.SelectedItem = dragedItemInGrid;
                }
                catch(Exception ex){ Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }
            }
        }

        private void grdActivitiesRepository_grdMain_MouseDoubleClick(object sender, EventArgs e)
        {
            EditActivity(sender, new RoutedEventArgs());
        }
    }
}
