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

namespace Ginger.Run.RunSetActions
{
    /// <summary>
    /// Interaction logic for RunSetActionScriptEditPage.xaml
    /// </summary>
    public partial class RunSetActionGenerateTestNGReportEditPage : Page
    {
        RunSetActionGenerateTestNGReport mRunSetActionScript;
        public RunSetActionGenerateTestNGReportEditPage(RunSetActionGenerateTestNGReport TestNGReport)
        {
            InitializeComponent();

            mRunSetActionScript = TestNGReport;

            App.ObjFieldBinding(TargetTestNGReportFolderBox, TextBox.TextProperty, mRunSetActionScript, RunSetActionGenerateTestNGReport.Fields.SaveResultsInSolutionFolderName);
            App.ObjFieldBinding(sourceActivityRadioBtn, RadioButton.IsCheckedProperty, mRunSetActionScript, RunSetActionGenerateTestNGReport.Fields.IsStatusByActivity);
            App.ObjFieldBinding(sourceActivitiesRadioBtn, RadioButton.IsCheckedProperty, mRunSetActionScript, RunSetActionGenerateTestNGReport.Fields.IsStatusByActivitiesGroup);
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                TargetTestNGReportFolderBox.Text = dlg.SelectedPath;
            }
        }
    }
}
