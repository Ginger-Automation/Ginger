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
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public interface ICLI
    {
        string Verb { get; }

        string FileExtension { get; }
        
        /// <summary>
        /// Create CLI content from runsetExecutor
        /// </summary>
        /// <param name="solution"></param>
        /// <param name="runsetExecutor"></param>
        /// <param name="cliHelper"></param>
        /// <returns></returns>
        string CreateConfigurationsContent(Solution solution, RunsetExecutor runsetExecutor, CLIHelper cliHelper);

        /// <summary>
        /// Parse the configurations content to pull general details like Solution details
        /// </summary>
        /// <param name="content"></param>
        /// <param name="cliHelper"></param>
        /// <param name="runsetExecutor"></param>
        void LoadGeneralConfigurations(string content, CLIHelper cliHelper);

        /// <summary>
        /// Parse the configurations content to pull specific details related to the Run set
        /// </summary>
        /// <param name="content"></param>
        /// <param name="cliHelper"></param>
        /// <param name="runsetExecutor"></param>
        void LoadRunsetConfigurations(string content, CLIHelper cliHelper, RunsetExecutor runsetExecutor);

        Task Execute(RunsetExecutor runsetExecutor);

        bool IsFileBasedConfig { get; }
    }


    /// <summary>
    /// in which mode CLI processor was used
    /// </summary>
    public enum eGingerCLIMode
    {
        none, run, grid, config, script, dynamic
    }
}
