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

using Amdocs.Ginger.Common.SelfHealingLib;
using Ginger.SolutionGeneral;
using GingerCore.GeneralLib;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.SolutionWindows
{
    /// <summary>
    /// Interaction logic for SolutionSelfHealingConfigPage.xaml
    /// </summary>
    public partial class SolutionSelfHealingConfigPage : Page
    {
        private Solution _solution;

        public SolutionSelfHealingConfigPage(Solution solution)
        {
            InitializeComponent();

            _solution = solution;
            BindElements();
            _solution.SelfHealingConfig.StartDirtyTracking();
        }

        private void BindElements()
        {
            BindingHandler.ObjFieldBinding(ByPropertyCheckBox, CheckBox.IsCheckedProperty, _solution.SelfHealingConfig, nameof(SolutionSelfHealingConfig.UsePropertyMatcher));
            BindingHandler.ObjFieldBinding(ByPropertyAcceptableScoreTextBox, TextBox.TextProperty, _solution.SelfHealingConfig, nameof(SolutionSelfHealingConfig.PropertyMatcherAcceptableScore));
            BindingHandler.ObjFieldBinding(ByImageCheckBox, CheckBox.IsCheckedProperty, _solution.SelfHealingConfig, nameof(SolutionSelfHealingConfig.UseImageMatcher));
            BindingHandler.ObjFieldBinding(ByImageAcceptableScoreTextBox, TextBox.TextProperty, _solution.SelfHealingConfig, nameof(SolutionSelfHealingConfig.ImageMatcherAcceptableScore));

            UpdateByPropertyScoreStackPanelVisibility();
            UpdateByImageScoreStackPanelVisibility();
        }

        private void UpdateByPropertyScoreStackPanelVisibility()
        {
            ByPropertyScoreStackPanel.Visibility = ByPropertyCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateByImageScoreStackPanelVisibility()
        {
            ByImageScoreStackPanel.Visibility = ByImageCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ByPropertyCheckBox_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            UpdateByPropertyScoreStackPanelVisibility();
        }

        private void ByImageCheckBox_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            UpdateByImageScoreStackPanelVisibility();
        }

        private void ByPropertyAcceptableScoreTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !(int.TryParse(ByPropertyAcceptableScoreTextBox.Text + e.Text, out int value) && value >= 1 && value <= 100);
        }

        private void ByImageAcceptableScoreTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !(int.TryParse(ByImageAcceptableScoreTextBox.Text + e.Text, out int value) && value >= 1 && value <= 100);
        }
    }
}
