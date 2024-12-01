#region License
/*
Copyright © 2014-2024 European Support Limited

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

using System.Collections.Generic;
using GingerCore;

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
        CheckInToSourceControl, GetLatestFromSourceControl, DownloadSolutionFromSourceControl, ResolveSourceControlConflicts, RevertChangesFromSourceControl,
        ExportExecutionDetails, UndoChanges,
        LoadingRunSet,
        RunCompleted,
        ExecutingRunSetAction,
        StartingRecorder,
        StoppingRecorder,
        ASCFTryToConnect, JavaDRiverTryToConnect,
        MainframeIncorrectConfiguration,
        ShowBetaFeatures,
        AnalyzerFixingIssues, AnalyzerSavingFixedIssues, AnalyzerIsAnalyzing,
        GingerHelpLibrary,
        LoadingSolution,
        ExportItem,
        StartAgentFailed,
        BusinessFlowConversion,
        Search, DownloadingMissingPluginPackages,
        GingerLoadingInfo,
        StaticStatusMessage, StaticStatusProcess, PasteProcess, CreatingBackupProcess,
        NewVersionAvailable, CleaningLegacyActions, PublishingToCentralDB,
        ExportingToBPMNFile,
        ExportingToBPMNZIP,
        LoadingTreeViewChildren,
        TestingDatabase
    }

    public static class StatusMsgsPool
    {
        public static void LoadStatusMsgsPool()
        {
            //Initialize the pool
            Reporter.StatusMsgsPool = new Dictionary<eStatusMsgKey, StatusMsg>
            {
                //Add To Status messages to the pool
                #region General Application Messages

                // FIXME not available outside amdocs
                { eStatusMsgKey.RecommendNewVersion, new StatusMsg(eStatusMsgType.INFO, "Upgrade Required", "Newer version of Ginger exist." + System.Environment.NewLine + "You can download the latest version from https://ginger.amdocs.com/", true, "Upgrade") },
                { eStatusMsgKey.NewVersionAvailable, new StatusMsg(eStatusMsgType.INFO, "New version ({0}) is available", "Newer version of Ginger is available." + System.Environment.NewLine + "You can download the latest version from https://ginger.amdocs.com/", true, "Upgrade") },
                { eStatusMsgKey.SaveItem, new StatusMsg(eStatusMsgType.PROCESS, "Saving", "Saving '{0}' {1}") },
                { eStatusMsgKey.RenameItem, new StatusMsg(eStatusMsgType.PROCESS, "Renaming", "Renaming all the references of '{0}' to '{1}'") },
                { eStatusMsgKey.DuplicateItem, new StatusMsg(eStatusMsgType.PROCESS, "Duplicating", "Duplicating the item '{0}'") },
                { eStatusMsgKey.ExitMode, new StatusMsg(eStatusMsgType.INFO, "Oops...", "Ginger was not closed properly. Please turn to support team.") },
                { eStatusMsgKey.ExportItem, new StatusMsg(eStatusMsgType.PROCESS, "Exporting", "Exporting '{0}' {1}") },
                { eStatusMsgKey.CreatingBackupProcess, new StatusMsg(eStatusMsgType.PROCESS, "CreatingBackupProcess", "Creating backup for '{0}'...") },
                { eStatusMsgKey.StaticStatusMessage, new StatusMsg(eStatusMsgType.INFO, "Message", "{0}") },
                { eStatusMsgKey.StaticStatusProcess, new StatusMsg(eStatusMsgType.PROCESS, "Process", "{0}") },
                { eStatusMsgKey.PasteProcess, new StatusMsg(eStatusMsgType.PROCESS, "Paste", "{0}") },
                #endregion General Application Messages

                #region Solution Messages
                { eStatusMsgKey.GetLatestSolution, new StatusMsg(eStatusMsgType.PROCESS, "Get Latest Solution", "Getting the latest updates of '{0}' Solution") },
                { eStatusMsgKey.NoDirtyItem, new StatusMsg(eStatusMsgType.PROCESS, "Save All", "No Unsaved item found.") },
                { eStatusMsgKey.LoadingSolution, new StatusMsg(eStatusMsgType.PROCESS, "Loading Solution", "{0}") },
                #endregion Solution Messages

                #region Analyzer
                { eStatusMsgKey.AnalyzerFixingIssues, new StatusMsg(eStatusMsgType.PROCESS, "Auto Fixing Issues", "Fixing the item '{0}'") },
                { eStatusMsgKey.AnalyzerSavingFixedIssues, new StatusMsg(eStatusMsgType.PROCESS, "Saving Auto Fixed Issues", "Saving the item '{0}'") },
                { eStatusMsgKey.AnalyzerIsAnalyzing, new StatusMsg(eStatusMsgType.PROCESS, "Analyzing...", "Analyzing the '{0}' {1} before execution") },
                #endregion Analyzer

                #region Agents Messages
                { eStatusMsgKey.StartAgent, new StatusMsg(eStatusMsgType.PROCESS, "Starting Agent", "Starting the agent '{0}' for '{1}'") },
                { eStatusMsgKey.StartAgents, new StatusMsg(eStatusMsgType.PROCESS, "Starting Agent/s", "Starting the agent/s:{0}") },
                { eStatusMsgKey.CreateAgentTip, new StatusMsg(eStatusMsgType.INFO, "Tip!", "Create a new 'Agent' which match to your " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " platform to allow platform connection") },
                { eStatusMsgKey.MainframeIncorrectConfiguration, new StatusMsg(eStatusMsgType.INFO, "Mainframe Server not Available", "Mainframe server is not available on configured address and port . Please Check configuration") },
                { eStatusMsgKey.StartAgentFailed, new StatusMsg(eStatusMsgType.INFO, "Start Agent Failed", "'{0}'") },
                { eStatusMsgKey.StopAgent, new StatusMsg(eStatusMsgType.PROCESS, "Stoping Agent", "Stoping the agent '{0}' for '{1}'") },
                #endregion Agents Messages

                #region BusinessFlows Messages
                { eStatusMsgKey.CreateBusinessFlowTip, new StatusMsg(eStatusMsgType.INFO, "Tip!", "Start automating by creating a new '" + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + "' and shifting to the 'Automate' tab") },
                { eStatusMsgKey.BusinessFlowConversion, new StatusMsg(eStatusMsgType.PROCESS, "Converting Actions", "Converting the Actions of the '{0}' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)) },
                { eStatusMsgKey.CleaningLegacyActions, new StatusMsg(eStatusMsgType.PROCESS, "Cleaning Legacy Actions", "Cleaning Legacy Actions...") },
                #endregion BusinessFlows Messages

                #region Execution Messages
                { eStatusMsgKey.CreatingReport, new StatusMsg(eStatusMsgType.PROCESS, "Creating Report", "Creating report for the '{0}' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " execution") },
                { eStatusMsgKey.LoadingRunSet, new StatusMsg(eStatusMsgType.PROCESS, "Loading " + GingerDicser.GetTermResValue(eTermResKey.RunSet), "Loading Ginger {0}") },
                #endregion Execution Messages

                #region Import/Export Messages
                { eStatusMsgKey.ALMTestSetImport, new StatusMsg(eStatusMsgType.PROCESS, "Importing QC/ALM Test Set", "Importing the ALM Test Set: '{0}'") },
                { eStatusMsgKey.ExportItemToALM, new StatusMsg(eStatusMsgType.PROCESS, "Exporting to ALM", "Exporting the item: '{0}'") },
                { eStatusMsgKey.ALMTestSetMap, new StatusMsg(eStatusMsgType.PROCESS, "Mapping To ALM Wizard upload", "Prepares 'Map To ALM' Wizard data.") },
                { eStatusMsgKey.ExportingToBPMNFile, new StatusMsg(eStatusMsgType.PROCESS, "Exporting BPMN", "Exporting BPMN File") },
                { eStatusMsgKey.ExportingToBPMNZIP, new StatusMsg(eStatusMsgType.PROCESS, "Exporting BPMN", "Exporting as Otoma Use Case Zip file") },
                #endregion Import/Export Messages

                #region BusinessFlows Messages
                { eStatusMsgKey.ScriptImported_RefreshSolution, new StatusMsg(eStatusMsgType.INFO, "Tip!", "Refresh the Solution to view the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " created from UFT Script") },
                #endregion BusinessFlows Messages

                #region Shared Repository Messages
                { eStatusMsgKey.AddingToSharedRepository, new StatusMsg(eStatusMsgType.PROCESS, "Adding Item to Repository", "Adding the item: '{0}' to the shared repository ") },
                { eStatusMsgKey.ExportExecutionDetails, new StatusMsg(eStatusMsgType.PROCESS, "Export Execution Details", "Export execution details of the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ": '{0}' to {1}.") },
                #endregion BusinessFlows Messages

                #region Source Control            
                { eStatusMsgKey.CheckInToSourceControl, new StatusMsg(eStatusMsgType.PROCESS, "Check-In to Source Control", "Current Operation: {0}") },
                { eStatusMsgKey.GetLatestFromSourceControl, new StatusMsg(eStatusMsgType.PROCESS, "Get Latest from Source Control", "Updating local solution files to the latest version.") },
                { eStatusMsgKey.DownloadSolutionFromSourceControl, new StatusMsg(eStatusMsgType.PROCESS, "Download Solution", "Downloading the solution '{0}' from source control.") },
                { eStatusMsgKey.ResolveSourceControlConflicts, new StatusMsg(eStatusMsgType.PROCESS, "Resolve Source Control Conflicts", "Resolving local solution conflicts.") },
                { eStatusMsgKey.RevertChangesFromSourceControl, new StatusMsg(eStatusMsgType.PROCESS, "Revert Changes from Source Control", "Undo local changes and revert to source.") },

                #endregion Source Control

                #region General
                { eStatusMsgKey.UndoChanges, new StatusMsg(eStatusMsgType.PROCESS, "Undo Changes", "Undo changes for the item '{0}'") },
                { eStatusMsgKey.LoadingTreeViewChildren, new StatusMsg(eStatusMsgType.PROCESS, "Loading Items", "Loading items...") },
                #endregion General

                { eStatusMsgKey.RunCompleted, new StatusMsg(eStatusMsgType.INFO, GingerDicser.GetTermResValue(eTermResKey.RunSet), "Execution Completed '{0}'") },
                { eStatusMsgKey.ExecutingRunSetAction, new StatusMsg(eStatusMsgType.PROCESS, "Executing " + GingerDicser.GetTermResValue(eTermResKey.RunSet) + "Action", "Executing '{0}'") },
                { eStatusMsgKey.StartingRecorder, new StatusMsg(eStatusMsgType.INFO, "Driver", "Starting Recorder") },
                { eStatusMsgKey.StoppingRecorder, new StatusMsg(eStatusMsgType.INFO, "Driver", "Stopping Recorder") },
                { eStatusMsgKey.ASCFTryToConnect, new StatusMsg(eStatusMsgType.INFO, "ASCF Driver", "Trying to connect #{0}") },
                { eStatusMsgKey.JavaDRiverTryToConnect, new StatusMsg(eStatusMsgType.INFO, "Java Driver", "Trying to connect #{0}") },
                { eStatusMsgKey.ShowBetaFeatures, new StatusMsg(eStatusMsgType.INFO, "Show beta Features is: ", "{0}") },
                { eStatusMsgKey.GingerHelpLibrary, new StatusMsg(eStatusMsgType.INFO, "Ginger Help Library is Ready!", "Press [F1] from anywhere and view User Guides & Videos related to the topic you working on!") },
                { eStatusMsgKey.Search, new StatusMsg(eStatusMsgType.PROCESS, "Searching...", "Searching {0}...") },
                { eStatusMsgKey.DownloadingMissingPluginPackages, new StatusMsg(eStatusMsgType.PROCESS, "Restoring Missing Plugins Packages", "Restoring Missing Plugin Packages...") },
                { eStatusMsgKey.GingerLoadingInfo, new StatusMsg(eStatusMsgType.PROCESS, "loading", "{0}") },
                { eStatusMsgKey.PublishingToCentralDB, new StatusMsg(eStatusMsgType.PROCESS, "Publishing...", "{0}") },
                { eStatusMsgKey.TestingDatabase, new StatusMsg(eStatusMsgType.PROCESS, "Testing Database...", "{0}") }
            };


        }
    }
}
