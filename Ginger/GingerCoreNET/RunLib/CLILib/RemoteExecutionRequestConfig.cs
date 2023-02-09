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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.RunLib.DynamicExecutionLib;
using Ginger.Run;
using Ginger.SolutionGeneral;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public class RemoteExecutionRequestConfig : ICLI
    {
        public string Verb => RequestAPIOptions.Verb;

        public string FileExtension => throw new NotImplementedException();

        public bool IsFileBasedConfig => false;

        public string CreateConfigurationsContent(Solution solution, RunsetExecutor runsetExecutor, CLIHelper cliHelper)
        {
            string json = DynamicExecutionManager.CreateRemoteExecutionRequestJSON(solution, runsetExecutor, cliHelper);
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }

        public string ExecuteFromRunsetShortCutWizard(string executionServiceUrl, string cliContent)
        {
            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(cliContent, Encoding.UTF8, "application/json");

                if (!executionServiceUrl.EndsWith("/"))
                {
                    executionServiceUrl = executionServiceUrl + "/";
                }
                var response = client.PostAsync(executionServiceUrl + "api/v1/executions", content);
                response.Wait(20000);
                
                var responseString = response.Result.ToString();
                if (response.Result.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to start process. error :" + responseString);
                    return response.Result.ReasonPhrase;
                }
                Reporter.ToLog(eLogLevel.DEBUG, responseString);

                Reporter.ToLog(eLogLevel.INFO,"Remote execution request created with "+ response.Result.Headers.Location.ToString().Split('?')[1]);

                return response.Result.StatusCode.ToString();
            }
        }
        public Task Execute(RunsetExecutor runsetExecutor)
        {
            throw new NotImplementedException();
        }

        public void LoadGeneralConfigurations(string content, CLIHelper cliHelper)
        {
            throw new NotImplementedException();
        }

        public void LoadRunsetConfigurations(string content, CLIHelper cliHelper, RunsetExecutor runsetExecutor)
        {
            throw new NotImplementedException();
        }
    }
}
