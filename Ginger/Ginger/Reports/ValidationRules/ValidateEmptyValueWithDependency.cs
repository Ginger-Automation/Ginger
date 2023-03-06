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

using amdocs.ginger.GingerCoreNET;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.ValidationRules
{
    public class ValidateEmptyValueWithDependency : ValidationRule
    {
        private string _Message = string.Empty;
        private object _DependencyObj = null;
        private string _DependentObjField = string.Empty;

        public ValidateEmptyValueWithDependency(object dependencyObj, string dependentObjField, string message = "Value must be provided")
        {
            _Message = message;
            _DependencyObj = dependencyObj;
            _DependentObjField = dependentObjField;

            this.ValidatesOnTargetUpdated = true; // Trigger the validation on init binding (load/init form)
            this.ValidationStep = ValidationStep.UpdatedValue; // force the rule to run after the new value is converted and written back (fix for issue: property not updated/binded on empty value)
        }

        private object GetBoundValue(object value)
        {
            if (value is BindingExpression)
            {
                // ValidationStep was UpdatedValue or CommittedValue (validate after setting)
                // Need to pull the value out of the BindingExpression.
                BindingExpression binding = (BindingExpression)value;

                // Get the bound object and name of the property
                string resolvedPropertyName = binding.GetType().GetProperty("ResolvedSourcePropertyName", BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance).GetValue(binding, null).ToString();
                object resolvedSource = binding.GetType().GetProperty("ResolvedSource", BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance).GetValue(binding, null);

                // Extract the value of the property
                object propertyValue = resolvedSource.GetType().GetProperty(resolvedPropertyName).GetValue(resolvedSource, null);

                return propertyValue;
            }
            else
            {
                return value;
            }
        }


        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            value = GetBoundValue(value);    

            if (_DependencyObj != null && !string.IsNullOrEmpty(_DependentObjField))
            {
                if (String.IsNullOrEmpty(value?.ToString()) && String.IsNullOrEmpty(_DependencyObj.GetType().GetProperty(_DependentObjField)?.GetValue(_DependencyObj)?.ToString()))
                {
                    return new ValidationResult(false, _Message);
                }
                else
                {
                    return new ValidationResult(true, null);
                }
            }
            else
            {
                return new ValidationResult(false, _Message);
            }
   
        }
    }
    
}
