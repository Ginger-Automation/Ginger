
using Amdocs.Ginger.Repository;
using Ginger.Run.RunSetActions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ginger.Reports;
using Amdocs.Ginger.Common;


namespace Ginger.Run.RunSetActions
{
    public class RunSetActionGenerateTestNGReport : RunSetActionBase
    {
        public new static class Fields
        {
            public static string SaveResultstoFolderName = "SaveResultstoFolderName";
            public static string OpenExecutionResultsFolder = "OpenExecutionResultsFolder";
            public static string SaveResultsInSolutionFolderName = "SaveResultsInSolutionFolderName";
            public static string SaveindividualBFReport = "SaveindividualBFReport";
            public static string IsStatusByActivitiesGroup = "IsStatusByActivitiesGroup";
            public static string IsStatusByActivity = "IsStatusByActivity";
        }

        public override List<RunSetActionBase.eRunAt> GetRunOptions()
        {
            List<RunSetActionBase.eRunAt> list = new List<RunSetActionBase.eRunAt>();
            list.Add(RunSetActionBase.eRunAt.ExecutionEnd);
            return list;
        }

        public override bool SupportRunOnConfig
        {
            get { return true; }
        }

        [IsSerializedForLocalRepository]
        public string SaveResultsInSolutionFolderName { get; set; }

        [IsSerializedForLocalRepository]
        public string SaveResultstoFolderName { get; set; }

        [IsSerializedForLocalRepository]
        public bool OpenExecutionResultsFolder { get; set; }

        [IsSerializedForLocalRepository]
        public bool SaveindividualBFReport { get; set; }

        [IsSerializedForLocalRepository]
        public bool IsStatusByActivitiesGroup { get; set; }

        private bool isStatusByActivity = true;
        [IsSerializedForLocalRepository]
        public bool IsStatusByActivity
        {
            get { return isStatusByActivity; }
            set { isStatusByActivity = value; }
        }

        public override void Execute(ReportInfo RI)
        {
            try
            {
                if (!string.IsNullOrEmpty(SaveResultsInSolutionFolderName))
                {
                    Reporter.ToStatus(eStatusMsgKey.SaveItem, null, SaveResultsInSolutionFolderName, "Execution Summary");
                    if (!Directory.Exists(SaveResultsInSolutionFolderName))
                    {
                        Directory.CreateDirectory(SaveResultsInSolutionFolderName);
                    }
                    SaveBFResults(RI, SaveResultsInSolutionFolderName, IsStatusByActivitiesGroup);
                    Reporter.HideStatusMessage();
                }
                else
                {
                    Errors = "Folder path not provided.";
                    Status = eRunSetActionStatus.Failed;
                }
            }
            catch (Exception ex)
            {
                Errors = ex.Message.ToString();
                Status = eRunSetActionStatus.Failed;
            }
        }

        //TODO: move to Run SetAction
        private void SaveBFResults(ReportInfo RI, string folder, bool statusByGroupActivity)
        {
            TestNGResultReport TNGReport = new TestNGResultReport();
            string xml = TNGReport.CreateReport(RI, statusByGroupActivity);

            System.IO.File.WriteAllLines(folder + @"\testng-results.xml", new string[] { xml });
        //TODO: let the user select file prefix
        }

        public override string GetEditPage()
        {
            //RunSetActionGenerateTestNGReportEditPage p = new RunSetActionGenerateTestNGReportEditPage(this);
            return "RunSetActionGenerateTestNGReportEditPage";
        }

        public override void PrepareDuringExecAction(ObservableList<GingerRunner> Gingers)
        {
            throw new NotImplementedException();
        }

        public override string Type { get { return "Generate TestNG Report"; } }
    }
}
