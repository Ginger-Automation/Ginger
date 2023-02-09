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

using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.Repository.TargetLib
{
    public class TargetPlugin : TargetBase
    {
        public override string Name { get { return mPluginID + "." + mServiceID; } }

        private string mPluginID;
        [IsSerializedForLocalRepository]
        public string PluginId
        {
            get
            {
                return mPluginID;
            }
            set
            {
                if (mPluginID != value)
                {
                    mPluginID = value;
                    OnPropertyChanged(nameof(PluginId));
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private string mServiceID;
        [IsSerializedForLocalRepository]
        public string ServiceId
        {
            get
            {
                return mServiceID;
            }
            set
            {
                if (mServiceID != value)
                {
                    mServiceID = value;
                    OnPropertyChanged(nameof(ServiceId));
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
    }    
}
