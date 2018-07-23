#region License
/*
Copyright © 2014-2018 European Support Limited

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
using GingerCore.GeneralLib;

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
            MailFromTextBox.Init(runSetActionSendFreeEmail, RunSetActionHTMLReportSendEmail.Fields.MailFrom);
            MailToTextBox.Init(runSetActionSendFreeEmail, RunSetActionHTMLReportSendEmail.Fields.MailTo);
            MailCCTextBox.Init(runSetActionSendFreeEmail, RunSetActionHTMLReportSendEmail.Fields.MailCC);
            SubjectTextBox.Init(runSetActionSendFreeEmail, RunSetActionHTMLReportSendEmail.Fields.Subject);
            BodyTextBox.Init(runSetActionSendFreeEmail, RunSetActionHTMLReportSendEmail.Fields.Bodytext);
            BodyTextBox.AdjustHight(100);
            App.ObjFieldBinding(SMTPMailHostTextBox, TextBox.TextProperty, runSetActionSendFreeEmail.Email, Email.Fields.SMTPMailHost);
            App.ObjFieldBinding(SMTPPortTextBox, TextBox.TextProperty, runSetActionSendFreeEmail.Email, Email.Fields.SMTPPort);
            App.ObjFieldBinding(SMTPUserTextBox, TextBox.TextProperty, runSetActionSendFreeEmail.Email, Email.Fields.SMTPUser);
            App.ObjFieldBinding(SMTPPassTextBox, TextBox.TextProperty, runSetActionSendFreeEmail.Email, Email.Fields.SMTPPass);
            App.FillComboFromEnumVal(EmailMethodComboBox, runSetActionSendFreeEmail.Email.EmailMethod);
            App.ObjFieldBinding(EmailMethodComboBox, ComboBox.SelectedValueProperty, runSetActionSendFreeEmail.Email, Email.Fields.EmailMethod);
            App.ObjFieldBinding(cbEnableSSL, CheckBox.IsCheckedProperty, runSetActionSendFreeEmail.Email, Email.Fields.EnableSSL);
        }

        private void EmailMethodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EmailMethodComboBox.SelectedItem.ToString() == "OUTLOOK")
            {
                SMTPConfig.Visibility = Visibility.Collapsed;
            }
            else
            {
                SMTPConfig.Visibility = Visibility.Visible;
            }
        }
    }
}
