using amdocs.ginger.GingerCoreNET;
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerCoreNET.RosLynLib;
using System;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public class CLIScriptFile : ICLI
    {
        bool ICLI.IsFileBasedConfig { get { return true; } }

        string mScriptFile;

        public string Identifier
        {
            get
            {
                return "Script";
            }
        }

        string ICLI.FileExtension
        {
            get
            {
                return "script";
            }
        }

        public string CreateContent(Solution solution, RunsetExecutor runsetExecutor, CLIHelper cliHelper)
        {
            string txt = string.Format("OpenSolution({0})", solution.Folder) + Environment.NewLine;
            txt += string.Format("OpenRunSet(\"{0}\",\"{1}\")", runsetExecutor.RunSetConfig.Name, runsetExecutor.RunsetExecutionEnvironment.Name) + Environment.NewLine;
            txt += "CreateExecutionSummaryJSON(\"FILENAME\")" + Environment.NewLine;
            txt += "if (Pass)" + Environment.NewLine;
            txt += "{" + Environment.NewLine;
            txt += "SendEmail()" + Environment.NewLine;
            txt += "}" + Environment.NewLine;
            return txt;
        }

        public void Execute(RunsetExecutor runsetExecutor)
        {                                    
            var rc = CodeProcessor.ExecuteNew(mScriptFile);            
        }

        public void LoadContent(string content, CLIHelper cliHelper, RunsetExecutor runsetExecutor)
        {
            mScriptFile = content;            
        }
    }
}
