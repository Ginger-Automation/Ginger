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

using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;
using GingerCore.GeneralLib;

namespace Ginger.Run.RunSetActions
{
    /// <summary>
    /// Interaction logic for RunSetActionEmailReportEditPage.xaml
    /// </summary>
    public partial class RunSetActionSendSMSEditPage : Page
    {
        public RunSetActionSendSMSEditPage(RunSetActionSendSMS runSetActionSendSMS)
        {
            InitializeComponent();

            if (runSetActionSendSMS.SMSEmail == null)
            {
                runSetActionSendSMS.SMSEmail = new Email();                
            }

            App.ObjFieldBinding(SMTPMailHostTextBox, TextBox.TextProperty, runSetActionSendSMS.SMSEmail, Email.Fields.SMTPMailHost);
            App.ObjFieldBinding(SMTPPortTextBox, TextBox.TextProperty, runSetActionSendSMS.SMSEmail, Email.Fields.SMTPPort);
            App.ObjFieldBinding(SMTPUserTextBox, TextBox.TextProperty, runSetActionSendSMS.SMSEmail, Email.Fields.SMTPUser);
            App.ObjFieldBinding(SMTPPassTextBox, TextBox.TextProperty, runSetActionSendSMS.SMSEmail, Email.Fields.SMTPPass);
            App.ObjFieldBinding(MailFromTextBox, TextBox.TextProperty, runSetActionSendSMS.SMSEmail, Email.Fields.MailFrom);
            App.ObjFieldBinding(MailToTextBox, TextBox.TextProperty, runSetActionSendSMS.SMSEmail, Email.Fields.MailTo);
            App.ObjFieldBinding(SubjectTextBox, TextBox.TextProperty, runSetActionSendSMS.SMSEmail, Email.Fields.Subject);
            App.ObjFieldBinding(BodyTextBox, TextBox.TextProperty, runSetActionSendSMS.SMSEmail, Email.Fields.Body);

            App.FillComboFromEnumVal(EmailMethodComboBox, runSetActionSendSMS.SMSEmail.EmailMethod);
            App.ObjFieldBinding(EmailMethodComboBox, ComboBox.SelectedValueProperty, runSetActionSendSMS.SMSEmail, Email.Fields.EmailMethod);

            App.ObjFieldBinding(SMTPMailHostTextBox, TextBox.TextProperty, runSetActionSendSMS.SMSEmail, Email.Fields.SMTPMailHost);
            App.ObjFieldBinding(SMTPPortTextBox, TextBox.TextProperty, runSetActionSendSMS.SMSEmail, Email.Fields.SMTPPort);
            App.ObjFieldBinding(SMTPUserTextBox, TextBox.TextProperty, runSetActionSendSMS.SMSEmail, Email.Fields.SMTPUser);
            App.ObjFieldBinding(SMTPPassTextBox, TextBox.TextProperty, runSetActionSendSMS.SMSEmail, Email.Fields.SMTPPass);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}