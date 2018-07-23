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

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Ginger.Reports;

namespace Ginger.Run.RunSetActions
{
    /// <summary>
    /// Interaction logic for RunSetActionHTMLReportEditPage.xaml
    /// </summary>
    public partial class RunSetActionHTMLReportEditPage : Page
    {
        private RunSetActionHTMLReport runSetActionHTMLReport;

        public RunSetActionHTMLReportEditPage(RunSetActionHTMLReport RunSetActionHTMLReport)
        {
            InitializeComponent();

            this.runSetActionHTMLReport = RunSetActionHTMLReport;

            App.ObjFieldBinding(HTMLReportFolderTextBox, TextBox.TextProperty, RunSetActionHTMLReport, RunSetActionHTMLReport.Fields.HTMLReportFolderName);
            App.ObjFieldBinding(UseAlternativeHTMLReportFolderCbx, CheckBox.IsCheckedProperty, RunSetActionHTMLReport, RunSetActionHTMLReport.Fields.isHTMLReportFolderNameUsed);
            App.ObjFieldBinding(UsePermanentHTMLReportFolderCbx, CheckBox.IsCheckedProperty, RunSetActionHTMLReport, RunSetActionHTMLReport.Fields.isHTMLReportPermanentFolderNameUsed);
            CurrentTemplatePickerCbx_Binding();
        }

        private void DefaultTemplatePickerCbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        public void CurrentTemplatePickerCbx_Binding()
        {
            CurrentTemplatePickerCbx.ItemsSource = null;

            if ((App.UserProfile.Solution != null) && (App.LocalRepository.GetSolutionHTMLReportConfigurations() != null) && (App.LocalRepository.GetSolutionHTMLReportConfigurations().Count > 0))
            {
                CurrentTemplatePickerCbx.ItemsSource = App.LocalRepository.GetSolutionHTMLReportConfigurations();
                CurrentTemplatePickerCbx.DisplayMemberPath = HTMLReportConfiguration.Fields.Name;
                CurrentTemplatePickerCbx.SelectedValuePath = HTMLReportConfiguration.Fields.ID;
                if ((runSetActionHTMLReport.selectedHTMLReportTemplateID != 0))
                {
                    CurrentTemplatePickerCbx.SelectedIndex = CurrentTemplatePickerCbx.Items.IndexOf(App.LocalRepository.GetSolutionHTMLReportConfigurations().Where(x => (x.ID == runSetActionHTMLReport.selectedHTMLReportTemplateID)).FirstOrDefault());
                    if (CurrentTemplatePickerCbx.SelectedIndex == -1)
                    {
                        CurrentTemplatePickerCbx.SelectedIndex = CurrentTemplatePickerCbx.Items.IndexOf(App.LocalRepository.GetSolutionHTMLReportConfigurations().Where(x => (x.IsDefault == true)).FirstOrDefault());
                    }
                }
                else
                {
                    CurrentTemplatePickerCbx.SelectedIndex = CurrentTemplatePickerCbx.Items.IndexOf(App.LocalRepository.GetSolutionHTMLReportConfigurations().Where(x => (x.IsDefault == true)).FirstOrDefault());
                }
            }
        }

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            string s = General.OpenSelectFolderDialog("Create HTML Report at Folder");
            if (s != null)
            {
                HTMLReportFolderTextBox.Text = s;
            }
        }

        private void UseAlternativeHTMLReportFolderCbx_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)UseAlternativeHTMLReportFolderCbx.IsChecked)
            {
                HTMLReportFolderPanel.IsEnabled = true;
            }
        }

        private void CurrentTemplatePickerCbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            runSetActionHTMLReport.selectedHTMLReportTemplateID = ((HTMLReportConfiguration)CurrentTemplatePickerCbx.SelectedItem).ID;
        }

        private void UseAlternativeHTMLReportFolderCbx_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)UseAlternativeHTMLReportFolderCbx.IsChecked)
            {
                HTMLReportFolderPanel.IsEnabled = false;
            }
        }
    }
}
