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

namespace Amdocs.Ginger.Common.Run
{
    public class RemoteServiceGrid : RepositoryItemBase
    {
        private string mName;
        [IsSerializedForLocalRepository]
        public string Name { get { return mName; } set { if (mName != value) { mName = value; OnPropertyChanged(nameof(Name)); } } }

        private string mHost;
        [IsSerializedForLocalRepository]
        public string Host { get { return mHost; } set { if (mHost != value) { mHost = value; OnPropertyChanged(nameof(Host)); } } }

        private int mHostPort;
        [IsSerializedForLocalRepository]
        public int HostPort { get { return mHostPort; } set { if (mHostPort != value) { mHostPort = value; OnPropertyChanged(nameof(HostPort)); } } }

        private bool mActive;
        [IsSerializedForLocalRepository]
        public bool Active { get { return mActive; }  set { if (mActive != value) { mActive = value; OnPropertyChanged(nameof(Active)); } } }


        public override string ItemName
        {
            get
            {
                return Name;
            }
            set
            {
                Name = value;
            }
        }
        public override string GetItemType()
        {
            return nameof(RemoteServiceGrid);
        }
    }
}
