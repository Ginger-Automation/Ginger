namespace Amdocs.Ginger.Common.GeneralLib
{
    public class ApiSettings
    {
        public string ApiKey { get; set; }

        public string endpoint { get; set; }
        public string deploymentName { get; set; }
        public string apiVersion { get; set; }

        public string Modelname { get; set; }

        public string SystemPrompt { get; set; }

        public string UserPrompt { get; set; }


    }


    public class Root
    {
        public ApiSettings ApiSettings { get; set; }
    }

}
