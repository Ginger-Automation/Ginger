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

using amdocs.ginger.GingerCoreNET;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.ValidationRules
{
    public class TextBoxEmptyAndDependentValidationRule : ValidationRule
    {
        private string _Message = string.Empty;
        private object _ConfigurationObj = null;
        private string _DependentObjField = string.Empty;
        private string _SourceObjField = string.Empty;

        public TextBoxEmptyAndDependentValidationRule(object configurationObj, string sourceObjField, string message = "Value must be provided")
        {
            _Message = message;
            _ConfigurationObj = configurationObj;
            _SourceObjField = sourceObjField;

            this.ValidatesOnTargetUpdated = true; // Trigger the validation on init binding (load/init form)
            //this.ValidationStep = ValidationStep.UpdatedValue; // force the rule to run after the new value is converted and written back (fix for issue: property not updated/binded on empty value)
        }

        public TextBoxEmptyAndDependentValidationRule(object configurationObj, string sourceObjField, string dependentObjField, string message = "Value must be provided")
        {
            _Message = message;
            _ConfigurationObj = configurationObj;
            _SourceObjField = sourceObjField;
            _DependentObjField = dependentObjField;

            this.ValidatesOnTargetUpdated = true; // Trigger the validation on init binding (load/init form)
            //this.ValidationStep = ValidationStep.UpdatedValue; // force the rule to run after the new value is converted and written back (fix for issue: property not updated/binded on empty value)
        }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {        
            // getting the user's new input value via reflection
            //value = _ConfigurationObj.GetType().GetProperty(_SourceObjField)?.GetValue(_ConfigurationObj)?.ToString();

            //// on init binding, if value is empty/blank ("") then Value = null 
            //// if user changes the value to blank than value will be object of type: System.Windows.Data.BindingExpressionBase
            //// (we are getting the object type BindingExpressionBase because of the setting: this.ValidationStep = ValidationStep.UpdatedValue;)
            //if (value != null) 
            //{

            //    var target = ((System.Windows.Data.BindingExpressionBase)value).Target; // get the UI object (textbox)

            //    if (target.GetType() == typeof(System.Windows.Controls.TextBox))
            //    {
            //        value = ((System.Windows.Controls.TextBox)target).Text;
            //    }
            //    else if (target.GetType() == typeof(System.Windows.Controls.ComboBox))
            //    {
            //        value = ((System.Windows.Controls.ComboBox)target).Text;
            //    }
            //}

            if (_ConfigurationObj != null && !string.IsNullOrEmpty(_DependentObjField))
            {
                if (String.IsNullOrEmpty(value?.ToString()) && String.IsNullOrEmpty(_ConfigurationObj.GetType().GetProperty(_DependentObjField)?.GetValue(_ConfigurationObj)?.ToString()))
                {
                    return new ValidationResult(false, _Message);
                }
                else
                {
                    return new ValidationResult(true, null);
                }
            }
            else if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return new ValidationResult(false, _Message);
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
    
}
