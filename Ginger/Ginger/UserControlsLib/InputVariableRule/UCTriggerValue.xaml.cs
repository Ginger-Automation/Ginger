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
using Amdocs.Ginger.Common;
using Ginger.Variables;
using GingerCore.GeneralLib;
using GingerCore.Variables;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.UserControlsLib
{
    /// <summary>
    /// Interaction logic for UCTriggerValue.xaml
    /// </summary>
    public partial class UCTriggerValue : UserControl, INotifyPropertyChanged
    {
        public static DependencyProperty SelectedSourceVariabelProperty = DependencyProperty.Register("SelectedSourceVariabel", typeof(VariableBase), typeof(UCTriggerValue), new PropertyMetadata(OnSelectedSourceVariabelPropertyChanged));

        public static DependencyProperty TriggerValueProperty = DependencyProperty.Register("TriggerValue", typeof(string), typeof(UCTriggerValue), new PropertyMetadata(OnTriggerValuePropertyChanged));

        public VariableBase SourceVariable
        {
            get { return (VariableBase)GetValue(SelectedSourceVariabelProperty); }
            set { SetValue(SelectedSourceVariabelProperty, value); }
        }

        public string TriggerValue
        {
            get { return (string)GetValue(TriggerValueProperty); }
            set { SetValue(TriggerValueProperty, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public UCTriggerValue()
        {
            InitializeComponent();
        }

        private static void OnSelectedSourceVariabelPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is UCTriggerValue control)
            {
                control.SelectedSourceVariabelPropertyChanged((VariableBase)args.NewValue);
            }
        }

        private void SelectedSourceVariabelPropertyChanged(VariableBase selectedVar)
        {
            TriggerValue = "";
            if (selectedVar != null && selectedVar.VariableType == "Selection List")
            {
                txtNumberValue.ClearControlsBindings();
                xValueExpressionTxtbox.ClearControlsBindings();
                xValueExpressionTxtbox.Visibility = Visibility.Collapsed;
                dateWindow.Visibility = Visibility.Collapsed;
                txtNumberValue.Visibility = Visibility.Collapsed;
                xVariablesValuesComboBox.Visibility = Visibility.Visible;
                xVariablesValuesComboBox.ItemsSource = ((VariableSelectionList)selectedVar).OptionalValuesList.Select(x => x.Value).ToList();
                xVariablesValuesComboBox.SelectionChanged += XVariablesValuesComboBox_SelectionChanged;
                xVariablesValuesComboBox.SelectedValue = selectedVar.Value;
            }

            if (selectedVar != null && selectedVar.VariableType == "String")
            {
                txtNumberValue.ClearControlsBindings();
                xVariablesValuesComboBox.Visibility = Visibility.Collapsed;
                dateWindow.Visibility = Visibility.Collapsed;
                txtNumberValue.Visibility = Visibility.Collapsed;
                xValueExpressionTxtbox.Visibility = Visibility.Visible;
                BindingHandler.ObjFieldBinding(xValueExpressionTxtbox, TextBox.TextProperty, this, nameof(TriggerValue));
                xValueExpressionTxtbox.Text = selectedVar.Value;
            }

            if (selectedVar != null && selectedVar.VariableType == "DateTime")
            {
                txtNumberValue.ClearControlsBindings();
                xValueExpressionTxtbox.ClearControlsBindings();
                xValueExpressionTxtbox.Visibility = Visibility.Collapsed;
                xVariablesValuesComboBox.Visibility = Visibility.Collapsed;
                txtNumberValue.Visibility = Visibility.Collapsed;
                dateWindow.Visibility = Visibility.Visible;
                dpDate.CustomFormat = ((VariableDateTime)selectedVar).DateTimeFormat;
                dpDate.Value = DateTime.Parse(Convert.ToDateTime(((VariableDateTime)selectedVar).Value).ToString(dpDate.CustomFormat, System.Globalization.CultureInfo.InvariantCulture));
                dpDate.MinDate = Convert.ToDateTime(((VariableDateTime)selectedVar).MinDateTime);
                dpDate.MaxDate = Convert.ToDateTime(((VariableDateTime)selectedVar).MaxDateTime);
            }

            if (selectedVar != null && selectedVar.VariableType == "Number")
            {
                xValueExpressionTxtbox.ClearControlsBindings();
                xValueExpressionTxtbox.Visibility = Visibility.Collapsed;
                xVariablesValuesComboBox.Visibility = Visibility.Collapsed;
                dateWindow.Visibility = Visibility.Collapsed;
                txtNumberValue.Visibility = Visibility.Visible;
                BindingHandler.ObjFieldBinding(txtNumberValue, TextBox.TextProperty, this, nameof(TriggerValue));
                txtNumberValue.AddValidationRule(new NumberValidationRule());
                txtNumberValue.AddValidationRule(new NumberValidationRule((VariableNumber)selectedVar));
                txtNumberValue.Text = selectedVar.Value;
            }
        }


        private void SetTriggerValue()
        {

        }

        private static void OnTriggerValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is UCTriggerValue control)
            {
                control.TriggerValuePropertyChanged((string)args.NewValue);
            }
        }

        private void TriggerValuePropertyChanged(string newValue)
        {
            if (SourceVariable != null)
            {
                if (SourceVariable.VariableType == "Selection List")
                {
                    xVariablesValuesComboBox.SelectedValue = newValue;
                }
                else if (SourceVariable.VariableType == "String")
                {
                    xValueExpressionTxtbox.Text = newValue;
                }
                else if (SourceVariable.VariableType == "DateTime" && !string.IsNullOrEmpty(newValue))
                {
                    dpDate.Value = DateTime.Parse(Convert.ToDateTime(newValue).ToString(((VariableDateTime)SourceVariable).DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture));
                }
                else if (SourceVariable.VariableType == "Number")
                {
                    txtNumberValue.Text = newValue;
                }
            }
            OnPropertyChanged(nameof(TriggerValue));
        }

        private void XVariablesValuesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.Items.Count > 0 && (comboBox).SelectedItem != null)
            {
                string var = (comboBox).SelectedItem.ToString();
                TriggerValue = var;
            }
        }

        private void dpDate_TextChanged(object sender, EventArgs e)
        {
            VariableDateTime vdt = (VariableDateTime)SourceVariable;
            if (!(vdt.CheckDateTimeWithInRange(dpDate.Value.ToString())))
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Input Value is not in range:- Maximum date :[{vdt.MaxDateTime}], Minimum Date:[{vdt.MinDateTime}]");
                dpDate.Focus();
                return;
            }
            else
            {
                TriggerValue = Convert.ToDateTime(dpDate.Value).ToString(vdt.DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public static DataTemplate GetTemplate(string sourcevariable, string triggervalue)
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory triggerValue = new FrameworkElementFactory(typeof(UCTriggerValue));

            Binding selectedsourceVariableBinding = new Binding(sourcevariable)
            {
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            triggerValue.SetBinding(UCTriggerValue.SelectedSourceVariabelProperty, selectedsourceVariableBinding);

            Binding triggerValuebinding = new Binding(triggervalue)
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            triggerValue.SetBinding(UCTriggerValue.TriggerValueProperty, triggerValuebinding);

            template.VisualTree = triggerValue;
            return template;
        }

    }
}
