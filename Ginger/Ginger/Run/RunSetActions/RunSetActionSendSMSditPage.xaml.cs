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
            }/*

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SMTPMailHostTextBox, TextBox.TextProperty, runSetActionSendSMS.SMSEmail, Email.Fields.SMTPMailHost);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SMTPPortTextBox, TextBox.TextProperty, runSetActionSendSMS.SMSEmail, Email.Fields.SMTPPort);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SMTPUserTextBox, TextBox.TextProperty, runSetActionSendSMS.SMSEmail, Email.Fields.SMTPUser);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SMTPPassTextBox, TextBox.TextProperty, runSetActionSendSMS.SMSEmail, Email.Fields.SMTPPass);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(MailFromTextBox, TextBox.TextProperty, runSetActionSendSMS.SMSEmail, Email.Fields.MailFrom);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(MailToTextBox, TextBox.TextProperty, runSetActionSendSMS.SMSEmail, Email.Fields.MailTo);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SubjectTextBox, TextBox.TextProperty, runSetActionSendSMS.SMSEmail, Email.Fields.Subject);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(BodyTextBox, TextBox.TextProperty, runSetActionSendSMS.SMSEmail, Email.Fields.Body);

            GingerCore.General.FillComboFromEnumObj(EmailMethodComboBox, runSetActionSendSMS.SMSEmail.EmailMethod);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(EmailMethodComboBox, ComboBox.SelectedValueProperty, runSetActionSendSMS.SMSEmail, Email.Fields.EmailMethod);

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SMTPMailHostTextBox, TextBox.TextProperty, runSetActionSendSMS.SMSEmail, Email.Fields.SMTPMailHost);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SMTPPortTextBox, TextBox.TextProperty, runSetActionSendSMS.SMSEmail, Email.Fields.SMTPPort);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SMTPUserTextBox, TextBox.TextProperty, runSetActionSendSMS.SMSEmail, Email.Fields.SMTPUser);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SMTPPassTextBox, TextBox.TextProperty, runSetActionSendSMS.SMSEmail, Email.Fields.SMTPPass);
        */}

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}