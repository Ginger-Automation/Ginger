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
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.CoreNET.Execution;


using Amdocs.Ginger.Repository;
using Ginger.Reports;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Ginger.Run.RunSetActions
{
    public class RunSetActionSaveResultsOperations : IRunSetActionSaveResultsOperations
    {
        public RunSetActionSaveResults RunSetActionSaveResults;
        public RunSetActionSaveResultsOperations(RunSetActionSaveResults RunSetActionSaveResults)
        {
            this.RunSetActionSaveResults = RunSetActionSaveResults;
            this.RunSetActionSaveResults.RunSetActionSaveResultsOperations = this;
        }
        public void Execute(IReportInfo RI)
        {
            ReportInfo mRI = (ReportInfo)RI;
            try
            {
                if (!string.IsNullOrEmpty(RunSetActionSaveResults.SaveResultsInSolutionFolderName))
                {
                    Reporter.ToStatus(eStatusMsgKey.SaveItem, null, RunSetActionSaveResults.SaveResultsInSolutionFolderName, "Execution Summary");
                    string folder = Path.Combine(WorkSpace.Instance.TestArtifactsFolder, RunSetActionSaveResults.SaveResultsInSolutionFolderName);
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                    SaveBFResults(mRI, folder);
                    Reporter.HideStatusMessage();
                }

                if (!string.IsNullOrEmpty(RunSetActionSaveResults.SaveResultstoFolderName))
                {
                    Reporter.ToStatus(eStatusMsgKey.SaveItem, null, RunSetActionSaveResults.SaveResultstoFolderName, "Execution Summary");
                    SaveBFResults(mRI, RunSetActionSaveResults.SaveResultstoFolderName);
                    Reporter.HideStatusMessage();
                }

                //****----- condition  to check each business flow  ***////
                if (!string.IsNullOrEmpty(RunSetActionSaveResults.SaveResultsInSolutionFolderName) || !string.IsNullOrEmpty(RunSetActionSaveResults.SaveResultstoFolderName))
                {
                    if (RunSetActionSaveResults.SaveindividualBFReport)
                    {
                        //
                    }
                }
                else
                {
                    RunSetActionSaveResults.Errors = "Folder path not provided";
                    Reporter.HideStatusMessage();
                    RunSetActionSaveResults.Status = RunSetActionBase.eRunSetActionStatus.Failed;
                }
            }
            catch (Exception ex)
            {
                RunSetActionSaveResults.Errors = "Failed: " + ex.Message;
                Reporter.HideStatusMessage();
                RunSetActionSaveResults.Status = RunSetActionBase.eRunSetActionStatus.Failed;
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

            if (RunSetActionSaveResults.OpenExecutionResultsFolder)
            {
                Process.Start(new ProcessStartInfo() { FileName = folder, UseShellExecute = true });
            }
        }


    }
}
