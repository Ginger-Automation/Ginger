using System;
using System.Collections.Generic;
using System.Text;
using amdocs.ginger.GingerCoreNET;
using Ginger.Run;
using GingerCoreNET.RosLynLib;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public class CLIScriptFile : ICLI
    {
        string mScriptFile;

        public string Identifier
        {
            get
            {
                return "ScriptFile";
            }
        }

        public string CreateContent(RunsetExecutor runsetExecutor)
        {
            string txt = string.Format("OpenSolution({0})", WorkSpace.Instance.Solution.Folder) + Environment.NewLine;
            txt += string.Format("RunRunSet({0})", runsetExecutor.RunSetConfig.Name) + Environment.NewLine;
            txt += "CreateHTMLReport(rep 1)" + Environment.NewLine;
            return txt;
        }

        public bool Execute(RunsetExecutor runsetExecutor)
        {                                    
            var rc = CodeProcessor.ExecuteNew(mScriptFile);
            return true;
        }

        public void LoadContent(string content, RunsetExecutor runsetExecutor)
        {
            mScriptFile = content;            
        }
    }
}
