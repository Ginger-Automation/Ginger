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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger.Reports;
using Ginger.UserControls;
using GingerCore.GeneralLib;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace Ginger.Run.RunSetActions
{
    /// <summary>
    /// Interaction logic for RunSetActionEmailReportEditPage.xaml
    /// </summary>
    public partial class RunSetActionSendEmailEditPage : Page
    {
        private RunSetActionSendEmail runSetActionEmailReport;

        
        public RunSetActionSendEmailEditPage(RunSetActionSendEmail runSetActionSendEmail)
        {/*
            InitializeComponent();
            
            this.runSetActionEmailReport = runSetActionSendEmail;

            if (runSetActionSendEmail.Email == null)
            {
                runSetActionSendEmail.Email = new Email();                
            }

            ObservableList<HTMLReportTemplate> HTMLReportTemplates = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportTemplate>();
            foreach (HTMLReportTemplate HT in HTMLReportTemplates)
            {
                CustomHTMLReportComboBox.Items.Add(HT.Name);
            }

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SMTPMailHostTextBox , TextBox.TextProperty, runSetActionSendEmail.Email, Email.Fields.SMTPMailHost);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SMTPPortTextBox, TextBox.TextProperty, runSetActionSendEmail.Email, Email.Fields.SMTPPort);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SMTPUserTextBox, TextBox.TextProperty, runSetActionSendEmail.Email, Email.Fields.SMTPUser);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SMTPPassTextBox, TextBox.TextProperty, runSetActionSendEmail.Email, Email.Fields.SMTPPass);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(MailFromTextBox, TextBox.TextProperty, runSetActionSendEmail.Email, Email.Fields.MailFrom);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(MailToTextBox, TextBox.TextProperty, runSetActionSendEmail.Email, Email.Fields.MailTo);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(MailCCTextBox, TextBox.TextProperty, runSetActionSendEmail.Email, Email.Fields.MailCC);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SubjectTextBox, TextBox.TextProperty, runSetActionSendEmail.Email, Email.Fields.Subject);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(BodyTextBox, TextBox.TextProperty, runSetActionSendEmail.Email, Email.Fields.Body);
            GingerCore.General.FillComboFromEnumObj(HTMLReportComboBox, runSetActionSendEmail.HTMLReportTemplate);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(HTMLReportComboBox, ComboBox.SelectedValueProperty, runSetActionSendEmail, RunSetActionSendEmail.Fields.HTMLReportTemplate);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(CustomHTMLReportComboBox, ComboBox.SelectedValueProperty, runSetActionSendEmail, RunSetActionSendEmail.Fields.CustomHTMLReportTemplate);
            GingerCore.General.FillComboFromEnumObj(EmailMethodComboBox, runSetActionSendEmail.Email.EmailMethod);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(EmailMethodComboBox, ComboBox.SelectedValueProperty, runSetActionSendEmail.Email, Email.Fields.EmailMethod);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SMTPMailHostTextBox, TextBox.TextProperty, runSetActionSendEmail.Email, Email.Fields.SMTPMailHost);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SMTPPortTextBox, TextBox.TextProperty, runSetActionSendEmail.Email, Email.Fields.SMTPPort);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SMTPUserTextBox, TextBox.TextProperty, runSetActionSendEmail.Email, Email.Fields.SMTPUser);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SMTPPassTextBox, TextBox.TextProperty, runSetActionSendEmail.Email, Email.Fields.SMTPPass);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(cbEnableSSL, CheckBox.IsCheckedProperty, runSetActionSendEmail.Email, Email.Fields.EnableSSL);
            */
            InitAttachmentsGrid();
        }

        private void InitAttachmentsGrid()
        {
            SetGridView();
            AttachmentsGrid.AddButton("Add Report", AddReport);
            AttachmentsGrid.AddButton("Add File", AddFile);
            if (runSetActionEmailReport.EmailAttachments == null) runSetActionEmailReport.EmailAttachments = new ObservableList<EmailAttachment>();
            AttachmentsGrid.DataSourceList = runSetActionEmailReport.EmailAttachments;
        }

        private void AddFile(object sender, RoutedEventArgs e)
        {
            if (General.SetupBrowseFile(new System.Windows.Forms.OpenFileDialog()
            {
                DefaultExt = ".*",
                Filter = "All Files (*.*)|*.*"
            },false) is string fileName)
            {
                runSetActionEmailReport.EmailAttachments.Add(new EmailAttachment() { Name = fileName, AttachmentType = EmailAttachment.eAttachmentType.File });
            }
        }

        private void AddReport(object sender, RoutedEventArgs e)
        {
            ReportTemplateSelector RTS = new ReportTemplateSelector();            
            RTS.ShowAsWindow();
            if (RTS.SelectedReportTemplate != null)
            {
                runSetActionEmailReport.EmailAttachments.Add(new EmailAttachment() { Name = RTS.SelectedReportTemplate.Name, AttachmentType = EmailAttachment.eAttachmentType.Report });
            }
        }

        private void SetGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            ObservableList<GridColView> viewCols = new ObservableList<GridColView>();
            view.GridColsView = viewCols;

            viewCols.Add(new GridColView() { Field = nameof(EmailAttachment.AttachmentType), WidthWeight = 50, BindingMode = BindingMode.OneTime });
            viewCols.Add(new GridColView() { Field = nameof(EmailAttachment.Name), WidthWeight = 300 });
            viewCols.Add(new GridColView() { Field = nameof(EmailAttachment.ZipIt), WidthWeight = 50, StyleType = GridColView.eGridColStyleType.CheckBox });            
            
            AttachmentsGrid.SetAllColumnsDefaultView(view);
            AttachmentsGrid.InitViewItems();
        }

        private void HTMLReportComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (runSetActionEmailReport.HTMLReportTemplate == RunSetActionSendEmail.eHTMLReportTemplate.Custom)
            {
                CustomReportSection.Visibility = Visibility.Visible;
            }
            else if (runSetActionEmailReport.HTMLReportTemplate == RunSetActionSendEmail.eHTMLReportTemplate.FreeText)
            {
                BodyWebBrowser.Visibility = Visibility.Collapsed;
                BodyTextBox.Visibility = Visibility.Visible;

                BodyTextBox.Text = "";
                CustomReportSection.Visibility = Visibility.Hidden;
            }
            else
            {
                BodyWebBrowser.Visibility = System.Windows.Visibility.Visible;
                BodyTextBox.Visibility = System.Windows.Visibility.Collapsed;

                ReportInfo RI = new ReportInfo(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment, WorkSpace.Instance.RunsetExecutor);
                ((RunSetActionSendEmailOperations)runSetActionEmailReport.RunSetActionSendEmailOperations).SetBodyFromHTMLReport(RI);

                BodyWebBrowser.NavigateToString(runSetActionEmailReport.Email.Body);
                CustomReportSection.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void CustomHTMLReportComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {  
            
            BodyWebBrowser.Visibility = System.Windows.Visibility.Visible;
            ReportInfo RI = new ReportInfo(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment, WorkSpace.Instance.RunsetExecutor);
            string html = String.Empty;
            
            if (CustomHTMLReportComboBox.SelectedItem == null)
                return;

            ObservableList<HTMLReportTemplate> HTMLReportTemplates = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportTemplate>();
            foreach (HTMLReportTemplate htr in HTMLReportTemplates)
            {
               if(htr.Name==CustomHTMLReportComboBox.SelectedItem.ToString())
               {
                   html = htr.HTML;
               }
            }

            HTMLReportPage HTP = new HTMLReportPage(RI,html);
            
            BodyWebBrowser.NavigateToString(HTP.HTML);
        }

        private void rfsrh_Click(object sender, RoutedEventArgs e)
        {
            string current = null;
            if (CustomHTMLReportComboBox.SelectedItem != null)
            {
                current = CustomHTMLReportComboBox.SelectedItem.ToString();
            }
            CustomHTMLReportComboBox.Items.Clear();

            ObservableList<HTMLReportTemplate> HTMLReportTemplates = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportTemplate>();
            foreach (var htr in HTMLReportTemplates)
            {
                CustomHTMLReportComboBox.Items.Add(htr.Name);
            }

            if (current != null)
            {
                CustomHTMLReportComboBox.SelectedItem = current;
            }
        }

        private void EmailMethodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EmailMethodComboBox.SelectedItem.ToString() == "OUTLOOK")
            {
                SMTPConfig.Visibility = Visibility.Collapsed;
            }
            else
            {
                SMTPConfig.Visibility = Visibility.Visible;
            }
        }
    }
}
