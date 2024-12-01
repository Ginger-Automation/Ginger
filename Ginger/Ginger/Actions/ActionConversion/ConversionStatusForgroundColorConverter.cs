#region License
/*
Copyright © 2014-2024 European Support Limited

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
using System.Windows.Media;

namespace Ginger.Actions.ActionConversion
{
    public class ConversionStatusForgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return (Amdocs.Ginger.Common.Enums.eImageType)value switch
                {
                    Amdocs.Ginger.Common.Enums.eImageType.Passed or Amdocs.Ginger.Common.Enums.eImageType.Finish => App.Current.TryFindResource("$PassedStatusColor") as SolidColorBrush,//green
                    Amdocs.Ginger.Common.Enums.eImageType.Running => App.Current.TryFindResource("$PrimaryColor_Black") as SolidColorBrush,//blue
                    Amdocs.Ginger.Common.Enums.eImageType.Pending => App.Current.TryFindResource("$PendingStatusColor") as SolidColorBrush,//orange 
                    Amdocs.Ginger.Common.Enums.eImageType.Stopped or Amdocs.Ginger.Common.Enums.eImageType.Failed => App.Current.TryFindResource("$HighlightColor_Red") as SolidColorBrush,//red
                    _ => App.Current.TryFindResource("$SkippedStatusColor") as SolidColorBrush,//gray
                };
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
