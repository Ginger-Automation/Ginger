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

using System;
using System.Windows;
using System.Windows.Controls;
using Amdocs.Ginger.Common;
using GingerCore;
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
            MailFromTextBox.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.MailFrom));
            MailToTextBox.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.Mailto));
            MailCCTextBox.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.Mailcc));
            SubjectTextBox.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.Subject));
            BodyTextBox.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.Body));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xSMTPPortTextBox, TextBox.TextProperty, mAct, nameof(ActeMail.Port));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xSMTPPassTextBox, TextBox.TextProperty, mAct, nameof(ActeMail.Pass));       
            xSMTPMailHostTextBox.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.Host));
            xSMTPUserTextBox.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.User));                                           
            GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(xcbEnableSSL, CheckBox.IsCheckedProperty, mAct.GetOrCreateInputParam(ActeMail.Fields.EnableSSL, "true"));
            GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(xcbConfigureCredential, CheckBox.IsCheckedProperty, mAct.GetOrCreateInputParam(ActeMail.Fields.ConfigureCredential,"false"));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(AttachmentFilename, TextBox.TextProperty, mAct, nameof(ActeMail.AttachmentFileName));
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

                xSMTPConfig.Visibility = Visibility.Collapsed;
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

                xSMTPConfig.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                String err = ex.Message;
            }
        }
        private void xcbConfigureCredential_Checked(object sender, RoutedEventArgs e)
        {
            xUserDetails.Visibility = Visibility.Visible;           
        }

        private void xcbConfigureCredential_Unchecked(object sender, RoutedEventArgs e)
        {
            xUserDetails.Visibility = Visibility.Collapsed;            
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
    }
}
