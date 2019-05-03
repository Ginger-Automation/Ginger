using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Ginger.Run;
using GingerCoreNET.SourceControl;
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
            string sConfig = null;
            if (CLIHelper.DownloadSolutionFromSourceControlBool == true)
            {
               
                sConfig = "SourceControlType=" + WorkSpace.Instance.Solution.SourceControl.GetSourceControlType.ToString() + Environment.NewLine;
                sConfig += "SourceControlUrl=" + WorkSpace.Instance.Solution.SourceControl.SourceControlURL.ToString() + Environment.NewLine;
                sConfig += "SourceControlUser=" + WorkSpace.Instance.Solution.SourceControl.SourceControlUser.ToString() + Environment.NewLine;
                sConfig += "SourceControlPassword=" + WorkSpace.Instance.Solution.SourceControl.SourceControlPass.ToString() + Environment.NewLine;
                if (WorkSpace.Instance.Solution.SourceControl.GetSourceControlType == SourceControlBase.eSourceControlType.GIT && WorkSpace.Instance.Solution.SourceControl.SourceControlProxyAddress.ToLower().ToString() == "true")
                {
                    sConfig += "SourceControlProxyServer=" + WorkSpace.Instance.Solution.SourceControl.SourceControlProxyAddress.ToString() + Environment.NewLine;
                    sConfig += "SourceControlProxyPort=" + WorkSpace.Instance.Solution.SourceControl.SourceControlProxyPort.ToString() + Environment.NewLine;
                }
            }
            sConfig += "Solution=" + WorkSpace.Instance.Solution.Folder + Environment.NewLine;
            sConfig += "Env=" + runsetExecutor.RunsetExecutionEnvironment.Name + Environment.NewLine;
            sConfig += "RunSet=" + runsetExecutor.RunSetConfig.Name + Environment.NewLine;
            sConfig += "RunAnalyzer=" + CLIHelper.RunAnalyzer.ToString() + Environment.NewLine;
            sConfig += "ShowAutoRunWindow=" + CLIHelper.ShowAutoRunWindow.ToString() + Environment.NewLine;

            // TODO: add source control and all other options !!!!!!!!!!!

            return sConfig;
        }

        public void LoadContent(string content, RunsetExecutor runsetExecutor)
        {
            
            CLIHelper.ShowAutoRunWindow = true; // // default is true to keep backword compatibility
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
                            CLIHelper.ShowAutoRunWindow = bool.Parse(value);
                            break;
                        case "RunAnalyzer":
                            CLIHelper.RunAnalyzer = bool.Parse(value);                            
                            break;
                        default:
                            Reporter.ToLog(eLogLevel.ERROR, "Unknown argument: '" + param + "'");
                            throw new ArgumentException("Unknown argument", param);
                    }
                }


                mCLIHelper.ProcessArgs(runsetExecutor);
            }            
        }


    }
}
