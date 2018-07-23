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

using System.Windows;
using System.Windows.Controls;
using Ginger.Reports;

namespace Ginger.Run.RunSetActions
{
    /// <summary>
    /// Interaction logic for RunSetActionSaveResultsEditPage.xaml
    /// </summary>
    public partial class RunSetActionSaveResultsEditPage : Page
    {
        public RunSetActionSaveResultsEditPage(RunSetActionSaveResults runSetActionSaveResults)
        {
            InitializeComponent();

            App.ObjFieldBinding(SaveResultstoFolderNameTextBox , TextBox.TextProperty, runSetActionSaveResults, RunSetActionSaveResults.Fields.SaveResultstoFolderName);
            App.ObjFieldBinding(TemplateNameTextBox, TextBox.TextProperty, runSetActionSaveResults, RunSetActionSaveResults.Fields.TemplateName);
            App.ObjFieldBinding(OpenExecutionResultsFolderCheckBox, CheckBox.IsCheckedProperty, runSetActionSaveResults, RunSetActionSaveResults.Fields.OpenExecutionResultsFolder);
            App.ObjFieldBinding(SaveindividualBFReportCheckBox, CheckBox.IsCheckedProperty, runSetActionSaveResults, RunSetActionSaveResults.Fields.SaveindividualBFReport);
            App.ObjFieldBinding(SaveResultstoInSolutionFolderNameTextBox, TextBox.TextProperty, runSetActionSaveResults, RunSetActionSaveResults.Fields.SaveResultsInSolutionFolderName);
            App.ObjFieldBinding(SaveResultstoFolderNameTextBox, TextBox.TextProperty, runSetActionSaveResults, RunSetActionSaveResults.Fields.SaveResultstoFolderName);
        }

        private void SelectTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            ReportTemplateSelector RTS = new ReportTemplateSelector();
            RTS.ShowAsWindow();
            if (RTS.SelectedReportTemplate != null)
            {
                TemplateNameTextBox.Text = RTS.SelectedReportTemplate.Name;
            }
        }

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            string s = General.OpenSelectFolderDialog("Save Results to Folder");
            if (s != null)
            {
                SaveResultstoFolderNameTextBox.Text = s;
            }
        }
    }
}