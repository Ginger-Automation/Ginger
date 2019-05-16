using System;
using System.Collections.Generic;
using System.Text;
using amdocs.ginger.GingerCoreNET;
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

        string ICLI.FileExtension
        {
            get
            {
                return "xml";
            }
        }

        public string CreateContent(RunsetExecutor runsetExecutor)
        {
            string xml = DynamicRunSetManager.CreateRunSet(runsetExecutor);
            return xml;            
        }

        public void Execute(RunsetExecutor runsetExecutor)
        {
            runsetExecutor.InitRunners();
            runsetExecutor.RunRunset();
        }

        public void LoadContent(string content, CLIHelper cliHelper, RunsetExecutor runsetExecutor)
        {
            DynamicRunSet dynamicRunSet =  DynamicRunSetManager.LoadContent(content);
            DynamicRunSetManager.LoadRunSet(runsetExecutor, dynamicRunSet);
        }
    }
}
