#region License
/*
Copyright © 2014-2023 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.LiteDBFolder;
using Amdocs.Ginger.CoreNET.Logger;
using Amdocs.Ginger.CoreNET.Utility;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using Ginger.Reports.GingerExecutionReport;
using GingerCore;
using GingerCore.DataSource;
using GingerCore.GeneralLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using static Ginger.Run.RunSetActions.RunSetActionBase;

namespace Ginger.Run.RunSetActions
{
    public class RunSetActionHTMLReportSendEmailOperations : IRunSetActionHTMLReportSendEmailOperations
    {
        public RunSetActionHTMLReportSendEmail RunSetActionHTMLReportSendEmail;
        public RunSetActionHTMLReportSendEmailOperations(RunSetActionHTMLReportSendEmail runSetActionHTMLReportSendEmail)
        {
            this.RunSetActionHTMLReportSendEmail = runSetActionHTMLReportSendEmail;
            this.RunSetActionHTMLReportSendEmail.RunSetActionHTMLReportSendEmailOperations = this;
        }
        private static readonly Object thisObj = new object();

        ValueExpression mValueExpression = null;

        private string emailReadyHtml = string.Empty;

        public string tempFolder = string.Empty;

        public string TemplatesFolder = string.Empty;

        bool IsExecutionStatistic = false;

        public string reportsResultFolder = string.Empty;

        public string ReportPath = string.Empty;
        private string reportTimeStamp = string.Empty;

        public void Execute(IReportInfo RI)
        {
            try
            {
                EmailOperations emailOperations = new EmailOperations(RunSetActionHTMLReportSendEmail.Email);
                RunSetActionHTMLReportSendEmail.Email.EmailOperations = emailOperations;

                HTMLReportsConfiguration currentConf = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
                //Reporter.ToLog(eLogLevel.DEBUG, "Run set operation send Email Staring execute");
                mValueExpression = new ValueExpression(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>(), false, "", false);
                string extraInformationCalculated = string.Empty;
                string calculatedName = string.Empty;
                //Make sure we clear in case use open the edit page twice
                RunSetActionHTMLReportSendEmail.Email.Attachments.Clear();
                RunSetActionHTMLReportSendEmail.Email.EmailOperations.alternateView = null;
                LiteDbRunSet liteDbRunSet = null;
                var loggerMode = WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod;
                const int MAXPATHLENGTH = 150;                
                if (loggerMode == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
                {
                    //Reporter.ToLog(eLogLevel.DEBUG, "Run set operation send Email: Using LiteDB and using new WebReportGenerator");
                    reportsResultFolder = Path.Combine(ExtensionMethods.GetReportDirectory(currentConf.HTMLReportsFolder), "Reports");
                    string RunsetName = General.RemoveInvalidFileNameChars(WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name);
                    string DateTimeStamp = DateTime.UtcNow.ToString("yyyymmddhhmmssfff");                    
                    int remainingChars = MAXPATHLENGTH - (reportsResultFolder.Length + DateTimeStamp.Length);
                    if(remainingChars > 0 && RunsetName.Length > remainingChars)
                    {
                        reportsResultFolder = Path.Combine(reportsResultFolder, $"{RunsetName.Substring(0, remainingChars)}_{DateTimeStamp}");
                    }
                    else
                    {
                        reportsResultFolder = Path.Combine(reportsResultFolder, $"{RunsetName}_{DateTimeStamp}");
                    }
                    
                    //Check if report directory already exists, if yes, change the timestamp to latest plus the retry number in report path, retry for 5 times, rare scenario where it will take 5 retries 
                    int numberOfRetry = 0;
                    while (Directory.Exists(reportsResultFolder) && numberOfRetry <= 5)
                    {
                        reportsResultFolder = reportsResultFolder.Replace(DateTimeStamp, DateTime.UtcNow.ToString("yyyymmddhhmmssfff") + "_" + ++numberOfRetry);
                    }
                    WebReportGenerator webReporterRunner = new WebReportGenerator();
                    liteDbRunSet = webReporterRunner.RunNewHtmlReport(reportsResultFolder, null, null, false);
                }

                tempFolder = WorkSpace.Instance.ReportsInfo.EmailReportTempFolder;

                TemplatesFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Reports", "GingerExecutionReport");

                //Reporter.ToLog(eLogLevel.DEBUG, "Run set operation send Email: TemplatesFolder=" + TemplatesFolder);

                string runSetFolder = string.Empty;
                if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder != null)
                {
                    runSetFolder = WorkSpace.Instance.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder;
                }
                else
                {
                    if (loggerMode == ExecutionLoggerConfiguration.DataRepositoryMethod.TextFile)
                    {
                        GingerExecutionEngine gr = new GingerExecutionEngine(new GingerRunner());
                        runSetFolder = gr.ExecutionLoggerManager.GetRunSetLastExecutionLogFolderOffline();
                    }

                }

                //Reporter.ToLog(eLogLevel.DEBUG, "Run set operation send Email: runSetFolder=" + runSetFolder);

                var ReportItem = RunSetActionHTMLReportSendEmail.EmailAttachments.Where(x => x.AttachmentType == EmailAttachment.eAttachmentType.Report).FirstOrDefault();

                if (RunSetActionHTMLReportSendEmail.HTMLReportTemplate == RunSetActionHTMLReportSendEmail.eHTMLReportTemplate.HTMLReport)
                {
                    if (loggerMode == ExecutionLoggerConfiguration.DataRepositoryMethod.TextFile)
                    {
                        if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.RunsetExecLoggerPopulated)
                        {
                            if (RunSetActionHTMLReportSendEmail.selectedHTMLReportTemplateID > -1)
                            {
                                int totalRunners = WorkSpace.Instance.RunsetExecutor.Runners.Count;
                                int totalPassed = WorkSpace.Instance.RunsetExecutor.Runners.Where(runner => runner.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed).Count();
                                int totalExecuted = totalRunners - WorkSpace.Instance.RunsetExecutor.Runners.Where(runner => runner.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending || runner.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped || runner.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked).Count();
                                ReportInfo offlineReportInfo = new ReportInfo(runSetFolder);
                                ((RunSetReport)offlineReportInfo.ReportInfoRootObject).RunSetExecutionRate = (totalExecuted * 100 / totalRunners).ToString();
                                ((RunSetReport)offlineReportInfo.ReportInfoRootObject).GingerRunnersPassRate = (totalPassed * 100 / totalRunners).ToString();
                                CreateSummaryViewReportForEmailAction(offlineReportInfo);
                            }
                            else
                            {
                                RunSetActionHTMLReportSendEmail.Errors = "Default Template is not available, add Report Template in Configuration.";
                                Reporter.HideStatusMessage();
                                RunSetActionHTMLReportSendEmail.Status = Ginger.Run.RunSetActions.RunSetActionBase.eRunSetActionStatus.Failed;
                                return;
                            }
                        }
                        else
                        {
                            RunSetActionHTMLReportSendEmail.Errors = "In order to get HTML report, please, perform executions before";
                            Reporter.HideStatusMessage();
                            RunSetActionHTMLReportSendEmail.Status = Ginger.Run.RunSetActions.RunSetActionBase.eRunSetActionStatus.Failed;
                            return;
                        }
                    }
                    else if (loggerMode == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
                    {
                        //Reporter.ToLog(eLogLevel.DEBUG, "Run set operation send Email: loggerMode is LiteDB");
                        try
                        {
                            CreateSummaryViewReportForEmailAction(liteDbRunSet);
                            // TODO: check multi run on same machine/user
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Run set operation send Email ,error :", ex);
                        }
                    }
                }

                if (RunSetActionHTMLReportSendEmail.EmailAttachments != null)
                {
                    foreach (EmailAttachment r in RunSetActionHTMLReportSendEmail.EmailAttachments)
                    {
                        //attach simple file
                        if (r.AttachmentType == EmailAttachment.eAttachmentType.File)
                        {
                            mValueExpression.Value = r.Name;
                            calculatedName = mValueExpression.ValueCalculated;
                            if (System.IO.File.Exists(calculatedName))
                            {
                                String TargetFileName = string.Empty;
                                if (r.ZipIt)
                                {
                                    String SubFolder = Path.Combine(tempFolder, Path.GetFileNameWithoutExtension(calculatedName));
                                    Directory.CreateDirectory(SubFolder);
                                    TargetFileName = Path.Combine(SubFolder, Path.GetFileName(calculatedName));
                                    if (File.Exists(TargetFileName))
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
                                AddAttachmentToEmail(RunSetActionHTMLReportSendEmail.Email, TargetFileName, r.ZipIt, EmailAttachment.eAttachmentType.File);
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
                            if (WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.TextFile)
                            {
                                mValueExpression.Value = rReport.ExtraInformation;
                                extraInformationCalculated = mValueExpression.ValueCalculated;
                                if (!string.IsNullOrEmpty(rReport.SelectedHTMLReportTemplateID.ToString()))
                                {
                                    if ((rReport.IsAlternameFolderUsed) && (extraInformationCalculated != null) && (extraInformationCalculated != string.Empty))
                                    {
                                        TargetFrameworkHelper.Helper.HTMLReportAttachment(extraInformationCalculated, ref emailReadyHtml, ref reportsResultFolder, runSetFolder, rReport, currentConf);
                                    }
                                    else
                                    {
                                        ObservableList<HTMLReportConfiguration> HTMLReportConfigurations = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>();
                                        reportsResultFolder = Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(runSetFolder),
                                                                                                                                                  false,
                                                                                                                                                  HTMLReportConfigurations.Where(x => (x.ID == rReport.SelectedHTMLReportTemplateID)).FirstOrDefault());
                                    }
                                }
                            }
                            else if (WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
                            {
                                if (rReport.IsAlternameFolderUsed)
                                {
                                    mValueExpression.Value = rReport.ExtraInformation;
                                    extraInformationCalculated = mValueExpression.ValueCalculated;

                                    var path = Path.Combine(extraInformationCalculated, $"{General.RemoveInvalidFileNameChars(WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name)}_{DateTime.UtcNow.ToString("yyyymmddhhmmss")}");
                                    if (Directory.Exists(path))
                                        Directory.Delete(path, true);
                                    IoHandler.Instance.CopyFolderRec(reportsResultFolder, path, true);
                                    reportsResultFolder = path;
                                }
                            }
                            if (!string.IsNullOrEmpty(reportsResultFolder))
                            {
                                AddAttachmentToEmail(RunSetActionHTMLReportSendEmail.Email, reportsResultFolder, r.ZipIt, EmailAttachment.eAttachmentType.Report);
                            }
                        }
                    }
                    long emailSize = CalculateAttachmentsSize(RunSetActionHTMLReportSendEmail.Email);

                    if (ReportItem != null)
                    {
                        if (((EmailHtmlReportAttachment)ReportItem).IsLinkEnabled || emailSize > 10000000)
                        {
                            // TODO: add warning or something !!!!

                            if (RunSetActionHTMLReportSendEmail.EmailAttachments.IndexOf(ReportItem) > -1)
                            {
                                if (RunSetActionHTMLReportSendEmail.Email.Attachments.Count > 0)
                                {
                                    RunSetActionHTMLReportSendEmail.Email.Attachments.RemoveAt(RunSetActionHTMLReportSendEmail.EmailAttachments.IndexOf(ReportItem));
                                }
                            }
                            if (!string.IsNullOrEmpty(reportsResultFolder))
                            {
                                string reportName = "\\GingerExecutionReport.html'";
                                if (loggerMode == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
                                    reportName = "\\viewreport.html'";
                                emailReadyHtml = emailReadyHtml.Replace("<!--FULLREPORTLINK-->", "<a href ='" + reportsResultFolder + reportName + " style ='font-size:16px;color:blue;text-decoration:underline'> Click Here to View Full Report </a>");
                                emailReadyHtml = emailReadyHtml.Replace("<!--WARNING-->", "");
                            }
                        }
                        else
                        {
                            if ((!((EmailHtmlReportAttachment)ReportItem).IsAlternameFolderUsed) && (emailSize > 10000000))
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
                if (RunSetActionHTMLReportSendEmail.HTMLReportTemplate == RunSetActionHTMLReportSendEmail.eHTMLReportTemplate.HTMLReport)
                {
                    RunSetActionHTMLReportSendEmail.Email.EmbededAttachment.Clear();
                    emailReadyHtml = emailReadyHtml.Replace("cid:gingerRunner", "cid:gingerRunner" + reportTimeStamp);
                    emailReadyHtml = emailReadyHtml.Replace("cid:Businessflow", "cid:Businessflow" + reportTimeStamp);
                    emailReadyHtml = emailReadyHtml.Replace("cid:Activity", "cid:Activity" + reportTimeStamp);
                    emailReadyHtml = emailReadyHtml.Replace("cid:Action", "cid:Action" + reportTimeStamp);

                    if (RunSetActionHTMLReportSendEmail.Email.EmailMethod.ToString() == "SMTP")
                    {
                        AlternateView alternativeView = AlternateView.CreateAlternateViewFromString(emailReadyHtml, null, MediaTypeNames.Text.Html);
                        alternativeView.ContentId = "htmlView";
                        alternativeView.TransferEncoding = TransferEncoding.SevenBit;
                        string beatLogoPath = Path.Combine(TemplatesFolder, "assets", "img", "@BeatLogo.png");
                        string gingerLogoPath = Path.Combine(TemplatesFolder, "assets", "img", "@Ginger.png");
                        string customerLogoPath = Path.Combine(TemplatesFolder, "assets", "img", "@Ginger.png");

                        if (File.Exists(beatLogoPath))
                            alternativeView.LinkedResources.Add(GetLinkedResource(GetImageStream(beatLogoPath), "beat"));
                        if (File.Exists(gingerLogoPath))
                            alternativeView.LinkedResources.Add(GetLinkedResource(GetImageStream(gingerLogoPath), "ginger"));
                        if (File.Exists(customerLogoPath))
                            alternativeView.LinkedResources.Add(GetLinkedResource(GetImageStream(customerLogoPath), "customer"));
                        if (!string.IsNullOrEmpty(RunSetActionHTMLReportSendEmail.Comments))
                        {
                            alternativeView.LinkedResources.Add(GetLinkedResource(GetImageStream(Path.Combine(TemplatesFolder, "assets", "img", "comments-icon.jpg")), "comment"));
                        }
                        if (IsExecutionStatistic)
                        {
                            if (File.Exists(Path.Combine(tempFolder, $"GingerRunner{reportTimeStamp}.jpeg")))
                                alternativeView.LinkedResources.Add(GetLinkedResource(GetImageStream(Path.Combine(tempFolder, $"GingerRunner{reportTimeStamp}.jpeg")), "gingerRunner" + reportTimeStamp));
                            if (File.Exists(Path.Combine(tempFolder, $"Action{reportTimeStamp}.jpeg")))
                                alternativeView.LinkedResources.Add(GetLinkedResource(GetImageStream(Path.Combine(tempFolder, $"Action{reportTimeStamp}.jpeg")), "Action" + reportTimeStamp));
                            if (File.Exists(Path.Combine(tempFolder, $"Activity{reportTimeStamp}.jpeg")))
                                alternativeView.LinkedResources.Add(GetLinkedResource(GetImageStream(Path.Combine(tempFolder, $"Activity{reportTimeStamp}.jpeg")), "Activity" + reportTimeStamp));
                            if (File.Exists(Path.Combine(tempFolder, $"Businessflow{reportTimeStamp}.jpeg")))
                                alternativeView.LinkedResources.Add(GetLinkedResource(GetImageStream(Path.Combine(tempFolder, $"Businessflow{reportTimeStamp}.jpeg")), "Businessflow" + reportTimeStamp));
                        }
                        RunSetActionHTMLReportSendEmail.Email.EmailOperations.alternateView = alternativeView;
                    }
                    else
                    {
                        RunSetActionHTMLReportSendEmail.Email.EmbededAttachment.Add(new KeyValuePair<string, string>(Path.Combine(TemplatesFolder, "assets", "img", "@BeatLogo.png"), "beat"));
                        RunSetActionHTMLReportSendEmail.Email.EmbededAttachment.Add(new KeyValuePair<string, string>(Path.Combine(TemplatesFolder, "assets", "img", "@Ginger.png"), "ginger"));
                        RunSetActionHTMLReportSendEmail.Email.EmbededAttachment.Add(new KeyValuePair<string, string>(Path.Combine(tempFolder, "CustomerLogo.png"), "customer"));
                        if (!string.IsNullOrEmpty(RunSetActionHTMLReportSendEmail.Comments))
                        {
                            RunSetActionHTMLReportSendEmail.Email.EmbededAttachment.Add(new KeyValuePair<string, string>(TemplatesFolder + @"\assets\\img\comments-icon.jpg", "comment"));
                        }
                        if (IsExecutionStatistic)
                        {
                            RunSetActionHTMLReportSendEmail.Email.EmbededAttachment.Add(new KeyValuePair<string, string>(Path.Combine(tempFolder, $"GingerRunner{reportTimeStamp}.jpeg"), "gingerRunner" + reportTimeStamp));
                            RunSetActionHTMLReportSendEmail.Email.EmbededAttachment.Add(new KeyValuePair<string, string>(Path.Combine(tempFolder, $"Action{reportTimeStamp}.jpeg"), "Action" + reportTimeStamp));
                            RunSetActionHTMLReportSendEmail.Email.EmbededAttachment.Add(new KeyValuePair<string, string>(Path.Combine(tempFolder, $"Activity{reportTimeStamp}.jpeg"), "Activity" + reportTimeStamp));
                            RunSetActionHTMLReportSendEmail.Email.EmbededAttachment.Add(new KeyValuePair<string, string>(Path.Combine(tempFolder, $"Businessflow{reportTimeStamp}.jpeg"), "Businessflow" + reportTimeStamp));
                        }
                    }
                }
                else
                {
                    if (loggerMode == ExecutionLoggerConfiguration.DataRepositoryMethod.TextFile)
                    {
                        if (ReportItem != null && !WorkSpace.Instance.RunsetExecutor.RunSetConfig.RunsetExecLoggerPopulated)
                        {
                            RunSetActionHTMLReportSendEmail.Errors = "In order to get HTML report, please, perform executions before";
                            Reporter.HideStatusMessage();
                            RunSetActionHTMLReportSendEmail.Status = Ginger.Run.RunSetActions.RunSetActionBase.eRunSetActionStatus.Failed;
                            return;
                        }
                    }
                    mValueExpression.Value = RunSetActionHTMLReportSendEmail.Bodytext;
                    if (ReportItem != null)
                    {
                        emailReadyHtml = "Full Report Shared Path =>" + reportsResultFolder + System.Environment.NewLine;
                    }
                    emailReadyHtml += mValueExpression.ValueCalculated;
                }
                //Reporter.ToLog(eLogLevel.DEBUG, "Run set operation send Email: Preparing email");
                mValueExpression.Value = RunSetActionHTMLReportSendEmail.MailFrom;
                RunSetActionHTMLReportSendEmail.Email.MailFrom = mValueExpression.ValueCalculated;
                mValueExpression.Value = RunSetActionHTMLReportSendEmail.MailTo;
                RunSetActionHTMLReportSendEmail.Email.MailTo = mValueExpression.ValueCalculated;
                mValueExpression.Value = RunSetActionHTMLReportSendEmail.MailCC;
                RunSetActionHTMLReportSendEmail.Email.MailCC = mValueExpression.ValueCalculated;
                mValueExpression.Value = RunSetActionHTMLReportSendEmail.Subject;
                RunSetActionHTMLReportSendEmail.Email.Subject = mValueExpression.ValueCalculated;
                RunSetActionHTMLReportSendEmail.Email.Body = emailReadyHtml;
                emailReadyHtml = string.Empty;

                if (RunSetActionHTMLReportSendEmail.Email.EmailMethod == Email.eEmailMethod.SMTP)
                {
                    mValueExpression.Value = RunSetActionHTMLReportSendEmail.MailFromDisplayName;
                    RunSetActionHTMLReportSendEmail.Email.MailFromDisplayName = mValueExpression.ValueCalculated;
                }
                bool isSuccess = false;
                try
                {
                    //Reporter.ToLog(eLogLevel.DEBUG, "Run set operation send Email: Before send email");                    
                    isSuccess = RunSetActionHTMLReportSendEmail.Email.EmailOperations.Send();
                    //Reporter.ToLog(eLogLevel.DEBUG, "Run set operation send Email: After send email result = " + isSuccess);
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to send mail", ex);
                    isSuccess = false;
                }
                if (isSuccess == false)
                {
                    RunSetActionHTMLReportSendEmail.Errors = RunSetActionHTMLReportSendEmail.Email.Event;
                    Reporter.HideStatusMessage();
                    RunSetActionHTMLReportSendEmail.Status = eRunSetActionStatus.Failed;
                }
            }
            finally
            {
                if (string.IsNullOrEmpty(tempFolder) == false && Directory.Exists(tempFolder))
                {
                    try
                    {
                        Directory.Delete(tempFolder, true);
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.WARN, string.Format("Failed to delete the eMail Report Temp Folder '{0}', Issue: '{1}'", tempFolder, ex.Message));
                    }
                }
            }
        }

        private void CreateSummaryViewReportForEmailAction(LiteDbRunSet liteDbRunSet)
        {
            reportTimeStamp = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss_fff");
            HTMLReportConfiguration currentTemplate = new HTMLReportConfiguration();
            ObservableList<HTMLReportConfiguration> HTMLReportConfigurations = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>();
            currentTemplate = HTMLReportConfigurations.Where(x => (x.ID == RunSetActionHTMLReportSendEmail.selectedHTMLReportTemplateID)).FirstOrDefault();
            if (currentTemplate == null && RunSetActionHTMLReportSendEmail.selectedHTMLReportTemplateID == 100)// for supporting dynamic runset report
            {
                currentTemplate = HTMLReportConfigurations.Where(x => (x.IsDefault == true)).FirstOrDefault();
            }
            TargetFrameworkHelper.Helper.CreateCustomerLogo(currentTemplate, tempFolder);
            if (currentTemplate == null)
            {
                currentTemplate = HTMLReportConfigurations.Where(x => (x.IsDefault == true)).FirstOrDefault();
            }
            if (liteDbRunSet == null || (liteDbRunSet.GetType() == typeof(Object)))
            {
                return;
            }

            string ReportHTML;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ReportHTML = Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetHTMLTemplate("EmailExecutionReport.html", TemplatesFolder);
            }
            else
            {
                ReportHTML = Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetHTMLTemplate("EmailExecutionReportOnLinux.html", TemplatesFolder);
            }

            List<KeyValuePair<int, int>> chartData = null;
            StringBuilder fieldsNamesHTMLTableCells = new StringBuilder();
            StringBuilder fieldsValuesHTMLTableCells = new StringBuilder();
            StringBuilder fieldsNamesHTMLTableCellsm = new StringBuilder();
            StringBuilder fieldsValuesHTMLTableCellsm = new StringBuilder();
            GetExecGeneralDetails(liteDbRunSet, currentTemplate, fieldsNamesHTMLTableCells, fieldsValuesHTMLTableCells, fieldsNamesHTMLTableCellsm, fieldsValuesHTMLTableCellsm);

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
                        if (!string.IsNullOrEmpty(executionStatisticsSection))
                        {
                            ReportHTML = ReportHTML.Replace(executionStatisticsSection, "");
                        }

                        IsExecutionStatistic = false;
                    }
                    else
                    {
                        chartData = GetExecStatistics(liteDbRunSet);
                    }
                }
                else if (selectedField.FieldKey == RunSetReport.Fields.ExecutedBusinessFlowsDetails)
                {
                    if (!selectedField.IsSelected)
                    {
                        string executionStatisticsSection = Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetStringBetween(ReportHTML, "<!--ExecutionBusinessFlowsDetails_Start-->", "<!--ExecutionBusinessFlowsDetails_End-->");
                        if (!string.IsNullOrEmpty(executionStatisticsSection))
                        {
                            ReportHTML = ReportHTML.Replace(executionStatisticsSection, "");
                        }
                    }
                    else
                    {
                        GetExecBFDetails(liteDbRunSet, currentTemplate, out fieldsNamesHTMLTableCells, out fieldsValuesHTMLTableCells);
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
                        {
                            ReportHTML = ReportHTML.Replace(executionStatisticsSection, "");
                        }
                    }
                    else
                    {
                        fieldsNamesHTMLTableCells = new StringBuilder();
                        fieldsValuesHTMLTableCells = new StringBuilder();
                        ExecutedActivitiesDetailsGenrator(liteDbRunSet, currentTemplate, ref ReportHTML, fieldsNamesHTMLTableCells, fieldsValuesHTMLTableCells);
                    }

                    ReportHTML = ReportHTML.Replace("{ActivitiesDetails_Headers}", fieldsNamesHTMLTableCells.ToString());
                    ReportHTML = ReportHTML.Replace("{ActivitiesDetails_Data}", fieldsValuesHTMLTableCells.ToString());
                }
                else if (selectedField.FieldKey == RunSetReport.Fields.FailuresDetails)
                {
                    bool isFailuresDetailsExists = false;

                    if (selectedField.IsSelected)
                    {
                        isFailuresDetailsExists = GetFailureDetails(liteDbRunSet, currentTemplate, out fieldsNamesHTMLTableCells, out fieldsValuesHTMLTableCells, isFailuresDetailsExists);
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
                        {
                            ReportHTML = ReportHTML.Replace(failureDetailsSection, "");
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(RunSetActionHTMLReportSendEmail.Comments))
            {
                mValueExpression.Value = RunSetActionHTMLReportSendEmail.Comments;
                ReportHTML = ReportHTML.Replace("{COMMENT}", "<img src='cid:comment'/>" + mValueExpression.ValueCalculated);
            }
            else
            {
                ReportHTML = ReportHTML.Replace("{COMMENT}", "");
            }
            fieldsNamesHTMLTableCells.Remove(0, fieldsNamesHTMLTableCells.Length);
            fieldsValuesHTMLTableCells.Remove(0, fieldsValuesHTMLTableCells.Length);
            emailReadyHtml = ReportHTML;
            ReportHTML = null;
        }

        private static bool GetFailureDetails(LiteDbRunSet liteDbRunSet, HTMLReportConfiguration currentTemplate, out StringBuilder fieldsNamesHTMLTableCells, out StringBuilder fieldsValuesHTMLTableCells, bool isFailuresDetailsExists)
        {
            fieldsNamesHTMLTableCells = new StringBuilder();
            fieldsValuesHTMLTableCells = new StringBuilder();
            List<int> listOfHandledGingerRunnersReport = new List<int>();
            bool firstIteration = true;
            foreach (LiteDbRunner GR in liteDbRunSet.RunnersColl.Where(x => x.RunStatus == eRunStatus.Failed.ToString()).OrderBy(x => x.Seq))
            {
                foreach (LiteDbBusinessFlow br in GR.BusinessFlowsColl.Where(x => x.RunStatus == eRunStatus.Failed.ToString()))
                {
                    foreach (LiteDbActivity ac in br.ActivitiesColl.Where(x => x.RunStatus == eRunStatus.Failed.ToString()).OrderBy(x => x.Seq))
                    {
                        foreach (LiteDbAction act in ac.ActionsColl.Where(x => x.RunStatus == eRunStatus.Failed.ToString()).OrderBy(x => x.Seq))
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
                                        fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                    }
                                    fieldsValuesHTMLTableCells.Append(@"<td style='padding: 10px; border: 1px solid #dddddd'>" + GR.Name + @"</td>");
                                }
                            }

                            // Business Flow Level
                            foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.BusinessFlowFieldsToSelect.Where(x => (x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                            {
                                if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.Seq)
                                {
                                    if (firstIteration)
                                    {
                                        fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Sequence</td>");
                                    }
                                    fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + br.Seq.ToString() + "</td>");
                                }
                                if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.Name)
                                {
                                    if (firstIteration)
                                    {
                                        fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                    }
                                    fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + br.Name + "</td>");
                                }
                            }

                            // Activity Level 
                            foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.ActivityFieldsToSelect.Where(x => (x.FieldName != "ScreenShot" && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                            {
                                string fieldName = EmailToObjectFieldName(selectedField_internal.FieldKey);
                                if (selectedField_internal.FieldKey == ActivityReport.Fields.Seq)
                                {
                                    if (firstIteration)
                                    {
                                        fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>Activity Sequence</td>");
                                    }
                                    fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + OverrideHTMLRelatedCharacters(ac.Seq.ToString()) + "</td>");
                                }
                                if (selectedField_internal.FieldKey == ActivityReport.Fields.ActivityName)
                                {
                                    if (firstIteration)
                                    {
                                        fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                    }
                                    fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + OverrideHTMLRelatedCharacters(ac.Name) + "</td>");
                                }
                            }

                            // Action Level
                            foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.ActionFieldsToSelect.Where(x => (x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                            {
                                string fieldName = EmailToObjectFieldName(selectedField_internal.FieldKey);
                                if (selectedField_internal.FieldKey == ActionReport.Fields.Seq)
                                {
                                    if (firstIteration)
                                    {
                                        fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>Action Execution Sequence</td>");
                                    }
                                    fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + OverrideHTMLRelatedCharacters(act.Seq.ToString()) + "</td>");
                                }
                                if (selectedField_internal.FieldKey == ActionReport.Fields.Description)
                                {
                                    if (firstIteration)
                                    {
                                        fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>Action Description</td>");
                                    }
                                    fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + OverrideHTMLRelatedCharacters(act.Description) + "</td>");
                                }
                                if (selectedField_internal.FieldKey == ActionReport.Fields.RunDescription)
                                {
                                    if (firstIteration)
                                    {
                                        fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                    }
                                    fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + OverrideHTMLRelatedCharacters(act.RunDescription) + "</td>");
                                }
                                if (selectedField_internal.FieldKey == ActionReport.Fields.Error)
                                {
                                    if (firstIteration)
                                    {
                                        fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                    }
                                    fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd; color:red;white-space:pre-wrap;white-space:-moz-pre-wrap;white-space:-pre-wrap;white-space:-o-pre-wrap;word-break: break-all;'>" + OverrideHTMLRelatedCharacters(act.Error) + "</td>");
                                }
                                if (selectedField_internal.FieldKey == ActionReport.Fields.ElapsedSecs)
                                {
                                    if (firstIteration)
                                    {
                                        fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                    }
                                    fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + OverrideHTMLRelatedCharacters(General.TimeConvert(Convert.ToString(act.GetType().GetProperty(fieldName).GetValue(act)))) + "</td>");
                                }
                                if ((selectedField_internal.FieldKey == ActionReport.Fields.CurrentRetryIteration) ||
                                    (selectedField_internal.FieldKey == ActionReport.Fields.ExInfo))
                                {
                                    if (firstIteration)
                                    {
                                        fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                    }

                                    fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + OverrideHTMLRelatedCharacters(Convert.ToString(act.GetType().GetProperty(fieldName).GetValue(act))) + "</td>");
                                }
                            }
                            fieldsValuesHTMLTableCells.Append("</tr>");
                            firstIteration = false;
                        }
                    }
                }
            }

            return isFailuresDetailsExists;
        }

        private static void GetExecBFDetails(LiteDbRunSet liteDbRunSet, HTMLReportConfiguration currentTemplate, out StringBuilder fieldsNamesHTMLTableCells, out StringBuilder fieldsValuesHTMLTableCells)
        {
            fieldsNamesHTMLTableCells = new StringBuilder();
            fieldsValuesHTMLTableCells = new StringBuilder();
            List<int> listOfHandledGingerRunnersReport = new List<int>();
            string tableColor = "<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>";
            string tableStyle = @"<td style='padding: 10px; border: 1px solid #dddddd'>";
            bool firstIteration = true;
            foreach (LiteDbRunner GR in (liteDbRunSet).RunnersColl.OrderBy(x => x.Seq))
            {
                foreach (LiteDbBusinessFlow br in GR.BusinessFlowsColl)
                {
                    List<string> selectedRunnerFields = new List<string> { GingerReport.Fields.Name, GingerReport.Fields.EnvironmentName, GingerReport.Fields.Seq };
                    fieldsValuesHTMLTableCells.Append("<tr>");
                    foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.GingerRunnerFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString() && selectedRunnerFields.Contains(x.FieldKey))))
                    {
                        string fieldName = EmailToObjectFieldName(selectedField_internal.FieldKey);
                        if (firstIteration && selectedField_internal.FieldKey != GingerReport.Fields.Seq)
                        {
                            fieldsNamesHTMLTableCells.Append(tableColor + selectedField_internal.FieldName + "</td>");
                        }
                        if ((selectedField_internal.FieldKey == GingerReport.Fields.Name) && (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.SummaryViewLevel.ToString()))
                        {
                            fieldsValuesHTMLTableCells.Append(tableStyle + GR.GetType().GetProperty(fieldName).GetValue(GR) + @"</td>");
                        }
                        else if (selectedField_internal.FieldKey == GingerReport.Fields.EnvironmentName)
                        {
                            fieldsValuesHTMLTableCells.Append(tableStyle + ExtensionMethods.OverrideHTMLRelatedCharacters(GR.GetType().GetProperty(fieldName).GetValue(GR) != null ? GR.GetType().GetProperty(fieldName).GetValue(GR).ToString() : string.Empty) + "</td>");
                        }
                        else if (selectedField_internal.FieldKey == GingerReport.Fields.Seq)
                        {
                            if (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.SummaryViewLevel.ToString())
                            {
                                int currentSeq = Convert.ToInt32(GR.GetType().GetProperty(fieldName).GetValue(GR).ToString());
                                if (!listOfHandledGingerRunnersReport.Contains(currentSeq))
                                {
                                    listOfHandledGingerRunnersReport.Add(currentSeq);
                                }
                            }
                        }
                    }
                    List<string> selectedBFFields = new List<string> { BusinessFlowReport.Fields.Seq, BusinessFlowReport.Fields.Name, BusinessFlowReport.Fields.Description, BusinessFlowReport.Fields.RunDescription,
                        BusinessFlowReport.Fields.ExecutionDuration, BusinessFlowReport.Fields.RunStatus, BusinessFlowReport.Fields.ExecutionRate, BusinessFlowReport.Fields.PassPercent };
                    foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.BusinessFlowFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString() && selectedBFFields.Contains(x.FieldKey))))
                    {
                        string fieldName = EmailToObjectFieldName(selectedField_internal.FieldKey);
                        if (firstIteration)
                        {
                            fieldsNamesHTMLTableCells.Append(tableColor + selectedField_internal.FieldName + "</td>");
                        }
                        if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.Seq)
                        {
                            fieldsValuesHTMLTableCells.Append(tableStyle + ExtensionMethods.OverrideHTMLRelatedCharacters(br.GetType().GetProperty(fieldName).GetValue(br).ToString()) + "</td>");
                        }
                        else if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.Name)
                        {
                            if (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.GingerRunnerLevel.ToString())
                            {
                                fieldsValuesHTMLTableCells.Append(tableStyle + br.GetType().GetProperty(fieldName).GetValue(br) + @"</td>");
                            }
                            else
                            {
                                fieldsValuesHTMLTableCells.Append(tableStyle + ExtensionMethods.OverrideHTMLRelatedCharacters(br.GetType().GetProperty(fieldName).GetValue(br).ToString()) + "</td>");
                            }
                        }
                        else if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.Description || selectedField_internal.FieldKey == BusinessFlowReport.Fields.RunDescription)
                        {
                            try
                            {
                                fieldsValuesHTMLTableCells.Append(tableStyle + ExtensionMethods.OverrideHTMLRelatedCharacters(br.GetType().GetProperty(fieldName).GetValue(br) != null ? br.GetType().GetProperty(fieldName).GetValue(br).ToString() : string.Empty) + "</td>");
                            }
                            catch
                            {
                                fieldsValuesHTMLTableCells.Append(tableStyle);
                            }
                        }
                        else if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.ExecutionDuration)
                        {
                            fieldsValuesHTMLTableCells.Append(tableStyle + ExtensionMethods.OverrideHTMLRelatedCharacters(br.GetType().GetProperty(fieldName).GetValue(br) != null ? General.TimeConvert(br.GetType().GetProperty(fieldName).GetValue(br).ToString()) : string.Empty) + "</td>");
                        }
                        else if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.RunStatus)
                        {
                            fieldsValuesHTMLTableCells.Append("<td style='padding:10px;border:1px solid #dddddd;' class='Status" + (br.GetType().GetProperty(fieldName).GetValue(br)).ToString() + "'>" + br.GetType().GetProperty(fieldName).GetValue(br) + "</td>");
                        }
                        else if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.ExecutionRate)
                        {
                            fieldsValuesHTMLTableCells.Append(tableStyle + br.GetType().GetProperty(fieldName).GetValue(br) + " %</td>");
                        }
                        else if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.PassPercent)
                        {
                            fieldsValuesHTMLTableCells.Append(tableStyle + br.GetType().GetProperty(fieldName).GetValue(br) + " %</td>");
                        }
                    }
                    fieldsValuesHTMLTableCells.Append("</tr>");
                    firstIteration = false;
                }
            }
        }

        private void ExecutedActivitiesDetailsGenrator(LiteDbRunSet liteDbRunSet, HTMLReportConfiguration currentTemplate, ref string reportHTML, StringBuilder fieldsNamesHTMLTableCells, StringBuilder fieldsValuesHTMLTableCells)
        {
            List<int> listOfHandledGingerRunnersReport = new List<int>();
            bool firstActivityIteration = true;
            string tableColor = "<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>";
            string tableStyle = @"<td style='padding: 10px; border: 1px solid #dddddd'>";
            foreach (LiteDbRunner GR in liteDbRunSet.RunnersColl.OrderBy(x => x.Seq))
            {
                string flowReportName = string.Empty;
                string executionSeq = string.Empty;
                string businessReportName = string.Empty;
                foreach (LiteDbBusinessFlow br in GR.BusinessFlowsColl)
                {
                    bool newBusinessFlow = true;
                    fieldsValuesHTMLTableCells.Append("<tr>");
                    List<string> selectedRunnerFields = new List<string> { GingerReport.Fields.Name, GingerReport.Fields.Seq };
                    foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.GingerRunnerFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString() && selectedRunnerFields.Contains(x.FieldKey))))
                    {
                        string fieldName = EmailToObjectFieldName(selectedField_internal.FieldKey);
                        if ((selectedField_internal.FieldKey == GingerReport.Fields.Name) && (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.SummaryViewLevel.ToString()))
                        {
                            if (firstActivityIteration)
                            {
                                fieldsNamesHTMLTableCells.Append(tableColor + selectedField_internal.FieldName + "</td>");
                            }
                            flowReportName = Convert.ToString(GR.GetType().GetProperty(fieldName).GetValue(GR));
                            fieldsValuesHTMLTableCells.Append(tableStyle + flowReportName + @"</td>");
                        }
                        else if (selectedField_internal.FieldKey == GingerReport.Fields.Seq)
                        {
                            if (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.SummaryViewLevel.ToString())
                            {
                                int currentSeq = Convert.ToInt32(Convert.ToString(GR.GetType().GetProperty(fieldName).GetValue(GR)));
                                if (!listOfHandledGingerRunnersReport.Contains(currentSeq))
                                {
                                    listOfHandledGingerRunnersReport.Add(currentSeq);
                                }
                            }
                        }
                    }
                    List<string> selectedBFFields = new List<string> { BusinessFlowReport.Fields.Seq, BusinessFlowReport.Fields.Name };
                    foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.BusinessFlowFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString() && selectedBFFields.Contains(x.FieldKey))))
                    {
                        string fieldName = EmailToObjectFieldName(selectedField_internal.FieldKey);
                        if (firstActivityIteration)
                        {
                            fieldsNamesHTMLTableCells.Append(tableColor + selectedField_internal.FieldName + "</td>");
                        }
                        if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.Seq)
                        {

                            executionSeq = ExtensionMethods.OverrideHTMLRelatedCharacters(Convert.ToString(br.GetType().GetProperty(fieldName).GetValue(br)));
                            fieldsValuesHTMLTableCells.Append(tableStyle + executionSeq + "</td>");
                        }
                        if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.Name)
                        {
                            if (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.GingerRunnerLevel.ToString())
                            {
                                businessReportName = br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br).ToString();
                                fieldsValuesHTMLTableCells.Append(tableStyle + businessReportName + @"</td>");
                            }
                            else
                            {
                                businessReportName = ExtensionMethods.OverrideHTMLRelatedCharacters(Convert.ToString(br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br)));
                                fieldsValuesHTMLTableCells.Append(tableStyle + businessReportName + "</td>");
                            }
                        }
                    }
                    // Activities details table 
                    foreach (LiteDbActivity activityReport in br.ActivitiesColl)
                    {
                        if (!newBusinessFlow)
                        {
                            fieldsValuesHTMLTableCells.Append(tableStyle + flowReportName + "</td>");
                            fieldsValuesHTMLTableCells.Append(tableStyle + executionSeq + "</td>");
                            fieldsValuesHTMLTableCells.Append(tableStyle + businessReportName + "</td>");
                        }
                        List<string> selectedActivityFields = new List<string> { ActivityReport.Fields.Seq, ActivityReport.Fields.ActivityGroupName, ActivityReport.Fields.ActivityName, ActivityReport.Fields.Description,
                            ActivityReport.Fields.RunDescription, ActivityReport.Fields.StartTimeStamp, ActivityReport.Fields.EndTimeStamp, ActivityReport.Fields.ElapsedSecs, ActivityReport.Fields.NumberOfActions,
                            ActivityReport.Fields.RunStatus, ActivityReport.Fields.ActionsDetails };
                        foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.ActivityFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString() && selectedActivityFields.Contains(x.FieldKey))))
                        {
                            string fieldName = EmailToObjectFieldName(selectedField_internal.FieldKey);
                            if (firstActivityIteration)
                            {
                                if (selectedField_internal.FieldKey == ActivityReport.Fields.Seq)
                                {
                                    fieldsNamesHTMLTableCells.Append(tableColor + "Activity " + selectedField_internal.FieldName + "</td>");
                                }
                                else
                                {
                                    fieldsNamesHTMLTableCells.Append(tableColor + selectedField_internal.FieldName + "</td>");
                                }
                            }
                            if (selectedField_internal.FieldKey == ActivityReport.Fields.ActivityName)
                            {
                                if (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.GingerRunnerLevel.ToString())
                                {
                                    fieldsValuesHTMLTableCells.Append(tableStyle + activityReport.GetType().GetProperty(fieldName).GetValue(activityReport) + @"</td>");
                                }
                                else
                                {
                                    fieldsValuesHTMLTableCells.Append(tableStyle + ExtensionMethods.OverrideHTMLRelatedCharacters(Convert.ToString(activityReport.GetType().GetProperty(fieldName).GetValue(activityReport))) + "</td>");
                                }
                            }
                            else if (selectedField_internal.FieldKey == ActivityReport.Fields.RunStatus)
                            {
                                string activityRunStatusValue = ExtensionMethods.OverrideHTMLRelatedCharacters(Convert.ToString(activityReport.GetType().GetProperty(fieldName).GetValue(activityReport)));
                                fieldsValuesHTMLTableCells.Append("<td class='Status" + activityRunStatusValue + "' style='padding: 10px; border: 1px solid #dddddd'>" + activityRunStatusValue + "</td>");
                            }
                            else if (selectedField_internal.FieldKey == ActivityReport.Fields.ElapsedSecs)
                            {
                                string activityElapsedSecsValue = ExtensionMethods.OverrideHTMLRelatedCharacters(Convert.ToString(activityReport.GetType().GetProperty(fieldName).GetValue(activityReport)));
                                fieldsValuesHTMLTableCells.Append(tableStyle + General.TimeConvert(activityElapsedSecsValue) + "</td>");
                            }
                            else
                            {
                                string activityHTMLValue = (activityReport.GetType().GetProperty(fieldName).GetValue(activityReport) == null) ? "N/A" : ExtensionMethods.OverrideHTMLRelatedCharacters(Convert.ToString(activityReport.GetType().GetProperty(fieldName).GetValue(activityReport)));
                                fieldsValuesHTMLTableCells.Append(tableStyle + activityHTMLValue + "</td>");
                            }

                        }
                        fieldsValuesHTMLTableCells.Append("</tr>");
                        newBusinessFlow = false;
                        firstActivityIteration = false;
                    }
                }
            }
        }

        private List<KeyValuePair<int, int>> GetExecStatistics(LiteDbRunSet liteDbRunSet)
        {
            List<KeyValuePair<int, int>> chartData;
            IsExecutionStatistic = true;
            int totalRunners = liteDbRunSet.RunnersColl.Count;
            int totalPassedRunners = liteDbRunSet.RunnersColl.Where(runner => runner.RunStatus == eRunStatus.Passed.ToString()).Count();
            int totalFailedRunners = liteDbRunSet.RunnersColl.Where(runner => runner.RunStatus == eRunStatus.Failed.ToString()).Count();
            int totalStoppedRunners = liteDbRunSet.RunnersColl.Where(runner => runner.RunStatus == eRunStatus.Stopped.ToString()).Count();
            int totalOtherRunners = totalRunners - (totalPassedRunners + totalFailedRunners + totalStoppedRunners);
            //Ginger Runners Place Holders
            chartData = new List<KeyValuePair<int, int>>();
            chartData.Add(new KeyValuePair<int, int>(totalPassedRunners, 0));
            chartData.Add(new KeyValuePair<int, int>(totalFailedRunners, 1));
            chartData.Add(new KeyValuePair<int, int>(totalStoppedRunners, 2));
            chartData.Add(new KeyValuePair<int, int>(totalOtherRunners, 3));
            CreateChart(chartData, "GingerRunner" + reportTimeStamp + ".jpeg", "Ginger Runners");

            int totalBFs = 0, totalPassedBFs = 0, totalStoppedBFs = 0, totalFailedBFs = 0, totalOtherBFs = 0;
            int totalActivities = 0, totalPassedActivities = 0, totalStoppedActivities = 0, totalFailedActivities = 0, totalOtherActivities = 0;
            int totalActions = 0, totalPassedActions = 0, totalFailedActions = 0, totalStoppedActions = 0, totalOtherActions = 0;
            foreach (LiteDbRunner liteDbRunner in liteDbRunSet.RunnersColl)
            {
                totalBFs += liteDbRunner.BusinessFlowsColl.Count;
                totalPassedBFs += liteDbRunner.BusinessFlowsColl.Where(bf => bf.RunStatus == eRunStatus.Passed.ToString()).Count();
                totalFailedBFs += liteDbRunner.BusinessFlowsColl.Where(bf => bf.RunStatus == eRunStatus.Failed.ToString()).Count();
                totalStoppedBFs += liteDbRunner.BusinessFlowsColl.Where(bf => bf.RunStatus == eRunStatus.Stopped.ToString()).Count();
                foreach (LiteDbBusinessFlow liteDbBusinessFlow in liteDbRunner.BusinessFlowsColl)
                {
                    totalActivities += liteDbBusinessFlow.ActivitiesColl.Count;
                    totalPassedActivities += liteDbBusinessFlow.ActivitiesColl.Where(ac => ac.RunStatus == eRunStatus.Passed.ToString()).Count();
                    totalFailedActivities += liteDbBusinessFlow.ActivitiesColl.Where(ac => ac.RunStatus == eRunStatus.Failed.ToString()).Count();
                    totalStoppedActivities += liteDbBusinessFlow.ActivitiesColl.Where(ac => ac.RunStatus == eRunStatus.Stopped.ToString()).Count();
                    foreach (LiteDbActivity liteDbActivity in liteDbBusinessFlow.ActivitiesColl)
                    {
                        totalActions += liteDbActivity.ActionsColl.Count;
                        totalPassedActions += liteDbActivity.ActionsColl.Where(ac => ac.RunStatus == eRunStatus.Passed.ToString()).Count();
                        totalFailedActions += liteDbActivity.ActionsColl.Where(ac => ac.RunStatus == eRunStatus.Failed.ToString()).Count();
                        totalStoppedActions += liteDbActivity.ActionsColl.Where(ac => ac.RunStatus == eRunStatus.Stopped.ToString()).Count();
                    }
                }
            }
            totalOtherBFs = totalBFs - (totalPassedBFs + totalFailedBFs + totalStoppedBFs);
            totalOtherActivities = totalActivities - (totalPassedActivities + totalFailedActivities + totalStoppedActivities);
            totalOtherActions = totalActions - (totalPassedActions + totalFailedActions + totalStoppedActions);

            // Business Flows Place Holders                        
            chartData = new List<KeyValuePair<int, int>>();
            chartData.Add(new KeyValuePair<int, int>(totalPassedBFs, 0));
            chartData.Add(new KeyValuePair<int, int>(totalFailedBFs, 1));
            chartData.Add(new KeyValuePair<int, int>(totalStoppedBFs, 2));
            chartData.Add(new KeyValuePair<int, int>(totalOtherBFs, 3));
            CreateChart(chartData, "Businessflow" + reportTimeStamp + ".jpeg", GingerDicser.GetTermResValue(eTermResKey.BusinessFlows));

            // Activities Place Holders                        
            chartData = new List<KeyValuePair<int, int>>();
            chartData.Add(new KeyValuePair<int, int>(totalPassedActivities, 0));
            chartData.Add(new KeyValuePair<int, int>(totalFailedActivities, 1));
            chartData.Add(new KeyValuePair<int, int>(totalStoppedActivities, 2));
            chartData.Add(new KeyValuePair<int, int>(totalOtherActivities, 3));
            CreateChart(chartData, "Activity" + reportTimeStamp + ".jpeg", GingerDicser.GetTermResValue(eTermResKey.Activities));

            // Actions Place Holders                        
            chartData = new List<KeyValuePair<int, int>>();
            chartData.Add(new KeyValuePair<int, int>(totalPassedActions, 0));
            chartData.Add(new KeyValuePair<int, int>(totalFailedActions, 1));
            chartData.Add(new KeyValuePair<int, int>(totalStoppedActions, 2));
            chartData.Add(new KeyValuePair<int, int>(totalOtherActions, 3));
            CreateChart(chartData, "Action" + reportTimeStamp + ".jpeg", "Actions");
            return chartData;
        }

        private static void GetExecGeneralDetails(LiteDbRunSet liteDbRunSet, HTMLReportConfiguration currentTemplate, StringBuilder fieldsNamesHTMLTableCellsRowOne, StringBuilder fieldsValuesHTMLTableCellsRowOne, StringBuilder fieldsNamesHTMLTableCellsRowTwo, StringBuilder fieldsValuesHTMLTableCellsRowTwo)
        {
            string tableColor = "<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>";
            string tableStyle = @"<td style='padding: 10px; border: 1px solid #dddddd'>";

            var totalColumnCount = currentTemplate.EmailSummaryViewFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())).Count(); ;
            var columnHeaderName = fieldsNamesHTMLTableCellsRowOne;
            var columnValue = fieldsValuesHTMLTableCellsRowOne;
            var columnCount = 0;
            foreach (HTMLReportConfigFieldToSelect selectedField in currentTemplate.EmailSummaryViewFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
            {
                // change row from one to two
                if (totalColumnCount > 6 && ++columnCount > 6)
                {
                    columnHeaderName = fieldsNamesHTMLTableCellsRowTwo;
                    columnValue = fieldsValuesHTMLTableCellsRowTwo;
                }
                string fieldName = EmailToObjectFieldName(selectedField.FieldKey);
                columnHeaderName.Append(string.Concat(tableColor, selectedField.FieldName, "</td>"));

                if (liteDbRunSet.GetType().GetProperty(fieldName).GetValue(liteDbRunSet) == null)
                {
                    columnValue.Append(string.Concat(tableStyle, " N/A </td>"));
                }
                else
                {
                    var columnDbValue = ExtensionMethods.OverrideHTMLRelatedCharacters(Convert.ToString(liteDbRunSet.GetType().GetProperty(fieldName).GetValue(liteDbRunSet)));

                    if (selectedField.FieldKey == RunSetReport.Fields.EnvironmentsDetails)
                    {
                        StringBuilder environmentNames_str = new StringBuilder();
                        liteDbRunSet.RunnersColl.Where(x => x.Environment != null && x.Environment != string.Empty).GroupBy(q => q.Environment).Select(q => q.First()).ToList().ForEach(x => environmentNames_str.Append(x.Environment + ", "));

                        columnValue.Append(string.Concat(tableStyle, environmentNames_str.ToString().TrimEnd(',', ' '), "</td>"));
                        environmentNames_str.Remove(0, environmentNames_str.Length);
                    }
                    else if (selectedField.FieldKey == RunSetReport.Fields.ExecutionDuration)
                    {
                        columnValue.Append(string.Concat(tableStyle, General.TimeConvert(columnDbValue), "</td>"));
                    }
                    else if (selectedField.FieldKey == RunSetReport.Fields.RunSetExecutionRate || selectedField.FieldKey == RunSetReport.Fields.GingerRunnersPassRate)
                    {
                        columnValue.Append(string.Concat(tableStyle, columnDbValue, '%', "</td>"));
                    }
                    else
                    {
                        columnValue.Append(string.Concat(tableStyle, columnDbValue, "</td>"));
                    }
                }
            }
        }

        private static string EmailToObjectFieldName(string fieldName)
        {
            Dictionary<string, string> adjustFieldNameDic = new Dictionary<string, string> { { RunSetReport.Fields.EnvironmentsDetails, nameof(LiteDbReportBase.Environment) },  { GingerReport.Fields.EnvironmentName, nameof(LiteDbReportBase.Environment) },
                { RunSetReport.Fields.ExecutionDuration, nameof(LiteDbReportBase.Elapsed) },{ActivityReport.Fields.ElapsedSecs, nameof(LiteDbReportBase.Elapsed) },{RunSetReport.Fields.RunSetExecutionRate, nameof(LiteDbReportBase.ExecutionRate) } ,
                {RunSetReport.Fields.GingerRunnersPassRate, nameof(LiteDbReportBase.PassRate) },{BusinessFlowReport.Fields.PassPercent, nameof(LiteDbReportBase.PassRate) }, {GingerReport.Fields.ExecutionDescription, nameof(LiteDbReportBase.RunDescription) },{ ActivityReport.Fields.ActivityName, nameof(LiteDbReportBase.Name) } };
            return adjustFieldNameDic.Keys.Contains(fieldName) ? adjustFieldNameDic[fieldName] : fieldName;
        }

        private void SetReportInfoFromLiteDb(ReportInfo reportInfoLiteDb, LiteDbRunSet liteDbRunSet)
        {
            reportInfoLiteDb.reportInfoLevel = ReportInfo.ReportInfoLevel.GingerLevel;
        }

        public void CreateSummaryViewReportForEmailAction(ReportInfo RI)
        {
            reportTimeStamp = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss_fff");
            HTMLReportConfiguration currentTemplate = new HTMLReportConfiguration();
            ObservableList<HTMLReportConfiguration> HTMLReportConfigurations = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>();
            currentTemplate = HTMLReportConfigurations.Where(x => (x.ID == RunSetActionHTMLReportSendEmail.selectedHTMLReportTemplateID)).FirstOrDefault();
            if (currentTemplate == null && RunSetActionHTMLReportSendEmail.selectedHTMLReportTemplateID == 100)// for supporting dynamic runset report
            {
                currentTemplate = HTMLReportConfigurations.Where(x => (x.IsDefault == true)).FirstOrDefault();
            }
            TargetFrameworkHelper.Helper.CreateCustomerLogo(currentTemplate, tempFolder);
            //System.Drawing.Image CustomerLogo = Ginger.General.Base64StringToImage(currentTemplate.LogoBase64Image.ToString());
            //CustomerLogo.Save(tempFolder + "/CustomerLogo.png");
            if (currentTemplate == null)
            {
                currentTemplate = HTMLReportConfigurations.Where(x => (x.IsDefault == true)).FirstOrDefault();
            }
            //Ginger.Reports.HTMLReportTemplatePage.EnchancingLoadedFieldsWithDataAndValidating(currentTemplate);
            if ((RI.ReportInfoRootObject == null) || (RI.ReportInfoRootObject.GetType() == typeof(Object)))
            {
                return;
            }

            string ReportHTML;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ReportHTML = Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetHTMLTemplate("EmailExecutionReport.html", TemplatesFolder);
            }
            else
            {
                ReportHTML = Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetHTMLTemplate("EmailExecutionReportOnLinux.html", TemplatesFolder);
            }

            List<KeyValuePair<int, int>> chartData = null;
            StringBuilder fieldsNamesHTMLTableCells = new StringBuilder();
            StringBuilder fieldsValuesHTMLTableCells = new StringBuilder();
            StringBuilder fieldsNamesHTMLTableCellsm = new StringBuilder();
            StringBuilder fieldsValuesHTMLTableCellsm = new StringBuilder();
            foreach (HTMLReportConfigFieldToSelect selectedField in currentTemplate.EmailSummaryViewFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
            {
                if (currentTemplate.EmailSummaryViewFieldsToSelect.IndexOf(selectedField) <= 6)
                {
                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField.FieldName + "</td>");

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
                            fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + ExtensionMethods.OverrideHTMLRelatedCharacters(General.TimeConvert(((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((RunSetReport)RI.ReportInfoRootObject)).ToString())) + "</td>");
                        }
                        else if ((selectedField.FieldKey == RunSetReport.Fields.StartTimeStamp) || (selectedField.FieldKey == RunSetReport.Fields.EndTimeStamp))
                        {
                            fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd' >" + DateTime.Parse(((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((RunSetReport)RI.ReportInfoRootObject)).ToString()).ToLocalTime().ToString() + " </td>");
                        }
                        else if ((selectedField.FieldKey == RunSetReport.Fields.RunSetExecutionRate) || (selectedField.FieldKey == RunSetReport.Fields.GingerRunnersPassRate))
                        {
                            fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + ExtensionMethods.OverrideHTMLRelatedCharacters(((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((RunSetReport)RI.ReportInfoRootObject)).ToString() + '%') + "</td>");
                        }
                        else
                        {
                            fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((RunSetReport)RI.ReportInfoRootObject)).ToString()) + "</td>");
                        }
                    }
                }
                else
                {
                    fieldsNamesHTMLTableCellsm.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField.FieldName + "</td>");
                    if (selectedField.FieldKey == RunSetReport.Fields.EnvironmentsDetails)
                    {
                        StringBuilder environmentNames_str2 = new StringBuilder();
                        ((RunSetReport)RI.ReportInfoRootObject).GingerReports.Where(x => x.EnvironmentName != null && x.EnvironmentName != string.Empty).GroupBy(q => q.EnvironmentName).Select(q => q.First()).ToList().ForEach(x => environmentNames_str2.Append(x.EnvironmentName + ", "));
                        fieldsValuesHTMLTableCellsm.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + environmentNames_str2.ToString().TrimEnd(',', ' ') + "</td>");
                        environmentNames_str2.Remove(0, environmentNames_str2.Length);
                    }
                    else
                    {
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
                        if (!string.IsNullOrEmpty(executionStatisticsSection))
                        {
                            ReportHTML = ReportHTML.Replace(executionStatisticsSection, "");
                        }

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
                        CreateChart(chartData, "Businessflow" + reportTimeStamp + ".jpeg", GingerDicser.GetTermResValue(eTermResKey.BusinessFlows));

                        List<BusinessFlowReport> bfTotalList = new List<BusinessFlowReport>();
                        ((RunSetReport)RI.ReportInfoRootObject).GingerReports.ForEach(x => x.BusinessFlowReports.ForEach(y => bfTotalList.Add(y)));
                        // Activities Place Holders                        
                        chartData = new List<KeyValuePair<int, int>>();
                        chartData.Add(new KeyValuePair<int, int>(bfTotalList.Select(x => x.TotalActivitiesPassed).ToList().Sum(), 0));
                        chartData.Add(new KeyValuePair<int, int>(bfTotalList.Select(x => x.TotalActivitiesFailed).ToList().Sum(), 1));
                        chartData.Add(new KeyValuePair<int, int>(bfTotalList.Select(x => x.TotalActivitiesStopped).ToList().Sum(), 2));
                        chartData.Add(new KeyValuePair<int, int>(bfTotalList.Select(x => x.TotalActivitiesOther).ToList().Sum(), 3));
                        CreateChart(chartData, "Activity" + reportTimeStamp + ".jpeg", GingerDicser.GetTermResValue(eTermResKey.Activities));

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
                        if (!string.IsNullOrEmpty(executionStatisticsSection))
                        {
                            ReportHTML = ReportHTML.Replace(executionStatisticsSection, "");
                        }
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
                                            fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                        }
                                        fieldsValuesHTMLTableCells.Append(@"<td style='padding: 10px; border: 1px solid #dddddd'>" + GR.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(GR) + @"</td>");
                                    }
                                    else if (selectedField_internal.FieldKey == GingerReport.Fields.EnvironmentName)
                                    {
                                        if (firstIteration)
                                        {
                                            fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
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
                                            fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                        }
                                        fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br).ToString()) + "</td>");
                                    }
                                    if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.Name)
                                    {
                                        if (firstIteration)
                                        {
                                            fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
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
                                            fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
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
                                    else if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.RunDescription)
                                    {
                                        if (firstIteration)
                                        {
                                            fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
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
                                            fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                        }
                                        fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) != null ? General.TimeConvert(br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br).ToString()) : string.Empty) + "</td>");
                                    }
                                    else if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.RunStatus)
                                    {
                                        if (firstIteration)
                                        {
                                            fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                        }
                                        fieldsValuesHTMLTableCells.Append("<td style='padding:10px;border:1px solid #dddddd;' class='Status" + (br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br)).ToString() + "'>" + br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) + "</td>");
                                    }
                                    else if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.PassPercent)
                                    {
                                        if (firstIteration)
                                        {
                                            fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
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
                        {
                            ReportHTML = ReportHTML.Replace(executionStatisticsSection, "");
                        }
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

                    if (selectedField.IsSelected)
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
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
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
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Sequence</td>");
                                                }
                                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) + "</td>");
                                            }
                                            if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.Name)
                                            {
                                                if (firstIteration)
                                                {
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
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
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>Activity Sequence</td>");
                                                }
                                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + OverrideHTMLRelatedCharacters(ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString()) + "</td>");
                                            }
                                            if (selectedField_internal.FieldKey == ActivityReport.Fields.ActivityName)
                                            {
                                                if (firstIteration)
                                                {
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
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
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>Action Execution Sequence</td>");
                                                }
                                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + OverrideHTMLRelatedCharacters(act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString()) + "</td>");
                                            }
                                            if (selectedField_internal.FieldKey == ActionReport.Fields.Description)
                                            {
                                                if (firstIteration)
                                                {
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>Action Description</td>");
                                                }
                                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + OverrideHTMLRelatedCharacters(act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString()) + "</td>");
                                            }
                                            if (selectedField_internal.FieldKey == ActionReport.Fields.Error)
                                            {
                                                if (firstIteration)
                                                {
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                                }
                                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd; color:red;white-space:pre-wrap;white-space:-moz-pre-wrap;white-space:-pre-wrap;white-space:-o-pre-wrap;word-break: break-all;'>" + OverrideHTMLRelatedCharacters(act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString()) + "</td>");
                                            }
                                            if (selectedField_internal.FieldKey == ActionReport.Fields.ElapsedSecs)
                                            {
                                                if (firstIteration)
                                                {
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                                }
                                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + OverrideHTMLRelatedCharacters(General.TimeConvert(act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString())) + "</td>");
                                            }
                                            if ((selectedField_internal.FieldKey == ActionReport.Fields.CurrentRetryIteration) ||
                                                (selectedField_internal.FieldKey == ActionReport.Fields.ExInfo))
                                            {
                                                if (firstIteration)
                                                {
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
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
                        {
                            ReportHTML = ReportHTML.Replace(failureDetailsSection, "");
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(RunSetActionHTMLReportSendEmail.Comments))
            {
                mValueExpression.Value = RunSetActionHTMLReportSendEmail.Comments;
                ReportHTML = ReportHTML.Replace("{COMMENT}", "<img src='cid:comment'/>" + mValueExpression.ValueCalculated);
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

        private void ExecutedActivitiesDetailsGenrator(ReportInfo RI, HTMLReportConfiguration currentTemplate, ref string reportHTML, StringBuilder fieldsNamesHTMLTableCells, StringBuilder fieldsValuesHTMLTableCells)
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
                                fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                            }
                            flowReportName = Convert.ToString(GR.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(GR));
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
                                fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                            }
                            executionSeq = Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br).ToString());
                            fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + executionSeq + "</td>");
                        }
                        if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.Name)
                        {
                            if (firstActivityIteration)
                            {
                                fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
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
                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + "Activity " + selectedField_internal.FieldName + "</td>");
                                }
                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(activityReport.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(activityReport).ToString()) + "</td>");
                            }
                            if (selectedField_internal.FieldKey == ActivityReport.Fields.ActivityGroupName)
                            {
                                if (firstActivityIteration)
                                {
                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                }
                                string activityGroupNameValue = (activityReport.ActivityGroupName == null) ? "N/A" : Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(Convert.ToString(activityReport.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(activityReport)));
                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + activityGroupNameValue + "</td>");
                            }
                            if (selectedField_internal.FieldKey == ActivityReport.Fields.ActivityName)
                            {
                                if (firstActivityIteration)
                                {
                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                }
                                if (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.GingerRunnerLevel.ToString())
                                {
                                    fieldsValuesHTMLTableCells.Append(@"<td style='padding: 10px; border: 1px solid #dddddd'>" + activityReport.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(activityReport) + @"</td>");
                                }
                                else
                                {
                                    fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(Convert.ToString(activityReport.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(activityReport))) + "</td>");
                                }
                            }
                            if (selectedField_internal.FieldKey == ActivityReport.Fields.Description)
                            {
                                if (firstActivityIteration)
                                {
                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                }
                                string activityDescriptionValue = (activityReport.Description == null) ? "N/A" : Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(Convert.ToString(activityReport.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(activityReport)));
                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + activityDescriptionValue + "</td>");
                            }
                            if (selectedField_internal.FieldKey == ActivityReport.Fields.RunDescription)
                            {
                                if (firstActivityIteration)
                                {
                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                }
                                string activityRunDescriptionValue = (activityReport.RunDescription == null) ? "N/A" : Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(Convert.ToString(activityReport.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(activityReport)));
                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + activityRunDescriptionValue + "</td>");
                            }
                            if (selectedField_internal.FieldKey == ActivityReport.Fields.StartTimeStamp)
                            {
                                if (firstActivityIteration)
                                {
                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                }
                                string activityStartTimeStampValue = Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(Convert.ToString(activityReport.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(activityReport)));
                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + activityStartTimeStampValue + "</td>");
                            }
                            if (selectedField_internal.FieldKey == ActivityReport.Fields.EndTimeStamp)
                            {
                                if (firstActivityIteration)
                                {
                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                }
                                string activityEndTimeStampValue = Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(Convert.ToString(activityReport.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(activityReport)));
                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + activityEndTimeStampValue + "</td>");
                            }
                            if (selectedField_internal.FieldKey == ActivityReport.Fields.ElapsedSecs)
                            {
                                if (firstActivityIteration)
                                {
                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                }
                                string activityElapsedSecsValue = Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(Convert.ToString(activityReport.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(activityReport)));
                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + General.TimeConvert(activityElapsedSecsValue) + "</td>");
                            }
                            if (selectedField_internal.FieldKey == ActivityReport.Fields.RunStatus)
                            {
                                if (firstActivityIteration)
                                {
                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                }
                                string activityRunStatusValue = Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(Convert.ToString(activityReport.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(activityReport)));
                                fieldsValuesHTMLTableCells.Append("<td class='Status" + activityRunStatusValue + "' style='padding: 10px; border: 1px solid #dddddd'>" + activityRunStatusValue + "</td>");
                            }
                            if (selectedField_internal.FieldKey == ActivityReport.Fields.NumberOfActions)
                            {
                                if (firstActivityIteration)
                                {
                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                }
                                string activityNumberOfActionsValue = Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(Convert.ToString(activityReport.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(activityReport)));
                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + activityNumberOfActionsValue + "</td>");
                            }
                            if (selectedField_internal.FieldKey == ActivityReport.Fields.ActionsDetails)
                            {
                                if (firstActivityIteration)
                                {
                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#7f7989' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                }
                                string activityActionsDetailsValue = Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(Convert.ToString(activityReport.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(activityReport)));
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
            TargetFrameworkHelper.Helper.CreateChart(y, chartName, Title, tempFolder);
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
            byte[] arr = new byte[0];
            if (!File.Exists(path))
            {
                return null;
            }
            try
            {
                System.Drawing.Image img = System.Drawing.Image.FromFile(path);
                using (MemoryStream ms = new MemoryStream())
                {
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    arr = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, "Error in GetImageStream", ex);
            }
            return arr;
        }
        //TODO: Move the Zipit function to Email.Addattach function
        void AddAttachmentToEmail(Email e, string FileName, bool ZipIt, EmailAttachment.eAttachmentType AttachmentType)
        {
            lock (thisObj)
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
                        if (!Directory.Exists(tempFolder))
                        {
                            Directory.CreateDirectory(tempFolder);

                        }
                        if (File.Exists(Path.Combine(tempFolder, ZipFileName)))
                        {
                            File.Delete(Path.Combine(tempFolder, ZipFileName));
                        }
                        ZipFile.CreateFromDirectory(FileName, Path.Combine(tempFolder, ZipFileName));
                    }
                    catch (Exception ex)
                    {
                        ZipFileName = Path.GetFileNameWithoutExtension(FileName) + DateTime.Now.ToString("MMddyyyy_HHmmssfff") + ".zip";
                        ZipFile.CreateFromDirectory(FileName, Path.Combine(tempFolder, ZipFileName));
                        Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                    }
                    e.Attachments.Add(Path.Combine(tempFolder, ZipFileName));
                }
                else
                {
                    e.Attachments.Add(FileName);
                }
            }
        }
        public long CalculateAttachmentsSize(Email email)
        {
            long size = 0;
            foreach (string s in email.Attachments)
            {
                FileInfo f = new FileInfo(s);
                size += f.Length;
            }
            return size;
        }

        public static string OverrideHTMLRelatedCharacters(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                text = text.Replace(@"<", "&#60;");
                text = text.Replace(@">", "&#62;");
                text = text.Replace(@"$", "&#36;");
                text = text.Replace(@"%", "&#37;");
                return text;
            }
            else
            {
                return string.Empty;
            }
        }

    }
}


