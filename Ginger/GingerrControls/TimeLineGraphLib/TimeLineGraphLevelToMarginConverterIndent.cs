using GingerUtils.TimeLine;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TimeLineControl
{
    public class TimeLineGraphLevelToMarginConverterIndent : IValueConverter
    {
        // converter for timeline graph to offset/indent the tree node item relative to its level
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // indent +19 pixel per level
            int xOffset = ((TimeLineEvent)value).Level * 19;
            return new Thickness(xOffset, 0, 0, 0);  
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
