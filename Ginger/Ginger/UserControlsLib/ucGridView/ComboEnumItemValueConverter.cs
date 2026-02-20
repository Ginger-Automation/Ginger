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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using GingerCore;
using GingerCore.GeneralLib; // for ComboEnumItem or adjust namespace if different

namespace Ginger.UserControlsLib.ucGridView
{
    /// <summary>
    /// Converts between the model value (either description string or enum object) and the ComboEnumItem.Value used by the ComboBox.
    /// ConverterParameter: "StoreDescription" (default) or "StoreEnum"
    /// </summary>
    public class ComboEnumItemValueConverter : IValueConverter
    {
        private readonly List<ComboEnumItem> _items;

        public ComboEnumItemValueConverter(List<ComboEnumItem> items)
        {
            _items = items ?? new List<ComboEnumItem>();
        }

        // Model -> Combo SelectedValue (enum/object)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            // If model already holds enum value -> return matching enum value (or itself)
            var byValue = _items.FirstOrDefault(i => Equals(i.Value, value));
            if (byValue != null) return byValue.Value;

            // If model holds the description string -> find item by text
            if (value is string s)
            {
                var byText = _items.FirstOrDefault(i => string.Equals(i.text, s, StringComparison.CurrentCultureIgnoreCase));
                if (byText != null) return byText.Value;
            }

            // Try to match by enum name if model contains enum name in string form
            var asString = value.ToString();
            var byName = _items.FirstOrDefault(i => string.Equals(i.Value?.ToString(), asString, StringComparison.CurrentCultureIgnoreCase));
            if (byName != null) return byName.Value;

            return value;
        }

        // Combo SelectedValue (enum/object) -> Model (description string or enum, based on parameter)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            var item = _items.FirstOrDefault(i => Equals(i.Value, value));
            string mode = (parameter as string) ?? "StoreDescription";

            if (item != null)
            {
                if (string.Equals(mode, "StoreEnum", StringComparison.OrdinalIgnoreCase))
                {
                    // return the enum/object itself
                    return item.Value;
                }
                else
                {
                    // default: return the enum description text to store in a string property
                    return item.text;
                }
            }

            // fallback: if parameter asks for enum and value is enum-like, return it
            if (string.Equals(mode, "StoreEnum", StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }

            // fallback: return value.ToString()
            return value.ToString();
        }
    }
}