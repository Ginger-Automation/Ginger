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

//using GingerCoreNET.ReporterLib;
//using System.Collections.Generic;

//namespace GingerCore
//{
    //public enum eStatusMsgKey
    //{
    //    RecommendNewVersion,
    //    SaveItem,
    //    ExportItem,
    //    SaveAll,
    //    ExitMode,
    //    NoDirtyItem,
    //    SaveAllSuccess,
    //    StartAgent,
    //    StartAgentFailed,
    //    StartAgents,
    //    GetLatestSolution,
    //    CreatingReport,
    //    CreateAgentTip,
    //    CreateBusinessFlowTip,
    //    ScriptImported_RefreshSolution,
    //    ALMTestSetImport, ExportItemToALM,
    //    AddingToSharedRepository,
    //    CheckInToSourceControl, GetLatestFromSourceControl, DownloadSolutionFromSourceControl,ResolveSourceControlConflicts,RevertChangesFromSourceControl,
    //    ExportExecutionDetails, UndoChanges,
    //    LoadingRunSet,
    //    RunCompleted,
    //    ExecutingRunSetAction,
    //    StartingRecorder,
    //    StoppingRecorder,
    //    ASCFTryToConnect, JavaDRiverTryToConnect,
    //    MainframeIncorrectConfiguration,
    //    ShowBetaFeatures,
    //    AnalyzerFixingIssues,AnalyzerSavingFixedIssues,AnalyzerIsAnalyzing,
    //    GingerHelpLibrary,
    //    LoadingSolution, BusinessFlowConversion
    //}

    //public static class GingerHelperMsgsPool
    //{
    //    public static void LoadGingerHelperMsgsPool()
    //    {
    //        // TODO: MERGE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

    //        //Initialize the pool
    //        Reporter.GingerHelperMsgsPool = new Dictionary<eStatusMsgKey, GingerHelperMsg>();

    //        //Add Ginger helper messages to the pool
    //        #region General Application Messages
    //        Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.RecommendNewVersion, new GingerHelperMsg(eGingerHelperMsgType.INFO, "Upgrade Required", "Newer version of Ginger exist." + System.Environment.NewLine + "You can download the latest version from https://github.com/Ginger-Automation/Ginger/releases", true, "Upgrade"));
    //        Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.SaveItem, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Saving", "Saving '{0}' {1}"));
    //        Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.ExportItem, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Exporting", "Exporting '{0}' {1}"));
    //        Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.SaveAll, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Saving", "Starting to Save solution..."));
    //        Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.ExitMode, new GingerHelperMsg(eGingerHelperMsgType.INFO, "Oops...","Ginger was not closed properly. Please turn to support team."));
    //        #endregion General Application Messages

    //        #region Solution Messages
    //        Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.GetLatestSolution, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Get Latest Solution", "Getting the latest updates of '{0}' Solution"));
    //        Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.NoDirtyItem, new GingerHelperMsg(eGingerHelperMsgType.INFO, "Save All", "No changed items found to be save."));
    //        Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.SaveAllSuccess, new GingerHelperMsg(eGingerHelperMsgType.INFO, "Save All", "Items saved successfully."));
    //        Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.LoadingSolution, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Loading Solution", "{0}"));
           
    //        #endregion Solution Messages

    //        #region Analyzer
    //        Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.AnalyzerFixingIssues, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Auto Fixing Issues", "Fixing the item '{0}'"));
    //        Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.AnalyzerSavingFixedIssues, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Saving Auto Fixed Issues", "Saving the item '{0}'"));
    //        Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.AnalyzerIsAnalyzing, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Analyzing...", "Analyzing the '{0}' {1} before execution"));
    //        #endregion Analyzer

    //        #region Agents Messages
    //        Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.StartAgent, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Starting Agent", "Starting the '{0}' Agent for the '{1}' Application." + System.Environment.NewLine + "Process can be aborted by re-clicking on Start button."));
    //        Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.StartAgents, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Starting Agent/s", "Starting the agent/s:{0}"));
    //        Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.CreateAgentTip, new GingerHelperMsg(eGingerHelperMsgType.INFO, "Tip!", "Create a new 'Agent' which match to your " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " platform to allow platform connection"));
    //        Reporter.GingerHelperMsgsPool.Add (eStatusMsgKey.MainframeIncorrectConfiguration, new GingerHelperMsg (eGingerHelperMsgType.INFO, "Mainframe Server not Available", "Mainframe server is not available on configured address and port . Please Check configuration"));
    //        Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.StartAgentFailed, new GingerHelperMsg(eGingerHelperMsgType.INFO,"Start Agent Failed", "'{0}'"));
    //        #endregion Agents Messages

    //        #region BusinessFlows Messages
    //        Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.CreateBusinessFlowTip, new GingerHelperMsg(eGingerHelperMsgType.INFO, "Tip!", "Start automating by creating a new '" + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + "' and shifting to the 'Automate' tab"));
    //        Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.BusinessFlowConversion, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Converting Actions", "Converting the Actions of the '{0}' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)));
    //        #endregion BusinessFlows Messages

    //        #region Execution Messages
    //        Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.CreatingReport, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Creating Report", "Creating report for the '{0}' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " execution"));
    //        Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.LoadingRunSet, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Loading " + GingerDicser.GetTermResValue(eTermResKey.RunSet), "Loading Ginger {0}"));
    //        #endregion Execution Messages

    //        #region Import/Export Messages
    //        Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.ALMTestSetImport, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Importing QC/ALM Test Set", "Importing the ALM Test Set: '{0}'"));
    //        Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.ExportItemToALM, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Exporting to ALM", "Exporting the item: '{0}'"));           
            
    //        #endregion Import/Export Messages
            
    //        #region BusinessFlows Messages
    //        Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.ScriptImported_RefreshSolution, new GingerHelperMsg(eGingerHelperMsgType.INFO, "Tip!", "Refresh the Solution to view the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " created from UFT Script"));
    //        #endregion BusinessFlows Messages

    //        #region Shared Repository Messages
    //        Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.AddingToSharedRepository, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Adding Item to Repository", "Adding the item: '{0}' to the shared repository "));
    //        Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.ExportExecutionDetails, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Export Execution Details", "Export execution details of the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ": '{0}' to {1}."));            
    //        #endregion BusinessFlows Messages

    //        #region Source Control            
    //         Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.CheckInToSourceControl, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Check-In to Source Control", "Current Operation: {0}"));
    //         Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.GetLatestFromSourceControl, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Get Latest from Source Control", "Updating local solution files to the latest version."));
    //         Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.DownloadSolutionFromSourceControl, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Download Solution", "Downloading the solution '{0}' from source control."));
    //         Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.ResolveSourceControlConflicts, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Resolve Source Control Conflicts", "Resolving local solution conflicts."));
    //         Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.RevertChangesFromSourceControl, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Revert Changes from Source Control", "Undo local changes and revert to source."));
            
    //        #endregion Source Control

    //        #region General
    //         Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.UndoChanges, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Undo Changes", "Undo changes for the item '{0}'"));
    //        #endregion General

    //         Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.RunCompleted, new GingerHelperMsg(eGingerHelperMsgType.INFO, GingerDicser.GetTermResValue(eTermResKey.RunSet), "Execution Completed '{0}'"));

    //         Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.ExecutingRunSetAction, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Executing " + GingerDicser.GetTermResValue(eTermResKey.RunSet)+" Action", "Executing '{0}'"));

    //         Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.StartingRecorder, new GingerHelperMsg(eGingerHelperMsgType.INFO, "Driver", "Starting Recorder"));

    //         Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.StoppingRecorder, new GingerHelperMsg(eGingerHelperMsgType.INFO, "Driver", "Stopping Recorder"));
    //         Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.ASCFTryToConnect, new GingerHelperMsg(eGingerHelperMsgType.INFO, "ASCF Driver", "Trying to connect #{0}"));
    //         Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.JavaDRiverTryToConnect, new GingerHelperMsg(eGingerHelperMsgType.INFO, "Java Driver", "Trying to connect #{0}"));

    //         Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.ShowBetaFeatures, new GingerHelperMsg(eGingerHelperMsgType.INFO, "Show beta Features is: ", "{0}"));

    //        Reporter.GingerHelperMsgsPool.Add(eStatusMsgKey.GingerHelpLibrary, new GingerHelperMsg(eGingerHelperMsgType.INFO, "Ginger Help Library is Ready!", "Press [F1] from anywhere and view User Guides & Videos related to the topic you working on!"));
    //    }
  //  }
//}
