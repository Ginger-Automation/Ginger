using Amdocs.Ginger.Common.UIElement;
using System;
using System.Windows;
using System.Windows.Data;

namespace Ginger.Actions.ActionConversion
{
    public class BooleanToVisiblityCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility result = ((bool)value) ? Visibility.Visible : Visibility.Collapsed;
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
