#region License
/*
Copyright © 2014-2018 European Support Limited

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

using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common;
using System.Linq;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace GingerCore.Platforms
{
    public class Platform : RepositoryItemBase
    {
        public new static partial class Fields
        {            
            public static string Active = "Active";
            public static string PlatformType = "PlatformType";
            public static string AgentName = "AgentName";
            public static string Agent = "Agent";
            public static string App = "App";
        }

        private bool mActive;
        [IsSerializedForLocalRepository]
        public bool Active { get { return mActive; } set { if (mActive != value) { mActive = value; OnPropertyChanged(nameof(Active)); } } }

        private ePlatformType mPlatformType;
        [IsSerializedForLocalRepository]
        public ePlatformType PlatformType { get { return mPlatformType; } set { if (mPlatformType != value) { mPlatformType = value; OnPropertyChanged(nameof(PlatformType)); } } }

        private string mAgentName;
        [IsSerializedForLocalRepository]
        public string AgentName { 
            get 
            {
                if (Agent != null)
                {
                    return this.Agent.Name;
                }
                else
                {
                    return mAgentName;
                }
            }
            set
            {
                mAgentName = value;
            }
        }
       
        // Used when running after mapping done and user click run
        private Agent mAgent;

        public Agent Agent { get { return mAgent; } set 
        { 
            mAgent = value;
            OnPropertyChanged(Fields.Agent); 
            OnPropertyChanged(Fields.AgentName);            
        } }

        public string Description {
            get
            {
                //TODO: switch case, retur nice desc
                return PlatformType.ToString();
            } 
        }

        public void SetDefualtAgent(ObservableList<Agent> Agents)
        {
            if (Agents == null) return;

            // set the first agent matching the platform                        
            Agent a = (from x in Agents where x.Platform == PlatformType select x).FirstOrDefault();
            if (a != null)
            {
                this.Agent = (Agent)a.CreateCopy(false);
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
    }
}
