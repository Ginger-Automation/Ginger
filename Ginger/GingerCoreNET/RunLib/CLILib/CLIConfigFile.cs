using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
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
        

        CLIHelper mCLIHelper = new CLIHelper();                

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
                            mCLIHelper.SetSourceControlType(value);
                            break;
                        case "SourceControlUrl":
                            mCLIHelper.SetSourceControlURL(value);                            
                            break;
                        case "SourceControlUser":
                            mCLIHelper.SetSourceControlUser(value);                            
                            break;
                        case "SourceControlPassword":
                            mCLIHelper.SetSourceControlPassword(value);                            
                            break;
                        case "PasswordEncrypted":
                            mCLIHelper.PasswordEncrypted(value);                            
                            break;
                        case "SourceControlProxyServer":
                            mCLIHelper.SourceControlProxyServer(value);                            
                            break;
                        case "SourceControlProxyPort":
                            mCLIHelper.SourceControlProxyPort(value);                            
                            break;
                        case "Solution":
                            mCLIHelper.Solution = value;                            
                            break;

                        case "Env":
                            mCLIHelper.Env = value;
                            //Reporter.ToLog(eLogLevel.DEBUG, "Selected Environment: '" + value + "'");
                            //ProjEnvironment env = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().Where(x => x.Name.ToLower().Trim() == value.ToLower().Trim()).FirstOrDefault();
                            //if (env != null)
                            //{
                            //    runsetExecutor.RunsetExecutionEnvironment = env;
                            //}
                            //else
                            //{
                            //    Reporter.ToLog(eLogLevel.ERROR, "Failed to find matching Environment in the Solution");
                            //    // TODO: throw
                            //    // return false;
                            //}
                            break;

                        case "RunSet":
                            mCLIHelper.Runset = value;
                            //Reporter.ToLog(eLogLevel.DEBUG, string.Format("Selected {0}: '{1}'", GingerDicser.GetTermResValue(eTermResKey.RunSet), value));
                            //ObservableList<RunSetConfig> RunSets = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>();
                            //RunSetConfig runSetConfig = RunSets.Where(x => x.Name.ToLower().Trim() == value.ToLower().Trim()).FirstOrDefault();
                            //if (runSetConfig != null)
                            //{
                            //    runsetExecutor.RunSetConfig = runSetConfig;
                            //}
                            //else
                            //{
                            //    Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to find matching {0} in the Solution", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                            //    // TODO: throw
                            //    // return false;
                            //}
                            break;
                        case "ShowAutoRunWindow":                            
                            Reporter.ToLog(eLogLevel.DEBUG, string.Format("NoAutoRunWindow {0}", value));
                            mCLIHelper.ShowAutoRunWindow = bool.Parse(value);
                            break;
                        case "RunAnalyzer":
                            mCLIHelper.RunAnalyzer = bool.Parse(value);                            
                            break;
                        default:
                            Reporter.ToLog(eLogLevel.ERROR, "UnKnown argument: '" + param + "'");
                            // TODO: throw
                             return;
                    }
                }


                mCLIHelper.ProcessArgs(runsetExecutor);
            }            
        }


    }
}
