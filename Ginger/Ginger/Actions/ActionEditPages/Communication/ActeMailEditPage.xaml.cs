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

using System;
using System.Windows;
using System.Windows.Controls;
using Amdocs.Ginger.Common;
using GingerCore.Actions.Communication;
using GingerCore.GeneralLib;
using CheckBox = System.Windows.Controls.CheckBox;
using System.Linq;
using System.ComponentModel;
using NUglify.Helpers;
using System.Diagnostics.CodeAnalysis;
using NPOI.HPSF;
using System.Dynamic;
using System.Collections.Generic;
using System.Xml.Schema;
using System.Linq.Dynamic.Core;
using Ginger.UserControlsLib.UCEmailConfigView;

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
            {
                attachmentFilenames = "";
            }

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

            UCEmailConfigView.Options options = new()
            {
                Context = Context.GetAsContext(mAct.Context),
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
            xEmailConfigView.Initialize(options);

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
            xEmailConfigView.xActionTypeSendRadioButton.IsChecked = mAct.eMailActionType == ActeMail.eEmailActionType.SendEmail;
            xEmailConfigView.xActionTypeReadRadioButton.IsChecked = mAct.eMailActionType == ActeMail.eEmailActionType.ReadEmail;
            xEmailConfigView.xFromVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.MailFrom));
            xEmailConfigView.xFromDisplayNameVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.MailFromDisplayName));
            xEmailConfigView.xToVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.Mailto));
            xEmailConfigView.xCCVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.Mailcc));
            xEmailConfigView.xSubjectVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.Subject));
            xEmailConfigView.xBodyFreeTextVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.Body));
            xEmailConfigView.xSMTPHostVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.Host));
            xEmailConfigView.xSMTPPortVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.Port));
            BindingHandler.ActInputValueBinding(xEmailConfigView.xEnableSSLOrTLS, CheckBox.IsCheckedProperty, mAct.GetOrCreateInputParam(ActeMail.Fields.EnableSSL, "true"));
            BindingHandler.ObjFieldBinding(xEmailConfigView.xAddCustomCertificate, CheckBox.IsCheckedProperty, mAct, nameof(ActeMail.IsValidationRequired));
            BindingHandler.ObjFieldBinding(xEmailConfigView.xCertificatePathTextBox, TextBox.TextProperty, mAct, nameof(ActeMail.CertificatePath));
            BindingHandler.ObjFieldBinding(xEmailConfigView.xCertificatePasswordTextBox, TextBox.TextProperty, mAct, nameof(ActeMail.CertificatePasswordUCValueExpression));
            BindingHandler.ActInputValueBinding(xEmailConfigView.xConfigureCredentials, CheckBox.IsCheckedProperty, mAct.GetOrCreateInputParam(ActeMail.Fields.ConfigureCredential, "false"));
            xEmailConfigView.xSMTPUserVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.User));
            BindingHandler.ObjFieldBinding(xEmailConfigView.xSMTPPasswordTextBox, TextBox.TextProperty, mAct, nameof(ActeMail.Pass));

            xEmailConfigView.xUserEmailVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.ReadUserEmail));
            BindingHandler.ObjFieldBinding(xEmailConfigView.xUserPasswordTextBox, TextBox.TextProperty, mAct, nameof(ActeMail.ReadUserPassword));
            xEmailConfigView.xClientIdVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.MSGraphClientId));
            xEmailConfigView.xTenantIdVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.MSGraphTenantId));
            xEmailConfigView.xFilterFolderAllRadioButton.IsChecked = mAct.FilterFolder == EmailReadFilters.eFolderFilter.All;
            xEmailConfigView.xFilterFolderSpecificRadioButton.IsChecked = mAct.FilterFolder == EmailReadFilters.eFolderFilter.Specific;
            xEmailConfigView.xFilterFolderAllRadioButton.Checked += xFilterFolderRadioButton_SelectionChanged;
            xEmailConfigView.xFilterFolderSpecificRadioButton.Checked += xFilterFolderRadioButton_SelectionChanged;
            xEmailConfigView.xFilterFolderNameVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.FilterFolderNames));
            xEmailConfigView.xFilterFromVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.FilterFrom));
            xEmailConfigView.xFilterToVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.FilterTo));
            xEmailConfigView.xFilterSubjectVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.FilterSubject));
            xEmailConfigView.xFilterBodyVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.FilterBody));
            xEmailConfigView.xHasAttachmentsComboBox.SelectedItem = FindComboBoxItem(
                xEmailConfigView.xHasAttachmentsComboBox, 
                item => (EmailReadFilters.eHasAttachmentsFilter)item.Value == mAct.FilterHasAttachments);
            xEmailConfigView.xFilterAttachmentContentTypeVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.FilterAttachmentContentType));
            xEmailConfigView.xDownloadAttachmentYesRadioButton.IsChecked = mAct.DownloadAttachments;
            xEmailConfigView.xDownloadAttachmentNoRadioButton.IsChecked = !mAct.DownloadAttachments;
            xEmailConfigView.xDownloadAttachmentYesRadioButton.Checked += (_, _) => mAct.DownloadAttachments = true;
            xEmailConfigView.xDownloadAttachmentNoRadioButton.Checked += (_, _) => mAct.DownloadAttachments = false;
            xEmailConfigView.xAttachmentDownloadPathVE.Init(Context.GetAsContext(mAct.Context), mAct, nameof(ActeMail.AttachmentDownloadPath));
            BindingHandler.ObjFieldBinding(xEmailConfigView.xFilterReceivedStartDateTextBox, TextBox.TextProperty, mAct, nameof(ActeMail.FilterReceivedStartDate));
            BindingHandler.ObjFieldBinding(xEmailConfigView.xFilterReceivedEndDateTextBox, TextBox.TextProperty, mAct, nameof(ActeMail.FilterReceivedEndDate));

            xEmailConfigView.xAttachmentsGrid.DataSourceList = mAttachments;

            xEmailConfigView.AddFileAttachment += xSendEMailConfigView_FileAdded;
            xEmailConfigView.EmailMethodChanged += xSendEMailConfigView_EmailMethodChanged;
            xEmailConfigView.AttachmentNameVEButtonClick += xSendEMailConfigView_NameValueExpressionButtonClick;
            xEmailConfigView.ActionTypeChanged += xSendEMailConfigView_ActionTypeChanged;
            xEmailConfigView.HasAttachmentsSelectionChanged += xSendEMailConfigView_HasAttachmentsSelectionChanged;
        }

        private static ComboEnumItem FindComboBoxItem(ComboBox comboBox, Predicate<ComboEnumItem> predicate)
        {
            foreach(ComboEnumItem item in comboBox.Items)
            {
                if (predicate(item))
                {
                    return item;
                }
            }

            return null!;
        }

        private void xFilterFolderRadioButton_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (xEmailConfigView.xFilterFolderAllRadioButton.IsChecked ?? false)
            {
                mAct.FilterFolder = EmailReadFilters.eFolderFilter.All;
            }
            else if (xEmailConfigView.xFilterFolderSpecificRadioButton.IsChecked ?? false)
            {
                mAct.FilterFolder = EmailReadFilters.eFolderFilter.Specific;
            }
        }

        private void xSendEMailConfigView_ActionTypeChanged(ActeMail.eEmailActionType selectedActionType)
        {
            mAct.eMailActionType = selectedActionType;
        }

        private void xSendEMailConfigView_NameValueExpressionButtonClick(object sender, RoutedEventArgs e)
        {
            Attachment currentItem = (Attachment)xEmailConfigView.xAttachmentsGrid.CurrentItem;
            int index = mAttachments.IndexOf(currentItem);
            ValueExpressionEditorPage veEditorPage = new(currentItem, nameof(Attachment.Name), Context.GetAsContext(mAct.Context));
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
                    {
                        return;
                    }
                    filename = value;
                    PropertyChangedEventHandler? handler = PropertyChanged;
                    handler?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
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
