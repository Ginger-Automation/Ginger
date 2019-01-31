using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Ginger.ApplicationModelsLib
{
    public class ButtonToDisableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isEnabled = false;
            if(value != null && !string.IsNullOrEmpty((string)value))
            {
                string eType = ((string)value).ToLower();
                if (eType.StartsWith("input.text".ToLower()) || eType.StartsWith("select".ToLower()) || 
                    eType.StartsWith("ComboBox".ToLower()) ||eType == "Span".ToLower() || eType == "ul".ToLower())
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
