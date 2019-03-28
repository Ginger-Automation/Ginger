#region License
/*
Copyright © 2014-2019 European Support Limited

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

using System.Windows;
using System.Windows.Controls;
using GingerCore;
using GingerCore.GeneralLib;
using amdocs.ginger.GingerCoreNET;

namespace Ginger.Run.RunSetActions
{
    /// <summary>
    /// Interaction logic for RunSetActionHTMLReportSendEmailEditPage.xaml
    /// </summary>
    public partial class RunSetActionSendFreeEmailEditPage : Page
    {
        public RunSetActionSendFreeEmailEditPage(RunSetActionSendFreeEmail runSetActionSendFreeEmail)
        {
            InitializeComponent();
            if (runSetActionSendFreeEmail.Email == null)
            {
                runSetActionSendFreeEmail.Email = new Email();
            }
            MailFromTextBox.Init(null, runSetActionSendFreeEmail, nameof(RunSetActionSendFreeEmail.MailFrom));
            MailToTextBox.Init(null, runSetActionSendFreeEmail, nameof(RunSetActionSendFreeEmail.MailTo));
            MailCCTextBox.Init(null, runSetActionSendFreeEmail, nameof(RunSetActionSendFreeEmail.MailCC));
            SubjectTextBox.Init(null, runSetActionSendFreeEmail, nameof(RunSetActionSendFreeEmail.Subject));
            BodyTextBox.Init(null, runSetActionSendFreeEmail, nameof(RunSetActionSendFreeEmail.Bodytext));
            BodyTextBox.AdjustHight(100);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xSMTPPortTextBox, TextBox.TextProperty, runSetActionSendFreeEmail.Email, nameof(Email.SMTPPort));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xSMTPPassTextBox, TextBox.TextProperty, runSetActionSendFreeEmail.Email, nameof(Email.SMTPPass));
            GingerCore.General.FillComboFromEnumObj(xEmailMethodComboBox, runSetActionSendFreeEmail.Email.EmailMethod);
            xSMTPMailHostTextBox.Init(null, runSetActionSendFreeEmail, nameof(RunSetActionSendFreeEmail.MailHost));
            xSMTPUserTextBox.Init(null, runSetActionSendFreeEmail, nameof(RunSetActionSendFreeEmail.MailUser));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xEmailMethodComboBox, ComboBox.SelectedValueProperty, runSetActionSendFreeEmail.Email, nameof(Email.EmailMethod));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xcbEnableSSL, CheckBox.IsCheckedProperty, runSetActionSendFreeEmail.Email, nameof(Email.EnableSSL));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xcbConfigureCredential, CheckBox.IsCheckedProperty, runSetActionSendFreeEmail.Email, nameof(Email.ConfigureCredential));
            if (string.IsNullOrEmpty(runSetActionSendFreeEmail.MailTo))
            {
                runSetActionSendFreeEmail.MailFrom =  WorkSpace.Instance.UserProfile.UserEmail;
            }
        }

        private void xEmailMethodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xEmailMethodComboBox.SelectedItem.ToString() == "OUTLOOK")
            {
                xSMTPConfig.Visibility = Visibility.Collapsed;
            }
            else
            {
                xSMTPConfig.Visibility = Visibility.Visible;
            }
        }

        private void xcbConfigureCredential_Checked(object sender, RoutedEventArgs e)
        {
            xSMTPUserTextBox.Visibility = Visibility.Visible;
            xSMTPPassTextBox.Visibility = Visibility.Visible;
            xLabelPass.Visibility = Visibility.Visible;
            xLabelUser.Visibility = Visibility.Visible;
        }

        private void xSMTPPassTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            bool res = false;
            if (!EncryptionHandler.IsStringEncrypted(xSMTPPassTextBox.Text))
            {
                xSMTPPassTextBox.Text = EncryptionHandler.EncryptString(xSMTPPassTextBox.Text, ref res);
                if (res == false)
                {
                    xSMTPPassTextBox.Text = string.Empty;
                }
            }
        }

        private void xcbConfigureCredential_Unchecked(object sender, RoutedEventArgs e)
        {
            xSMTPUserTextBox.Visibility = Visibility.Collapsed;
            xSMTPPassTextBox.Visibility = Visibility.Collapsed;
            xLabelPass.Visibility = Visibility.Collapsed;
            xLabelUser.Visibility = Visibility.Collapsed;
        }
    }
}
 
//        }
//    }
//}