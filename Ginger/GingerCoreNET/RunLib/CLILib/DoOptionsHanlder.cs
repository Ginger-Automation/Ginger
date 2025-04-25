#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.CoreNET.Reports;
using Amdocs.Ginger.Repository;
using Ginger.AnalyzerLib;
using Ginger.Reports;
using Ginger.Run;
using GingerCore;
using GraphQLClient.Clients;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{

    public class DoOptionsHandler
    {
        public static event EventHandler<BusinessFlow> AutomateBusinessFlowEvent;
        public static event EventHandler<RunSetConfig> LoadRunSetConfigEvent;
        public static event EventHandler<Activity> LoadSharedRepoEvent;
        public static event EventHandler LoadSourceControlDownloadPage;
        DoOptions mOpts;
        CLIHelper mCLIHelper = new();
        public async Task RunAsync(DoOptions opts)
        {
            mOpts = opts;
            if (opts.UseTempSolutionFolder)
            {
                mOpts.Solution = SetSolutionPathToTempFolder(opts.URL);
            }
            switch (opts.Operation)
            {
                case DoOptions.DoOperation.analyze:
                    DoAnalyze();
                    break;
                case DoOptions.DoOperation.clean:
                    // TODO: remove execution folder, backups and more
                    break;
                case DoOptions.DoOperation.info:
                    DoInfo();
                    break;
                case DoOptions.DoOperation.open:
                    await DoOpenAsync();
                    break;
                case DoOptions.DoOperation.MultiPOMUpdate:
                    await DoMultiPOMUpdate();
                    break;
            }
        }


        private void DoInfo()
        {
            // TODO: print info on solution, how many BFs etc, try to read all items - for Linux deser test
            WorkSpace.Instance.OpenSolution(mOpts.Solution);
            StringBuilder stringBuilder = new StringBuilder(Environment.NewLine);
            stringBuilder.Append("Solution Name  :").Append(WorkSpace.Instance.Solution.Name).Append(Environment.NewLine);
            stringBuilder.Append("Business Flows :").Append(WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>().Count).Append(Environment.NewLine);
            stringBuilder.Append("Agents         :").Append(WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>().Count).Append(Environment.NewLine);

            // TODO: add more info

            Reporter.ToLog(eLogLevel.INFO, stringBuilder.ToString());
        }

        /// <summary>
        /// Opens the solution specified in the options.
        /// </summary>
        /// <param name="solutionFolder">The folder path of the solution to open.</param>
        /// <param name="encryptionKey">The encryption key for the solution, if any.</param>
        private async Task DoOpenAsync()
        {
            string solutionFolder = mOpts.Solution;
            string encryptionKey = mOpts.EncryptionKey;
            try
            {
                // Check if solutionFolder is null or empty
                if (string.IsNullOrWhiteSpace(solutionFolder))
                {
                    Reporter.ToLog(eLogLevel.ERROR, "The provided solution folder path is null or empty.");
                    return;
                }
                // Check if the folder path contains the solution file name
                if (solutionFolder.Contains("Ginger.Solution.xml"))
                {
                    solutionFolder = Path.GetDirectoryName(solutionFolder)?.Trim() ?? string.Empty;

                    if (string.IsNullOrEmpty(solutionFolder))
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Invalid solution folder path derived from the solution file.");
                        return;
                    }
                }

                // Attempt to open the solution
                mCLIHelper.AddCLIGitProperties(mOpts);
                mCLIHelper.SetWorkSpaceGitProperties(mOpts);
                mCLIHelper.SetEncryptionKey(encryptionKey);
                if (mOpts.PasswordEncrypted)
                {
                    mCLIHelper.PasswordEncrypted(mOpts.PasswordEncrypted.ToString());
                }
                mCLIHelper.Solution = mOpts.Solution;
                if (!await mCLIHelper.LoadSolutionAsync())
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to Download/update Solution from source control");
                    LoadSourceControlDownloadPage?.Invoke(null, EventArgs.Empty);
                    return;
                }


                if (!string.IsNullOrWhiteSpace(mOpts.ExecutionId))
                {
                    if (await OpenRunSetByExecutionId())
                    {
                        return;
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Runset not found by given Execution ID:{mOpts.ExecutionId}");
                    }
                }

                if (!string.IsNullOrWhiteSpace(mOpts.RunSetId))
                {
                    if (LoadCLIRunSetByID())
                    {
                        return;
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Runset not found by given ID:{mOpts.RunSetId}");
                    }
                }

                if (!string.IsNullOrWhiteSpace(mOpts.RunSetName))
                {
                    if (OpenCLIRunSetByName())
                    {
                        return;
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Runset not found by given Name:{mOpts.RunSetName}");
                    }
                }

                if (!string.IsNullOrWhiteSpace(mOpts.BusinessFlowId))
                {
                    var businessFlow = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<BusinessFlow>(Guid.Parse(mOpts.BusinessFlowId));
                    if (businessFlow != null)
                    {
                        AutomateBusinessFlowEvent?.Invoke(null, businessFlow);
                        return;
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Businessflow not found by given ID:{mOpts.BusinessFlowId}");
                    }
                }

                if (!string.IsNullOrWhiteSpace(mOpts.BusinessFlowName))
                {
                    var businessFlow = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>()
                        .FirstOrDefault(bf => bf.Name.Equals(mOpts.BusinessFlowName, StringComparison.OrdinalIgnoreCase));
                    if (businessFlow != null)
                    {
                        AutomateBusinessFlowEvent?.Invoke(null, businessFlow);
                        return;
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Businessflow not found by given Name:{mOpts.BusinessFlowName}");
                    }
                }

                if (!string.IsNullOrWhiteSpace(mOpts.SharedActivityId))
                {
                    var sharedActivity = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<Activity>(Guid.Parse(mOpts.SharedActivityId));
                    if (sharedActivity != null)
                    {
                        LoadSharedRepoEvent?.Invoke(null, sharedActivity);
                        return;
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Shared Activity not found by given id:{mOpts.SharedActivityId}");
                    }
                }

                if (!string.IsNullOrWhiteSpace(mOpts.SharedActivityName))
                {
                    var sharedActivity = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>().FirstOrDefault(sa => sa.ActivityName.Equals(mOpts.SharedActivityName, StringComparison.OrdinalIgnoreCase));
                    if (sharedActivity != null)
                    {
                        LoadSharedRepoEvent?.Invoke(null, sharedActivity);
                        return;
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Shared Activity not found by given Name:{mOpts.SharedActivityName}");
                    }
                }

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"An unexpected error occurred while opening the solution in folder '{solutionFolder}'. Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Opens the CLI runset by name.
        /// </summary>
        /// <returns>True if the runset is found and opened successfully, otherwise false.</returns>
        private bool OpenCLIRunSetByName()
        {
            bool autoLoadLastRunSetFlag = WorkSpace.Instance.UserProfile.AutoLoadLastRunSet;
            try
            {
                ObservableList<RunSetConfig> allRunsets = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>();
                RunSetConfig cliRunset = allRunsets.FirstOrDefault(runsets => runsets.Name == mOpts.RunSetName);

                if (cliRunset != null)
                {
                    WorkSpace.Instance.UserProfile.AutoLoadLastRunSet = true;
                    WorkSpace.Instance.UserProfile.RecentRunset = cliRunset.Guid;
                    LoadRunSetConfigEvent?.Invoke(sender: this, cliRunset);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                WorkSpace.Instance.UserProfile.AutoLoadLastRunSet = autoLoadLastRunSetFlag;
            }
        }

        /// <summary>
        /// Loads the CLI runset by ID.
        /// </summary>
        /// <returns>True if the runset is found and loaded successfully, otherwise false.</returns>
        private bool LoadCLIRunSetByID()
        {
            bool autoLoadLastRunSetFlag = WorkSpace.Instance.UserProfile.AutoLoadLastRunSet;
            try
            {
                var cliRunset = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<RunSetConfig>(Guid.Parse(mOpts.RunSetId));

                if (cliRunset != null)
                {
                    WorkSpace.Instance.UserProfile.AutoLoadLastRunSet = true;
                    WorkSpace.Instance.UserProfile.RecentRunset = cliRunset.Guid;
                    LoadRunSetConfigEvent?.Invoke(sender: this, cliRunset);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                WorkSpace.Instance.UserProfile.AutoLoadLastRunSet = autoLoadLastRunSetFlag;
            }
        }

        /// <summary>
        /// Opens the runset by execution ID.
        /// </summary>
        /// <returns>True if the runset is found and opened successfully, otherwise false.</returns>
        private async Task<bool> OpenRunSetByExecutionId()
        {
            bool autoLoadLastRunSetFlag = WorkSpace.Instance.UserProfile.AutoLoadLastRunSet;
            DateTime? executionStartTime = null; // Initialize the variable

            try
            {
                string endPoint = GingerRemoteExecutionUtils.GetReportDataServiceUrl();
                if (!string.IsNullOrEmpty(endPoint))
                {
                    GraphQlClient graphQlClient = new GraphQlClient($"{endPoint}api/graphql");
                    ExecutionReportGraphQLClient executionReportGraphQLClient = new ExecutionReportGraphQLClient(graphQlClient);
                    var response = await executionReportGraphQLClient.FetchDataBySolutionAndExecutionId(WorkSpace.Instance.Solution.Guid, Guid.Parse(mOpts.ExecutionId));

                    if (response?.Data?.Runsets?.Nodes.Count > 0)
                    {
                        var node = response.Data.Runsets.Nodes.First();
                        executionStartTime = node.StartTime;
                        var cliRunset = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<RunSetConfig>(node.EntityId.Value);
                        if (cliRunset != null)
                        {
                            WorkSpace.Instance.UserProfile.RecentRunset = cliRunset.Guid;
                            WorkSpace.Instance.UserProfile.AutoLoadLastRunSet = true;
                            LoadRunSetConfigEvent?.Invoke(sender: this, cliRunset);
                            return true;
                        }
                    }
                }

                return await LoadVirtualRunset(executionStartTime);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while connecting remote.", ex);
                return false;
            }
            finally
            {
                WorkSpace.Instance.UserProfile.AutoLoadLastRunSet = autoLoadLastRunSetFlag;
            }
        }

        /// <summary>
        /// Loads a virtual runset based on the provided execution ID.
        /// </summary>
        /// <returns>True if the virtual runset is loaded successfully, otherwise false.</returns>
        private async Task<bool> LoadVirtualRunset(DateTime? executionStartTime)
        {
            if (!Guid.TryParse(mOpts.ExecutionId, out Guid executionId))
            {
                Reporter.ToLog(eLogLevel.ERROR, "Invalid Execution ID format.");
                return false;
            }

            RunSetReport runsetReport = new()
            {
                GUID = mOpts.ExecutionId,
                RunSetGuid = executionId,
            };
            if (executionStartTime != null)
            {
                runsetReport.StartTimeStamp = (DateTime)executionStartTime;
            }
            RunsetFromReportLoader _runsetFromReportLoader = new RunsetFromReportLoader();
            RunSetConfig? runset = await _runsetFromReportLoader.LoadAsync(runsetReport, mOpts.ExecutionConfigurationSourceUrl);
            if (runset != null)
            {
                WorkSpace.Instance.UserProfile.RecentRunset = runset.Guid;
                WorkSpace.Instance.UserProfile.AutoLoadLastRunSet = true;
                LoadRunSetConfigEvent?.Invoke(sender: this, runset);
                return true;
            }
            return false;
        }

        private void DoAnalyze()
        {
            WorkSpace.Instance.OpenSolution(mOpts.Solution);

            AnalyzerUtils analyzerUtils = new AnalyzerUtils();
            ObservableList<AnalyzerItemBase> issues = [];
            analyzerUtils.RunSolutionAnalyzer(WorkSpace.Instance.Solution, issues);

            if (issues.Count == 0)
            {
                Reporter.ToLog(eLogLevel.INFO, "Analyzer- No Issues found");
            }
            else
            {
                Reporter.ToLog(eLogLevel.WARN, "Analyzer- Issues found, Total count: " + issues.Count);
            }

            foreach (AnalyzerItemBase issue in issues)
            {
                StringBuilder stringBuilder = new StringBuilder(Environment.NewLine);
                stringBuilder.Append("Description :").Append(issue.Description).Append(Environment.NewLine);
                stringBuilder.Append("Details     :").Append(issue.Details).Append(Environment.NewLine);
                stringBuilder.Append("Class       :").Append(issue.ItemClass).Append(Environment.NewLine);
                stringBuilder.Append("Name        :").Append(issue.ItemName).Append(Environment.NewLine);
                stringBuilder.Append("Impact      :").Append(issue.Impact).Append(Environment.NewLine);

                switch (issue.IssueType)
                {
                    case AnalyzerItemBase.eType.Error:
                        Reporter.ToLog(eLogLevel.ERROR, stringBuilder.ToString());
                        break;
                    case AnalyzerItemBase.eType.Info:
                        Reporter.ToLog(eLogLevel.INFO, stringBuilder.ToString());
                        break;
                    case AnalyzerItemBase.eType.Warning:
                        Reporter.ToLog(eLogLevel.WARN, stringBuilder.ToString());
                        break;
                }

            }
        }

        private string SetSolutionPathToTempFolder(string sourceControlUrl)
        {
            return mCLIHelper.GetTempFolderPathForRepo(sourceControlUrl);
        }

        /// <summary>
        /// Update MultiPOM Update.
        /// </summary>
        /// <param name="solutionFolder">The folder path of the solution to open.</param>
        /// <param name="encryptionKey">The encryption key for the solution, if any.</param>
        private async Task DoMultiPOMUpdate()
        {
            WorkSpace.Instance.GingerCLIMode = eGingerCLIMode.run;
            string solutionFolder = mOpts.Solution;
            string encryptionKey = mOpts.EncryptionKey;

            try
            {
                // Validate solution folder path
                if (string.IsNullOrWhiteSpace(solutionFolder))
                {
                    Reporter.ToLog(eLogLevel.ERROR, "The provided solution folder path is null or empty.");
                    return;
                }

                // Adjust solution folder path if it contains the solution file name
                if (solutionFolder.Contains("Ginger.Solution.xml"))
                {
                    solutionFolder = Path.GetDirectoryName(solutionFolder)?.Trim() ?? string.Empty;

                    if (string.IsNullOrEmpty(solutionFolder))
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Invalid solution folder path derived from the solution file.");
                        return;
                    }
                }

                // Set up CLI helper properties
                mCLIHelper.AddCLIGitProperties(mOpts);
                mCLIHelper.SetWorkSpaceGitProperties(mOpts);
                mCLIHelper.SetEncryptionKey(encryptionKey);

                if (mOpts.PasswordEncrypted)
                {
                    mCLIHelper.PasswordEncrypted(mOpts.PasswordEncrypted.ToString());
                }

                mCLIHelper.Solution = mOpts.Solution;

                // Load the solution
                if (!await mCLIHelper.LoadSolutionAsync())
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to Download/update Solution from source control");
                    LoadSourceControlDownloadPage?.Invoke(null, EventArgs.Empty);
                    return;
                }

                // Update POM data based on target application
                if (string.IsNullOrEmpty(mOpts.TargetApplication))
                {
                    await UpdateMultiPOMData(mOpts.ApplicationModels, mOpts.RunSets);
                }
                else
                {
                    string targetApplication = mOpts.TargetApplication;
                    ApplicationPlatform? targetApp = null;

                    if (Guid.TryParse(targetApplication, out Guid parsedGuid))
                    {
                        targetApp = WorkSpace.Instance.Solution.ApplicationPlatforms.FirstOrDefault(x => x.Guid.Equals(parsedGuid));
                    }
                    else
                    {
                        targetApp = WorkSpace.Instance.Solution.ApplicationPlatforms
                            .FirstOrDefault(ta => ta.AppName == targetApplication && (ta.Platform == ePlatformType.Web || ta.Platform == ePlatformType.Mobile));
                    }

                    if (targetApp == null)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Target application '{targetApplication}' not found.");
                        return;
                    }

                    var applicationPOMModels = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>()
                        .Where(model => model.TargetApplicationKey.Guid == targetApp.Guid)
                        .ToList();

                    var ApplicationPOMModelrunsetConfigMapping = new Dictionary<ApplicationPOMModel, List<RunSetConfig>>();
                    var runSetConfigList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>();
                    var businessFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GingerCore.BusinessFlow>();
                    var multiPomRunSetMappingsList = GingerCoreNET.GeneralLib.General.GetSelectedRunsetList(
                        runSetConfigList, businessFlows, applicationPOMModels, ApplicationPOMModelrunsetConfigMapping
                    );

                    foreach (var item in multiPomRunSetMappingsList)
                    {
                        await GingerCoreNET.GeneralLib.General.RunSelectedRunset(item, multiPomRunSetMappingsList, mCLIHelper);
                    }
                    var statuses = multiPomRunSetMappingsList.Select(m => m.PomUpdateStatus);
                    Reporter.ToLog(eLogLevel.INFO, $"POM Status: {string.Join(", ", statuses)}");
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"An unexpected error occurred while updating POM. Error:",ex);
            }
        }

        private async Task UpdateMultiPOMData(string POMGuids, string RunsetGuids)
        {
            var POMGuidsList = !string.IsNullOrEmpty(POMGuids)
                ? POMGuids.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(g => Guid.Parse(g.Trim())).ToList()
                : new List<Guid>();

            var RunsetGuidsList = !string.IsNullOrEmpty(RunsetGuids)
                ? RunsetGuids.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(g => Guid.Parse(g.Trim())).ToList()
                : new List<Guid>();

            var ApplicationPOMModelrunsetConfigMapping = new Dictionary<ApplicationPOMModel, List<RunSetConfig>>();

            if (POMGuidsList.Any() && RunsetGuidsList.Any())
            {
                var applicationPOMModels = POMGuidsList
                     .Select(pomGuid => WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<ApplicationPOMModel>(pomGuid))
                     .ToList();

                var runSetConfigList = new ObservableList<RunSetConfig>(
                    RunsetGuidsList.Select(runsetGuid => WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<RunSetConfig>(runsetGuid))
                );

               var mPOMModels = GingerCoreNET.GeneralLib.General.ConvertListToObservableList(
                    applicationPOMModels
                        .Where(x => WorkSpace.Instance.Solution.GetTargetApplicationPlatform(x.TargetApplicationKey) == GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ePlatformType.Web)
                        .ToList()
                );

                var businessFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GingerCore.BusinessFlow>();
                var multiPomRunSetMappingsList = GingerCoreNET.GeneralLib.General.GetSelectedRunsetList(
                    runSetConfigList, businessFlows, mPOMModels, ApplicationPOMModelrunsetConfigMapping
                );

                foreach (var item in multiPomRunSetMappingsList)
                {
                    await GingerCoreNET.GeneralLib.General.RunSelectedRunset(item, multiPomRunSetMappingsList, mCLIHelper);
                }
                var statuses = multiPomRunSetMappingsList.Select(m => m.PomUpdateStatus);
                Reporter.ToLog(eLogLevel.INFO, $"POM Status: {string.Join(", ", statuses)}");
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, $"With given info POMs or Runsets not found.");
                return;
            }
        }

    }
}
