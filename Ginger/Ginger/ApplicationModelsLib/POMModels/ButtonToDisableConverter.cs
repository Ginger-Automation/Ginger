using Amdocs.Ginger.Common.UIElement;
using System;
using System.Windows.Data;

namespace Ginger.ApplicationModelsLib
{
    public class ButtonToDisableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isEnabled = false;
            if(value != null)
            {
                eElementType eType = (eElementType)value;
                if (eType == eElementType.TextBox || eType == eElementType.Text ||
                    eType == eElementType.ComboBox || eType == eElementType.ComboBoxOption ||
                    eType == eElementType.List || eType == eElementType.ListItem)
                {
                    isEnabled = true;
                }
            }
            return isEnabled;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
