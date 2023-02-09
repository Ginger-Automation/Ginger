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
using Amdocs.Ginger.Common.SelfHealingLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ginger.Run
{
    /// <summary>
    /// Interaction logic for GingerSelfHealingConfiguration.xaml
    /// </summary>
    public partial class GingerSelfHealingConfiguration : Page
    {
        GenericWindow genWin = null;
        RunSetConfig mRunSetConfig;

        public GingerSelfHealingConfiguration()
        {
            InitializeComponent();

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xEnableSelfHealingChkBox, CheckBox.IsCheckedProperty, WorkSpace.Instance.AutomateTabSelfHealingConfiguration, nameof(SelfHealingConfig.EnableSelfHealing));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xAutoFixAnalyzerChkBox, CheckBox.IsCheckedProperty, WorkSpace.Instance.AutomateTabSelfHealingConfiguration, nameof(SelfHealingConfig.AutoFixAnalyzerIssue));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xRePrioritizeChkBox, CheckBox.IsCheckedProperty, WorkSpace.Instance.AutomateTabSelfHealingConfiguration, nameof(SelfHealingConfig.ReprioritizePOMLocators));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xAutoUpdateModelChkBox, CheckBox.IsCheckedProperty, WorkSpace.Instance.AutomateTabSelfHealingConfiguration, nameof(SelfHealingConfig.AutoUpdateApplicationModel));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xAutoExecuteInSimulationChkBox, CheckBox.IsCheckedProperty, WorkSpace.Instance.AutomateTabSelfHealingConfiguration, nameof(SelfHealingConfig.AutoExecuteInSimulationMode));

            ShowHideConfigPanel();
        }

        public GingerSelfHealingConfiguration(RunSetConfig runSetConfig)
        {
            InitializeComponent();
            mRunSetConfig = runSetConfig;

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xEnableSelfHealingChkBox, CheckBox.IsCheckedProperty, mRunSetConfig.SelfHealingConfiguration, nameof(SelfHealingConfig.EnableSelfHealing));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xAutoFixAnalyzerChkBox, CheckBox.IsCheckedProperty, mRunSetConfig.SelfHealingConfiguration, nameof(SelfHealingConfig.AutoFixAnalyzerIssue));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xRePrioritizeChkBox, CheckBox.IsCheckedProperty, mRunSetConfig.SelfHealingConfiguration, nameof(SelfHealingConfig.ReprioritizePOMLocators));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xAutoUpdateModelChkBox, CheckBox.IsCheckedProperty, mRunSetConfig.SelfHealingConfiguration, nameof(SelfHealingConfig.AutoUpdateApplicationModel));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xAutoExecuteInSimulationChkBox, CheckBox.IsCheckedProperty, mRunSetConfig.SelfHealingConfiguration, nameof(SelfHealingConfig.AutoExecuteInSimulationMode));

            ShowHideConfigPanel();
        }

        public void ShowAsWindow()
        {
            this.Width = 350;
            this.Height = 350;
            GingerCore.General.LoadGenericWindow(ref genWin, App.MainWindow, Ginger.eWindowShowStyle.Dialog, this.Title, this);
        }

        private void xEnableSelfHealingChkBox_Click(object sender, RoutedEventArgs e)
        {
            ShowHideConfigPanel();
        }

        private void ShowHideConfigPanel()
        {
            if (xEnableSelfHealingChkBox.IsChecked == true)
            {
                xSelfHealingConfigPanel.Visibility = Visibility.Visible;
            }
            else
            {
                xSelfHealingConfigPanel.Visibility = Visibility.Collapsed;
               
                if (mRunSetConfig != null)
                {
                    mRunSetConfig.SelfHealingConfiguration.AutoFixAnalyzerIssue = false;
                    mRunSetConfig.SelfHealingConfiguration.ReprioritizePOMLocators = false;
                    mRunSetConfig.SelfHealingConfiguration.AutoUpdateApplicationModel = false;
                    mRunSetConfig.SelfHealingConfiguration.SaveChangesInSourceControl = false;
                    mRunSetConfig.SelfHealingConfiguration.AutoExecuteInSimulationMode = false;
                }

            }
        }
    }
}
