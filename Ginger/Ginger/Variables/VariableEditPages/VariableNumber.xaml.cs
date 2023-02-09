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
using Amdocs.Ginger.ValidationRules;
using GingerCore.Variables;
using System;
using System.Windows.Controls;

namespace Ginger.Variables
{
    /// <summary>
    /// Interaction logic for VariableNumber.xaml
    /// </summary>
    public partial class VariableNumberPage : Page
    {
        VariableNumber variableNumber;
        public VariableNumberPage(VariableNumber varNumber)
        {
            variableNumber = varNumber;
            InitializeComponent();

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(rdoInputDecimal, RadioButton.IsCheckedProperty, varNumber, nameof(VariableNumber.IsDecimalValue));

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtMinValue, TextBox.TextProperty, varNumber, nameof(VariableNumber.MinValue));
            txtMinValue.AddValidationRule(new EmptyValidationRule());

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtMaxValue, TextBox.TextProperty, varNumber, nameof(VariableNumber.MaxValue));
            txtMaxValue.AddValidationRule(new EmptyValidationRule());

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtPrecisionValue, TextBox.TextProperty, varNumber, nameof(VariableNumber.PrecisionValue));
            txtPrecisionValue.AddValidationRule(new EmptyValidationRule());

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtNumberValue, TextBox.TextProperty, varNumber, nameof(VariableNumber.InitialNumberValue));
            txtNumberValue.AddValidationRule(new NumberValidationRule());
            txtNumberValue.AddValidationRule(new NumberValidationRule(variableNumber));


            SetNummericType();

        }

        private void SetNummericType()
        {
            if (!variableNumber.IsDecimalValue)
            {
                rdoInputInt.IsChecked = true;
                pnlPrecision.Visibility = System.Windows.Visibility.Collapsed;
                if (string.IsNullOrEmpty(variableNumber.MinValue))
                {
                    variableNumber.MinValue = Int32.MinValue.ToString();
                }

                if (string.IsNullOrEmpty(variableNumber.MaxValue))
                {
                    variableNumber.MaxValue = Int32.MaxValue.ToString();
                }
            }
            else
            {
                rdoInputDecimal.IsChecked = true;
                pnlPrecision.Visibility = System.Windows.Visibility.Visible;
            }

        }

        private void SetNumericVaueOnRediobuttonChange()
        {
            variableNumber.Value = txtNumberValue.Text;
            variableNumber.InitialNumberValue = variableNumber.Value;
        }

        private void rdoInputDecimal_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            pnlPrecision.Visibility = System.Windows.Visibility.Visible;
            variableNumber.MinValue = float.MinValue.ToString();
            variableNumber.MaxValue = float.MaxValue.ToString();
            if (variableNumber.PrecisionValue == null)
            {
                variableNumber.PrecisionValue = "2"; //default
            }
            SetNumericVaueOnRediobuttonChange();
        }

        private void rdoInputInt_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            pnlPrecision.Visibility = System.Windows.Visibility.Collapsed;
            variableNumber.MinValue = Int32.MinValue.ToString();
            variableNumber.MaxValue = Int32.MaxValue.ToString();
            SetNumericVaueOnRediobuttonChange();
        }


    }
       
}
