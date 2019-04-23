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

        public ActivitiesListViewPage(BusinessFlow businessFlow, Context context)
        {
            InitializeComponent();

            mBusinessFlow = businessFlow;
            mContext = context;

            SetListView();
        }

        private void SetListView()
        {
            xActivitiesListView.Title = GingerDicser.GetTermResValue(eTermResKey.Activities);
            xActivitiesListView.ListImageType = Amdocs.Ginger.Common.Enums.eImageType.Activity;

            xActivitiesListView.SetDefaultListDataTemplate(new ActivityListItemInfo(mContext));

            xActivitiesListView.AddBtnVisiblity = Visibility.Collapsed;

            List<ListItemOperation> operationsListToAdd = new List<ListItemOperation>();
            ListItemOperation groupsManager = new ListItemOperation();
            groupsManager.ImageType = Amdocs.Ginger.Common.Enums.eImageType.ActivitiesGroup;
            groupsManager.ImageSize = 17;
            groupsManager.ToolTip = GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups, prefixString: "Open", suffixString: "Manager");
            groupsManager.OperationHandler = OpenGroupsManagerHandler;
            operationsListToAdd.Add(groupsManager);
            xActivitiesListView.AddListOperations(operationsListToAdd);

            xActivitiesListView.DataSourceList = mBusinessFlow.Activities;
        }

        private void OpenGroupsManagerHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroupsManagerPage activitiesGroupsManagerPage = new ActivitiesGroupsManagerPage(mBusinessFlow);
            activitiesGroupsManagerPage.ShowAsWindow();
        }

        public void UpdateBusinessFlow(BusinessFlow updateBusinessFlow)
        {
            mBusinessFlow = updateBusinessFlow;
            mContext.BusinessFlow = mBusinessFlow;
            if (mBusinessFlow != null)
            {
                xActivitiesListView.DataSourceList = mBusinessFlow.Activities;
            }
            else
            {
                xActivitiesListView.DataSourceList = null;
            }
        }
    }
}
