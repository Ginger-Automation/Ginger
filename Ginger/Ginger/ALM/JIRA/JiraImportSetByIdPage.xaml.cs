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

using Amdocs.Ginger.Common;
using GingerCore.ALM.JIRA;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ginger.ALM.JIRA
{
    /// <summary>
    /// Interaction logic for JiraImportTestByIdPage.xaml
    /// </summary>
    public partial class JiraImportSetByIdPage : Page
    {
        GenericWindow _pageGenericWin = null;
        private string mImportDestinationPath = string.Empty;
        private bool importStatus;
        private string testSetId = "";
        public JiraImportSetByIdPage(string importDestinationPath = "")
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
        private void UpdateStatus(string msg="")
        {
            if (importStatus == false)
            {
                lblStatus.Content = msg;
                lblStatus.Visibility = System.Windows.Visibility.Visible;
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button importButton = new Button();
            importButton.Content = "Import";
            importButton.ToolTip = "Import The Selected Test Set";
            importButton.Click += new RoutedEventHandler(ImportTestSet);

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, new ObservableList<Button> { importButton });
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
                if (ALMIntegration.Instance.ImportSelectedTestSets(mImportDestinationPath, new List<JiraTestSet>() { new JiraTestSet { Key = testSetId } } ) == true)
                {
                    _pageGenericWin.Close();
                }
                else
                {
                    importStatus = false;
                    UpdateStatus("Incorrect Test Set Id");
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
