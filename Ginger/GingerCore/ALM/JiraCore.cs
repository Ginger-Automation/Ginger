using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ALM_Common.DataContracts;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Activities;

namespace GingerCore.ALM
{
    public class JiraCore : ALMCore
    {
        public override ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override ObservableList<Activity> GingerActivitiesRepo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override bool ConnectALMProject()
        {
            throw new NotImplementedException();
        }

        public override bool ConnectALMServer()
        {
            throw new NotImplementedException();
        }

        public override Dictionary<Guid, string> CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, bool useREST = false)
        {
            throw new NotImplementedException();
        }

        public override bool DisconnectALMProjectStayLoggedIn()
        {
            throw new NotImplementedException();
        }

        public override void DisconnectALMServer()
        {
            throw new NotImplementedException();
        }

        public override bool ExportExecutionDetailsToALM(BusinessFlow bizFlow, ref string result, bool exectutedFromAutomateTab = false, PublishToALMConfig publishToALMConfig = null)
        {
            throw new NotImplementedException();
        }

        public override List<string> GetALMDomainProjects(string ALMDomainName)
        {
            throw new NotImplementedException();
        }

        public override List<string> GetALMDomains()
        {
            throw new NotImplementedException();
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
