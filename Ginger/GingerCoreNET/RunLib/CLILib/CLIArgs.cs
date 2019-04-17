using System;
using System.Collections.Generic;
using System.Text;
using Ginger.Run;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public class CLIArgs : ICLI
    {
        public string CreateContent(RunsetExecutor runsetExecutor)
        {
            return "/Solution=s1 /RunSet:def aold 1 /Env:koko 1";
        }

        public bool Execute()
        {
            return true;
        }

        public string Identifier()
        {
            return "RunsetArgs";
        }

        public void LoadContent(string content, RunsetExecutor runsetExecutor)
        {
            
        }
    }
}
