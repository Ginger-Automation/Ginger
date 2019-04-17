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
            string fileName = arg1[1].Trim();

            SetCLIHandler(param, fileName);
            mCLIHandler.Execute();

        }

        private void SetCLIHandler(string param, string fileName)
        {
            // TODO: get all classes impl ICLI and check Identifier then set

            if (param.StartsWith("ConfigFile"))  // Old key=value runset config file
            {
                mCLIHandler = new CLIConfigFile();
                string config = ReadFile(fileName);
                mCLIHandler.LoadContent(config, WorkSpace.Instance.RunsetExecutor);
                mCLIHandler.Execute();
            }
            else if (param.StartsWith("ScriptFile")) // New C# Roslyn code
            {
                CLIScriptFile cLIScriptFile = new CLIScriptFile();
                string script = ReadFile(fileName);
                cLIScriptFile.LoadContent(script, null);
                cLIScriptFile.Execute();

            }
            else if (param.StartsWith("DynamicFile")) // xml with dynamic runset creation
            {
                CLIDynamicXML CLIDynamicXML = new CLIDynamicXML();
                string script = ReadFile(fileName);
                CLIDynamicXML.LoadContent(script, WorkSpace.Instance.RunsetExecutor);
                CLIDynamicXML.Execute();

            }

            // TODO: add ///
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
