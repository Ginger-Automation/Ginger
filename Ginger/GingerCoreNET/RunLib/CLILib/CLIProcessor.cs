using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using GingerCoreNET.RosLynLib;
using System.IO;

namespace Amdocs.Ginger.CoreNET.RunLib
{
    public class CLIProcessor
    {
        public static void ExecuteArgs(string[] args)
        {
            WorkSpace.Instance.RunningInExecutionMode = true;
            Reporter.ReportAllAlsoToConsole = true;  //needed so all reportering will be added to Console                             
            ConsoleWorkspaceEventHandler consoleWorkspaceEventHandler = new ConsoleWorkspaceEventHandler();

            string[] arg1 = args[0].Split('=');
            string param = arg1[0].Trim();
            string fileName = arg1[1].Trim();
            if (param.StartsWith("ConfigFile"))  // Old key=value runset config file
            {
                ExecuteConfig(fileName);                
            }
            else if (param.StartsWith("ScriptFile")) // New C# Roslyn code
            {
                ExecutScript(fileName);
            }
            // TODO: dynamic
            // TODO: add ///
            // TODO: Excel 
        }

        private static void ExecuteConfig(string configFile)
        {        
            string config = ReadFile(configFile);
            CLIConfigFile configFileProcessor = new CLIConfigFile();
            configFileProcessor.RunConfig(config, WorkSpace.Instance.RunsetExecutor);
        }

        private static void ExecutScript(string scriptFile)
        {
            string script = ReadFile(scriptFile);
            var rc = CodeProcessor.ExecuteNew(script);
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
