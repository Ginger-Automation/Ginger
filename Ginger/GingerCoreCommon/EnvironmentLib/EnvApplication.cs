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
using System.Collections.Generic;
using System.Linq;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace GingerCore.Environments
{
    public class EnvApplication : RepositoryItemBase
    {

        private string mName;
        [IsSerializedForLocalRepository]
        public string Name { get { return mName; } set { if (mName != value) { mName = value; OnPropertyChanged(nameof(Name)); } } }

        private string mCoreProductName;
        [IsSerializedForLocalRepository]
        public string CoreProductName { get { return mCoreProductName; } set { if (mCoreProductName != value) { mCoreProductName = value; OnPropertyChanged(nameof(CoreProductName)); } } }

        private string mDescription;
        [IsSerializedForLocalRepository]
        public string Description { get { return mDescription; } set { if (mDescription != value) { mDescription = value; OnPropertyChanged(nameof(Description)); } } }

        private string mCoreVersion;
        [IsSerializedForLocalRepository]
        public string CoreVersion { get { return mCoreVersion; } set { if (mCoreVersion != value) { mCoreVersion = value; OnPropertyChanged(nameof(CoreVersion)); } } }

        private string mAppVersion;
        [IsSerializedForLocalRepository]
        public string AppVersion { get { return mAppVersion; } set { if (mAppVersion != value) { mAppVersion = value; OnPropertyChanged(nameof(AppVersion)); } } }

        private string mUrl;
        [IsSerializedForLocalRepository]
        public string Url { get { return mUrl; } set { if (mUrl != value) { mUrl = value; OnPropertyChanged(nameof(Url)); } } }

        private string mVendor;
        [IsSerializedForLocalRepository]
        public string Vendor { get { return mVendor; } set { if (mVendor != value) { mVendor = value; OnPropertyChanged(nameof(Vendor)); } } }

        private bool mActive;
        [IsSerializedForLocalRepository]
        public bool Active { get { return mActive; } set { if (mActive != value) { mActive = value; OnPropertyChanged(nameof(Active)); } } }

        private string mGingerOpsAppId; //Ginger Analytics Application ID
        [IsSerializedForLocalRepository]
        public string GingerOpsAppId { get { return mGingerOpsAppId; } set { if (mGingerOpsAppId != value) { mGingerOpsAppId = value; OnPropertyChanged(nameof(GingerOpsAppId)); } } }

        private string mGingerOpsStatus; //Ginger Analytics import status
        public string GingerOpsStatus { get { return mGingerOpsStatus; } set { if (mGingerOpsStatus != value) { mGingerOpsStatus = value; OnPropertyChanged(nameof(GingerOpsStatus)); } } }

        private string mGingerOpsRemark; //Ginger Analytics Remakrs if any during import
        public string GingerOpsRemark { get { return mGingerOpsRemark; } set { if (mGingerOpsRemark != value) { mGingerOpsRemark = value; OnPropertyChanged(nameof(GingerOpsRemark)); } } }

        [IsSerializedForLocalRepository]
        public ObservableList<IDatabase> Dbs { get; set; } = [];

        [IsSerializedForLocalRepository]
        public ObservableList<UnixServer> UnixServers = [];

        [IsSerializedForLocalRepository]
        public ObservableList<GeneralParam> GeneralParams = [];

        [IsSerializedForLocalRepository]
        public ObservableList<LoginUser> LoginUsers = [];


        public override string GetNameForFileName() { return Name; }

        public GeneralParam GetParam(string ParamName)
        {
            GeneralParam GP = (from p in GeneralParams where p.Name == ParamName select p).FirstOrDefault();
            return GP;
        }
        public VariableBase GetVariable(string ParamName)
        {
            ConvertGeneralParamsToVariable();
            return Variables.FirstOrDefault((variable) => variable.Name.Equals(ParamName));

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
        public override string GetItemType()
        {
            return nameof(EnvApplication);
        }

        public void AddVariable(VariableBase newVar)
        {

            SetUniqueVariableName(newVar);
            Variables.Add(newVar);
        }
        public void SetUniqueVariableName(VariableBase var)
        {
            if (string.IsNullOrEmpty(var.Name))
            {
                var.Name = "Variable";
            }
            if (Variables.FirstOrDefault(x => x.Name == var.Name) == null)
            {
                return; //no name like it
            }
            List<VariableBase> sameNameObjList = this.Variables.Where(x => x.Name == var.Name).ToList<VariableBase>();

            if (sameNameObjList.Count == 1 && sameNameObjList[0] == var)
            {
                return; //Same internal object
            }
            //Set unique name
            int counter = 2;
            while ((Variables.FirstOrDefault(x => x.Name == var.Name + "_" + counter.ToString())) != null)
            {
                counter++;
            }
            var.Name = var.Name + "_" + counter.ToString();
        }

        public void ConvertGeneralParamsToVariable()
        {
            if (GeneralParams == null || GeneralParams.Count == 0)
            {
                return;
            }

            foreach (var generalParam in GeneralParams)
            {
                if (generalParam.Encrypt)
                {
                    var variablePassword = new VariablePasswordString()
                    {
                        Description = generalParam.Description,
                        Name = generalParam.Name,
                        SetAsInputValue = false,
                        SetAsOutputValue = false
                    };

                    variablePassword.SetInitialValue(generalParam.Value);
                    Variables.Add(variablePassword);
                }
                else
                {
                    if (generalParam?.Value != null && generalParam.Value.Contains('{'))
                    {
                        Variables.Add(
                         new VariableDynamic()
                         {
                             Name = generalParam.Name,
                             Description = generalParam.Description,
                             ValueExpression = generalParam.Value,
                             SetAsInputValue = false,
                             SetAsOutputValue = false
                         }
                        );
                    }
                    else
                    {
                        Variables.Add(
                             new VariableString()
                             {
                                 Name = generalParam.Name,
                                 Description = generalParam.Description,
                                 Value = generalParam.Value,
                                 SetAsInputValue = false,
                                 SetAsOutputValue = false
                             }
                            );
                    }

                }
            }
            GeneralParams.Clear();
        }

        private eImageType mItemImageType;
        public override eImageType ItemImageType
        {
            get
            {
                return Platform switch
                {
                    ePlatformType.NA => eImageType.Question,
                    ePlatformType.Web => eImageType.Globe,
                    ePlatformType.WebServices => eImageType.Exchange,
                    ePlatformType.Java => eImageType.Java,
                    ePlatformType.Mobile => eImageType.Mobile,
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

        }

        public override string ItemNameField
        {
            get
            {
                return nameof(this.Name);
            }
        }
        //public ePlatformType Platform
        //{
        //    get;
        //    set;
        //} = ePlatformType.NA;
        private ePlatformType mPlatform = ePlatformType.NA;

        [IsSerializedForLocalRepository]
        public ePlatformType Platform
        {
            get => mPlatform;
            set
            {
                if (mPlatform != value)
                {
                    mPlatform = value;
                    OnPropertyChanged(nameof(Platform));
                    // Notify UI the description changed
                    OnPropertyChanged(nameof(PlatformDescription));
                    // ItemImageType depends on Platform so notify it too
                    OnPropertyChanged(nameof(ItemImageType));
                }
            }
        }

        // Expose the enum description for UI binding (uses existing helper)
        public string PlatformDescription
        {
            get
            {
                try
                {
                    // Use the shared helper that returns EnumValueDescription (falls back to name)
                    return Amdocs.Ginger.Common.GeneralLib.General.GetEnumValueDescription(typeof(ePlatformType), this.Platform);
                }
                catch
                {
                    return this.Platform.ToString();
                }
            }
        }


        public void SetDataFromAppPlatform(ObservableList<ApplicationPlatform> ApplicationPlatforms)
        {
            ApplicationPlatform applicationPlatform = ApplicationPlatforms.FirstOrDefault((app) => app.Guid.Equals(this.ParentGuid) || app.AppName.Equals(this.Name));

            if (applicationPlatform != null)
            {
                this.Name = applicationPlatform.AppName;
                this.Platform = applicationPlatform.Platform;
            }

        }
        [IsSerializedForLocalRepository]
        public readonly ObservableList<VariableBase> Variables = [];
    }
}
