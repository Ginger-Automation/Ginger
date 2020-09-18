using ALM_Common.DataContracts;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Activities;
using GingerCore.Variables;
using OctaneSDK.Connector;
using OctaneSDK.Connector.Authentication;
using OctaneSDK.Connector.Credentials;
using OctaneSDK.Services;
using OctaneSDK.Services.RequestContext;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerCore.ALM
{
    public class OctaneCore : ALMCore
    {
        public override ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override ObservableList<Activity> GingerActivitiesRepo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public RestConnector mOctaneRestConnector;
        public EntityService entityService;
        protected LwssoAuthenticationStrategy lwssoAuthenticationStrategy;
        protected WorkspaceContext workspaceContext;
        protected SharedSpaceContext sharedSpaceContext;

        public override bool ConnectALMProject()
        {
            throw new NotImplementedException();
        }

        public override bool ConnectALMServer()
        {
            try
            {
                string host;
                string userName;
                string password;
                Reporter.ToLog(eLogLevel.DEBUG, "Connecting to qTest server");
                mOctaneRestConnector = new RestConnector();
                entityService = new EntityService(mOctaneRestConnector);
                if (!mOctaneRestConnector.IsConnected())
                {
                    string ignoreServerCertificateValidation = "false"; //ConfigurationManager.AppSettings["ignoreServerCertificateValidation"];
                    if (ignoreServerCertificateValidation != null && ignoreServerCertificateValidation.ToLower().Equals("true"))
                    {
                        NetworkSettings.IgnoreServerCertificateValidation();
                    }
                    NetworkSettings.EnableAllSecurityProtocols();

                    host = ALMCore.DefaultAlmConfig.ALMServerURL;

                    // If webAppUrl is empty we do not try to connect.
                    if (string.IsNullOrWhiteSpace(host)) return false;

                    IConnectionInfo connectionInfo;
                    //string clientId = ConfigurationManager.AppSettings["clientId"];
                    //if (clientId != null)
                    //{
                    //    userName = clientId;
                    //    connectionInfo = new APIKeyConnectionInfo(clientId, ConfigurationManager.AppSettings["clientSecret"]);
                    //}
                    //else
                    //{
                    userName = ALMCore.DefaultAlmConfig.ALMUserName;  // "Jinendrag"; //ConfigurationManager.AppSettings["userName"];
                    password = ALMCore.DefaultAlmConfig.ALMPassword;  //"Amdocs@123";//ConfigurationManager.AppSettings["password"];
                        connectionInfo = new UserPassConnectionInfo(userName, password);
                   // }

                    lwssoAuthenticationStrategy = new LwssoAuthenticationStrategy(connectionInfo);
                    var result =  Task.Run(() =>
                    {
                        return  mOctaneRestConnector.Connect(host, lwssoAuthenticationStrategy);
                     
                    });

                    return result.Result;

                  //  return mOctaneRestConnector.IsConnected();


                  


                    //var sharedSpaceId = 1001; // int.Parse(ConfigurationManager.AppSettings["sharedSpaceId"]);
                    //var workspaceId = 4002; //int.Parse(ConfigurationManager.AppSettings["workspaceId"]);

                    //workspaceContext = new WorkspaceContext(sharedSpaceId, workspaceId);
                    //sharedSpaceContext = new SharedSpaceContext(sharedSpaceId);

                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Connecting to qTest server", ex);
                mOctaneRestConnector = null;
                return false;
            }
            return false;
        }

        public override Dictionary<Guid, string> CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, List<ExternalItemFieldBase> defectsFields, bool useREST = false)
        {
            throw new NotImplementedException();
        }

        public override bool DisconnectALMProjectStayLoggedIn()
        {
            throw new NotImplementedException();
        }

        public override void DisconnectALMServer()
        {

            var result = Task.Run(() =>
            {
                return mOctaneRestConnector.DisconnectAsync();

            }).Result;

          
        }

        public override bool ExportExecutionDetailsToALM(BusinessFlow bizFlow, ref string result, bool exectutedFromAutomateTab = false, PublishToALMConfig publishToALMConfig = null)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, string> GetALMDomainProjects(string ALMDomainName)
        {
            var dummy= new Dictionary<string, string>();
            dummy.Add("1", "Dummy");

            return dummy;
        }

        public override List<string> GetALMDomains()
        {
            return new List<string>() { "Test"};
        }

        public override ObservableList<ExternalItemFieldBase> GetALMItemFields(BackgroundWorker bw, bool online, ResourceType resourceType = ResourceType.ALL)
        {
            throw new NotImplementedException();
        }

        public override bool IsServerConnected()
        {
            throw new NotImplementedException();
        }

    
    }
}
