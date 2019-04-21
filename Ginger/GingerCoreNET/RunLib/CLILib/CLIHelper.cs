using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger;
using Ginger.Run;
using GingerCore;
using GingerCore.Environments;
using GingerCoreNET.SourceControl;
using System;
using System.Linq;
using System.Reflection;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public class CLIHelper
    {
        static readonly string ENCRYPTION_KEY = "D3^hdfr7%ws4Kb56=Qt";//????? !!!!!!!!!!!!!!!!!!!

        public string Solution;
        public string Env;
        public string Runset;
        public string scURL;
        public string scUser;
        public string scPswd;
        public bool ShowAutoRunWindow = false; // default is false except in ConfigFile which is true to keep backword compatibility
        public bool RunAnalyzer { get; internal set; }

        RunsetExecutor mRunsetExecutor;
        UserProfile mUserProfile;        

        public void ProcessArgs(RunsetExecutor runsetExecutor)
        {
            mRunsetExecutor = runsetExecutor;
            DownloadSolutionFromSourceControl();
            OpenSolution();
            SelectEnv();
            SelectRunset();
            SetRunAnalyzer();
            HandleAutoRunWindow();
        }

        private void HandleAutoRunWindow()
        {
            if(ShowAutoRunWindow)
            {
                RepositoryItemHelper.RepositoryItemFactory.ShowAutoRunWindow();
            }
        }

        private void SetRunAnalyzer()
        {
            mRunsetExecutor.RunSetConfig.RunWithAnalyzer = RunAnalyzer;
        }

        private void SelectRunset()
        {            
            Reporter.ToLog(eLogLevel.DEBUG, string.Format("Selected {0}: '{1}'", GingerDicser.GetTermResValue(eTermResKey.RunSet), Runset));
            ObservableList<RunSetConfig> RunSets = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>();
            RunSetConfig runSetConfig = RunSets.Where(x => x.Name.ToLower().Trim() == Runset.ToLower().Trim()).FirstOrDefault();
            if (runSetConfig != null)
            {
                mRunsetExecutor.RunSetConfig = runSetConfig;
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to find matching {0} in the Solution", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                // TODO: throw
                // return false;
            }
        }

        private void SelectEnv()
        {           
            Reporter.ToLog(eLogLevel.DEBUG, "Selected Environment: '" + Env + "'");
            ProjEnvironment env = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().Where(x => x.Name.ToLower().Trim() == Env.ToLower().Trim()).FirstOrDefault();
            if (env != null)
            {
                mRunsetExecutor.RunsetExecutionEnvironment = env;
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to find matching Environment in the Solution");
                // TODO: throw
                // return false;
            }
        }

        private void DownloadSolutionFromSourceControl()
        {
            if (scURL != null && scUser != "" && scPswd != null)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Downloading Solution from source control");
                if (scURL.IndexOf(".git") != -1)
                {
                    // App.DownloadSolution(value.Substring(0, value.IndexOf(".git") + 4));
                    RepositoryItemHelper.RepositoryItemFactory.DownloadSolution(scURL.Substring(0, scURL.IndexOf(".git") + 4));
                }
                else
                {
                    // App.DownloadSolution(value);
                    RepositoryItemHelper.RepositoryItemFactory.DownloadSolution(scURL);
                }
            }
        }

        internal void SetSourceControlPassword(string value)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Selected SourceControlPassword: '" + value + "'");
            mUserProfile.SourceControlPass = value;
            scPswd = value;
        }

        internal void PasswordEncrypted(string value)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "PasswordEncrypted: '" + value + "'");
            string pswd = mUserProfile.SourceControlPass;
            if (value == "Y")
            {
                pswd = EncryptionHandler.DecryptwithKey(mUserProfile.SourceControlPass, ENCRYPTION_KEY);
            }

            if (mUserProfile.SourceControlType == SourceControlBase.eSourceControlType.GIT && pswd == "")
            {
                pswd = "Test";
            }

            mUserProfile.SourceControlPass = pswd;
        }

        internal void SourceControlProxyPort(string value)
        {
            if (value == "")
            {
                mUserProfile.SolutionSourceControlConfigureProxy = false;
            }
            else
            {
                mUserProfile.SolutionSourceControlConfigureProxy = true;
            }

            Reporter.ToLog(eLogLevel.INFO, "Selected SourceControlProxyPort: '" + value + "'");
            mUserProfile.SolutionSourceControlProxyPort = value;
        }

        internal void SourceControlProxyServer(string value)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Selected SourceControlProxyServer: '" + value + "'");
            if (value == "")
            {
                mUserProfile.SolutionSourceControlConfigureProxy = false;
            }
            else
            {
                mUserProfile.SolutionSourceControlConfigureProxy = true;
            }

            if (value != "" && !value.ToUpper().StartsWith("HTTP://"))
            {
                value = "http://" + value;
            }

            mUserProfile.SolutionSourceControlProxyAddress = value;
        }

        internal void SetSourceControlUser(string value)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Selected SourceControlUser: '" + value + "'");
            if (mUserProfile.SourceControlType == SourceControlBase.eSourceControlType.GIT && value == "")
            {
                value = "Test";
            }

            mUserProfile.SourceControlUser = value;
            scUser = value;
        }

        internal void SetSourceControlURL(string value)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Selected SourceControlUrl: '" + value + "'");
            if (mUserProfile.SourceControlType == SourceControlBase.eSourceControlType.SVN)
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
            mUserProfile.SourceControlURL = value;
            scURL = value;
        }

        internal void SetSourceControlType(string value)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Selected SourceControlType: '" + value + "'");
            if (value.Equals("GIT"))
            {
                mUserProfile.SourceControlType = SourceControlBase.eSourceControlType.GIT;
            }
            else if (value.Equals("SVN"))
            {
                mUserProfile.SourceControlType = SourceControlBase.eSourceControlType.SVN;
            }
            else
            {
                mUserProfile.SourceControlType = SourceControlBase.eSourceControlType.None;
            }
        }

        private void OpenSolution()
        {

            Reporter.ToLog(eLogLevel.DEBUG, "Loading the Solution: '" + Solution + "'");
            try
            {
                if (WorkSpace.Instance.OpenSolution(Solution) == false)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to load the Solution");
                    // TODO: throw
                    return;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to load the Solution");
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                // TODO: throw
                return;
            }
        }
    }
}
