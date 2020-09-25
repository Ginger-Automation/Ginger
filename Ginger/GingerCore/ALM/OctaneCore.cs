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
using Octane_Repository;
using ALM_Common.Data_Contracts;
using Octane_Repository.BLL;
using OctaneSDK.Entities.Base;
using OctaneSDK.Entities.WorkItems;
using OctaneSDK.Entities.Tests;
using OctaneSDK.Entities.Requirements;

namespace GingerCore.ALM
{
    public class OctaneCore : ALMCore
    {
        public override ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override ObservableList<Activity> GingerActivitiesRepo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ProjectArea ProjectArea { get; private set; }

        public RestConnector mOctaneRestConnector;
        public EntityService entityService;
        protected LwssoAuthenticationStrategy lwssoAuthenticationStrategy;
        protected WorkspaceContext workspaceContext;
        protected SharedSpaceContext sharedSpaceContext;
        protected OctaneRepository octaneRepository;

        public override bool ConnectALMProject()
        {
            return this.ConnectALMServer();
        }

        public override bool ConnectALMServer()
        {
            try
            {
                if (octaneRepository == null)
                    octaneRepository = new OctaneRepository();
                Reporter.ToLog(eLogLevel.DEBUG, "Connecting to Octane server");
                return Task.Run(() =>
                    {
                        return octaneRepository.IsLoginValid(
                            new LoginDTO()
                            {
                                User = ALMCore.DefaultAlmConfig.ALMUserName,
                                Password = ALMCore.DefaultAlmConfig.ALMPassword,
                                Server = ALMCore.DefaultAlmConfig.ALMServerURL
                            });
                    }).Result;
                //mOctaneRestConnector = new RestConnector();
                //entityService = new EntityService(mOctaneRestConnector);
                //if (!mOctaneRestConnector.IsConnected())
                //{
                //    string ignoreServerCertificateValidation = "false"; //ConfigurationManager.AppSettings["ignoreServerCertificateValidation"];
                //    if (ignoreServerCertificateValidation != null && ignoreServerCertificateValidation.ToLower().Equals("true"))
                //    {
                //        NetworkSettings.IgnoreServerCertificateValidation();
                //    }
                //    NetworkSettings.EnableAllSecurityProtocols();                  

                //    // If webAppUrl is empty we do not try to connect.
                //    if (string.IsNullOrWhiteSpace(ALMCore.DefaultAlmConfig.ALMServerURL)) return false;

                //    IConnectionInfo connectionInfo;

                //    connectionInfo = new UserPassConnectionInfo(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword);

                //    lwssoAuthenticationStrategy = new LwssoAuthenticationStrategy(connectionInfo);
                //    var result =  Task.Run(() =>
                //    {
                //        return  mOctaneRestConnector.Connect(ALMCore.DefaultAlmConfig.ALMServerURL, lwssoAuthenticationStrategy);

                //    });
                //    return result.Result;       

                //}
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Connecting to Octane server", ex);
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
                this.octaneRepository.DisconnectProject();
                return true;
            }).Result;
        }

        public override bool ExportExecutionDetailsToALM(BusinessFlow bizFlow, ref string result, bool exectutedFromAutomateTab = false, PublishToALMConfig publishToALMConfig = null)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, string> GetALMDomainProjects(string ALMDomainName)
        {
            AlmResponseWithData<AlmDomainColl> domains = Task.Run(() =>
            {
                return octaneRepository.GetLoginProjects(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL);
            }).Result;
            return domains.DataResult.Where(f => f.DomainName.Equals(ALMDomainName)).FirstOrDefault().Projects.ToDictionary(project => project.ProjectName, project => project.ProjectName);
        }

        public override List<string> GetALMDomains()
        {
            AlmResponseWithData<AlmDomainColl> domains = Task.Run(() =>
            {
                return octaneRepository.GetLoginProjects(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL);
            }).Result;

            return domains.DataResult.Select(f => f.DomainName).ToList();
        }

        public override ObservableList<ExternalItemFieldBase> GetALMItemFields(BackgroundWorker bw, bool online, ResourceType resourceType = ResourceType.ALL)
        {
            ObservableList<ExternalItemFieldBase> fields = new ObservableList<ExternalItemFieldBase>();
            string resourse = string.Empty;
            AlmResponseWithData<AlmDomainColl> domains = Task.Run(() =>
            {
                return octaneRepository.GetLoginProjects(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL);
            }).Result;
            AlmDomain domain = domains.DataResult.Where(f => f.DomainName.Equals(ALMCore.DefaultAlmConfig.ALMDomain)).FirstOrDefault();
            ProjectArea project = domain.Projects.Where(p => p.ProjectName.Equals(ALMCore.DefaultAlmConfig.ALMProjectName)).FirstOrDefault();
            LoginDTO loginDto = new LoginDTO()
            {
                User = ALMCore.DefaultAlmConfig.ALMUserName,
                Password = ALMCore.DefaultAlmConfig.ALMPassword,
                Server = ALMCore.DefaultAlmConfig.ALMServerURL,
                SharedSpaceId = domain.DomainId,
                WorkSpaceId = project.ProjectId
            };
            Dictionary<string, List<string>> listnodes = Task.Run(() =>
            {
                return octaneRepository.GetListNodes(loginDto);
            }).Result;

            if (resourceType == ALM_Common.DataContracts.ResourceType.ALL)
            {
                resourse = Octane_Repository.BLL.Extensions.ConvertResourceType(ResourceType.TEST_CASE);
                fields.Append(
                    AddFieldsValues(octaneRepository.GetEntityFields(resourse, loginDto), "Test Case",
                    listnodes, octaneRepository.GetPhases(loginDto, resourse)));

                resourse = Octane_Repository.BLL.Extensions.ConvertResourceType(ResourceType.TEST_SET);
                fields.Append(
                    AddFieldsValues(octaneRepository.GetEntityFields(resourse, loginDto), "Test Suit",
                    listnodes, octaneRepository.GetPhases(loginDto, resourse)));

                resourse = Octane_Repository.BLL.Extensions.ConvertResourceType(ResourceType.REQUIREMENT);
                fields.Append(
                    AddFieldsValues(
                        octaneRepository.GetEntityFields(resourse, loginDto), "Requirement",
                        listnodes, octaneRepository.GetPhases(loginDto, resourse)));

                resourse = Octane_Repository.BLL.Extensions.ConvertResourceType(ResourceType.TEST_RUN);
                fields.Append(
                    AddFieldsValues(
                        octaneRepository.GetEntityFields(resourse, loginDto), "Run",
                        listnodes, octaneRepository.GetPhases(loginDto, resourse)));
            }
            else
            {

                resourse = Octane_Repository.BLL.Extensions.ConvertResourceType(resourceType);

                fields.Append(AddFieldsValues(Task.Run(() =>
                {
                    return octaneRepository.GetEntityFields(resourse, loginDto);
                }).Result, resourse, listnodes, Task.Run(() =>
                {
                    return octaneRepository.GetPhases(loginDto, resourse);
                }).Result));
            }
            return fields;
        }

        private static ObservableList<ExternalItemFieldBase> AddFieldsValues(ListResult<FieldMetadata> entityFields, string entityType, Dictionary<string, List<string>> listnodes, Dictionary<string, List<string>> phases)
        {
            ObservableList<ExternalItemFieldBase> fields = new ObservableList<ExternalItemFieldBase>();

            if ((entityFields != null) && (entityFields.total_count.Value > 0))
            {
                foreach (FieldMetadata field in entityFields.data)
                {
                    if (string.IsNullOrEmpty(field.Label) || !field.VisibleInUI)
                    {
                        continue;
                    }

                    ExternalItemFieldBase itemfield = new ExternalItemFieldBase();
                    itemfield.ID = field.Label;
                    itemfield.ExternalID = field.Label;
                    itemfield.Name = field.Name;
                    itemfield.Mandatory = field.IsRequired;
                    itemfield.SystemFieled = !field.IsUserField;
                    
                    if (itemfield.Mandatory)
                    {
                        itemfield.ToUpdate = true;
                    }
                    itemfield.ItemType = entityType;
                    itemfield.Type = field.FieldType;

                    if (listnodes != null && listnodes.ContainsKey(field.Name) && listnodes[field.Name].Any())
                        itemfield.PossibleValues = new ObservableList<string>(listnodes[field.Name]);

                    else if (listnodes != null && listnodes.ContainsKey(entityType.ToLower() + "_" +field.Name) && listnodes[entityType.ToLower() + "_" + field.Name].Any())
                        itemfield.PossibleValues = new ObservableList<string>(listnodes[entityType.ToLower() + "_" + field.Name]);

                    else if (phases != null && phases.ContainsKey(field.Name) && phases[field.Name].Any())
                        itemfield.PossibleValues = new ObservableList<string>(phases[field.Name]);
                    fields.Add(itemfield);
                }
            }
            return fields;
        }

        public override bool IsServerConnected()
        {
            throw new NotImplementedException();
        }


    }
}
