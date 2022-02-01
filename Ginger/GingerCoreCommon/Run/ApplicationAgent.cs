﻿#region License
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
        public IApplicationAgentOperations ApplicationAgentOperations;
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
                return mAppID;// ApplicationAgentOperations.GetAppID(mAppID); // need to fix this
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
                if (mAgent != null)
                    mAgent.PropertyChanged -= Agent_OnPropertyChange;

                mAgent = value;
                if (mAgent != null)
                {
                    AgentName = mAgent.Name;
                    mAgent.PropertyChanged += Agent_OnPropertyChange;
                }
                OnPropertyChanged(nameof(Agent));
                OnPropertyChanged(nameof(AgentName));
                OnPropertyChanged(nameof(AgentID));
                OnPropertyChanged(nameof(AppAndAgent));
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
            if (e.PropertyName == nameof(Agent.Name))
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
                return ApplicationAgentOperations.PossibleAgents;
            }
        }
    }
}
