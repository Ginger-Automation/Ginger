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
using Amdocs.Ginger.Common;
using System.Linq;

namespace GingerCore.Environments
{
    public class EnvApplication : RepositoryItemBase
    {
        public override bool UseNewRepositorySerializer { get { return true; } }

        public  static class Fields
        {
            public static string Name = "Name";
            public static string CoreProductName = "CoreProductName";
            public static string Description = "Description";
            public static string CoreVersion = "CoreVersion";
            public static string AppVersion = "AppVersion";
            public static string Url = "Url";
            public static string Vendor = "Vendor";
            public static string Active = "Active";
        }

        private string mName { get; set; }
        [IsSerializedForLocalRepository]
        public string Name { get { return mName; } set { if (mName != value) { mName = value; OnPropertyChanged(nameof(Name)); } } }

        [IsSerializedForLocalRepository]
        public string CoreProductName { get; set; }

        [IsSerializedForLocalRepository]
        public string Description { get; set; }

        [IsSerializedForLocalRepository]
        public string CoreVersion { get; set; }

        [IsSerializedForLocalRepository]
        public string AppVersion { get; set; }

        [IsSerializedForLocalRepository]
        public string Url { get; set; }

        [IsSerializedForLocalRepository]
        public string Vendor { get; set; }

        [IsSerializedForLocalRepository]
        public bool Active { get; set; }

        [IsSerializedForLocalRepository]
        public ObservableList<Database> Dbs = new ObservableList<Database>();

        [IsSerializedForLocalRepository]
        public ObservableList<UnixServer> UnixServers = new ObservableList<UnixServer>();

        [IsSerializedForLocalRepository]
        public ObservableList<GeneralParam> GeneralParams = new ObservableList<GeneralParam>();

        [IsSerializedForLocalRepository]
        public ObservableList<LoginUser> LoginUsers = new ObservableList<LoginUser>();


        public override string GetNameForFileName() { return Name; }

        public GeneralParam GetParam(string ParamName)
        {
            GeneralParam GP = (from p in GeneralParams where p.Name == ParamName select p).FirstOrDefault();
            return GP;
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
