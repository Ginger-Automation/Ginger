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

using GingerCore.GeneralLib;
using System.Windows;
using System.Windows.Controls;
using GingerCore;
using Amdocs.Ginger.Common;
using amdocs.ginger.GingerCoreNET;
using CheckBox = System.Windows.Controls.CheckBox;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common.Repository;
using GingerCore.Actions.WebServices;
using System.IO;

namespace Ginger.Run.RunSetActions
{
    /// <summary>
    /// Interaction logic for RunSetActionDeliveryMethodConfigPage.xaml
    /// </summary>
    public partial class RunSetActionDeliveryMethodConfigPage : Page
    {
        public RunSetActionDeliveryMethodConfigPage(Email email)
        {
            InitializeComponent();
            Context context = new Context() { Environment = WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment };
            xSMTPMailHostTextBox.Init(context, email, nameof(Email.SMTPMailHost));
            BindingHandler.ObjFieldBinding(xSMTPPortTextBox, TextBox.TextProperty, email, nameof(Email.SMTPPort));
            xSMTPUserTextBox.Init(context, email, nameof(Email.SMTPUser));
            BindingHandler.ObjFieldBinding(xSMTPPassTextBox, TextBox.TextProperty, email, nameof(Email.SMTPPass));
            GingerCore.General.FillComboFromEnumObj(xEmailMethodComboBox, email.EmailMethod);
            BindingHandler.ObjFieldBinding(xEmailMethodComboBox, ComboBox.SelectedValueProperty, email, nameof(Email.EmailMethod));
            BindingHandler.ObjFieldBinding(xcbEnableSSL, CheckBox.IsCheckedProperty, email, nameof(Email.EnableSSL));
            BindingHandler.ObjFieldBinding(xcbConfigureCredential, CheckBox.IsCheckedProperty, email, nameof(Email.ConfigureCredential));
            BindingHandler.ObjFieldBinding(xcbIsValidationRequired, CheckBox.IsCheckedProperty, email, nameof(Email.IsValidationRequired));
            BindingHandler.ObjFieldBinding(xcbCertificatePathTextBox, TextBox.TextProperty, email, nameof(Email.CertificatePath));
            BindingHandler.ObjFieldBinding(CertificatePasswordUCValueExpression, TextBox.TextProperty, email, nameof(Email.CertificatePasswordUCValueExpression));
        }
        private void CertificateSelection_Changed(object sender, RoutedEventArgs e)
        {
            CertificateStackPanel.Visibility = System.Windows.Visibility.Visible;
        }
        private void CertificateSelection_Unchecked(object sender, RoutedEventArgs e)
        {
            CertificateStackPanel.Visibility = System.Windows.Visibility.Collapsed;
        }
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();

            dlg.DefaultExt = "*.crt";
            dlg.Filter = "CRT Files (*.crt)|*.crt";
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
        private void xcbConfigureCredential_Unchecked(object sender, RoutedEventArgs e)
        {
            xSMTPUserTextBox.Visibility = Visibility.Collapsed;
            xSMTPPassTextBox.Visibility = Visibility.Collapsed;
            xLabelPass.Visibility = Visibility.Collapsed;
            xLabelUser.Visibility = Visibility.Collapsed;
        }
        private void xSMTPPassTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!EncryptionHandler.IsStringEncrypted(xSMTPPassTextBox.Text))
            {
                xSMTPPassTextBox.Text = EncryptionHandler.EncryptwithKey(xSMTPPassTextBox.Text);                
            }
        }
    }
}
