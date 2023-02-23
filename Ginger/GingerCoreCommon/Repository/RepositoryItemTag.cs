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

namespace Amdocs.Ginger.Repository
{
    public class RepositoryItemTag : RepositoryItemBase
    {
        public static class Fields
        {
            public static string Name = "Name";
            public static string Description = "Description";          
        }


        string mName = string.Empty;
        [IsSerializedForLocalRepository]
        public string Name { get { return mName; } set { if (mName != value) { mName = value; OnPropertyChanged(Fields.Name); } } }

        string mDescription = string.Empty;
        [IsSerializedForLocalRepository]
        public string Description
        {
            get { return mDescription; }
            set { if (mDescription != value) { mDescription = value; OnPropertyChanged(Fields.Description); } }
        }


        public override string ItemName { get { return Name; } set { Name = value; } }

        public override string GetItemType()
        {
            return nameof(RepositoryItemTag);
        }
    }
}
