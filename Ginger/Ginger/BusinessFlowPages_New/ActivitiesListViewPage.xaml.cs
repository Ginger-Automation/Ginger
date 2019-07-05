using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger.Activities;
using Ginger.BusinessFlowPages.ListHelpers;
using Ginger.Repository;
using Ginger.UserControlsLib.UCListView;
using GingerCore;
using GingerCore.Activities;
using GingerWPF.DragDropLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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
        }

        private void ActivitiesListView_PreviewDragItem(object sender, EventArgs e)
        {
            if (DragDrop2.DragInfo.DataIsAssignableToType(typeof(Activity))
                || DragDrop2.DragInfo.DataIsAssignableToType(typeof(ActivitiesGroup)))
            {
                // OK to drop                         
                DragDrop2.DragInfo.DragIcon = GingerWPF.DragDropLib.DragInfo.eDragIcon.Copy;
            }
        }

        private void ActivitiesListView_ItemDropped(object sender, EventArgs e)
        {
            object droppedItem = ((DragInfo)sender).Data as object;
            if (droppedItem != null)
            {                
                if (droppedItem is Activity)
                {
                    Activity droppedActivityIns = (Activity)((Activity)droppedItem).CreateInstance(true);
                    ActivitiesGroup parentGroup = null;
                    droppedActivityIns.Active = true;
                    if (mBusinessFlow.CurrentActivity != null)
                    {                        
                        parentGroup = mBusinessFlow.ActivitiesGroups.Where(x => x.Name == mBusinessFlow.CurrentActivity.ActivitiesGroupID).FirstOrDefault();
                    }
                    else
                    {                        
                        parentGroup = mBusinessFlow.AddActivitiesGroup();
                    }
                    mBusinessFlow.SetActivityTargetApplication(droppedActivityIns);
                    mBusinessFlow.AddActivity(droppedActivityIns, parentGroup);
                    mBusinessFlow.CurrentActivity = droppedActivityIns;
                }
                else if (droppedItem is ActivitiesGroup)
                {
                    ActivitiesGroup droppedGroupIns = (ActivitiesGroup)((ActivitiesGroup)droppedItem).CreateInstance(true);
                    mBusinessFlow.AddActivitiesGroup(droppedGroupIns);
                    ObservableList<Activity> activities = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
                    mBusinessFlow.ImportActivitiesGroupActivitiesFromRepository(droppedGroupIns, activities, false);
                    mBusinessFlow.AttachActivitiesGroupsAndActivities();
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
        }
    }
}
