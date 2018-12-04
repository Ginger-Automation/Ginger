#region License
/*
Copyright © 2014-2018 European Support Limited

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
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Ginger.UserControls;
using GingerCore;
using GingerCore.GeneralLib;
using Ginger.Reports;
using GingerCore.Actions;
using Ginger.Actions;
using amdocs.ginger.GingerCoreNET;

namespace Ginger.Run.RunSetActions
{
    /// <summary>
    /// Interaction logic for RunSetActionHTMLReportSendEmailEditPage.xaml
    /// </summary>
    public partial class RunSetActionHTMLReportSendEmailEditPage : Page
    {
        private RunSetActionHTMLReportSendEmail runSetActionHTMLReportSendEmail;
       public enum eAttachmentType
        {
            [EnumValueDescription("Report")]
            Report,
            [EnumValueDescription("File")]
            File           
        }
        public RunSetActionHTMLReportSendEmailEditPage(RunSetActionHTMLReportSendEmail runSetActionHTMLReportSendEmail)
        {
            InitializeComponent();
            this.runSetActionHTMLReportSendEmail = runSetActionHTMLReportSendEmail;
            if (runSetActionHTMLReportSendEmail.Email == null)
            {
                runSetActionHTMLReportSendEmail.Email = new Email();                
            }                
            MailFromTextBox.Init(runSetActionHTMLReportSendEmail, RunSetActionHTMLReportSendEmail.Fields.MailFrom);
            MailToTextBox.Init(runSetActionHTMLReportSendEmail, RunSetActionHTMLReportSendEmail.Fields.MailTo);
            MailCCTextBox.Init(runSetActionHTMLReportSendEmail, RunSetActionHTMLReportSendEmail.Fields.MailCC);
            SubjectTextBox.Init(runSetActionHTMLReportSendEmail, RunSetActionHTMLReportSendEmail.Fields.Subject);

            BodyTextBox.Init(runSetActionHTMLReportSendEmail, RunSetActionHTMLReportSendEmail.Fields.Bodytext);
            CommentTextBox.Init(runSetActionHTMLReportSendEmail, RunSetActionHTMLReportSendEmail.Fields.Comments);
            BodyTextBox.AdjustHight(100);
            App.ObjFieldBinding(xSMTPPortTextBox, TextBox.TextProperty, runSetActionHTMLReportSendEmail.Email, Email.Fields.SMTPPort);
            App.ObjFieldBinding(xSMTPPassTextBox, TextBox.TextProperty, runSetActionHTMLReportSendEmail.Email, Email.Fields.SMTPPass);
            App.FillComboFromEnumVal(xEmailMethodComboBox, runSetActionHTMLReportSendEmail.Email.EmailMethod);
            xSMTPMailHostTextBox.Init(runSetActionHTMLReportSendEmail, RunSetActionHTMLReportSendEmail.Fields.MailHost);            
            xSMTPUserTextBox.Init(runSetActionHTMLReportSendEmail, RunSetActionHTMLReportSendEmail.Fields.MailUser);            
            App.ObjFieldBinding(xEmailMethodComboBox, ComboBox.SelectedValueProperty, runSetActionHTMLReportSendEmail.Email, Email.Fields.EmailMethod);            
            App.ObjFieldBinding(xcbEnableSSL, CheckBox.IsCheckedProperty, runSetActionHTMLReportSendEmail.Email, Email.Fields.EnableSSL);
            App.ObjFieldBinding(xcbConfigureCredential, CheckBox.IsCheckedProperty, runSetActionHTMLReportSendEmail.Email, Email.Fields.ConfigureCredential);
            if (string.IsNullOrEmpty(runSetActionHTMLReportSendEmail.MailTo))
            {
                runSetActionHTMLReportSendEmail.MailTo = App.UserProfile.UserEmail;
            }
            InitAttachmentsGrid();
            RadioButtonInit();
        }
        public void RadioButtonInit()
        {
            string currentValue = runSetActionHTMLReportSendEmail.HTMLReportTemplate.ToString();
            foreach (RadioButton rdb in Panel.Children)
                if (rdb.Tag.ToString() == currentValue)
                {
                    rdb.IsChecked = true;
                    break;
                }
        }
        private void InitAttachmentsGrid()
        {
            SetGridView();
            AttachmentsGrid.AddToolbarTool("@AddHTMLReport_16x16.png", "Add Report", HTMLReportsConfigurationConfigWindow);           
            AttachmentsGrid.AddToolbarTool("@AddScript_16x16.png", "Add File", new RoutedEventHandler(AddFile));
            
            if (runSetActionHTMLReportSendEmail.EmailAttachments == null) runSetActionHTMLReportSendEmail.EmailAttachments = new ObservableList<EmailAttachment>();
            AttachmentsGrid.RowDoubleClick += HTMLReportsConfigurationConfigWindow;
            AttachmentsGrid.DataSourceList = runSetActionHTMLReportSendEmail.EmailAttachments;
            DefaultTemplatePickerCbx_Binding();
        }

        private void AddFile(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.DefaultExt = ".*";
            dlg.Filter = "All Files (*.*)|*.*";

            if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {                
                runSetActionHTMLReportSendEmail.EmailAttachments.Add(new EmailAttachment() { Name = dlg.FileName, AttachmentType = EmailAttachment.eAttachmentType.File });
            }
        }

        private void AddReport(object sender, RoutedEventArgs e)
        {
            ReportTemplateSelector RTS = new ReportTemplateSelector();
            RTS.ShowAsWindow();
            if (RTS.SelectedReportTemplate != null)
            {
                runSetActionHTMLReportSendEmail.EmailAttachments.Add(new EmailAttachment() { Name = RTS.SelectedReportTemplate.Name, AttachmentType = EmailAttachment.eAttachmentType.Report });
            }
        }

        private void SetGridView()
        {

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            view.GridColsView = viewCols;

            viewCols.Add(new GridColView() { Field = EmailAttachment.Fields.AttachmentType, WidthWeight = 100, BindingMode = BindingMode.OneTime });
            viewCols.Add(new GridColView() { Field = EmailAttachment.Fields.Name, WidthWeight = 200 });
            viewCols.Add(new GridColView() { Field = "...", Header = "...", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.GridAttachment.Resources["ParamValueExpressionButton"]});
            viewCols.Add(new GridColView() { Field = EmailAttachment.Fields.ExtraInformation, WidthWeight = 250 });            
            viewCols.Add(new GridColView() { Field = EmailAttachment.Fields.ZipIt, WidthWeight = 50, Header = "Zip It", HorizontalAlignment = System.Windows.HorizontalAlignment.Center, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.GridAttachment.Resources["ReportAttachment"]});
            
            AttachmentsGrid.SetAllColumnsDefaultView(view);
            AttachmentsGrid.InitViewItems();
        }

        public void HTMLReportsConfigurationConfigWindow()
        {
            EmailHtmlReportAttachment emailAttachment;
            if (runSetActionHTMLReportSendEmail.EmailAttachments.Where(x => x.AttachmentType == EmailAttachment.eAttachmentType.Report).ToList().Count < 1)
            {                           
                    emailAttachment = new EmailHtmlReportAttachment { ZipIt = true, SelectedHTMLReportTemplateID = 1 };
                    runSetActionHTMLReportSendEmail.EmailAttachments.Add(emailAttachment);
                    HTMLReportAttachmentConfigurationPage pg = new HTMLReportAttachmentConfigurationPage(emailAttachment);
                    pg.ShowAsWindow();                           
            }
            else if(AttachmentsGrid.CurrentItem!=null)
            {
                if (AttachmentsGrid.CurrentItem.GetType() == typeof(EmailHtmlReportAttachment))
                {
                    emailAttachment = ((EmailHtmlReportAttachment)AttachmentsGrid.CurrentItem);
                    if (emailAttachment != null)
                    {
                        HTMLReportAttachmentConfigurationPage pg = new HTMLReportAttachmentConfigurationPage(emailAttachment);
                        pg.ShowAsWindow();
                    }
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKeys.NoItemWasSelected);
            }
        }
        private void HTMLReportsConfigurationConfigWindow(object sender, EventArgs e)
        {            
            HTMLReportsConfigurationConfigWindow();
        }
        private void HTMLReportsConfigurationConfigWindow(object sender, System.Windows.RoutedEventArgs e)
        {
            HTMLReportsConfigurationConfigWindow();
        }       
        private void RadioFreeTextOption_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                comment.Visibility = Visibility.Collapsed;
                BodyTextBox.Visibility = Visibility.Visible;
                runSetActionHTMLReportSendEmail.HTMLReportTemplate = RunSetActionHTMLReportSendEmail.eHTMLReportTemplate.FreeText;
            }
            catch (Exception ex)
            {
                String err = ex.Message;
            }
        }
        private void RadioHTMLMailOption_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                comment.Visibility = Visibility.Visible;
                BodyTextBox.Visibility = Visibility.Collapsed;
                runSetActionHTMLReportSendEmail.HTMLReportTemplate = RunSetActionHTMLReportSendEmail.eHTMLReportTemplate.HTMLReport;
            }
            catch (Exception ex)
            {
                String err = ex.Message;
            }
        }
        private void DefaultTemplatePickerCbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            runSetActionHTMLReportSendEmail.selectedHTMLReportTemplateID = ((HTMLReportConfiguration)DefaultTemplatePickerCbx.SelectedItem).ID;
        }
        public void DefaultTemplatePickerCbx_Binding()
        {
            DefaultTemplatePickerCbx.ItemsSource = null;

            ObservableList<HTMLReportConfiguration> HTMLReportConfigurations = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>();
            if ((App.UserProfile.Solution != null) &&  (HTMLReportConfigurations.Count > 0))
            {
                DefaultTemplatePickerCbx.ItemsSource = HTMLReportConfigurations;
                DefaultTemplatePickerCbx.DisplayMemberPath = HTMLReportConfiguration.Fields.Name;
                DefaultTemplatePickerCbx.SelectedValuePath = HTMLReportConfiguration.Fields.ID;
                if (runSetActionHTMLReportSendEmail.selectedHTMLReportTemplateID == 0)
                {
                    DefaultTemplatePickerCbx.SelectedIndex = DefaultTemplatePickerCbx.Items.IndexOf(HTMLReportConfigurations.Where(x => (x.IsDefault == true)).FirstOrDefault());
                }
                DefaultTemplatePickerCbx.SelectedIndex = DefaultTemplatePickerCbx.Items.IndexOf(HTMLReportConfigurations.Where(x => (x.ID == runSetActionHTMLReportSendEmail.selectedHTMLReportTemplateID)).FirstOrDefault());
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

        private void GridParamVEButton_Click(object sender, RoutedEventArgs e)
        {
            EmailAttachment item =(EmailAttachment) AttachmentsGrid.CurrentItem;
            
            if(item.AttachmentType == EmailAttachment.eAttachmentType.File)
            {
                ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(AttachmentsGrid.CurrentItem, EmailAttachment.Fields.Name, true);

                VEEW.ShowAsWindow();
            }
        }

        private void xcbConfigureCredential_Checked(object sender, RoutedEventArgs e)
        {            
                xSMTPUserTextBox.Visibility = Visibility.Visible;
                xSMTPPassTextBox.Visibility = Visibility.Visible;
                xLabelPass.Visibility = Visibility.Visible;
                xLabelUser.Visibility = Visibility.Visible;
        }

        private void xSMTPPassTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            bool res, checkValueDecrypt;
            res = false;
            checkValueDecrypt = true;                  
            EncryptionHandler.DecryptString(xSMTPPassTextBox.Text, ref checkValueDecrypt);

            if (!checkValueDecrypt) xSMTPPassTextBox.Text = EncryptionHandler.EncryptString(xSMTPPassTextBox.Text, ref res);
        }

        private void xcbConfigureCredential_Unchecked(object sender, RoutedEventArgs e)
        {
            xSMTPUserTextBox.Visibility = Visibility.Collapsed;
            xSMTPPassTextBox.Visibility = Visibility.Collapsed;
            xLabelPass.Visibility = Visibility.Collapsed;
            xLabelUser.Visibility = Visibility.Collapsed;
        }
    }
}
