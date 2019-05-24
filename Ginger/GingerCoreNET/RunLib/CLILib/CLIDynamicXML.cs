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
                return "Dynamic";
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
            DynamicGingerExecution dynamicExecution =  DynamicRunSetManager.LoadDynamicExecutionFromXML(content);
            if (dynamicExecution.SolutionDetails.SourceControlDetails != null)
            {
                cliHelper.SetSourceControlType(dynamicExecution.SolutionDetails.SourceControlDetails.Type);
                cliHelper.SetSourceControlURL(dynamicExecution.SolutionDetails.SourceControlDetails.Url);
                cliHelper.SetSourceControlUser(dynamicExecution.SolutionDetails.SourceControlDetails.User);
                cliHelper.SetSourceControlPassword(dynamicExecution.SolutionDetails.SourceControlDetails.Password);
                if (string.IsNullOrEmpty(dynamicExecution.SolutionDetails.SourceControlDetails.ProxyServer) == false)
                {
                    cliHelper.SourceControlProxyServer(dynamicExecution.SolutionDetails.SourceControlDetails.ProxyServer);
                    cliHelper.SourceControlProxyPort(dynamicExecution.SolutionDetails.SourceControlDetails.ProxyPort);
                }
            }
            cliHelper.Solution = dynamicExecution.SolutionDetails.Path;
            cliHelper.ShowAutoRunWindow = dynamicExecution.ShowAutoRunWindow;

            AddRunset addRunset = dynamicExecution.AddRunsets[0];//for now supporting only one Run set execution
            cliHelper.Env = addRunset.Environment;            
            cliHelper.RunAnalyzer = addRunset.RunAnalyzer;

            DynamicRunSetManager.CreateRealRunSetFromDynamic(runsetExecutor, addRunset);
        }

        public void Execute(RunsetExecutor runsetExecutor)
        {
            runsetExecutor.RunRunset();
        }
    }
}
