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

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.ComponentModel;

namespace Ginger.UserControlsLib.PieChart
{
    /// <summary>
    /// Obtain the value of the property from the item, which is currently displayed by the pie chart.
    /// </summary>
    [ValueConversion(typeof(object), typeof(string))]
    public class LegendConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            // the item which we are displaying is bound to the Tag property
            TextBlock label = (TextBlock)value;
            object item = label.Tag;

            // find the item container
            DependencyObject container = (DependencyObject)Helpers.FindElementOfTypeUp((Visual)value, typeof(ListBoxItem));

            // locate the items control which it belongs to
            ItemsControl owner = ItemsControl.ItemsControlFromItemContainer(container);

            // locate the legend
            Legend legend = (Legend)Helpers.FindElementOfTypeUp(owner, typeof(Legend));
            
            PropertyDescriptorCollection filterPropDesc = TypeDescriptor.GetProperties(item);
            object itemValue = filterPropDesc[legend.PlottedProperty].GetValue(item);
            return itemValue;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
