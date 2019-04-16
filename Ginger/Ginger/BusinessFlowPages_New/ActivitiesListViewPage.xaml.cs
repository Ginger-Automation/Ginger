using Amdocs.Ginger.Common;
using Ginger.BusinessFlowPages.ListViewItems;
using Ginger.UserControlsLib.UCListView;
using GingerCore;
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

            ////TODO: move DataTemplate into ListView
            //DataTemplate dataTemp = new DataTemplate();

            //FrameworkElementFactory stackPnlFac = new FrameworkElementFactory(typeof(StackPanel));

            //FrameworkElementFactory activitiesGroupListItemFac = new FrameworkElementFactory(typeof(UcListViewItem));
            //activitiesGroupListItemFac.SetBinding(UcListViewItem.ItemProperty, new Binding());
            //activitiesGroupListItemFac.SetValue(UcListViewItem.ItemInfoProperty, new ActivitiesGroupListItemInfo(mContext));
            //stackPnlFac.AppendChild(activitiesGroupListItemFac);

            //FrameworkElementFactory activitiesListViewFac = new FrameworkElementFactory(typeof(UcListView));
            //activitiesListViewFac.SetValue(MarginProperty, new Padding(10, 0, 0, 0));
            //activitiesGroupListItemFac.SetBinding(UcListViewItem.ItemProperty, new Binding());
            //activitiesGroupListItemFac.SetValue(UcListViewItem.ItemInfoProperty, new ActivitiesGroupListItemInfo(mContext));
            //stackPnlFac.AppendChild(activitiesListViewFac);


            //dataTemp.VisualTree = stackPnlFac;
            //xActivitiesListView.List.ItemTemplate = dataTemp;
            //xActivitiesListView.DataSourceList = mBusinessFlow.ActivitiesGroups;


            xActivitiesListView.SetDefaultListDataTemplate(new ActivityListItemInfo(mContext));

            xActivitiesListView.AddBtnVisiblity = Visibility.Collapsed;

            xActivitiesListView.DataSourceList = mBusinessFlow.Activities;
        }

        public void UpdateBusinessFlow(BusinessFlow updateBusinessFlow)
        {
            mBusinessFlow = updateBusinessFlow;
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
