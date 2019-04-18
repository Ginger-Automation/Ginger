using System;
using System.Collections.Generic;
using System.Text;
using amdocs.ginger.GingerCoreNET;
using Ginger.Run;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public class CLIArgs : ICLI
    {
        string ICLI.Identifier
        {
            get
            {
                return "Args";
            }
        }

        string ICLI.FileExtension
        {
            get
            {
                return null;
            }
        }

        public string CreateContent(RunsetExecutor runsetExecutor)
        {
            string Args = string.Format("/Soluion:{0}", WorkSpace.Instance.Solution.Folder);
            Args += string.Format("/Runset:{0}", runsetExecutor.RunSetConfig.Name);
            Args += string.Format("/Env:{0}", runsetExecutor.RunsetExecutionEnvironment.Name);
            return Args;
        }

        public bool Execute(RunsetExecutor runsetExecutor)
        {
            return true;
        }
        

        public void LoadContent(string content, RunsetExecutor runsetExecutor)
        {
            
        }
    }
}
