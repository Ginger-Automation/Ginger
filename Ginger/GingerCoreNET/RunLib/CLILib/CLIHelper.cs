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

using AccountReport.Contracts;
using AccountReport.Contracts.ResponseModels;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.GenAIServices;
using Amdocs.Ginger.CoreNET.Run.RunListenerLib.CenteralizedExecutionLogger;
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.AnalyzerLib;
using Ginger.Configurations;
using Ginger.ExecuterService.Contracts.V1.ExecutionConfiguration;
using Ginger.Reports;
using Ginger.Run;
using Ginger.SourceControl;
using GingerCore;
using GingerCore.Environments;
using GingerCoreNET.SourceControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static GingerCoreNET.SourceControl.SourceControlBase;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public enum eCLIType
    {
        Config, Dynamic, Script, Arguments
    }

    public class CLIHelper : INotifyPropertyChanged
    {
        public string Solution;
        public string Env;
        public string Runset;
        public string SourceControlURL;
        public string SourcecontrolUser;
        public string sourceControlPass;
        public string EncryptionKey;
        public eSourceControlType sourceControlType;
        public bool sourceControlPassEncrypted;
        public eAppReporterLoggingLevel AppLoggingLevel;
        public string ExecutionId;

        public bool SealightsEnable;
        public string SealightsUrl;
        public string SealightsAgentToken;
        public string SealightsLabID;
        public string SealightsSessionID;
        public string SealightsSessionTimeOut;
        public string SealightsTestStage;
        public string SealightsEntityLevel;
        public bool SealightsTestRecommendations;

        public bool ReRunFailed;
        public string ReferenceExecutionID;
        public string RerunLevel;
        public string SourceApplication;
        public string SourceApplicationUser;

        ProgressNotifier progressNotifier = new();
        ProgressStatus progressStatus;
        public bool SelfHealingCheckInConfigured;
        public static event EventHandler<string> GitProgresStatus;
        bool mShowAutoRunWindow; // default is false except in ConfigFile which is true to keep backward compatibility        
        public bool ShowAutoRunWindow
        {
            get
            {
                return mShowAutoRunWindow;
            }
            set
            {
                mShowAutoRunWindow = value;
                OnPropertyChanged(nameof(ShowAutoRunWindow));
            }
        }

        bool mDownloadUpgradeSolutionFromSourceControl;
        public bool DownloadUpgradeSolutionFromSourceControl
        {
            get
            {
                return mDownloadUpgradeSolutionFromSourceControl;
            }
            set
            {
                mDownloadUpgradeSolutionFromSourceControl = value;
                OnPropertyChanged(nameof(DownloadUpgradeSolutionFromSourceControl));
            }
        }

        /// <summary>
        /// To include Global variables used in Runset in CLI dynamic JSON configuration
        /// </summary>
        bool mGlobalVariableConfiguration;
        public bool GlobalVariableConfiguration
        {
            get
            {
                return mGlobalVariableConfiguration;
            }
            set
            {
                mGlobalVariableConfiguration = value;
                OnPropertyChanged(nameof(GlobalVariableConfiguration));
            }
        }


        bool mSetEnvironmentDetails;
        public bool SetEnvironmentDetails
        {
            get
            {
                return mSetEnvironmentDetails;
            }
            set
            {
                mSetEnvironmentDetails = value;
                OnPropertyChanged(nameof(SetEnvironmentDetails));
            }
        }

        bool mAgentDetails;
        public bool SetAgentDetails
        {
            get
            {
                return mAgentDetails;
            }
            set
            {
                mAgentDetails = value;
                OnPropertyChanged(nameof(SetAgentDetails));
            }
        }

        bool mSetAlmConnectionDetails;
        public bool SetAlmConnectionDetails
        {
            get
            {
                return mSetAlmConnectionDetails;
            }
            set
            {
                mSetAlmConnectionDetails = value;
                OnPropertyChanged(nameof(SetAlmConnectionDetails));
            }
        }

        bool mSetSealightsSettings;
        public bool SetSealightsSettings
        {
            get
            {
                return mSetSealightsSettings;
            }
            set
            {
                mSetSealightsSettings = value;
                OnPropertyChanged(nameof(SetSealightsSettings));
            }
        }



        bool mRunAnalyzer;
        public bool RunAnalyzer
        {
            get
            {
                return mRunAnalyzer;
            }
            set
            {
                mRunAnalyzer = value;
                OnPropertyChanged(nameof(RunAnalyzer));
            }
        }

        string mTestArtifactsFolder;
        public string TestArtifactsFolder
        {
            get
            {
                return mTestArtifactsFolder;
            }
            set
            {
                mTestArtifactsFolder = value;
                OnPropertyChanged(nameof(TestArtifactsFolder));
            }
        }

        bool mUndoSolutionLocalChanges;
        public bool UndoSolutionLocalChanges
        {
            get
            {
                return mUndoSolutionLocalChanges;
            }
            set
            {
                mUndoSolutionLocalChanges = value;
                OnPropertyChanged(nameof(UndoSolutionLocalChanges));
            }
        }


        RunsetExecutor mRunsetExecutor;
        //UserProfile WorkSpace.Instance.UserProfile;
        RunSetConfig mRunSetConfig;

        /// <summary>
        /// Adds CLI Git properties from the provided SourceControlOptions.
        /// </summary>
        /// <param name="runOptions">The SourceControlOptions containing the Git properties.</param>
        internal void AddCLIGitProperties(SourceControlOptions runOptions)
        {
            SourceControlURL = runOptions.URL;
            SourcecontrolUser = runOptions.User;
            sourceControlType = runOptions.SCMType;
            SetSourceControlBranch(runOptions.Branch);
            sourceControlPass = runOptions.Pass;
            sourceControlPassEncrypted = runOptions.PasswordEncrypted;
            SourceControlProxyServer(runOptions.SourceControlProxyServer);
            SourceControlProxyPort(runOptions.SourceControlProxyPort);
        }


        /// <summary>
        /// Sets the workspace Git properties from the provided SourceControlOptions.
        /// </summary>
        /// <param name="runOptions">The SourceControlOptions containing the Git properties.</param>
        internal void SetWorkSpaceGitProperties(SourceControlOptions runOptions)
        {
            if (WorkSpace.Instance.UserProfile == null)
            {
                WorkSpace.Instance.UserProfile = new UserProfile();
                UserProfileOperations userProfileOperations = new UserProfileOperations(WorkSpace.Instance.UserProfile);
                WorkSpace.Instance.UserProfile.UserProfileOperations = userProfileOperations;
            }
            WorkSpace.Instance.UserProfile.URL = runOptions.URL;
            WorkSpace.Instance.UserProfile.Username = runOptions.User;
            WorkSpace.Instance.UserProfile.Type = runOptions.SCMType;
            WorkSpace.Instance.UserProfile.UserProfileOperations.SourceControlIgnoreCertificate = runOptions.ignoreCertificate;
            WorkSpace.Instance.UserProfile.UserProfileOperations.SourceControlUseShellClient = runOptions.useScmShell;
            WorkSpace.Instance.UserProfile.EncryptedPassword = runOptions.Pass;
            WorkSpace.Instance.UserProfile.Password = runOptions.Pass;
        }


        /// <summary>
        /// Loads the solution asynchronously, optionally preventing the saving of CLI Git credentials.
        /// </summary>
        /// <param name="doNotSaveCLIGitCredentials">If set to true, prevents saving CLI Git credentials.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.</returns>
        public async Task<bool> LoadSolutionAsync(bool doNotSaveCLIGitCredentials = false)
        {
            try
            {
                Reporter.ToLog(eLogLevel.INFO, "Loading Solution...");
                await DownloadSolutionFromSourceControl();
                //this workspace flag is used to prevent to store credentials on user profile which comes form deeplink
                var doNotSaveDeeplinkCredentials = WorkSpace.Instance.UserProfile.DoNotSaveCredentialsOnUserProfile;
                //if this flag is true we setting it as false, so while opening solution existing credentials will be loaded.
                if (WorkSpace.Instance.UserProfile.DoNotSaveCredentialsOnUserProfile)
                {
                    WorkSpace.Instance.UserProfile.DoNotSaveCredentialsOnUserProfile = false;
                }
                var isSolutionOpened = OpenSolution();
                if (isSolutionOpened && !doNotSaveDeeplinkCredentials && !doNotSaveCLIGitCredentials)
                {
                    SetSourceControlParaOnUserProfile();
                }
                return isSolutionOpened;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Unexpected error occurred while opening the Solution", ex);
                return false;
            }
        }

        void SetSourceControlParaOnUserProfile()
        {

            WorkSpace.Instance.UserProfile.RecentDownloadedSolutionGuid = WorkSpace.Instance.Solution.Guid;
            var SetSourceControlParaOnUserProfile = WorkSpace.Instance.UserProfile.GetSolutionSourceControlInfo(WorkSpace.Instance.Solution.Guid);
            SetSourceControlParaOnUserProfile.SourceControlInfo.Type = WorkSpace.Instance.UserProfile.Type;
            SetSourceControlParaOnUserProfile.SourceControlInfo.Url = WorkSpace.Instance.UserProfile.URL;
            SetSourceControlParaOnUserProfile.SourceControlInfo.Username = WorkSpace.Instance.UserProfile.Username;
            SetSourceControlParaOnUserProfile.SourceControlInfo.Password = WorkSpace.Instance.UserProfile.Password;
            SetSourceControlParaOnUserProfile.SourceControlInfo.Branch = WorkSpace.Instance.UserProfile.Branch;
            if (Solution.Contains(".git", StringComparison.OrdinalIgnoreCase))
            {

                SetSourceControlParaOnUserProfile.SourceControlInfo.LocalFolderPath = Solution.Substring(0, Solution.LastIndexOf('\\'));
            }
            else
            {
                SetSourceControlParaOnUserProfile.SourceControlInfo.LocalFolderPath = Solution;
            }
            WorkSpace.Instance.UserProfile.UserProfileOperations.SaveUserProfile();
        }



        public bool LoadRunset(RunsetExecutor runsetExecutor)
        {
            try
            {
                Reporter.ToLog(eLogLevel.INFO, string.Format("Loading {0}", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                mRunsetExecutor = runsetExecutor;
                if (mRunsetExecutor.RunSetConfig == null)
                {
                    SelectRunset();
                    mRunSetConfig.ReRunConfigurations.Active = ReRunFailed;
                    if (ReRunFailed)
                    {
                        mRunSetConfig.ReRunConfigurations.ReferenceExecutionID = Guid.Parse(ReferenceExecutionID);
                        mRunSetConfig.ReRunConfigurations.RerunLevel = (eReRunLevel)Enum.Parse(typeof(eReRunLevel), RerunLevel);
                    }
                }
                else
                {
                    mRunSetConfig = mRunsetExecutor.RunSetConfig;
                }
                SelectEnv();
                mRunSetConfig.RunWithAnalyzer = RunAnalyzer;
                if (mRunSetConfig.ReRunConfigurations != null && mRunSetConfig.ReRunConfigurations.Active)
                {
                    if (mRunSetConfig.ReRunConfigurations.ReferenceExecutionID == Guid.Empty || mRunSetConfig.ReRunConfigurations.ReferenceExecutionID == null)
                    {
                        Reporter.ToLog(eLogLevel.INFO, $"ReferenceExecutionId is empty,so checking for recent ExecutionId from Centerlized Report Service");
                        mRunSetConfig.ReRunConfigurations.ReferenceExecutionID = GetLastExecutionIdBySolutionAndRunsetId(WorkSpace.Instance.Solution.Guid, mRunSetConfig.Guid);
                    }
                    bool result = CheckforReRunConfig();
                    if (!result)
                    {
                        return result;
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.INFO, $"Using ReferenceExecutionId for Re run = {mRunSetConfig.ReRunConfigurations.ReferenceExecutionID}");
                    }
                }

                HandleAutoRunWindow();

                mRunSetConfig.SelfHealingConfiguration = mRunsetExecutor.RunSetConfig.SelfHealingConfiguration;

                if (!string.IsNullOrEmpty(ExecutionId))
                {
                    WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID = Guid.Parse(ExecutionId);
                }
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, string.Format("Unexpected error occurred while loading the {0}", GingerDicser.GetTermResValue(eTermResKey.RunSet)), ex);
                return false;
            }
        }

        public Guid GetLastExecutionIdBySolutionAndRunsetId(Guid soluionGuid, Guid runsetGuid)
        {
            List<RunSetReport> runsetsReport = [];
            runsetsReport = new GingerRemoteExecutionUtils().GetRunsetExecutionInfo(soluionGuid, runsetGuid);
            return runsetsReport != null ? Guid.Parse(runsetsReport.FirstOrDefault().GUID) : Guid.Empty;
        }


        public void PostExecution()
        {
            if (ShowAutoRunWindow)
            {
                TargetFrameworkHelper.Helper.WaitForAutoRunWindowClose();
            }
        }

        public bool PrepareRunsetForExecution()
        {
            try
            {
                Reporter.ToLog(eLogLevel.INFO, string.Format("Preparing {0} for Execution", GingerDicser.GetTermResValue(eTermResKey.RunSet)));

                if (!ShowAutoRunWindow)
                {
                    Reporter.ToLog(eLogLevel.INFO, string.Format("Loading {0} Runners", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                    mRunsetExecutor.InitRunners();
                }

                if (mRunSetConfig.RunWithAnalyzer)
                {
                    Reporter.ToLog(eLogLevel.INFO, string.Format("Running {0} Analyzer", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                    AnalyzerUtils analyzerUtils = new AnalyzerUtils();
                    if (analyzerUtils.AnalyzeRunset(mRunSetConfig, true))
                    {
                        Reporter.ToLog(eLogLevel.WARN, string.Format("Stopping {0} execution due to Analyzer issues", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, string.Format("Unexpected error occurred while preparing {0} for Execution", GingerDicser.GetTermResValue(eTermResKey.RunSet)), ex);
                return false;
            }
        }

        private bool CheckforReRunConfig()
        {
            bool Result = true;

            if (mRunSetConfig.ReRunConfigurations.ReferenceExecutionID != null)
            {
                if (WorkSpace.Instance.Solution.LoggerConfigurations.PublishLogToCentralDB == ExecutionLoggerConfiguration.ePublishToCentralDB.Yes
                    && !string.IsNullOrEmpty(GingerPlayEndPointManager.GetAccountReportServiceUrl()))
                {
                    AccountReportApiHandler accountReportApiHandler = new AccountReportApiHandler(GingerPlayEndPointManager.GetAccountReportServiceUrl());
                    if (mRunSetConfig.ReRunConfigurations.RerunLevel == eReRunLevel.RunSet)
                    {
                        List<RunsetHLInfoResponse> accountReportRunset = accountReportApiHandler.GetRunsetExecutionDataFromCentralDB((Guid)mRunSetConfig.ReRunConfigurations.ReferenceExecutionID);
                        if (accountReportRunset != null && accountReportRunset.Count > 0)
                        {
                            if (accountReportRunset.Any(x => !x.Status.Equals(AccountReport.Contracts.Enum.eExecutionStatus.Failed.ToString(), StringComparison.CurrentCultureIgnoreCase)))
                            {
                                Reporter.ToLog(eLogLevel.INFO, string.Format("The Runset is already Passed or In_progress for provided reference execution id: {0}", mRunSetConfig.ReRunConfigurations.ReferenceExecutionID));
                                Result = false;
                            }
                        }
                        else
                        {
                            Reporter.ToLog(eLogLevel.INFO, string.Format("No record found to re run for reference execution id: {0}", mRunSetConfig.ReRunConfigurations.ReferenceExecutionID));
                            Result = false;
                        }
                    }
                    else if (mRunSetConfig.ReRunConfigurations.RerunLevel == eReRunLevel.Runner)
                    {
                        List<AccountReportRunner> accountReportRunnerList = accountReportApiHandler.GetRunnerExecutionDataFromCentralDB((Guid)mRunSetConfig.ReRunConfigurations.ReferenceExecutionID);
                        if (accountReportRunnerList != null)
                        {
                            if (accountReportRunnerList.Any(x => x.RunStatus.Equals(AccountReport.Contracts.Enum.eExecutionStatus.Failed)))
                            {
                                var FailedRunnerGuidList = accountReportRunnerList.Where(x => x.RunStatus.Equals(AccountReport.Contracts.Enum.eExecutionStatus.Failed)).Select(x => x.EntityId);
                                foreach (GingerRunner runner in mRunsetExecutor.RunSetConfig.GingerRunners)
                                {
                                    if (!FailedRunnerGuidList.Contains(runner.Guid))
                                    {
                                        runner.Active = false;
                                    }
                                }
                            }
                            else
                            {
                                Reporter.ToLog(eLogLevel.INFO, string.Format("All runner are already Pass, in reference execution id: {0}", mRunSetConfig.ReRunConfigurations.ReferenceExecutionID));
                                Result = false;
                            }
                        }
                        else
                        {
                            Reporter.ToLog(eLogLevel.INFO, string.Format("No record found to re run for reference execution id: {0}", mRunSetConfig.ReRunConfigurations.ReferenceExecutionID));
                            Result = false;
                        }
                    }
                    else if (mRunSetConfig.ReRunConfigurations.RerunLevel == eReRunLevel.BusinessFlow)
                    {
                        List<AccountReportBusinessFlow> accountReportBusinessFlows = accountReportApiHandler.GetBusinessflowExecutionDataFromCentralDB((Guid)mRunSetConfig.ReRunConfigurations.ReferenceExecutionID);
                        if (accountReportBusinessFlows != null && accountReportBusinessFlows.Count > 0)
                        {
                            if (accountReportBusinessFlows.Any(x => x.RunStatus.Equals(AccountReport.Contracts.Enum.eExecutionStatus.Failed)))
                            {

                                var FailedBFGuidList = accountReportBusinessFlows.Where(x => x.RunStatus.Equals(AccountReport.Contracts.Enum.eExecutionStatus.Failed)).Select(x => x.InstanceGUID);
                                foreach (GingerRunner runner in mRunsetExecutor.RunSetConfig.GingerRunners)
                                {
                                    foreach (BusinessFlowRun business in runner.BusinessFlowsRunList)
                                    {
                                        if (!FailedBFGuidList.Contains(business.BusinessFlowInstanceGuid))
                                        {
                                            business.BusinessFlowIsActive = false;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Reporter.ToLog(eLogLevel.INFO, string.Format("All flows are already Passed, in reference execution id: {0}", mRunSetConfig.ReRunConfigurations.ReferenceExecutionID));
                                Result = false;
                            }
                        }
                        else
                        {
                            Reporter.ToLog(eLogLevel.INFO, string.Format("No record found to re run for reference execution id: {0}", mRunSetConfig.ReRunConfigurations.ReferenceExecutionID));
                            Result = false;
                        }
                    }
                }
                else
                {
                    Reporter.ToLog(eLogLevel.INFO, string.Format("Account report is not configured PublishLogToCentralDB: {0}", WorkSpace.Instance.Solution.LoggerConfigurations.PublishLogToCentralDB));
                    Result = false;
                }
            }
            else
            {
                Reporter.ToLog(eLogLevel.INFO, string.Format("Reference execution id is empty: {0}", mRunSetConfig.ReRunConfigurations.ReferenceExecutionID));
                Result = false;
            }

            return Result;
        }
        internal void SetTestArtifactsFolder()
        {
            if (!string.IsNullOrEmpty(mTestArtifactsFolder))
            {
                WorkSpace.Instance.TestArtifactsFolder = mTestArtifactsFolder;
            }
        }

        //private void SetDebugLevel()
        //{
        //    Reporter.AppLoggingLevel = AppLoggingLevel;
        //}

        private void HandleAutoRunWindow()
        {
            if (ShowAutoRunWindow)
            {
                Reporter.ToLog(eLogLevel.INFO, "Showing Auto Run Window");
                TargetFrameworkHelper.Helper.ShowAutoRunWindow();
            }
            else
            {
                Reporter.ToLog(eLogLevel.INFO, "Not Showing Auto Run Window");
            }
        }


        private void SelectRunset()
        {
            Reporter.ToLog(eLogLevel.INFO, string.Format("Selected {0}: '{1}'", GingerDicser.GetTermResValue(eTermResKey.RunSet), Runset));
            ObservableList<RunSetConfig> RunSets = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>();
            mRunSetConfig = RunSets.FirstOrDefault(x => x.Name.ToLower().Trim() == Runset.ToLower().Trim());
            if (mRunSetConfig != null)
            {
                mRunsetExecutor.RunSetConfig = mRunSetConfig;
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to find matching {0} in the Solution", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                Reporter.ToUser(eUserMsgKey.CannotRunShortcut);
                // TODO: throw
                // return false;
            }
        }

        private void SelectEnv()
        {
            Reporter.ToLog(eLogLevel.INFO, "Selected Environment: '" + Env + "'");
            if (String.IsNullOrEmpty(Env) == false)
            {
                ProjEnvironment env = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().FirstOrDefault(x => x.Name.ToLower().Trim() == Env.ToLower().Trim());
                if (env != null)
                {
                    mRunsetExecutor.RunsetExecutionEnvironment = env;
                    return;
                }
                else
                {
                    if (Env != "Default")
                    {
                        Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to find the Environment '{0}' in the Solution", Env));
                        throw new Exception(string.Format("Failed to find the Environment '{0}' in the Solution", Env));
                    }
                }
            }

            if (mRunsetExecutor.RunsetExecutionEnvironment == null && (String.IsNullOrEmpty(Env) || Env == "Default"))
            {
                if (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().Any())
                {
                    mRunsetExecutor.RunsetExecutionEnvironment = WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<ProjEnvironment>();
                    Reporter.ToLog(eLogLevel.INFO, $"Auto Selected the default Environment: '{mRunsetExecutor.RunsetExecutionEnvironment.Name}'");
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Cannot auto select default environment since solution do not contain any Environment, please add Environment to the Solution");
                    throw new Exception("Failed to find any Environment in the Solution");
                }
            }
        }

        private async Task DownloadSolutionFromSourceControl()
        {
            try
            {
                progressNotifier.StatusUpdateHandler += ProgressNotifier_ProgressUpdated;
                progressStatus = new();
                if (!string.IsNullOrEmpty(SourceControlURL) && !string.IsNullOrEmpty(SourcecontrolUser) && !string.IsNullOrEmpty(sourceControlPass))
                {
                    Reporter.ToLog(eLogLevel.INFO, "Downloading/updating Solution from source control");
                    bool solutionDownloadedSuccessfully = false;

                    if (WorkSpace.Instance.GingerCLIMode == eGingerCLIMode.run)
                    {
                        solutionDownloadedSuccessfully = SourceControlIntegration.DownloadSolution(Solution, UndoSolutionLocalChanges, progressNotifier);
                    }
                    else
                    {
                        solutionDownloadedSuccessfully = await Task.Run(() => SourceControlIntegration.DownloadSolution(Solution, UndoSolutionLocalChanges, progressNotifier));
                    }

                    if (!solutionDownloadedSuccessfully)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to Download/update Solution from source control");
                    }
                }
                Reporter.ToLog(eLogLevel.INFO, "Solution downloaded/updated successfully");
            }
            catch (Exception)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to Download/update Solution from source control");
            }
            finally
            {
                progressStatus = null;
                progressNotifier.StatusUpdateHandler -= ProgressNotifier_ProgressUpdated;
            }
        }


        /// <summary>
        /// Updates the progress of the download and logs the progress percentage.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A tuple containing the number of completed steps and the total number of steps.</param>
        private void ProgressNotifier_ProgressUpdated(object sender, (string ProgressType, int CompletedSteps, int TotalSteps) e)
        {
            try
            {
                double progress = Math.Round(((double)e.CompletedSteps / e.TotalSteps) * 100, 2);
                if (e.CompletedSteps > 0 && e.TotalSteps > 0 && e.CompletedSteps <= e.TotalSteps)
                {
                    const double epsilon = 0.0001;
                    if (progressStatus == null || Math.Abs(progress) < epsilon)
                    {
                        return;
                    }
                    string gitProgress = $"{e.ProgressType}{progress:F2}% ";
                    progressStatus.ProgressMessage = gitProgress;
                    progressStatus.ProgressStep = e.CompletedSteps;
                    progressStatus.TotalSteps = e.TotalSteps;
                    Reporter.ToLog(eLogLevel.INFO, null, progressInformer: progressStatus);
                    GitProgresStatus?.Invoke(this, gitProgress);
                }
                else
                {
                    return;
                }
            }
            catch (Exception t)
            {
                Reporter.ToLog(eLogLevel.ERROR, t.Message);
            }
        }
        internal void SetSourceControlBranch(string value)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Selected SourceControlBranch: '{value}'");
            WorkSpace.Instance.UserProfile.Branch = value;
        }

        public bool SetSealights()
        {
            WorkSpace.Instance.Solution.SealightsConfiguration.SealightsLog = SealightsEnable ? SealightsConfiguration.eSealightsLog.Yes : SealightsConfiguration.eSealightsLog.No;

            if (SealightsEnable)
            {
                // Validation
                if (SealightsUrl != null && SealightsAgentToken != null && (SealightsLabID != null || SealightsSessionID != null) &&
                    SealightsTestStage != null && SealightsEntityLevel != null)
                {
                    // Override the Sealights Solution's settings with the CLI's settings
                    if (SealightsUrl != null)
                    {
                        WorkSpace.Instance.Solution.SealightsConfiguration.SealightsURL = SealightsUrl;
                    }

                    if (SealightsAgentToken != null)
                    {
                        WorkSpace.Instance.Solution.SealightsConfiguration.SealightsAgentToken = SealightsAgentToken;
                    }

                    if (SealightsLabID != null)
                    {
                        WorkSpace.Instance.Solution.SealightsConfiguration.SealightsLabId = SealightsLabID;
                    }

                    if (SealightsSessionID != null)
                    {
                        WorkSpace.Instance.Solution.SealightsConfiguration.SealightsBuildSessionID = SealightsSessionID;
                    }

                    if (SealightsTestStage != null)
                    {
                        WorkSpace.Instance.Solution.SealightsConfiguration.SealightsTestStage = SealightsTestStage;
                    }

                    if (SealightsSessionTimeOut != null)
                    {
                        WorkSpace.Instance.Solution.SealightsConfiguration.SealightsSessionTimeout = SealightsSessionTimeOut;
                    }

                    if (SealightsEntityLevel != null)
                    {
                        WorkSpace.Instance.Solution.SealightsConfiguration.SealightsReportedEntityLevel = (SealightsConfiguration.eSealightsEntityLevel)Enum.Parse(typeof(SealightsConfiguration.eSealightsEntityLevel), SealightsEntityLevel);
                    }

                    WorkSpace.Instance.Solution.SealightsConfiguration.SealightsTestRecommendations = SealightsTestRecommendations ? SealightsConfiguration.eSealightsTestRecommendations.Yes : SealightsConfiguration.eSealightsTestRecommendations.No;

                    // Override the Sealights RunSet's settings with the CLI's settings
                    WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsLabId = SealightsLabID;
                    WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsBuildSessionID = SealightsSessionID;
                    WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsTestStage = SealightsTestStage;
                    WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsTestRecommendations = SealightsTestRecommendations ? SealightsConfiguration.eSealightsTestRecommendations.Yes : SealightsConfiguration.eSealightsTestRecommendations.No;

                    if (WorkSpace.Instance.Solution.SealightsConfiguration.SealightsSessionTimeout == null)
                    {
                        WorkSpace.Instance.Solution.SealightsConfiguration.SealightsBuildSessionID = "14400"; // Default setting
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        internal void SetSourceControlPassword(string value)
        {
            WorkSpace.Instance.UserProfile.Password = value;
            sourceControlPass = value;
        }

        internal void SetEncryptionKey(string value)
        {
            EncryptionKey = value;
        }

        internal void PasswordEncrypted(string value)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"PasswordEncrypted: '{value}'");
            var pswd = WorkSpace.Instance.UserProfile.Password;
            if (value is "Y" or "true" or "True")
            {
                try
                {
                    pswd = EncryptionHandler.DecryptwithKey(pswd, EncryptionKey);
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to decrypt the source control password" + ex.Message);//not showing ex details for not showing the password by mistake in log
                }
            }


            WorkSpace.Instance.UserProfile.Password = pswd;
            sourceControlPass = pswd;
        }

        internal void SourceControlProxyPort(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                WorkSpace.Instance.UserProfile.IsProxyConfigured = false;
            }
            else
            {
                WorkSpace.Instance.UserProfile.IsProxyConfigured = true;
            }

            Reporter.ToLog(eLogLevel.DEBUG, $"Selected SourceControlProxyPort: '{value}'");
            WorkSpace.Instance.UserProfile.ProxyPort = value;
        }

        internal void SourceControlProxyServer(string value)
        {

            Reporter.ToLog(eLogLevel.DEBUG, $"Selected SourceControlProxyServer: '{value}'");
            if (string.IsNullOrEmpty(value))
            {
                WorkSpace.Instance.UserProfile.IsProxyConfigured = false;

            }
            else
            {
                WorkSpace.Instance.UserProfile.IsProxyConfigured = true;
                if (!value.StartsWith("HTTP://", StringComparison.CurrentCultureIgnoreCase))
                {
                    value = "http://" + value;
                }
            }

            WorkSpace.Instance.UserProfile.ProxyAddress = value;
        }

        internal void SetSourceControlUser(string value)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Selected SourceControlUser: '{value}'");
            if (WorkSpace.Instance.UserProfile.Type == SourceControlBase.eSourceControlType.GIT && value == "")
            {
                value = "Test";
            }

            WorkSpace.Instance.UserProfile.Username = value;
            SourcecontrolUser = value;
        }

        internal void SetSourceControlURL(string value)
        {
            Reporter.ToLog(eLogLevel.DEBUG, $"Selected SourceControlUrl: '{value}'");

            if (WorkSpace.Instance.UserProfile.Type == SourceControlBase.eSourceControlType.SVN)
            {
                if (!value.ToUpper().Contains("/SVN") && !value.ToUpper().Contains("/SVN/"))
                {
                    value = $"{value}svn/";
                }
                if (!value.ToUpper().EndsWith("/"))
                {
                    value = $"{value}/";
                }
            }
            WorkSpace.Instance.UserProfile.URL = value;
            SourceControlURL = value;
        }

        internal void SetSourceControlType(string value)
        {

            Reporter.ToLog(eLogLevel.DEBUG, $"Selected SourceControlType: '{value}'");
            if (value.Equals("GIT"))
            {
                WorkSpace.Instance.UserProfile.Type = SourceControlBase.eSourceControlType.GIT;
            }
            else if (value.Equals("SVN"))
            {
                WorkSpace.Instance.UserProfile.Type = SourceControlBase.eSourceControlType.SVN;
            }
            else
            {
                WorkSpace.Instance.UserProfile.Type = SourceControlBase.eSourceControlType.None;
            }
        }

        private bool OpenSolution()
        {
            try
            {
                if (Solution != null)
                {
                    return WorkSpace.Instance.OpenSolution(Solution, EncryptionKey);
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to load the Solution, Solution path is empty");
                    return false;
                }

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to load the Solution");
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                // TODO: throw
                return false;
            }
        }

        public void CloseSolution()
        {
            try
            {
                WorkSpace.Instance.CloseSolution();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Unexpected Error occurred while closing the Solution", ex);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        internal void SaveAndCommitSelfHealingChanges()
        {
            WorkSpace.Instance.Solution.SolutionOperations.SaveSolution();

            //TODO: We don't have save all option yet. iterating each item then save it. So, need to add save all option on solution level
            var POMs = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>();

            foreach (ApplicationPOMModel pom in POMs)
            {
                if (pom.DirtyStatus == Common.Enums.eDirtyStatus.Modified)
                {
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(pom);
                }
            }

            var BFs = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
            foreach (var bf in BFs)
            {
                if (bf.DirtyStatus == Common.Enums.eDirtyStatus.Modified)
                {
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(bf);
                }
            }

            if (!SourceControlIntegration.CommitSelfHealingChanges(Solution))
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to Check-in self healing changes in source control");
            }
        }
        internal void SetSealightsEnable(bool? value)
        {
            if (value != null)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Selected SealightsEnable: '" + value + "'");
                SealightsEnable = (bool)value;
            }
        }
        internal void SetSealightsUrl(string value)
        {
            SealightsUrl = value;
        }
        internal void SetSealightsAgentToken(string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                SealightsAgentToken = value;
            }
        }
        internal void SetSealightsLabID(string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Selected SealightsLabID: '" + value + "'");
                SealightsLabID = value;
            }
        }
        internal void SetSealightsBuildSessionID(string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Selected SealightsBSId: '" + value + "'");
                SealightsSessionID = value;
            }
        }
        internal void SetSealightsSessionTimeout(int? value)
        {
            if (value != null)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Selected SealightsSessionTimeout: '" + value.ToString() + "'");
                SealightsSessionTimeOut = value.ToString();
            }
            else
            {
                SealightsSessionTimeOut = "14400";
            }
        }
        internal void SetSealightsTestStage(string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Selected SealightsTestStage: '" + value + "'");
                SealightsTestStage = value;
            }
        }
        internal void SetSealightsEntityLevel(SealightsDetails.eSealightsEntityLevel? value)
        {
            if (value != null)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Selected SealightsEntityLevel: '" + value + "'");
                SealightsEntityLevel = (Enum.GetName(typeof(SealightsDetails.eSealightsEntityLevel), value));
                return;
            }
            SealightsEntityLevel = Enum.GetName(typeof(SealightsDetails.eSealightsEntityLevel), default(SealightsDetails.eSealightsEntityLevel));
        }
        internal void SetSealightsTestRecommendations(bool? value)
        {
            if (value != null)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Selected SealightsTestRecommendations: '" + value + "'");
                SealightsTestRecommendations = (bool)value;
            }
        }

        /// <summary>
        /// Sets the source application and user in the RunSetConfig object.
        /// If the SourceApplication property is empty, it sets it to "Ginger CLI".
        /// If the SourceApplicationUser property is empty, it sets it to the current user's username.
        /// </summary>
        internal void SetSourceAppAndUser()
        {
            if (string.IsNullOrEmpty(mRunSetConfig.SourceApplication))
            {
                mRunSetConfig.SourceApplication = string.IsNullOrEmpty(this.SourceApplication) ? "Ginger CLI" : this.SourceApplication;
            }
            if (string.IsNullOrEmpty(mRunSetConfig.SourceApplicationUser))
            {
                mRunSetConfig.SourceApplicationUser = string.IsNullOrEmpty(this.SourceApplicationUser) ? System.Environment.UserName : this.SourceApplicationUser;
            }
        }

        /// <summary>
        /// Constructs a temporary folder path by appending the repository name (extracted from the source control URL) 
        /// to the system's temporary directory.
        /// </summary>
        /// <param name="sourceControlUrl"> The source control URL used to extract the repository name. If null, the instance's SourceControlURL property will be used. </param>
        /// <returns>
        /// A string representing the path to a temporary folder, combining the system's temp directory 
        /// with the repository name extracted from the source control URL.
        /// </returns>
        public string GetTempFolderPathForRepo(string sourceControlUrl = null)
        {
            string urlToUse = sourceControlUrl ?? SourceControlURL;
            string repoName = string.Empty;

            if (!string.IsNullOrEmpty(urlToUse))
            {
                // Remove trailing slash if present
                urlToUse = urlToUse.TrimEnd('/');

                repoName = Path.GetFileNameWithoutExtension(urlToUse.Split('/').Last());
            }

            return Path.Combine(Path.GetTempPath(), repoName);
        }
    }
}
