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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger;
using GingerWPF.WizardLib;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace GingerWPF.PluginsLib.AddPluginWizardLib
{
    /// <summary>
    /// Interaction logic for PluginInfoPage.xaml
    /// </summary>
    public partial class PlugPackageinInfoPage : Page, IWizardPage
    {
        AddPluginPackageWizard wiz;
        PluginPackage mPluginPackage;

        public PlugPackageinInfoPage()
        {
            InitializeComponent();                                 
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch(WizardEventArgs.EventType)
            {
                case EventType.Init:
                    wiz = (AddPluginPackageWizard)WizardEventArgs.Wizard;
                    break;
                case EventType.Active:
                    mPluginPackage = wiz.PluginPackage;
                    xIDTextBox.Text = mPluginPackage.PluginID;
                    xVersionTextBox.Text = mPluginPackage.PluginPackageVersion;
                    FolderTextBox.BindControl(mPluginPackage, nameof(PluginPackage.Folder));

                    List<StandAloneAction> actions = mPluginPackage.LoadServicesInfoFromFile();

                    // show distict list of the services
                    ServicesGrid.ItemsSource = (from x in actions select x.ServiceID).Distinct();

                    // TODO: get selected service only  - add radio show al or per selected
                    
                    ActionsDataGrid.ItemsSource = actions;
                    
                    break;

            }
            
        }

        private void PluginsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void ServicesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
