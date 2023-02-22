#region License
/*
Copyright © 2014-2023 European Support Limited

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
using Ginger.Environments;
using Ginger.SolutionGeneral;
using GingerCoreNET.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib;
using GingerWPF.WizardLib;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.SolutionLib
{
    /// <summary>
    /// Interaction logic for RecentSolutionsPage.xaml
    /// </summary>
    public partial class RecentSolutionsPage : Page
    {
        public RecentSolutionsPage()
        {
            InitializeComponent();
            SetRecentSolutions();
        }

        private void SetRecentSolutions()
        {           
            //WorkSpace.Instance.UserProfile.SetRecentSolutionsObjects();
            //recentSolutionsListBox.ItemsSource = WorkSpace.Instance.UserProfile.RecentSolutionsObjects;                       
        }

        private void OpenSolutionbutton_Click(object sender, RoutedEventArgs e)
        {
            string s = Ginger.General.OpenSelectFolderDialog("Select Solution Folder");
            if (!string.IsNullOrEmpty(s))
            {
                WorkSpace.Instance.OpenSolution(s);
            }
        }

        private void recentSolutionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            string folder = ((Solution)recentSolutionsListBox.SelectedItem).Folder;
            WorkSpace.Instance.OpenSolution(folder);
        }

        private void NewSolutionbutton_Click(object sender, RoutedEventArgs e)
        {
            WizardWindow.ShowWizard(new NewSolutionWizard());
        }
    }
}
