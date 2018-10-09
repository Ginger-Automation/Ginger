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

namespace GingerCore.Platforms
{
    public class TargetApplication : RepositoryItemBase
    {
        public new static partial class Fields
        {
            public static string Selected = "Selected";
            public static string AppName = "AppName";        
        }

        //TOOD: is it needed to serialized?
        public bool Selected { get; set; }

        //TODO: how about use GUID or add it for in case
        private string mAppName;
        [IsSerializedForLocalRepository]
        public string AppName
        {
            get
            {
                return mAppName;
            }
            set
            {
                mAppName = value;
                OnPropertyChanged(nameof(AppName));
            }
        }

        // Save the last agent who executed on this App, for reports
        [IsSerializedForLocalRepository]
        public string LastExecutingAgentName { get; set; }


        public override string ItemName
        {
            get
            {
                return string.Empty;
            }
            set
            {
                return;
            }
        }
    }
}
