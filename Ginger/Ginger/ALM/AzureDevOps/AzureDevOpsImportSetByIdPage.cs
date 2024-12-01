#region License
/*
Copyright © 2014-2024 European Support Limited

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

using Amdocs.Ginger.CoreNET.ALMLib.Azure;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ginger.ALM.AzureDevOps
{
    /// <summary>
    /// Interaction logic for AzureDevOpsImportTestByIdPage.xaml
    /// </summary>
    public partial class AzureDevOpsImportSetByIdPage : Page
    {
        GenericWindow _pageGenericWin = null;
        private string mImportDestinationPath = string.Empty;
        private bool importStatus;
        private string testSetId = "";
        public AzureDevOpsImportSetByIdPage(string importDestinationPath = "")
        {
            InitializeComponent();
            mImportDestinationPath = importDestinationPath;
            ResetStatus();
        }
        private void ResetStatus()
        {
            lblStatus.Content = "";
            lblStatus.Visibility = System.Windows.Visibility.Collapsed;
        }
        private void UpdateStatus(string msg = "")
        {
            if (!importStatus)
            {
                lblStatus.Content = msg;
                lblStatus.Visibility = System.Windows.Visibility.Visible;
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button importButton = new Button
            {
                Content = "Import",
                ToolTip = "Import The Selected Test Suite"
            };
            importButton.Click += new RoutedEventHandler(ImportTestSet);

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, [importButton]);
        }
        private void ImportTestSet(object sender, RoutedEventArgs e)
        {
            ImportSelectedTestSet();
        }

        private void ImportSelectedTestSet()
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            ResetStatus();
            testSetId = txtTestSetId.Text;
            if (testSetId != "")
            {
                if (ALMIntegration.Instance.ImportSelectedTestSets(mImportDestinationPath, new List<AzureTestPlan>() { new AzureTestPlan { AzureID = testSetId } }) == true)
                {
                    _pageGenericWin.Close();
                }
                else
                {
                    importStatus = false;
                    UpdateStatus("Incorrect Test Suite Id");
                }
            }
            else
            {
                importStatus = false;
                UpdateStatus("Test Set Id cannot be empty");
            }
            Mouse.OverrideCursor = null;
        }
        private void ImportBtn_Click(object sender, RoutedEventArgs e)
        {
            ImportSelectedTestSet();
        }
    }
}
