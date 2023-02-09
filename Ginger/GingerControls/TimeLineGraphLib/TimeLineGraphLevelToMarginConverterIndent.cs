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
