#region License
/*
Copyright © 2014-2019 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

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
