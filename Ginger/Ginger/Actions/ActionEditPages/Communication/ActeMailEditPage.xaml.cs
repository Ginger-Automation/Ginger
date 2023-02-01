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
using Amdocs.Ginger.Common;
using GingerCore.Actions.Communication;
using GingerCore.GeneralLib;
using CheckBox = System.Windows.Controls.CheckBox;
using Ginger.UserControlsLib.UCSendEMailConfig;
using System.Linq;
using System.ComponentModel;
using NUglify.Helpers;
using System.Diagnostics.CodeAnalysis;
using NPOI.HPSF;
using System.Dynamic;
using System.Collections.Generic;
using System.Xml.Schema;
using System.Linq.Dynamic.Core;

namespace Ginger.Actions.Communication
{
    /// <summary>
    /// Interaction logic for ActeMailEditPage.xaml
    /// </summary>
    public partial class ActeMailEditPage : Page
    {
        private ActeMail mAct;
        private ObservableList<Attachment> mAttachments;

        public ActeMailEditPage(ActeMail act)
        {
            InitializeComponent();
            mAct = act;
            CreateAttachmentList();
            InitializeXSendEMailConfigView();
        }

        [MemberNotNull(nameof(mAttachments))]
        private void CreateAttachmentList()
        {
            string attachmentFilenames = mAct.AttachmentFileName;
            if (attachmentFilenames == null)
                attachmentFilenames = "";

            mAttachments = new(attachmentFilenames.Split(";", StringSplitOptions.RemoveEmptyEntries).Select(filename => new Attachment(filename)));
            mAttachments.ForEach(attachment => attachment.PropertyChanged += Attachment_PropertyChanged);
            mAttachments.CollectionChanged += mAttachments_CollectionChanged;
        }

        private void Attachment_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            RefreshAttachmentFileName();
        }

        private void mAttachments_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RefreshAttachmentFileName();
        }

        private void RefreshAttachmentFileName()
        {
            if (mAttachments.Count == 0)
            {
                mAct.AttachmentFileName = "";
                return;
            }

            mAct.AttachmentFileName = mAttachments.Select(attachment => attachment.Name).Aggregate((aggr, filename) => $"{aggr};{filename}");
        }

        private void InitializeXSendEMailConfigView()
        {
            if (!Enum.TryParse(mAct.MailOption, out Email.eEmailMethod defaultEmailMethod))
            {
                mAct.MailOption = defaultEmailMethod.ToString();
            }

            UCSendEMailConfigView.Options options = new()
            {
                AttachmentsEnabled = true,
                SupportedAttachmentTypes = new eAttachmentType[] { eAttachmentType.File },
                AllowAttachmentExtraInformation = false,
                AllowZippedAttachment = false,
                AttachmentGridBindingMap = new()
                {
                    Type = nameof(Attachment.Type),
                    Name = nameof(Attachment.Name)
                },
                CCEnabled = true,
                DefaultEmailMethod = defaultEmailMethod,
                FromDisplayNameEnabled = true,
                SupportedBodyContentTypes = new eBodyContentType[] { eBodyContentType.FreeText }
            };
            xSendEMailConfigView.Initialize(options);

            if (string.IsNullOrEmpty(mAct.MailFromDisplayName))
            {
                mAct.MailFromDisplayName = "_Amdocs Ginger Automation";
            }

            if(string.IsNullOrEmpty(mAct.AttachmentDownloadPath))
            {
                mAct.AttachmentDownloadPath = @"~\\Documents\EmailAttachments";
            }

            BindSendEMailConfigView();
        }

        private void BindSendEMailConfigView()
        {
            xSendEMailConfigView.xActionTypeSendRadioButton.IsChecked = mAct.eMailActionType == ActeMail.eEmailActionType.SendEmail;
            xSendEMailConfigView.xActionTypeReadRadioButton.IsChecked = mAct.eMailActionType == ActeMail.eEmailActionType.ReadEmail;
            xSendEMailConfigView.xFromVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.MailFrom));
            xSendEMailConfigView.xFromDisplayNameVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.MailFromDisplayName));
            xSendEMailConfigView.xToVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.Mailto));
            xSendEMailConfigView.xCCVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.Mailcc));
            xSendEMailConfigView.xSubjectVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.Subject));
            xSendEMailConfigView.xBodyFreeTextVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.Body));
            xSendEMailConfigView.xSMTPHostVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.Host));
            xSendEMailConfigView.xSMTPPortVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.Port));
            BindingHandler.ActInputValueBinding(xSendEMailConfigView.xEnableSSLOrTLS, CheckBox.IsCheckedProperty, mAct.GetOrCreateInputParam(ActeMail.Fields.EnableSSL, "true"));
            BindingHandler.ObjFieldBinding(xSendEMailConfigView.xAddCustomCertificate, CheckBox.IsCheckedProperty, mAct, nameof(ActeMail.IsValidationRequired));
            BindingHandler.ObjFieldBinding(xSendEMailConfigView.xCertificatePathTextBox, TextBox.TextProperty, mAct, nameof(ActeMail.CertificatePath));
            BindingHandler.ObjFieldBinding(xSendEMailConfigView.xCertificatePasswordTextBox, TextBox.TextProperty, mAct, nameof(ActeMail.CertificatePasswordUCValueExpression));
            BindingHandler.ActInputValueBinding(xSendEMailConfigView.xConfigureCredentials, CheckBox.IsCheckedProperty, mAct.GetOrCreateInputParam(ActeMail.Fields.ConfigureCredential, "false"));
            xSendEMailConfigView.xSMTPUserVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.User));
            BindingHandler.ObjFieldBinding(xSendEMailConfigView.xSMTPPasswordTextBox, TextBox.TextProperty, mAct, nameof(ActeMail.Pass));

            xSendEMailConfigView.xUserEmailVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.ReadUserEmail));
            BindingHandler.ObjFieldBinding(xSendEMailConfigView.xUserPasswordTextBox, TextBox.TextProperty, mAct, nameof(ActeMail.ReadUserPassword));
            xSendEMailConfigView.xClientIdVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.MSGraphClientId));
            xSendEMailConfigView.xTenantIdVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.MSGraphTenantId));
            xSendEMailConfigView.xFilterFolderAllRadioButton.IsChecked = mAct.FilterFolder == EmailReadFilters.eFolderFilter.All;
            xSendEMailConfigView.xFilterFolderSpecificRadioButton.IsChecked = mAct.FilterFolder == EmailReadFilters.eFolderFilter.Specific;
            xSendEMailConfigView.xFilterFolderAllRadioButton.Checked += xFilterFolderRadioButton_SelectionChanged;
            xSendEMailConfigView.xFilterFolderSpecificRadioButton.Checked += xFilterFolderRadioButton_SelectionChanged;
            xSendEMailConfigView.xFilterFolderNameVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.FilterFolderName));
            xSendEMailConfigView.xFilterFromVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.FilterFrom));
            xSendEMailConfigView.xFilterToVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.FilterTo));
            xSendEMailConfigView.xFilterSubjectVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.FilterSubject));
            xSendEMailConfigView.xFilterBodyVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.FilterBody));
            xSendEMailConfigView.xHasAttachmentsComboBox.SelectedItem = FindComboBoxItem(
                xSendEMailConfigView.xHasAttachmentsComboBox, 
                item => (EmailReadFilters.eHasAttachmentsFilter)item.Value == mAct.FilterHasAttachments);
            xSendEMailConfigView.xFilterAttachmentContentTypeVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.FilterAttachmentContentType));
            xSendEMailConfigView.xDownloadAttachmentYesRadioButton.IsChecked = mAct.DownloadAttachments;
            xSendEMailConfigView.xDownloadAttachmentNoRadioButton.IsChecked = !mAct.DownloadAttachments;
            xSendEMailConfigView.xDownloadAttachmentYesRadioButton.Checked += (sender, e) => 
                mAct.DownloadAttachments = xSendEMailConfigView.xDownloadAttachmentYesRadioButton.IsChecked ?? false;
            xSendEMailConfigView.xAttachmentDownloadPathVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.AttachmentDownloadPath));
            xSendEMailConfigView.xFilterReceivedStartDateVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.FilterReceivedStartDate));
            xSendEMailConfigView.xFilterReceivedEndDateVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.FilterReceivedEndDate));

            xSendEMailConfigView.xAttachmentsGrid.DataSourceList = mAttachments;

            xSendEMailConfigView.AddFileAttachment += xSendEMailConfigView_FileAdded;
            xSendEMailConfigView.EmailMethodChanged += xSendEMailConfigView_EmailMethodChanged;
            xSendEMailConfigView.NameValueExpressionButtonClick += xSendEMailConfigView_NameValueExpressionButtonClick;
            xSendEMailConfigView.ActionTypeChanged += xSendEMailConfigView_ActionTypeChanged;
            xSendEMailConfigView.HasAttachmentsSelectionChanged += xSendEMailConfigView_HasAttachmentsSelectionChanged;
        }

        private static ComboEnumItem FindComboBoxItem(ComboBox comboBox, Predicate<ComboEnumItem> predicate)
        {
            foreach(ComboEnumItem item in comboBox.Items)
            {
                if (predicate(item))
                    return item;
            }

            return null;
        }

        private void xFilterFolderRadioButton_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (xSendEMailConfigView.xFilterFolderAllRadioButton.IsChecked ?? false)
                mAct.FilterFolder = EmailReadFilters.eFolderFilter.All;
            else if (xSendEMailConfigView.xFilterFolderSpecificRadioButton.IsChecked ?? false)
                mAct.FilterFolder = EmailReadFilters.eFolderFilter.Specific;
        }

        private void xSendEMailConfigView_ActionTypeChanged(ActeMail.eEmailActionType selectedActionType)
        {
            mAct.eMailActionType = selectedActionType;
        }

        private void xSendEMailConfigView_NameValueExpressionButtonClick(object sender, RoutedEventArgs e)
        {
            Attachment currentItem = (Attachment)xSendEMailConfigView.xAttachmentsGrid.CurrentItem;
            int index = mAttachments.IndexOf(currentItem);
            ValueExpressionEditorPage veEditorPage = new(currentItem, nameof(Attachment.Name), null);
            veEditorPage.ShowAsWindow();
            mAttachments.RemoveAt(index);
            mAttachments.Insert(index, currentItem);
        }

        private void xSendEMailConfigView_FileAdded(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new()
            {
                DefaultExt = ".*",
                Filter = "All Files (*.*)|*.*"
            };
            string filename = General.SetupBrowseFile(openFileDialog);

            if (string.IsNullOrEmpty(filename))
            {
                return;
            }

            mAttachments.Add(new Attachment(filename));
        }

        private void xSendEMailConfigView_EmailMethodChanged(Email.eEmailMethod selectedEmailMethod)
        {
            mAct.MailOption = selectedEmailMethod.ToString();
        }

        private void xSendEMailConfigView_HasAttachmentsSelectionChanged(EmailReadFilters.eHasAttachmentsFilter selectedValue)
        {
            mAct.FilterHasAttachments = selectedValue;
        }

        public sealed class Attachment : INotifyPropertyChanged
        {
            public eAttachmentType Type { get => eAttachmentType.File; }
            private string filename;
            public string Name
            {
                get => filename;
                set
                {
                    if (value == filename)
                        return;
                    filename = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                }
            }
            public Attachment(string filename)
            {
                this.filename = filename;
            }

            public event PropertyChangedEventHandler? PropertyChanged;
        }
    }
}
