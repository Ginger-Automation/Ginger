using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        public VariablesRepositoryPage VariablesRepoPage;

        BusinessFlow mBusinessFlow;

        Context mContext;
        public Visibility ShowActionsRepository
        {
            get { return tbiActions.Visibility; }
            set { tbiActions.Visibility = value; }
        }
        public Visibility ShowVariablesRepository
        {
            get { return tbiVariables.Visibility; }
            set { tbiVariables.Visibility = value; }
        }

        public SharedRepositoryNavPage(Context context)     //(BusinessFlow businessFlow = null)
        {
            InitializeComponent();
            mContext = context;
            mBusinessFlow = mContext.BusinessFlow;

            xVariablesTextBlock.Text = GingerDicser.GetTermResValue(eTermResKey.Variables);
        }

        public void UpdateBusinessFlow(BusinessFlow bf)
        {
            mBusinessFlow = bf;
            if (ActionsRepoPage != null)
            {
                ActionsRepoPage.UpdateBusinessFlow(mBusinessFlow);
            }
            if (VariablesRepoPage != null)
            {
                VariablesRepoPage.UpdateBusinessFlow(mBusinessFlow);
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (tabRepository.SelectedItem != null)
                {
                    foreach (TabItem tab in tabRepository.Items)
                    {
                        foreach (object ctrl in ((StackPanel)(tab.Header)).Children)

                            if (ctrl.GetType() == typeof(TextBlock))
                            {
                                if (tabRepository.SelectedItem == tab)
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
            if (tabRepository.SelectedItem == tbActivitiesGroups)
            {
                if (((string)tbActivitiesGroups.Tag) != "Done")
                {
                    ActivitiesGroupsRepoPage = new ActivitiesGroupsRepositoryPage(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ActivitiesGroup>(), mBusinessFlow);
                    frmActivitiesGroups.Content = ActivitiesGroupsRepoPage;
                    // Mark that this tab is loaded with info
                    tbActivitiesGroups.Tag = "Done";
                }
            }

            if (tabRepository.SelectedItem == tbiActivities)
            {
                if (((string)tbiActivities.Tag) != "Done")
                {
                    ActivitiesRepoPage = new ActivitiesRepositoryPage(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Activity>(), mBusinessFlow);
                    frmActivities.Content = ActivitiesRepoPage;
                    // Mark that this tab is loaded with info
                    tbiActivities.Tag = "Done";
                }
            }

            if (tabRepository.SelectedItem == tbiActions)
            {
                if (((string)tbiActions.Tag) != "Done")
                {
                    ActionsRepoPage = new ActionsRepositoryPage(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Act>(), mBusinessFlow);
                    frmActions.Content = ActionsRepoPage;
                    // Mark that this tab is loaded with info
                    tbiActions.Tag = "Done";
                }
            }

            if (tabRepository.SelectedItem == tbiVariables)
            {
                if (((string)tbiVariables.Tag) != "Done")
                {
                    VariablesRepoPage = new VariablesRepositoryPage(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<VariableBase>(), mBusinessFlow);
                    frmVariables.Content = VariablesRepoPage;
                    // Mark that this tab is loaded with info
                    tbiVariables.Tag = "Done";
                }
            }
        }

        public void RefreshCurrentRepo()
        {
            tbiActions.Tag = string.Empty;
            tbiVariables.Tag = string.Empty;

            //to re-load current
            if (tabRepository.SelectedItem != null)
            {
                TabItem selected = (TabItem)tabRepository.SelectedItem;
                tabRepository.SelectedItem = null;
                tabRepository.SelectedItem = selected;
            }
        }
    }
}
