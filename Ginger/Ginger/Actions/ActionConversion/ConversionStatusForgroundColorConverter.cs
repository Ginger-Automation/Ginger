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
                        return (SolidColorBrush)(new BrushConverter().ConvertFrom("#109717"));//green

                    case Amdocs.Ginger.Common.Enums.eImageType.Running:
                        return (SolidColorBrush)(new BrushConverter().ConvertFrom("#152B37"));//blue

                    case Amdocs.Ginger.Common.Enums.eImageType.Pending:
                        return (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFC268"));//orange

                    case Amdocs.Ginger.Common.Enums.eImageType.Stopped:
                    case Amdocs.Ginger.Common.Enums.eImageType.Failed:
                        return (SolidColorBrush)(new BrushConverter().ConvertFrom("#DC3812"));//red

                    default:
                        return (SolidColorBrush)(new BrushConverter().ConvertFrom("#D3D3D3"));//gray
                }
            }
            else
            {
                return (SolidColorBrush)(new BrushConverter().ConvertFrom("#D3D3D3"));//gray
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
