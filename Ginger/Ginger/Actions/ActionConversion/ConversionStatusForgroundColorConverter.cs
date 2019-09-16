using Amdocs.Ginger.CoreNET;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Ginger.Actions.ActionConversion
{
    public class ConversionStatusForgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                switch ((Amdocs.Ginger.Common.Enums.eImageType)value)
                {
                    case Amdocs.Ginger.Common.Enums.eImageType.Passed:
                    case Amdocs.Ginger.Common.Enums.eImageType.Finish:
                        return App.Current.TryFindResource("$PassedStatusColor") as SolidColorBrush; //green

                    case Amdocs.Ginger.Common.Enums.eImageType.Running:
                        return App.Current.TryFindResource("$Color_DarkBlue") as SolidColorBrush;//blue

                    case Amdocs.Ginger.Common.Enums.eImageType.Pending:
                        return App.Current.TryFindResource("$PendingStatusColor") as SolidColorBrush;//orange 

                    case Amdocs.Ginger.Common.Enums.eImageType.Stopped:
                    case Amdocs.Ginger.Common.Enums.eImageType.Failed:
                        return App.Current.TryFindResource("$HighlightColor_Red") as SolidColorBrush;//red

                    default:
                        return App.Current.TryFindResource("$SkippedStatusColor") as SolidColorBrush;//gray
                }
            }
            else
            {
                return App.Current.TryFindResource("$SkippedStatusColor") as SolidColorBrush;//gray
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
