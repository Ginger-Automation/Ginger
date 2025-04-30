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
