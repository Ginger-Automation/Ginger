#region License
/*
Copyright © 2014-2019 European Support Limited

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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ginger.GeneralValidationRules
{
    public class FileValidationRule : ValidationRule
    {
        string fileExtension = "";  //default File Extension
     
        public FileValidationRule(string fileExt)
        {
            fileExtension = fileExt;
        }        
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return new ValidationResult(false, "Value cannot be null");
            }
            else if(!System.IO.File.Exists((string)value))
            {
                return new ValidationResult(false, "File not found");
            }
            else if (!((string)value).ToLower().EndsWith(fileExtension.ToLower()))
            {
                return new ValidationResult(false, "File Name Should end with " + fileExtension);
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
}
