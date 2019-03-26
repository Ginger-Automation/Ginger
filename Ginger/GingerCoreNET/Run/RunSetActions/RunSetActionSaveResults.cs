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
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.CoreNET.Execution;


using Amdocs.Ginger.Repository;
using Ginger.Reports;
using GingerCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

        ReportInfo mRI;
        public override void Execute(ReportInfo RI)
        {
            mRI = RI;
            try
            {
                if (!string.IsNullOrEmpty(SaveResultsInSolutionFolderName))
                {

                    Reporter.ToStatus(eStatusMsgKey.SaveItem, null, SaveResultsInSolutionFolderName, "Execution Summary");
                    string folder = Path.Combine(WorkSpace.Instance.Solution.Folder, @"ExecutionResults");
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

                    Reporter.HideStatusMessage();
                }

                if (!string.IsNullOrEmpty(SaveResultstoFolderName))
                {
                    Reporter.ToStatus(eStatusMsgKey.SaveItem, null, SaveResultstoFolderName, "Execution Summary");
                    SaveBFResults(RI, SaveResultstoFolderName);
                    Reporter.HideStatusMessage();
                }

                //****----- condition  to check each business flow  ***////
                if (!string.IsNullOrEmpty(SaveResultsInSolutionFolderName) || !string.IsNullOrEmpty(SaveResultstoFolderName))
                {
                    if (SaveindividualBFReport)
                    {
                        ObservableList<BusinessFlow> BFs = new ObservableList<BusinessFlow>();

                        foreach (GingerRunner GR in WorkSpace.Instance.RunsetExecutor.Runners)
                        {
                            foreach (BusinessFlow bf in GR.BusinessFlows)
                            {
                                if (bf.Active == true)
                                {
                                    if (bf.RunStatus.ToString() == nameof(eRunStatus.Passed) || bf.RunStatus.ToString() == nameof(eRunStatus.Failed) || bf.RunStatus.ToString() == nameof(eRunStatus.Stopped))
                                    {
                                        // !!!!!!!!!!!!!!!!!!!
                                        ReportInfo BFRI = new ReportInfo(mRI.Environment.ProjEnvironment, bf);

                                        string TempRepFileName = RepositoryItemHelper.RepositoryItemFactory.GenerateTemplate(TemplateName, BFRI);
                                        String RepFileName = DateTime.Now.ToString("dMMMyyyy_HHmmss_fff") + "_" + WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name + "_" + GR.Name + "_" + bf.Name + "_" + WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment.Name;

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
                    Reporter.HideStatusMessage();
                    Status = RunSetActionBase.eRunSetActionStatus.Failed;
                }
            }
            catch (Exception ex)
            {
                Errors = "Failed: " + ex.Message;
                Reporter.HideStatusMessage();
                Status = RunSetActionBase.eRunSetActionStatus.Failed;
            }
        }
        //TODO: move to Run SetAction
        private void SaveBFResults(ReportInfo RI, string folder)
        {
            string repFileName = null;// IReportTemplate.GenerateReport(TemplateName, RI);
            string FName = Path.GetFileName(repFileName);

            //TODO: let the user select file prefix
            string TargetFileName = Path.Combine(folder, FName);

            File.Copy(repFileName, TargetFileName);

            if (OpenExecutionResultsFolder)
            {
                System.Diagnostics.Process.Start(folder);
            }
        }

        public override string GetEditPage()
        {
            //RunSetActionSaveResultsEditPage p = new RunSetActionSaveResultsEditPage(this);
            return "RunSetActionSaveResultsEditPage";
        }


        public override void PrepareDuringExecAction(ObservableList<GingerRunner> Gingers)
        {
            throw new NotImplementedException();
        }



        public override string Type { get { return "Save Results"; } }
    }
}
