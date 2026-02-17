#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
namespace Amdocs.Ginger.Common.GeneralLib
     /// <summary>
    /// Configuration settings for Azure OpenAI API integration
    /// </summary>
{
    public class ApiSettings
    {
        /// <summary>
        /// The API key for authentication with Azure OpenAI service
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// The Azure OpenAI service endpoint URL
        /// </summary>

        public string Endpoint { get; set; }

        /// <summary>
        /// The deployment name configured in Azure OpenAI
        /// </summary>
        public string DeploymentName { get; set; }
        /// <summary>
        /// The API version to use when making requests
        /// </summary>
        public string ApiVersion { get; set; }
        /// <summary>
        /// The name of the model to use for generation
        /// </summary>
        public string Modelname { get; set; }
        /// <summary>
        /// The system prompt to use for AI generation
        /// </summary>
        public string SystemPrompt { get; set; }
        /// <summary>
        /// The user prompt to use for AI generation
        /// </summary>
        public string UserPrompt { get; set; }
    }
    /// <summary>
    /// Root container for API settings in configuration file
    /// </summary>
    public class Root
    {
        /// <summary>
        /// The API settings configuration
        /// </summary>
        public ApiSettings ApiSettings { get; set; }
    }
}
