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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace GingerCoreCommonTest.Repository
{
    public class MyComplextRepositoryItemChild : RepositoryItemBase
    {

        public override string ItemName { get { return Name; } set => throw new NotImplementedException(); }

        string mName;
        

        [IsSerializedForLocalRepository]
        public string Name
        {
            get { return mName; }
            set
            {
                if (mName != value)
                {
                    mName = value;
                    OnPropertyChanged(nameof(MyComplextRepositoryItem.Name));
                }
            }
        }
        [IsSerializedForLocalRepository]
        public string Description { get; set; }


        // Not serialized/not saved
        string mStatus;
        public string Status
        {
            get { return mStatus; }
            set
            {
                if (mStatus != value)
                {
                    mStatus = value;
                    OnPropertyChanged(nameof(MyComplextRepositoryItem.Status));
                }
            }
        }

    }
}
