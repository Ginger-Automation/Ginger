#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Ginger.ExecuterService.Contracts;
using Ginger.ExecuterService.Contracts.V1.ExecutionConfiguration;
using Ginger.Run;
using GingerCore;
using GingerCore.Platforms;
using NUglify.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicExecutionLib
{
    internal class AgentConfigOperations
    {
        /// <summary>
        /// Converts a list of GingerRunners to a list of AgentConfig.
        /// </summary>
        /// <param name="GingerRunners">The list of GingerRunners to convert.</param>
        /// <returns>A list of AgentConfig.</returns>
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
                                AgentType = (eAgentDriverType)applicationAgent.Agent.AgentType,
                                PlatformType = Enum.TryParse(applicationAgent.AppPlatformName, out eTartgetPlatformType platformType) ? platformType : eTartgetPlatformType.NA,
                                DriverType = (eDriverType)applicationAgent.Agent.DriverType,
                                DriverParameter = new List<DriverParameters>()
                            };

                            foreach (var para in applicationAgent.Agent.DriverConfiguration)
                            {
                                var driverParameters = new DriverParameters()
                                {
                                    Name = para.Parameter
                                };
                                if (string.IsNullOrEmpty(para.Value))
                                {
                                    driverParameters.Value = "";
                                }
                                else
                                {
                                    driverParameters.Value = para.Value;
                                }
                                agentconfig.DriverParameter.Add(driverParameters);
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
        /// <summary>
        /// Checks if the names in the list are unique.
        /// </summary>
        /// <typeparam name="T">The type of the items in the list.</typeparam>
        /// <param name="List">The list to check.</param>
        /// <exception cref="InvalidOperationException">Thrown if the names are not unique.</exception>
        public static void CheckIfNameIsUnique<T>(IEnumerable<T> List)
        {
            var distinctCount = List.Select((agent) =>
            {
                var nameProperty = typeof(T).GetProperty("Name");
                if (nameProperty == null)
                {
                    throw new InvalidOperationException($"Type {typeof(T).Name} does not contain a property named 'Name'");
                }
                return nameProperty.GetValue(agent);
            }).Distinct().Count();

            var actualCount = List.Count();
            if (distinctCount != actualCount)
            {
                throw new InvalidOperationException($"{typeof(T).Name.Replace("Config", "")} Name should be distinct in the list");
            }
        }

        /// <summary>
        /// Updates the details of existing agents.
        /// </summary>
        /// <param name="ExistingAgentsFromJson">The existing agents to update.</param>
        /// <param name="AllAgentInGinger">The list of all agents in Ginger.</param>
        public static void UpdateExistingAgentDetails(IEnumerable<AgentConfig> ExistingAgentsFromJson, ObservableList<Agent> AllAgentInGinger)
        {
            ExistingAgentsFromJson?.ForEach((jsonAgent) =>
            {
                var existingAgentFromGinger = DynamicExecutionManager.FindItemByIDAndName(new Tuple<string, Guid?>(nameof(Agent.Guid), jsonAgent.Guid), new Tuple<string, string>(nameof(Agent.Name), jsonAgent.Name), AllAgentInGinger);

                if (existingAgentFromGinger != null)
                {
                    existingAgentFromGinger.Name = jsonAgent.Name;
                    existingAgentFromGinger.Guid = jsonAgent.Guid;
                    existingAgentFromGinger.AgentType = (Agent.eAgentType)jsonAgent.AgentType;
                    existingAgentFromGinger.DriverType = (Agent.eDriverType)jsonAgent.DriverType;
                    existingAgentFromGinger.Platform = (GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ePlatformType)jsonAgent.PlatformType;
                    foreach (var newParam in jsonAgent.DriverParameter)
                    {
                        var existingParam = existingAgentFromGinger.DriverConfiguration.FirstOrDefault(p => p.Parameter == newParam.Name);
                        if (existingParam != null && existingParam.Value != newParam.Value)
                        {
                            existingParam.Value = newParam.Value;
                        }
                    }

                }
            });
        }

        /// <summary>
        /// Adds new agent details.
        /// </summary>
        /// <param name="NewAgentsInConfig">The new agents to add.</param>
        /// <param name="AllAgentInGinger">The list of all agents in Ginger.</param>
        public static void AddNewAgentDetails(IEnumerable<AgentConfig> NewAgentsInConfig, ObservableList<Agent> AllAgentInGinger)
        {
            NewAgentsInConfig?.ForEach((agent) =>
            {
                ValidateAgentConfig(AllAgentInGinger, agent);
                var newAgent = new Agent()
                {
                    Name = agent.Name,
                    Guid = agent.Guid,
                    AgentType = (Agent.eAgentType)agent.AgentType,
                    DriverType = (Agent.eDriverType)agent.DriverType,
                    Platform = (GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ePlatformType)agent.PlatformType
                };

                newAgent.DriverConfiguration = new ObservableList<DriverConfigParam>(
                        agent.DriverParameter.Select(dp => new DriverConfigParam
                        {
                            Parameter = dp.Name,
                            Value = dp.Value
                        })
                    );

                AllAgentInGinger.Add(newAgent);

            });

        }
        /// <summary>
        /// Validates the agent configuration.
        /// </summary>
        /// <param name="AllAgentsInGinger">The list of all agents in Ginger.</param>
        /// <param name="VirtualAgent">The virtual agent to validate.</param>
        /// <exception cref="InvalidOperationException">Thrown if the agent already exists.</exception>
        public static void ValidateAgentConfig(ObservableList<Agent> AllAgentsInGinger, AgentConfig VirtualAgent)
        {
            var doesAgentExist = AllAgentsInGinger.Any(existingAgent =>existingAgent.Guid.Equals(VirtualAgent.Guid) ||existingAgent.Name.Equals(VirtualAgent.Name));

            if (doesAgentExist)
            {
                throw new InvalidOperationException($"The Agent {VirtualAgent.Name} already exists in Ginger. Please make sure that the Name and GUID is unique");
            }
        }

    }
}
