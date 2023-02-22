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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Repository;

namespace GingerCoreCommonTest.Repository
{
    public class MyComplextRepositoryItem : RepositoryItemBase 
    {
        public enum etatus
        {
            Unknown,
            Candidate,
            Development,
            Active,
            Suspended,
            Retired
        }

        private string mName;

        [IsSerializedForLocalRepository]
        public string Name  {get { return mName; }
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

        [IsSerializedForLocalRepository]
        public etatus Status{ get; set; }

        public string DontSaveMe { get; set; }

        public string notserinodirtytrack { get; set; }

        [IsSerializedForLocalRepository]
        public ObservableList<MyComplextRepositoryItemChild> childs { get; set; }

        //TODO: remove - we use property
        public override string ItemName { get { return Name; } set { Name = value; } }
        
        public MyComplextRepositoryItem()
        {
            //DirtyHelper.StartDirtyTracking(this);
        }

        public MyComplextRepositoryItem(string Name)
        {
            this.Name = Name;
        }

      
    }
}
