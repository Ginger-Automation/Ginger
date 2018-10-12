#region License
/*
Copyright © 2014-2018 European Support Limited

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

        BusinessFlow mBusinessFlow;

        public Visibility ShowActivitiesGroupRepository
        {
            get { return tbiActivitiesGroups.Visibility; }
            set { tbiActivitiesGroups.Visibility = value; }
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
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("@Skin1_ColorB");
                                else
                                    ((TextBlock)ctrl).Foreground = (SolidColorBrush)FindResource("@Skin1_ColorA");

                                ((TextBlock)ctrl).FontWeight = FontWeights.Bold;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error in Action Edit Page tabs style", ex);
            }

            // We do looad on demand
            if (tabRepository.SelectedItem == tbiActivitiesGroups)
            {
                if (((string)tbiActivitiesGroups.Tag) != "Done")
                {
                    ActivitiesGroupsRepoPage = new ActivitiesGroupsRepositoryPage(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ActivitiesGroup>(), mBusinessFlow);
                    frmActivitiesGroups.Content = ActivitiesGroupsRepoPage;
                    // Mark that this tab is loaded with info
                    tbiActivitiesGroups.Tag = "Done";
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
                    ActionsRepoPage = new ActionsRepositoryPage(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<Act>());
                    frmActions.Content = ActionsRepoPage;
                    // Mark that this tab is loaded with info
                    tbiActions.Tag = "Done";
                }
            }

            if (tabRepository.SelectedItem == tbiVariables)
            {
                if (((string)tbiVariables.Tag) != "Done")
                {                    
                    VariablesRepoPage = new VariablesRepositoryPage(WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<VariableBase>());
                    frmVariables.Content = VariablesRepoPage;
                    // Mark that this tab is loaded with info
                    tbiVariables.Tag = "Done";
                }
            }
        }

        public void RefreshCurrentRepo()
        {
            tbiActivitiesGroups.Tag = string.Empty;
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
