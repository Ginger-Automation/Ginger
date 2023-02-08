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
using GingerCore;
using System;
using System.Windows;

namespace Ginger.GeneralLib
{
    /// <summary>
    /// Interaction logic for GeneralErrorWindow.xaml
    /// </summary>
    public partial class GeneralErrorWindow : Window
    {        
        public static void ShowError(Exception ex)
        {
            GeneralErrorWindow GEW = new GeneralErrorWindow(ex);
            GEW.Show();
        }

        public GeneralErrorWindow(Exception ex)
        {
            InitializeComponent();

            ErrorMessageTextBox.Text = ex.Message;
            string FullInfo = "Error:" + ex.Message + Environment.NewLine;
            FullInfo += "Source:" + ex.Source + Environment.NewLine;
            FullInfo += "Stack Trace: " + ex.StackTrace;
            FullInfoTextBox.Text =  FullInfo;
        }

        private void MoreInfoButton_Click(object sender, RoutedEventArgs e)
        {
            FullInfoTextBox.Visibility = System.Windows.Visibility.Visible;
            MoreInfoButton.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CopyToClipboardButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(FullInfoTextBox.Text);            
            Reporter.ToUser(eUserMsgKey.CopiedErrorInfo);
        }
    }
}
