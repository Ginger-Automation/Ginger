#region License
/*
Copyright © 2014-2023 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace GingerCore.Platforms
{
    public class ApplicationAgentOperations : IApplicationAgentOperations
    {
        public ApplicationAgent ApplicationAgent;
        public ApplicationAgentOperations(ApplicationAgent applicationAgent)
        {
            this.ApplicationAgent = applicationAgent;
            this.ApplicationAgent.ApplicationAgentOperations = this;   
        }

        public List<IAgent> PossibleAgents
        {
            get
            {
                List<IAgent> possibleAgents = new List<IAgent>();

                //find out the target application platform
                ApplicationPlatform ap = WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => x.AppName == ApplicationAgent.AppName).FirstOrDefault();//todo: make it be based on AppID and not name
                if (ap != null)
                {
                    ePlatformType appPlatform = ap.Platform;

                    //get the solution Agents which match to this platform                     
                    List<Agent> agents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>().Where(x => x.Platform == appPlatform || x.ServiceId == ApplicationAgent.AppName).ToList();
                    if (agents != null)
                    {
                        foreach (IAgent agent in agents)
                        {
                            if (((Agent)agent).AgentOperations == null)
                            {
                                ((Agent)agent).AgentOperations = new AgentOperations((Agent)agent);
                            }
                            possibleAgents.Add(agent);
                        }
                    }

                    //adding special case for Web on which also Mobile Web Agents are allowed
                    if(appPlatform == ePlatformType.Web)
                    {
                        List<Agent> mobileAgents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>().Where(x => x.Platform == ePlatformType.Mobile).ToList();
                        if (mobileAgents != null)
                        {
                            foreach (IAgent mobileAgent in mobileAgents)
                            {
                                if (((Agent)mobileAgent).DriverConfiguration.Where(x => x.ItemName == "AppType" && x.Value == "Web").FirstOrDefault() != null)
                                {
                                    if (((Agent)mobileAgent).AgentOperations == null)
                                    {
                                        ((Agent)mobileAgent).AgentOperations = new AgentOperations((Agent)mobileAgent);
                                    }
                                    possibleAgents.Add(mobileAgent);
                                }
                            }
                        }
                    }
                }
                return possibleAgents;
            }
        }
    }
}
