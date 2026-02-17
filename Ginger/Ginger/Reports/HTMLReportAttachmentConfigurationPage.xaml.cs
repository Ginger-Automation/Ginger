#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET;
using Ginger.Actions;
using Ginger.Run.RunSetActions;
using GingerCore;
using GingerCore.DataSource;
using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Reports
{
    /// <summary>
    /// Interaction logic for HTMLReportAttachmentConfigurationPage.xaml
    /// </summary>
    public partial class HTMLReportAttachmentConfigurationPage : Page
    {
        GenericWindow _pageGenericWin = null;
        EmailHtmlReportAttachment mEmailAttachment = new EmailHtmlReportAttachment();
        private bool IsLinkEnabled;
        private bool IsAccountReportLinkEnabled;
        private bool IsZipFolderAttachementEnabled;

        public HTMLReportAttachmentConfigurationPage(EmailHtmlReportAttachment emailAttachment)
        {
            InitializeComponent();
            mEmailAttachment = emailAttachment;
            Init();
        }

        private void Init()
        {
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(DefaultTemplatePickerCbx, ComboBox.SelectedValueProperty, mEmailAttachment, nameof(EmailHtmlReportAttachment.SelectedHTMLReportTemplateID));
            HTMLReportFolderTextBox.Init(null, mEmailAttachment, nameof(EmailAttachment.ExtraInformation), true, true, UCValueExpression.eBrowserType.Folder, "*.*", null);

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(UseAlternativeHTMLReportFolderCbx, CheckBox.IsCheckedProperty, mEmailAttachment, nameof(EmailHtmlReportAttachment.IsAlternameFolderUsed));
            RadioButtonInit();

            DefaultTemplatePickerCbx.ItemsSource = null;

            ObservableList<HTMLReportConfiguration> HTMLReportConfigurations = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>();
            if (WorkSpace.Instance.Solution != null && HTMLReportConfigurations.Count > 0)
            {
                DefaultTemplatePickerCbx.ItemsSource = HTMLReportConfigurations;
                DefaultTemplatePickerCbx.DisplayMemberPath = nameof(HTMLReportConfiguration.Name);
                DefaultTemplatePickerCbx.SelectedValuePath = nameof(HTMLReportConfiguration.ID);
                int index = 0;
                foreach (var config in HTMLReportConfigurations)
                {
                    if (string.Equals(config.ItemName, mEmailAttachment.ItemName, StringComparison.Ordinal))
                    {
                        DefaultTemplatePickerCbx.SelectedIndex = index;
                        break;
                    }
                    index++;

                }
                if (index >= HTMLReportConfigurations.Count)
                {
                    DefaultTemplatePickerCbx.SelectedIndex = 0;
                }
            }

        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            ObservableList<Button> winButtons = [];
            Button SaveAllButton = new Button
            {
                Content = "Ok"
            };
            SaveAllButton.Click += new RoutedEventHandler(OkButton_Click);
            winButtons.Add(SaveAllButton);
            RoutedEventHandler closeHandler = new RoutedEventHandler(Close_ButtonClick);
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, winButtons, true,"Close", closeHandler);
        }
        private void Close_ButtonClick(object sender, RoutedEventArgs e)
        {           
            _pageGenericWin?.Close();
        }
        public void RadioButtonInit()
        {

            if ((!mEmailAttachment.IsLinkEnabled && !mEmailAttachment.IsZipFolderAttachementEnabled) && (!string.IsNullOrEmpty(GingerRemoteExecutionUtils.GetReportHTMLServiceUrl()) && !string.IsNullOrEmpty(GingerRemoteExecutionUtils.GetReportDataServiceUrl())))
            {
                xAccountReportLink.IsChecked = true;
                return;
            }
            if (mEmailAttachment.IsAccountReportLinkEnabled)
            {
                xAccountReportLink.IsChecked = true;
            }
            else if (mEmailAttachment.IsLinkEnabled)
            {
                RadioLinkOption.IsChecked = true;
            }
            else
            {
                RadioZippedReportOption.IsChecked = true;
            }

            if (!string.IsNullOrEmpty(GingerRemoteExecutionUtils.GetReportHTMLServiceUrl()) && !string.IsNullOrEmpty(GingerRemoteExecutionUtils.GetReportDataServiceUrl()))
            {
                xAccountReportLink.IsEnabled = true;
            }
            else
            {
                xAccountReportLink.IsEnabled = false;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpression mVE = new ValueExpression(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>(), false, "", false);
            string extraInformationCalculated = string.Empty;
            mVE.Value = mEmailAttachment.ExtraInformation;
            extraInformationCalculated = mVE.ValueCalculated;
            if ((bool)UseAlternativeHTMLReportFolderCbx.IsChecked)
            {
                if ((extraInformationCalculated == null) || (extraInformationCalculated.Length < 3))
                {
                    Reporter.ToUser(eUserMsgKey.FolderNameTextBoxIsEmpty);
                    return;
                }
                else if (extraInformationCalculated.Length > 100)
                {
                    Reporter.ToUser(eUserMsgKey.FolderNamesAreTooLong);
                    return;
                }
            }

            if (mEmailAttachment != null)
            {
                mEmailAttachment.Name = DefaultTemplatePickerCbx.Text;
                mEmailAttachment.IsLinkEnabled = IsLinkEnabled;
                mEmailAttachment.IsAccountReportLinkEnabled = IsAccountReportLinkEnabled;
                mEmailAttachment.IsZipFolderAttachementEnabled = IsZipFolderAttachementEnabled;
            }

            _pageGenericWin.Close();
        }
        private void UseAlternativeHTMLReportFolderCbx_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)UseAlternativeHTMLReportFolderCbx.IsChecked)
            {
                HTMLReportFolderPanel.IsEnabled = true;
            }
        }
        private void xAccountReportLink_Checked(object sender, RoutedEventArgs e)
        {

            try
            {
                IsAccountReportLinkEnabled = true;
                IsLinkEnabled = false;
                IsZipFolderAttachementEnabled = false;
                ZipReportlbl.Visibility = Visibility.Collapsed;
                Linklbl.Visibility = Visibility.Collapsed;
                AccountReportlbl.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                String err = ex.Message;
            }


        }
        private void LinkOption_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                IsLinkEnabled = true;
                IsAccountReportLinkEnabled = false;
                IsZipFolderAttachementEnabled = false;
                ZipReportlbl.Visibility = Visibility.Collapsed;
                Linklbl.Visibility = Visibility.Visible;
                AccountReportlbl.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                String err = ex.Message;
            }
        }
        private void RadioZippedOption_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                IsLinkEnabled = false;
                IsAccountReportLinkEnabled = false;
                IsZipFolderAttachementEnabled = true;
                ZipReportlbl.Visibility = Visibility.Visible;
                Linklbl.Visibility = Visibility.Collapsed;
                AccountReportlbl.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                String err = ex.Message;
            }
        }
        private void UseAlternativeHTMLReportFolderCbx_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)UseAlternativeHTMLReportFolderCbx.IsChecked)
            {
                HTMLReportFolderPanel.IsEnabled = false;
            }
        }
        private void HTMLReportFolderTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void DefaultTemplatePickerCbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mEmailAttachment.SelectedHTMLReportTemplateID = ((HTMLReportConfiguration)DefaultTemplatePickerCbx.SelectedItem).ID;
        }

        public static bool HasWritePermission(string FilePath)
        {
            try
            {
                if (!FilePath.Trim().EndsWith("\\"))
                {
                    FilePath += @"\";
                }
                FileSystemSecurity security;
                if (File.Exists(FilePath))
                {
                    security = FileSystemAclExtensions.GetAccessControl(new FileInfo(FilePath));// File.GetAccessControl(FilePath);
                }
                else
                {
                    security = FileSystemAclExtensions.GetAccessControl(new DirectoryInfo(FilePath)); //Directory.GetAccessControl(System.IO.Path.GetDirectoryName(FilePath));
                }
                var rules = security.GetAccessRules(true, true, typeof(NTAccount));

                var currentUser = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                bool result = false;
                foreach (FileSystemAccessRule rule in rules)
                {
                    if (0 == (rule.FileSystemRights &
                        (FileSystemRights.WriteData | FileSystemRights.Write)))
                    {
                        continue;
                    }

                    if (rule.IdentityReference.Value.StartsWith("S-1-"))
                    {
                        var sid = new SecurityIdentifier(rule.IdentityReference.Value);
                        if (!currentUser.IsInRole(sid))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (!currentUser.IsInRole(rule.IdentityReference.Value))
                        {
                            continue;
                        }
                    }

                    if (rule.AccessControlType == AccessControlType.Deny)
                    {
                        return false;
                    }

                    if (rule.AccessControlType == AccessControlType.Allow)
                    {
                        result = true;
                    }
                }
                return result;
            }
            catch
            {
                return false;
            }
        }


    }
}
