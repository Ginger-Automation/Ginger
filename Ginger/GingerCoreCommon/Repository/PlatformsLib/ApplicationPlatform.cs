#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using System;

namespace GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib
{
    public class ApplicationPlatform : RepositoryItemBase
    {
        //[IsSerializedForLocalRepository]
        //public Guid GUID { get; set; }//Need to be deleted, conflicts with base 'Guid' property
        
        string mAppName;
        [IsSerializedForLocalRepository]
        public string AppName
        {
            get
            {
                return mAppName;
            }
            set
            {
                if (mAppName != value)
                {
                    mAppName = value;
                    OnPropertyChanged(nameof(AppName));
                }
            }
        }

        // Core is the generic name of the application like: Notepad, we will search packaged based on core app                
        public string Core { get; set; }

        string mCoreVersion;
        [IsSerializedForLocalRepository]
        public string CoreVersion
        {
            get
            {
                return mCoreVersion;
            }
            set
            {
                if (mCoreVersion != value)
                {
                    mCoreVersion = value;
                    OnPropertyChanged(nameof(CoreVersion));
                }
            }
        }

        //[IsSerializedForLocalRepository]
        //public Guid CoreGUID { get; set; }//needs to be deleted

        ePlatformType mPlatform;
        [IsSerializedForLocalRepository]
        public ePlatformType Platform
        {
            get
            {
                return mPlatform;
            }
            set
            {
                if (mPlatform != value)
                {
                    mPlatform = value;
                    OnPropertyChanged(nameof(Platform));
                    // notify UI that the description changed as well
                    OnPropertyChanged(nameof(PlatformDescription));
                }
            }
        }

        public string PlatformDescription
        {
            get
            {
                try
                {
                    // Return human friendly description if available, otherwise enum name
                    return Amdocs.Ginger.Common.GeneralLib.General.GetEnumValueDescription(typeof(ePlatformType), this.Platform);
                }
                catch
                {
                    return this.Platform.ToString();
                }
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    return;

                // If value is already the enum name, try parse directly
                if (Enum.TryParse<ePlatformType>(value, ignoreCase: true, out var parsedByName))
                {
                    if (this.Platform != parsedByName)
                    {
                        this.Platform = parsedByName;
                        OnPropertyChanged(nameof(Platform));
                        OnPropertyChanged(nameof(PlatformDescription));
                    }
                    return;
                }

                // Try to match the provided description to an enum value
                foreach (ePlatformType p in Enum.GetValues(typeof(ePlatformType)))
                {
                    string desc;
                    try
                    {
                        desc = Amdocs.Ginger.Common.GeneralLib.General.GetEnumValueDescription(typeof(ePlatformType), p);
                    }
                    catch
                    {
                        desc = p.ToString();
                    }

                    if (!string.IsNullOrEmpty(desc) && string.Equals(desc, value, StringComparison.OrdinalIgnoreCase))
                    {
                        if (this.Platform != p)
                        {
                            this.Platform = p;
                            OnPropertyChanged(nameof(Platform));
                            OnPropertyChanged(nameof(PlatformDescription));
                        }
                        return;
                    }
                }

                // If nothing matched - ignore silently (keeps previous safe state)
            }
        }
        string mDescription;
        [IsSerializedForLocalRepository]
        public string Description
        {
            get
            {
                return mDescription;
            }
            set
            {
                if (mDescription != value)
                {
                    mDescription = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

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

        public eImageType PlatformImage
        {
            get
            {
                return GetPlatformImage(mPlatform);
            }
        }



        public static eImageType GetPlatformImage(ePlatformType platformType = ePlatformType.NA)
        {
            return platformType switch
            {
                ePlatformType.NA => eImageType.Question,
                ePlatformType.Web => eImageType.Globe,
                ePlatformType.WebServices => eImageType.Exchange,
                ePlatformType.Java => eImageType.Java,
                ePlatformType.Mobile => eImageType.MultipleScreen,
                ePlatformType.Windows => eImageType.WindowsIcon,
                ePlatformType.PowerBuilder => eImageType.Runing,
                ePlatformType.DOS => eImageType.Dos,
                ePlatformType.VBScript => eImageType.CodeFile,
                ePlatformType.Unix => eImageType.Linux,
                ePlatformType.MainFrame => eImageType.Server,
                ePlatformType.ASCF => eImageType.Screen,
                ePlatformType.Service => eImageType.Retweet,
                _ => eImageType.Empty,
            };
        }

        public override string GetItemType()
        {
            return "ApplicationPlatform";
        }
        public override string ItemNameField
        {
            get
            {
                return ItemName;
            }
        }

        private string mGingerOpsAppId; //GingerOps Application ID
        [IsSerializedForLocalRepository]
        public string GingerOpsAppId { get { return mGingerOpsAppId; } set { if (mGingerOpsAppId != value) { mGingerOpsAppId = value; OnPropertyChanged(nameof(GingerOpsAppId)); } } }
    }
}
