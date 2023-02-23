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
    /// Interaction logic for VRTExternalConfigurationsPage.xaml
    /// </summary>
    public partial class VRTExternalConfigurationsPage : GingerUIPage
    {
        VRTConfiguration _VRTConfiguration = null;

        public VRTExternalConfigurationsPage()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            _VRTConfiguration = WorkSpace.Instance.Solution.VRTConfiguration;
            CurrentItemToSave = WorkSpace.Instance.Solution;
            SetControls();
            _VRTConfiguration.StartDirtyTracking();
        }

        private void SetControls()
        {
            Context mContext = new Context();
            xAPIURLTextBox.Init(mContext, _VRTConfiguration, nameof(VRTConfiguration.ApiUrl));
            xAPIKeyTextBox.Init(mContext, _VRTConfiguration, nameof(VRTConfiguration.ApiKey));
            xProjectTextBox.Init(mContext, _VRTConfiguration, nameof(VRTConfiguration.Project));
            xBranchNameTextBox.Init(mContext, _VRTConfiguration, nameof(VRTConfiguration.BranchName));
            xDiffToleranceTextBox.Init(mContext, _VRTConfiguration, nameof(VRTConfiguration.DifferenceTolerance));
            xEnableSoftAssertRadioButton.Init(typeof(VRTConfiguration.eFailActionOnCheckpointMismatch), xEnableSoftAssertPanel, _VRTConfiguration, nameof(VRTConfiguration.FailActionOnCheckpointMismatch));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xOsCheckBox, CheckBox.IsCheckedProperty, _VRTConfiguration, nameof(VRTConfiguration.OS));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xAgentCheckBox, CheckBox.IsCheckedProperty, _VRTConfiguration, nameof(VRTConfiguration.Agent));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xEnvironmentCheckBox, CheckBox.IsCheckedProperty, _VRTConfiguration, nameof(VRTConfiguration.Environment));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xViewportCheckBox, CheckBox.IsCheckedProperty, _VRTConfiguration, nameof(VRTConfiguration.Viewport));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xActivityTagsCheckBox, CheckBox.IsCheckedProperty, _VRTConfiguration, nameof(VRTConfiguration.ActivityTags));

            ApplyValidationRules();
        }

        private void ApplyValidationRules()
        {
            // check if fields have been populated (font-end validation)
            xAPIURLTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("URL cannot be empty"));
            xAPIKeyTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("Key cannot be empty"));
            xProjectTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("Project Name cannot be empty"));
            xBranchNameTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("Branch Name cannot be empty"));
            xDiffToleranceTextBox.ValueTextBox.AddValidationRule(new ValidateDecimalNumberInputRule("Enter decimal number only"));

            CallVRTConfigPropertyChange();
        }

        private void CallVRTConfigPropertyChange()
        {
            // need in order to trigger the validation's rules on init binding (load/init form)
            _VRTConfiguration.OnPropertyChanged(nameof(VRTConfiguration.ApiUrl));
            _VRTConfiguration.OnPropertyChanged(nameof(VRTConfiguration.ApiKey));
            _VRTConfiguration.OnPropertyChanged(nameof(VRTConfiguration.Project));
            _VRTConfiguration.OnPropertyChanged(nameof(VRTConfiguration.BranchName));
        }

        private void xSaveButton_Click(object sender, RoutedEventArgs e)
        {
            WorkSpace.Instance.Solution.SolutionOperations.SaveSolution(true, SolutionGeneral.Solution.eSolutionItemToSave.LoggerConfiguration);
        }
    }
}
