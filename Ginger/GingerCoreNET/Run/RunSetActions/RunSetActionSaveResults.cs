//using Amdocs.Ginger.Common;
//using Amdocs.Ginger.CoreNET.Run.RunsetActions;
//using Amdocs.Ginger.Repository;
//using GingerCoreNET.ReporterLib;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;

//namespace Amdocs.Ginger.CoreNET.Run.RunSetActions
//{
//    public class RunSetActionSaveResults : RunSetActionBase
//    {
//        public new static class Fields
//        {
//            public static string SaveResultstoFolderName = "SaveResultstoFolderName";
//            public static string TemplateName = "TemplateName";
//            public static string OpenExecutionResultsFolder = "OpenExecutionResultsFolder";
//            public static string SaveResultsInSolutionFolderName = "SaveResultsInSolutionFolderName";
//            public static string SaveindividualBFReport = "SaveindividualBFReport";
//        }

//        public override List<RunSetActionBase.eRunAt> GetRunOptions()
//        {
//            List<RunSetActionBase.eRunAt> list = new List<RunSetActionBase.eRunAt>();
//            list.Add(RunSetActionBase.eRunAt.ExecutionStart);
//            list.Add(RunSetActionBase.eRunAt.ExecutionEnd);
//            return list;
//        }

//        public override bool SupportRunOnConfig
//        {
//            get { return true; }
//        }

//        [IsSerializedForLocalRepository]
//        public string SaveResultsInSolutionFolderName { get; set; }

//        [IsSerializedForLocalRepository]
//        public string SaveResultstoFolderName { get; set; }

//        [IsSerializedForLocalRepository]
//        public string TemplateName { get; set; }

//        [IsSerializedForLocalRepository]
//        public bool OpenExecutionResultsFolder { get; set; }

//        [IsSerializedForLocalRepository]
//        public bool SaveindividualBFReport { get; set; }

//        public override void Execute(ReportInfo RI)
//        {
//            try
//            {
//                if (!string.IsNullOrEmpty(SaveResultsInSolutionFolderName))
//                {

//                    Reporter.ToGingerHelper(eGingerHelperMsgKey.SaveItem, null, SaveResultsInSolutionFolderName, "Execution Summary");
//                    string folder = Path.Combine(App.UserProfile.Solution.Folder, @"ExecutionResults");
//                    if (!Directory.Exists(folder))
//                    {
//                        Directory.CreateDirectory(folder);
//                    }
//                    folder = Path.Combine(folder, SaveResultsInSolutionFolderName);
//                    if (!Directory.Exists(folder))
//                    {
//                        Directory.CreateDirectory(folder);
//                    }
//                    SaveBFResults(RI, folder);

//                    Reporter.CloseGingerHelper();
//                }

//                if (!string.IsNullOrEmpty(SaveResultstoFolderName))
//                {
//                    Reporter.ToGingerHelper(eGingerHelperMsgKey.SaveItem, null, SaveResultstoFolderName, "Execution Summary");
//                    SaveBFResults(RI, SaveResultstoFolderName);
//                    Reporter.CloseGingerHelper();
//                }

//                //****----- condition  to check each business flow  ***////
//                if (!string.IsNullOrEmpty(SaveResultsInSolutionFolderName) || !string.IsNullOrEmpty(SaveResultstoFolderName))
//                {
//                    if (SaveindividualBFReport)
//                    {
//                        ObservableList<IBusinessFlow> BFs = new ObservableList<IBusinessFlow>();

//                        foreach (GingerRunner GR in App.RunsetExecutor.Runners)
//                        {
//                            foreach (IBusinessFlow bf in GR.BusinessFlows)
//                            {
//                                if (bf.Active == true)
//                                {
//                                    if (bf.RunStatus == nameof (Execution.eRunStatus.Passed) || bf.RunStatus == nameof(Execution.eRunStatus.Failed) || bf.RunStatus == nameof(Execution.eRunStatus.Stopped))
//                                    {
//                                        ReportInfo BFRI = new ReportInfo(App.AutomateTabGingerRunner.ProjEnvironment, bf);

//                                        string TempRepFileName = ReportTemplate.GenerateReport(TemplateName, BFRI);
//                                        String RepFileName = DateTime.Now.ToString("dMMMyyyy_HHmmss_fff") + "_" + App.RunsetExecutor.RunSetConfig.Name + "_" + GR.Name + "_" + bf.Name + "_" + App.RunsetExecutor.RunsetExecutionEnvironment.Name;

//                                        while (RepFileName.Length > 250)
//                                        {
//                                            RepFileName = RepFileName.Substring(0, 250);
//                                        }

//                                        RepFileName = RepFileName + ".pdf";

//                                        if (!string.IsNullOrEmpty(SaveResultsInSolutionFolderName))
//                                        {
//                                            System.IO.File.Copy(TempRepFileName, SaveResultsInSolutionFolderName + "\\" + RepFileName);
//                                        }

//                                        if (SaveResultsInSolutionFolderName != SaveResultstoFolderName)
//                                        {
//                                            if (!string.IsNullOrEmpty(SaveResultstoFolderName))
//                                            {
//                                                System.IO.File.Copy(TempRepFileName, SaveResultstoFolderName + "\\" + RepFileName);
//                                            }

//                                        }
//                                        if (System.IO.File.Exists(Path.GetTempPath() + RepFileName))
//                                            System.IO.File.Delete(Path.GetTempPath() + RepFileName);
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }
//                else
//                {
//                    Errors = "Folder path not provided";
//                    Reporter.CloseGingerHelper();
//                    Status = RunSetActionBase.eRunSetActionStatus.Failed;
//                }
//            }
//            catch (Exception ex)
//            {
//                Errors = "Failed: " + ex.Message;
//                Reporter.CloseGingerHelper();
//                Status = RunSetActionBase.eRunSetActionStatus.Failed;
//            }
//        }
//        //TODO: move to Run SetAction
//        private void SaveBFResults(ReportInfo RI, string folder)
//        {
//            string repFileName = ReportTemplate.GenerateReport(TemplateName, RI);
//            string FName = Path.GetFileName(repFileName);

//            //TODO: let the user select file prefix
//            string TargetFileName = Path.Combine(folder, FName);

//            File.Copy(repFileName, TargetFileName);

//            if (OpenExecutionResultsFolder)
//            {
//                System.Diagnostics.Process.Start(folder);
//            }
//        }

//        public override Page GetEditPage()
//        {
//            RunSetActionSaveResultsEditPage p = new RunSetActionSaveResultsEditPage(this);
//            return p;
//        }

//        public override void PrepareDuringExecAction(ObservableList<GingerRunner> Gingers)
//        {
//            throw new NotImplementedException();
//        }


//        public override string Type { get { return "Save Results"; } }
//    }
//}
