#region License
/*
Copyright © 2014-2026 European Support Limited

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

using Ginger.UserControls;
using GingerCore.Environments;
using GingerWPF.WizardLib;
using System.Windows.Controls;

namespace Ginger.Environments.GingerOpsEnvWizardLib
{
    /// <summary>
    /// Interaction logic for GingerOpsApplicationPage.xaml
    /// </summary>
    public partial class GingerOpsApplicationPage : Page, IWizardPage
    {
        AddGingerOpsEnvWizard mWizard;

        public GingerOpsApplicationPage()
        {
            InitializeComponent();
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = nameof(ProjEnvironment.Name), Header = "Environment", WidthWeight = 40 },
                new GridColView() { Field = nameof(ProjEnvironment.GingerOpsStatus), Header = "Status", WidthWeight = 40 },
                new GridColView() { Field = nameof(ProjEnvironment.GingerOpsRemark), Header = "Remark", WidthWeight = 60 },
            ]
            };
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
                    mWizard = (AddGingerOpsEnvWizard)WizardEventArgs.Wizard;
                    break;
                case EventType.Active:
                    SelectApplicationGrid.DataSourceList = mWizard.ImportedEnvs;
                    break;
                default:
                    break;
            }

        }

    }
}
