#region License
/*
Copyright © 2014-2022 European Support Limited

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
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using GingerCore;
using GingerCore.Actions.Communication;
using GingerCore.GeneralLib;
using CheckBox = System.Windows.Controls.CheckBox;
using Amdocs.Ginger.Repository;
using NPOI.HPSF;
using Org.BouncyCastle.Asn1.X509;

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
            xMailFromDisplayNameTextBox.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.MailFromDisplayName));
            MailToTextBox.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.Mailto));
            MailCCTextBox.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.Mailcc));
            SubjectTextBox.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.Subject));
            
            BodyTextBox.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.Body));
            BodyTextBox.AdjustHight(100);

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xSMTPPortTextBox, TextBox.TextProperty, mAct, nameof(ActeMail.Port));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xSMTPPassTextBox, TextBox.TextProperty, mAct, nameof(ActeMail.Pass));       
            xSMTPMailHostTextBox.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.Host));
            xSMTPUserTextBox.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.User));                                           
            GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(xcbEnableSSL, CheckBox.IsCheckedProperty, mAct.GetOrCreateInputParam(ActeMail.Fields.EnableSSL, "true"));
                        BindingHandler.ObjFieldBinding(IsValidationRequired, CheckBox.IsCheckedProperty, mAct, nameof(ActeMail.IsValidationRequired));
            BindingHandler.ObjFieldBinding(xcbCertificatePathTextBox, TextBox.TextProperty, mAct, nameof(ActeMail.CertificatePath));
            BindingHandler.ObjFieldBinding(CertificatePasswordUCValueExpression, TextBox.TextProperty, mAct, nameof(ActeMail.CertificatePasswordUCValueExpression));
            GingerCore.GeneralLib.BindingHandler.ActInputValueBinding(xcbConfigureCredential, CheckBox.IsCheckedProperty, mAct.GetOrCreateInputParam(ActeMail.Fields.ConfigureCredential,"false"));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(AttachmentFilename, TextBox.TextProperty, mAct, nameof(ActeMail.AttachmentFileName));

            if (mAct.MailOption != null && mAct.MailOption == Email.eEmailMethod.OUTLOOK.ToString())
            {
                RadioOutlookMailOption.IsChecked = true;
            }
            else
            {
                RadioSMTPMailOption.IsChecked = true;
                if(string.IsNullOrEmpty(mAct.MailFromDisplayName))
                {
                    mAct.MailFromDisplayName = "_Amdocs Ginger Automation";
                }
            }
            ShowDisplayNameOption();
        }
        private void xcbValidationRequired_checked(object sender, RoutedEventArgs e)
        {
            IsValidationRequired.IsChecked = true;
            CertificateStackPanel.Visibility = Visibility.Visible;
        }
        private void xcbValidationRequired_unchecked(object sender, RoutedEventArgs e)
        {
            IsValidationRequired.IsChecked = false;
            CertificateStackPanel.Visibility = Visibility.Collapsed;
        }
        private void BrowseButton_Certificate(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();           
            dlg.DefaultExt = "*.cer or .CRT or .crt or .pem or .pfx or .p12";
            dlg.Filter = "All Certificate Files ((*.crt,*.cer ,*.CRT ,*.CER ,*.pem , *.pfx ,*.p12 )|*.crt;*.cer;*.CER;*.CRT,*.pem;*.pfx;*.p12";
            string SolutionFolder = WorkSpace.Instance.Solution.Folder.ToUpper();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                string FileName = dlg.FileName.ToUpper();
                if (FileName.Contains(SolutionFolder))
                {
                    FileName = FileName.Replace(SolutionFolder, @"~\");
                }
                FileName = WorkSpace.Instance.SolutionRepository.ConvertFullPathToBeRelative(FileName);
                xcbCertificatePathTextBox.Text = FileName;
                string targetPath = System.IO.Path.Combine(SolutionFolder, @"Documents\EmailCertificates");
                if (!System.IO.Directory.Exists(targetPath))
                {
                    System.IO.Directory.CreateDirectory(targetPath);
                }

                string destFile = System.IO.Path.Combine(targetPath, System.IO.Path.GetFileName(FileName));

                int fileNum = 1;
                string copySufix = "_Copy";
                while (System.IO.File.Exists(destFile))
                {
                    fileNum++;
                    string newFileName = System.IO.Path.GetFileNameWithoutExtension(destFile);
                    if (newFileName.IndexOf(copySufix) != -1)
                    {
                        newFileName = newFileName.Substring(0, newFileName.IndexOf(copySufix));
                    }
                    newFileName = newFileName + copySufix + fileNum.ToString() + System.IO.Path.GetExtension(destFile);
                    destFile = System.IO.Path.Combine(targetPath, newFileName);
                }

                System.IO.File.Copy(FileName, destFile, true);
                xcbCertificatePathTextBox.Text = @"~\Documents\EmailCertificates\" + System.IO.Path.GetFileName(destFile);
                xcbCertificatePathTextBox.AcceptsReturn = true;
                xcbCertificatePathTextBox.Visibility = Visibility.Visible;
            }
        }

        private void ShowDisplayNameOption()
        {
            if (mAct.MailOption != null && mAct.MailOption == Email.eEmailMethod.SMTP.ToString())
            {
                xLabelMailFromDisplayName.Visibility = Visibility.Visible;
                xMailFromDisplayNameTextBox.Visibility = Visibility.Visible;
            }
            else
            {
                xLabelMailFromDisplayName.Visibility = Visibility.Collapsed;
                xMailFromDisplayNameTextBox.Visibility = Visibility.Collapsed;
            }
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

                xLabelMailFromDisplayName.Visibility = Visibility.Visible;
                xMailFromDisplayNameTextBox.Visibility = Visibility.Visible;
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
                
                xLabelMailFromDisplayName.Visibility = Visibility.Visible;
                xMailFromDisplayNameTextBox.Visibility = Visibility.Visible;
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
                xSMTPPassTextBox.Text = EncryptionHandler.EncryptwithKey(xSMTPPassTextBox.Text);                
            }
        }

        private void Label_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowDisplayNameOption();
        }
    }
}
