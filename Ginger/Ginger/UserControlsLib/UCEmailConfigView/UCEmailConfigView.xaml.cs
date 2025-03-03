#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions.Communication;
using GingerCore.GeneralLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.UserControlsLib.UCEmailConfigView
{
    public enum eBodyContentType
    {
        FreeText,
        HTMLReport
    }

    public enum eAttachmentType
    {
        [EnumValueDescription("File")]
        File,
        [EnumValueDescription("Report")]
        HTMLReport
    }

    /// <summary>
    /// Interaction logic for UCEmailConfigView.xaml
    /// </summary>
    public partial class UCEmailConfigView : UserControl
    {
        public class Options
        {
            public Context? Context { get; set; } = null;
            public eBodyContentType[] SupportedBodyContentTypes { get; set; } = { eBodyContentType.FreeText };
            public int MaxBodyCharCount { get; set; } = 0;
            public bool FromDisplayNameEnabled { get; set; } = true;
            public bool ReadDisplayNameEnabled { get; set; } = true;
            public bool CCEnabled { get; set; } = true;
            public bool AttachmentsEnabled { get; set; } = true;
            public bool AllowAttachmentExtraInformation { get; set; } = false;
            public bool AllowZippedAttachment { get; set; } = false;
            public eAttachmentType[] SupportedAttachmentTypes { get; set; } = { eAttachmentType.File };
            public AttachmentGridBindingMap AttachmentGridBindingMap { get; set; } = new AttachmentGridBindingMap();
            public Email.eEmailMethod DefaultEmailMethod { get; set; } = Email.eEmailMethod.SMTP;
            public Email.readEmailMethod DefaultReadEmailMethod { get; set; } = Email.readEmailMethod.MSGraphAPI;
            public eBodyContentType DefaultBodyContentType { get; set; } = eBodyContentType.FreeText;
        }

        public sealed class AttachmentGridBindingMap
        {
            public string Type { get; set; } = nameof(Type);
            public string Name { get; set; } = nameof(Name);
            public string ExtraInformation { get; set; } = nameof(ExtraInformation);
            public string Zipped { get; set; } = nameof(Zipped);
        }

        private bool mIsDisplayNameFieldEnabled;
        private bool readEmailFieldEnabled;
        private Context? mContext;

        public delegate void EmailMethodChangeHandler(Email.eEmailMethod selectedEmailMethod);
        public event EmailMethodChangeHandler? EmailMethodChanged;
        public delegate void ReadmailMethodChangeHandler(ActeMail.ReadEmailActionType selectedReadEmailMethod);
        public event ReadmailMethodChangeHandler? ReadmailMethodChanged;


        public delegate void ActionTypeChangedHandler(ActeMail.eEmailActionType selectedActionType);
        public event ActionTypeChangedHandler? ActionTypeChanged;
        public delegate void HasAttachmentsSelectionChangedHandler(EmailReadFilters.eHasAttachmentsFilter selectedValue);
        public event HasAttachmentsSelectionChangedHandler? HasAttachmentsSelectionChanged;

        public event RoutedEventHandler? AddFileAttachment;
        public event RoutedEventHandler? AddHTMLReportAttachment;
        public event RoutedEventHandler? AttachmentNameVEButtonClick;

        public UCEmailConfigView()
        {
            InitializeComponent();
            AddEncryptionHandlerForPasswordControls();
        }

        private void AddEncryptionHandlerForPasswordControls()
        {
            xSMTPPasswordTextBox.LostFocus += (_, _) => xSMTPPasswordTextBox.Text = EncryptPassword(xSMTPPasswordTextBox.Text);
            xCertificatePasswordTextBox.LostFocus += (_, _) => xCertificatePasswordTextBox.Text = EncryptPassword(xCertificatePasswordTextBox.Text);
            xUserPasswordTextBox.LostFocus += (_, _) => xUserPasswordTextBox.Text = EncryptPassword(xUserPasswordTextBox.Text);
        }

        private string EncryptPassword(string password)
        {
            if (!string.IsNullOrEmpty(password) && !password.Contains("{Var Name"))
            {
                return Encrypt(password);
            }
            return password;
        }

        public void Initialize(Options options)
        {
            mContext = options.Context;
            SetBodyContentType(options.SupportedBodyContentTypes, options.DefaultBodyContentType);
            SetMaxBodyCharCount(options.MaxBodyCharCount);
            mIsDisplayNameFieldEnabled = options.FromDisplayNameEnabled;
            readEmailFieldEnabled = options.ReadDisplayNameEnabled;
            SetCCVisibility(ConvertBooleanToVisibility(options.CCEnabled));
            if (options.AttachmentsEnabled)
            {
                InitializeAttachmentsGrid(options);
            }
            SetDefaultEmailMethod(options.DefaultEmailMethod);

            xEmailReadMethod.SelectionChanged += xReadEmailMethod_SelectionChanged;
            SetDefaultReadmailMethod(options.DefaultReadEmailMethod);
            InitializeHasAttachmentsComboBoxItems();
        }

        private void SetBodyContentType(eBodyContentType[] supportedBodyContentTypes, eBodyContentType defaultBodyContentType)
        {
            bool doesSupportMultipleBodyContentType = supportedBodyContentTypes.Length > 1;
            if (doesSupportMultipleBodyContentType)
            {
                ShowBodyContentTypeRadioButtons(supportedBodyContentTypes);
            }
            else
            {
                MakeBodyContentTypeContainerVisible(supportedBodyContentTypes[0]);
            }
            SetDefaultBodyContentType(defaultBodyContentType, supportedBodyContentTypes);
        }

        private void ShowBodyContentTypeRadioButtons(eBodyContentType[] supportedBodyContentTypes)
        {
            xBodyContentTypeRadioButtonsContainer.Visibility = Visibility.Visible;

            Dictionary<eBodyContentType, RadioButton> contentTypeRadioButtonMap = new()
            {
                { eBodyContentType.FreeText, xBodyContentTypeFreeTextRadioButton },
                { eBodyContentType.HTMLReport, xBodyContentTypeHTMLReportRadioButton }
            };

            bool isDefaultValueSet = false;
            foreach (eBodyContentType bodyContentType in supportedBodyContentTypes)
            {
                contentTypeRadioButtonMap[bodyContentType].Visibility = Visibility.Visible;
                if (!isDefaultValueSet)
                {
                    contentTypeRadioButtonMap[bodyContentType].IsChecked = true;
                }
            }
        }

        private void MakeBodyContentTypeContainerVisible(eBodyContentType bodyContentType)
        {
            if (bodyContentType == eBodyContentType.FreeText)
            {
                xBodyContentTypeFreeTextContainer.Visibility = Visibility.Visible;
            }
            else if (bodyContentType == eBodyContentType.HTMLReport)
            {
                xBodyContentTypeHTMLReportContainer.Visibility = Visibility.Visible;
            }
        }

        private void SetMaxBodyCharCount(int maxBodyCharCount)
        {
            if (maxBodyCharCount is <= 0 or int.MaxValue)
            {
                return;
            }

            xBodyFreeTextVE.ValueTextBox.MaxLength = maxBodyCharCount;
            xMaxBodyLengthLabel.Content = $"(upto {maxBodyCharCount} characters)";
            xMaxBodyLengthLabel.Visibility = Visibility.Visible;
        }

        private void SetCCVisibility(Visibility visibility)
        {
            xCCContainer.Visibility = visibility;
        }

        private void InitializeAttachmentsGrid(Options options)
        {
            xAttachmentsTab.Visibility = Visibility.Visible;
            SetAttachmentsGridViewDef(options);

            if (options.SupportedAttachmentTypes.Contains(eAttachmentType.HTMLReport))
            {
                xAttachmentsGrid.AddToolbarTool("@AddHTMLReport_16x16.png", "Add Report", TriggerAddHTMLReportAttachmentEvent);
            }
            if (options.SupportedAttachmentTypes.Contains(eAttachmentType.File))
            {
                xAttachmentsGrid.AddToolbarTool("@AddScript_16x16.png", "Add File", TriggerAddFileAttachmentEvent);
            }
        }

        private void SetAttachmentsGridViewDef(Options options)
        {
            bool isMultipleFileTypeSupported = options.SupportedAttachmentTypes.Length > 1;
            bool isExtraInformationAllowed = options.AllowAttachmentExtraInformation;
            bool isZippedAttachmentAllowed = options.AllowZippedAttachment;
            AttachmentGridBindingMap bindingMap = options.AttachmentGridBindingMap;

            GridViewDef gridViewDef = new(GridViewDef.DefaultViewName);
            ObservableList<GridColView> gridColsView = [];
            gridViewDef.GridColsView = gridColsView;

            if (isMultipleFileTypeSupported)
            {
                gridColsView.Add(new GridColView()
                {
                    Header = "Type",
                    Field = bindingMap.Type,
                    WidthWeight = 100,
                    BindingMode = BindingMode.OneTime
                });
            }

            gridColsView.Add(new GridColView()
            {
                Header = "Name",
                Field = bindingMap.Name,
                WidthWeight = 200
            });

            gridColsView.Add(new GridColView()
            {
                Field = "...",
                Header = "...",
                WidthWeight = 20,
                StyleType = GridColView.eGridColStyleType.Template,
                CellTemplate = (DataTemplate)xAttachmentsTab.Resources["NameVEButtonCellTemplate"]
            });

            if (isExtraInformationAllowed)
            {
                gridColsView.Add(new GridColView()
                {
                    Header = "Extra Information",
                    Field = bindingMap.ExtraInformation,
                    WidthWeight = 250
                });
            }

            if (isZippedAttachmentAllowed)
            {
                gridColsView.Add(new GridColView()
                {
                    Header = "Zipped",
                    Field = bindingMap.Zipped,
                    WidthWeight = 50,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    StyleType = GridColView.eGridColStyleType.Template,
                    CellTemplate = (DataTemplate)xAttachmentsTab.Resources["ZippedCellTemplate"]
                });
            }

            xAttachmentsGrid.SetAllColumnsDefaultView(gridViewDef);
            xAttachmentsGrid.InitViewItems();
        }

        private void SetDefaultEmailMethod(Email.eEmailMethod defaultEmailMethod)
        {
            switch (defaultEmailMethod)
            {
                case Email.eEmailMethod.SMTP:
                    xEmailMethodSMTP.IsSelected = true;
                    return;
                case Email.eEmailMethod.OUTLOOK:
                    xEmailMethodOutlook.IsSelected = true;
                    return;
                default:
                    throw new NotImplementedException($"logic for setting {defaultEmailMethod} as default email method is not implemented");
            }
        }

        private void SetDefaultReadmailMethod(Email.readEmailMethod defaultReadmailMethod)
        {
            switch (defaultReadmailMethod)
            {
                case Email.readEmailMethod.MSGraphAPI:
                    xEmailReadMethodMSGraph.IsSelected = true;
                    return;
                case Email.readEmailMethod.IMAP:
                    xEmailReadMethodIMAP.IsSelected = true;
                    return;
                default:
                    throw new NotImplementedException($"logic for setting {defaultReadmailMethod} as default Raedemail method is not implemented");
            }
        }

        private void InitializeHasAttachmentsComboBoxItems()
        {
            xHasAttachmentsComboBox.BindControl(Enum.GetValues<EmailReadFilters.eHasAttachmentsFilter>());
            xHasAttachmentsComboBox.SelectionChanged += (_, _) => TriggerHasAttachmentsSelectionChanged();
        }

        private void TriggerHasAttachmentsSelectionChanged()
        {
            EmailReadFilters.eHasAttachmentsFilter selectedValue = (EmailReadFilters.eHasAttachmentsFilter)xHasAttachmentsComboBox.SelectedValue;
            HasAttachmentsSelectionChanged?.Invoke(selectedValue);
        }

        private void SetDefaultBodyContentType(eBodyContentType defaultBodyContentType, eBodyContentType[] supportedBodyContentType)
        {
            bool supportsOnlyOneBodyContentType = supportedBodyContentType.Length < 2;
            bool doesNotContainDefaultBodyContentType = !supportedBodyContentType.Contains(defaultBodyContentType);

            if (defaultBodyContentType == eBodyContentType.FreeText)
            {
                xBodyContentTypeFreeTextRadioButton.IsChecked = true;
                return;
            }

            if (supportsOnlyOneBodyContentType || doesNotContainDefaultBodyContentType)
            {
                return;
            }

            switch (defaultBodyContentType)
            {
                case eBodyContentType.FreeText:
                    xBodyContentTypeFreeTextRadioButton.IsChecked = true;
                    return;
                case eBodyContentType.HTMLReport:
                    xBodyContentTypeHTMLReportRadioButton.IsChecked = true;
                    return;
                default:
                    throw new NotImplementedException($"logic for setting {defaultBodyContentType} as default body content type is not implemented");
            }
        }


        private void xReadEmailMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeReadEmailFieldVisibility();
            TriggerReadEmailMethodChangedEvent();
        }

        private void ChangeReadEmailFieldVisibility()
        {
            if (xEmailReadMethodMSGraph.IsSelected)
            {
                xClientIdGrid.Visibility = Visibility.Visible;
                xTenantIdGrid.Visibility = Visibility.Visible;
                xImapHostGrid.Visibility = Visibility.Collapsed;
                xImapPortGrid.Visibility = Visibility.Collapsed;
                passwdLabel.Content = "User Password:";
                xUserEmailVE.ToolTip = "Please enter the User Id, that is configured in Azure Portal";
            }
            else
            {
                xClientIdGrid.Visibility = Visibility.Collapsed;
                xTenantIdGrid.Visibility = Visibility.Collapsed;
                xImapHostGrid.Visibility = Visibility.Visible;
                xImapPortGrid.Visibility = Visibility.Visible;
                passwdLabel.Content = "User App Password:";
                xUserEmailVE.ToolTip = "Please enter your IMAP Email Id";
            }
        }
        private void TriggerReadEmailMethodChangedEvent()
        {
            ComboBoxItem selectedReadmailMethodComboBoxItem = (ComboBoxItem)xEmailReadMethod.SelectedItem;
            ActeMail.ReadEmailActionType selectedReadmailMethod = Enum.Parse<ActeMail.ReadEmailActionType>((string)selectedReadmailMethodComboBoxItem.Tag);
            ReadmailMethodChanged?.Invoke(selectedReadmailMethod);
        }


        private void xActionType_Changed(object sender, RoutedEventArgs e)
        {
            ChangeDetailContainerVisibility();
            TriggerActionTypeChangedEvent();
        }

        private void ChangeDetailContainerVisibility()
        {
            if (xSendDetailContainer == null || xReadDetailContainer == null)
            {
                return;
            }

            if (xActionTypeSendRadioButton.IsChecked ?? false)
            {
                xSendDetailContainer.Visibility = Visibility.Visible;
                xReadDetailContainer.Visibility = Visibility.Collapsed;
            }
            else if (xActionTypeReadRadioButton.IsChecked ?? false)
            {
                xSendDetailContainer.Visibility = Visibility.Collapsed;
                xReadDetailContainer.Visibility = Visibility.Visible;
            }
        }

        private void TriggerActionTypeChangedEvent()
        {
            ActeMail.eEmailActionType selectedActionType = default;

            if (xActionTypeSendRadioButton.IsChecked ?? false)
            {
                selectedActionType = ActeMail.eEmailActionType.SendEmail;
            }
            else if (xActionTypeReadRadioButton.IsChecked ?? false)
            {
                selectedActionType = ActeMail.eEmailActionType.ReadEmail;
            }

            ActionTypeChanged?.Invoke(selectedActionType);
        }

        private void xBodyContentType_Changed(object sender, RoutedEventArgs e)
        {
            xBodyContentTypeFreeTextContainer.Visibility = ConvertBooleanToVisibility(xBodyContentTypeFreeTextRadioButton.IsChecked ?? false);
            xBodyContentTypeHTMLReportContainer.Visibility = ConvertBooleanToVisibility(xBodyContentTypeHTMLReportRadioButton.IsChecked ?? false);
        }

        private Visibility ConvertBooleanToVisibility(bool boolean)
        {
            return boolean ? Visibility.Visible : Visibility.Collapsed;
        }

        private void TriggerAddFileAttachmentEvent(object sender, RoutedEventArgs e)
        {
            RoutedEventHandler? handler = AddFileAttachment;
            handler?.Invoke(sender, e);
        }

        private void TriggerAddHTMLReportAttachmentEvent(object sender, RoutedEventArgs e)
        {
            RoutedEventHandler? handler = AddHTMLReportAttachment;
            handler?.Invoke(sender, e);
        }

        //AllSpecificChecked          

        private void xEmailMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeFromDisplayFieldVisibility();
            TriggerEmailMethodChangedEvent();
        }

        private void ChangeFromDisplayFieldVisibility()
        {
            if (!mIsDisplayNameFieldEnabled)
            {
                return;
            }

            if (xEmailMethodSMTP.IsSelected)
            {
                xFromDisplayContainer.Visibility = Visibility.Visible;
            }
            else
            {
                xFromDisplayContainer.Visibility = Visibility.Collapsed;
            }
        }

        private void TriggerEmailMethodChangedEvent()
        {
            ComboBoxItem selectedEmailMethodComboBoxItem = (ComboBoxItem)xEmailMethod.SelectedItem;
            Email.eEmailMethod selectedEmailMethod = Enum.Parse<Email.eEmailMethod>((string)selectedEmailMethodComboBoxItem.Tag);
            EmailMethodChanged?.Invoke(selectedEmailMethod);
        }

        private void xAttachmentNameVEButton_Click(object sender, RoutedEventArgs e)
        {
            RoutedEventHandler? handler = AttachmentNameVEButtonClick;
            handler?.Invoke(sender, e);
        }

        private string Encrypt(string value)
        {
            if (EncryptionHandler.IsStringEncrypted(value))
            {
                return value;
            }

            return EncryptionHandler.EncryptwithKey(value);
        }

        private void xBrowseCertificateButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                DefaultExt = "*.crt",
                Filter = "CRT Files (*.crt)|*.crt"
            };
            string SolutionFolder = WorkSpace.Instance.Solution.Folder.ToUpper();
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filename = openFileDialog.FileName.ToUpper();
                if (filename.Contains(SolutionFolder))
                {
                    filename = filename.Replace(SolutionFolder, @"~\");
                }
                filename = WorkSpace.Instance.SolutionRepository.ConvertFullPathToBeRelative(filename);
                xCertificatePathTextBox.Text = filename;
                string targetPath = System.IO.Path.Combine(SolutionFolder, @"Documents\EmailCertificates");
                if (!System.IO.Directory.Exists(targetPath))
                {
                    System.IO.Directory.CreateDirectory(targetPath);
                }

                string destinationFilePath = System.IO.Path.Combine(targetPath, System.IO.Path.GetFileName(filename));

                int fileNum = 1;
                string copySufix = "_Copy";
                while (System.IO.File.Exists(destinationFilePath))
                {
                    fileNum++;
                    string newFileName = System.IO.Path.GetFileNameWithoutExtension(destinationFilePath);
                    if (newFileName.IndexOf(copySufix) != -1)
                    {
                        newFileName = newFileName[..newFileName.IndexOf(copySufix)];
                    }
                    newFileName = newFileName + copySufix + fileNum.ToString() + System.IO.Path.GetExtension(destinationFilePath);
                    destinationFilePath = System.IO.Path.Combine(targetPath, newFileName);
                }

                System.IO.File.Copy(filename, destinationFilePath, true);
                xCertificatePathTextBox.Text = @"~\Documents\EmailCertificates\" + System.IO.Path.GetFileName(destinationFilePath);
            }
        }

        private void xFilterReceivedStartDateVEEditorButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage veEditorPage = new(xFilterReceivedStartDateTextBox, nameof(TextBox.Text), mContext!);
            veEditorPage.ShowAsWindow();
        }

        private void xReceivedStartDateDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime selectedDate = xReceivedStartDateDatePicker.SelectedDate ?? DateTime.Now;
            DateTime dateTime = new(selectedDate.Ticks, DateTimeKind.Local);

            xFilterReceivedStartDateTextBox.Text = dateTime.ToString("yyyy-MM-ddTHH:mm:ssK");
        }

        private void xFilterReceivedEndDateVEEditorButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage veEditorPage = new(xFilterReceivedEndDateTextBox, nameof(TextBox.Text), mContext!);
            veEditorPage.ShowAsWindow();
        }

        private void xReceivedEndDateDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime selectedDate = xReceivedEndDateDatePicker.SelectedDate ?? DateTime.Now;
            DateTime dateTime = new(selectedDate.Ticks, DateTimeKind.Local);

            xFilterReceivedEndDateTextBox.Text = dateTime.ToString("yyyy-MM-ddTHH:mm:ssK");
        }

        private void xUserPasswordVEEditorButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage veEditorPage = new(xUserPasswordTextBox, nameof(TextBox.Text), mContext!);
            veEditorPage.ShowAsWindow();
        }
    }
}