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

using Amdocs.Ginger.Common;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GingerCore;
using Ginger.UserControls;
using amdocs.ginger.GingerCoreNET;
using System.Windows.Input;
using System.Text.RegularExpressions;
using Ginger.ValidationRules;
using Ginger.UserControlsLib;

namespace Ginger.Configurations
{
    /// <summary>
    /// Interaction logic for ApplitoolsExternalConfigurationsPage.xaml
    /// </summary>
    public partial class ApplitoolsExternalConfigurationsPage : GingerUIPage
    {
        ApplitoolsConfiguration _ApplitoolsConfiguration = new ApplitoolsConfiguration();

        public ApplitoolsExternalConfigurationsPage()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            _ApplitoolsConfiguration = WorkSpace.Instance.Solution.ApplitoolsConfiguration;
            CurrentItemToSave = WorkSpace.Instance.Solution;
            SetControls();
            _ApplitoolsConfiguration.StartDirtyTracking();
        }

        private void SetControls()
        {
            Context mContext = new Context();
            xAPIURLTextBox.Init(mContext, _ApplitoolsConfiguration, nameof(ApplitoolsConfiguration.ApiUrl));
            xAPIKeyTextBox.Init(mContext, _ApplitoolsConfiguration, nameof(ApplitoolsConfiguration.ApiKey));
            ApplyValidationRules();
        }

        private void ApplyValidationRules()
        {
            // check if fields have been populated (font-end validation)
            xAPIURLTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("URL cannot be empty"));
            xAPIKeyTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("Key cannot be empty"));

            CallApplitoolsConfigPropertyChange();
        }

        private void CallApplitoolsConfigPropertyChange()
        {
            // need in order to trigger the validation's rules on init binding (load/init form)
            _ApplitoolsConfiguration.OnPropertyChanged(nameof(ApplitoolsConfiguration.ApiUrl));
            _ApplitoolsConfiguration.OnPropertyChanged(nameof(ApplitoolsConfiguration.ApiKey));
        }

        private void xSaveButton_Click(object sender, RoutedEventArgs e)
        {
            WorkSpace.Instance.Solution.SolutionOperations.SaveSolution(true, SolutionGeneral.Solution.eSolutionItemToSave.LoggerConfiguration);
        }
    }
}
