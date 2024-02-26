#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Amdocs.Ginger.Repository;

namespace GingerCore.Activities
{
    public class Consumer : RepositoryItemBase
    {

        private Guid mConsumerGuid;
        [IsSerializedForLocalRepository]
        public Guid ConsumerGuid
        {
            get { return mConsumerGuid; }
            set
            {
                if (mConsumerGuid != value)
                {
                    mConsumerGuid = value;
                    OnPropertyChanged(nameof(ConsumerGuid));
                }
            }
        }




        private string mName;
        public string Name
        {
            get
            {
                return mName;
            }
            set
            {
                if (mName != value)
                {
                    mName = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public override string ItemName
        {
            get
            {
                return this.Name;
            }
            set
            {
                this.Name = value;
            }
        }
    }
}
