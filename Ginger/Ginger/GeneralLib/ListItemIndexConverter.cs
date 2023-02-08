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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace GingerWPF.GeneralLib
{
    // We use this converter in list where we want to show the index number of each row
    public class ListItemIndexConverter : IValueConverter
    {
        // value is a ListViewItem that contains the item - usually our ObseravleList item(s)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {            
            ListViewItem LBI = (ListViewItem)value;
            ListView LB = GetParentOfType<ListView>((ListViewItem)LBI);
            int i = LB.ItemContainerGenerator.IndexFromContainer(LBI);
            i++;  // base 1 counting
            return i.ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { throw new NotImplementedException(); }

        public T GetParentOfType<T>(DependencyObject element) where T : DependencyObject
        {
            Type type = typeof(T);
            if (element == null) return null;
            DependencyObject parent = VisualTreeHelper.GetParent(element);
            if (parent == null && ((FrameworkElement)element).Parent is DependencyObject)
                parent = ((FrameworkElement)element).Parent;
            if (parent == null) return null;
            else if (parent.GetType() == type || parent.GetType().IsSubclassOf(type))
                return parent as T;
            return GetParentOfType<T>(parent);
        }
    }
}