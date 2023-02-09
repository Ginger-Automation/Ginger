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

using Amdocs.Ginger.Common;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GingerCore.GeneralLib
{
    /// <summary>
    /// Interaction logic for InputBoxWindow.xaml
    /// </summary>
    public partial class InputBoxWindow : Window
    {
        private string mOriginalValue;

        public static InputBoxWindow CurrentInputBoxWindow = null;


        ~InputBoxWindow()
        {
            CurrentInputBoxWindow = null;
        }

        public static bool OpenDialog(string title, string message, ref string Value, bool isMultiline = false)
        {
            InputBoxWindow IBW = new InputBoxWindow();
            IBW.Init(title, message, ref Value, isMultiline);
            CurrentInputBoxWindow = IBW;
            IBW.ShowDialog();            
            if (IBW.OK)
            {
                Value = IBW.value;                
                return true;
            }
            else
            {
                return false;
            }
            
        }

        public static bool OpenDialog(string title, string message, Object obj, string Property)
        {            
            InputBoxWindow IBW = new InputBoxWindow();
         
            IBW.Init(title, message, obj, Property);

            IBW.ShowDialog();
            if (IBW.OK)
            {
                return true;
            }
            else
            {
                IBW.RestoreOriginalValue();
                return false;
            }
        }

        private void RestoreOriginalValue()
        {
            ValueTextBox.Text = mOriginalValue;
        }

        public string value = null;
        public bool OK = false;

        public void Init(string title, string message, ref string Value, bool isMultiline)
        {
            if (!isMultiline)
            {
                ValueTextBox.TextWrapping = TextWrapping.NoWrap;
                ValueTextBox.AcceptsReturn = false;
            }
            winTitle.Content = title;
            MessageLabel.Text = message;
            ValueTextBox.Text = Value;
            ValueTextBox.Focus();
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

        public void Init(string title, string message, Object obj, string Property)
        {
            winTitle.Content = title;
            MessageLabel.Text = message;
            ObjFieldBinding(ValueTextBox, TextBox.TextProperty, obj, Property);
            mOriginalValue = ValueTextBox.Text;
            ValueTextBox.Focus();
        }

        public InputBoxWindow()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            value = ValueTextBox.Text;
            OK = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {                        
            this.Close();
        }

        public static bool GetInputWithValidation(string header, string label, ref string resultValue, char[] CharsNotAllowed = null, bool isMultiline = false)
        {
            bool returnWindow = OpenDialog(header, label, ref resultValue, isMultiline);

            if (returnWindow)
            {
                resultValue = resultValue.Trim();
                if (string.IsNullOrEmpty(resultValue.Trim()))
                {
                    Reporter.ToUser(eUserMsgKey.ValueIssue, "Value cannot be empty");
                    return GetInputWithValidation(header, label, ref resultValue, CharsNotAllowed, isMultiline);
                }
                if (CharsNotAllowed != null)
                {
                    if (!(resultValue.IndexOfAny(CharsNotAllowed) < 0))
                    {
                        System.Text.StringBuilder builder = new System.Text.StringBuilder();
                        foreach (char value in CharsNotAllowed)
                        {
                            builder.Append(value);
                            builder.Append(" ");
                        }
                        Reporter.ToUser(eUserMsgKey.ValueIssue, "Value cannot contain characters like: " + builder);
                        return GetInputWithValidation(header, label, ref resultValue, CharsNotAllowed, isMultiline);
                    }
                }
            }
            return returnWindow;
        }

    }
}
