#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Ginger.SolutionGeneral;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace Ginger
{
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : Page
    {
        public StartPage()
        {
            InitializeComponent();

            //TODO: load from external - so easier to update
            lblAppVersion.Content = "Version " + Amdocs.Ginger.Common.GeneralLib.ApplicationInfo.ApplicationUIversion;

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(autoLoadLastSolCheckBox, CheckBox.IsCheckedProperty, WorkSpace.Instance.UserProfile, nameof(UserProfile.AutoLoadLastSolution));
            SetRecentSolutions();
        }

        private void SetRecentSolutions()
        {
            try
            {
                ObservableList<Hyperlink> recentSolutionsLinksList = [];
                foreach (Solution sol in ((UserProfileOperations)WorkSpace.Instance.UserProfile.UserProfileOperations).RecentSolutionsAsObjects)
                {
                    Hyperlink solLink = new Hyperlink
                    {
                        Tag = sol.Name,
                        ToolTip = sol.Folder
                    };
                    recentSolutionsLinksList.Add(solLink);
                }

                if (recentSolutionsLinksList.Count > 0)
                {
                    recentSolutionsListBox.ItemsSource = recentSolutionsLinksList;
                }
                else
                {
                    recentSolutionsListBox.Visibility = System.Windows.Visibility.Hidden;
                    recentSolutionsLabel.Visibility = System.Windows.Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to set the latest solutions links", ex);
            }
        }

        private void RecentSolution_Click(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                string selectedSolFolder = ((Hyperlink)sender).ToolTip.ToString().ToUpper();
                Solution selectedSol = ((UserProfileOperations)WorkSpace.Instance.UserProfile.UserProfileOperations).RecentSolutionsAsObjects.FirstOrDefault(x => x.Folder.ToUpper() == selectedSolFolder);

                if (selectedSol != null)
                {
                    WorkSpace.Instance.OpenSolution(selectedSol.Folder);
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.SolutionLoadError, "Selected Solution was not found");
                }

                e.Handled = true;
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.SolutionLoadError, ex);
            }
        }

        private void HandleLinkClick(object sender, RoutedEventArgs e)
        {
            Hyperlink hl = (Hyperlink)sender;
            string navigateUri = hl.NavigateUri.ToString();
            Process.Start(new ProcessStartInfo() { FileName = navigateUri, UseShellExecute = true });
            e.Handled = true;
        }
    }
}
