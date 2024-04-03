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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger.SolutionWindows;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Environments;
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
            xAddApplicationToSolution.ButtonTextSize = 12;
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(EnvApplication.Active), Header = " ", StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = nameof(EnvApplication.Name), Header = GingerDicser.GetTermResValue(eTermResKey.TargetApplication), WidthWeight = 60 });
            view.GridColsView.Add(new GridColView() { Field = nameof(EnvApplication.ItemImageType), Header = " ", StyleType = GridColView.eGridColStyleType.ImageMaker, WidthWeight = 5, MaxWidth = 16 });
            view.GridColsView.Add(new GridColView() { Field = nameof(EnvApplication.Platform), Header = "Platform Type", WidthWeight = 40 });

            SelectApplicationGrid.SetAllColumnsDefaultView(view);
            SelectApplicationGrid.InitViewItems();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (AddEnvironmentWizard)WizardEventArgs.Wizard;

                    foreach (ApplicationPlatform appPlat in WorkSpace.Instance.Solution.ApplicationPlatforms)
                    {
                        EnvApplication envApp = new EnvApplication() { Name = appPlat.AppName, Platform  = appPlat.Platform, ParentGuid = appPlat.Guid};
                        envApp.Active = true;
                        mWizard.apps.Add(envApp);
                    }

                    if (mWizard.apps.Count == 0)
                    {
                        mWizard.apps.Add(new EnvApplication() { Name = "MyApplication", Platform = ePlatformType.NA });
                    }

                    SelectApplicationGrid.DataSourceList = mWizard.apps;
                    break;
            }

        }

        private void xAddAppBtn_Click(object sender, RoutedEventArgs e)
        {
            string newAppName = "NewApp";
            EnvApplication envApp = new EnvApplication() { Name = newAppName };
            if (GingerCore.General.GetInputWithValidation("Add Environment Application", "Application Name:", ref newAppName, null, false, envApp))
            {
                if (mWizard.apps.FirstOrDefault(x => x.Name == newAppName) == null)
                {
                    envApp.Name = newAppName;
                    envApp.Active = true;
                    mWizard.apps.Add(envApp);
                }
            }
        }

        private void AddApplicationToSolution(object sender, RoutedEventArgs e)
        {
            AddApplicationPage applicationPage = new(WorkSpace.Instance.Solution);

            applicationPage.ShowAsWindow();

            foreach (ApplicationPlatform selectedApp in applicationPage.SelectApplicationGrid.Grid.SelectedItems)
            {
                EnvApplication envApp = new EnvApplication() { Name = selectedApp.AppName, Platform = selectedApp.Platform, ParentGuid = selectedApp.Guid, ItemImageType = selectedApp.PlatformImage };
                envApp.Active = true;
                mWizard.apps.Add(envApp);
            }
        }
    }
}
