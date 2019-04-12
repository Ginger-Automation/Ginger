using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using GingerCoreNET.RosLynLib;

namespace Amdocs.Ginger.CoreNET.RunLib
{
    public class CLIProcessor
    {
        public static void ExecuteArgs(string[] args)
        {
            WorkSpace.Instance.RunningInExecutionMode = true;
            Reporter.ReportAllAlsoToConsole = true;  //needed so all reportering will be added to Console                             
            ConsoleWorkspaceEventHandler consoleWorkspaceEventHandler = new ConsoleWorkspaceEventHandler();

            if (args[0].StartsWith("ConfigFile"))  // Old
            {
                // This Ginger is running with run set config will do the run and close GingerInitApp();                                
                ConfigFileProcessor configFileProcessor = new ConfigFileProcessor();
                configFileProcessor.ExecuteRunSetConfigFile();
            }
            else if (args[0].StartsWith("ScriptFile")) // New
            {
                ExecutScript(args[0]);
            }
        }

        private static void ExecutScript(string scriptFile)
        {
            if (!System.IO.File.Exists(scriptFile))
            {
                Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, "File not found");
                return;
            }
            string script = System.IO.File.ReadAllText(scriptFile);
            var rc = CodeProcessor.ExecuteNew(script);
        }



    }
}
