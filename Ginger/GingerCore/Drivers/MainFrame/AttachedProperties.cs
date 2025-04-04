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

using System.Windows;
using System.Windows.Controls;

namespace GingerCore.Drivers.MainFrame
{
    public class AttachedProperties
    {
        public static int GetCaretLocation(DependencyObject obj)
        {
            return (int)obj.GetValue(CaretLocationProperty);
        }

        public static void SetCaretLocation(DependencyObject obj, int value)
        {
            obj.SetValue(CaretLocationProperty, value);
        }

        public static readonly DependencyProperty CaretLocationProperty =
            DependencyProperty.RegisterAttached("CaretLocation", typeof(int), typeof(AttachedProperties), new PropertyMetadata(new PropertyChangedCallback(CaretChanged)));

        private static void CaretChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox tb)
            {
                tb.CaretIndex = (int)e.NewValue;
            }
        }
    }
}