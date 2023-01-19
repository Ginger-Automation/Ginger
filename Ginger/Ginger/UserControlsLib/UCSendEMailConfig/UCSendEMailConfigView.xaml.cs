using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger.Run.RunSetActions;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions.Communication;
using GingerCore.GeneralLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ginger.UserControlsLib.UCSendEMailConfig
{
    public enum eBodyContentType
    {
        FreeText,
        HTMLReport
    }

    public enum eAttachmentType
    {
        File,
        HTMLReport
    }

    /// <summary>
    /// Interaction logic for UCSendEMailConfigView.xaml
    /// </summary>
    public partial class UCSendEMailConfigView : UserControl
    {
        public class Options
        {
            public eBodyContentType[] SupportedBodyContentTypes { get; set; } = { eBodyContentType.FreeText };
            public int MaxBodyCharCount { get; set; } = 0;
            public bool FromDisplayNameEnabled { get; set; } = true;
            public bool CCEnabled { get; set; } = true;
            public bool AttachmentsEnabled { get; set; } = true;
            public bool AllowAttachmentExtraInformation { get; set; } = false;
            public bool AllowZippedAttachment { get; set; } = false;
            public eAttachmentType[] SupportedAttachmentTypes { get; set; } = { eAttachmentType.File };
            public Email.eEmailMethod DefaultEmailMethod { get; set; } = Email.eEmailMethod.SMTP;
        }

        public interface IAttachmentBindingAdapter : INotifyPropertyChanged
        {
            public eAttachmentType Type { get; }
            public string Name { get; set; }
            public string ExtraInformation { get; set; }
            public bool Zipped { get; set; }
        }

        private bool isDisplayNameFieldEnabled;

        public delegate void EmailMethodChangeHandler(Email.eEmailMethod selectedEmailMethod);
        public event EmailMethodChangeHandler? EmailMethodChanged;

        public event RoutedEventHandler? FileAdded;
        public event RoutedEventHandler? HTMLReportAdded;

        public UCSendEMailConfigView()
        {
            InitializeComponent();
            AddEncryptionHandlerForPasswordControls();
        }

        private void AddEncryptionHandlerForPasswordControls()
        {
            xSMTPPasswordTextBox.LostFocus += (sender, e) => xSMTPPasswordTextBox.Text = Encrypt(xSMTPPasswordTextBox.Text);
            xCertificatePasswordTextBox.LostFocus += (sender, e) => xCertificatePasswordTextBox.Text = Encrypt(xCertificatePasswordTextBox.Text);
        }

        public void Initialize(Options options)
        {
            SetBodyContentType(options.SupportedBodyContentTypes);
            SetMaxBodyCharCount(options.MaxBodyCharCount);
            isDisplayNameFieldEnabled = options.FromDisplayNameEnabled;
            SetCCVisibility(ConvertBooleanToVisibility(options.CCEnabled));
            if (options.AttachmentsEnabled)
                InitializeAttachmentsGrid(options);
            SetDefaultEmailMethod(options.DefaultEmailMethod);
        }

        private void SetBodyContentType(eBodyContentType[] supportedBodyContentTypes)
        {
            bool doesSupportMultipleBodyContentType = supportedBodyContentTypes.Length > 1;
            if (doesSupportMultipleBodyContentType)
                ShowBodyContentTypeRadioButtons(supportedBodyContentTypes);
            else
                MakeBodyContentTypeContainerVisible(supportedBodyContentTypes[0]);
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
            foreach (eBodyContentType bodyContenType in supportedBodyContentTypes)
            {
                contentTypeRadioButtonMap[bodyContenType].Visibility = Visibility.Visible;
                if(!isDefaultValueSet)
                {
                    contentTypeRadioButtonMap[bodyContenType].IsChecked = true;
                }
            }
        }

        private void MakeBodyContentTypeContainerVisible(eBodyContentType bodyContentType)
        {
            if (bodyContentType == eBodyContentType.FreeText)
                xBodyContentTypeFreeTextContainer.Visibility = Visibility.Visible;
            else if (bodyContentType == eBodyContentType.HTMLReport)
                xBodyContentTypeHTMLReportContainer.Visibility = Visibility.Visible;
        }

        private void SetMaxBodyCharCount(int maxBodyCharCount)
        {
            if (maxBodyCharCount <= 0 || maxBodyCharCount == int.MaxValue)
                return;

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
            SetAttachmentsGridViewDef(options.AllowAttachmentExtraInformation, options.AllowZippedAttachment);

            if(options.SupportedAttachmentTypes.Contains(eAttachmentType.HTMLReport))
                xAttachmentsGrid.AddToolbarTool("@AddHTMLReport_16x16.png", "Add Report", TriggerHTMLReportAddedEvent);
            if(options.SupportedAttachmentTypes.Contains(eAttachmentType.File))
                xAttachmentsGrid.AddToolbarTool("@AddScript_16x16.png", "Add File", TriggerFileAddedEvent);

            //xAttachmentsGrid.DataSourceList = runSetActionHTMLReportSendEmail.EmailAttachments;
        }

        private void SetAttachmentsGridViewDef(bool allowExtraInformation, bool allowZippedAttachment)
        {
            GridViewDef gridViewDef = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> gridColsView = new ObservableList<GridColView>();
            gridViewDef.GridColsView = gridColsView;

            gridColsView.Add(new GridColView()
            {
                Header = "Type",
                Field = nameof(IAttachmentBindingAdapter.Type),
                WidthWeight = 100,
                BindingMode = BindingMode.OneTime
            });
            gridColsView.Add(new GridColView() 
            { 
                Header = "Name", 
                Field = nameof(IAttachmentBindingAdapter.Name), 
                WidthWeight = 200 
            });
            gridColsView.Add(new GridColView() 
            { 
                Field = "...", 
                Header = "...", 
                WidthWeight = 20,
                StyleType = GridColView.eGridColStyleType.Template, 
                CellTemplate = (DataTemplate)xAttachmentsTab.Resources["NameVEButtonCellTemplate"] });

            if (allowExtraInformation)
            {
                gridColsView.Add(new GridColView() 
                { 
                    Header = "Extra Information", 
                    Field = nameof(IAttachmentBindingAdapter.ExtraInformation), 
                    WidthWeight = 250 
                });
            }
            if (allowZippedAttachment)
            {
                gridColsView.Add(new GridColView() 
                { 
                    Header = "Zipped", 
                    Field = nameof(IAttachmentBindingAdapter.Zipped), 
                    WidthWeight = 50, 
                    HorizontalAlignment = HorizontalAlignment.Center, 
                    StyleType = GridColView.eGridColStyleType.Template, 
                    CellTemplate = (DataTemplate)xAttachmentsTab.Resources["ZippedCellTemplate"] });
            }

            xAttachmentsGrid.SetAllColumnsDefaultView(gridViewDef);
            xAttachmentsGrid.InitViewItems();
        }

        private void SetDefaultEmailMethod(Email.eEmailMethod defaultEmailMethod)
        {
            switch(defaultEmailMethod)
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

        private void xBodyContentType_Changed(object sender, RoutedEventArgs e)
        {
            xBodyContentTypeFreeTextContainer.Visibility = ConvertBooleanToVisibility(xBodyContentTypeFreeTextRadioButton.IsChecked ?? false);
            xBodyContentTypeHTMLReportContainer.Visibility = ConvertBooleanToVisibility(xBodyContentTypeHTMLReportRadioButton.IsChecked ?? false);
        }

        private Visibility ConvertBooleanToVisibility(bool boolean)
        {
            return boolean ? Visibility.Visible : Visibility.Collapsed;
        }

        private void TriggerFileAddedEvent(object sender, RoutedEventArgs e)
        {
            FileAdded?.Invoke(sender, e);
        }

        private void TriggerHTMLReportAddedEvent(object sender, RoutedEventArgs e)
        {
            HTMLReportAdded?.Invoke(sender, e);
        }

        private void xEmailMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeFromDisplayFieldVisibility();
            TriggerEmailMethodChangedEvent();
        }

        private void ChangeFromDisplayFieldVisibility()
        {
            if (!isDisplayNameFieldEnabled)
                return;

            if (xEmailMethodSMTP.IsSelected)
                xFromDisplayContainer.Visibility = Visibility.Visible;
            else
                xFromDisplayContainer.Visibility = Visibility.Collapsed;
        }

        private void TriggerEmailMethodChangedEvent()
        {
            ComboBoxItem selectedEmailMethodComboBoxItem = (ComboBoxItem)xEmailMethod.SelectedItem;
            Email.eEmailMethod selectedEmailMethod = Enum.Parse<Email.eEmailMethod>((string)selectedEmailMethodComboBoxItem.Tag);
            EmailMethodChanged?.Invoke(selectedEmailMethod);
        }

        private void xNameVEButton_Click(object sender, RoutedEventArgs e)
        {
            IAttachmentBindingAdapter item = (IAttachmentBindingAdapter)xAttachmentsGrid.CurrentItem;

            if (item.Type == eAttachmentType.File)
            {
                ValueExpressionEditorPage veEditorPage = new(xAttachmentsGrid.CurrentItem, nameof(EmailAttachment.Name), null);
                veEditorPage.ShowAsWindow();
            }
        }

        private string Encrypt(string value)
        {
            if (EncryptionHandler.IsStringEncrypted(value))
                return value;

            return EncryptionHandler.EncryptwithKey(value);
        }

        private void xBrowseCertificateButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            openFileDialog.DefaultExt = "*.crt";
            openFileDialog.Filter = "CRT Files (*.crt)|*.crt";
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
                        newFileName = newFileName.Substring(0, newFileName.IndexOf(copySufix));
                    }
                    newFileName = newFileName + copySufix + fileNum.ToString() + System.IO.Path.GetExtension(destinationFilePath);
                    destinationFilePath = System.IO.Path.Combine(targetPath, newFileName);
                }

                System.IO.File.Copy(filename, destinationFilePath, true);
                xCertificatePathTextBox.Text = @"~\Documents\EmailCertificates\" + System.IO.Path.GetFileName(destinationFilePath);
            }
        }
    }
}