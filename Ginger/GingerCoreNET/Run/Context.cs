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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Amdocs.Ginger.Common.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Environments;

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
