#region License
/*
Copyright © 2014-2019 European Support Limited

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
using Amdocs.Ginger.Common.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Environments;
using GingerCore.Platforms;
using GingerCoreNET;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.ComponentModel;
using System.Linq;

namespace Amdocs.Ginger.Common
{
    public class Context : INotifyPropertyChanged
    {
        private GingerRunner mRunner;
        public GingerRunner Runner
        {
            get {
                return mRunner;
            }
            set {
                if(mRunner != value)
                {
                    mRunner = value;
                    SetAgent();
                    OnPropertyChanged(nameof(Runner));
                }
            }
        }
        
        private ProjEnvironment mEnvironment;
        public ProjEnvironment Environment
        {
            get
            {
                return mEnvironment;
            }
            set
            {
                if (mEnvironment != value)
                {
                    mEnvironment = value;
                    OnPropertyChanged(nameof(Environment));
                }
            }
        }

        private BusinessFlow mBusinessFlow;
        public BusinessFlow BusinessFlow
        {
            get
            {
                return mBusinessFlow;
            }
            set
            {
                if (mBusinessFlow != value)
                {
                    mBusinessFlow = value;
                    SetAgent();
                    OnPropertyChanged(nameof(BusinessFlow));
                }
            }
        }

        private Activity mActivity;
        public Activity Activity
        {
            get
            {
                return mActivity;
            }
            set
            {
                if (mActivity != value)
                {
                    mActivity = value;
                    if (mActivity != null)
                    {
                        mActivity.PropertyChanged += Activity_PropertyChanged;
                        TargetApplication = mActivity.TargetApplication;
                        ActivityPlatform = (from x in WorkSpace.Instance.Solution.ApplicationPlatforms
                                            where x.AppName == mActivity.TargetApplication
                                            select x.Platform).FirstOrDefault();
                        SetAgent();
                    }
                    else
                    {
                        TargetApplication = string.Empty;
                        ActivityPlatform = ePlatformType.NA;
                    }
                    OnPropertyChanged(nameof(Activity));
                }
            }
        }
        
        private TargetBase mTarget;
        public TargetBase Target
        {
            get
            {
                return mTarget;
            }
            set
            {
                if (mTarget != value)
                {
                    mTarget = value;
                    OnPropertyChanged(nameof(Target));
                }
            }
        }

        private string mTargetApplication;
        public string TargetApplication
        {
            get
            {
                return mTargetApplication;
            }
            set
            {
                if (mTargetApplication != value)
                {
                    mTargetApplication = value;
                    OnPropertyChanged(nameof(TargetApplication));
                }
            }
        }

        private ApplicationAgent mAppAgent;
        public ApplicationAgent AppAgent
        {
            get
            {
                return mAppAgent;
            }
            set
            {
                if (mAppAgent != value)
                {
                    mAppAgent = value;
                    mAppAgent.PropertyChanged += AppAgent_PropertyChanged;
                    Agent = mAppAgent.Agent;
                    OnPropertyChanged(nameof(AppAgent));
                }
            }
        }
        
        private Agent mAgent;
        public Agent Agent
        {
            get
            {
                return mAgent;
            }
            set
            {
                if (mAgent != value)
                {
                    mAgent = value;
                    mAgent.PropertyChanged += Agent_PropertyChanged;
                    OnPropertyChanged(nameof(Agent));
                }
            }
        }
        
        private string mAgentStatus;
        public string AgentStatus
        {
            get
            {
                return mAgentStatus;
            }
            set
            {
                if (mAgentStatus != value)
                {
                    mAgentStatus = value;
                    OnPropertyChanged(nameof(AgentStatus));
                }
            }
        }

        private ePlatformType mActivityPlatform;
        public ePlatformType ActivityPlatform
        {
            get
            {
                return mActivityPlatform;
            }
            set
            {
                if (mActivityPlatform != value)
                {
                    mActivityPlatform = value;
                    OnPropertyChanged(nameof(ActivityPlatform));
                }
            }
        }

        /// <summary>
        /// This event is used to handle the Activity's TargetApplciation changed functionality
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Activity_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Activity.TargetApplication))
            {
                TargetApplication = Convert.ToString(((Activity)sender).TargetApplication);
            }
        }

        /// <summary>
        /// This event is used to handle the Agent's Status changed functionality
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Agent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Agent.Status))
            {
                AgentStatus = Convert.ToString(((Agent)sender).Status);
            }
        }

        /// <summary>
        /// This event is used to handle the ApplicationAgents Agent changed functionality
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppAgent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Agent))
            {
                Agent = ((ApplicationAgent)sender).Agent;
            }
        }

        /// <summary>
        /// This method is used to set the current agent for the activity selected
        /// </summary>
        private void SetAgent()
        {
            if (BusinessFlow != null && Runner != null && BusinessFlow.CurrentActivity != null)
            {
                AppAgent = AgentHelper.GetAppAgent(Activity, Runner, this);
            }
        }

        public static Context GetAsContext(object contextObj)
        {
            if (contextObj != null && contextObj is Context)
            {
                return (Context)contextObj;
            }
            else
            {
                return null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
