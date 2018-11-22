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
    /// Interaction logic for VariableStringPage.xaml
    /// </summary>
    public partial class VariableDynamicPage : Page
    {
        private VariableDynamic mVariableDynamic;

        public VariableDynamicPage(VariableDynamic var)
        {            
            InitializeComponent();
            mVariableDynamic = var;
            App.ObjFieldBinding(ValueExpressionTextBox, TextBox.TextProperty, var, nameof(VariableDynamic.ValueExpression));
            ValueExpressionTextBox.Init(mVariableDynamic, nameof(VariableDynamic.ValueExpression));
            mVariableDynamic.PropertyChanged +=mVariableDynamic_PropertyChanged; 
        }

        private void mVariableDynamic_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == VariableBase.Fields.Formula)
            {
                ValueExpressionTextBox.Init(mVariableDynamic, nameof(VariableDynamic.ValueExpression));
            }
        }
    }
}
