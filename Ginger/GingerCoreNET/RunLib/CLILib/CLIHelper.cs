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
using Amdocs.Ginger.Repository;
using Ginger.AnalyzerLib;
using Ginger.Reports;
using Ginger.Run;
using Ginger.SourceControl;
using GingerCore;
using GingerCore.Environments;
using GingerCoreNET.SourceControl;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using static Ginger.Reports.ExecutionLoggerConfiguration;
using static GingerCoreNET.SourceControl.SourceControlBase;
using Ginger.Configurations;
using static Ginger.Configurations.SealightsConfiguration;
using Ginger.ExecuterService.Contracts.V1.ExecutionConfiguration;

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

        public bool SelfHealingCheckInConfigured;

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

        public bool LoadSolution()
        {
            try
            {
                Reporter.ToLog(eLogLevel.INFO, "Loading Solution...");
                // SetDebugLevel();//disabling because it is overwriting the UserProfile setting for logging level
                DownloadSolutionFromSourceControl();
                return OpenSolution();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Unexpected error occurred while Loading the Solution", ex);
                return false;
            }
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
                }
                else
                {
                    mRunSetConfig = mRunsetExecutor.RunSetConfig;
                }
                SelectEnv();
                mRunSetConfig.RunWithAnalyzer = RunAnalyzer;
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

        internal void SetTestArtifactsFolder()
        {
            if (!string.IsNullOrEmpty(mTestArtifactsFolder))
            {
                WorkSpace.Instance.TestArtifactsFolder = mTestArtifactsFolder;
            }
        }

        private void SetDebugLevel()
        {
            Reporter.AppLoggingLevel = AppLoggingLevel;
        }

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
            mRunSetConfig = RunSets.Where(x => x.Name.ToLower().Trim() == Runset.ToLower().Trim()).FirstOrDefault();
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
                ProjEnvironment env = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().Where(x => x.Name.ToLower().Trim() == Env.ToLower().Trim()).FirstOrDefault();
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
                if (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().Count > 0)
                {
                    mRunsetExecutor.RunsetExecutionEnvironment = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().First();
                    Reporter.ToLog(eLogLevel.INFO, "Auto Selected the default Environment: '" + mRunsetExecutor.RunsetExecutionEnvironment.Name + "'");
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Cannot auto select default environment since solution do not contain any Environment, please add Environment to the Solution");
                    throw new Exception("Failed to find any Environment in the Solution");
                }
            }
        }

        private void DownloadSolutionFromSourceControl()
        {
            if (SourceControlURL != null && SourcecontrolUser != "" && sourceControlPass != null)
            {
                Reporter.ToLog(eLogLevel.INFO, "Downloading/updating Solution from source control");
                if (!SourceControlIntegration.DownloadSolution(Solution, UndoSolutionLocalChanges))
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to Download/update Solution from source control");
                }
            }
        }

        internal void SetSourceControlBranch(string value)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Selected SourceControlBranch: '" + value + "'");
            WorkSpace.Instance.UserProfile.SolutionSourceControlBranch = value;
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
                    if (SealightsUrl != null) WorkSpace.Instance.Solution.SealightsConfiguration.SealightsURL = SealightsUrl;
                    if (SealightsAgentToken != null) WorkSpace.Instance.Solution.SealightsConfiguration.SealightsAgentToken = SealightsAgentToken;
                    if (SealightsLabID != null) WorkSpace.Instance.Solution.SealightsConfiguration.SealightsLabId = SealightsLabID;
                    if (SealightsSessionID != null) WorkSpace.Instance.Solution.SealightsConfiguration.SealightsBuildSessionID = SealightsSessionID;
                    if (SealightsTestStage != null) WorkSpace.Instance.Solution.SealightsConfiguration.SealightsTestStage = SealightsTestStage;
                    if (SealightsSessionTimeOut != null) WorkSpace.Instance.Solution.SealightsConfiguration.SealightsSessionTimeout = SealightsSessionTimeOut;
                    if (SealightsEntityLevel != null) WorkSpace.Instance.Solution.SealightsConfiguration.SealightsReportedEntityLevel = (SealightsConfiguration.eSealightsEntityLevel)Enum.Parse(typeof(SealightsConfiguration.eSealightsEntityLevel), SealightsEntityLevel);
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
            //Reporter.ToLog(eLogLevel.DEBUG, "Selected SourceControlPassword: '" + value + "'");//we should not show the password in log
            WorkSpace.Instance.UserProfile.SourceControlPass = value;
            sourceControlPass = value;
        }

        internal void SetEncryptionKey(string value)
        {
            EncryptionKey = value;
        }

        internal void PasswordEncrypted(string value)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "PasswordEncrypted: '" + value + "'");
            string pswd = WorkSpace.Instance.UserProfile.SourceControlPass;
            if (value == "Y" || value == "true" || value == "True")
            {
                try
                {
                    pswd = EncryptionHandler.DecryptwithKey(WorkSpace.Instance.UserProfile.SourceControlPass, EncryptionKey);
                }
                catch (Exception ex)
                {
                    string mess = ex.Message; //To avoid warning of ex not used
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to decrypt the source control password");//not showing ex details for not showing the password by mistake in log
                }
            }


            WorkSpace.Instance.UserProfile.SourceControlPass = pswd;
            sourceControlPass = pswd;
        }

        internal void SourceControlProxyPort(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                WorkSpace.Instance.UserProfile.SolutionSourceControlConfigureProxy = false;
            }
            else
            {
                WorkSpace.Instance.UserProfile.SolutionSourceControlConfigureProxy = true;
            }

            Reporter.ToLog(eLogLevel.DEBUG, "Selected SourceControlProxyPort: '" + value + "'");
            WorkSpace.Instance.UserProfile.SolutionSourceControlProxyPort = value;
        }

        internal void SourceControlProxyServer(string value)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Selected SourceControlProxyServer: '" + value + "'");
            if (string.IsNullOrEmpty(value))
            {
                WorkSpace.Instance.UserProfile.SolutionSourceControlConfigureProxy = false;
            }
            else
            {
                WorkSpace.Instance.UserProfile.SolutionSourceControlConfigureProxy = true;
                if (!value.ToUpper().StartsWith("HTTP://"))
                {
                    value = "http://" + value;
                }
            }

            WorkSpace.Instance.UserProfile.SolutionSourceControlProxyAddress = value;
        }

        internal void SetSourceControlUser(string value)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Selected SourceControlUser: '" + value + "'");
            if (WorkSpace.Instance.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.GIT && value == "")
            {
                value = "Test";
            }

            WorkSpace.Instance.UserProfile.SourceControlUser = value;
            SourcecontrolUser = value;
        }

        internal void SetSourceControlURL(string value)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Selected SourceControlUrl: '" + value + "'");
            if (WorkSpace.Instance.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.SVN)
            {
                if (!value.ToUpper().Contains("/SVN") && !value.ToUpper().Contains("/SVN/"))
                {
                    value = value + "svn/";
                }
                if (!value.ToUpper().EndsWith("/"))
                {
                    value = value + "/";
                }
            }
            WorkSpace.Instance.UserProfile.SourceControlURL = value;
            SourceControlURL = value;
        }

        internal void SetSourceControlType(string value)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Selected SourceControlType: '" + value + "'");
            if (value.Equals("GIT"))
            {
                WorkSpace.Instance.UserProfile.SourceControlType = SourceControlBase.eSourceControlType.GIT;
            }
            else if (value.Equals("SVN"))
            {
                WorkSpace.Instance.UserProfile.SourceControlType = SourceControlBase.eSourceControlType.SVN;
            }
            else
            {
                WorkSpace.Instance.UserProfile.SourceControlType = SourceControlBase.eSourceControlType.None;
            }
        }

        private bool OpenSolution()
        {
            try
            {
                return WorkSpace.Instance.OpenSolution(Solution, EncryptionKey);
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
    }
}
