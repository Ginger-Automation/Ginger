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

using GingerCore;
using System.Collections.Generic;

namespace Amdocs.Ginger.Common
{
    public enum eStatusMsgKey
    {
        RecommendNewVersion,
        SaveItem,
        RenameItem,
        DuplicateItem,
        ExitMode,
        NoDirtyItem,
        StartAgent,
        StopAgent,
        StartAgents,
        GetLatestSolution,
        CreatingReport,
        CreateAgentTip,
        CreateBusinessFlowTip,
        ScriptImported_RefreshSolution,
        ALMTestSetImport, ExportItemToALM, ALMTestSetMap,
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
        LoadingSolution,
        ExportItem,
        StartAgentFailed,
        BusinessFlowConversion,
        Search, DownloadingMissingPluginPackages,
        GingerLoadingInfo,
        StaticStatusMessage, StaticStatusProcess, PasteProcess, CreatingBackupProcess,
        NewVersionAvailable, CleaningLegacyActions, PublishingToCentralDB
    }

    public static class StatusMsgsPool
    {
        public static void LoadStatusMsgsPool()
        {
            //Initialize the pool
            Reporter.StatusMsgsPool = new Dictionary<eStatusMsgKey, StatusMsg>();

            //Add To Status messages to the pool
            #region General Application Messages

            // FIXME not available outside amdocs
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.RecommendNewVersion, new StatusMsg(eStatusMsgType.INFO, "Upgrade Required", "Newer version of Ginger exist." + System.Environment.NewLine + "You can download the latest version from https://ginger.amdocs.com/", true, "Upgrade"));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.NewVersionAvailable, new StatusMsg(eStatusMsgType.INFO, "New version ({0}) is available", "Newer version of Ginger is available." + System.Environment.NewLine + "You can download the latest version from https://ginger.amdocs.com/", true, "Upgrade"));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.SaveItem, new StatusMsg(eStatusMsgType.PROCESS, "Saving", "Saving '{0}' {1}"));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.RenameItem, new StatusMsg(eStatusMsgType.PROCESS, "Renaming", "Renaming all the references of '{0}' to '{1}'"));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.DuplicateItem, new StatusMsg(eStatusMsgType.PROCESS, "Duplicating", "Duplicating the item '{0}'"));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.ExitMode, new StatusMsg(eStatusMsgType.INFO, "Oops...", "Ginger was not closed properly. Please turn to support team."));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.ExportItem, new StatusMsg(eStatusMsgType.PROCESS, "Exporting", "Exporting '{0}' {1}"));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.CreatingBackupProcess, new StatusMsg(eStatusMsgType.PROCESS, "CreatingBackupProcess", "Creating backup for '{0}'..."));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.StaticStatusMessage, new StatusMsg(eStatusMsgType.INFO, "Message", "{0}"));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.StaticStatusProcess, new StatusMsg(eStatusMsgType.PROCESS, "Process", "{0}"));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.PasteProcess, new StatusMsg(eStatusMsgType.PROCESS, "Paste", "{0}"));
            #endregion General Application Messages

            #region Solution Messages
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.GetLatestSolution, new StatusMsg(eStatusMsgType.PROCESS, "Get Latest Solution", "Getting the latest updates of '{0}' Solution"));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.NoDirtyItem, new StatusMsg(eStatusMsgType.PROCESS, "Save All", "No Unsaved item found."));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.LoadingSolution, new StatusMsg(eStatusMsgType.PROCESS, "Loading Solution", "{0}"));
            #endregion Solution Messages

            #region Analyzer
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.AnalyzerFixingIssues, new StatusMsg(eStatusMsgType.PROCESS, "Auto Fixing Issues", "Fixing the item '{0}'"));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.AnalyzerSavingFixedIssues, new StatusMsg(eStatusMsgType.PROCESS, "Saving Auto Fixed Issues", "Saving the item '{0}'"));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.AnalyzerIsAnalyzing, new StatusMsg(eStatusMsgType.PROCESS, "Analyzing...", "Analyzing the '{0}' {1} before execution"));
            #endregion Analyzer

            #region Agents Messages
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.StartAgent, new StatusMsg(eStatusMsgType.PROCESS, "Starting Agent", "Starting the agent '{0}' for '{1}'"));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.StartAgents, new StatusMsg(eStatusMsgType.PROCESS, "Starting Agent/s", "Starting the agent/s:{0}"));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.CreateAgentTip, new StatusMsg(eStatusMsgType.INFO, "Tip!", "Create a new 'Agent' which match to your " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " platform to allow platform connection"));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.MainframeIncorrectConfiguration, new StatusMsg(eStatusMsgType.INFO, "Mainframe Server not Available", "Mainframe server is not available on configured address and port . Please Check configuration"));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.StartAgentFailed, new StatusMsg(eStatusMsgType.INFO, "Start Agent Failed", "'{0}'"));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.StopAgent, new StatusMsg(eStatusMsgType.PROCESS, "Stoping Agent", "Stoping the agent '{0}' for '{1}'"));
            #endregion Agents Messages

            #region BusinessFlows Messages
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.CreateBusinessFlowTip, new StatusMsg(eStatusMsgType.INFO, "Tip!", "Start automating by creating a new '" + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + "' and shifting to the 'Automate' tab"));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.BusinessFlowConversion, new StatusMsg(eStatusMsgType.PROCESS, "Converting Actions", "Converting the Actions of the '{0}' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.CleaningLegacyActions, new StatusMsg(eStatusMsgType.PROCESS, "Cleaning Legacy Actions", "Cleaning Legacy Actions..."));
            #endregion BusinessFlows Messages

            #region Execution Messages
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.CreatingReport, new StatusMsg(eStatusMsgType.PROCESS, "Creating Report", "Creating report for the '{0}' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " execution"));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.LoadingRunSet, new StatusMsg(eStatusMsgType.PROCESS, "Loading " + GingerDicser.GetTermResValue(eTermResKey.RunSet), "Loading Ginger {0}"));
            #endregion Execution Messages

            #region Import/Export Messages
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.ALMTestSetImport, new StatusMsg(eStatusMsgType.PROCESS, "Importing QC/ALM Test Set", "Importing the ALM Test Set: '{0}'"));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.ExportItemToALM, new StatusMsg(eStatusMsgType.PROCESS, "Exporting to ALM", "Exporting the item: '{0}'"));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.ALMTestSetMap, new StatusMsg(eStatusMsgType.PROCESS, "Mapping To ALM Wizard upload" , "Prepares 'Map To ALM' Wizard data."));
            #endregion Import/Export Messages

            #region BusinessFlows Messages
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.ScriptImported_RefreshSolution, new StatusMsg(eStatusMsgType.INFO, "Tip!", "Refresh the Solution to view the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " created from UFT Script"));
            #endregion BusinessFlows Messages

            #region Shared Repository Messages
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.AddingToSharedRepository, new StatusMsg(eStatusMsgType.PROCESS, "Adding Item to Repository", "Adding the item: '{0}' to the shared repository "));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.ExportExecutionDetails, new StatusMsg(eStatusMsgType.PROCESS, "Export Execution Details", "Export execution details of the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ": '{0}' to {1}."));
            #endregion BusinessFlows Messages

            #region Source Control            
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.CheckInToSourceControl, new StatusMsg(eStatusMsgType.PROCESS, "Check-In to Source Control", "Current Operation: {0}"));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.GetLatestFromSourceControl, new StatusMsg(eStatusMsgType.PROCESS, "Get Latest from Source Control", "Updating local solution files to the latest version."));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.DownloadSolutionFromSourceControl, new StatusMsg(eStatusMsgType.PROCESS, "Download Solution", "Downloading the solution '{0}' from source control."));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.ResolveSourceControlConflicts, new StatusMsg(eStatusMsgType.PROCESS, "Resolve Source Control Conflicts", "Resolving local solution conflicts."));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.RevertChangesFromSourceControl, new StatusMsg(eStatusMsgType.PROCESS, "Revert Changes from Source Control", "Undo local changes and revert to source."));

            #endregion Source Control

            #region General
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.UndoChanges, new StatusMsg(eStatusMsgType.PROCESS, "Undo Changes", "Undo changes for the item '{0}'"));
            #endregion General

            Reporter.StatusMsgsPool.Add(eStatusMsgKey.RunCompleted, new StatusMsg(eStatusMsgType.INFO, GingerDicser.GetTermResValue(eTermResKey.RunSet), "Execution Completed '{0}'"));

            Reporter.StatusMsgsPool.Add(eStatusMsgKey.ExecutingRunSetAction, new StatusMsg(eStatusMsgType.PROCESS, "Executing " + GingerDicser.GetTermResValue(eTermResKey.RunSet) + "Action", "Executing '{0}'"));

            Reporter.StatusMsgsPool.Add(eStatusMsgKey.StartingRecorder, new StatusMsg(eStatusMsgType.INFO, "Driver", "Starting Recorder"));

            Reporter.StatusMsgsPool.Add(eStatusMsgKey.StoppingRecorder, new StatusMsg(eStatusMsgType.INFO, "Driver", "Stopping Recorder"));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.ASCFTryToConnect, new StatusMsg(eStatusMsgType.INFO, "ASCF Driver", "Trying to connect #{0}"));
            Reporter.StatusMsgsPool.Add(eStatusMsgKey.JavaDRiverTryToConnect, new StatusMsg(eStatusMsgType.INFO, "Java Driver", "Trying to connect #{0}"));

            Reporter.StatusMsgsPool.Add(eStatusMsgKey.ShowBetaFeatures, new StatusMsg(eStatusMsgType.INFO, "Show beta Features is: ", "{0}"));

            Reporter.StatusMsgsPool.Add(eStatusMsgKey.GingerHelpLibrary, new StatusMsg(eStatusMsgType.INFO, "Ginger Help Library is Ready!", "Press [F1] from anywhere and view User Guides & Videos related to the topic you working on!"));

            Reporter.StatusMsgsPool.Add(eStatusMsgKey.Search, new StatusMsg(eStatusMsgType.PROCESS, "Searching...", "Searching {0}..."));

            Reporter.StatusMsgsPool.Add(eStatusMsgKey.DownloadingMissingPluginPackages, new StatusMsg(eStatusMsgType.PROCESS, "Restoring Missing Plugins Packages", "Restoring Missing Plugin Packages..."));

            Reporter.StatusMsgsPool.Add(eStatusMsgKey.GingerLoadingInfo, new StatusMsg(eStatusMsgType.PROCESS, "loading", "{0}"));

            Reporter.StatusMsgsPool.Add(eStatusMsgKey.PublishingToCentralDB, new StatusMsg(eStatusMsgType.PROCESS, "Publishing...", "{0}"));



        }
    }
}
