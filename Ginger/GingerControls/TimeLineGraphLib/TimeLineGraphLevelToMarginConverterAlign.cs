using GingerUtils.TimeLine;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TimeLineControl
{
    public class TimeLineGraphLevelToMarginConverterAlign : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // indent -19 pixel per level so all items will be in same line
            return new Thickness(((TimeLineEvent)value).Level * -19, 0, 0, 0);  
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
