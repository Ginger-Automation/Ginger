using Amdocs.Ginger.Common;
using Ginger.ExecuterService.Contracts.V1.ExecutionConfiguration;
using Ginger.Run;
using GingerCore;
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
                                    Name = para.Parameter
                                };
                                if (string.IsNullOrEmpty(para.Value))
                                {
                                    a.Value = "";
                                }
                                else
                                {
                                    a.Value = para.Value;
                                }
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
        public static void CheckIfNameIsUnique<T>(IEnumerable<T> List)
        {
            var distinctCount = List.Select((agent) => typeof(T).GetProperty("Name").GetValue(agent)).Distinct().Count();
            var actualCount = List.Count();
            if (distinctCount != actualCount)
            {
                throw new InvalidOperationException($"{typeof(T).Name.Replace("Config", "")} Name should be distinct in the list");
            }
        }

        public static void UpdateExistingAgentDetails(IEnumerable<AgentConfig> ExistingAgents, ObservableList<Agent> AllAgentInGinger)
        {
            ExistingAgents?.ForEach((agent) =>
            {
                var agentFromGinger = DynamicExecutionManager.FindItemByIDAndName(new Tuple<string, Guid?>(nameof(Agent.Guid), agent.Guid), new Tuple<string, string>(nameof(Agent.Name), agent.Name), AllAgentInGinger);

                if (agentFromGinger != null)
                {
                    agentFromGinger.Name = agent.Name;
                    agentFromGinger.Guid = agent.Guid;
                    agentFromGinger.DriverConfiguration = new ObservableList<DriverConfigParam>(
                        agent.DriverParameter.Select(dp => new DriverConfigParam
                        {
                            Parameter = dp.Name,
                            Value = dp.Value
                        })
                    );
                }
            });
        }

        public static void AddNewAgentDetails(IEnumerable<AgentConfig> NewAgentsInConfig, ObservableList<Agent> AllAgentInGinger)
        {
            NewAgentsInConfig?.ForEach((agent) =>
            {
                ValidateAgentConfig(AllAgentInGinger, agent);
                var newAgent = new Agent()
                {
                    Name = agent.Name,
                    Guid = agent.Guid,
                };

                newAgent.DriverConfiguration = new ObservableList<DriverConfigParam>(
                        agent.DriverParameter.Select(dp => new DriverConfigParam
                        {
                            Parameter = dp.Name,
                            Value = dp.Value
                        })
                    );

                AllAgentInGinger.Add( newAgent );

            });

        }
        public static void ValidateAgentConfig(ObservableList<Agent> AllEnvironmentsInGinger, AgentConfig AgentConfig)
        {
            var doesEnvironmentExist = AllEnvironmentsInGinger.Any((existingEnvironmentInGinger) => existingEnvironmentInGinger.Guid.Equals(AgentConfig.Guid));
            if (!doesEnvironmentExist)
            {
                doesEnvironmentExist = AllEnvironmentsInGinger.Any((existingEnvironmentInGinger) => existingEnvironmentInGinger.Name.Equals(AgentConfig.Name));
            }

            if (doesEnvironmentExist)
            {
                throw new InvalidOperationException($"The Agent {AgentConfig.Name} already exists in Ginger. Please make sure that the Name and GUID is unique");
            }
        }
        
    }
}
