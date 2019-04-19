using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Ginger;
using Ginger.Run;
using GingerCore;
using GingerCore.Environments;
using GingerCoreNET.SourceControl;
using System;
using System.Linq;
using System.Reflection;

namespace Amdocs.Ginger.CoreNET.RunLib
{
    public class CLIConfigFile : ICLI
    {
        static readonly string ENCRYPTION_KEY = "D3^hdfr7%ws4Kb56=Qt";//????? !!!!!!!!!!!!!!!!!!!

        bool mShowAutoRunWindow = true;
        
        UserProfile mUserProfile;

        public string Identifier
        {
            get
            {
                return "ConfigFile";
            }
        }

        string ICLI.FileExtension
        {
            get
            {
                return "Config";
            }
        }

        public bool Execute(RunsetExecutor runsetExecutor)
        {
            WorkSpace.Instance.RunsetExecutor.InitRunners();
            runsetExecutor.RunRunset();
            return true;           
        }



        bool ProcessConfig(string config)
        {
            return true;
        }

       
        public string CreateContent(RunsetExecutor runsetExecutor)
        {
            string sConfig = "Solution=" + WorkSpace.Instance.Solution.Folder + Environment.NewLine;
            sConfig += "Env=" + runsetExecutor.RunsetExecutionEnvironment.Name + Environment.NewLine;
            sConfig += "RunSet=" + runsetExecutor.RunSetConfig.Name + Environment.NewLine;

            sConfig += "RunAnalyzer=" + runsetExecutor.RunSetConfig.RunWithAnalyzer + Environment.NewLine;

            // TODO: add source control and all other options !!!!!!!!!!!

             return sConfig;
        }

        public void LoadContent(string content, RunsetExecutor runsetExecutor)
        {
            string scURL = null;
            string scUser = null;
            string scPswd = null;

            using (System.IO.StringReader reader = new System.IO.StringReader(content))
            {
                string arg;
                while ((arg = reader.ReadLine()) != null)
                {
                    int i = arg.IndexOf('=');
                    string param = arg.Substring(0, i).Trim();
                    string value = arg.Substring(i + 1).Trim();

                    switch (param)
                    {
                        case "SourceControlType":
                            Reporter.ToLog(eLogLevel.DEBUG, "Selected SourceControlType: '" + value + "'");
                            if (value.Equals("GIT"))
                                mUserProfile.SourceControlType = SourceControlBase.eSourceControlType.GIT;
                            else if (value.Equals("SVN"))
                                mUserProfile.SourceControlType = SourceControlBase.eSourceControlType.SVN;
                            else
                                mUserProfile.SourceControlType = SourceControlBase.eSourceControlType.None;
                            break;

                        case "SourceControlUrl":
                            Reporter.ToLog(eLogLevel.DEBUG, "Selected SourceControlUrl: '" + value + "'");
                            if (mUserProfile.SourceControlType == SourceControlBase.eSourceControlType.SVN)
                            {
                                if (!value.ToUpper().Contains("/SVN") && !value.ToUpper().Contains("/SVN/"))
                                    value = value + "svn/";
                                if (!value.ToUpper().EndsWith("/"))
                                    value = value + "/";
                            }
                            mUserProfile.SourceControlURL = value;
                            scURL = value;
                            break;

                        case "SourceControlUser":
                            Reporter.ToLog(eLogLevel.DEBUG, "Selected SourceControlUser: '" + value + "'");
                            if (mUserProfile.SourceControlType == SourceControlBase.eSourceControlType.GIT && value == "")
                                value = "Test";
                            mUserProfile.SourceControlUser = value;
                            scUser = value;
                            break;

                        case "SourceControlPassword":
                            Reporter.ToLog(eLogLevel.DEBUG, "Selected SourceControlPassword: '" + value + "'");
                            mUserProfile.SourceControlPass = value;
                            scPswd = value;
                            break;

                        case "PasswordEncrypted":
                            Reporter.ToLog(eLogLevel.DEBUG, "PasswordEncrypted: '" + value + "'");
                            string pswd = mUserProfile.SourceControlPass;
                            if (value == "Y")
                                pswd = EncryptionHandler.DecryptwithKey(mUserProfile.SourceControlPass, ENCRYPTION_KEY);
                            if (mUserProfile.SourceControlType == SourceControlBase.eSourceControlType.GIT && pswd == "")
                                pswd = "Test";
                            mUserProfile.SourceControlPass = pswd;
                            break;

                        case "SourceControlProxyServer":
                            Reporter.ToLog(eLogLevel.DEBUG, "Selected SourceControlProxyServer: '" + value + "'");
                            if (value == "")
                                mUserProfile.SolutionSourceControlConfigureProxy = false;
                            else
                                mUserProfile.SolutionSourceControlConfigureProxy = true;
                            if (value != "" && !value.ToUpper().StartsWith("HTTP://"))
                                value = "http://" + value;
                            mUserProfile.SolutionSourceControlProxyAddress = value;
                            break;

                        case "SourceControlProxyPort":
                            if (value == "")
                                mUserProfile.SolutionSourceControlConfigureProxy = false;
                            else
                                mUserProfile.SolutionSourceControlConfigureProxy = true;
                            Reporter.ToLog(eLogLevel.INFO, "Selected SourceControlProxyPort: '" + value + "'");
                            mUserProfile.SolutionSourceControlProxyPort = value;
                            break;

                        case "Solution":
                            if (scURL != null && scUser != "" && scPswd != null)
                            {
                                Reporter.ToLog(eLogLevel.DEBUG, "Downloading Solution from source control");
                                if (value.IndexOf(".git") != -1)
                                {
                                    // App.DownloadSolution(value.Substring(0, value.IndexOf(".git") + 4));
                                    RepositoryItemHelper.RepositoryItemFactory.DownloadSolution(value.Substring(0, value.IndexOf(".git") + 4));
                                }
                                else
                                {
                                    // App.DownloadSolution(value);
                                    RepositoryItemHelper.RepositoryItemFactory.DownloadSolution(value);
                                }
                            }
                            Reporter.ToLog(eLogLevel.DEBUG, "Loading the Solution: '" + value + "'");
                            try
                            {
                                if (WorkSpace.Instance.OpenSolution(value) == false)
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
                            break;

                        case "Env":
                            Reporter.ToLog(eLogLevel.DEBUG, "Selected Environment: '" + value + "'");
                            ProjEnvironment env = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().Where(x => x.Name.ToLower().Trim() == value.ToLower().Trim()).FirstOrDefault();
                            if (env != null)
                            {
                                runsetExecutor.RunsetExecutionEnvironment = env;
                            }
                            else
                            {
                                Reporter.ToLog(eLogLevel.ERROR, "Failed to find matching Environment in the Solution");
                                // TODO: throw
                                // return false;
                            }
                            break;

                        case "RunSet":
                            Reporter.ToLog(eLogLevel.DEBUG, string.Format("Selected {0}: '{1}'", GingerDicser.GetTermResValue(eTermResKey.RunSet), value));
                            ObservableList<RunSetConfig> RunSets = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>();
                            RunSetConfig runSetConfig = RunSets.Where(x => x.Name.ToLower().Trim() == value.ToLower().Trim()).FirstOrDefault();
                            if (runSetConfig != null)
                            {
                                runsetExecutor.RunSetConfig = runSetConfig;
                            }
                            else
                            {
                                Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to find matching {0} in the Solution", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                                // TODO: throw
                                // return false;
                            }
                            break;
                        case "ShowAutoRunWindow":
                            Reporter.ToLog(eLogLevel.DEBUG, string.Format("NoAutoRunWindow {0}", value));
                            mShowAutoRunWindow = bool.Parse(value);
                            break;
                        case "RunAnalyzer":
                            runsetExecutor.RunSetConfig.RunWithAnalyzer = bool.Parse(value); 
                            break;
                        default:
                            Reporter.ToLog(eLogLevel.ERROR, "UnKnown argument: '" + param + "'");
                            // TODO: throw
                             return;
                    }
                }
            }            
        }


    }
}
