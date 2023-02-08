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

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GingerCore.GeneralLib
{
    /// <summary>
    /// Interaction logic for InputBoxWindow.xaml
    /// </summary>
    public partial class ComboBoxWindow : Window
    {
        private string mOriginalValue;

        public static bool OpenDialog(string title, string message, List<string> mValues, ref string Value)
        {
            ComboBoxWindow CBW = new ComboBoxWindow();
            CBW.Init(title, message, mValues, ref Value);
            CBW.ShowDialog();
            if (CBW.OK)
            {
                Value = CBW.value;
                return true;
            }
            else
            {
                return false;
            }                
        }

        public static bool OpenDialog(string title, string message, List<string> mValues, Object obj, string Property)
        {
            ComboBoxWindow CBW = new ComboBoxWindow();
            CBW.Init(title, message, mValues, obj, Property);
            CBW.ShowDialog();
            if (CBW.OK)
            {
                return true;
            }
            else
            {
                CBW.RestoreOriginalValue();
                return false;
            }
        }

        private void RestoreOriginalValue()
        {
            ValueComboBox.SelectedValue = mOriginalValue;
        }

        public string value = null;
        public bool OK = false;

        public void Init(string title, string message, List<string> mValues,ref string Value)
        {
            GingerCore.General.FillComboFromList(ValueComboBox, mValues);
            
            winTitle.Content = title;
            MessageLabel.Text = message;
            ValueComboBox.Text = Value;
            ValueComboBox.Focus();
        }

        void ObjFieldBinding(System.Windows.Controls.Control control, DependencyProperty dependencyProperty, object obj, string property)
        {            
            Binding b = new Binding();
            b.Source = obj;
            b.Path = new PropertyPath(property);
            b.Mode = BindingMode.TwoWay;
            b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            control.SetBinding(dependencyProperty, b);
        }

        public void Init(string title, string message, List<string> mValues, Object obj, string Property)
        {
            GingerCore.General.FillComboFromList(ValueComboBox, mValues);
            
            winTitle.Content = title;
            MessageLabel.Text = message;
            
            ObjFieldBinding(ValueComboBox, ComboBox.SelectedValueProperty, obj, Property);

            mOriginalValue = ValueComboBox.Text;

            ValueComboBox.Focus();
        }

        public ComboBoxWindow()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            value = ValueComboBox.Text;
            OK = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {                        
            this.Close();
        }
    }
}
