#region License
/*
Copyright © 2014-2022 European Support Limited

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

namespace Ginger.Configurations
{
    /// <summary>
    /// Interaction logic for ExecutionResultsConfiguration.xaml
    /// </summary>
    public partial class ExternalConfigurationsPage : Page
    {
        VRTConfiguration _VRTConfiguration = new VRTConfiguration();

        public ExternalConfigurationsPage()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            _VRTConfiguration = WorkSpace.Instance.Solution.VRTConfiguration;
            _VRTConfiguration.StartDirtyTracking();
            SetControls();
        }

        private void SetControls()
        {
            Context mContext = new Context();
            xAPIURLTextBox.Init(mContext, _VRTConfiguration, nameof(VRTConfiguration.ApiUrl));
            xAPIKeyTextBox.Init(mContext, _VRTConfiguration, nameof(VRTConfiguration.ApiKey));
            xProjectTextBox.Init(mContext, _VRTConfiguration, nameof(VRTConfiguration.Project));
            xBranchNameTextBox.Init(mContext, _VRTConfiguration, nameof(VRTConfiguration.BranchName));
            xEnableSoftAssertRadioButton.Init(typeof(VRTConfiguration.eEnableSoftAssert),
                xEnableSoftAssertPanel, _VRTConfiguration,
                nameof(VRTConfiguration.EnableSoftAssert));
            ApplyValidationRules();
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xVRTExpander, Expander.VisibilityProperty, WorkSpace.Instance.UserProfile, nameof(WorkSpace.Instance.UserProfile.ShowEnterpriseFeatures), bindingConvertor: new GingerCore.GeneralLib.BoolVisibilityConverter(), BindingMode: System.Windows.Data.BindingMode.OneWay);

            // this added in ordre to apply the validation when turnining ON the Enterprise Features Flag (the validation wont apply when the Enterprise Features Flag initially was OFF)
            xVRTExpander.IsVisibleChanged += XVRTExpander_IsVisibleChanged;
        }

        private void XVRTExpander_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ApplyValidationRules();
        }

        private void ApplyValidationRules()
        {
            // check if fields have been populated (font-end validation)
            xAPIURLTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("Url cannot be empty"));
            xAPIKeyTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("Key cannot be empty"));
            xProjectTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("Project Name cannot be empty"));

            CallSealightsConfigPropertyChange();
        }

        private void CallSealightsConfigPropertyChange()
        {
            // need in order to trigger the validation's rules on init binding (load/init form)
            _VRTConfiguration.OnPropertyChanged(nameof(VRTConfiguration.ApiUrl));
            _VRTConfiguration.OnPropertyChanged(nameof(VRTConfiguration.ApiKey));
            _VRTConfiguration.OnPropertyChanged(nameof(VRTConfiguration.Project));
        }

        private void xSaveButton_Click(object sender, RoutedEventArgs e)
        {
            WorkSpace.Instance.Solution.SolutionOperations.SaveSolution(true, SolutionGeneral.Solution.eSolutionItemToSave.LoggerConfiguration);
        }

    }
}
