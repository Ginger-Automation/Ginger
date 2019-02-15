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

namespace GingerCore.Environments
{
    public class LoginUser : RepositoryItemBase
    {        

        [IsSerializedForLocalRepository]
        public string UserProfileName { get; set; }

        [IsSerializedForLocalRepository]
        public string Type { get; set; }

        [IsSerializedForLocalRepository]
        public string Username { get; set; }

        [IsSerializedForLocalRepository]
        public string Password { get; set; }

        public override string ItemName
        {
            get
            {
                return this.UserProfileName;
            }
            set
            {
                this.UserProfileName = value;
            }
        }
    }
}
