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

using Amdocs.Ginger.Common;
using Ginger.Actions;
using Ginger.Run.RunSetActions;
using GingerCore;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Security.Principal;
using System.Security.AccessControl;
using System.IO;
using amdocs.ginger.GingerCoreNET;
using GingerCore.DataSource;

using Amdocs.Ginger.Common.InterfacesLib;

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
               
        public HTMLReportAttachmentConfigurationPage(EmailHtmlReportAttachment emailAttachment)
        {
            InitializeComponent();
            mEmailAttachment = emailAttachment;
            Init();
        }

        private void Init()
        {
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(DefaultTemplatePickerCbx, ComboBox.SelectedValueProperty, mEmailAttachment, EmailHtmlReportAttachment.Fields.SelectedHTMLReportTemplateID);
            HTMLReportFolderTextBox.Init(null, mEmailAttachment, EmailAttachment.Fields.ExtraInformation, true, true, UCValueExpression.eBrowserType.Folder,"*.*", null); 
            
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(UseAlternativeHTMLReportFolderCbx, CheckBox.IsCheckedProperty, mEmailAttachment, EmailHtmlReportAttachment.Fields.IsAlternameFolderUsed);
            RadioButtonInit(mEmailAttachment.IsLinkEnabled);

            DefaultTemplatePickerCbx.ItemsSource = null;

            ObservableList<HTMLReportConfiguration> HTMLReportConfigurations = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>();
            if ( WorkSpace.Instance.Solution != null  && HTMLReportConfigurations.Count > 0)
            {
                DefaultTemplatePickerCbx.ItemsSource = HTMLReportConfigurations;
                DefaultTemplatePickerCbx.DisplayMemberPath = HTMLReportConfiguration.Fields.Name;
                DefaultTemplatePickerCbx.SelectedValuePath = HTMLReportConfiguration.Fields.ID;
            }
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {                            
            ObservableList<Button> winButtons = new ObservableList<Button>();
            Button SaveAllButton = new Button();
            SaveAllButton.Content = "Ok";
            SaveAllButton.Click += new RoutedEventHandler(OkButton_Click);
            winButtons.Add(SaveAllButton);
          
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, winButtons, false);
        }
      
        public void RadioButtonInit(bool IsLinkEnabled)
        {            
            if (IsLinkEnabled == true)
                RadioLinkOption.IsChecked = true;
            else
                RadioZippedReportOption.IsChecked = true;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpression mVE=new ValueExpression(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>(), false, "", false);
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
        
        private void LinkOption_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                IsLinkEnabled = true;
                ZipReportlbl.Visibility = Visibility.Collapsed;
                Linklbl.Visibility = Visibility.Visible;
            }
            catch(Exception ex)
            {
                String err = ex.Message;
            }
        }
        private void RadioZippedOption_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                IsLinkEnabled = false;
                ZipReportlbl.Visibility = Visibility.Visible;
                Linklbl.Visibility = Visibility.Collapsed;
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
                    security = File.GetAccessControl(FilePath);
                }
                else
                {
                    security = Directory.GetAccessControl(System.IO.Path.GetDirectoryName(FilePath));
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
                        return false;
                    if (rule.AccessControlType == AccessControlType.Allow)
                        result = true;
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
