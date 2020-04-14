#region License
/*
Copyright © 2014-2020 European Support Limited

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
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtNumberValue, TextBox.TextProperty, varNumber, nameof(VariableNumber.InitialNumberValue));
            txtNumberValue.AddValidationRule(new NumberValidationRule());
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(rdoInputDecimal, RadioButton.IsCheckedProperty, varNumber, nameof(VariableNumber.IsDecimalValue));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtMinValue, TextBox.TextProperty, varNumber, nameof(VariableNumber.MinValue));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtMaxValue, TextBox.TextProperty, varNumber, nameof(VariableNumber.MaxValue));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtPrecisionValue, TextBox.TextProperty, varNumber, nameof(VariableNumber.PrecisionValue));
            SetNummericType();

            if (variableNumber.InitialNumberValue != null)
            {
                txtNumberValue.Text = variableNumber.InitialNumberValue;
            }
        }

        private void SetNummericType()
        {
            if (!variableNumber.IsDecimalValue)
            {
                rdoInputInt.IsChecked = true;
                pnlPrecision.Visibility = System.Windows.Visibility.Collapsed;
                if (variableNumber.MinValue == null)
                {
                    variableNumber.MinValue = Int32.MinValue;
                }

                if (variableNumber.MaxValue == null)
                {
                    variableNumber.MaxValue = Int32.MaxValue;
                }

            }
            else
            {
                rdoInputDecimal.IsChecked = true;
                pnlPrecision.Visibility = System.Windows.Visibility.Visible;
            }

        }

        private void rdoInputDecimal_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            pnlPrecision.Visibility = System.Windows.Visibility.Visible;
            variableNumber.MinValue = float.MinValue;
            variableNumber.MaxValue = float.MaxValue;
            if (variableNumber.PrecisionValue == null)
            {
                variableNumber.PrecisionValue = 2; //default
            }
        }

        private void rdoInputInt_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            pnlPrecision.Visibility = System.Windows.Visibility.Collapsed;
            variableNumber.MinValue = Int32.MinValue;
            variableNumber.MaxValue = Int32.MaxValue;
        }

        private void txtNumberValue_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            //txtNumberValue.BindControl(variableNumber,nameof(VariableNumber.InitialNumberValue));
            

            //double retNum;
            //bool isNum = double.TryParse(txtNumberValue.Text, System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            //if (isNum)
            //{
            //    //variableNumber.InitialNumberValue = txtNumberValue.Text;
            //    if (!variableNumber.IsDecimalValue)
            //    {
            //        bool isInt = double.TryParse(txtNumberValue.Text, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            //        if (isInt)
            //        {
            //            variableNumber.InitialNumberValue = Convert.ToInt32(txtNumberValue.Text).ToString();
            //        }
            //    }
            //    else
            //    {
            //        variableNumber.InitialNumberValue = txtNumberValue.Text;
            //    }

            //}
            //txtNumberValue.ClearControlsBindings();
        }
    }
       
}
