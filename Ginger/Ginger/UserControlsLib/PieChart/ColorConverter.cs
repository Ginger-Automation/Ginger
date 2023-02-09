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

namespace Ginger.UserControlsLib.PieChart
{
    /// <summary>
    /// Converter which uses the IColorSelector associated with the Legend to
    /// select a suitable color for rendering an item.
    /// </summary>
    [ValueConversion(typeof(object), typeof(Brush))]
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            // find the item 
            FrameworkElement element = (FrameworkElement)value;
            object item = element.Tag;

            // find the item container
            DependencyObject container = (DependencyObject)Helpers.FindElementOfTypeUp(element, typeof(ListBoxItem));

            // locate the items control which it belongs to
            ItemsControl owner = ItemsControl.ItemsControlFromItemContainer(container);

            // locate the legend
            Legend legend = (Legend)Helpers.FindElementOfTypeUp(owner, typeof(Legend));

            CollectionView collectionView = (CollectionView)CollectionViewSource.GetDefaultView(owner.DataContext);

            // locate this item (which is bound to the tag of this element) within the collection
            int index = collectionView.IndexOf(item);

            if (legend.ColorSelector != null)
                return legend.ColorSelector.SelectBrush(item, index);
            else
                return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
