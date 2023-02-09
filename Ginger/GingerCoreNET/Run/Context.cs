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
using Amdocs.Ginger.Common.Repository;
using Ginger.Run;
using Ginger.Run.RunSetActions;
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
    public class Context : INotifyPropertyChanged, IContext
    {
        private IGingerExecutionEngine mRunner;
        public IGingerExecutionEngine Runner
        {
            get
            {
                return mRunner;
            }
            set
            {
                if (mRunner != value)
                {
                    mRunner = value;
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

        private ePlatformType mPlatform;
        public ePlatformType Platform
        {
            get
            {
                return mPlatform;
            }
            set
            {
                if (mPlatform != value)
                {
                    mPlatform = value;
                    OnPropertyChanged(nameof(Platform));
                }
            }
        }

        private RunSetActionBase mRunsetAction;
        public RunSetActionBase RunsetAction
        {
            get
            {
                return mRunsetAction;
            }
            set
            {
                if (mRunsetAction != value)
                {
                    mRunsetAction = value;
                    OnPropertyChanged(nameof(RunsetAction));
                }
            }
        }

        private eExecutedFrom mExecutedFrom;
        public eExecutedFrom ExecutedFrom
        {
            get
            {
                return mExecutedFrom;
            }
            set
            {
                mExecutedFrom = value;
                OnPropertyChanged(nameof(ExecutedFrom));
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
