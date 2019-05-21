using Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib;
using Ginger.Run;
using Ginger.SolutionGeneral;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public class CLIDynamicXML : ICLI
    {
        bool ICLI.IsFileBasedConfig { get { return true; } }

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

        public string CreateContent(Solution solution, RunsetExecutor runsetExecutor, CLIHelper cliHelper)
        {
            string xml = DynamicRunSetManager.CreateRunSet(runsetExecutor);
            return xml;            
        }

        public void Execute(RunsetExecutor runsetExecutor)
        {
            runsetExecutor.RunRunset();
        }

        public void LoadContent(string content, CLIHelper cliHelper, RunsetExecutor runsetExecutor)
        {
            DynamicRunSet dynamicRunSet =  DynamicRunSetManager.LoadContent(content);
            DynamicRunSetManager.LoadRunSet(runsetExecutor, dynamicRunSet);
        }
    }
}
