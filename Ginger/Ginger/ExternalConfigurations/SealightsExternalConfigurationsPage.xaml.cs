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
    /// Interaction logic for SealightsConfiguration.xaml
    /// </summary>
    public partial class SealightsExternalConfigurationsPage : GingerUIPage
    {
        SealightsConfiguration _SealightsConfiguration = null;

        public SealightsExternalConfigurationsPage()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            _SealightsConfiguration = WorkSpace.Instance.Solution.SealightsConfiguration;
            CurrentItemToSave = WorkSpace.Instance.Solution;
            SetControls();
            _SealightsConfiguration.StartDirtyTracking();
        }

        private void SetControls()
        {
            xSealightsLogRadioButton.Init(typeof(SealightsConfiguration.eSealightsLog),
               xSealightsLogPanel, _SealightsConfiguration,
               nameof(SealightsConfiguration.SealightsLog), SealightsLogRadioButton_CheckedHandler);

            Context mContext = new Context();
            xSealightsURLTextBox.Init(mContext, _SealightsConfiguration, nameof(SealightsConfiguration.SealightsURL));
            xSealighsAgentTokenTextBox.Init(mContext, _SealightsConfiguration, nameof(SealightsConfiguration.SealightsAgentToken));
            xSealighsLabIdTextBox.Init(mContext, _SealightsConfiguration, nameof(SealightsConfiguration.SealightsLabId));
            xSealightsTestStageTextBox.Init(mContext, _SealightsConfiguration, nameof(SealightsConfiguration.SealightsTestStage));
            xSealighsBuildSessionIDTextBox.Init(mContext, _SealightsConfiguration, nameof(SealightsConfiguration.SealightsBuildSessionID));
            xSealighsSessionTimeoutTextBox.Init(mContext, _SealightsConfiguration, nameof(SealightsConfiguration.SealightsSessionTimeout));
            xSealighsReportedEntityLevelComboBox.BindControl(_SealightsConfiguration, nameof(SealightsConfiguration.SealightsReportedEntityLevel));
            ApplyValidationRules();
            if (xSealighsSessionTimeoutTextBox.ValueTextBox.Text.Trim() == "")
            {
                xSealighsSessionTimeoutTextBox.ValueTextBox.Text = "14400";
            }
            xSealightsTestRecommendationsRadioButton.Init(typeof(SealightsConfiguration.eSealightsTestRecommendations), xSealightsTestRecommendationsPanel, _SealightsConfiguration, nameof(SealightsConfiguration.SealightsTestRecommendations), SealightsTestRecommendationRadioButton_CheckedHandler);
        }

        private void ApplyValidationRules()
        {
            // check if fields have been populated (font-end validation)
            xSealighsBuildSessionIDTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValueWithDependency(_SealightsConfiguration, nameof(SealightsConfiguration.SealightsLabId), "Lab ID or Build Session ID must be provided"));
            xSealighsLabIdTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValueWithDependency(_SealightsConfiguration, nameof(SealightsConfiguration.SealightsBuildSessionID), "Lab ID or Build Session ID must be provided"));
            xSealightsURLTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("Url cannot be empty"));
            xSealighsAgentTokenTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("Token cannot be empty"));
            xSealightsTestStageTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("Test Stage cannot be empty"));
            xSealighsSessionTimeoutTextBox.ValueTextBox.AddValidationRule(new ValidateNumberInputRule());
            xSealighsReportedEntityLevelComboBox.AddValidationRule(new ValidateEmptyValue("Entity Level cannot be empty"));

            CallSealightsConfigPropertyChange();
        }

        private void CallSealightsConfigPropertyChange()
        {
            // need in order to trigger the validation's rules on init binding (load/init form)
            _SealightsConfiguration.OnPropertyChanged(nameof(SealightsConfiguration.SealightsURL));
            _SealightsConfiguration.OnPropertyChanged(nameof(SealightsConfiguration.SealightsAgentToken));
            _SealightsConfiguration.OnPropertyChanged(nameof(SealightsConfiguration.SealightsLabId));
            _SealightsConfiguration.OnPropertyChanged(nameof(SealightsConfiguration.SealightsTestStage));
            _SealightsConfiguration.OnPropertyChanged(nameof(SealightsConfiguration.SealightsBuildSessionID));
            _SealightsConfiguration.OnPropertyChanged(nameof(SealightsConfiguration.SealightsReportedEntityLevel));
        }

        private void xSaveButton_Click(object sender, RoutedEventArgs e)
        {
            WorkSpace.Instance.Solution.SolutionOperations.SaveSolution(true, SolutionGeneral.Solution.eSolutionItemToSave.LoggerConfiguration);
        }

        private void SealightsLogRadioButton_CheckedHandler(object sender, RoutedEventArgs e)
        {
            string value = ((RadioButton)sender).Tag?.ToString();

            SealightsConfiguration.eSealightsLog sealightsLog;

            Enum.TryParse(value, out sealightsLog);

            if (sealightsLog == SealightsConfiguration.eSealightsLog.Yes)
            {
                xSealightsExecutionLoggerGrid.Visibility = Visibility.Visible;

                // Adding Init and validation in ordre to fix the issue: Validation is not working when Sealights is not Enable on the Init/load Configuration form
                Context mContext = new Context();
                xSealightsURLTextBox.Init(mContext, _SealightsConfiguration, nameof(SealightsConfiguration.SealightsURL));
                xSealighsAgentTokenTextBox.Init(mContext, _SealightsConfiguration, nameof(SealightsConfiguration.SealightsAgentToken));
                xSealighsLabIdTextBox.Init(mContext, _SealightsConfiguration, nameof(SealightsConfiguration.SealightsLabId));
                xSealightsTestStageTextBox.Init(mContext, _SealightsConfiguration, nameof(SealightsConfiguration.SealightsTestStage));
                xSealighsBuildSessionIDTextBox.Init(mContext, _SealightsConfiguration, nameof(SealightsConfiguration.SealightsBuildSessionID));
                xSealighsSessionTimeoutTextBox.Init(mContext, _SealightsConfiguration, nameof(SealightsConfiguration.SealightsSessionTimeout));
                xSealighsReportedEntityLevelComboBox.BindControl(_SealightsConfiguration, nameof(SealightsConfiguration.SealightsReportedEntityLevel));

                ApplyValidationRules();
            }
            else
            {
                xSealightsExecutionLoggerGrid.Visibility = Visibility.Collapsed;
            }
        }
        private void SealightsTestRecommendationRadioButton_CheckedHandler(object sender, RoutedEventArgs e)
        {
            string value = ((RadioButton)sender).Tag?.ToString();

            SealightsConfiguration.eSealightsTestRecommendations sealightsTestRecommendations;

            Enum.TryParse(value, out sealightsTestRecommendations);

            if (sealightsTestRecommendations == SealightsConfiguration.eSealightsTestRecommendations.Yes)
            {
                _SealightsConfiguration.SealightsTestRecommendations = SealightsConfiguration.eSealightsTestRecommendations.Yes;
            }
            else
            {
                _SealightsConfiguration.SealightsTestRecommendations = SealightsConfiguration.eSealightsTestRecommendations.No;
            }
        }
    }
}
