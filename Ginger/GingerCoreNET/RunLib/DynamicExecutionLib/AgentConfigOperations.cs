using Ginger.ExecuterService.Contracts.V1.ExecutionConfiguration;
using Ginger.Run;
using GingerCore.Environments;
using GingerCore.Platforms;
using NPOI.OpenXmlFormats.Dml;
using NUglify.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicExecutionLib
{
    internal class AgentConfigOperations
    {
        public static List<AgentConfig> ConvertToAgentRunsetConfig(IList<GingerRunner> GingerRunners)
        {
            List<AgentConfig> agentConfigList = [];
            foreach (GingerRunner runner in GingerRunners)
            {
                if (runner != null && runner.ApplicationAgents != null)
                {
                    foreach (ApplicationAgent applicationAgent in runner.ApplicationAgents)
                    {
                        if (applicationAgent.Agent == null)
                        {
                            continue; 
                        }
                        else
                        {
                            var agentconfig = new AgentConfig
                            {
                                Name = applicationAgent.Agent.Name,
                                Guid = applicationAgent.Agent.Guid,
                                Exist = true,
                                DriverParameter = new List<DriverParameters>()
                            };

                            foreach (var para in applicationAgent.Agent.DriverConfiguration)
                            {
                                var a = new DriverParameters()
                                {
                                    Name = para.Parameter,
                                    Value = para.Value
                                };
                                agentconfig.DriverParameter.Add(a);
                            }

                            if (!agentConfigList.Any(ac => ac.Guid == agentconfig.Guid))
                            {
                                agentConfigList.Add(agentconfig);
                            }
                        }
                    }
                }
            }

            return agentConfigList;
        }
    }
}
