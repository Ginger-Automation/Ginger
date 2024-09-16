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
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Ginger.Environments.AddEnvironmentWizardLib;

namespace Ginger.Environments.GingerAnalyticsEnvWizardLib
{
    /// <summary>
    /// Interaction logic for GingerAnalyticsApplicationPage.xaml
    /// </summary>
    public partial class GingerAnalyticsApplicationPage : Page, IWizardPage
    {
        AddGingerAnalyticsEnvWizard mWizard;

        public GingerAnalyticsApplicationPage()
        {
            InitializeComponent();
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(EnvApplication.Name), Header = "Environment", WidthWeight = 60 });
            view.GridColsView.Add(new GridColView() { Field = nameof(EnvApplication.GingerAnalyticsStatus), Header = "Status", WidthWeight = 40 });
            view.GridColsView.Add(new GridColView() { Field = nameof(EnvApplication.GingerAnalyticsRemark), Header = "Remark", WidthWeight = 40 });
            SelectApplicationGrid.SetAllColumnsDefaultView(view);
            SelectApplicationGrid.InitViewItems();
        }

        private int CountSelectedItems()
        {
            int counter = 0;
            foreach (EnvApplication EnvApplication in SelectApplicationGrid.DataSourceList)
            {
                if (EnvApplication.Active)
                {
                    counter++;
                }
            }
            return counter;
        }


        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (AddGingerAnalyticsEnvWizard)WizardEventArgs.Wizard;
                    SelectApplicationGrid.DataSourceList = mWizard.ImportedEnvs;
                break;
                default:
                    break;
            }

        }

    }
}
