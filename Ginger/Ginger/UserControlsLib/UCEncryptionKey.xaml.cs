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

using Ginger.SolutionGeneral;
using Ginger.SolutionWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ginger.UserControlsLib
{
    /// <summary>
    /// Interaction logic for UCEncryptionKey.xaml
    /// </summary>  
    public partial class UCEncryptionKey : UserControl
    {
        public Solution mSolution;
        public UCEncryptionKey()
        {
            InitializeComponent();
        }
        private void ShowPassword_PreviewMouseDown(object sender, MouseButtonEventArgs e) => ShowPasswordFunction();
        private void ShowPassword_PreviewMouseUp(object sender, MouseButtonEventArgs e) => HidePasswordFunction();
        private void ShowPassword_MouseLeave(object sender, MouseEventArgs e) => HidePasswordFunction();

        private void ValidateKeyMouseDown(object sender, MouseButtonEventArgs e) => ValidateKey();

        private void CopyToClipboardKeyMouseDown(object sender, MouseButtonEventArgs e) => CopyToClipBoard();

        private void UpdateKeyMouseDown(object sender, MouseButtonEventArgs e) => UpdateKeyClick();

        private void ShowPasswordFunction()
        {
            ShowPassword.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Invisible;
            EncryptionKeyTextBox.Visibility = Visibility.Visible;
            EncryptionKeyPasswordBox.Visibility = Visibility.Hidden;
            EncryptionKeyTextBox.Text = EncryptionKeyPasswordBox.Password;
        }

        private void HidePasswordFunction()
        {
            ShowPassword.ImageType = Amdocs.Ginger.Common.Enums.eImageType.Visible;
            EncryptionKeyTextBox.Visibility = Visibility.Hidden;
            EncryptionKeyPasswordBox.Visibility = Visibility.Visible;
        }

        public bool ValidateKey()
        {
            if (!string.IsNullOrEmpty(EncryptionKeyPasswordBox.Password) && mSolution.SolutionOperations.ValidateKey(EncryptionKeyPasswordBox.Password))
            {
                ValidFlag.Visibility = Visibility.Visible;
                InvalidFlag.Visibility = Visibility.Collapsed;
                return true;
            }
            else
            {
                ValidFlag.Visibility = Visibility.Collapsed;
                InvalidFlag.Visibility = Visibility.Visible;
                return false;
            }
        }

        public bool CheckKeyCombination()
        {
            Regex regex = new Regex(@"^.*(?=.{8,16})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!*@#$%^&+=]).*$");
            if (!regex.IsMatch(EncryptionKeyPasswordBox.Password))
            {
                InvalidFlag.Visibility = Visibility.Visible;
                ValidFlag.Visibility = Visibility.Collapsed;
                EncryptionKeyPasswordBox.Visibility = Visibility.Visible;
                EncryptionkeyError.Visibility = Visibility.Visible;
                EncryptionkeyError.Content = "Encryption key must be 8-16 in length and should contain at least 1 cap, 1 small, 1 digit and 1 special char.";
                EncryptionkeyError.Foreground = Brushes.Red;
                EncryptionkeyError.FontSize = 9;
                return false;
            }
            InvalidFlag.Visibility = Visibility.Collapsed;
            ValidFlag.Visibility = Visibility.Visible;
            EncryptionkeyError.Visibility = Visibility.Collapsed;
            return true;
        }

        public void CopyToClipBoard()
        {
            Clipboard.SetText(EncryptionKeyPasswordBox.Password);
        }
        public void ChangeLabel(string newLabel)
        {
            this.Label.Content = newLabel;
        }

        public void UpdateKeyClick()
        {

        }

        private async void EncryptionKeyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            //this inner method checks if user is still typing
            async Task<bool> UserKeepsTyping()
            {
                string txt = EncryptionKeyPasswordBox.Password;
                await Task.Delay(2000);
                return txt != EncryptionKeyPasswordBox.Password;
            }
            if (await UserKeepsTyping()) { return; }
            bool IsEncrytedStrAvailableOnSol = !string.IsNullOrEmpty(mSolution.EncryptedValidationString);
            if (IsEncrytedStrAvailableOnSol)
            {
                ValidateKey();
            }
            else
            {
                CheckKeyCombination();
            }
        }
    }
}
