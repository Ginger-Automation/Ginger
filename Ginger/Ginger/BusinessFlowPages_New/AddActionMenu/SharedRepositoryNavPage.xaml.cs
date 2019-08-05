using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.Variables;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ginger.BusinessFlowsLibNew.AddActionMenu
{
    /// <summary>
    /// Interaction logic for RepositoryPage.xaml
    /// </summary>
    public partial class SharedRepositoryNavPage : Page
    {
        public ActivitiesGroupsRepositoryPage ActivitiesGroupsRepoPage;
        public ActivitiesRepositoryPage ActivitiesRepoPage;
        public ActionsRepositoryPage ActionsRepoPage;

        Context mContext;
        public Visibility ShowActionsRepository
        {
            get { return xTabActions.Visibility; }
            set { xTabActions.Visibility = value; }
        }


        public SharedRepositoryNavPage(Context context)     
        {
            InitializeComponent();
            mContext = context;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (xTabRepository.SelectedItem != null)
                {
                    foreach (TabItem tab in xTabRepository.Items)
                    {
                        foreach (object ctrl in ((StackPanel)(tab.Header)).Children)

                            if (ctrl.GetType() == typeof(TextBlock))
                            {
                                if (xTabRepository.SelectedItem == tab)
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$SelectionColor_Pink");
                                else
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("$Color_DarkBlue");

                                ((TextBlock)ctrl).FontWeight = FontWeights.Bold;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in Action Edit Page tabs style", ex);
            }

            // We do load on demand
            if (xTabRepository.SelectedItem == xTabActivitiesGroups)
            {
                if (((string)xTabActivitiesGroups.Tag) != "Done")
                {
                    ActivitiesGroupsRepoPage = new ActivitiesGroupsRepositoryPage(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ActivitiesGroup>(), mContext);
                    xFrameActivitiesGroups.Content = ActivitiesGroupsRepoPage;
                    // Mark that this tab is loaded with info
                    xTabActivitiesGroups.Tag = "Done";
                }
            }

            if (xTabRepository.SelectedItem == xTabActivities)
            {
                if (((string)xTabActivities.Tag) != "Done")
                {
                    ActivitiesRepoPage = new ActivitiesRepositoryPage(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Activity>(), mContext);
                    xFrameActivities.Content = ActivitiesRepoPage;
                    // Mark that this tab is loaded with info
                    xTabActivities.Tag = "Done";
                }
            }

            if (xTabRepository.SelectedItem == xTabActions)
            {
                if (((string)xTabActions.Tag) != "Done")
                {
                    ActionsRepoPage = new ActionsRepositoryPage(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Act>(), mContext);
                    xFrameActions.Content = ActionsRepoPage;
                    // Mark that this tab is loaded with info
                    xTabActions.Tag = "Done";
                }
            }
        }

        public void RefreshCurrentRepo()
        {
            xTabActions.Tag = string.Empty;

            //to re-load current
            if (xTabRepository.SelectedItem != null)
            {
                TabItem selected = (TabItem)xTabRepository.SelectedItem;
                xTabRepository.SelectedItem = null;
                xTabRepository.SelectedItem = selected;
            }
        }
    }
}
