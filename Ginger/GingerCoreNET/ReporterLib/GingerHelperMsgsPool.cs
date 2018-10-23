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

using GingerCore;
using System.Collections.Generic;

namespace GingerCoreNET.ReporterLib
{
    public enum eGingerHelperMsgKey
    {
        RecommendNewVersion,
        SaveItem,
        ExitMode,
        NoDirtyItem,
        StartAgent,
        StartAgents,
        GetLatestSolution,
        CreatingReport,
        CreateAgentTip,
        CreateBusinessFlowTip,
        ScriptImported_RefreshSolution,
        ALMTestSetImport, ExportItemToALM,
        AddingToSharedRepository,
        CheckInToSourceControl, GetLatestFromSourceControl, DownloadSolutionFromSourceControl,ResolveSourceControlConflicts,RevertChangesFromSourceControl,
        ExportExecutionDetails, UndoChanges,
        LoadingRunSet,
        RunCompleted,
        ExecutingRunSetAction,
        StartingRecorder,
        StoppingRecorder,
        ASCFTryToConnect, JavaDRiverTryToConnect,
        MainframeIncorrectConfiguration,
        ShowBetaFeatures,
        AnalyzerFixingIssues,AnalyzerSavingFixedIssues,AnalyzerIsAnalyzing,
        GingerHelpLibrary,
        LoadingSolution
    }

    public static class GingerHelperMsgsPool
    {
        public static void LoadGingerHelperMsgsPool()
        {
            //Initialize the pool
            Reporter.GingerHelperMsgsPool = new Dictionary<eGingerHelperMsgKey, GingerHelperMsg>();

            //Add Ginger helper messages to the pool
            #region General Application Messages
            Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.RecommendNewVersion, new GingerHelperMsg(eGingerHelperMsgType.INFO, "Upgrade Required", "Newer version of Ginger exist." + System.Environment.NewLine + "You can download the latest version from http://cmitechint1srv:8089/", true, "Upgrade"));
            Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.SaveItem, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Saving", "Saving '{0}' {1}"));
            Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.ExitMode, new GingerHelperMsg(eGingerHelperMsgType.INFO, "Oops...","Ginger was not closed properly. Please turn to support team."));
            #endregion General Application Messages

            #region Solution Messages
            Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.GetLatestSolution, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Get Latest Solution", "Getting the latest updates of '{0}' Solution"));
            Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.NoDirtyItem, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Save All", "No Unsaved item found."));
            Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.LoadingSolution, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Loading Solution", "{0}"));
            #endregion Solution Messages

            #region Analyzer
            Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.AnalyzerFixingIssues, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Auto Fixing Issues", "Fixing the item '{0}'"));
            Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.AnalyzerSavingFixedIssues, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Saving Auto Fixed Issues", "Saving the item '{0}'"));
            Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.AnalyzerIsAnalyzing, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Analyzing...", "Analyzing the '{0}' {1} before execution"));
            #endregion Analyzer

            #region Agents Messages
            Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.StartAgent, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Starting Agent", "Starting the agent '{0}' for '{1}'"));
            Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.StartAgents, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Starting Agent/s", "Starting the agent/s:{0}"));
            Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.CreateAgentTip, new GingerHelperMsg(eGingerHelperMsgType.INFO, "Tip!", "Create a new 'Agent' which match to your " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " platform to allow platform connection"));
            Reporter.GingerHelperMsgsPool.Add (eGingerHelperMsgKey.MainframeIncorrectConfiguration, new GingerHelperMsg (eGingerHelperMsgType.INFO, "Mainframe Server not Available", "Mainframe server is not available on configured address and port . Please Check configuration"));
            #endregion Agents Messages

            #region BusinessFlows Messages
            Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.CreateBusinessFlowTip, new GingerHelperMsg(eGingerHelperMsgType.INFO, "Tip!", "Start automating by creating a new '" + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + "' and shifting to the 'Automate' tab"));
            #endregion BusinessFlows Messages

            #region Execution Messages
            Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.CreatingReport, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Creating Report", "Creating report for the '{0}' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " execution"));
            Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.LoadingRunSet, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Loading " + GingerDicser.GetTermResValue(eTermResKey.RunSet), "Loading Ginger {0}"));
            #endregion Execution Messages

            #region Import/Export Messages
            Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.ALMTestSetImport, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Importing QC/ALM Test Set", "Importing the ALM Test Set: '{0}'"));
            Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.ExportItemToALM, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Exporting to ALM", "Exporting the item: '{0}'"));           
            
            #endregion Import/Export Messages
            
            #region BusinessFlows Messages
            Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.ScriptImported_RefreshSolution, new GingerHelperMsg(eGingerHelperMsgType.INFO, "Tip!", "Refresh the Solution to view the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " created from UFT Script"));
            #endregion BusinessFlows Messages

            #region Shared Repository Messages
            Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.AddingToSharedRepository, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Adding Item to Repository", "Adding the item: '{0}' to the shared repository "));
            Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.ExportExecutionDetails, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Export Execution Details", "Export execution details of the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ": '{0}' to {1}."));            
            #endregion BusinessFlows Messages

            #region Source Control            
             Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.CheckInToSourceControl, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Check-In to Source Control", "Current Operation: {0}"));
             Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.GetLatestFromSourceControl, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Get Latest from Source Control", "Updating local solution files to the latest version."));
             Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.DownloadSolutionFromSourceControl, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Download Solution", "Downloading the solution '{0}' from source control."));
             Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.ResolveSourceControlConflicts, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Resolve Source Control Conflicts", "Resolving local solution conflicts."));
             Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.RevertChangesFromSourceControl, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Revert Changes from Source Control", "Undo local changes and revert to source."));
            
            #endregion Source Control

            #region General
             Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.UndoChanges, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Undo Changes", "Undo changes for the item '{0}'"));
            #endregion General

             Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.RunCompleted, new GingerHelperMsg(eGingerHelperMsgType.INFO, GingerDicser.GetTermResValue(eTermResKey.RunSet), "Execution Completed '{0}'"));

             Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.ExecutingRunSetAction, new GingerHelperMsg(eGingerHelperMsgType.PROCESS, "Executing " + GingerDicser.GetTermResValue(eTermResKey.RunSet) +"Action", "Executing '{0}'"));

             Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.StartingRecorder, new GingerHelperMsg(eGingerHelperMsgType.INFO, "Driver", "Starting Recorder"));

             Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.StoppingRecorder, new GingerHelperMsg(eGingerHelperMsgType.INFO, "Driver", "Stopping Recorder"));
             Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.ASCFTryToConnect, new GingerHelperMsg(eGingerHelperMsgType.INFO, "ASCF Driver", "Trying to connect #{0}"));
             Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.JavaDRiverTryToConnect, new GingerHelperMsg(eGingerHelperMsgType.INFO, "Java Driver", "Trying to connect #{0}"));

             Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.ShowBetaFeatures, new GingerHelperMsg(eGingerHelperMsgType.INFO, "Show beta Features is: ", "{0}"));

            Reporter.GingerHelperMsgsPool.Add(eGingerHelperMsgKey.GingerHelpLibrary, new GingerHelperMsg(eGingerHelperMsgType.INFO, "Ginger Help Library is Ready!", "Press [F1] from anywhere and view User Guides & Videos related to the topic you working on!"));
        }
    }
}