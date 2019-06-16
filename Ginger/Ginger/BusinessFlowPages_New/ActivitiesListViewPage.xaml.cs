using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger.Activities;
using Ginger.BusinessFlowPages.ListHelpers;
using Ginger.Repository;
using Ginger.UserControlsLib.UCListView;
using GingerCore;
using System.Collections.Generic;
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

        public UcListView ListView
        {
            get { return xActivitiesListView; }
        }

        public ActivitiesListViewPage(BusinessFlow businessFlow, Context context)
        {
            InitializeComponent();

            mBusinessFlow = businessFlow;
            mContext = context;

            SetListView();
            SetSharedRepositoryMark();
        }
        
        private void SetListView()
        {
            //List Title
            xActivitiesListView.Title = GingerDicser.GetTermResValue(eTermResKey.Activities);
            xActivitiesListView.ListImageType = Amdocs.Ginger.Common.Enums.eImageType.Activity;

            //List Items
            ActivitiesListHelper activityListItemInfo = new ActivitiesListHelper(mContext);
            activityListItemInfo.ActivityListItemEvent += ActivityListItemInfo_ActivityListItemEvent;
            xActivitiesListView.SetDefaultListDataTemplate(activityListItemInfo);

            //List Data
            xActivitiesListView.DataSourceList = mBusinessFlow.Activities;

            //List Grouping
            xActivitiesListView.AddGrouping(nameof(Activity.ActivitiesGroupID));
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

        private void OpenGroupsManagerHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroupsManagerPage activitiesGroupsManagerPage = new ActivitiesGroupsManagerPage(mBusinessFlow);
            activitiesGroupsManagerPage.ShowAsWindow();
            xActivitiesListView.UpdateGrouping();
        }

        public void UpdateBusinessFlow(BusinessFlow updateBusinessFlow)
        {
            mBusinessFlow = updateBusinessFlow;
            mContext.BusinessFlow = mBusinessFlow;
            if (mBusinessFlow != null)
            {
                xActivitiesListView.DataSourceList = mBusinessFlow.Activities;
                xActivitiesListView.UpdateGrouping();
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
