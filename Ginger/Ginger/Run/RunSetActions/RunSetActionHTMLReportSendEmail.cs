#region License
/*
Copyright © 2014-2018 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License"); 
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

using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common;
using System;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Ginger.Reports;
using GingerCore;
using GingerCore.GeneralLib;
using System.IO.Compression;
using System.Net.Mime;
using System.Net.Mail;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.DataVisualization.Charting;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Security.Principal;
using Amdocs.Ginger;
using amdocs.ginger.GingerCoreNET;
using GingerCore.DataSource;

namespace Ginger.Run.RunSetActions
{
    public class RunSetActionHTMLReportSendEmail : RunSetActionBase
    {
        public enum eHTMLReportTemplate
        {
            HTMLReport,
            [EnumValueDescription("Free Text")]
            FreeText
        }

        public new static class Fields
        {
            public static string HTMLReportTemplate = "HTMLReportTemplate";
            public static string Bodytext = "Bodytext";
            public static string Comments = "Comments";
            public static string MailFrom = "MailFrom";
            public static string MailTo = "MailTo";
            public static string MailCC = "MailCC";
            public static string Subject = "Subject";
            public static string MailUser = "MailUser";
            public static string MailHost = "MailHost";
        }

        public override bool SupportRunOnConfig
        {
            get { return true; }
        }
        public override string Type { get { return "Send HTML Report Email"; } }

        public override List<RunSetActionBase.eRunAt> GetRunOptions()
        {
            List<RunSetActionBase.eRunAt> list = new List<RunSetActionBase.eRunAt>();
            list.Add(RunSetActionBase.eRunAt.ExecutionEnd);
            return list;
        }

        [IsSerializedForLocalRepository]
        public Email Email = new Email();

        ValueExpression mValueExpression = null;
        ValueExpression mVE
        {
            get {
                if (mValueExpression == null)
                {
                    mValueExpression = new ValueExpression(App.RunsetExecutor.RunsetExecutionEnvironment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>(), false, "", false, App.UserProfile.Solution.Variables);
                }
                return mValueExpression;
            }
        }

        //User can attach several templates to the email
        // attach template + RI
        // attach its own file
        [IsSerializedForLocalRepository]
        public ObservableList<EmailAttachment> EmailAttachments = new ObservableList<EmailAttachment>();

        private eHTMLReportTemplate mHTMLReportTemplate;
        [IsSerializedForLocalRepository]
        public eHTMLReportTemplate HTMLReportTemplate { get { return mHTMLReportTemplate; } set { if (mHTMLReportTemplate != value) { mHTMLReportTemplate = value; OnPropertyChanged(Fields.HTMLReportTemplate); } } }

        [IsSerializedForLocalRepository]
        public int selectedHTMLReportTemplateID { get; set; }

        private string mComments;
        [IsSerializedForLocalRepository]
        public string Comments { get { return mComments; } set { if (mComments != value) { mComments = value; OnPropertyChanged(Fields.Comments); } } }

        private string mBodytext;
        [IsSerializedForLocalRepository]
        public string Bodytext { get { return mBodytext; } set { if (mBodytext != value) { mBodytext = value; OnPropertyChanged(Fields.Bodytext); } } }

        //
        private string mMailFrom;
        [IsSerializedForLocalRepository]
        public string MailFrom { get { return mMailFrom; } set { if (mMailFrom != value) { mMailFrom = value; OnPropertyChanged(Fields.MailFrom); } } }
        
        private string mMailCC;
        [IsSerializedForLocalRepository]
        public string MailCC { get { return mMailCC; } set { if (mMailCC != value) { mMailCC = value; OnPropertyChanged(Fields.MailCC); } } }

        private string mSubject;
        [IsSerializedForLocalRepository]
        public string Subject { get { return mSubject; } set { if (mSubject != value) { mSubject = value; OnPropertyChanged(Fields.Subject); } } }
        
        private string mMailTo;
        [IsSerializedForLocalRepository]
        public string MailTo { get { return mMailTo; } set { if (mMailTo != value) { mMailTo = value; OnPropertyChanged(Fields.MailTo); } } }

        private string mMailHost;
        [IsSerializedForLocalRepository]
        public string MailHost { get { return mMailHost; } set { if (mMailHost != value) { mMailHost = value; OnPropertyChanged(Fields.MailHost); } } }

        private string mMailUser;
        [IsSerializedForLocalRepository]
        public string MailUser { get { return mMailUser; } set { if (mMailUser != value) { mMailUser = value; OnPropertyChanged(Fields.MailUser); } } }


        private string emailReadyHtml = string.Empty;

        public string tempFolder = string.Empty;

        public string TemplatesFolder = string.Empty;

        bool IsExecutionStatistic = false;

        public string reportsResultFolder = string.Empty;

        public string ReportPath = string.Empty;
        private string reportTimeStamp = string.Empty;
        public override void Execute(ReportInfo RI)
        {
            long s1 = new long();
            string extraInformationCalculated = string.Empty;
            string calculatedName = string.Empty;
            //Make sure we clear in case use open the edit page twice
            Email.Attachments.Clear();
            Email.alternateView = null;
            if (!System.IO.Directory.Exists(App.TempFolder))
                System.IO.Directory.CreateDirectory(App.TempFolder);
            tempFolder = App.TempFolder;
            TemplatesFolder = (Ginger.Reports.GingerExecutionReport.ExtensionMethods.getGingerEXEFileName() + @"Reports\GingerExecutionReport\").Replace("Ginger.exe", "");
            string runSetFolder = string.Empty;
            if (App.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder!= null)
            {
                 runSetFolder = App.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder;
                 AutoLogProxy.UserOperationStart("Online Report");
            }
            else
            {
                runSetFolder = ExecutionLogger.GetRunSetLastExecutionLogFolderOffline();
                AutoLogProxy.UserOperationStart("Offline Report");
            }
            
            var ReportItem = EmailAttachments.Where(x => x.AttachmentType == EmailAttachment.eAttachmentType.Report).FirstOrDefault();

            if (HTMLReportTemplate == RunSetActionHTMLReportSendEmail.eHTMLReportTemplate.FreeText)
            {
                if (ReportItem != null && !App.RunsetExecutor.RunSetConfig.RunsetExecLoggerPopulated)
                {
                    Errors = "In order to get HTML report, please, perform executions before";
                    Reporter.CloseGingerHelper();
                    Status = Ginger.Run.RunSetActions.RunSetActionBase.eRunSetActionStatus.Failed;
                    return;
                }
                mVE.Value = Bodytext;
                emailReadyHtml = @"<p><!--FULLREPORTLINK--><p>";
                emailReadyHtml += mVE.ValueCalculated;
            }
            else
            {
                if (App.RunsetExecutor.RunSetConfig.RunsetExecLoggerPopulated)
                {
                    if (selectedHTMLReportTemplateID > 0)
                    {
                        CreateSummaryViewReportForEmailAction(new ReportInfo(runSetFolder));
                    }
                }
                else
                {
                    Errors = "In order to get HTML report, please, perform executions before";
                    Reporter.CloseGingerHelper();
                    Status = Ginger.Run.RunSetActions.RunSetActionBase.eRunSetActionStatus.Failed;
                    return;
                }
            }

            if (EmailAttachments != null)
            {
                foreach (EmailAttachment r in EmailAttachments)
                {
                    //attach simple file
                    if (r.AttachmentType == EmailAttachment.eAttachmentType.File)
                    {
                        mVE.Value = r.Name;
                        calculatedName = mVE.ValueCalculated;
                        if (System.IO.File.Exists(calculatedName))
                        {
                            String TargetFileName = string.Empty;
                            if (r.ZipIt)
                            {
                                String SubFolder = Path.Combine(tempFolder, Path.GetFileNameWithoutExtension(calculatedName));
                                Directory.CreateDirectory(SubFolder);
                                TargetFileName = Path.Combine(SubFolder, Path.GetFileName(calculatedName));
                                if(File.Exists(TargetFileName))
                                {
                                    File.Delete(TargetFileName);
                                }
                                System.IO.File.Copy(calculatedName, TargetFileName);
                                TargetFileName = SubFolder;
                            }
                            else
                            {
                                TargetFileName = calculatedName;
                            }
                            AddAttachmentToEmail(Email, TargetFileName, r.ZipIt, EmailAttachment.eAttachmentType.File);
                        }
                        else
                        {
                            emailReadyHtml += "ERROR: File not found: " + calculatedName;
                        }
                    }
                    //attach report - after generating from template                    
                    if (r is EmailHtmlReportAttachment)
                    {
                        EmailHtmlReportAttachment rReport = ((EmailHtmlReportAttachment)r);
                        HTMLReportsConfiguration currentConf = App.UserProfile.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
                        mVE.Value = rReport.ExtraInformation;
                        extraInformationCalculated = mVE.ValueCalculated;
                        if (!string.IsNullOrEmpty(rReport.SelectedHTMLReportTemplateID.ToString()))
                        {
                            if ((rReport.IsAlternameFolderUsed) && (extraInformationCalculated != null) && (extraInformationCalculated != string.Empty))
                            {
                                // check if user have write permission on attachment folder. If not - do not put attachment, and add warning to email body
                                if (!HTMLReportAttachmentConfigurationPage.HasWritePermission(extraInformationCalculated))
                                {
                                    emailReadyHtml = emailReadyHtml.Replace("<!--WARNING-->",
                                    "<b>Full report attachment failed, </b>" +
                                    "Error: User '" + WindowsIdentity.GetCurrent().Name.ToString() + "' have no write permission on provided alternative folder - " + extraInformationCalculated + ". Attachment in it not saved.");
                                }
                                else
                                {
                                    emailReadyHtml = emailReadyHtml.Replace("<!--WARNING-->", "");
                                    ObservableList<HTMLReportConfiguration> HTMLReportConfigurations = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>();
                                    reportsResultFolder = Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(runSetFolder),
                                                                                                                                            false,
                                                                                                                                            HTMLReportConfigurations.Where(x => (x.ID == rReport.SelectedHTMLReportTemplateID)).FirstOrDefault(),
                                                                                                                                            extraInformationCalculated + "\\" + System.IO.Path.GetFileName(runSetFolder),false, currentConf.HTMLReportConfigurationMaximalFolderSize);
                                }
                            }
                            else
                            {
                                ObservableList<HTMLReportConfiguration> HTMLReportConfigurations = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>();
                                reportsResultFolder = Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(runSetFolder),
                                                                                                                                          false,
                                                                                                                                          HTMLReportConfigurations.Where(x => (x.ID == rReport.SelectedHTMLReportTemplateID)).FirstOrDefault());
                            }
                        }
                        if (!string.IsNullOrEmpty(reportsResultFolder))
                        {
                            AddAttachmentToEmail(Email, reportsResultFolder, r.ZipIt, EmailAttachment.eAttachmentType.Report);
                        }
                    }
                }
                s1 = CalculateFileSize(Email);

                if (ReportItem != null)
                {
                    if (((EmailHtmlReportAttachment)ReportItem).IsLinkEnabled || s1 > 10000000)
                    {
                        if (EmailAttachments.IndexOf(ReportItem) > -1)
                        {
                            if (Email.Attachments.Count > 0)
                            {
                                Email.Attachments.RemoveAt(EmailAttachments.IndexOf(ReportItem));
                            }
                        }
                        if (!string.IsNullOrEmpty(reportsResultFolder))
                        {
                            emailReadyHtml = emailReadyHtml.Replace("<!--FULLREPORTLINK-->", "<a href ='" + reportsResultFolder + "\\GingerExecutionReport.html'" + " style ='font-size:16px;color:blue;text-decoration:underline'> Click Here to View Full Report </a>");
                            emailReadyHtml = emailReadyHtml.Replace("<!--WARNING-->", "");
                        }
                    }
                    else
                    {
                        if ((!((EmailHtmlReportAttachment)ReportItem).IsAlternameFolderUsed) && (s1 > 10000000))
                        {
                            emailReadyHtml = emailReadyHtml.Replace("<!--FULLREPORTLINK-->", string.Empty);
                            emailReadyHtml = emailReadyHtml.Replace("<!--WARNING-->",
                                        "<b>Full report attachment failed, </b>" +
                                        "Error: Attachment size is bigger than 10 Mb and alternative folder was not provided. Attachment is not saved.");
                        }
                        else
                        {
                            emailReadyHtml = emailReadyHtml.Replace("<!--FULLREPORTLINK-->", string.Empty);
                            emailReadyHtml = emailReadyHtml.Replace("<!--WARNING-->", string.Empty);
                        }
                    }
                }
            }
            else
            {
                emailReadyHtml = emailReadyHtml.Replace("<!--FULLREPORTLINK-->", string.Empty);
                emailReadyHtml = emailReadyHtml.Replace("<!--WARNING-->", "");
            }
            if (HTMLReportTemplate == RunSetActionHTMLReportSendEmail.eHTMLReportTemplate.HTMLReport)
            {
                Email.EmbededAttachment.Clear();
                emailReadyHtml = emailReadyHtml.Replace("cid:gingerRunner", "cid:gingerRunner" + reportTimeStamp);
                emailReadyHtml = emailReadyHtml.Replace("cid:Businessflow", "cid:Businessflow" + reportTimeStamp);
                emailReadyHtml = emailReadyHtml.Replace("cid:Activity", "cid:Activity" + reportTimeStamp);
                emailReadyHtml = emailReadyHtml.Replace("cid:Action", "cid:Action" + reportTimeStamp);

                if (Email.EmailMethod.ToString() == "SMTP")
                {
                    AlternateView alternativeView = AlternateView.CreateAlternateViewFromString(emailReadyHtml, null, MediaTypeNames.Text.Html);
                    alternativeView.ContentId = "htmlView";
                    alternativeView.TransferEncoding = TransferEncoding.SevenBit;
                    alternativeView.LinkedResources.Add(GetLinkedResource(GetImageStream(TemplatesFolder + @"\assets\\img\@BeatLogo.png"), "beat"));
                    alternativeView.LinkedResources.Add(GetLinkedResource(GetImageStream(TemplatesFolder + @"\assets\\img\@Ginger.png"), "ginger"));
                    alternativeView.LinkedResources.Add(GetLinkedResource(GetImageStream(tempFolder + @"\CustomerLogo.png"), "customer"));
                    if (!string.IsNullOrEmpty(Comments))
                    {
                        alternativeView.LinkedResources.Add(GetLinkedResource(GetImageStream(TemplatesFolder + @"\assets\\img\comments-icon.jpg"), "comment"));
                    }
                    if (IsExecutionStatistic)
                    {
                        alternativeView.LinkedResources.Add(GetLinkedResource(GetImageStream(tempFolder + "\\GingerRunner" + reportTimeStamp + ".jpeg"), "gingerRunner" + reportTimeStamp));
                        alternativeView.LinkedResources.Add(GetLinkedResource(GetImageStream(tempFolder + "\\Action" + reportTimeStamp + ".jpeg"), "Action" + reportTimeStamp));
                        alternativeView.LinkedResources.Add(GetLinkedResource(GetImageStream(tempFolder + "\\Activity" + reportTimeStamp + ".jpeg"), "Activity" + reportTimeStamp));
                        alternativeView.LinkedResources.Add(GetLinkedResource(GetImageStream(tempFolder + "\\Businessflow" + reportTimeStamp + ".jpeg"), "Businessflow" + reportTimeStamp));
                    }
                    Email.alternateView = alternativeView;
                }
                else
                {
                    Email.EmbededAttachment.Add(new KeyValuePair<string, string>(TemplatesFolder + @"\assets\\img\@BeatLogo.png", "beat"));
                    Email.EmbededAttachment.Add(new KeyValuePair<string, string>(TemplatesFolder + @"\assets\\img\@Ginger.png", "ginger"));
                    Email.EmbededAttachment.Add(new KeyValuePair<string, string>(tempFolder + @"\CustomerLogo.png", "customer"));
                    if (!string.IsNullOrEmpty(Comments))
                    {
                        Email.EmbededAttachment.Add(new KeyValuePair<string, string>(TemplatesFolder + @"\assets\\img\comments-icon.jpg", "comment"));
                    }
                    if (IsExecutionStatistic)
                    {
                        Email.EmbededAttachment.Add(new KeyValuePair<string, string>(tempFolder + "\\GingerRunner" + reportTimeStamp + ".jpeg", "gingerRunner" + reportTimeStamp));
                        Email.EmbededAttachment.Add(new KeyValuePair<string, string>(tempFolder + "\\Action" + reportTimeStamp + ".jpeg", "Action" + reportTimeStamp));
                        Email.EmbededAttachment.Add(new KeyValuePair<string, string>(tempFolder + "\\Activity" + reportTimeStamp + ".jpeg", "Activity" + reportTimeStamp));
                        Email.EmbededAttachment.Add(new KeyValuePair<string, string>(tempFolder + "\\Businessflow" + reportTimeStamp + ".jpeg", "Businessflow" + reportTimeStamp));
                    }
                }
            }

            mVE.Value = MailFrom;
            Email.MailFrom = mVE.ValueCalculated;
            mVE.Value = MailTo;
            Email.MailTo = mVE.ValueCalculated;
            mVE.Value = MailCC;
            Email.MailCC = mVE.ValueCalculated;
            mVE.Value = Subject;
            Email.Subject = mVE.ValueCalculated;
            mVE.Value = MailHost;
            Email.SMTPMailHost = mVE.ValueCalculated;
            mVE.Value = MailUser;
            Email.SMTPUser = mVE.ValueCalculated;
            Email.Body = emailReadyHtml;
            emailReadyHtml = string.Empty;              
            bool isSuccess;
            isSuccess = Email.Send();
            if (isSuccess == false)
            {
                Errors = Email.Event;
                Reporter.CloseGingerHelper();
                Status = Ginger.Run.RunSetActions.RunSetActionBase.eRunSetActionStatus.Failed;
            }
        }
        public void CreateSummaryViewReportForEmailAction(ReportInfo RI)
        {
            reportTimeStamp = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss_fff");
            HTMLReportConfiguration currentTemplate = new HTMLReportConfiguration();
            ObservableList<HTMLReportConfiguration> HTMLReportConfigurations = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>();
            currentTemplate = HTMLReportConfigurations.Where(x => (x.ID == selectedHTMLReportTemplateID)).FirstOrDefault();
            if (currentTemplate == null)
            {
                currentTemplate = HTMLReportConfigurations.Where(x => (x.IsDefault == true)).FirstOrDefault();
            }
            System.Drawing.Image CustomerLogo = Ginger.General.Base64StringToImage(currentTemplate.LogoBase64Image.ToString());
            CustomerLogo.Save(tempFolder + "/CustomerLogo.png");
            Ginger.Reports.HTMLReportTemplatePage.EnchancingLoadedFieldsWithDataAndValidating(currentTemplate);
            if ((RI.ReportInfoRootObject == null) || (RI.ReportInfoRootObject.GetType() == typeof(Object)))
            {
                return;
            }
            string ReportHTML = Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetHTMLTemplate("EmailExecutionReport.html", TemplatesFolder);
            List<KeyValuePair<int, int>> chartData = null;
            StringBuilder fieldsNamesHTMLTableCells = new StringBuilder();
            StringBuilder fieldsValuesHTMLTableCells = new StringBuilder();
            StringBuilder fieldsNamesHTMLTableCellsm = new StringBuilder();
            StringBuilder fieldsValuesHTMLTableCellsm = new StringBuilder();
            foreach (HTMLReportConfigFieldToSelect selectedField in currentTemplate.EmailSummaryViewFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
            {
                if (currentTemplate.EmailSummaryViewFieldsToSelect.IndexOf(selectedField) <= 5)
                {
                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField.FieldName + "</td>");

                    if (((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((RunSetReport)RI.ReportInfoRootObject)) == null)
                    {
                        if (selectedField.FieldKey == RunSetReport.Fields.EnvironmentsDetails)
                        {
                            StringBuilder environmentNames_str = new StringBuilder();
                            ((RunSetReport)RI.ReportInfoRootObject).GingerReports.Where(x => x.EnvironmentName != null && x.EnvironmentName != string.Empty).GroupBy(q => q.EnvironmentName).Select(q => q.First()).ToList().ForEach(x => environmentNames_str.Append(x.EnvironmentName + ", "));
                            fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + environmentNames_str.ToString().TrimEnd(',', ' ') + "</td>");
                            environmentNames_str.Remove(0, environmentNames_str.Length);
                        }
                        else
                        {
                            fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'> N/A </td>");
                        }
                    }
                    else
                    {

                        if (selectedField.FieldKey == RunSetReport.Fields.ExecutionDuration)
                        {
                            fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(GingerCore.General.TimeConvert(((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((RunSetReport)RI.ReportInfoRootObject)).ToString())) + "</td>");
                        }
                        else if ((selectedField.FieldKey == ActionReport.Fields.StartTimeStamp) || (selectedField.FieldKey == ActionReport.Fields.EndTimeStamp))
                        {
                            fieldsValuesHTMLTableCells.Append("<td>" + DateTime.Parse(((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((RunSetReport)RI.ReportInfoRootObject)).ToString()).ToLocalTime().ToString() + "</td>");
                        }
                        else
                        {
                            fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((RunSetReport)RI.ReportInfoRootObject)).ToString()) + "</td>");
                        }
                    }
                }
                else
                {
                    fieldsNamesHTMLTableCellsm.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField.FieldName + "</td>");
                    if (((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((RunSetReport)RI.ReportInfoRootObject)) == null)
                    {
                        fieldsValuesHTMLTableCellsm.Append("<td style='padding: 10px; border: 1px solid #dddddd'> N/A </td>");
                    }
                    else
                    {
                        fieldsValuesHTMLTableCellsm.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((RunSetReport)RI.ReportInfoRootObject)).ToString()) + "</td>");
                    }
                }
            }

            ReportHTML = ReportHTML.Replace("{GeneralDetails_Headers}", fieldsNamesHTMLTableCellsm.ToString());
            ReportHTML = ReportHTML.Replace("{GeneralDetails_Data}", fieldsValuesHTMLTableCellsm.ToString());
            ReportHTML = ReportHTML.Replace("{ExecutionGeneralDetails_Headers}", fieldsNamesHTMLTableCells.ToString());
            ReportHTML = ReportHTML.Replace("{ExecutionGeneralDetails_Data}", fieldsValuesHTMLTableCells.ToString());
            ReportHTML = ReportHTML.Replace("{ReportCreated}", DateTime.Now.ToString());
            ReportHTML = ReportHTML.Replace("{ReportLevel}", "");
            fieldsNamesHTMLTableCells.Remove(0, fieldsNamesHTMLTableCells.Length);
            fieldsValuesHTMLTableCells.Remove(0, fieldsValuesHTMLTableCells.Length);
            foreach (HTMLReportConfigFieldToSelect selectedField in currentTemplate.EmailSummaryViewFieldsToSelect.Where(x => (x.FieldType == Ginger.Reports.FieldsType.Section.ToString())))
            {
                if (selectedField.FieldKey == RunSetReport.Fields.ExecutionStatisticsDetails)
                {
                    if (!selectedField.IsSelected)
                    {
                        string executionStatisticsSection = Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetStringBetween(ReportHTML, "<!--ExecutionStatisticsDetails_Start-->", "<!--ExecutionStatisticsDetails_End-->");
                        if(!string.IsNullOrEmpty(executionStatisticsSection))
                            ReportHTML = ReportHTML.Replace(executionStatisticsSection, "");
                        IsExecutionStatistic = false;
                    }
                    else
                    {
                        IsExecutionStatistic = true;
                        //Ginger Runners Place Holders
                        chartData = new List<KeyValuePair<int, int>>();
                        chartData.Add(new KeyValuePair<int, int>(((RunSetReport)RI.ReportInfoRootObject).TotalGingerRunnersPassed, 0));
                        chartData.Add(new KeyValuePair<int, int>(((RunSetReport)RI.ReportInfoRootObject).TotalGingerRunnersFailed, 1));
                        chartData.Add(new KeyValuePair<int, int>(((RunSetReport)RI.ReportInfoRootObject).TotalGingerRunnersStopped, 2));
                        chartData.Add(new KeyValuePair<int, int>(((RunSetReport)RI.ReportInfoRootObject).TotalGingerRunnersOther, 3));
                        CreateChart(chartData, "GingerRunner" + reportTimeStamp + ".jpeg", "Ginger Runners");

                        // Business Flows Place Holders                        
                        chartData = new List<KeyValuePair<int, int>>();
                        chartData.Add(new KeyValuePair<int, int>(((RunSetReport)RI.ReportInfoRootObject).GingerReports.Select(x => x.TotalBusinessFlowsPassed).ToList().Sum(), 0));
                        chartData.Add(new KeyValuePair<int, int>(((RunSetReport)RI.ReportInfoRootObject).GingerReports.Select(x => x.TotalBusinessFlowsFailed).ToList().Sum(), 1));
                        chartData.Add(new KeyValuePair<int, int>(((RunSetReport)RI.ReportInfoRootObject).GingerReports.Select(x => x.TotalBusinessFlowsStopped).ToList().Sum(), 2));
                        chartData.Add(new KeyValuePair<int, int>(((RunSetReport)RI.ReportInfoRootObject).GingerReports.Select(x => x.TotalBusinessFlowsOther).ToList().Sum(), 3));
                        CreateChart(chartData, "Businessflow" + reportTimeStamp + ".jpeg", "Business Flows");

                        List<BusinessFlowReport> bfTotalList = new List<BusinessFlowReport>();
                        ((RunSetReport)RI.ReportInfoRootObject).GingerReports.ForEach(x => x.BusinessFlowReports.ForEach(y => bfTotalList.Add(y)));
                        // Activities Place Holders                        
                        chartData = new List<KeyValuePair<int, int>>();
                        chartData.Add(new KeyValuePair<int, int>(bfTotalList.Select(x => x.TotalActivitiesPassed).ToList().Sum(), 0));
                        chartData.Add(new KeyValuePair<int, int>(bfTotalList.Select(x => x.TotalActivitiesFailed).ToList().Sum(), 1));
                        chartData.Add(new KeyValuePair<int, int>(bfTotalList.Select(x => x.TotalActivitiesStopped).ToList().Sum(), 2));
                        chartData.Add(new KeyValuePair<int, int>(bfTotalList.Select(x => x.TotalActivitiesOther).ToList().Sum(), 3));
                        CreateChart(chartData, "Activity" + reportTimeStamp + ".jpeg", "Activities");

                        List<ActivityReport> activitiesTotalList = new List<ActivityReport>();
                        bfTotalList.ForEach(x => x.Activities.ForEach(y => activitiesTotalList.Add(y)));
                        // Actions Place Holders                        
                        chartData = new List<KeyValuePair<int, int>>();
                        chartData.Add(new KeyValuePair<int, int>(activitiesTotalList.Select(x => x.TotalActionsPassed).ToList().Sum(), 0));
                        chartData.Add(new KeyValuePair<int, int>(activitiesTotalList.Select(x => x.TotalActionsFailed).ToList().Sum(), 1));
                        chartData.Add(new KeyValuePair<int, int>(activitiesTotalList.Select(x => x.TotalActionsStopped).ToList().Sum(), 2));
                        chartData.Add(new KeyValuePair<int, int>(activitiesTotalList.Select(x => x.TotalActionsOther).ToList().Sum(), 3));
                        CreateChart(chartData, "Action" + reportTimeStamp + ".jpeg", "Actions");
                    }
                }
                else if (selectedField.FieldKey == RunSetReport.Fields.ExecutedBusinessFlowsDetails)
                {
                    if (!selectedField.IsSelected)
                    {
                        string executionStatisticsSection = Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetStringBetween(ReportHTML, "<!--ExecutionBusinessFlowsDetails_Start-->", "<!--ExecutionBusinessFlowsDetails_End-->");
                        if(!string.IsNullOrEmpty(executionStatisticsSection))
                            ReportHTML = ReportHTML.Replace(executionStatisticsSection, "");
                    }
                    else
                    {
                        fieldsNamesHTMLTableCells = new StringBuilder();
                        fieldsValuesHTMLTableCells = new StringBuilder();
                        List<int> listOfHandledGingerRunnersReport = new List<int>();
                        bool firstIteration = true;
                        foreach (GingerReport GR in ((RunSetReport)RI.ReportInfoRootObject).GingerReports.OrderBy(x => x.Seq))
                        {
                            GR.AllIterationElements = currentTemplate.ShowAllIterationsElements;
                            foreach (BusinessFlowReport br in GR.BusinessFlowReports)
                            {
                                br.AllIterationElements = currentTemplate.ShowAllIterationsElements;

                                fieldsValuesHTMLTableCells.Append("<tr>");
                                foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.GingerRunnerFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                                {
                                    if ((selectedField_internal.FieldKey == GingerReport.Fields.Name) && (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.SummaryViewLevel.ToString()))
                                    {
                                        if (firstIteration)
                                        {
                                            fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                        }
                                        fieldsValuesHTMLTableCells.Append(@"<td style='padding: 10px; border: 1px solid #dddddd'>" + GR.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(GR) + @"</td>");
                                    }
                                    else if (selectedField_internal.FieldKey == GingerReport.Fields.EnvironmentName)
                                    {
                                        if (firstIteration)
                                        {
                                            fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                        }
                                        fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(GR.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(GR) != null ? GR.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(GR).ToString() : string.Empty) + "</td>");
                                    }
                                    else if (selectedField_internal.FieldKey == GingerReport.Fields.Seq)
                                    {
                                        if (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.SummaryViewLevel.ToString())
                                        {
                                            int currentSeq = Convert.ToInt32(GR.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(GR).ToString());
                                            if (!listOfHandledGingerRunnersReport.Contains(currentSeq))
                                            {
                                                listOfHandledGingerRunnersReport.Add(currentSeq);
                                            }
                                        }
                                    }
                                }

                                foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.BusinessFlowFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                                {
                                    if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.Seq)
                                    {
                                        if (firstIteration)
                                        {
                                            fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                        }
                                        fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br).ToString()) + "</td>");
                                    }
                                    if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.Name)
                                    {
                                        if (firstIteration)
                                        {
                                            fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                        }
                                        if (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.GingerRunnerLevel.ToString())
                                        {
                                            fieldsValuesHTMLTableCells.Append(@"<td style='padding: 10px; border: 1px solid #dddddd'>" + br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) + @"</td>");
                                        }
                                        else
                                        {
                                            fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br).ToString()) + "</td>");
                                        }
                                    }
                                    else if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.Description)
                                    {
                                        if (firstIteration)
                                        {
                                            fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                        }
                                        try
                                        {
                                            fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) != null ? br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br).ToString() : string.Empty) + "</td>");
                                        }
                                        catch
                                        {
                                            fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'></td>");
                                        }
                                    }
                                    else if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.ExecutionDescription)
                                    {
                                        if (firstIteration)
                                        {
                                            fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                        }
                                        try
                                        {
                                            fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) != null ? br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br).ToString() : string.Empty) + "</td>");
                                        }
                                        catch
                                        {
                                            fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'></td>");
                                        }
                                    }
                                    else if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.ExecutionDuration)
                                    {
                                        if (firstIteration)
                                        {
                                            fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                        }
                                        fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) != null ? GingerCore.General.TimeConvert(br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br).ToString()) : string.Empty) + "</td>");
                                    }
                                    else if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.RunStatus)
                                    {
                                        if (firstIteration)
                                        {
                                            fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                        }
                                        fieldsValuesHTMLTableCells.Append("<td style='padding:10px;border:1px solid #dddddd;' class='Status" + (br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br)).ToString() + "'>" + br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) + "</td>");
                                    }
                                    else if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.PassPercent)
                                    {
                                        if (firstIteration)
                                        {
                                            fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                        }
                                        fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) + " %</td>");
                                    }
                                }
                                fieldsValuesHTMLTableCells.Append("</tr>");
                                firstIteration = false;
                            }
                        }
                    }

                    ReportHTML = ReportHTML.Replace("{BusinessFlowsDetails_Headers}", fieldsNamesHTMLTableCells.ToString());
                    ReportHTML = ReportHTML.Replace("{BusinessFlowsDetails_Data}", fieldsValuesHTMLTableCells.ToString());
                }
                else if (selectedField.FieldKey == RunSetReport.Fields.ExecutedActivitiesDetails)
                {
                    if (!selectedField.IsSelected)
                    {
                        string executionStatisticsSection = Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetStringBetween(ReportHTML, "<!--ExecutionActivitiesDetails_Start-->", "<!--ExecutionActivitiesDetails_End-->");
                        if (!string.IsNullOrEmpty(executionStatisticsSection))
                            ReportHTML = ReportHTML.Replace(executionStatisticsSection, "");
                    }
                    else
                    {
                        fieldsNamesHTMLTableCells = new StringBuilder();
                        fieldsValuesHTMLTableCells = new StringBuilder();
                        ExecutedActivitiesDetailsGenrator(RI, currentTemplate, ref ReportHTML, fieldsNamesHTMLTableCells, fieldsValuesHTMLTableCells);
                    }

                    ReportHTML = ReportHTML.Replace("{ActivitiesDetails_Headers}", fieldsNamesHTMLTableCells.ToString());
                    ReportHTML = ReportHTML.Replace("{ActivitiesDetails_Data}", fieldsValuesHTMLTableCells.ToString());
                }
                else if (selectedField.FieldKey == RunSetReport.Fields.FailuresDetails)
                {
                    bool isFailuresDetailsExists = false;

                    if(selectedField.IsSelected)
                    {
                        fieldsNamesHTMLTableCells = new StringBuilder();
                        fieldsValuesHTMLTableCells = new StringBuilder();
                        List<int> listOfHandledGingerRunnersReport = new List<int>();
                        bool firstIteration = true;
                        foreach (GingerReport GR in ((RunSetReport)RI.ReportInfoRootObject).GingerReports.Where(x => x.GingerExecutionStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed).OrderBy(x => x.Seq))
                        {
                            foreach (BusinessFlowReport br in GR.BusinessFlowReports.Where(x => x.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed.ToString()))
                            {
                                foreach (ActivityReport ac in br.Activities.Where(x => x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed.ToString()).OrderBy(x => x.Seq))
                                {
                                    foreach (ActionReport act in ac.ActionReports.Where(x => x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed.ToString()).OrderBy(x => x.Seq))
                                    {
                                        isFailuresDetailsExists = true;

                                        // Ginger Runner Level
                                        fieldsValuesHTMLTableCells.Append("<tr>");
                                        foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.GingerRunnerFieldsToSelect.Where(x => (x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                                        {
                                            if (selectedField_internal.FieldKey == GingerReport.Fields.Name)
                                            {
                                                if (firstIteration)
                                                {
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                                }
                                                fieldsValuesHTMLTableCells.Append(@"<td style='padding: 10px; border: 1px solid #dddddd'>" + GR.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(GR) + @"</td>");
                                            }
                                        }

                                        // Business Flow Level
                                        foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.BusinessFlowFieldsToSelect.Where(x => (x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                                        {
                                            if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.Seq)
                                            {
                                                if (firstIteration)
                                                {
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>Business Flow Sequence</td>");
                                                }
                                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) + "</td>");
                                            }
                                            if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.Name)
                                            {
                                                if (firstIteration)
                                                {
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                                }
                                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) + "</td>");
                                            }
                                        }

                                        // Activity Level 
                                        foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.ActivityFieldsToSelect.Where(x => (x.FieldName != "ScreenShot" && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                                        {
                                            if (selectedField_internal.FieldKey == ActivityReport.Fields.Seq)
                                            {
                                                if (firstIteration)
                                                {
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>Activity Sequence</td>");
                                                }
                                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + OverrideHTMLRelatedCharacters(ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString()) + "</td>");
                                            }
                                            if (selectedField_internal.FieldKey == ActivityReport.Fields.ActivityName)
                                            {
                                                if (firstIteration)
                                                {
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                                }
                                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + OverrideHTMLRelatedCharacters(ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString()) + "</td>");
                                            }
                                        }

                                        // Action Level
                                        foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.ActionFieldsToSelect.Where(x => (x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                                        {
                                            if (selectedField_internal.FieldKey == ActionReport.Fields.Seq)
                                            {
                                                if (firstIteration)
                                                {
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>Action Execution Sequence</td>");
                                                }
                                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + OverrideHTMLRelatedCharacters(act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString()) + "</td>");
                                            }
                                            if (selectedField_internal.FieldKey == ActionReport.Fields.Description)
                                            {
                                                if (firstIteration)
                                                {
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>Action Description</td>");
                                                }
                                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + OverrideHTMLRelatedCharacters(act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString()) + "</td>");
                                            }
                                            if (selectedField_internal.FieldKey == ActionReport.Fields.Error)
                                            {
                                                if (firstIteration)
                                                {
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                                }
                                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd; color:red;white-space:pre-wrap;white-space:-moz-pre-wrap;white-space:-pre-wrap;white-space:-o-pre-wrap;word-break: break-all;'>" + OverrideHTMLRelatedCharacters(act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString()) + "</td>");
                                            }
                                            if ((selectedField_internal.FieldKey == ActionReport.Fields.ElapsedSecs) ||
                                                (selectedField_internal.FieldKey == ActionReport.Fields.CurrentRetryIteration) ||
                                                (selectedField_internal.FieldKey == ActionReport.Fields.ExInfo))
                                            {
                                                if (firstIteration)
                                                {
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                                }
                                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + OverrideHTMLRelatedCharacters(act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString()) + "</td>");
                                            }
                                        }
                                        fieldsValuesHTMLTableCells.Append("</tr>");
                                        firstIteration = false;
                                    }
                                }
                            }
                        }
                    }
                    if (isFailuresDetailsExists)
                    {
                        ReportHTML = ReportHTML.Replace("{FailuresDetails_Headers}", fieldsNamesHTMLTableCells.ToString());
                        ReportHTML = ReportHTML.Replace("{FailuresDetails_Data}", fieldsValuesHTMLTableCells.ToString());
                    }
                    else
                    {
                        string failureDetailsSection = Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetStringBetween(ReportHTML, "<!--FailuresDetails_Start-->", "<!--FailuresDetails_End-->");
                        if (!string.IsNullOrEmpty(failureDetailsSection))
                            ReportHTML = ReportHTML.Replace(failureDetailsSection, "");
                    }
                }
            }
            if (!string.IsNullOrEmpty(Comments))
            {
                mVE.Value = Comments;
                ReportHTML = ReportHTML.Replace("{COMMENT}", "<img src='cid:comment'/>" + mVE.ValueCalculated);
            }
            else
            {
                ReportHTML = ReportHTML.Replace("{COMMENT}", "");
            }
            fieldsNamesHTMLTableCells.Remove(0, fieldsNamesHTMLTableCells.Length);
            fieldsValuesHTMLTableCells.Remove(0, fieldsValuesHTMLTableCells.Length);
            emailReadyHtml = ReportHTML;
            RI = null;
            ReportHTML = null;
        }

        private void ExecutedActivitiesDetailsGenrator(ReportInfo RI, HTMLReportConfiguration currentTemplate, ref string reportHTML , StringBuilder fieldsNamesHTMLTableCells, StringBuilder fieldsValuesHTMLTableCells)
        {
            List<int> listOfHandledGingerRunnersReport = new List<int>();
            bool firstActivityIteration = true;
            foreach (GingerReport GR in ((RunSetReport)RI.ReportInfoRootObject).GingerReports.OrderBy(x => x.Seq))
            {
                GR.AllIterationElements = currentTemplate.ShowAllIterationsElements;
                string flowReportName = string.Empty;
                string executionSeq = string.Empty;
                string businessReportName = string.Empty;
                foreach (BusinessFlowReport br in GR.BusinessFlowReports)
                {
                    br.AllIterationElements = currentTemplate.ShowAllIterationsElements;
                    bool newBusinessFlow = true;
                    fieldsValuesHTMLTableCells.Append("<tr>");
                    foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.GingerRunnerFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                    {
                        if ((selectedField_internal.FieldKey == GingerReport.Fields.Name) && (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.SummaryViewLevel.ToString()))
                        {
                            if (firstActivityIteration)
                            {
                                fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                            }
                            flowReportName = GR.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(GR).ToString();
                            fieldsValuesHTMLTableCells.Append(@"<td style='padding: 10px; border: 1px solid #dddddd'>" + flowReportName + @"</td>");
                        }
                        else if (selectedField_internal.FieldKey == GingerReport.Fields.Seq)
                        {
                            if (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.SummaryViewLevel.ToString())
                            {
                                int currentSeq = Convert.ToInt32(GR.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(GR).ToString());
                                if (!listOfHandledGingerRunnersReport.Contains(currentSeq))
                                {
                                    listOfHandledGingerRunnersReport.Add(currentSeq);
                                }
                            }
                        }
                    }

                    foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.BusinessFlowFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                    {
                        if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.Seq)
                        {
                            if (firstActivityIteration)
                            {
                                fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                            }
                            executionSeq = Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br).ToString());
                            fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + executionSeq + "</td>");
                        }
                        if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.Name)
                        {
                            if (firstActivityIteration)
                            {
                                fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                            }
                            if (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.GingerRunnerLevel.ToString())
                            {
                                businessReportName = br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br).ToString();
                                fieldsValuesHTMLTableCells.Append(@"<td style='padding: 10px; border: 1px solid #dddddd'>" + businessReportName + @"</td>");
                            }
                            else
                            {
                                businessReportName = Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br).ToString());
                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + businessReportName + "</td>");
                            }
                        }
                    }
                    // Activities details table 
                    foreach (ActivityReport activityReport in br.Activities)
                    {
                        if (!newBusinessFlow)
                        {
                            fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + flowReportName + "</td>");
                            fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + executionSeq + "</td>");
                            fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + businessReportName + "</td>");
                        }
                        foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.ActivityFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                        {

                            if (selectedField_internal.FieldKey == ActivityReport.Fields.Seq)
                            {
                                if (firstActivityIteration)
                                {
                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + "Activity " + selectedField_internal.FieldName + "</td>");
                                }
                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(activityReport.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(activityReport).ToString()) + "</td>");
                            }
                            if (selectedField_internal.FieldKey == ActivityReport.Fields.ActivityGroupName)
                            {
                                if (firstActivityIteration)
                                {
                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                }
                                string activityGroupNameValue = (activityReport.ActivityGroupName == null) ? "N/A" : Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(activityReport.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(activityReport).ToString());
                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + activityGroupNameValue + "</td>");
                            }
                            if (selectedField_internal.FieldKey == ActivityReport.Fields.ActivityName)
                            {
                                if (firstActivityIteration)
                                {
                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                }
                                if (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.GingerRunnerLevel.ToString())
                                {
                                    fieldsValuesHTMLTableCells.Append(@"<td style='padding: 10px; border: 1px solid #dddddd'>" + activityReport.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(activityReport) + @"</td>");
                                }
                                else
                                {
                                    fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(activityReport.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(activityReport).ToString()) + "</td>");
                                }
                            }
                            if (selectedField_internal.FieldKey == ActivityReport.Fields.Description)
                            {
                                if (firstActivityIteration)
                                {
                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                }
                                string activityDescriptionValue = (activityReport.Description == null) ? "N/A" : Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(activityReport.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(activityReport).ToString());
                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + activityDescriptionValue + "</td>");
                            }
                            if (selectedField_internal.FieldKey == ActivityReport.Fields.RunDescription)
                            {
                                if (firstActivityIteration)
                                {
                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                }
                                string activityRunDescriptionValue = (activityReport.RunDescription == null) ? "N/A" : Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(activityReport.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(activityReport).ToString());
                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + activityRunDescriptionValue + "</td>");
                            }
                            if (selectedField_internal.FieldKey == ActivityReport.Fields.StartTimeStamp)
                            {
                                if (firstActivityIteration)
                                {
                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                }
                                string activityStartTimeStampValue = Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(activityReport.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(activityReport).ToString());
                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + activityStartTimeStampValue + "</td>");
                            }
                            if (selectedField_internal.FieldKey == ActivityReport.Fields.EndTimeStamp)
                            {
                                if (firstActivityIteration)
                                {
                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                }
                                string activityEndTimeStampValue = Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(activityReport.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(activityReport).ToString());
                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + activityEndTimeStampValue + "</td>");
                            }
                            if (selectedField_internal.FieldKey == ActivityReport.Fields.ElapsedSecs)
                            {
                                if (firstActivityIteration)
                                {
                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                }
                                string activityElapsedSecsValue = Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(activityReport.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(activityReport).ToString());
                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + activityElapsedSecsValue + "</td>");
                            }
                            if (selectedField_internal.FieldKey == ActivityReport.Fields.RunStatus)
                            {
                                if (firstActivityIteration)
                                {
                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                }
                                string activityRunStatusValue = Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(activityReport.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(activityReport).ToString());
                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + activityRunStatusValue + "</td>");
                            }
                            if (selectedField_internal.FieldKey == ActivityReport.Fields.NumberOfActions)
                            {
                                if (firstActivityIteration)
                                {
                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                }
                                string activityNumberOfActionsValue = Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(activityReport.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(activityReport).ToString());
                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + activityNumberOfActionsValue + "</td>");
                            }
                            if (selectedField_internal.FieldKey == ActivityReport.Fields.ActionsDetails)
                            {
                                if (firstActivityIteration)
                                {
                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                }
                                string activityActionsDetailsValue = Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(activityReport.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(activityReport).ToString());
                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + activityActionsDetailsValue + "</td>");
                            }

                        }
                        fieldsValuesHTMLTableCells.Append("</tr>");
                        newBusinessFlow = false;
                        firstActivityIteration = false;
                    }
                }
            }
        }

        private void CreateChart(List<KeyValuePair<int, int>> y, string chartName, string Title)
        {
            Chart Chart1 = new Chart();
            List<string> x = new List<string>() { "Passed", "Failed", "Stopped", "Other" };
            List<int> yList = (from ylist in y select ylist.Key).ToList();
            int xAxis = 0;
            string total = "";
            Chart1.BackColor = System.Drawing.Color.AliceBlue;
            Chart1.BackColor = System.Drawing.Color.White;
            Chart1.Series.Add(new Series());
            ChartArea a1 = new ChartArea();
            a1.Name = "Area";
            Chart1.ChartAreas.Add(a1);
            a1.InnerPlotPosition = new ElementPosition(12, 10, 78, 78);
            Chart1.Series[0].ChartArea = "Area";
            Chart1.Series[0].Points.DataBindXY(x, yList);
            Chart1.Series["Series1"].Label = "#VALX (#VALY)";
            Chart1.Series[0].ChartType = SeriesChartType.Doughnut;
            Chart1.Series[0]["DoughnutRadius"] = "20";
            Chart1.Series[0]["DoughnutInnerRadius"] = "99";
            Chart1.Series[0]["PieLabelStyle"] = "Outside";
            Chart1.Series[0].BorderWidth = 1;
            Chart1.Series[0].BorderDashStyle = ChartDashStyle.Dot;
            Chart1.Series[0].BorderColor = System.Drawing.Color.FromArgb(200, 26, 59, 105);
            foreach (KeyValuePair<int, int> l in y)
            {
                if (l.Key == 0)
                {
                    Chart1.Series[0].Points[l.Value].BorderColor = System.Drawing.Color.White;
                    Chart1.Series["Series1"].Points[l.Value].AxisLabel = "";
                    Chart1.Series["Series1"].Points[l.Value].Label = "";
                }
            }
            Chart1.Series[0].Points[0].Color = Chart1.Series[0].Points[0].LabelForeColor = GingerCore.General.makeColor("#008000");
            Chart1.Series[0].Points[1].Color = Chart1.Series[0].Points[1].LabelForeColor = GingerCore.General.makeColor("#FF0000");
            Chart1.Series[0].Points[2].Color = Chart1.Series[0].Points[2].LabelForeColor = GingerCore.General.makeColor("#ff57ab");
            Chart1.Series[0].Points[3].Color = Chart1.Series[0].Points[3].LabelForeColor = GingerCore.General.makeColor("#1B3651");
            Chart1.Series[0].Font = new Font("sans-serif", 9, System.Drawing.FontStyle.Bold);
            Chart1.Height = 180;
            Chart1.Width = 310;
            System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(GingerCore.General.makeColor("#e3dfdb"));
            System.Drawing.SolidBrush myBrush1 = new System.Drawing.SolidBrush(GingerCore.General.makeColor("#1B3651"));
            Chart1.Titles.Add("NewTitle");
            Chart1.Titles["Title1"].Text = Title;
            Chart1.Titles["Title1"].Font = new Font("sans-serif", 11, System.Drawing.FontStyle.Bold);
            Chart1.Titles["Title1"].ForeColor = GingerCore.General.makeColor("#1B3651");
            MemoryStream m = new MemoryStream();
            Chart1.SaveImage(m, ChartImageFormat.Png);
            Bitmap bitMapImage = new System.Drawing.Bitmap(m);
            Graphics graphicImage = Graphics.FromImage(bitMapImage);
            graphicImage.SmoothingMode = SmoothingMode.AntiAlias;
            graphicImage.FillEllipse(myBrush, 132, 75, 50, 50);
            total = yList.Sum().ToString();
            if (total.Length == 1)
            {
                xAxis = 151;
            }
            else if (total.Length == 2)
            {
                xAxis = 145;
            }
            else if (total.Length == 3)
            {
                xAxis = 142;
            }
            else if (total.Length == 4)
            {
                xAxis = 140;
            }
            graphicImage.DrawString(yList.Sum().ToString(), new Font("sans-serif", 9, System.Drawing.FontStyle.Bold), myBrush1, new System.Drawing.Point(xAxis, 91));
            m = new MemoryStream();
            bitMapImage.Save(tempFolder + "\\" + chartName, ImageFormat.Jpeg);
            graphicImage.Dispose();
            bitMapImage.Dispose();
        }
        public LinkedResource GetLinkedResource(byte[] imageBytes, string id)
        {
            ContentType c = new ContentType("image/png");
            LinkedResource linkedResource = new LinkedResource(new MemoryStream(imageBytes));
            linkedResource.ContentType = c;
            linkedResource.ContentId = id;
            linkedResource.TransferEncoding = TransferEncoding.Base64;
            return linkedResource;
        }
        public byte[] GetImageStream(string path)
        {
            if (!File.Exists(path))
                return null;
            System.Drawing.Image img = System.Drawing.Image.FromFile(path);
            byte[] arr;
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                arr = ms.ToArray();
            }
            return arr;
        }
        //TODO: Move the Zipit function to Email.Addattach function
        void AddAttachmentToEmail(Email e, string FileName, bool ZipIt, EmailAttachment.eAttachmentType AttachmentType)
        {
            if (ZipIt)
            {
                //We use this trick to get valid temp unique file name, then convert it to folder
                //Create sub dir to hold the file
                // Copy the file to the sub folder, keep the name
                // Create target zip file name               
                String ZipFileName = "";
                if (AttachmentType == EmailAttachment.eAttachmentType.Report)
                {
                    ZipFileName = "Full HTML Report.zip";
                }
                else
                {
                    ZipFileName = Path.GetFileNameWithoutExtension(FileName) + ".zip";
                }
                //Create the Zip file if file not exists otherwise delete existing one and then create new.
                try
                {
                    if (File.Exists(tempFolder + @"\" + ZipFileName))
                    {
                        File.Delete(tempFolder + @"\" + ZipFileName);
                    }
                    ZipFile.CreateFromDirectory(FileName, tempFolder + @"\" + ZipFileName);                    
                }
                catch (Exception ex)
                {
                    ZipFileName = Path.GetFileNameWithoutExtension(FileName) + DateTime.Now.ToString("MMddyyyy_HHmmss") + ".zip";
                    ZipFile.CreateFromDirectory(FileName, tempFolder + @"\" + ZipFileName);
                    Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                }
                e.Attachments.Add(tempFolder + @"\" + ZipFileName);
            }
            else
            {
                e.Attachments.Add(FileName);
            }
        }
        public long CalculateFileSize(Email e)
        {
            long s1 = new long();
            foreach (string s in e.Attachments)
            {
                FileInfo f = new FileInfo(s);
                s1 += f.Length;
            }
            return s1;
        }
        public override Page GetEditPage()
        {
            RunSetActionHTMLReportSendEmailEditPage RSAEREP = new RunSetActionHTMLReportSendEmailEditPage(this);
            return RSAEREP;
        }
        public static string OverrideHTMLRelatedCharacters(string text)
        {
            text = text.Replace(@"<", "&#60;");
            text = text.Replace(@">", "&#62;");
            text = text.Replace(@"$", "&#36;");
            text = text.Replace(@"%", "&#37;");
            return text;
        }

        public override void PrepareDuringExecAction(ObservableList<GingerRunner> Gingers)
        {
            throw new NotImplementedException();
        }
    }
}


