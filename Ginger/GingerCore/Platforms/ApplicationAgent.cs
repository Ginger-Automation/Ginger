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
using System.ComponentModel;

namespace GingerCore.Platforms
{
    public class ApplicationAgent : RepositoryItemBase
    {
        private Agent mAgent;

        public new static partial class Fields
        {
            public static string Selected = "Selected";
            public static string AppName = "AppName";
            public static string AgentName = "AgentName";
            public static string AppAndAgent = "AppAndAgent";
            public static string Agent = "Agent";
        }


        //Change to target
        private string mAppName;
        [IsSerializedForLocalRepository]
        public string AppName {
            get
            {
                return mAppName;
            }
            set
            {
                if (mAppName != value)
                {
                    mAppName = value;
                    OnPropertyChanged(Fields.AppName);
                }
            }
        }
        
        // No need to serialized as it used only in runtime        
        public Agent Agent 
        {
            get { return mAgent; }
            set
            {
                if (mAgent != null) mAgent.PropertyChanged -= Agent_OnPropertyChange;
                mAgent = value;
                if (mAgent != null)
                {
                    AgentName = mAgent.Name;
                    mAgent.PropertyChanged += Agent_OnPropertyChange;
                }
                OnPropertyChanged(Fields.Agent);  
                OnPropertyChanged(Fields.AgentName);  
                OnPropertyChanged(Fields.AppAndAgent);                
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
                mAgentName = value;
                OnPropertyChanged(Fields.AgentName);
            }
        }

        public string AppAndAgent
        {
            get
            {
                string s = AppName + ":" ;
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
            if (e.PropertyName == Agent.Fields.Name)
            {
               OnPropertyChanged(Fields.AgentName);
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
