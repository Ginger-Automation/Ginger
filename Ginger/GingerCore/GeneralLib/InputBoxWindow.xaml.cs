#region License
/*
Copyright © 2014-2026 European Support Limited

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
using System.Collections.Generic;
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
        private Func<string, bool, string> mDialogSaveHandler;
        private CheckBox mOptionCheckBox;

        public static InputBoxWindow CurrentInputBoxWindow = null;

        private const int MAX_LENGTH = 30;
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

        public static bool OpenOptionDialog(
            string title,
            string message,
            ref string Value,
            string checkBoxText,
            Func<string, bool, string> saveHandler,
            out bool checkBoxValue)
        {
            InputBoxWindow IBW = new InputBoxWindow();
            IBW.Init(title, message, ref Value, false);
            IBW.mOptionCheckBox =
                LogicalTreeHelper.FindLogicalNode(IBW, "SyncWithEntityNameCheckBox") as CheckBox;
            if (IBW.mOptionCheckBox == null)
            {
                throw new InvalidOperationException(
                    "The input dialog checkbox 'SyncWithEntityNameCheckBox' was not found.");
            }
            IBW.mOptionCheckBox.Content = checkBoxText;
            IBW.mOptionCheckBox.Visibility = Visibility.Visible;
            IBW.mDialogSaveHandler = saveHandler;
            IBW.SizeToContent = SizeToContent.Height;
            IBW.Height = double.NaN;
            CurrentInputBoxWindow = IBW;
            IBW.ShowDialog();

            if (IBW.OK)
            {
                Value = IBW.value;
                checkBoxValue = IBW.mOptionCheckBox.IsChecked == true;
                return true;
            }

            checkBoxValue = false;
            return false;
        }

        public static bool OpenDialog(string title, string message, ref string Value, List<string> possibleValues)
        {
            InputBoxWindow IBW = new InputBoxWindow();
            if (possibleValues == null || possibleValues.Count == 0)
            {
                throw new ArgumentException("possibleValues cannot be null or empty", nameof(possibleValues));
            }
            IBW.Init(title, message, possibleValues);
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
            ValueTextBox.MaxLength = message.Contains("Folder") ? MAX_LENGTH : 0;
            ValueTextBox.Focus();
        }

        public void Init(string title, string message, List<string> possibleValues)
        {
            winTitle.Content = title;
            ValueTextBox.MaxLength = message.Contains("Folder") ? MAX_LENGTH : 0;
            MessageLabel.Text = message;
            if (possibleValues == null || possibleValues.Count == 0)
            {
                throw new ArgumentException("possibleValues cannot be null or empty", nameof(possibleValues));
            }
            ValueTextBox.Visibility = System.Windows.Visibility.Collapsed;
            xValueComboBox.Visibility = System.Windows.Visibility.Visible;
            xValueComboBox.ItemsSource = possibleValues;
            xValueComboBox.SelectedIndex = 0;//auto selecting first value
            xValueComboBox.Focus();
        }

        public void Init(string title, string message, Object obj, string Property)
        {
            winTitle.Content = title;
            ValueTextBox.MaxLength = message.Contains("Folder") ? MAX_LENGTH : 0;
            MessageLabel.Text = message;
            ObjFieldBinding(ValueTextBox, TextBox.TextProperty, obj, Property);
            mOriginalValue = ValueTextBox.Text;
            ValueTextBox.Focus();
        }



        void ObjFieldBinding(System.Windows.Controls.Control control, DependencyProperty dependencyProperty, object obj, string property)
        {
            Binding b = new Binding
            {
                Source = obj,
                Path = new PropertyPath(property),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            control.SetBinding(dependencyProperty, b);
        }

       

        public InputBoxWindow()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (xValueComboBox.Visibility == System.Windows.Visibility.Visible)
            {
                value = (string)xValueComboBox.SelectedValue;
            }
            else
            {
                value = ValueTextBox.Text;
            }

            if (mDialogSaveHandler != null)
            {
                string errorMessage = mDialogSaveHandler(
                    value,
                    mOptionCheckBox?.IsChecked == true);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    TextBlock errorTextBlock = (TextBlock)FindName("RenameErrorMessage");
                    errorTextBlock.Text = errorMessage;
                    errorTextBlock.Visibility = Visibility.Visible;
                    errorTextBlock.UpdateLayout();
                    UpdateLayout();
                    return;
                }
            }

            OK = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBlock errorTextBlock = (TextBlock)FindName("RenameErrorMessage");
            if (errorTextBlock != null && errorTextBlock.Visibility == Visibility.Visible)
            {
                errorTextBlock.Text = string.Empty;
                errorTextBlock.Visibility = Visibility.Collapsed;
                UpdateLayout();
            }
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
