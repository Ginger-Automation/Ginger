using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib;
using Ginger.Run;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public class CLIDynamicXML : ICLI
    {
        public string Identifier
        {
            get
            {
                return "DynamicXML";
            }
        }

        public string CreateContent(RunsetExecutor runsetExecutor)
        {
            string xml = DynamicRunSetManager.CreateRunSet(runsetExecutor);
            return xml;            
        }

        public bool Execute(RunsetExecutor runsetExecutor)
        {
            return true;
        }

        public void LoadContent(string content, RunsetExecutor runsetExecutor)
        {
            DynamicRunSet dynamicRunSet =  DynamicRunSetManager.LoadContent(content);
            DynamicRunSetManager.LoadRunSet(runsetExecutor, dynamicRunSet);
        }
    }
}
