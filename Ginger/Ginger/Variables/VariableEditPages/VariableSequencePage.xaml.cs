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

using GingerCore.Variables;
using System.Windows.Controls;

namespace Ginger.Variables
{
    /// <summary>
    /// Interaction logic for VariableRandomNumerPage.xaml
    /// </summary>
    public partial class VariableSequencePage : Page
    {
        public VariableSequencePage(VariableSequence var)
        {
            InitializeComponent();

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtMinValue, TextBox.TextProperty, var, nameof(VariableSequence.Min));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtMaxValue, TextBox.TextProperty, var, nameof(VariableSequence.Max));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(txtInterval, TextBox.TextProperty, var, nameof(VariableSequence.Interval));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(cbInteger, CheckBox.IsCheckedProperty, var, nameof(VariableSequence.IsInteger));
        }
    }
}
