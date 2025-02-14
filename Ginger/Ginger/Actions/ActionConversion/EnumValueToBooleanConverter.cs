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

using System;
using System.Windows.Data;

namespace Ginger.Actions.ActionConversion
{
    public class EnumValueToBooleanConverter<TEnum> : IValueConverter where TEnum : struct, Enum
    {
        public TEnum ValueToCheck { get; set; }
        public bool IsCheckEqual { get; set; }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (Enum.TryParse(value?.ToString(), out TEnum enumValue))
            {
                if (IsCheckEqual)
                {
                    return enumValue.Equals(ValueToCheck);
                }
                else
                {
                    return !enumValue.Equals(ValueToCheck);

                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
