using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Ginger.Run;
using System;

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

        public void Execute(RunsetExecutor runsetExecutor)
        {
            WorkSpace.Instance.RunsetExecutor.InitRunners();
            runsetExecutor.RunRunset();            
        }

       
        public string CreateContent(RunsetExecutor runsetExecutor)
        {
            string sConfig = "Solution=" + WorkSpace.Instance.Solution.Folder + Environment.NewLine;
            sConfig += "Env=" + runsetExecutor.RunsetExecutionEnvironment.Name + Environment.NewLine;
            sConfig += "RunSet=" + runsetExecutor.RunSetConfig.Name + Environment.NewLine;
            sConfig += "RunAnalyzer=" + runsetExecutor.RunSetConfig.RunWithAnalyzer + Environment.NewLine;
            sConfig += "ShowAutoRunWindow=" + mCLIHelper.ShowAutoRunWindow.ToString() + Environment.NewLine;

            // TODO: add source control and all other options !!!!!!!!!!!

            return sConfig;
        }

        public void LoadContent(string content, RunsetExecutor runsetExecutor)
        {
            mCLIHelper.ShowAutoRunWindow = true; // // default is true to keep backword compatibility
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
                            break;
                        case "RunSet":
                            mCLIHelper.Runset = value;                            
                            break;
                        case "ShowAutoRunWindow":                                                        
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
