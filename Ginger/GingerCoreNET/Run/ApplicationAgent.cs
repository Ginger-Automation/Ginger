#region License
/*
Copyright © 2014-2021 European Support Limited

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
    public class ApplicationAgent : RepositoryItemBase, IApplicationAgent
    {
        private Agent mAgent;

        //Change to target
        private string mAppName;
        [IsSerializedForLocalRepository]
        public string AppName
        {
            get
            {
                return mAppName;
            }
            set
            {
                if (mAppName != value)
                {
                    mAppName = value;
                    OnPropertyChanged(nameof(AppName));
                }
            }
        }

        private Guid mAppID;
        [IsSerializedForLocalRepository]
        public Guid AppID
        {
            get
            {
                if (mAppID == Guid.Empty)
                {
                    ApplicationPlatform appPlat = WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => x.AppName == AppName).FirstOrDefault();
                    if (appPlat != null)
                    {
                        mAppID = appPlat.Guid;
                    }
                }
                return mAppID;
            }
            set
            {
                if (mAppID != value)
                {
                    mAppID = value;
                    OnPropertyChanged(nameof(AppID));
                }
            }
        }

        // No need to serialized as it used only in runtime        
        public Agent Agent
        {
            get { return mAgent; }
            set
            {
                bool bTriggerPropertyChange = true;
                if (mAgent != null) mAgent.PropertyChanged -= Agent_OnPropertyChange;
                mAgent = (Agent)value;
                if (mAgent != null)
                {
                    if (mAgent.Name == AgentName)
                    {
                        bTriggerPropertyChange = false;
                    }

                    AgentName = mAgent.Name;
                    mAgent.PropertyChanged += Agent_OnPropertyChange;
                }
                if (bTriggerPropertyChange)
                {
                    OnPropertyChanged(nameof(Agent));
                    OnPropertyChanged(nameof(AgentName));
                    OnPropertyChanged(nameof(AgentID));
                    OnPropertyChanged(nameof(AppAndAgent));
                }
            }
        }

        private string mAgentName;
        [IsSerializedForLocalRepository]
        public string AgentName
        {
            get
            {
                if (Agent != null)
                {
                    if (mAgentName != Agent.Name)
                        mAgentName = Agent.Name;
                }
                else if (string.IsNullOrEmpty(mAgentName))
                {
                    mAgentName = string.Empty;
                }
                return mAgentName;
            }
            set
            {
                if (mAgentName != value)
                {
                    mAgentName = value;
                    OnPropertyChanged(nameof(AgentName));
                }
            }
        }

        private Guid mAgentID;
        [IsSerializedForLocalRepository]
        public Guid AgentID
        {
            get
            {
                if (Agent != null)
                {
                    if (mAgentID != Agent.Guid)
                        mAgentID = Agent.Guid;
                }
                return mAgentID;
            }
            set
            {
                mAgentID = value;
                OnPropertyChanged(nameof(AgentID));
            }
        }

        public string AppAndAgent
        {
            get
            {
                string s = AppName + ":";
                if (mAgent != null)
                {
                    s += mAgent.Name;
                }
                else
                {
                    s += " Agent not defined";
                }
                return s;
            }
        }

        private void Agent_OnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == GingerCore.Agent.Fields.Name)
            {
                OnPropertyChanged(nameof(AgentName));
            }
        }

        public override string ItemName
        {
            get
            {
                return string.Empty;
            }
            set
            {
                return;
            }
        }

        IAgent IApplicationAgent.Agent
        {
            get { return Agent; }
            set { Agent = (Agent)value; }
        }

        public List<IAgent> PossibleAgents
        {
            get
            {
                List<IAgent> possibleAgents = new List<IAgent>();

                //find out the target application platform
                ApplicationPlatform ap = WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => x.AppName == AppName).FirstOrDefault();//todo: make it be based on AppID and not name
                if (ap != null)
                {
                    ePlatformType appPlatform = ap.Platform;

                    //get the solution Agents which match to this platform                     
                    List<Agent> agents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>().Where(x => x.Platform == appPlatform || x.ServiceId == AppName).ToList();
                    if (agents != null)
                    {
                        foreach (IAgent agent in agents)
                        {
                            possibleAgents.Add(agent);
                        }
                    }
                }
                return possibleAgents;
            }
        }
    }
}
