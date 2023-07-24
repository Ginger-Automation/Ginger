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
using Amdocs.Ginger.Repository;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Ginger.UserControlsLib.POMLearnig
{
    internal class ValidateCustomRelativeXpath : ValidationRule
    {

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var row = ((System.Windows.Data.BindingGroup)value).Items[0] as CustomRelativeXpathTemplate;
            Regex rexXpathReg = new Regex("\\/\\/(\\*|[a-zA-Z]*)\\[[^[(){}\\]]*\\]$", RegexOptions.Compiled);
            Regex iOSPredStrRegex = new Regex(@"([a-zA-Z0-9= '\[\]\{\}]*)|\{([a-zA-Z0-9]*)\}");

            if (row.Value != null && rexXpathReg.IsMatch(row.Value))
            {
                row.Status = CustomRelativeXpathTemplate.SyntaxValidationStatus.Passed;
                return new ValidationResult(true, "Valid Relative XPath Format");
            }
            else if (row.Value != null && iOSPredStrRegex.IsMatch(row.Value))
            {
                row.Status = CustomRelativeXpathTemplate.SyntaxValidationStatus.Passed;
                return new ValidationResult(true, "Valid iOS Predicate String Format");
            }
            else
            {
                row.Status = CustomRelativeXpathTemplate.SyntaxValidationStatus.Failed;
                return new ValidationResult(false, "Invalid relative xpath format");

            }
        }
    }
}