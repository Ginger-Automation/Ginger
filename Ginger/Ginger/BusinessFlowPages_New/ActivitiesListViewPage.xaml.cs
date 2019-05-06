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

        CollectionView groupView;
        private void SetListView()
        {
            //List Title
            xActivitiesListView.Title = GingerDicser.GetTermResValue(eTermResKey.Activities);
            xActivitiesListView.ListImageType = Amdocs.Ginger.Common.Enums.eImageType.Activity;

            //List Items
            xActivitiesListView.SetDefaultListDataTemplate(new ActivityListItemInfo(mContext));


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
            groupView = (CollectionView)CollectionViewSource.GetDefaultView(xActivitiesListView.List.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription(nameof(Activity.ActivitiesGroupID));
            groupView.GroupDescriptions.Clear();
            groupView.GroupDescriptions.Add(groupDescription);

            //CollectionViewSource view2 = (CollectionView)CollectionViewSource.GetDefaultView(xActivitiesListView.List.ItemsSource);
            //PropertyGroupDescription groupDescription2 = new PropertyGroupDescription(nameof(Activity.ActivitiesGroupID));
            //view2.IsLiveGroupingRequested = true;
            //view2.GroupDescriptions.Add(groupDescription2);
        }

        private void OpenGroupsManagerHandler(object sender, RoutedEventArgs e)
        {
            ActivitiesGroupsManagerPage activitiesGroupsManagerPage = new ActivitiesGroupsManagerPage(mBusinessFlow);
            activitiesGroupsManagerPage.ShowAsWindow();
            groupView.Refresh();
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
