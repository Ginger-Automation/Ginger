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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using GingerCore.Environments;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Environments.AddEnvironmentWizardLib
{
    /// <summary>
    /// Interaction logic for AddNewEnvAppsPage.xaml
    /// </summary>
    public partial class AddNewEnvAppsPage : Page, IWizardPage
    {
        AddEnvironmentWizard mWizard;
        
        public AddNewEnvAppsPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (AddEnvironmentWizard)WizardEventArgs.Wizard;

                    foreach (ApplicationPlatform appPlat in  WorkSpace.Instance.Solution.ApplicationPlatforms)
                    {
                        EnvApplication envApp = new EnvApplication() { Name = appPlat.AppName };
                        envApp.Active = true;
                        mWizard.apps.Add(envApp);
                    }

                    if (mWizard.apps.Count == 0)
                    {
                        mWizard.apps.Add(new EnvApplication() { Name = "MyApplication" });
                    }

                    xAppsListBox.ItemsSource = mWizard.apps;
                    break;
                
            }  

        }

        private void xAddAppBtn_Click(object sender, RoutedEventArgs e)
        {
            string newAppName = "NewApp";
            EnvApplication envApp = new EnvApplication() { Name = newAppName };
            if (GingerCore.General.GetInputWithValidation("Add Environment Application", "Application Name:", ref newAppName, null, false, envApp))
            {
                if (mWizard.apps.Where(x => x.Name == newAppName).FirstOrDefault() == null)
                {                    
                    envApp.Active = true;
                    mWizard.apps.Add(envApp);
                }
            }
        }
    }
}
