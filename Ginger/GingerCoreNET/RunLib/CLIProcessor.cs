using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.Environments;
using GingerCoreNET.RosLynLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib
{
    public class CLIProcessor
    {
        public static void ExecuteArgs(string[] args)
        {

            WorkSpace.Instance.RunningInExecutionMode = true;
            Reporter.ReportAllAlsoToConsole = true;  //needed so all reportering will be added to Console                             
            ConsoleWorkspaceEventHandler consoleWorkspaceEventHandler = new ConsoleWorkspaceEventHandler();

            

            string[] param = args[0].Split('=');
            if (param[0].StartsWith("ScriptFile"))
            {
                ExecutScript(param[1]);
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
