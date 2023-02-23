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

using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace Ginger.UserControls
{
    public class GridColView
    {
        public GridColView()
        {
            CellTemplate = null;
            CellValuesList = null;            
            HorizontalAlignment = HorizontalAlignment.Left;
        }

        public enum eGridColStyleType
        {
            Text,
            CheckBox,
            ComboBox,
            Link,
            Template,            
            DataColGrid,
            Image,
            ImageMaker,
        }

        public string Field { get; set; }             
        public eGridColStyleType? StyleType { get; set; }
        public DataTemplate CellTemplate { get; set; }

        public string ComboboxDisplayMemberField { get; set; }
        public string ComboboxSelectedValueField { get; set; }
        public string ComboboxSortBy { get; set; }

        // For Combo we can provide list of strings or list of ComboEnumItem
        // Cell Value list for combo is value+description

        // Cell ValuesList can be list of the following types which are handled in UCGrid
        // can be simple List<string>
        // or List<ComboEnumItem>
        public IEnumerable CellValuesList { get; set; }
        public BindingMode? BindingMode { get; set; }

        public string Header { get; set; }   
        public double? MaxWidth { get; set; }
        public double? WidthWeight { get; set; }
        public int? Order { get; set; }
        public bool? Visible { get; set; }
        public bool? ReadOnly { get; set; }
        public ListSortDirection? SortDirection { get; set; }
        public string BindImageCol { get; set; }
        public HorizontalAlignment HorizontalAlignment { get; set; }
        public ColumnPropertyConverter PropertyConverter { get; set; }
        public bool? AllowSorting { get; set; }

        public Style Style { get; set; }
    }

    public class ColumnPropertyConverter
    {
        public ColumnPropertyConverter(IValueConverter converter, DependencyProperty property)
        {
            Converter = converter;
            Property = property;
        }

        public IValueConverter Converter { get; set; }
        public DependencyProperty Property { get; set; }
    }
}