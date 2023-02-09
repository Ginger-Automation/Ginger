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

using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.SolutionGeneral;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib;
using GingerWPF.WizardLib;
using System;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.SolutionLib
{
    /// <summary>
    /// Interaction logic for NewSolutionNameFolderWizardPage.xaml
    /// </summary>
    public partial class NewSolutionNameFolderWizardPage : Page, IWizardPage
    {
        Solution mSolution;
        public NewSolutionNameFolderWizardPage(Solution solution)
        {
            InitializeComponent();

            mSolution = solution;

            SolutionNameTextBox.BindControl(mSolution, nameof(Solution.Name));
            SolutionFolderTextBox.BindControl(mSolution, nameof(Solution.Folder));
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "Select Solution folder";
            dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            if (mSolution.Folder != string.Empty)
                dlg.SelectedPath = mSolution.Folder;
            dlg.ShowNewFolderButton = true;
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                SolutionFolderTextBox.Text = dlg.SelectedPath;
            }
        }
    }
}