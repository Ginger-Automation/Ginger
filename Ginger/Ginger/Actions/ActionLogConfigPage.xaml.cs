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

using Amdocs.Ginger.Common.Actions;
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

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActionLogConfigPage.xaml
    /// </summary>
    public partial class ActionLogConfigPage : Page
    {
        public ActionLogConfigPage(ActionLogConfig actionLogConfig)
        {
            InitializeComponent();

            ActionLogTextBox.BindControl(actionLogConfig, nameof(ActionLogConfig.ActionLogText));
            LogInputVariablesCheckBox.BindControl(actionLogConfig, nameof(ActionLogConfig.LogInputVariables));
            LogOuputVariablesCheckBox.BindControl(actionLogConfig, nameof(ActionLogConfig.LogOutputVariables));
            LogRunStatusCheckBox.BindControl(actionLogConfig, nameof(ActionLogConfig.LogRunStatus));
            LogErrorCheckBox.BindControl(actionLogConfig, nameof(ActionLogConfig.LogError));
            LogElapsedTimeCheckBox.BindControl(actionLogConfig, nameof(ActionLogConfig.LogElapsedTime));
        }

    }
}
