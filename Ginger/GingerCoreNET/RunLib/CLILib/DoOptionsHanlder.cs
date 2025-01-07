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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Reports;
using Ginger.AnalyzerLib;
using Ginger.Reports;
using Ginger.Run;
using GingerCore;
using GraphQLClient.Clients;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{

    public class DoOptionsHandler
    {
        public static event EventHandler<BusinessFlow> AutomateBusinessFlowEvent;
        public static event EventHandler<RunSetConfig> LoadRunSetConfigEvent;
        public static event EventHandler<string> LoadSharedRepoEvent;
        DoOptions mOpts;
        CLIHelper mCLIHelper = new();
        public async Task RunAsync(DoOptions opts)
        {
            mOpts = opts;
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
                if (!mCLIHelper.LoadSolution())
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to Download/update Solution from source control");
                    return;
                }



                if (!string.IsNullOrWhiteSpace(mOpts.ExecutionId))
                {
                    bool autoLoadLastRunSetFlag = WorkSpace.Instance.UserProfile.AutoLoadLastRunSet;
                    try
                    {
                        string endPoint = GingerRemoteExecutionUtils.GetReportDataServiceUrl();
                        if (!string.IsNullOrEmpty(endPoint))
                        {
                            GraphQlClient graphQlClient = new GraphQlClient($"{endPoint}api/graphql");
                            ExecutionReportGraphQLClient executionReportGraphQLClient = new ExecutionReportGraphQLClient(graphQlClient);
                            var response = await executionReportGraphQLClient.FetchDataBySolutionAndExecutionId(WorkSpace.Instance.Solution.Guid, Guid.Parse(mOpts.ExecutionId));

                            if (response?.Data?.Runsets?.Nodes.Count == 1)
                            {
                                var node = response.Data.Runsets.Nodes.First();

                                if (node.ExecutionId == Guid.Parse(mOpts.ExecutionId))
                                {
                                    var runSetReport = new RunSetReport
                                    {
                                        GUID = node.ExecutionId.ToString(),
                                        RunSetGuid = node.EntityId.Value,
                                        Name = node.Name,
                                    };
                                    ObservableList<RunSetConfig> allRunsets = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>();
                                    RunSetConfig cliRunset = allRunsets.FirstOrDefault(runsets => runsets.Guid == node.EntityId.Value);
                                    if (cliRunset != null)
                                    {
                                        WorkSpace.Instance.UserProfile.RecentRunset = cliRunset.Guid;
                                        WorkSpace.Instance.UserProfile.AutoLoadLastRunSet = true;
                                        LoadRunSetConfigEvent?.Invoke(sender: this, cliRunset);
                                        return;
                                    }
                                    else
                                    {
                                        if (await LoadVirtualRunset())
                                        {
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    if (await LoadVirtualRunset())
                                    {
                                        return;
                                    }
                                }

                            }
                            else
                            {
                                if (await LoadVirtualRunset())
                                {
                                    return;
                                }
                            }
                        }
                        else
                        {
                            if (await LoadVirtualRunset())
                            {
                                return;
                            }
                        }
                        Reporter.ToLog(eLogLevel.ERROR, $"Runset not found by given Execution ID:{mOpts.ExecutionId}");
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Error occurred while connecting remote.", ex);
                    }
                    finally
                    {
                        WorkSpace.Instance.UserProfile.AutoLoadLastRunSet = autoLoadLastRunSetFlag;
                    }
                }
                if (!string.IsNullOrWhiteSpace(mOpts.RunSetId))
                {
                    bool autoLoadLastRunSetFlag = WorkSpace.Instance.UserProfile.AutoLoadLastRunSet;
                    try
                    {
                        ObservableList<RunSetConfig> allRunsets = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>();
                        RunSetConfig cliRunset = allRunsets.FirstOrDefault(runsets => runsets.Guid.ToString() == mOpts.RunSetId);

                        if (cliRunset != null)
                        {
                            WorkSpace.Instance.UserProfile.AutoLoadLastRunSet = true;
                            WorkSpace.Instance.UserProfile.RecentRunset = cliRunset.Guid;
                            LoadRunSetConfigEvent?.Invoke(sender: this, cliRunset);
                            return;
                        }
                        else
                        {
                            Reporter.ToLog(eLogLevel.ERROR, $"Runset not found by given ID:{mOpts.RunSetId}");
                        }
                    }
                    finally
                    {
                        WorkSpace.Instance.UserProfile.AutoLoadLastRunSet = autoLoadLastRunSetFlag;
                    }
                }

                if (!string.IsNullOrWhiteSpace(mOpts.RunSetName))
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
                            return;
                        }
                        else
                        {
                            Reporter.ToLog(eLogLevel.ERROR, $"Runset not found by given Name:{mOpts.RunSetName}");
                        }
                    }
                    finally
                    {
                        WorkSpace.Instance.UserProfile.AutoLoadLastRunSet = autoLoadLastRunSetFlag;
                    }
                }

                if (!string.IsNullOrWhiteSpace(mOpts.BusinessFlowId))
                {
                    var businessFlow = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>()
                        .FirstOrDefault(bf => bf.Guid.ToString().Equals(mOpts.BusinessFlowId, StringComparison.OrdinalIgnoreCase));
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
                    LoadSharedRepoEvent?.Invoke(null, "Dummey");
                    //to do
                }
                if (!string.IsNullOrWhiteSpace(mOpts.SharedActivityName))
                {
                    //to do
                }


            }
            catch (Exception ex)
            {
                // Handle any other unexpected errors
                Reporter.ToLog(eLogLevel.ERROR, $"An unexpected error occurred while opening the solution in folder '{solutionFolder}'. Error: {ex.Message}");
            }
        }

        private async Task<bool> LoadVirtualRunset()
        {
            RunSetReport runsetReport = new() { GUID = mOpts.ExecutionId };
            RunsetFromReportLoader _runsetFromReportLoader = new RunsetFromReportLoader();
            RunSetConfig? runset = await _runsetFromReportLoader.LoadAsync(runsetReport);
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
        public string GetFirstNonNullPropertyName()
        {
            if (!string.IsNullOrWhiteSpace(mOpts.ExecutionId))
            {
                return nameof(mOpts.ExecutionId);
            }
            if (!string.IsNullOrWhiteSpace(mOpts.RunSetId))
            {
                return nameof(mOpts.RunSetId);
            }
            if (!string.IsNullOrWhiteSpace(mOpts.RunSetName))
            {
                return nameof(mOpts.RunSetName);
            }
            if (!string.IsNullOrWhiteSpace(mOpts.BusinessFlowId))
            {
                return nameof(mOpts.BusinessFlowId);
            }
            if (!string.IsNullOrWhiteSpace(mOpts.BusinessFlowName))
            {
                return nameof(mOpts.BusinessFlowName);
            }
            if (!string.IsNullOrWhiteSpace(mOpts.SharedActivityId))
            {
                return nameof(mOpts.SharedActivityId);
            }
            if (!string.IsNullOrWhiteSpace(mOpts.SharedActivityName))
            {
                return nameof(mOpts.SharedActivityName);
            }

            return null;
        }
    }
}
