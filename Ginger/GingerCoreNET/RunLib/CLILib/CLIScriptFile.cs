#region License
/*
Copyright © 2014-2023 European Support Limited

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

using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerCoreNET.RosLynLib;
using System;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public class CLIScriptFile : ICLI
    {
        bool ICLI.IsFileBasedConfig { get { return true; } }

        string mScriptFile;

        public string Verb
        {
            get
            {
                return ScriptOptions.Verb;
            }
        }

        string ICLI.FileExtension
        {
            get
            {
                return "script";
            }
        }

        public string CreateConfigurationsContent(Solution solution, RunsetExecutor runsetExecutor, CLIHelper cliHelper)
        {
            string txt = string.Format("OpenSolution(@\"{0}\");", solution.Folder) + Environment.NewLine;
            txt += string.Format("OpenRunSet(\"{0}\",\"{1}\");", runsetExecutor.RunSetConfig.Name, runsetExecutor.RunsetExecutionEnvironment.Name) + Environment.NewLine;
            txt += "CreateExecutionSummaryJSON(\"FILENAME\");" + Environment.NewLine;
            //txt += "if (Pass)" + Environment.NewLine;
            //txt += "{" + Environment.NewLine;
            //txt += "SendEmail()" + Environment.NewLine;
            //txt += "}" + Environment.NewLine;
            return txt;
        }

        public void LoadGeneralConfigurations(string content, CLIHelper cliHelper)
        {
            mScriptFile = content;
        }

        public void LoadRunsetConfigurations(string content, CLIHelper cliHelper, RunsetExecutor runsetExecutor)
        {
            
        }

        public async Task Execute(RunsetExecutor runsetExecutor)
        {
            await Task.Run(() =>
            {
                var rc = CodeProcessor.ExecuteNew(mScriptFile);
            });                   
        }
    }
}
