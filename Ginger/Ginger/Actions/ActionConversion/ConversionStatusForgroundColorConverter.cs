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
