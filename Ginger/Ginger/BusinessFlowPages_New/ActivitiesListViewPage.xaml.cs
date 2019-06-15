using Amdocs.Ginger.Common;
using Ginger.Activities;
using Ginger.BusinessFlowPages.ListViewItems;
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
        }
        
        private void SetListView()
        {
            //List Title
            xActivitiesListView.Title = GingerDicser.GetTermResValue(eTermResKey.Activities);
            xActivitiesListView.ListImageType = Amdocs.Ginger.Common.Enums.eImageType.Activity;

            //List Items
            ActivityListItemInfo activityListItemInfo = new ActivityListItemInfo(mContext);
            activityListItemInfo.ActivityListItemEvent += ActivityListItemInfo_ActivityListItemEvent;
            xActivitiesListView.SetDefaultListDataTemplate(activityListItemInfo);

            //List tools bar
            xActivitiesListView.AddBtnVisiblity = Visibility.Collapsed;

            List<ListItemOperation> operationsListToAdd = new List<ListItemOperation>();
            ListItemOperation groupsManager = new ListItemOperation();
            groupsManager.ImageType = Amdocs.Ginger.Common.Enums.eImageType.ActivitiesGroup;
            groupsManager.ImageSize = 17;
            groupsManager.ToolTip = GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups, prefixString: "Open", suffixString: "Manager");
            groupsManager.OperationHandler = OpenGroupsManagerHandler;
            operationsListToAdd.Add(groupsManager);
            xActivitiesListView.AddListOperations(operationsListToAdd);

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
            }
            else
            {
                xActivitiesListView.DataSourceList = null;
            }
        }
    }
}
