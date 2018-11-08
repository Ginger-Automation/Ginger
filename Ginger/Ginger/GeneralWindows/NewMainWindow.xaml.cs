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
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerWPF.BusinessFlowsLib;
using GingerWPF.TestLib;
using System.Windows;

namespace GingerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class NewMainWindow : Window
    {
        BusinessFlowsPage mBusinessFlowsPage;
        public NewMainWindow()
        {
            InitializeComponent();

            //((WorkSpaceLib.WorkSpaceEventHandler)WorkSpace.Instance.EventHandler).GingerMainWindow = this;

            SolutionFrame.SetContent(new NewStartPage());

            //Hide all tabs until a solution is open
            SetTabsVisibility(false);
        }

        internal void AutomateBusinessFlow(BusinessFlow BF)
        {
            // BusinessFlowsFrame.Content = new BusinessFlowAutomatePage(BF);
        }

        private void SetTabsVisibility(bool v)
        {
            Visibility v1 = Visibility.Collapsed;
            if (v) v1 = Visibility.Visible;

            BusinessFlowsTab.Visibility = v1;
            RunTab.Visibility = v1;
            ConfigurationsTab.Visibility = v1;
            ResourcesTab.Visibility = v1;
        }

        internal void ShowSolution(Solution solution)
        {
            SetTabsVisibility(true);
            BusinessFlowsTab.IsSelected = true;
            SolutionLabel.Content = solution.Name;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // WorkSpace.Instance.CleanupBeforeAppClosing();
        }

        internal void SolutionClosed()
        {
            mBusinessFlowsPage = null;
            SetTabsVisibility(false);
            BusinessFlowsFrame.Content = null;
            RunFrame.Content = null;
            ResourcesFrame.Content = null;
            ConfigurationsFrame.Content = null;
            //Must do refresh so frames contents will go away and become null
            this.Refresh();
        }

        internal void ShowBusinessFlows()
        {
            MainTabControl.SelectedItem = BusinessFlowsTab;
            BusinessFlowsFrame.Content = mBusinessFlowsPage;
            // TODO: refresh BFs?
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            //TODO: verify/cleanup 
            var v = App.Current.Windows;
            App.Current.Shutdown();
        }

        private void MainTabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (MainTabControl.SelectedItem == BusinessFlowsTab)
            {
                if (BusinessFlowsFrame.Content == null)
                {
                    RepositoryFolder<BusinessFlow> RF = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<BusinessFlow>();
                    mBusinessFlowsPage = new BusinessFlowsPage(RF);
                    BusinessFlowsFrame.SetContent(mBusinessFlowsPage);
                }
            }

            if (MainTabControl.SelectedItem == RunTab)
            {
                //Temp - try list sync using SR cache
                if (RunFrame.Content == null)
                {
                    RunFrame.SetContent(new ListsPage());
                }
            }

            //TODO: keep all open pages and cache !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! load o

            if (MainTabControl.SelectedItem == ResourcesTab)
            {
                if (ResourcesFrame.Content == null)
                {
                    //ResourcesFrame.SetContent(new ResourcesPage());
                }
            }

            if (MainTabControl.SelectedItem == ConfigurationsTab)
            {
                if (ConfigurationsFrame.Content == null)
                {
                    // ConfigurationsFrame.SetContent(new ConfigurationsPage());
                }
            }
        }
    }
}
