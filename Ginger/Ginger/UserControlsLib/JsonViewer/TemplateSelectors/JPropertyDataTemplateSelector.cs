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

using Newtonsoft.Json.Linq;
using System.Windows;
using System.Windows.Controls;

namespace JsonViewerDemo.TemplateSelectors
{
    public sealed class JPropertyDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PrimitivePropertyTemplate { get; set; }
        public DataTemplate ComplexPropertyTemplate { get; set; }
        public DataTemplate ArrayPropertyTemplate { get; set; }
        public DataTemplate ObjectPropertyTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null)
                return null;

            if (container is not FrameworkElement frameworkElement)
                return null;

            var type = item.GetType();
            if (type == typeof(JProperty))
            {
                var jProperty = item as JProperty;
                return jProperty.Value.Type switch
                {
                    JTokenType.Object => frameworkElement.FindResource("ObjectPropertyTemplate") as DataTemplate,
                    JTokenType.Array => frameworkElement.FindResource("ArrayPropertyTemplate") as DataTemplate,
                    _ => frameworkElement.FindResource("PrimitivePropertyTemplate") as DataTemplate,
                };
            }

            var key = new DataTemplateKey(type);
            return frameworkElement.FindResource(key) as DataTemplate;
        }
    }
}
