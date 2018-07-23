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

using System;
using System.Windows;
using System.Windows.Controls;
using GingerCore.Actions.Communication;
using GingerCore.GeneralLib;

namespace Ginger.Actions.Communication
{
    /// <summary>
    /// Interaction logic for ActeMailEditPage.xaml
    /// </summary>
    public partial class ActeMailEditPage : Page
    {
        ActeMail mAct;

        public ActeMailEditPage(ActeMail act)
        {
            InitializeComponent();
            mAct = act;
            Bind();
        }

        private void Bind()
        {
            App.FillComboFromEnumVal(EmailActionComboBox, mAct.eMailActionType);
            App.ObjFieldBinding(EmailActionComboBox, ComboBox.TextProperty, mAct, ActeMail.Fields.eMailActionType);
            App.ObjFieldBinding(MailFromTextBox, TextBox.TextProperty, mAct, ActeMail.Fields.MailFrom);
            App.ObjFieldBinding(MailToTextBox, TextBox.TextProperty, mAct, ActeMail.Fields.Mailto);
            App.ObjFieldBinding(MailCCTextBox, TextBox.TextProperty, mAct, ActeMail.Fields.Mailcc);
            App.ObjFieldBinding(SubjectTextBox, TextBox.TextProperty, mAct, ActeMail.Fields.Subject);
            App.ObjFieldBinding(BodyTextBox, TextBox.TextProperty, mAct, ActeMail.Fields.Body);
            App.ObjFieldBinding(AttachmentFilename, TextBox.TextProperty, mAct, ActeMail.Fields.AttachmentFileName);
            App.ObjFieldBinding(MailHost, TextBox.TextProperty, mAct, ActeMail.Fields.Host);
            App.ObjFieldBinding(Port, TextBox.TextProperty, mAct, ActeMail.Fields.Port);
            App.ObjFieldBinding(User, TextBox.TextProperty, mAct, ActeMail.Fields.User);
            App.ObjFieldBinding(Pass, TextBox.TextProperty, mAct, ActeMail.Fields.Pass);            
            GingerCore.General.ActInputValueBinding(cbEnableSSL, CheckBox.IsCheckedProperty, mAct.GetOrCreateInputParam(ActeMail.Fields.EnableSSL,"true"));
        
            if (mAct.MailOption!=null && mAct.MailOption == Email.eEmailMethod.OUTLOOK.ToString())
                RadioOutlookMailOption.IsChecked = true;
            else
                RadioSMTPMailOption.IsChecked = true;                      
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.OpenFileDialog();          
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                mAct.AttachmentFileName = dlg.FileName;
            }
        }

        //update screen on select of Outlook Radio Button
        private void RadioOutlookMailOption_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                mAct.MailOption = Email.eEmailMethod.OUTLOOK.ToString();

                LabelFrom.Visibility = System.Windows.Visibility.Collapsed;
                MailFromTextBox.Visibility = System.Windows.Visibility.Collapsed;
                             
                MailHost.Visibility = System.Windows.Visibility.Collapsed;
                LabelMailHost.Visibility = System.Windows.Visibility.Collapsed;

                Port.Visibility = System.Windows.Visibility.Collapsed;
                LabelPort.Visibility = System.Windows.Visibility.Collapsed;

                User.Visibility = System.Windows.Visibility.Collapsed;
                LabelUser.Visibility = System.Windows.Visibility.Collapsed;

                Pass.Visibility = System.Windows.Visibility.Collapsed;
                LabelPass.Visibility = System.Windows.Visibility.Collapsed;
                cbEnableSSL.Visibility = System.Windows.Visibility.Collapsed;
            }
            catch(Exception ex)
            {
                String err = ex.Message;
            }

        }

        //Update screen on select of SMTP Radio Button
        private void RadioSMTPMailOption_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                mAct.MailOption = Email.eEmailMethod.SMTP.ToString();

                MailFromTextBox.IsEnabled = true;

                MailFromTextBox.Visibility = System.Windows.Visibility.Visible;
                LabelFrom.Visibility = System.Windows.Visibility.Visible;

                MailHost.Visibility = System.Windows.Visibility.Visible;
                LabelMailHost.Visibility = System.Windows.Visibility.Visible;

                Port.Visibility = System.Windows.Visibility.Visible;
                LabelPort.Visibility = System.Windows.Visibility.Visible;

                User.Visibility = System.Windows.Visibility.Visible;
                LabelUser.Visibility = System.Windows.Visibility.Visible;

                Pass.Visibility = System.Windows.Visibility.Visible;
                LabelPass.Visibility = System.Windows.Visibility.Visible;
                cbEnableSSL.Visibility = System.Windows.Visibility.Visible;
            }
            catch (Exception ex)
            {
                String err = ex.Message;
            }
        }
                
    }
}
