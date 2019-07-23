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
using Ginger.SolutionWindows.TreeViewItems;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.Variables;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ginger.Repository
{
    /// <summary>
    /// Interaction logic for RepositoryPage.xaml
    /// </summary>
    public partial class RepositoryPage : Page
    {
        public ActivitiesGroupsRepositoryPage ActivitiesGroupsRepoPage;
        public ActivitiesRepositoryPage ActivitiesRepoPage;
        public ActionsRepositoryPage ActionsRepoPage;
        public VariablesRepositoryPage VariablesRepoPage;

        Context mContext = new Context();
        BusinessFlow mBusinessFlow;

        public Visibility ShowActivitiesGroupRepository
        {
            get { return tbActivitiesGroups.Visibility; }
            set { tbActivitiesGroups.Visibility = value; }
        }
        public Visibility ShowActivitiesRepository
        {
            get { return tbiActivities.Visibility; }
            set { tbiActivities.Visibility = value; }
        }
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

        public RepositoryPage(BusinessFlow businessFlow = null)
        {
            InitializeComponent();

            mBusinessFlow = businessFlow;
            mContext.BusinessFlow = businessFlow;

            xActivitiesGroupsTextBlock.Text = GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups);
            xActivitiesTextBlock.Text = GingerDicser.GetTermResValue(eTermResKey.Activities);
            xVariablesTextBlock.Text = GingerDicser.GetTermResValue(eTermResKey.Variables);
        }

        public void UpdateBusinessFlow(BusinessFlow bf)
        {
            mBusinessFlow = bf;
            mContext.BusinessFlow = bf;
            if (ActivitiesGroupsRepoPage != null)
            {
                ActivitiesGroupsRepoPage.UpdateBusinessFlow(mBusinessFlow);
            }
            if (ActivitiesRepoPage != null)
            {
                ActivitiesRepoPage.UpdateBusinessFlow(mBusinessFlow);
            }
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
                    ActivitiesGroupsRepoPage = new ActivitiesGroupsRepositoryPage(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ActivitiesGroup>(), mContext);
                    frmActivitiesGroups.Content = ActivitiesGroupsRepoPage;
                    // Mark that this tab is loaded with info
                    tbActivitiesGroups.Tag = "Done";
                }
            }

            if (tabRepository.SelectedItem == tbiActivities)
            {
                if (((string)tbiActivities.Tag) != "Done")
                {
                    ActivitiesRepoPage = new ActivitiesRepositoryPage(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Activity>(), mContext);
                    frmActivities.Content = ActivitiesRepoPage;
                    // Mark that this tab is loaded with info
                    tbiActivities.Tag = "Done";
                }
            }

            if (tabRepository.SelectedItem == tbiActions)
            {
                if (((string)tbiActions.Tag) != "Done")
                {
                    ActionsRepoPage = new ActionsRepositoryPage(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Act>(), mContext);
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
            tbActivitiesGroups.Tag = string.Empty;
            tbiActivities.Tag = string.Empty;
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
