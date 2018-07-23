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

using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using Ginger.Reports;
using GingerCore;
using GingerCore.Repository;

namespace Ginger.Run.RunSetActions
{
    public class RunSetActionSaveResults : RunSetActionBase
    {
        public new static class Fields
        {
            public static string SaveResultstoFolderName = "SaveResultstoFolderName";
            public static string TemplateName = "TemplateName";
            public static string OpenExecutionResultsFolder = "OpenExecutionResultsFolder";
            public static string SaveResultsInSolutionFolderName = "SaveResultsInSolutionFolderName";
            public static string SaveindividualBFReport = "SaveindividualBFReport";
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

        [IsSerializedForLocalRepository]
        public string SaveResultsInSolutionFolderName { get; set; }

        [IsSerializedForLocalRepository]
        public string SaveResultstoFolderName { get; set; }

        [IsSerializedForLocalRepository]
        public string TemplateName { get; set; }

        [IsSerializedForLocalRepository]
        public bool OpenExecutionResultsFolder { get; set; }

        [IsSerializedForLocalRepository]
        public bool SaveindividualBFReport { get; set; }
        
        public override void Execute(ReportInfo RI)
        {
            try
            {
                if (!string.IsNullOrEmpty(SaveResultsInSolutionFolderName))
                {

                    Reporter.ToGingerHelper(eGingerHelperMsgKey.SaveItem, null, SaveResultsInSolutionFolderName, "Execution Summary");
                    string folder = Path.Combine(App.UserProfile.Solution.Folder, @"ExecutionResults");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                    folder = Path.Combine(folder, SaveResultsInSolutionFolderName);
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                    SaveBFResults(RI, folder);

                    Reporter.CloseGingerHelper();
                }

                if (!string.IsNullOrEmpty(SaveResultstoFolderName))
                {
                    Reporter.ToGingerHelper(eGingerHelperMsgKey.SaveItem, null, SaveResultstoFolderName, "Execution Summary");
                    SaveBFResults(RI, SaveResultstoFolderName);
                    Reporter.CloseGingerHelper();
                }

                //****----- condition  to check each business flow  ***////
                if (!string.IsNullOrEmpty(SaveResultsInSolutionFolderName) || !string.IsNullOrEmpty(SaveResultstoFolderName))
                {
                    if (SaveindividualBFReport)
                    {
                        ObservableList<BusinessFlow> BFs = new ObservableList<BusinessFlow>();

                        foreach (GingerRunner GR in App.RunsetExecutor.Runners)
                        {
                            foreach (BusinessFlow bf in GR.BusinessFlows)
                            {
                                if (bf.Active == true)
                                {
                                    if (bf.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed || bf.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed || bf.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped)
                                    {
                                        ReportInfo BFRI = new ReportInfo(App.AutomateTabGingerRunner.ProjEnvironment, bf);

                                        string TempRepFileName = ReportTemplate.GenerateReport(TemplateName, BFRI);
                                        String RepFileName = DateTime.Now.ToString("dMMMyyyy_HHmmss_fff") + "_" + App.RunsetExecutor.RunSetConfig.Name + "_" + GR.Name + "_" + bf.Name + "_" + App.RunsetExecutor.RunsetExecutionEnvironment.Name;

                                        while (RepFileName.Length > 250)
                                        {
                                            RepFileName = RepFileName.Substring(0, 250);
                                        }

                                        RepFileName = RepFileName + ".pdf";

                                        if (!string.IsNullOrEmpty(SaveResultsInSolutionFolderName))
                                        {
                                            System.IO.File.Copy(TempRepFileName, SaveResultsInSolutionFolderName + "\\" + RepFileName);
                                        }

                                        if (SaveResultsInSolutionFolderName != SaveResultstoFolderName)
                                        {
                                            if (!string.IsNullOrEmpty(SaveResultstoFolderName))
                                            {
                                                System.IO.File.Copy(TempRepFileName, SaveResultstoFolderName + "\\" + RepFileName);
                                            }

                                        }
                                        if (System.IO.File.Exists(Path.GetTempPath() + RepFileName))
                                            System.IO.File.Delete(Path.GetTempPath() + RepFileName);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Errors = "Folder path not provided";
                    Reporter.CloseGingerHelper();
                    Status = Ginger.Run.RunSetActions.RunSetActionBase.eRunSetActionStatus.Failed;
                }
            }
            catch(Exception ex)
            {
                Errors = "Failed: " + ex.Message;
                Reporter.CloseGingerHelper();
                Status = Ginger.Run.RunSetActions.RunSetActionBase.eRunSetActionStatus.Failed;
            }
        }
        //TODO: move to Run SetAction
        private void SaveBFResults(ReportInfo RI, string folder)
        {
            string repFileName = ReportTemplate.GenerateReport(TemplateName, RI);
            string FName= Path.GetFileName(repFileName);
            
            //TODO: let the user select file prefix
          string TargetFileName = Path.Combine(folder,FName);
            
            File.Copy(repFileName, TargetFileName);
            
            if (OpenExecutionResultsFolder)
            {
                System.Diagnostics.Process.Start(folder);
            }
        }

        public override Page GetEditPage()
        {
            RunSetActionSaveResultsEditPage p = new RunSetActionSaveResultsEditPage(this);
            return p;
        }

        public override void PrepareDuringExecAction(ObservableList<GingerRunner> Gingers)
        {
            throw new NotImplementedException();
        }

        public override string Type { get { return "Save Results"; } }
    }
}
