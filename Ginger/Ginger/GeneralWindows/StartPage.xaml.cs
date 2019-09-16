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

using Amdocs.Ginger.Common;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using GingerCore;
using Ginger.Environments;
using System.Diagnostics;
using Ginger.SolutionGeneral;
using amdocs.ginger.GingerCoreNET;

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
            lblAppVersion.Content = "Version " + Amdocs.Ginger.Common.GeneralLib.ApplicationInfo.ApplicationVersionWithInfo;
                                  
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(autoLoadLastSolCheckBox, CheckBox.IsCheckedProperty,  WorkSpace.Instance.UserProfile, nameof(UserProfile.AutoLoadLastSolution));
            SetRecentSolutions();
        }

        private void SetRecentSolutions()
        {
            try
            {
                ObservableList<Hyperlink> recentSolutionsLinksList = new ObservableList<Hyperlink>();
                foreach (Solution sol in  WorkSpace.Instance.UserProfile.RecentSolutionsAsObjects)
                {
                    Hyperlink solLink = new Hyperlink();
                    solLink.Tag = sol.Name;
                    solLink.ToolTip = sol.Folder;
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
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to set the latest solutions links", ex);
            }
        }

        private void RecentSolution_Click(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                string selectedSolFolder = ((Hyperlink)sender).ToolTip.ToString().ToUpper();
                Solution selectedSol =  WorkSpace.Instance.UserProfile.RecentSolutionsAsObjects.Where(x=>x.Folder.ToUpper() == selectedSolFolder).FirstOrDefault();

                if (selectedSol != null)
                {
                    WorkSpace.Instance.OpenSolution(selectedSol.Folder);                    
                }
                else
                    Reporter.ToUser(eUserMsgKey.SolutionLoadError, "Selected Solution was not found");

                e.Handled = true;
            }
            catch(Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.SolutionLoadError, ex);
            }
        }

        private void HandleLinkClick(object sender, RoutedEventArgs e)
        {
            Hyperlink hl = (Hyperlink)sender;
            string navigateUri = hl.NavigateUri.ToString();
            Process.Start(new ProcessStartInfo(navigateUri));
            e.Handled = true;
        }
    }
}
