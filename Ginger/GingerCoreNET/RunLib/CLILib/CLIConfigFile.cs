﻿using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerCoreNET.SourceControl;
using System;

namespace Amdocs.Ginger.CoreNET.RunLib
{
    public class CLIConfigFile : ICLI
    {
        bool ICLI.IsFileBasedConfig { get { return true; } }

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

        public async void Execute(RunsetExecutor runsetExecutor)
        {            
            runsetExecutor.RunRunset();            
        }


        public string CreateContent(Solution solution, RunsetExecutor runsetExecutor, CLIHelper cliHelper)
        {
            string sConfig = null;
            if (cliHelper.DownloadUpgradeSolutionFromSourceControl == true)
            {               
                sConfig = "SourceControlType=" + WorkSpace.Instance.Solution.SourceControl.GetSourceControlType.ToString() + Environment.NewLine;
                sConfig += "SourceControlUrl=" + WorkSpace.Instance.Solution.SourceControl.SourceControlURL.ToString() + Environment.NewLine;
                sConfig += "SourceControlUser=" + WorkSpace.Instance.Solution.SourceControl.SourceControlUser.ToString() + Environment.NewLine;
                sConfig += "SourceControlPassword=" + WorkSpace.Instance.Solution.SourceControl.SourceControlPass.ToString() + Environment.NewLine;
                if (WorkSpace.Instance.Solution.SourceControl.SourceControlProxyAddress != null)
                {
                    if (WorkSpace.Instance.Solution.SourceControl.GetSourceControlType == SourceControlBase.eSourceControlType.GIT && (WorkSpace.Instance.Solution.SourceControl.SourceControlProxyAddress.ToLower().ToString() == "true"))
                    {
                        sConfig += "SourceControlProxyServer=" + WorkSpace.Instance.Solution.SourceControl.SourceControlProxyAddress.ToString() + Environment.NewLine;
                        sConfig += "SourceControlProxyPort=" + WorkSpace.Instance.Solution.SourceControl.SourceControlProxyPort.ToString() + Environment.NewLine;
                    }
                }
            }
            sConfig += "Solution=" + WorkSpace.Instance.Solution.Folder + Environment.NewLine;
            sConfig += "Env=" + runsetExecutor.RunsetExecutionEnvironment.Name + Environment.NewLine;
            sConfig += "RunSet=" + runsetExecutor.RunSetConfig.Name + Environment.NewLine;
            sConfig += "RunAnalyzer=" + cliHelper.RunAnalyzer.ToString() + Environment.NewLine;
            sConfig += "ShowAutoRunWindow=" + cliHelper.ShowAutoRunWindow.ToString() + Environment.NewLine;

            return sConfig;
        }

        public void LoadContent(string content, CLIHelper cliHelper, RunsetExecutor runsetExecutor)
        {
            cliHelper.ShowAutoRunWindow = true; // // default is true to keep backword compatibility
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
                            cliHelper.SetSourceControlType(value);
                            break;
                        case "SourceControlUrl":
                            cliHelper.SetSourceControlURL(value);                            
                            break;
                        case "SourceControlUser":
                            cliHelper.SetSourceControlUser(value);                            
                            break;
                        case "SourceControlPassword":
                            cliHelper.SetSourceControlPassword(value);                            
                            break;
                        case "PasswordEncrypted":
                            cliHelper.PasswordEncrypted(value);                            
                            break;
                        case "SourceControlProxyServer":
                            cliHelper.SourceControlProxyServer(value);                            
                            break;
                        case "SourceControlProxyPort":
                            cliHelper.SourceControlProxyPort(value);                            
                            break;
                        case "Solution":
                            cliHelper.Solution = value;                            
                            break;
                        case "Env":
                            cliHelper.Env = value;                        
                            break;
                        case "RunSet":
                            cliHelper.Runset = value;                            
                            break;
                        case "ShowAutoRunWindow":
                            cliHelper.ShowAutoRunWindow = bool.Parse(value);
                            break;
                        case "RunAnalyzer":
                            cliHelper.RunAnalyzer = bool.Parse(value);                            
                            break;
                        default:
                            Reporter.ToLog(eLogLevel.ERROR, "Unknown argument: '" + param + "'");
                            throw new ArgumentException("Unknown argument", param);
                    }
                }                
            }            
        }


    }
}
