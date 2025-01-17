using Amdocs.Ginger.Common.SelfHealingLib;
using Ginger.SolutionGeneral;
using GingerCore.GeneralLib;
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
