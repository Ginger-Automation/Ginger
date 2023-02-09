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

using Amdocs.Ginger.Repository;
using GingerCore;

namespace Ginger.Support
{
    public class SupportFile : RepositoryItemBase
    {
        public enum eFileType
        {
            Word = 1,
            HTML = 2,
            Video = 3,
            PDF = 4
        }

        public enum eTargetUser
        {
            RnD = 1,
            Developer = 2,
            Tester = 3
        }
      

        [IsSerializedForLocalRepository]
        public string Title { get; set; }

        [IsSerializedForLocalRepository]
        public string Description { get; set; }

        [IsSerializedForLocalRepository]
        public string Notes { get; set; }

        [IsSerializedForLocalRepository]
        public string FileLocation { get; set; }

        public override string ItemName
        {
            get
            {
                return this.Title;
            }
            set
            {
                this.Title = value;
            }
        }
    }
}
