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

        private void InitializeXSendEMailConfigView()
        {
            Enum.TryParse(mAct.MailOption, out Email.eEmailMethod defaultEmailMethod);
            UCSendEMailConfigView.Options options = new()
            {
                AttachmentsEnabled = true,
                SupportedAttachmentTypes = new eAttachmentType[] { eAttachmentType.File },
                AllowAttachmentExtraInformation = false,
                AllowZippedAttachment = false,
                CCEnabled = true,
                DefaultEmailMethod = defaultEmailMethod,
                FromDisplayNameEnabled = true,
                SupportedBodyContentTypes = new eBodyContentType[] { eBodyContentType.FreeText }
            };
            xSendEMailConfigView.Initialize(options);
            BindSendEMailConfigView();
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

        private void BindSendEMailConfigView()
        {
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

            xSendEMailConfigView.xAttachmentsGrid.DataSourceList = mAttachments;

            xSendEMailConfigView.FileAdded += xSendEMailConfigView_FileAdded;
            xSendEMailConfigView.EmailMethodChanged += xSendEMailConfigView_EmailMethodChanged;
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

        private sealed class Attachment : UCSendEMailConfigView.IAttachmentBindingAdapter
        {
            public eAttachmentType Type { get => eAttachmentType.File; }

            private string mName;
            public string Name
            {
                get
                {
                    return mName;
                }
                set
                {
                    if (value == mName)
                        return;
                    mName = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
            public string ExtraInformation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public bool Zipped { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public event PropertyChangedEventHandler? PropertyChanged;

            public Attachment(string filename)
            {
                mName = filename;
            }

            public void OnPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
