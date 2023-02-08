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

using GingerPlugIns.ActionsLib;
using GingerPlugIns.GeneralLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GingerPlugIns
{
    public static class ExtensionMethods
    {
        public static void BindControl(this TextBox TextBox, ActionParam AP, BindingMode bm = BindingMode.TwoWay)
        {
            ObjFieldBinding(TextBox, TextBox.TextProperty, AP, "Value", bm);
        }

        public static void BindControl(this CheckBox CheckBox, ActionParam AP, BindingMode bm = BindingMode.TwoWay)
        {
            ObjFieldBinding(CheckBox, CheckBox.IsCheckedProperty, AP, "Value", bm);
        }

        public static void BindControl(this ComboBox ComboBox, ActionParam AP, List<ComboBoxListItem> list)
        {
            ComboBox.ItemsSource = list;
            ComboBox.DisplayMemberPath = nameof(ComboBoxListItem.Text);
            ComboBox.SelectedValuePath = nameof(ComboBoxListItem.Value);
            // simple bind to combo which have only text 
            ObjFieldBinding(ComboBox, ComboBox.SelectedValueProperty , AP, "Value", BindingMode.TwoWay);
        }
        
        private static void ObjFieldBinding(System.Windows.Controls.Control control, DependencyProperty dependencyProperty, object obj, string property, BindingMode BindingMode = BindingMode.TwoWay)
        {
            //TODO: add Inotify on the obj.attr - so code changes to property will be reflected
            //TODO: check perf impact + reuse exisitng binding on same obj.prop
            try
            {
                Binding b = new Binding();
                b.Source = obj;
                b.Path = new PropertyPath(property);
                b.Mode = BindingMode;
                b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                control.SetBinding(dependencyProperty, b);
            }
            catch (Exception ex)
            {
                //it is possible we load an old enum or something else which will cause the binding to fail
                // Can happen also if the bind field name is incorrect
                // mark the control in red, instead of not openning the Page
                // Set a tool tip with the error
                
                control.Style = null; // remove style so red will show
                control.Background = System.Windows.Media.Brushes.LightPink;
                control.BorderThickness = new Thickness(2);
                control.BorderBrush = System.Windows.Media.Brushes.Red;
                control.ToolTip = "Error binding control to property: " + Environment.NewLine + property + " Please open a defect with all information,  " + Environment.NewLine + ex.Message;
            }
        }
    }
}