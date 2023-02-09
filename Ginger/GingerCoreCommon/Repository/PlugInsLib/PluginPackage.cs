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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Actions;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.GeneralLib;
using Newtonsoft.Json;

namespace Amdocs.Ginger.Repository
{
    public class PluginPackage : RepositoryItemBase
    {
        public IPluginPackageOperations PluginPackageOperations;

        [IsSerializedForLocalRepository]
        public string PluginId { get; set;  }

        [IsSerializedForLocalRepository]
        public string PluginPackageVersion { get; set; }


        // When in dev mode we can add package from folder  which will ref a local folder
        string mLocalFolder;
        [IsSerializedForLocalRepository]
        public string LocalFolder
        {
            get
            {
                return mLocalFolder;
            }
            set
            {
                mLocalFolder = value;
                mFolder = value;
            }
        }


        public bool Isloaded = false;
        // must have empty constructor
        public PluginPackage()
        {
        }


        public PluginPackage(string folder)
        {
            mFolder = folder;
        }

        
        string mFolder;
        public string Folder
        {
            get
            {
                if (string.IsNullOrEmpty(mFolder))
                {
                    //if (!string.IsNullOrEmpty(LocalFolder))
                    //{
                    //    mFolder = LocalFolder;  // use for debug or when adding local folder instead of download
                    //}
                    //else
                    //{
                    mFolder = Path.Combine(LocalPluginsFolder, PluginId, PluginPackageVersion);
                    //}
                }

                return mFolder;
            }
            set
            {
                if (mFolder != value)
                {
                    mFolder = value;
                    PluginPackageOperations.LoadInfoFromJSON();
                    PluginPackageOperations.LoadServicesFromJSON();
                    OnPropertyChanged(nameof(Folder));
                }
            }
        }


        public static string LocalPluginsFolder
        {
            get
            {
                //string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                //userFolder = Path.Combine(userFolder, "Ginger", "PluginPackages");
                //return userFolder;
                string folder= Path.Combine(General.LocalUserApplicationDataFolderPath, "PluginsPackages");
                if(Directory.Exists(folder) == false)
                {
                    Directory.CreateDirectory(folder);
                }
                return folder;
            }
        }
      
        public override string ItemName { get { return PluginId; } set {  } }


        public override string GetItemType()
        {
            return nameof(PluginPackage);
        }

        public override string GetNameForFileName()
        {
            return PluginId;
        }


        public override eImageType ItemImageType
        {
            get
            {
                return eImageType.PluginPackage;
            }
        }

        public override string ItemNameField
        {
            get
            {
                return nameof(this.PluginId);
            }
        }


    }
}
