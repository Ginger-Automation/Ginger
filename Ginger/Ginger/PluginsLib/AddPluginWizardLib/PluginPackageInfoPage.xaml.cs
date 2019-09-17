#region License
/*
Copyright © 2014-2019 European Support Limited

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
using System;
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
                    try
                    {
                        wiz.PluginPackage = new PluginPackage(wiz.Folder);
                    }
                    catch(Exception ex)
                    {
                        Reporter.ToUser(eUserMsgKey.StaticErrorMessage, string.Format("Failed to find the Plugin package, error: '{0}'", ex));
                    }
                    if (wiz.PluginPackage != null)
                    {
                        mPluginPackage = wiz.PluginPackage;
                        xIDTextBox.Text = mPluginPackage.PluginId;
                        xVersionTextBox.Text = mPluginPackage.PluginPackageVersion;
                        FolderTextBox.BindControl(mPluginPackage, nameof(PluginPackage.Folder));
                        mPluginPackage.LoadServicesFromJSON();
                        ServicesGrid.ItemsSource = mPluginPackage.Services;
                        ActionsDataGrid.ItemsSource = mPluginPackage.Services[0].Actions;
                    }
                    break;

            }
            
        }

        private void PluginsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void ServicesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PluginServiceInfo pluginServiceInfo = (PluginServiceInfo)ServicesGrid.CurrentItem;
            ActionsDataGrid.ItemsSource = pluginServiceInfo.Actions;
        }
    }
}
