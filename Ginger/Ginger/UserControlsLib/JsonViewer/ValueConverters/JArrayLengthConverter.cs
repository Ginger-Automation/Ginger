#region License
/*
Copyright © 2014-2025 European Support Limited

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

using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace JsonViewerDemo.ValueConverters
{
    public sealed class JArrayLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not JToken jToken)
                throw new Exception("Wrong type for this converter");

            switch (jToken.Type)
            {
                case JTokenType.Array:
                    var arrayLen = jToken.Children().Count();
                    return string.Format("[{0}]", arrayLen);
                case JTokenType.Property:
                    var propertyArrayLen = jToken.Children().FirstOrDefault().Children().Count();
                    return string.Format("[ {0} ]", propertyArrayLen);
                default:
                    throw new Exception("Type should be JProperty or JArray");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(GetType().Name + " can only be used for one way conversion.");
        }
    }
}
