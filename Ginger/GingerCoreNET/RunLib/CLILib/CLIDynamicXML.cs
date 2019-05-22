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
            string xml = DynamicRunSetManager.CreateDynamicRunSetXML(solution, runsetExecutor, cliHelper);
            return xml;            
        }


        public void LoadContent(string content, CLIHelper cliHelper, RunsetExecutor runsetExecutor)
        {
            DynamicRunSet dynamicRunSet =  DynamicRunSetManager.LoadDynamicRunsetFromXML(content);
            if (string.IsNullOrEmpty(dynamicRunSet.SolutionSourceControlType) == false)
            {
                cliHelper.SetSourceControlType(dynamicRunSet.SolutionSourceControlType);
                cliHelper.SetSourceControlURL(dynamicRunSet.SolutionSourceControlUrl);
                cliHelper.SetSourceControlUser(dynamicRunSet.SolutionSourceControlUser);
                cliHelper.SetSourceControlPassword(dynamicRunSet.SolutionSourceControlPassword);
                if (string.IsNullOrEmpty(dynamicRunSet.SolutionSourceControlProxyServer) == false)
                {
                    cliHelper.SourceControlProxyServer(dynamicRunSet.SolutionSourceControlProxyServer);
                    cliHelper.SourceControlProxyPort(dynamicRunSet.SolutionSourceControlProxyPort);
                }
            }
            cliHelper.Solution = dynamicRunSet.SolutionPath;
            cliHelper.Env = dynamicRunSet.Environemnt;
            cliHelper.ShowAutoRunWindow = dynamicRunSet.ShowAutoRunWindow;
            cliHelper.RunAnalyzer = dynamicRunSet.RunAnalyzer;

            DynamicRunSetManager.LoadRealRunSetFromDynamic(runsetExecutor, dynamicRunSet);
        }

        public void Execute(RunsetExecutor runsetExecutor)
        {
            runsetExecutor.RunRunset();
        }
    }
}
