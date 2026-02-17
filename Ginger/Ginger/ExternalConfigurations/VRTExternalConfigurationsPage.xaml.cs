#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Ginger.UserControlsLib;
using Ginger.ValidationRules;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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

            if (!string.IsNullOrEmpty(_VRTConfiguration.ApiKey))  
            {
                xAPIKeyTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("API Key cannot be empty"));

                // Now that validation is attached, remove the binding so the real key won't show
                BindingOperations.ClearBinding(xAPIKeyTextBox.ValueTextBox, TextBox.TextProperty);

                // Show a generic mask; don't leak length
                const string masked = "••••••••••••••••••••";
                xAPIKeyTextBox.ValueTextBox.Text = masked;
                xAPIKeyTextBox.ValueTextBox.Tag = masked; // remember the mask marker
            }

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
            // Preserve existing key unless user edited the field
            var apiKeyBox = xAPIKeyTextBox.ValueTextBox;
            string masked = apiKeyBox.Tag as string;

            if (!string.IsNullOrEmpty(masked) && !string.Equals(apiKeyBox.Text, masked))
            {
                if (string.IsNullOrWhiteSpace(apiKeyBox.Text))
                {
                    Reporter.ToLog(eLogLevel.ERROR, "API Key cannot be empty");
                    apiKeyBox.Text = masked;
                    return;
                }
                _VRTConfiguration.ApiKey = apiKeyBox.Text;  // assign new key
                apiKeyBox.Text = masked;                     // re-mask after save
            }
            // else: field untouched or no mask in use -> keep existing _VRTConfiguration.ApiKey

            WorkSpace.Instance.Solution.SolutionOperations.SaveSolution(true, SolutionGeneral.Solution.eSolutionItemToSave.LoggerConfiguration);
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
