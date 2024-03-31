#region License
/*
Copyright © 2014-2024 European Support Limited

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
using System.Linq;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using GingerCore.Variables;

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

        [IsSerializedForLocalRepository]
        public ObservableList<IDatabase> Dbs { get; set; } = new ObservableList<IDatabase>();

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
        public VariableBase GetVariable(string ParamName)
        {
            return Variables.FirstOrDefault((variable)=>variable.Name.Equals(ParamName));

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
            if (string.IsNullOrEmpty(var.Name)) var.Name = "Variable";
            if (Variables.FirstOrDefault(x => x.Name == var.Name) == null) return; //no name like it

            List<VariableBase> sameNameObjList =
                this.Variables.Where(x => x.Name == var.Name).ToList<VariableBase>();
            if (sameNameObjList.Count == 1 && sameNameObjList[0] == var) return; //Same internal object

            //Set unique name
            int counter = 2;
            while ((Variables.FirstOrDefault(x => x.Name == var.Name + "_" + counter.ToString())) != null)
                counter++;
            var.Name = var.Name + "_" + counter.ToString();
        }

        public void ConvertGeneralParamsToVariable()
        {
            if (GeneralParams == null || GeneralParams.Count == 0) return;

            foreach (var generalParam in GeneralParams)
            {
                if (generalParam.Encrypt)
                {
                    var variablePassword = new VariablePasswordString()
                    {
                        Description = generalParam.Description,
                        Name = generalParam.Name
                    };

                    variablePassword.SetInitialValue(generalParam.Value);
                    Variables.Add(variablePassword);
                }
                else
                {
                    Variables.Add(
                         new VariableDynamic()
                         {
                             Name = generalParam.Name,
                             Description = generalParam.Description,
                             ValueExpression = generalParam.Value
                         }
                        );
                }
            }
            GeneralParams.Clear();
        }

        public override eImageType ItemImageType
        {
            get
            {
                return eImageType.Application;
            }
        }

        public override string ItemNameField
        {
            get
            {
                return nameof(this.Name);
            }
        }
        [IsSerializedForLocalRepository]
        public readonly ObservableList<VariableBase> Variables = new();
    }
}
