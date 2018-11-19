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
    public class UnixServer : RepositoryItemBase
    {        

        public  static class Fields
        {
            public static string Name = "Name";
            public static string Description = "Description";
            public static string Host = "Host";
            public static string Username = "Username";
            public static string Password = "Password";
            public static string PrivateKey = "PrivateKey";
            public static string PrivateKeyPassPhrase = "PrivateKeyPassPhrase";
            public static string RootPath = "RootPath";
        }

        [IsSerializedForLocalRepository]
        public string Name { get; set; }

        [IsSerializedForLocalRepository]
        public string Description { get; set; }

        [IsSerializedForLocalRepository]
        public string Host { get; set; }

        [IsSerializedForLocalRepository]
        public string Username { get; set; }

        [IsSerializedForLocalRepository]
        public string PrivateKey { get; set; }

        [IsSerializedForLocalRepository]
        public string PrivateKeyPassPhrase { get; set; }

        [IsSerializedForLocalRepository]
        public string Password { get; set; }

        [IsSerializedForLocalRepository]
        public string RootPath { get; set; }

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
