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
using System;

namespace GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib
{
    public class ApplicationPlatform : RepositoryItemBase
    {
        [IsSerializedForLocalRepository]
        public Guid GUID { get; set; }

        // AppName can be NotePad, but if we have several running in same flow it will be Notepad1, Notepad2, Notepad3        
        [IsSerializedForLocalRepository]
        public string AppName { get; set; }

        // Core is the generic name of the application like: Notepad, we will search packaaged based on core app        
        [IsSerializedForLocalRepository]
        public string Core { get; set; }

        [IsSerializedForLocalRepository]
        public string CoreVersion { get; set; }

        [IsSerializedForLocalRepository]
        public Guid CoreGUID { get; set; }

        [IsSerializedForLocalRepository]
        public ePlatformType Platform { get; set; }

        [IsSerializedForLocalRepository]
        public string Description { get; set; }

        // No need to serialzed used for temp selection only
        public bool Selected { get; set; }

        // Save the last agent who executed on this App for reloading it as the defult selection next time
        public string LastMappedAgentName { get; set; }

        public string NameBeforeEdit;


        public override string GetNameForFileName()
        {
            return AppName;
        }

        public override string ItemName
        {
            get
            {
                return this.AppName;
            }
            set
            {
                this.AppName = value;
            }
        }
    }
}
