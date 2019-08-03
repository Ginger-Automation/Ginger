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
using Ginger.BusinessFlowPages.ListHelpers;
using Ginger.Repository;
using Ginger.UserControlsLib.UCListView;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerWPF.DragDropLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Ginger.BusinessFlowPages
{
    /// <summary>
    /// Interaction logic for NewActivitiesPage.xaml
    /// </summary>
    public partial class ActivitiesListViewPage : Page
    {
        BusinessFlow mBusinessFlow;
        Context mContext;
        Ginger.General.eRIPageViewMode mPageViewMode;

        public UcListView ListView
        {
            get { return xActivitiesListView; }
        }

        public ActivitiesListViewPage(BusinessFlow businessFlow, Context context, Ginger.General.eRIPageViewMode pageViewMode)
        {
            InitializeComponent();

            mBusinessFlow = businessFlow;
            mContext = context;
            mPageViewMode = pageViewMode;

            SetListView();
            SetSharedRepositoryMark();
        }

        /// <summary>
        ///             CanContentScroll = true ===> Scrolling Mode = Items and it supports Virtugalization
        ///             CanContentScroll = false ===> Scrolling Mode = Pixels but disables Virtualization
        ///             As we're grouping the Activities based on Activity Groups, scrolling is thus effected and ScrollingMode being Items produces messy scrolling experience
        ///             Thus, we're disabling the ListView's ScrollViewer.CanContentScroll property as false for smooth scrolling
        /// </summary>
        private void SetListView()
        {
            //List Title
            xActivitiesListView.Title = GingerDicser.GetTermResValue(eTermResKey.Activities);
            xActivitiesListView.ListImageType = Amdocs.Ginger.Common.Enums.eImageType.Activity;

            //List Items
            ActivitiesListViewHelper activityListItemInfo = new ActivitiesListViewHelper(mContext, mPageViewMode);
            activityListItemInfo.ActivityListItemEvent += ActivityListItemInfo_ActivityListItemEvent;
            xActivitiesListView.SetDefaultListDataTemplate(activityListItemInfo);

            //List Data
            xActivitiesListView.DataSourceList = mBusinessFlow.Activities;

            //List Grouping
            xActivitiesListView.AddGrouping(nameof(Activity.ActivitiesGroupID));

            xActivitiesListView.PreviewDragItem += ActivitiesListView_PreviewDragItem;
            xActivitiesListView.ItemDropped += ActivitiesListView_ItemDropped;
            xActivitiesListView.SameFrameItemDropped += ActivitiesListView_SameFrameItemDropped;

            // Disable ScrollViewer's CanContentScroll property for smooth scrolling 
            xActivitiesListView.xListView.SetValue(ScrollViewer.CanContentScrollProperty, false);
        }

        private void ActivitiesListView_SameFrameItemDropped(object sender, EventArgs e)
        {
            object droppedItem = ((DragInfo)sender).Data as object;
            if (droppedItem != null)
            {
                if (droppedItem is Activity)
                {
                    Activity draggedActivity = droppedItem as Activity;
                    Activity activityDroppedOn = DragDrop2.GetRepositoryItemHit(ListView) as Activity;

                    if (activityDroppedOn != null)
                    {
                        if (activityDroppedOn.ActivitiesGroupID != draggedActivity.ActivitiesGroupID)
                        {
                            draggedActivity.ActivitiesGroupID = activityDroppedOn.ActivitiesGroupID;
                            ListView.UpdateGrouping();
                        }
                        else
                        {
                            DragDrop2.ShuffleControlsItems(draggedActivity, activityDroppedOn, ListView);
                        }
                    }
                }
            }
        }

        private void ActivitiesListView_PreviewDragItem(object sender, EventArgs e)
        {
            if (DragDrop2.DragInfo.DataIsAssignableToType(typeof(Activity))
                || DragDrop2.DragInfo.DataIsAssignableToType(typeof(ActivitiesGroup)))
            {
                // OK to drop
                if ((sender as DragInfo).DragSource == (sender as DragInfo).DragTarget)
                {
                    DragDrop2.DragInfo.DragIcon = GingerWPF.DragDropLib.DragInfo.eDragIcon.Move;
                }
                else
                {
                    DragDrop2.DragInfo.DragIcon = GingerWPF.DragDropLib.DragInfo.eDragIcon.Copy;
                }
            }
            else
            {
                DragDrop2.DragInfo.DragIcon = GingerWPF.DragDropLib.DragInfo.eDragIcon.DoNotDrop;
            }
        }

        private void ActivitiesListView_ItemDropped(object sender, EventArgs e)
        {
            object droppedItem = ((DragInfo)sender).Data as object;
            if (droppedItem != null)
            {
                if (droppedItem is Activity)
                {
                    string activityGroupID = null;

                    Activity activityDroppedOn = DragDrop2.GetRepositoryItemHit(ListView) as Activity;

                    if (activityDroppedOn != null)
                    {
                        activityGroupID = activityDroppedOn.ActivitiesGroupID;
                    }

                    List<Activity> list = new List<Activity>();
                    list.Add((Activity)droppedItem);
                    ActionsFactory.AddActivitiesFromSRHandler(list, mContext.BusinessFlow, activityGroupID);
                }
                else if (droppedItem is ActivitiesGroup)
                {
                    List<ActivitiesGroup> list = new List<ActivitiesGroup>();
                    list.Add((ActivitiesGroup)droppedItem);
                    ActionsFactory.AddActivitiesGroupsFromSRHandler(list, mContext.BusinessFlow);
                }
            }
        }

        private void ActivityListItemInfo_ActivityListItemEvent(ActivityListItemEventArgs EventArgs)
        {
            switch (EventArgs.EventType)
            {
                case ActivityListItemEventArgs.eEventType.UpdateGrouping:
                    xActivitiesListView.UpdateGrouping();
                    break;
            }
        }


        public void UpdateBusinessFlow(BusinessFlow updateBusinessFlow)
        {
            mBusinessFlow = updateBusinessFlow;
            mContext.BusinessFlow = mBusinessFlow;
            if (mBusinessFlow != null)
            {
                xActivitiesListView.DataSourceList = mBusinessFlow.Activities;
                xActivitiesListView.AddGrouping(nameof(Activity.ActivitiesGroupID));
                SetSharedRepositoryMark();
            }
            else
            {
                xActivitiesListView.DataSourceList = null;
            }
        }

        private void SetSharedRepositoryMark()
        {
            ObservableList<Activity> srActivities = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
            SharedRepositoryOperations.MarkSharedRepositoryItems((IEnumerable<object>)mBusinessFlow.Activities, (IEnumerable<object>)srActivities);
            ObservableList<ActivitiesGroup> sharedActivitiesGroups = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
            SharedRepositoryOperations.MarkSharedRepositoryItems((IEnumerable<object>)mBusinessFlow.ActivitiesGroups, (IEnumerable<object>)sharedActivitiesGroups);
        }
    }
}
