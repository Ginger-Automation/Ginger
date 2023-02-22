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

using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.WorkSpaceLib;
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
                if (mAppID == Guid.Empty)
                {
                    ApplicationPlatform appPlat = GingerCoreCommonWorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => x.AppName == AppName).FirstOrDefault();
                    if (appPlat != null)
                    {
                        return appPlat.Guid;
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

        ApplicationPlatform mAppPlatform = null;
        public ApplicationPlatform AppPlatform
        {
            get
            {
                if (mAppPlatform == null)
                {
                    mAppPlatform = GingerCoreCommonWorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => x.AppName == AppName).FirstOrDefault();
                }
                return mAppPlatform;
            }
        }

        public eImageType AppPlatformImage
        {
            get
            {
                if (AppPlatform != null)
                {
                    return AppPlatform.PlatformImage;
                }
                else
                {
                    return eImageType.Null;
                }
            }
        }

        public string AppPlatformName
        {
            get
            {
                if (AppPlatform != null)
                {
                    return AppPlatform.Platform.ToString();
                }
                else
                {
                    return ePlatformType.NA.ToString();
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
                {
                    mAgent.PropertyChanged -= Agent_OnPropertyChange;
                }

                mAgent = value;
                if (mAgent != null)
                {
                    // check if the AgentName & Id is diff before setting the value to avoid dirty status to become modified when unnecessary
                    if (AgentName != mAgent.Name)
                    {
                        AgentName = mAgent.Name;
                    }
                    if (AgentID != mAgent.Guid)
                    {
                        AgentID = mAgent.Guid;
                    }
                    mAgent.PropertyChanged += Agent_OnPropertyChange;
                }
                OnPropertyChanged(nameof(Agent));
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
                    mAgentID = Agent.Guid;
                }
                return mAgentID;
            }
            set
            {
                if (mAgentID != value)
                {
                    mAgentID = value;
                    OnPropertyChanged(nameof(AgentID));
                }
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
