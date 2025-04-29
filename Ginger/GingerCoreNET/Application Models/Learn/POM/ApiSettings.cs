namespace Amdocs.Ginger.CoreNET.Application_Models
{
    public class ApiSettings
    {
        public string ApiKey { get; set; }

        public string endpoint { get; set; }
        public string deploymentName { get; set; }
        public string apiVersion { get; set; }

        public string Modelname { get; set; }

        public string Prompt { get; set; }

        public string UserPrompt { get; set; }


    }


    public class Root
    {
        public ApiSettings ApiSettings { get; set; }
    }

}
