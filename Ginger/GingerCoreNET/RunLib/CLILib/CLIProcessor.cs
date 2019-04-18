using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using System;
using System.IO;

namespace Amdocs.Ginger.CoreNET.RunLib
{
    public class CLIProcessor
    {
        ICLI mCLIHandler;
        public void ExecuteArgs(string[] args)
        {
            WorkSpace.Instance.RunningInExecutionMode = true;
            Reporter.ReportAllAlsoToConsole = true;  //needed so all reportering will be added to Console                             
            ConsoleWorkspaceEventHandler consoleWorkspaceEventHandler = new ConsoleWorkspaceEventHandler();

            string[] arg1 = args[0].Split('=');
            string param = arg1[0].Trim();
            string value = arg1[1].Trim();

            SetCLIHandler(param, value);
            

            WorkSpace.Instance.RunsetExecutor.InitRunners();
            mCLIHandler.Execute(WorkSpace.Instance.RunsetExecutor);

        }

        private void SetCLIHandler(string param, string value)
        {
            // TODO: get all classes impl ICLI and check Identifier then set

            switch (param)
            {
                case "ConfigFile":
                    mCLIHandler = new CLIConfigFile();
                    string config = ReadFile(value);
                    mCLIHandler.LoadContent(config, WorkSpace.Instance.RunsetExecutor);
                    break;
                case "ScriptFile":
                    mCLIHandler = new CLIScriptFile();
                    string script = ReadFile(value);
                    mCLIHandler.LoadContent(script, null);
                    break;
                case "DynamicFile":
                    CLIDynamicXML CLIDynamicXML = new CLIDynamicXML();
                    string dynamicXML = ReadFile(value);
                    CLIDynamicXML.LoadContent(dynamicXML, WorkSpace.Instance.RunsetExecutor);
                    break;
                case "Args":
                    CLIArgs CLIArgs = new CLIArgs();                    
                    CLIArgs.LoadContent(value, WorkSpace.Instance.RunsetExecutor);
                    break;

            }
          
            // TODO: Excel 
        }

        private static string ReadFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, "File not found: " + fileName);
                throw new FileNotFoundException("Cannot find file", fileName);
            }
            string txt = File.ReadAllText(fileName);
            return txt;
        }
    }
}
