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

using Amdocs.Ginger.Repository;
using Ginger;
using GingerWPF.WizardLib;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.PluginsLib.AddPluginWizardLib
{
    /// <summary>
    /// Interaction logic for SelectPluginFolderPage.xaml
    /// </summary>
    public partial class SelectPlugPackageinFolderPage : Page, IWizardPage
    {
        AddPluginPackageWizard wiz;
        public SelectPlugPackageinFolderPage()
        {
            InitializeComponent();     
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {       
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    wiz = (AddPluginPackageWizard)WizardEventArgs.Wizard;
                    FolderTextBox.BindControl(wiz, nameof(AddPluginPackageWizard.Folder));
                    break;
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string s = General.OpenSelectFolderDialog("Select Plugin Folder");
            if (!string.IsNullOrEmpty(s))
            {
                FolderTextBox.Text = s;
            }
        }

        private void FolderTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO: check folder exist and info file exist
            wiz.PluginPackage = new PluginPackage(FolderTextBox.Text);
        }
    }
}
