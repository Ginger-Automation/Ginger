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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using Ginger.Reports.HTMLReports;
using GingerCore.GeneralLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Ginger.Run.RunSetActions
{
    public class RunSetActionSendEmail : RunSetActionBase
    {
        public enum eHTMLReportTemplate
        {
            [EnumValueDescription("Free Text")]
            FreeText,
            Summary,
            Detailed,
            Plain,
            Custom
        }

        public override List<RunSetActionBase.eRunAt> GetRunOptions()
        {
            List<RunSetActionBase.eRunAt> list = new List<RunSetActionBase.eRunAt>();
            list.Add(RunSetActionBase.eRunAt.ExecutionStart);
            list.Add(RunSetActionBase.eRunAt.ExecutionEnd);
            return list;
        }

        public override bool SupportRunOnConfig
        {
            get { return true; }
        }

        public new static class Fields
        {
            public static string HTMLReportTemplate = "HTMLReportTemplate";
            public static string CustomHTMLReportTemplate = "CustomHTMLReportTemplate";

        }

        [IsSerializedForLocalRepository]
        public Email Email = new Email();

        //User can attach several templates to the email
        // Attach template + RI
        // Attach its own file
        [IsSerializedForLocalRepository]
        public ObservableList<EmailAttachment> EmailAttachments = new ObservableList<EmailAttachment>();

        [IsSerializedForLocalRepository]
        public eHTMLReportTemplate HTMLReportTemplate { get; set; }
        [IsSerializedForLocalRepository]
        public string CustomHTMLReportTemplate { get; set; }

        public override void Execute(ReportInfo RI)
        {
            //Make sure we clear in case use open the edit page twice
            Email.Attachments.Clear();

            //for compatibility with old HTML report sent by email
            if (HTMLReportTemplate != RunSetActionSendEmail.eHTMLReportTemplate.FreeText)
            {
                SetBodyFromHTMLReport(RI);
            }

            if (EmailAttachments != null)
            {
                foreach (EmailAttachment r in EmailAttachments)
                {
                    //attach simple file
                    if (r.AttachmentType == EmailAttachment.eAttachmentType.File)
                    {
                        if (System.IO.File.Exists(r.Name))
                        {
                            AddAttachmentToEmail(Email, r.Name, r.ZipIt);
                        }
                        else
                        {
                            Email.Body = "ERROR: File not found: " + r.Name + Environment.NewLine + Email.Body;
                        }
                    }

                    //attach report - after generating from template
                    if (r.AttachmentType == EmailAttachment.eAttachmentType.Report)
                    {
                        string repFileName = null;// ReportTemplate.GenerateReport(r.Name, RI);
                        if (repFileName != null)
                        {
                            AddAttachmentToEmail(Email, repFileName, r.ZipIt);
                        }
                        else
                        {
                            Email.Body = "ERROR: Report Template not found: " + r + Environment.NewLine + Email.Body;
                        }
                    }
                }
            }
            Email.Send();
        }

        public void SetBodyFromHTMLReport(ReportInfo RI)
        {
            HTMLReportBase HRB = null;

            switch (HTMLReportTemplate)
            {
                case eHTMLReportTemplate.Detailed:
                    HRB = new HTMLDetailedReport();
                    break;
                case eHTMLReportTemplate.Summary:
                    HRB = new HTMLSummaryReport();
                    break;
                case eHTMLReportTemplate.Plain:
                    HRB = new HTMLPlainReport();
                    break;

                case eHTMLReportTemplate.Custom:

                    string html = String.Empty;

                    HTMLReportTemplate HTMLR = new HTMLReportTemplate();
                    ObservableList<HTMLReportTemplate> HTMLReports = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportTemplate>();
                    foreach (HTMLReportTemplate HT in HTMLReports)
                    {
                        if (HT.Name == CustomHTMLReportTemplate)
                        {
                            html = HT.HTML;
                            HTMLR = HT;
                        }
                    }

                    //HTMLReportPage HTP = new HTMLReportPage(RI, html);

                    //Email.Body = HTP.HTML;

                    return;
                    // break;
            }

            if (HRB != null)
            {
                Email.Body = HRB.CreateReport(RI);
                CustomHTMLReportTemplate = String.Empty;
            }
            else
            {
                throw new Exception("Unknown HTML Report type - " + HTMLReportTemplate.ToString());
            }
        }


        //TODO: Move the Zipit function to Email.Addattach function
        void AddAttachmentToEmail(Email e, string FileName, bool ZipIt)
        {
            if (ZipIt)
            {
                //We use this trick to get valid temp unique file name, then convert it to folder
                string tempFolder = Path.GetTempFileName();
                File.Delete(tempFolder);
                Directory.CreateDirectory(tempFolder);

                //Create sub dir to hold the file
                String SubFolder = Path.Combine(tempFolder, Path.GetFileNameWithoutExtension(FileName));
                Directory.CreateDirectory(SubFolder);

                // Copy the file to the sub folder, keep the name
                String TargetFileName = Path.Combine(SubFolder, Path.GetFileName(FileName));
                System.IO.File.Copy(FileName, TargetFileName);

                // Create target zip file name
                String ZipFileName = Path.Combine(tempFolder, Path.GetFileNameWithoutExtension(FileName) + ".zip");

                //Create the Zip file
                ZipFile.CreateFromDirectory(SubFolder, ZipFileName);

                e.Attachments.Add(ZipFileName);
            }
            else
            {
                e.Attachments.Add(FileName);
            }
        }

        public override string GetEditPage()
        {
            //RunSetActionSendEmailEditPage RSAEREP = new RunSetActionSendEmailEditPage(this);
            return "RunSetActionSendEmailEditPage";
        }

        public override void PrepareDuringExecAction(ObservableList<GingerRunner> Gingers)
        {
            throw new NotImplementedException();
        }

        public override string Type { get { return "Send Email"; } }
    }
}
