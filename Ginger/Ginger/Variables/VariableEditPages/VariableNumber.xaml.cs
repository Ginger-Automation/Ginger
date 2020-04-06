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
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Ginger.Variables
{
    /// <summary>
    /// Interaction logic for VariableNumber.xaml
    /// </summary>
    public partial class VariableNumberPage : Page
    {
        public VariableNumberPage(VariableNumber varNumber)
        {
            InitializeComponent();
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtNumberValue, TextBox.TextProperty, varNumber, nameof(VariableNumber.InitialNumberValue));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(rdoInputInt,RadioButton.IsCheckedProperty ,varNumber, nameof(VariableNumber.IsIntegerValue));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtMinValue, TextBox.TextProperty, varNumber, nameof(VariableNumber.MinValue));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtMaxValue, TextBox.TextProperty, varNumber, nameof(VariableNumber.MaxValue));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtPrecisionValue, TextBox.TextProperty, varNumber, nameof(VariableNumber.PrecisionValue));
            SetNummericType();
        }

        private void SetNummericType()
        {
            if(new VariableNumber().IsIntegerValue)
            {
                rdoInputInt.IsChecked = true;
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
        }

        private void rdoInputInt_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            pnlPrecision.Visibility = System.Windows.Visibility.Collapsed;
        }

        //private void txtNumberValue_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        //{
        //  var variableNumber =  new VariableNumber();
        //    double retNum;
        //    bool isNum = Double.TryParse(txtNumberValue.Text, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
        //    if(isNum)
        //    {
        //        variableNumber.InitialNumberValue = Convert.ToDouble(txtNumberValue.Text);
        //    }
        //    else
        //    {
        //        e.Handled = false;
        //    }
            
        //}
        //private void txtNumberValue_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Regex regex = new Regex("[^0-9]+");
        //    e.Handled = regex.IsMatch(txtNumberValue.Text);
        //}
    }
}
