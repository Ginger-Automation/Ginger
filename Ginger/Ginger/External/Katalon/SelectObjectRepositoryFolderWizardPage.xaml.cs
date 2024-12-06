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

using Amdocs.Ginger.Common;
using GingerCore.GeneralLib;
using GingerWPF.WizardLib;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.External.Katalon
{
    /// <summary>
    /// Interaction logic for SelectObjectRepositoryFolderWizardPage.xaml
    /// </summary>
    public partial class SelectObjectRepositoryFolderWizardPage : Page, IWizardPage
    {
        private readonly ImportKatalonObjectRepositoryWizard _wizard;

        public SelectObjectRepositoryFolderWizardPage(ImportKatalonObjectRepositoryWizard wizard)
        {
            if (wizard == null)
            {
                throw new ArgumentNullException(nameof(wizard));
            }

            InitializeComponent();

            _wizard = wizard;
            BindingHandler.ObjFieldBinding(DirectoryTextBox, TextBox.TextProperty, _wizard, nameof(ImportKatalonObjectRepositoryWizard.SelectedDirectory));
        }

        public void WizardEvent(WizardEventArgs e)
        {
            switch (e.EventType)
            {
                case EventType.Active:
                    _wizard.mWizardWindow?.SetFinishButtonEnabled(false);
                    break;
                case EventType.LeavingForNextPage:
                    if (string.IsNullOrWhiteSpace(_wizard.SelectedDirectory))
                    {
                        e.CancelEvent = true;
                        Reporter.ToUser(eUserMsgKey.InvalidKatalonObjectRepository, "Please select a folder containing Katalon Object Repository files.");
                        break;
                    }
                    if (!Directory.Exists(_wizard.SelectedDirectory.Trim()))
                    {
                        e.CancelEvent = true;
                        Reporter.ToUser(eUserMsgKey.InvalidKatalonObjectRepository, $"The selected folder does not exist.");
                        break;
                    }
                    break;
                default:
                    break;
            }
        }

        private void DirectoryBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            using System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new();
            string directory = General.SetupBrowseFolder(folderBrowserDialog);
            _wizard.SelectedDirectory = directory;
            _wizard.POMViewModels.ClearAll();
        }
    }
}
