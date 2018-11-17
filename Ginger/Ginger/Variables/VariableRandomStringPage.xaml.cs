#region License
/*
Copyright © 2014-2018 European Support Limited

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

using System.Windows.Controls;
using GingerCore.Variables;

namespace Ginger.Variables
{
    /// <summary>
    /// Interaction logic for VariableRandomNumerPage.xaml
    /// </summary>
    public partial class VariableRandomStringPage : Page
    {
        public VariableRandomStringPage(VariableRandomString var)
        {
            InitializeComponent();

            App.ObjFieldBinding(txtMinValue, TextBox.TextProperty, var, nameof(VariableRandomString.Min));
            App.ObjFieldBinding(txtMaxValue, TextBox.TextProperty, var, nameof(VariableRandomString.Max));
            
            App.ObjFieldBinding(cbDigit, CheckBox.IsCheckedProperty, var, nameof(VariableRandomString.IsDigit));
            App.ObjFieldBinding(cbLower, CheckBox.IsCheckedProperty, var, nameof(VariableRandomString.IsLowerCase));
            App.ObjFieldBinding(cbUpper, CheckBox.IsCheckedProperty, var, nameof(VariableRandomString.IsUpperCase));
            App.ObjFieldBinding(cbLowerCaseAndDigits, CheckBox.IsCheckedProperty, var, nameof(VariableRandomString.IsLowerCaseAndDigits));
            App.ObjFieldBinding(cbUpperCaseAndDigits, CheckBox.IsCheckedProperty, var, nameof(VariableRandomString.IsUpperCaseAndDigits));
        }
    }
}
